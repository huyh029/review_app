namespace reviewApi.Service.General
{
    public interface ITreeBuilderService
    {
        void AddTreeNode<T>(T entity, CreateTreeNodeRequest treeNode, string parentCode, int index = 1, 
            Action<T, string, string, int, CreateTreeNodeRequest> configureEntity = null,
            Func<CreateTreeNodeRequest, T> createChildEntity = null) where T : class, new();

        List<TreeNodeDto<T>> BuildTree<T>(List<T> items, Func<T, string> getCode, Func<T, string> getParentCode, 
            Func<T, TreeNodeDto<T>> mapToDto) where T : class;
    }

    public class CreateTreeNodeRequest
    {
        public string DisplayCode { get; set; }
        public string Content { get; set; }
        public decimal? MaxScore { get; set; }
        public string ScoreType { get; set; }
        public List<CreateTreeNodeRequest> Children { get; set; } = new();
    }

    public class TreeNodeDto<T> where T : class
    {
        public string Code { get; set; }
        public string ParentCode { get; set; }
        public T Data { get; set; }
        public List<TreeNodeDto<T>> Children { get; set; } = new();
    }
}
