namespace reviewApi.DTO.Page.Configuration
{
    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; }
        public PaginationInfo Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
    }
}
