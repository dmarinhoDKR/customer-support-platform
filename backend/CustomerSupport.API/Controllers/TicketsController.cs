using Microsoft.AspNetCore.Authorization;
using CustomerSupport.API.DTOs;
using CustomerSupport.Domain.Entities;
using CustomerSupport.Domain.Enums;
using CustomerSupport.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TicketsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TicketDto>>> GetAll([FromQuery] TicketFilterDto filter)
    {
        var query = _context.Tickets
            .Include(x => x.Category)
            .Include(x => x.AssignedToUser)
            .Include(x => x.CreatedByUser)
            .AsQueryable();

        if (filter.Status.HasValue)
        {
            if (!Enum.IsDefined(typeof(TicketStatus), filter.Status.Value))
                return BadRequest("The informed status filter is invalid.");

            var status = (TicketStatus)filter.Status.Value;
            query = query.Where(x => x.Status == status);
        } 

        if (filter.Priority.HasValue)
        {
            if (!Enum.IsDefined(typeof(TicketPriority), filter.Priority.Value))
                return BadRequest("The informed priority filter is invalid.");

            var priority = (TicketPriority)filter.Priority.Value;
            query = query.Where(x => x.Priority == priority);
        }

        if (filter.AssignedToUserId.HasValue)
            query = query.Where(x => x.AssignedToUserId == filter.AssignedToUserId.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);

        var sortBy = filter.SortBy?.Trim().ToLowerInvariant();
        var sortDirection = filter.SortDirection?.Trim().ToLowerInvariant();

        var descending = sortDirection == "desc";

        query = sortBy switch
        {
            "title" => descending ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "priority" => descending ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority),
            "createdat" => descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updatedat" => descending ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var pageSize = filter.PageSize;
        var pageNumber = filter.PageNumber;

        if (filter.Limit.HasValue)
            pageSize = filter.Limit.Value;

        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var totalCount = await query.CountAsync();

        var skip = (pageNumber - 1) * pageSize;

        if (filter.Offset.HasValue)
            skip = filter.Offset.Value;

        if (skip < 0)
            skip = 0;

        var tickets = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new TicketDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Status = x.Status.ToString(),
                Priority = x.Priority.ToString(),
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                CreatedByUserId = x.CreatedByUserId,
                CreatedByUserName = x.CreatedByUser != null ? x.CreatedByUser.FullName : string.Empty,
                AssignedToUserId = x.AssignedToUserId,
                AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.FullName : null,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        var result = new PagedResultDto<TicketDto>
        {
            Items = tickets,
            PageNumber = filter.Offset.HasValue ? 0 : pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Limit = filter.Limit ?? pageSize,
            Offset = filter.Offset ?? skip
        };

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDto>> GetById(int id)
    {
        var ticket = await _context.Tickets
            .Include(x => x.Category)
            .Include(x => x.AssignedToUser)
            .Include(x => x.CreatedByUser)
            .Where(x => x.Id == id)
            .Select(x => new TicketDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Status = x.Status.ToString(),
                Priority = x.Priority.ToString(),
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                CreatedByUserId = x.CreatedByUserId,
                CreatedByUserName = x.CreatedByUser != null ? x.CreatedByUser.FullName : string.Empty,
                AssignedToUserId = x.AssignedToUserId,
                AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.FullName : null,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (ticket is null)
            return NotFound();

        return Ok(ticket);
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<TicketCommentDto>>> GetComments(int id)
    {
        var ticketExists = await _context.Tickets.AnyAsync(x => x.Id == id);

        if (!ticketExists)
            return NotFound("Ticket not found.");

        var comments = await _context.TicketComments
            .Include(x => x.User)
            .Where(x => x.TicketId == id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new TicketCommentDto
            {
                Id = x.Id,
                TicketId = x.TicketId,
                UserId = x.UserId,
                UserName = x.User != null ? x.User.FullName : string.Empty,
                Content = x.Content,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<TicketCommentDto>> AddComment(int id, CreateTicketCommentDto dto)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
            return NotFound("Ticket not found.");

        var userExists = await _context.Users.AnyAsync(x => x.Id == dto.UserId);

        if (!userExists)
            return BadRequest("The informed userId does not exist.");

        var comment = new TicketComment
        {
            TicketId = id,
            UserId = dto.UserId,
            Content = dto.Content
        };

        _context.TicketComments.Add(comment);
        await _context.SaveChangesAsync();

        var createdComment = await _context.TicketComments
            .Include(x => x.User)
            .Where(x => x.Id == comment.Id)
            .Select(x => new TicketCommentDto
            {
                Id = x.Id,
                TicketId = x.TicketId,
                UserId = x.UserId,
                UserName = x.User != null ? x.User.FullName : string.Empty,
                Content = x.Content,
                CreatedAt = x.CreatedAt
            })
            .FirstAsync();

        return Ok(createdComment);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<TicketDto>> UpdateStatus(int id, UpdateTicketStatusDto dto)
    {
        var ticket = await _context.Tickets
            .Include(x => x.Category)
            .Include(x => x.AssignedToUser)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
            return NotFound("Ticket not found.");

        var userExists = await _context.Users.AnyAsync(x => x.Id == dto.ChangedByUserId);

        if (!userExists)
            return BadRequest("The informed changedByUserId does not exist.");

        if (!Enum.IsDefined(typeof(TicketStatus), dto.NewStatus))
            return BadRequest("The informed newStatus is invalid.");

        var oldStatus = ticket.Status;
        var newStatus = (TicketStatus)dto.NewStatus;

        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        var statusHistory = new TicketStatusHistory
        {
            TicketId = ticket.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedByUserId = dto.ChangedByUserId
        };

        _context.TicketStatusHistories.Add(statusHistory);
        await _context.SaveChangesAsync();

        var updatedTicket = new TicketDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status.ToString(),
            Priority = ticket.Priority.ToString(),
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category != null ? ticket.Category.Name : string.Empty,
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByUserName = ticket.CreatedByUser != null ? ticket.CreatedByUser.FullName : string.Empty,
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = ticket.AssignedToUser != null ? ticket.AssignedToUser.FullName : null,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };

        return Ok(updatedTicket);
    }

    [HttpPut("{id:int}/assign")]
    public async Task<ActionResult<TicketDto>> AssignTicket(int id, AssignTicketDto dto)
    {
        var ticket = await _context.Tickets
            .Include(x => x.Category)
            .Include(x => x.AssignedToUser)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
            return NotFound("Ticket not found.");

        var assignedUser = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == dto.AssignedToUserId);

        if (assignedUser is null)
            return BadRequest("The informed assignedToUserId does not exist.");

        if (assignedUser.Role is null)
            return StatusCode(500, "Assigned user role is not assigned.");

        if (assignedUser.Role.Name != "Agent" && assignedUser.Role.Name != "Admin")
            return BadRequest("Only Admin or Agent users can be assigned to tickets.");

        ticket.AssignedToUserId = dto.AssignedToUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var updatedTicket = new TicketDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status.ToString(),
            Priority = ticket.Priority.ToString(),
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category != null ? ticket.Category.Name : string.Empty,
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByUserName = ticket.CreatedByUser != null ? ticket.CreatedByUser.FullName : string.Empty,
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = assignedUser.FullName,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };

        return Ok(updatedTicket);
    }

    [HttpGet("{id:int}/status-history")]
    public async Task<ActionResult<IEnumerable<TicketStatusHistoryDto>>> GetStatusHistory(int id)
    {
        var ticketExists = await _context.Tickets.AnyAsync(x => x.Id == id);

        if (!ticketExists)
            return NotFound("Ticket not found.");

        var history = await _context.TicketStatusHistories
            .Include(x => x.ChangedByUser)
            .Where(x => x.TicketId == id)
            .OrderBy(x => x.ChangedAt)
            .Select(x => new TicketStatusHistoryDto
            {
                Id = x.Id,
                TicketId = x.TicketId,
                OldStatus = x.OldStatus.ToString(),
                NewStatus = x.NewStatus.ToString(),
                ChangedByUserId = x.ChangedByUserId,
                ChangedByUserName = x.ChangedByUser != null ? x.ChangedByUser.FullName : string.Empty,
                ChangedAt = x.ChangedAt
            })
            .ToListAsync();

        return Ok(history);
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketDto dto)
    {
        var categoryExists = await _context.Categories.AnyAsync(x => x.Id == dto.CategoryId);

        if (!categoryExists)
            return BadRequest("The informed category does not exist.");

        var createdByUserExists = await _context.Users.AnyAsync(x => x.Id == dto.CreatedByUserId);

        if (!createdByUserExists)
            return BadRequest("The informed createdByUserId does not exist.");

        if (dto.AssignedToUserId.HasValue)
        {
            var assignedUserExists = await _context.Users.AnyAsync(x => x.Id == dto.AssignedToUserId.Value);

            if (!assignedUserExists)
                return BadRequest("The informed assignedToUserId does not exist.");
        }

        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            CreatedByUserId = dto.CreatedByUserId,
            AssignedToUserId = dto.AssignedToUserId,
            Priority = (TicketPriority)dto.Priority,
            Status = TicketStatus.Open
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        var createdTicket = await _context.Tickets
            .Include(x => x.Category)
            .Include(x => x.AssignedToUser)
            .Include(x => x.CreatedByUser)
            .Where(x => x.Id == ticket.Id)
            .Select(x => new TicketDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Status = x.Status.ToString(),
                Priority = x.Priority.ToString(),
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                CreatedByUserId = x.CreatedByUserId,
                CreatedByUserName = x.CreatedByUser != null ? x.CreatedByUser.FullName : string.Empty,
                AssignedToUserId = x.AssignedToUserId,
                AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.FullName : null,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, createdTicket);
    }

}