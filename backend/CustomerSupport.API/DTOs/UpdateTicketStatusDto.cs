namespace CustomerSupport.API.DTOs;

public class UpdateTicketStatusDto
{
    public int ChangedByUserId { get; set; }
    public int NewStatus { get; set; }
}