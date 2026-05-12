using reviewApi.DTO.Page.Configuration;
using reviewApi.Models;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Service.Repositories.Page.Configuration
{
    public class EvaluationObjectService : IEvaluationObjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EvaluationObjectService> _logger;

        public EvaluationObjectService(IUnitOfWork unitOfWork, ILogger<EvaluationObjectService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PaginatedResponse<EvaluationObjectDto>> GetAllAsync(string search = null, int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting all evaluation objects with search: {Search}, page: {Page}, pageSize: {PageSize}", search, page, pageSize);
                
                var query = _unitOfWork.EvaluationObjects.GetAll().AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(o => o.Code.ToLower().Contains(search) || o.Name.ToLower().Contains(search)).AsQueryable();
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Apply pagination
                var skip = (page - 1) * pageSize;
                var dtos = query.Skip(skip).Take(pageSize).Select(o => new EvaluationObjectDto
                {
                    Code = o.Code,
                    Name = o.Name,
                    IsActive = o.IsActive
                }).ToList();

                return new PaginatedResponse<EvaluationObjectDto>
                {
                    Data = dtos,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = page,
                        TotalPages = totalPages,
                        TotalItems = totalItems,
                        ItemsPerPage = pageSize
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllAsync");
                throw;
            }
        }

        public async Task<EvaluationObjectDto> GetByCodeAsync(string code)
        {
            try
            {
                _logger.LogInformation("Getting evaluation object by code: {Code}", code);
                var obj = _unitOfWork.EvaluationObjects.FindFirst(o => o.Code == code);
                if (obj == null)
                {
                    _logger.LogWarning("Evaluation object not found: {Code}", code);
                    return null;
                }

                return new EvaluationObjectDto
                {
                    Code = obj.Code,
                    Name = obj.Name,
                    IsActive = obj.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCodeAsync");
                throw;
            }
        }

        public async Task<EvaluationObjectDto> CreateAsync(CreateEvaluationObjectRequest request)
        {
            try
            {
                _logger.LogInformation("Creating evaluation object: {Code}", request.Code);

                // Check if already exists
                var existing = _unitOfWork.EvaluationObjects.FindFirst(o => o.Code == request.Code);
                if (existing != null)
                {
                    throw new Exception($"Evaluation object with code {request.Code} already exists");
                }

                var obj = new EvaluationObject
                {
                    Code = request.Code,
                    Name = request.Name,
                    IsActive = 1
                };

                _unitOfWork.EvaluationObjects.Add(obj);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Evaluation object created successfully: {Code}", request.Code);

                return new EvaluationObjectDto
                {
                    Code = obj.Code,
                    Name = obj.Name,
                    IsActive = obj.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAsync");
                throw;
            }
        }

        public async Task<EvaluationObjectDto> UpdateAsync(string code, UpdateEvaluationObjectRequest request)
        {
            try
            {
                _logger.LogInformation("Updating evaluation object: {Code}", code);

                var obj = _unitOfWork.EvaluationObjects.FindFirst(o => o.Code == code);
                if (obj == null)
                {
                    throw new Exception($"Evaluation object with code {code} not found");
                }

                obj.Name = request.Name;
                obj.IsActive = request.IsActive;

                _unitOfWork.EvaluationObjects.Update(obj);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Evaluation object updated successfully: {Code}", code);

                return new EvaluationObjectDto
                {
                    Code = obj.Code,
                    Name = obj.Name,
                    IsActive = obj.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateAsync");
                throw;
            }
        }

        public async Task DeleteAsync(string code)
        {
            try
            {
                _logger.LogInformation("Deleting evaluation object: {Code}", code);

                var obj = _unitOfWork.EvaluationObjects.FindFirst(o => o.Code == code);
                if (obj == null)
                    throw new Exception($"Evaluation object with code {code} not found");

                DeleteRelatedRecords(new List<string> { code });

                _unitOfWork.EvaluationObjects.Remove(obj);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Evaluation object deleted successfully: {Code}", code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAsync");
                throw;
            }
        }

        public async Task DeleteManyAsync(DeleteManyEvaluationObjectRequest request)
        {
            try
            {
                var query = _unitOfWork.EvaluationObjects.GetAll().AsQueryable();

                if (request.IsAll)
                {
                    if (!string.IsNullOrWhiteSpace(request.Filter?.Search))
                    {
                        var s = request.Filter.Search.ToLower();
                        query = query.Where(o => o.Code.ToLower().Contains(s) || o.Name.ToLower().Contains(s));
                    }
                    if (request.ExcludeIds?.Count > 0)
                        query = query.Where(o => !request.ExcludeIds.Contains(o.Code));
                }
                else
                {
                    query = query.Where(o => request.IncludeIds.Contains(o.Code));
                }

                var codes = query.Select(o => o.Code).ToList();
                _logger.LogInformation("DeleteMany: deleting {Count} evaluation objects", codes.Count);

                DeleteRelatedRecords(codes);

                var objects = _unitOfWork.EvaluationObjects.GetAll()
                    .Where(o => codes.Contains(o.Code)).ToList();
                foreach (var o in objects)
                    _unitOfWork.EvaluationObjects.Remove(o);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteManyAsync");
                throw;
            }
        }

        private void DeleteRelatedRecords(List<string> codes)
        {
            // Resolve codes to Guid IDs
            var objectIds = _unitOfWork.EvaluationObjects.GetAll()
                .Where(o => codes.Contains(o.Code))
                .Select(o => o.Id)
                .ToList();

            // EvaluationFlowObjects uses Restrict
            var flowObjects = _unitOfWork.EvaluationFlowObjects.GetAll()
                .Where(efo => objectIds.Contains(efo.EvaluationObjectId))
                .ToList();
            foreach (var efo in flowObjects)
                _unitOfWork.EvaluationFlowObjects.Remove(efo);

            // EvaluationObjectRoles uses Cascade but remove explicitly for safety
            var objectRoles = _unitOfWork.EvaluationObjectRoles.GetAll()
                .Where(eor => objectIds.Contains(eor.EvaluationObjectId))
                .ToList();
            foreach (var eor in objectRoles)
                _unitOfWork.EvaluationObjectRoles.Remove(eor);
        }
    }
}
