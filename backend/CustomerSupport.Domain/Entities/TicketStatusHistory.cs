using CustomerSupport.Domain.Enums;

namespace CustomerSupport.Domain.Entities;

public class TicketStatusHistory
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public TicketStatus OldStatus { get; set; }
    public TicketStatus NewStatus { get; set; }
    public int ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public Ticket? Ticket { get; set; }
    public User? ChangedByUser { get; set; }
}