using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerSupport.API.DTOs;
using FluentAssertions;

namespace CustomerSupport.Tests.Tickets;

public class CreateTicketCommentTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateTicketCommentTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTicketComment_WithValidPayload_ShouldReturnCreatedComment()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new LoginDto
        {
            Email = "admin@customersupport.com",
            Password = "admin123"
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        authBody.Should().NotBeNull();
        authBody!.Token.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authBody.Token);

        var createTicketResponse = await _client.PostAsJsonAsync("/api/Tickets", new CreateTicketDto
        {
            Title = "Ticket for comment test",
            Description = "This ticket will receive a comment during automated testing.",
            CategoryId = 1,
            CreatedByUserId = 1,
            AssignedToUserId = null,
            Priority = 2
        });

        createTicketResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTicket = await createTicketResponse.Content.ReadFromJsonAsync<TicketResponseDto>();

        createdTicket.Should().NotBeNull();

        var commentRequest = new CreateTicketCommentDto
        {
            UserId = 1,
            Content = "This is a test comment created by the integration test."
        };

        var commentResponse = await _client.PostAsJsonAsync(
            $"/api/Tickets/{createdTicket!.Id}/comments",
            commentRequest
        );

        commentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdComment = await commentResponse.Content.ReadFromJsonAsync<TicketCommentResponseDto>();

        createdComment.Should().NotBeNull();
        createdComment!.TicketId.Should().Be(createdTicket.Id);
        createdComment.UserId.Should().Be(commentRequest.UserId);
        createdComment.Content.Should().Be(commentRequest.Content);
    }

    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public int? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
    }

    public class TicketCommentResponseDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}