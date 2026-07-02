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
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(
        AppDbContext context,
        ILogger<TicketsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TicketDto>>> GetAll([FromQuery] TicketFilterDto filter)
    {
        _logger.LogInformation(
            "Listing tickets with filters Status={Status}, Priority={Priority}, AssignedToUserId={AssignedToUserId}, CategoryId={CategoryId}, PageNumber={PageNumber}, PageSize={PageSize}, Limit={Limit}, Offset={Offset}, SortBy={SortBy}, SortDirection={SortDirection}.",
            filter.Status,
            filter.Priority,
            filter.AssignedToUserId,
            filter.CategoryId,
            filter.PageNumber,
            filter.PageSize,
            filter.Limit,
            filter.Offset,
            filter.SortBy,
            filter.SortDirection);

        var query = _context.Tickets
            .Include(x => x.Category)
            .Include(x => x.AssignedToUser)
            .Include(x => x.CreatedByUser)
            .AsQueryable();

        if (filter.Status.HasValue)
        {
            if (!Enum.IsDefined(typeof(TicketStatus), filter.Status.Value))
            {
                _logger.LogWarning("Ticket listing failed. Invalid status filter {Status}.", filter.Status);
                return BadRequest("The informed status filter is invalid.");
            }

            var status = (TicketStatus)filter.Status.Value;
            query = query.Where(x => x.Status == status);
        } 

        if (filter.Priority.HasValue)
        {
            if (!Enum.IsDefined(typeof(TicketPriority), filter.Priority.Value))
            {
                _logger.LogWarning("Ticket listing failed. Invalid priority filter {Priority}.", filter.Priority);
                return BadRequest("The informed priority filter is invalid.");
            }

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

        _logger.LogInformation(
            "Tickets listed successfully. Returned {ReturnedCount} items out of {TotalCount}.",
            tickets.Count,
            totalCount);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDto>> GetById(int id)
    {
        _logger.LogInformation("Fetching ticket {TicketId}.", id);

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
        {
            _logger.LogWarning("Ticket {TicketId} was not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Ticket {TicketId} fetched successfully.", id);
        return Ok(ticket);
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<TicketCommentDto>>> GetComments(int id)
    {
        _logger.LogInformation("Fetching comments for ticket {TicketId}.", id);

        var ticketExists = await _context.Tickets.AnyAsync(x => x.Id == id);

        if (!ticketExists)
        {
            _logger.LogWarning("Comments fetch failed. Ticket {TicketId} was not found.", id);
            return NotFound("Ticket not found.");
        }

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

        _logger.LogInformation(
            "Comments fetched successfully for ticket {TicketId}. Returned {CommentCount} comments.",
            id,
            comments.Count);
        return Ok(comments);
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<TicketCommentDto>> AddComment(int id, CreateTicketCommentDto dto)
    {
        _logger.LogInformation(
            "Creating comment for ticket {TicketId} by user {UserId}.",
            id,
            dto.UserId);

        var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
        {
            _logger.LogWarning("Comment creation failed. Ticket {TicketId} was not found.", id);
            return NotFound("Ticket not found.");
        }

        var userExists = await _context.Users.AnyAsync(x => x.Id == dto.UserId);

        if (!userExists)
        {
            _logger.LogWarning(
                "Comment creation failed. User {UserId} was not found for ticket {TicketId}.",
                dto.UserId,
                id);
            return BadRequest("The informed userId does not exist.");
        }

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

        _logger.LogInformation(
            "Comment {CommentId} created successfully for ticket {TicketId}.",
            createdComment.Id,
            id);

        return Ok(createdComment);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<TicketDto>> UpdateStatus(int id, UpdateTicketStatusDto dto)
    {
        _logger.LogInformation(
            "Updating status for ticket {TicketId} to {NewStatus} by user {ChangedByUserId}.",
            id,
            dto.NewStatus,
            dto.ChangedByUserId);

        var ticket = await _context.Tickets
            .Include(x => x.Category)
            .Include(x => x.AssignedToUser)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
        {
            _logger.LogWarning("Status update failed. Ticket {TicketId} was not found.", id);
            return NotFound("Ticket not found.");
        }

        var userExists = await _context.Users.AnyAsync(x => x.Id == dto.ChangedByUserId);

        if (!userExists)
        {
            _logger.LogWarning(
                "Status update failed. ChangedByUserId {ChangedByUserId} was not found for ticket {TicketId}.",
                dto.ChangedByUserId,
                id);
            return BadRequest("The informed changedByUserId does not exist.");
        }

        if (!Enum.IsDefined(typeof(TicketStatus), dto.NewStatus))
        {
            _logger.LogWarning(
                "Status update failed. Invalid new status {NewStatus} for ticket {TicketId}.",
                dto.NewStatus,
                id);
            return BadRequest("The informed newStatus is invalid.");
        }

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

        _logger.LogInformation(
            "Ticket {TicketId} status updated successfully from {OldStatus} to {NewStatus}.",
            ticket.Id,
            oldStatus,
            newStatus);
        return Ok(updatedTicket);
    }

    [HttpPut("{id:int}/assign")]
    public async Task<ActionResult<TicketDto>> AssignTicket(int id, AssignTicketDto dto)
    {
        _logger.LogInformation(
            "Assigning ticket {TicketId} to user {AssignedToUserId}.",
            id,
            dto.AssignedToUserId);

        var ticket = await _context.Tickets
            .Include(x => x.Category)
            .Include(x => x.AssignedToUser)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ticket is null)
        {
            _logger.LogWarning("Ticket assignment failed. Ticket {TicketId} was not found.", id);
            return NotFound("Ticket not found.");
        }

        var assignedUser = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == dto.AssignedToUserId);

        if (assignedUser is null)
        {
            _logger.LogWarning(
                "Ticket assignment failed. Assigned user {AssignedToUserId} was not found for ticket {TicketId}.",
                dto.AssignedToUserId,
                id);
            return BadRequest("The informed assignedToUserId does not exist.");
        }

        if (assignedUser.Role is null)
        {
            _logger.LogError(
                "Ticket assignment failed. Assigned user {AssignedToUserId} has no role assigned.",
                dto.AssignedToUserId);
            return StatusCode(500, "Assigned user role is not assigned.");
        }

        if (assignedUser.Role.Name != "Agent" && assignedUser.Role.Name != "Admin")
        {
            _logger.LogWarning(
                "Ticket assignment failed. User {AssignedToUserId} with role {Role} cannot be assigned to ticket {TicketId}.",
                dto.AssignedToUserId,
                assignedUser.Role.Name,
                id);
            return BadRequest("Only Admin or Agent users can be assigned to tickets.");
        }

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

        _logger.LogInformation(
            "Ticket {TicketId} assigned successfully to user {AssignedToUserId}.",
            ticket.Id,
            dto.AssignedToUserId);
        return Ok(updatedTicket);
    }

    [HttpGet("{id:int}/status-history")]
    public async Task<ActionResult<IEnumerable<TicketStatusHistoryDto>>> GetStatusHistory(int id)
    {
        _logger.LogInformation("Fetching status history for ticket {TicketId}.", id);

        var ticketExists = await _context.Tickets.AnyAsync(x => x.Id == id);

        if (!ticketExists)
        {
            _logger.LogWarning("Status history fetch failed. Ticket {TicketId} was not found.", id);
            return NotFound("Ticket not found.");
        }

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

        _logger.LogInformation(
            "Status history fetched successfully for ticket {TicketId}. Returned {HistoryCount} items.",
            id,
            history.Count);
        return Ok(history);
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketDto dto)
    {
        _logger.LogInformation(
            "Creating ticket with title {Title} for user {CreatedByUserId}.",
            dto.Title,
            dto.CreatedByUserId); 
        var categoryExists = await _context.Categories.AnyAsync(x => x.Id == dto.CategoryId);

        if (!categoryExists)
        {
            _logger.LogWarning(
                "Ticket creation failed. Category {CategoryId} was not found.",
                dto.CategoryId);
            return BadRequest("The informed category does not exist.");
        }

        var createdByUserExists = await _context.Users.AnyAsync(x => x.Id == dto.CreatedByUserId);

        if (!createdByUserExists)
        {
            _logger.LogWarning(
                "Ticket creation failed. CreatedByUserId {CreatedByUserId} was not found.",
                dto.CreatedByUserId);
            return BadRequest("The informed createdByUserId does not exist.");
        }

        if (dto.AssignedToUserId.HasValue)
        {
            var assignedUserExists = await _context.Users.AnyAsync(x => x.Id == dto.AssignedToUserId.Value);

            if (!assignedUserExists)
            {
                _logger.LogWarning(
                    "Ticket creation failed. AssignedToUserId {AssignedToUserId} was not found.",
                    dto.AssignedToUserId.Value);
                return BadRequest("The informed assignedToUserId does not exist.");
            }
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

        _logger.LogInformation(
            "Ticket {TicketId} created successfully.",
            ticket.Id);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, createdTicket);
    }

}