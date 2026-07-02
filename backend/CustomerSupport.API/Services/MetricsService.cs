namespace CustomerSupport.API.Services;

public class MetricsService
{
    private readonly DateTime _startedAtUtc = DateTime.UtcNow;

    private int _totalRequests;
    private int _successfulRequests;
    private int _failedRequests;

    private int _loginAttempts;
    private int _loginSuccesses;
    private int _loginFailures;

    private int _ticketsCreated;
    private int _commentsCreated;
    private int _statusUpdates;
    private int _ticketAssignments;

    public DateTime StartedAtUtc => _startedAtUtc;
    public double UptimeSeconds => (DateTime.UtcNow - _startedAtUtc).TotalSeconds;

    public int TotalRequests => _totalRequests;
    public int SuccessfulRequests => _successfulRequests;
    public int FailedRequests => _failedRequests;

    public int LoginAttempts => _loginAttempts;
    public int LoginSuccesses => _loginSuccesses;
    public int LoginFailures => _loginFailures;

    public int TicketsCreated => _ticketsCreated;
    public int CommentsCreated => _commentsCreated;
    public int StatusUpdates => _statusUpdates;
    public int TicketAssignments => _ticketAssignments;

    public void RegisterRequest(int statusCode)
    {
        Interlocked.Increment(ref _totalRequests);

        if (statusCode >= 400)
        {
            Interlocked.Increment(ref _failedRequests);
            return;
        }

        Interlocked.Increment(ref _successfulRequests);
    }

    public void RegisterLoginAttempt()
    {
        Interlocked.Increment(ref _loginAttempts);
    }

    public void RegisterLoginSuccess()
    {
        Interlocked.Increment(ref _loginSuccesses);
    }

    public void RegisterLoginFailure()
    {
        Interlocked.Increment(ref _loginFailures);
    }

    public void RegisterTicketCreated()
    {
        Interlocked.Increment(ref _ticketsCreated);
    }

    public void RegisterCommentCreated()
    {
        Interlocked.Increment(ref _commentsCreated);
    }

    public void RegisterStatusUpdate()
    {
        Interlocked.Increment(ref _statusUpdates);
    }

    public void RegisterTicketAssignment()
    {
        Interlocked.Increment(ref _ticketAssignments);
    }
}