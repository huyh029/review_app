import { Component, ChangeDetectionStrategy, ChangeDetectorRef, ViewChild, ViewChildren, QueryList, ElementRef, AfterViewChecked, AfterViewInit, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmojiPickerComponent } from '../emoji-picker/emoji-picker.component';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';
import { CommentItem } from '../../protected/evaluation-board/evaluation-board.service';
import { AuthService } from '../../auth/services/auth.service';

@Component({
  selector: 'app-comment-section',
  standalone: true,
  imports: [CommonModule, FormsModule, EmojiPickerComponent, ConfirmationDialogComponent, DatePipe],
  templateUrl: './comment-section.component.html',
  styleUrls: ['./comment-section.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CommentSectionComponent implements AfterViewChecked, AfterViewInit, OnChanges {
  @ViewChild('commentTextarea') commentTextarea!: ElementRef<HTMLTextAreaElement>;
  @ViewChild('commentInput') commentInput!: ElementRef<HTMLInputElement>;
  @ViewChild('addMenuContainer') addMenuContainer!: ElementRef;
  @ViewChild('commentsList') commentsList!: ElementRef;
  @ViewChildren('commentElement') commentElements!: QueryList<ElementRef>;

  @Input() evaluationId: string | null = null;
  @Input() inputOnly: boolean = false;
  @Input() comments: any[] = [];
  @Input() set apiComments(value: CommentItem[]) {
    this._apiComments = value;
    this.flatApiComments = this.flattenComments(value);
    this.scrollToBottom();
  }
  get apiComments(): CommentItem[] { return this._apiComments; }
  private _apiComments: CommentItem[] = [];
  flatApiComments: CommentItem[] = [];

  private flattenComments(comments: CommentItem[]): CommentItem[] {
    const result: CommentItem[] = [];
    const collect = (items: CommentItem[]) => {
      for (const c of items) {
        result.push(c);
        if (c.replies?.length) collect(c.replies);
      }
    };
    collect(comments);
    return result.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
  }
  @Output() commentAdded = new EventEmitter<any>();
  @Output() commentDeleted = new EventEmitter<number>();
  @Output() reactionAdded = new EventEmitter<{ commentIndex: number; commentId?: string; emoji: string }>();
  @Output() reactionRemoved = new EventEmitter<{ commentIndex: number; commentId?: string; userName: string }>();
  @Output() sendComment = new EventEmitter<{ content: string; replyToCommentId?: string; files?: File[] }>();
  @Output() deleteApiComment = new EventEmitter<string>();

  currentUserId: string = '';
  currentUser: string = '';

  // Make Math available in template
  Math = Math;

  // Comment properties
  commentText: string = '';
  isTextareaFocused: boolean = false;
  isAddButtonRotated: boolean = false;
  showAddMenu: boolean = false;
  private shouldFocusTextarea: boolean = false;
  isRecording: boolean = false;
  mediaRecorder: MediaRecorder | null = null;
  audioChunks: Blob[] = [];
  recordingTime: number = 0;
  recordingInterval: any = null;
  recordingCompleted: boolean = false;
  isSavingRecording: boolean = false;
  recordedAudioUrl: string | null = null;
  isPlayingAudio: boolean = false;
  audioPlayer: HTMLAudioElement | null = null;
  playbackTime: number = 0;
  recordingDuration: number = 0;

  // File attachment properties
  attachedFiles: { name: string; size: number; url: string; type: string }[] = [];
  rawFiles: File[] = [];
  fileInputRef: HTMLInputElement | null = null;

  // UI state properties
  hoveredCommentIndex: number = -1;
  replyingToIndex: number = -1;
  showReactionPicker: number = -1;
  showReactionsDetail: number = -1;
  selectedReactionFilter: string = '';

  constructor(private cdr: ChangeDetectorRef, private authService: AuthService) {
    const user = this.authService.getUser();
    if (user) {
      this.currentUserId = user.id;
      this.currentUser = user.fullName;
    }
  }

  ngOnChanges(changes: SimpleChanges) {}

  loadApiComments() {}

  ngAfterViewChecked() {
    if (this.shouldFocusTextarea && this.commentTextarea) {
      this.commentTextarea.nativeElement.focus();
      this.shouldFocusTextarea = false;
    }
  }

  ngAfterViewInit() {
    this.scrollToBottom();
  }

  onEmojiSelected(emoji: string) {
    this.commentText += emoji;
    this.shouldFocusTextarea = true;
    this.cdr.markForCheck();
  }

  onTextareaInput() {
    this.shouldFocusTextarea = true;
    if (this.commentTextarea) {
      const textarea = this.commentTextarea.nativeElement;
      textarea.style.height = 'auto';
      const scrollHeight = Math.min(textarea.scrollHeight, 160);
      textarea.style.height = scrollHeight + 'px';
    }
    this.cdr.markForCheck();
  }

  onTextareaFocus() {
    this.isTextareaFocused = true;
    this.cdr.markForCheck();
  }

  onTextareaBlur() {
    this.isTextareaFocused = false;
    this.cdr.markForCheck();
  }

  onTextareaKeyDown(event: any) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.onSendComment();
    }
  }

  onPaste(event: ClipboardEvent) {
    const items = event.clipboardData?.items;
    if (!items) return;

    for (let i = 0; i < items.length; i++) {
      if (items[i].kind === 'file') {
        const file = items[i].getAsFile();
        if (file) {
          this.handleFileUpload(file);
        }
      }
    }
  }

  onRecordAudio() {
    this.isRecording = true;
    this.recordingTime = 0;
    this.audioChunks = [];
    this.startRecording();
    this.cdr.markForCheck();
  }

  private async startRecording() {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      this.mediaRecorder = new MediaRecorder(stream);
      this.mediaRecorder.ondataavailable = (event) => {
        this.audioChunks.push(event.data);
      };
      this.mediaRecorder.start();

      this.recordingInterval = setInterval(() => {
        this.recordingTime++;
        if (this.recordingTime >= 180) {
          this.completeRecording();
        }
        this.cdr.markForCheck();
      }, 1000);
    } catch (error) {
      console.error('Error accessing microphone:', error);
      this.isRecording = false;
      this.cdr.markForCheck();
    }
  }

  cancelRecording() {
    if (this.mediaRecorder && this.isRecording) {
      this.mediaRecorder.stop();
      this.mediaRecorder.stream.getTracks().forEach(track => track.stop());
    }
    clearInterval(this.recordingInterval);
    this.isRecording = false;
    this.recordingTime = 0;
    this.audioChunks = [];
    this.cdr.markForCheck();
  }

  getFormattedTime(): string {
    const minutes = Math.floor(this.recordingTime / 60);
    const seconds = this.recordingTime % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  completeRecording() {
    if (this.mediaRecorder && this.isRecording) {
      this.mediaRecorder.stop();
      this.mediaRecorder.stream.getTracks().forEach(track => track.stop());
    }
    clearInterval(this.recordingInterval);
    this.isRecording = false;

    const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm' });
    this.recordedAudioUrl = URL.createObjectURL(audioBlob);
    this.recordingCompleted = true;
    this.recordingDuration = this.recordingTime;
    this.cdr.markForCheck();
  }

  playRecordedAudio() {
    if (this.recordedAudioUrl) {
      this.audioPlayer = new Audio(this.recordedAudioUrl);
      this.audioPlayer.play();
      this.isPlayingAudio = true;

      this.audioPlayer.ontimeupdate = () => {
        this.playbackTime = this.audioPlayer?.currentTime || 0;
        this.cdr.markForCheck();
      };

      this.audioPlayer.onended = () => {
        this.isPlayingAudio = false;
        this.playbackTime = 0;
        this.cdr.markForCheck();
      };
    }
  }

  stopPlayingAudio() {
    if (this.audioPlayer) {
      this.audioPlayer.pause();
      this.isPlayingAudio = false;
      this.playbackTime = 0;
      this.cdr.markForCheck();
    }
  }

  deleteRecording() {
    this.recordedAudioUrl = null;
    this.recordingCompleted = false;
    this.recordingTime = 0;
    this.playbackTime = 0;
    this.audioChunks = [];
    this.cdr.markForCheck();
  }

  onAddFile() {
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.multiple = true;
    fileInput.style.display = 'none';
    fileInput.onchange = (event: any) => {
      const files = event.target.files;
      for (let i = 0; i < files.length; i++) {
        this.handleFileUpload(files[i]);
      }
      document.body.removeChild(fileInput);
    };
    document.body.appendChild(fileInput);
    fileInput.click();
  }

  private handleFileUpload(file: File) {
    this.rawFiles.push(file);
    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.attachedFiles.push({
        name: file.name,
        size: file.size,
        url: e.target.result,
        type: file.type
      });
      this.cdr.markForCheck();
    };
    reader.readAsDataURL(file);
  }

  removeAttachedFile(index: number) {
    this.attachedFiles.splice(index, 1);
    this.rawFiles.splice(index, 1);
    this.cdr.markForCheck();
  }

  getFileIcon(fileType: string): string {
    if (fileType.indexOf('image') !== -1) return '🖼️';
    if (fileType.indexOf('pdf') !== -1) return '📄';
    if (fileType.indexOf('word') !== -1 || fileType.indexOf('document') !== -1) return '📝';
    if (fileType.indexOf('sheet') !== -1) return '📊';
    return '📎';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }

  timeAgo(dateStr: string): string {
    const now = new Date();
    const date = new Date(dateStr);
    const diff = Math.floor((now.getTime() - date.getTime()) / 1000);
    if (diff < 60) return 'Vừa xong';
    if (diff < 3600) return `${Math.floor(diff / 60)} phút trước`;
    if (diff < 86400) return `${Math.floor(diff / 3600)} giờ trước`;
    if (diff < 2592000) return `${Math.floor(diff / 86400)} ngày trước`;
    if (diff < 31536000) return `${Math.floor(diff / 2592000)} tháng trước`;
    return `${Math.floor(diff / 31536000)} năm trước`;
  }

  onAddButtonClick() {
    this.isAddButtonRotated = !this.isAddButtonRotated;
    this.showAddMenu = !this.showAddMenu;
    this.cdr.markForCheck();
  }

  closeAddMenu() {
    this.isAddButtonRotated = false;
    this.showAddMenu = false;
    this.cdr.markForCheck();
  }

  onSendComment() {
    if (this.evaluationId && (this.commentText.trim() || this.rawFiles.length > 0)) {
      this.sendComment.emit({
        content: this.commentText.trim(),
        replyToCommentId: this.replyingToIndex >= 0 ? this.flatApiComments[this.replyingToIndex]?.id : undefined,
        files: [...this.rawFiles]
      });
      this.commentText = '';
      this.attachedFiles = [];
      this.rawFiles = [];
      this.recordedAudioUrl = null;
      this.recordingCompleted = false;
      this.isAddButtonRotated = false;
      this.showAddMenu = false;
      this.replyingToIndex = -1;
      this.cdr.markForCheck();
      return;
    }

    const commentData = {
      text: this.commentText.trim(),
      files: this.attachedFiles,
      audio: this.recordedAudioUrl,
      timestamp: new Date(),
      author: this.currentUser,
      avatar: undefined,
      replyTo: this.replyingToIndex >= 0 ? this.replyingToIndex : undefined
    };

    this.comments.push(commentData);
    this.commentAdded.emit(commentData);
    this.commentText = '';
    this.attachedFiles = [];
    this.recordedAudioUrl = null;
    this.recordingCompleted = false;
    this.isAddButtonRotated = false;
    this.showAddMenu = false;
    this.replyingToIndex = -1;
    this.cdr.markForCheck();
    this.scrollToBottom();
  }

  onCommentHover(index: number) {
    this.hoveredCommentIndex = index;
    this.cdr.markForCheck();
  }

  onCommentLeave() {
    this.hoveredCommentIndex = -1;
    this.cdr.markForCheck();
  }

  onDeleteComment(index: number) {
    this.pendingDeleteIndex = index;
    this.showDeleteConfirm = true;
    this.cdr.markForCheck();
  }

  onConfirmDelete() {
    const index = this.pendingDeleteIndex;
    this.showDeleteConfirm = false;
    this.pendingDeleteIndex = null;
    if (index === null) return;
    if (this.evaluationId && this.flatApiComments[index]) {
      this.deleteApiComment.emit(this.flatApiComments[index].id);
      return;
    }
    this.comments.splice(index, 1);
    this.commentDeleted.emit(index);
    this.cdr.markForCheck();
  }

  onCancelDelete() {
    this.showDeleteConfirm = false;
    this.pendingDeleteIndex = null;
    this.cdr.markForCheck();
  }

  onReplyComment(index: number) {
    this.replyingToIndex = index;
    this.shouldFocusTextarea = true;
    this.cdr.markForCheck();
    
    // Focus input immediately
    setTimeout(() => {
      if (this.commentInput) {
        this.commentInput.nativeElement.focus();
      }
    }, 100);
  }

  cancelReply() {
    this.replyingToIndex = -1;
    this.cdr.markForCheck();
  }

  onReactComment(index: number) {
    this.showReactionPicker = this.showReactionPicker === index ? -1 : index;
    this.cdr.markForCheck();
  }

  addReaction(index: number, emoji: string) {
    const target = this.evaluationId ? this.flatApiComments : this.comments;
    const comment = target[index];
    if (!comment) return;
    if (!comment['reactions']) comment['reactions'] = [];
    if (!comment['reactionUsers']) comment['reactionUsers'] = [];
    comment['reactions'].push(emoji);
    comment['reactionUsers'].push({ emoji, name: this.currentUser, avatar: '' });
    this.reactionAdded.emit({ commentIndex: index, commentId: comment['id'], emoji });
    this.showReactionPicker = -1;
    this.cdr.markForCheck();
  }

  toggleReactionsDetail(index: number) {
    this.showReactionsDetail = this.showReactionsDetail === index ? -1 : index;
    this.selectedReactionFilter = '';
    this.cdr.markForCheck();
  }

  get activeReactionComment(): any {
    if (this.showReactionsDetail === -1) return null;
    const target = this.evaluationId ? this.flatApiComments : this.comments;
    const comment = target[this.showReactionsDetail];
    if (!comment) return null;

    // Nếu là API comment, build reactionUsers từ reactions array
    if (this.evaluationId && comment.reactions?.length && typeof comment.reactions[0] === 'object') {
      const apiReactions = comment.reactions as { id: string; userId: string; emoji: string }[];
      return {
        ...comment,
        reactions: apiReactions.map((r: any) => r.emoji),
        reactionUsers: apiReactions.map((r: any) => ({
          emoji: r.emoji,
          name: r.userId === this.currentUserId ? this.currentUser : `User ${r.userId}`,
        }))
      };
    }
    return comment;
  }

  getCommentTitle(comment: CommentItem): string {
    if (comment.replyToCommentId === null) {
      // Không reply: hiện username, ẩn nếu là mình
      return comment.userId === this.currentUserId ? '' : comment.userName;
    }

    const target = this.flatApiComments.find(c => c.id === comment.replyToCommentId);
    const isSelf = comment.userId === this.currentUserId;
    const targetIsSelf = target?.userId === this.currentUserId;
    const senderName = isSelf ? 'Bạn' : comment.userName;
    const targetName = isSelf && targetIsSelf ? 'chính mình' : targetIsSelf ? 'bạn' : (target?.userName ?? '');

    return `${senderName} đã trả lời ${targetName}`;
  }

  getReplyTargetName(replyToCommentId: string | null): string {
    if (!replyToCommentId) return '';
    const target = this.flatApiComments.find(c => c.id === replyToCommentId);
    return target ? target.userName : '';
  }

  getReplyTargetContent(replyToCommentId: string | null): string {
    if (!replyToCommentId) return '';
    const target = this.flatApiComments.find(c => c.id === replyToCommentId);
    return target ? target.content : '';
  }

  scrollToApiComment(commentId: string | null) {
    if (!commentId) return;
    const index = this.flatApiComments.findIndex(c => c.id === commentId);
    if (index >= 0) this.scrollToComment(index);
  }

  // Normalize reactions thành array emoji string, hỗ trợ cả format API { id, userId, emoji } lẫn string
  private normalizeReactions(reactions: any[]): string[] {
    if (!reactions) return [];
    return reactions.map(r => typeof r === 'string' ? r : r.emoji);
  }

  getUniqueReactions(reactions: any[]): string[] {
    return [...new Set(this.normalizeReactions(reactions))];
  }

  getReactionCount(reactions: any[], emoji: string): number {
    return this.normalizeReactions(reactions).filter(r => r === emoji).length;
  }

  getGroupedReactionsByUser(reactionUsers: any[]): any[] {
    if (!reactionUsers) return [];
    const grouped: { [key: string]: string[] } = {};
    reactionUsers.forEach(user => {
      if (!grouped[user.name]) {
        grouped[user.name] = [];
      }
      grouped[user.name].push(user.emoji);
    });
    return Object.entries(grouped).map(([name, emojis]) => ({ name, emojis }));
  }

  getUniqueEmojisWithCount(emojis: string[]): { emoji: string; count: number }[] {
    const emojiMap: { [key: string]: number } = {};
    emojis.forEach(emoji => {
      emojiMap[emoji] = (emojiMap[emoji] || 0) + 1;
    });
    return Object.entries(emojiMap).map(([emoji, count]) => ({ emoji, count }));
  }

  removeReaction(commentIndex: number, userName: string) {
    const target = this.evaluationId ? this.flatApiComments : this.comments;
    const comment = target[commentIndex];
    if (!comment) return;

    // Lấy danh sách emoji của user cần xóa trước
    const userEmojis = (comment['reactionUsers'] || [])
      .filter((u: any) => u.name === userName)
      .map((u: any) => u.emoji);

    // Xóa từng emoji một khỏi reactions
    userEmojis.forEach((emoji: string) => {
      const idx = comment['reactions'].indexOf(emoji);
      if (idx !== -1) comment['reactions'].splice(idx, 1);
    });

    // Xóa user khỏi reactionUsers
    comment['reactionUsers'] = (comment['reactionUsers'] || []).filter((u: any) => u.name !== userName);

    this.reactionRemoved.emit({ commentIndex, commentId: comment['id'], userName });
    this.cdr.markForCheck();
  }

  scrollToComment(commentIndex: number) {
    setTimeout(() => {
      const commentElement = this.commentElements?.toArray()[commentIndex];
      if (commentElement) {
        commentElement.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    }, 0);
  }

  scrollToBottom() {
    setTimeout(() => {
      if (this.commentsList) {
        this.commentsList.nativeElement.scrollTop = this.commentsList.nativeElement.scrollHeight;
      }
    }, 100);
  }

  isSingleEmoji(text: string): boolean {
    if (!text?.trim()) return false;
    const emojiRegex = /^\p{Emoji_Presentation}$/u;
    return emojiRegex.test(text.trim());
  }

  lightboxUrl: string | null = null;
  lightboxType: 'image' | 'video' = 'image';

  showDeleteConfirm = false;
  pendingDeleteIndex: number | null = null;

  getFileUrl(filePath: string): string {
    if (!filePath) return '';
    if (filePath.startsWith('http')) return filePath;
    // Use proxy path - works for <img> tags without auth headers
    return filePath; // filePath is like /uploads/comments/xxx.mp4, proxied via proxy.conf.json
  }

  async getBlobUrl(filePath: string): Promise<string> {
    // Use the files API endpoint which has [AllowAnonymous]
    const fileName = filePath.split('/').pop();
    const apiUrl = `/api/page/evaluation-board/comments/files/${fileName}`;
    const token = localStorage.getItem('token');
    const headers: Record<string, string> = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const response = await fetch(apiUrl, { headers });
    if (!response.ok) throw new Error(`Failed to fetch file: ${response.status}`);
    const blob = await response.blob();
    return URL.createObjectURL(blob);
  }

  isImageFile(fileType: string, fileName: string, filePath?: string): boolean {
    if (fileType?.startsWith('image/')) return true;
    const checkName = filePath?.split('/').pop() ?? fileName;
    const ext = checkName?.split('.').pop()?.toLowerCase();
    return ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp', 'svg'].includes(ext ?? '');
  }

  isVideoFile(fileType: string, fileName: string, filePath?: string): boolean {
    if (fileType?.startsWith('video/')) return true;
    const checkName = filePath?.split('/').pop() ?? fileName;
    const ext = checkName?.split('.').pop()?.toLowerCase();
    return ['mp4', 'webm', 'ogg', 'mov', 'avi', 'mkv'].includes(ext ?? '');
  }

  openLightbox(path: string, type: 'image' | 'video' = 'image') {
    console.log('[openLightbox] called', { path, type });
    this.lightboxType = type;
    if (type === 'video') {
      // Use direct URL for video to support range requests (seeking/streaming)
      const fileName = path.split('/').pop();
      const url = `/api/page/evaluation-board/comments/files/${fileName}`;
      console.log('[openLightbox] video url:', url);
      this.lightboxUrl = url + '?t=' + Date.now();
      this.cdr.markForCheck();
    } else {
      this.getBlobUrl(path).then(blobUrl => {
        console.log('[openLightbox] image blob url:', blobUrl);
        this.lightboxUrl = blobUrl;
        this.cdr.markForCheck();
      }).catch(err => {
        console.error('[openLightbox] Failed to load file:', err);
      });
    }
  }

  closeLightbox() {
    if (this.lightboxUrl?.startsWith('blob:')) {
      URL.revokeObjectURL(this.lightboxUrl);
    }
    this.lightboxUrl = null;
    this.cdr.markForCheck();
  }

  openFileUrl(path: string) {
    window.open(path, '_blank');
  }

  sendLikeMessage() {
    if (this.evaluationId) {
      this.sendComment.emit({
        content: '👍',
        replyToCommentId: this.replyingToIndex >= 0 ? this.flatApiComments[this.replyingToIndex]?.id : undefined
      });
      this.replyingToIndex = -1;
      this.cdr.markForCheck();
      return;
    }

    const likeMessage = {
      text: '👍',
      files: [],
      audio: null,
      timestamp: new Date(),
      author: this.currentUser,
      avatar: undefined,
      replyTo: this.replyingToIndex >= 0 ? this.replyingToIndex : undefined,
      reactions: [],
      reactionUsers: []
    };

    this.comments.push(likeMessage);
    this.commentAdded.emit(likeMessage);
    this.replyingToIndex = -1;
    this.cdr.markForCheck();
    this.scrollToBottom();
  }
}
