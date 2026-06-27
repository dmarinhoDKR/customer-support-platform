using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerSupport.API.DTOs;
using FluentAssertions;

namespace CustomerSupport.Tests.Tickets;

public class AssignTicketTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AssignTicketTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AssignTicket_WithValidPayload_ShouldUpdateAssignedUser()
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

        var createResponse = await _client.PostAsJsonAsync("/api/Tickets", new CreateTicketDto
        {
            Title = "Ticket for assignment test",
            Description = "Created to validate assignment flow.",
            CategoryId = 1,
            CreatedByUserId = 1,
            AssignedToUserId = null,
            Priority = 2
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTicket = await createResponse.Content.ReadFromJsonAsync<TicketResponseDto>();

        createdTicket.Should().NotBeNull();
        createdTicket!.Id.Should().BeGreaterThan(0);

        var assignResponse = await _client.PutAsJsonAsync(
            $"/api/Tickets/{createdTicket.Id}/assign",
            new AssignTicketDto
            {
                AssignedToUserId = 2
            });

        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedTicket = await assignResponse.Content.ReadFromJsonAsync<TicketResponseDto>();

        updatedTicket.Should().NotBeNull();
        updatedTicket!.AssignedToUserId.Should().Be(2);
        updatedTicket.AssignedToUserName.Should().Be("Agent User");
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
}