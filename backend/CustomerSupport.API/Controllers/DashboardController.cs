using Microsoft.AspNetCore.Authorization;
using CustomerSupport.API.DTOs;
using CustomerSupport.Domain.Enums;
using CustomerSupport.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var summary = new DashboardSummaryDto
        {
            TotalTickets = await _context.Tickets.CountAsync(),
            OpenTickets = await _context.Tickets.CountAsync(x => x.Status == TicketStatus.Open),
            InProgressTickets = await _context.Tickets.CountAsync(x => x.Status == TicketStatus.InProgress),
            WaitingForCustomerTickets = await _context.Tickets.CountAsync(x => x.Status == TicketStatus.WaitingCustomer),
            ResolvedTickets = await _context.Tickets.CountAsync(x => x.Status == TicketStatus.Resolved),
            ClosedTickets = await _context.Tickets.CountAsync(x => x.Status == TicketStatus.Closed),
            LowPriorityTickets = await _context.Tickets.CountAsync(x => x.Priority == TicketPriority.Low),
            MediumPriorityTickets = await _context.Tickets.CountAsync(x => x.Priority == TicketPriority.Medium),
            HighPriorityTickets = await _context.Tickets.CountAsync(x => x.Priority == TicketPriority.High),
            CriticalPriorityTickets = await _context.Tickets.CountAsync(x => x.Priority == TicketPriority.Critical)
        };

        return Ok(summary);
    }
}