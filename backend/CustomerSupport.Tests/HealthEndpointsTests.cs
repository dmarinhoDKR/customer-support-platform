using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

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
        public int CommentsCreated { get; set; }
        public int StatusUpdates { get; set; }
        public int Assignments { get; set; }
    }
}