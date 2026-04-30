namespace CustomerSupport.API.DTOs;

public class TicketFilterDto
{
    public int? Status { get; set; }
    public int? Priority { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? CategoryId { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}