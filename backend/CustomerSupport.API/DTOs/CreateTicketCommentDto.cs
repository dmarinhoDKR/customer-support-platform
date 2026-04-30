namespace CustomerSupport.API.DTOs;

public class CreateTicketCommentDto
{
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
}