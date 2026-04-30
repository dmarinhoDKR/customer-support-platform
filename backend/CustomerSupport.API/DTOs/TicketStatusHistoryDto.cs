namespace CustomerSupport.API.DTOs;

public class TicketStatusHistoryDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public int ChangedByUserId { get; set; }
    public string ChangedByUserName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}