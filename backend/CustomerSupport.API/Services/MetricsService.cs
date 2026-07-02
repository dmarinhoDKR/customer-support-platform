namespace CustomerSupport.API.Services;

public class MetricsService
{
    private readonly DateTime _startedAtUtc = DateTime.UtcNow;
    private int _totalRequests;
    private int _failedRequests;

    public DateTime StartedAtUtc => _startedAtUtc;
    public int TotalRequests => _totalRequests;
    public int FailedRequests => _failedRequests;
    public double UptimeSeconds => (DateTime.UtcNow - _startedAtUtc).TotalSeconds;

    public void RegisterRequest(int statusCode)
    {
        Interlocked.Increment(ref _totalRequests);

        if (statusCode >= 400)
        {
            Interlocked.Increment(ref _failedRequests);
        }
    }
}