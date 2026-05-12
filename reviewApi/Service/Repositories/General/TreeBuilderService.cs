using reviewApi.Models;
using reviewApi.Service.General;

namespace reviewApi.Service.Repositories.General
{
    public class TreeBuilderService : ITreeBuilderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TreeBuilderService> _logger;

        public TreeBuilderService(IUnitOfWork unitOfWork, ILogger<TreeBuilderService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public void AddTreeNode<T>(T entity, CreateTreeNodeRequest treeNode, string parentCode, int index = 1, 
            Action<T, string, string, int, CreateTreeNodeRequest> configureEntity = null,
            Func<CreateTreeNodeRequest, T> createChildEntity = null) where T : class, new()
        {
            // Generate VirtualCode based on tree structure: 1, 1.1, 1.2, 1.2.1, etc.
            string virtualCode = string.IsNullOrEmpty(parentCode)
                ? index.ToString()
                : $"{parentCode}.{index}";

            // Allow caller to configure the entity with virtualCode, parentCode, and content
            configureEntity?.Invoke(entity, virtualCode, parentCode ?? string.Empty, index, treeNode);

            // Add entity to repository
            var repository = GetRepository<T>();
            repository.Add(entity);

            // Recursively add children
            if (treeNode.Children != null && treeNode.Children.Count > 0)
            {
                for (int i = 0; i < treeNode.Children.Count; i++)
                {
                    var childEntity = createChildEntity != null ? createChildEntity(treeNode.Children[i]) : new T();
                    AddTreeNode(childEntity, treeNode.Children[i], virtualCode, i + 1, configureEntity, createChildEntity);
                }
            }
        }

        public List<TreeNodeDto<T>> BuildTree<T>(List<T> items, Func<T, string> getCode, Func<T, string> getParentCode, 
            Func<T, TreeNodeDto<T>> mapToDto) where T : class
        {
            var result = new List<TreeNodeDto<T>>();
            var itemDict = items.ToDictionary(item => getCode(item));

            // Find root nodes (no parent or parent not in list)
            var rootItems = items.Where(item => 
            {
                var parentCode = getParentCode(item);
                return string.IsNullOrEmpty(parentCode) || !itemDict.ContainsKey(parentCode);
            }).ToList();

            // Build tree for each root
            foreach (var rootItem in rootItems)
            {
                var treeNode = BuildTreeNode(rootItem, items, getCode, getParentCode, mapToDto);
                result.Add(treeNode);
            }

            return result;
        }

        private TreeNodeDto<T> BuildTreeNode<T>(T item, List<T> allItems, Func<T, string> getCode, 
            Func<T, string> getParentCode, Func<T, TreeNodeDto<T>> mapToDto) where T : class
        {
            var code = getCode(item);
            var dto = mapToDto(item);
            dto.Code = code;
            dto.ParentCode = getParentCode(item);

            // Find children
            var children = allItems.Where(child => getParentCode(child) == code).ToList();
            foreach (var child in children)
            {
                var childNode = BuildTreeNode(child, allItems, getCode, getParentCode, mapToDto);
                dto.Children.Add(childNode);
            }

            return dto;
        }

        private IGenericRepository<T> GetRepository<T>() where T : class
        {
            // This is a simplified approach - in real implementation, you might use reflection or factory pattern
            if (typeof(T) == typeof(Criteria))
            {
                return _unitOfWork.Criterias as IGenericRepository<T>;
            }
            
            throw new NotSupportedException($"Repository for type {typeof(T).Name} is not supported");
        }
    }
}
