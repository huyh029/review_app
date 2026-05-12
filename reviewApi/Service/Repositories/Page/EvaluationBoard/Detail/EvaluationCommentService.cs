using reviewApi.DTO.Page.EvaluationBoard;
using reviewApi.Models;
using reviewApi.Service.Page.EvaluationBoard.Detail;

namespace reviewApi.Service.Repositories.Page.EvaluationBoard.Detail
{
    public class EvaluationCommentService : IEvaluationCommentService
    {
        private readonly IUnitOfWork _uow;

        public EvaluationCommentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<CommentDto>> GetCommentsAsync(Guid evaluationId)
        {
            var users = _uow.Users.GetAll().ToDictionary(u => u.Id);

            var allComments = _uow.Comments.Find(c => c.EvaluationId == evaluationId).ToList();
            var commentIds = allComments.Select(c => c.Id).ToList();
            var reactions = _uow.CommentReactions.Find(r => commentIds.Contains(r.CommentId)).ToList();
            var files = _uow.CommentFiles.Find(f => commentIds.Contains(f.CommentId)).ToList();

            var rootComments = allComments.Where(c => c.ReplyToCommentId == null).OrderBy(c => c.CreatedAt).ToList();

            return rootComments.Select(c => MapComment(c, allComments, reactions, files, users)).ToList();
        }

        public async Task<CommentDto> AddCommentAsync(Guid userId, AddCommentRequest request)
        {
            var comment = new Comment
            {
                EvaluationId = request.EvaluationId,
                UserId = userId,
                Content = request.Content,
                ReplyToCommentId = request.ReplyToCommentId,
                CreatedAt = DateTime.UtcNow
            };

            _uow.Comments.Add(comment);
            await _uow.SaveChangesAsync();

            var users = _uow.Users.GetAll().ToDictionary(u => u.Id);
            return MapComment(comment, new List<Comment>(), new List<CommentReaction>(), new List<CommentFile>(), users);
        }

        public async Task DeleteCommentAsync(Guid id, Guid userId)
        {
            var comment = _uow.Comments.FindFirst(c => c.Id == id && c.UserId == userId)
                ?? throw new Exception("Không tìm thấy bình luận");

            var replies = _uow.Comments.Find(c => c.ReplyToCommentId == id).ToList();

            foreach (var reply in replies)
            {
                reply.ReplyToCommentId = null;
            }

            var reactions = _uow.CommentReactions.Find(r => r.CommentId == id).ToList();
            var files = _uow.CommentFiles.Find(f => f.CommentId == id).ToList();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file.FilePath);
                var fullPath = Path.Combine("wwwroot", "uploads", "comments", fileName);
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }

            _uow.CommentReactions.RemoveRange(reactions);
            _uow.CommentFiles.RemoveRange(files);
            _uow.Comments.Remove(comment);

            await _uow.SaveChangesAsync();
        }

        public async Task AddReactionAsync(Guid commentId, AddReactionRequest request)
        {
            var existing = _uow.CommentReactions.FindFirst(r =>
                r.CommentId == commentId && r.UserId == request.UserId && r.Emoji == request.Emoji);

            if (existing != null)
            {
                _uow.CommentReactions.Remove(existing);
            }
            else
            {
                _uow.CommentReactions.Add(new CommentReaction
                {
                    CommentId = commentId,
                    UserId = request.UserId,
                    Emoji = request.Emoji,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _uow.SaveChangesAsync();
        }

        public async Task DeleteReactionAsync(Guid commentId, Guid userId)
        {
            var reactions = _uow.CommentReactions.Find(r => r.CommentId == commentId && r.UserId == userId).ToList();
            _uow.CommentReactions.RemoveRange(reactions);
            await _uow.SaveChangesAsync();
        }

        public async Task AddFileAsync(Guid commentId, IFormFile file)
        {
            var uploadsDir = Path.Combine("wwwroot", "uploads", "comments");
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var isVideo = new[] { ".mp4", ".mov", ".avi", ".mkv", ".webm" }.Contains(ext);
            var guid = Guid.NewGuid().ToString();
            var savedFileName = $"{guid}_{file.FileName}";
            var savedPath = Path.Combine(uploadsDir, savedFileName);

            using (var stream = new FileStream(savedPath, FileMode.Create))
                await file.CopyToAsync(stream);

            string finalFileName = savedFileName;
            string finalPath = $"/uploads/comments/{savedFileName}";
            string finalType = DetectMimeTypeFromBytes(file.OpenReadStream(), file.FileName, file.ContentType);

            if (isVideo)
            {
                var ffmpegExe = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "ffmpeg-master-latest-win64-gpl", "bin", "ffmpeg.exe");
                var outputFileName = $"{guid}_converted.mp4";
                var outputPath = Path.Combine(uploadsDir, outputFileName);

                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ffmpegExe,
                        Arguments = $"-y -i \"{savedPath}\" -vcodec libx264 -acodec aac -preset fast \"{outputPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    var process = System.Diagnostics.Process.Start(psi)!;
                    var stderrTask = process.StandardError.ReadToEndAsync();
                    var stdoutTask = process.StandardOutput.ReadToEndAsync();
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromMinutes(5));
                    await process.WaitForExitAsync(cts.Token);
                    await Task.WhenAll(stderrTask, stdoutTask);

                    if (process.ExitCode == 0 && File.Exists(outputPath))
                    {
                        File.Delete(savedPath);
                        finalFileName = outputFileName;
                        finalPath = $"/uploads/comments/{outputFileName}";
                        finalType = "video/mp4";
                    }
                }
                catch { /* dùng file gốc nếu lỗi */ }
            }

            _uow.CommentFiles.Add(new CommentFile
            {
                CommentId = commentId,
                FileName = file.FileName,
                FileSize = file.Length,
                FileType = finalType,
                FilePath = finalPath
            });

            await _uow.SaveChangesAsync();
        }

        private static string GetMimeType(string fileName, string fallback)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext switch
            {
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                ".mkv" => "video/x-matroska",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => fallback
            };
        }

        private static string DetectMimeTypeFromBytes(Stream stream, string fileName, string fallback)
        {
            var header = new byte[12];
            stream.Read(header, 0, header.Length);
            stream.Position = 0;

            if (header.Length >= 8 &&
                header[4] == 'f' && header[5] == 't' && header[6] == 'y' && header[7] == 'p')
                return "video/mp4";

            if (header[0] == 0x1A && header[1] == 0x45 && header[2] == 0xDF && header[3] == 0xA3)
                return "video/webm";

            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                return "image/png";

            if (header[0] == 0xFF && header[1] == 0xD8)
                return "image/jpeg";

            if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46)
                return "image/gif";

            return GetMimeType(fileName, fallback);
        }

        private static CommentDto MapComment(
            Comment c,
            List<Comment> allComments,
            List<CommentReaction> allReactions,
            List<CommentFile> allFiles,
            Dictionary<Guid, Models.User> users)
        {
            users.TryGetValue(c.UserId, out var user);
            var replies = allComments.Where(r => r.ReplyToCommentId == c.Id).OrderBy(r => r.CreatedAt).ToList();

            return new CommentDto
            {
                Id = c.Id,
                EvaluationId = c.EvaluationId,
                UserId = c.UserId,
                UserName = user?.FullName ?? "",
                Content = c.Content,
                ReplyToCommentId = c.ReplyToCommentId,
                CreatedAt = c.CreatedAt,
                Replies = replies.Select(r => MapComment(r, allComments, allReactions, allFiles, users)).ToList(),
                Reactions = allReactions.Where(r => r.CommentId == c.Id).Select(r => new CommentReactionDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Emoji = r.Emoji
                }).ToList(),
                Files = allFiles.Where(f => f.CommentId == c.Id).Select(f => new CommentFileDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileType = f.FileType,
                    FileSize = f.FileSize
                }).ToList()
            };
        }
    }
}
