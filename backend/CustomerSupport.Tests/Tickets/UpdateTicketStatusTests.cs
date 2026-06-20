using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerSupport.API.DTOs;
using FluentAssertions;

namespace CustomerSupport.Tests.Tickets;

public class UpdateTicketStatusTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UpdateTicketStatusTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateTicketStatus_WithValidPayload_ShouldReturnUpdatedTicket()
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
            Title = "Ticket for status update test",
            Description = "This ticket will be updated during automated testing.",
            CategoryId = 1,
            CreatedByUserId = 1,
            AssignedToUserId = null,
            Priority = 2
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTicket = await createResponse.Content.ReadFromJsonAsync<TicketResponseDto>();

        createdTicket.Should().NotBeNull();

        var updateRequest = new UpdateTicketStatusDto
        {
            NewStatus = 2,
            ChangedByUserId = 1
        };

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/Tickets/{createdTicket!.Id}/status",
            updateRequest
        );

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedTicket = await updateResponse.Content.ReadFromJsonAsync<TicketResponseDto>();

        updatedTicket.Should().NotBeNull();
        updatedTicket!.Id.Should().Be(createdTicket.Id);
        updatedTicket.Status.Should().Be("InProgress");
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