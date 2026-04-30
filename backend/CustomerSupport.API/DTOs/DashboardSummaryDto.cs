namespace CustomerSupport.API.DTOs;

public class DashboardSummaryDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int WaitingForCustomerTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int LowPriorityTickets { get; set; }
    public int MediumPriorityTickets { get; set; }
    public int HighPriorityTickets { get; set; }
    public int CriticalPriorityTickets { get; set; }
}