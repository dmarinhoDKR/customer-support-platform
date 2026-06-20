using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerSupport.API.DTOs;
using FluentAssertions;

namespace CustomerSupport.Tests.Tickets;

public class GetTicketStatusHistoryTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetTicketStatusHistoryTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTicketStatusHistory_AfterStatusUpdate_ShouldReturnHistoryItems()
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
            Title = "Ticket for status history test",
            Description = "This ticket will have its status updated during automated testing.",
            CategoryId = 1,
            CreatedByUserId = 1,
            AssignedToUserId = null,
            Priority = 2
        });

        createTicketResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTicket = await createTicketResponse.Content.ReadFromJsonAsync<TicketResponseDto>();

        createdTicket.Should().NotBeNull();

        var updateStatusResponse = await _client.PutAsJsonAsync(
            $"/api/Tickets/{createdTicket!.Id}/status",
            new UpdateTicketStatusDto
            {
                NewStatus = 2,
                ChangedByUserId = 1
            });

        updateStatusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var historyResponse = await _client.GetAsync($"/api/Tickets/{createdTicket.Id}/status-history");

        historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await historyResponse.Content.ReadFromJsonAsync<List<TicketStatusHistoryItemDto>>();

        history.Should().NotBeNull();
        history.Should().NotBeEmpty();

        var latestEntry = history!.First();

        latestEntry.TicketId.Should().Be(createdTicket.Id);
        latestEntry.ChangedByUserId.Should().Be(1);
        latestEntry.NewStatus.Should().Be("InProgress");
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

    public class TicketStatusHistoryItemDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public int ChangedByUserId { get; set; }
        public string ChangedByUserName { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}