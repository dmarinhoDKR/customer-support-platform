using CustomerSupport.Domain.Enums;

namespace CustomerSupport.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public int CategoryId { get; set; }
    public int CreatedByUserId { get; set; }
    public int? AssignedToUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Category? Category { get; set; }
    public User? CreatedByUser { get; set; }
    public User? AssignedToUser { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = [];
    public ICollection<TicketStatusHistory> StatusHistory { get; set; } = [];
}