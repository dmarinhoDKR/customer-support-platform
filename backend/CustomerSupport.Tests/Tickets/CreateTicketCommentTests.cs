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
    public async Task CreateComment_WithValidPayload_ShouldReturnCreatedComment()
    {
        await AuthenticateAsync();

        var createTicketResponse = await _client.PostAsJsonAsync("/api/Tickets", new CreateTicketDto
        {
            Title = "Ticket for comment creation",
            Description = "Created for comment creation test.",
            CategoryId = 1,
            CreatedByUserId = 1,
            AssignedToUserId = null,
            Priority = 2
        });

        createTicketResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTicket = await createTicketResponse.Content.ReadFromJsonAsync<TicketResponseDto>();

        createdTicket.Should().NotBeNull();

        var response = await _client.PostAsJsonAsync(
            $"/api/Tickets/{createdTicket!.Id}/comments",
            new CreateTicketCommentDto
            {
                UserId = 1,
                Content = "Comment created by integration test"
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TicketCommentResponseDto>();

        body.Should().NotBeNull();
        body!.TicketId.Should().Be(createdTicket.Id);
        body.UserId.Should().Be(1);
        body.Content.Should().Be("Comment created by integration test");
        body.UserName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateComment_WithNonExistingTicket_ShouldReturnNotFound()
    {
        await AuthenticateAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/Tickets/9999/comments",
            new CreateTicketCommentDto
            {
                UserId = 1,
                Content = "Invalid comment target"
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task AuthenticateAsync()
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
    }

    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class TicketCommentResponseDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}