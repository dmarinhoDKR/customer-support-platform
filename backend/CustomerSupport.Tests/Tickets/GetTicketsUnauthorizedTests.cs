using System.Net;
using FluentAssertions;

namespace CustomerSupport.Tests.Tickets;

public class GetTicketsUnauthorizedTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetTicketsUnauthorizedTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTickets_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/Tickets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}