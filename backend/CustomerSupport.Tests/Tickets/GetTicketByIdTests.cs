using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using System.Net.Http.Headers;
using CustomerSupport.API.DTOs;

namespace CustomerSupport.Tests.Tickets;

public class GetTicketByIdTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetTicketByIdTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTicketById_WithExistingId_ShouldReturnTicket()
    {
        await AuthenticateAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/Tickets", new CreateTicketDto
        {
            Title = "Ticket for get by id test",
            Description = "Created during integration test.",
            CategoryId = 1,
            CreatedByUserId = 1,
            AssignedToUserId = null,
            Priority = 2
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTicket = await createResponse.Content.ReadFromJsonAsync<TicketResponseDto>();

        createdTicket.Should().NotBeNull();

        var response = await _client.GetAsync($"/api/Tickets/{createdTicket!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TicketResponseDto>();

        body.Should().NotBeNull();
        body!.Id.Should().Be(createdTicket.Id);
        body.Title.Should().Be("Ticket for get by id test");
        body.Description.Should().Be("Created during integration test.");
        body.Status.Should().NotBeNullOrWhiteSpace();
        body.Priority.Should().NotBeNullOrWhiteSpace();
        body.CategoryName.Should().NotBeNullOrWhiteSpace();
        body.CreatedByUserName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetTicketById_WithNonExistingId_ShouldReturnNotFound()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/Tickets/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
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
}