namespace CustomerSupport.API.DTOs;

public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}