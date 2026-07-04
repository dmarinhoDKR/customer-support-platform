using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using CustomerSupport.API.DTOs;


namespace CustomerSupport.Tests;

public class HealthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task MetricsEndpoint_ShouldReturnMetricsPayload()
    {
        await _client.GetAsync("/health");
        await AuthenticateAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/Tickets", new CreateTicketDto
        {
            Title = "Metrics ticket",
            Description = "Created to validate metrics counters.",
            CategoryId = 1,
            CreatedByUserId = 1,
            AssignedToUserId = null,
            Priority = 2
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTicket = await createResponse.Content.ReadFromJsonAsync<TicketResponseDto>();

        createdTicket.Should().NotBeNull();

        var ticketId = createdTicket!.Id;

        var getTicketResponse = await _client.GetAsync($"/api/Tickets/{ticketId}");
        getTicketResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getCommentsResponse = await _client.GetAsync($"/api/Tickets/{ticketId}/comments");
        getCommentsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getStatusHistoryResponse = await _client.GetAsync($"/api/Tickets/{ticketId}/status-history");
        getStatusHistoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missingTicketResponse = await _client.GetAsync("/api/Tickets/9999");
        missingTicketResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var missingCommentsResponse = await _client.GetAsync("/api/Tickets/9999/comments");
        missingCommentsResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var missingStatusHistoryResponse = await _client.GetAsync("/api/Tickets/9999/status-history");
        missingStatusHistoryResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var response = await _client.GetAsync("/metrics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<MetricsResponseDto>();

        body.Should().NotBeNull();
        body!.StartedAtUtc.Should().NotBe(default);
        body.UptimeSeconds.Should().BeGreaterThanOrEqualTo(0);

        body.Requests.Should().NotBeNull();
        body.Requests.ByEndpoint.Should().NotBeNull();
        body.Requests.ByEndpoint.Should().ContainKey("/health");
        body.Requests.ByEndpoint.Should().BeAssignableTo<IDictionary<string, int>>();

        body.Auth.Should().NotBeNull();
        body.Tickets.Should().NotBeNull();
        body.Tickets.Fetched.Should().BeGreaterThan(0);
        body.Tickets.CommentsFetched.Should().BeGreaterThan(0);
        body.Tickets.StatusHistoryFetched.Should().BeGreaterThan(0);

        body.Tickets.CreationFailures.Should().BeGreaterThanOrEqualTo(0);
        body.Tickets.FetchFailures.Should().BeGreaterThan(0);
        body.Tickets.CommentCreationFailures.Should().BeGreaterThanOrEqualTo(0);
        body.Tickets.CommentsFetchFailures.Should().BeGreaterThan(0);
        body.Tickets.StatusUpdateFailures.Should().BeGreaterThanOrEqualTo(0);
        body.Tickets.StatusHistoryFetchFailures.Should().BeGreaterThan(0);
        body.Tickets.AssignmentFailures.Should().BeGreaterThanOrEqualTo(0);
    }

    public class MetricsResponseDto
    {
        public DateTime StartedAtUtc { get; set; }
        public double UptimeSeconds { get; set; }
        public RequestsDto Requests { get; set; } = new();
        public AuthDto Auth { get; set; } = new();
        public TicketsDto Tickets { get; set; } = new();
    }

    public class RequestsDto
    {
        public int Total { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public Dictionary<string, int> ByEndpoint { get; set; } = new();
    }

    public class AuthDto
    {
        public int LoginAttempts { get; set; }
        public int LoginSuccesses { get; set; }
        public int LoginFailures { get; set; }
    }

    public class TicketsDto
    {
        public int Created { get; set; }
        public int CreationFailures { get; set; }
        public int Fetched { get; set; }
        public int FetchFailures { get; set; }
        public int CommentsCreated { get; set; }
        public int CommentCreationFailures { get; set; }
        public int CommentsFetched { get; set; }
        public int CommentsFetchFailures { get; set; }
        public int StatusUpdates { get; set; }
        public int StatusUpdateFailures { get; set; }
        public int StatusHistoryFetched { get; set; }
        public int StatusHistoryFetchFailures { get; set; }
        public int Assignments { get; set; }
        public int AssignmentFailures { get; set; }
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