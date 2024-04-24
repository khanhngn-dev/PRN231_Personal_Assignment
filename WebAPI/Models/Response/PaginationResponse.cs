namespace WebAPI.Models.Response
{
    public class PaginationResponse<T> where T : class
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
