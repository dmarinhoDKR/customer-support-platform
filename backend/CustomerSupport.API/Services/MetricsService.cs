using System.Collections.Concurrent;

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
    private int _ticketCreationFailures;
    private int _ticketsFetched;
    private int _ticketFetchFailures;
    private int _commentsCreated;
    private int _commentCreationFailures;
    private int _commentsFetched;
    private int _commentsFetchFailures;
    private int _statusUpdates;
    private int _statusUpdateFailures;
    private int _statusHistoryFetched;
    private int _statusHistoryFetchFailures;
    private int _ticketAssignments;
    private int _ticketAssignmentFailures;

    private readonly ConcurrentDictionary<string, int> _requestsByEndpoint = new();

    public DateTime StartedAtUtc => _startedAtUtc;
    public double UptimeSeconds => (DateTime.UtcNow - _startedAtUtc).TotalSeconds;

    public int TotalRequests => _totalRequests;
    public int SuccessfulRequests => _successfulRequests;
    public int FailedRequests => _failedRequests;

    public int LoginAttempts => _loginAttempts;
    public int LoginSuccesses => _loginSuccesses;
    public int LoginFailures => _loginFailures;

    public int TicketsCreated => _ticketsCreated;
    public int TicketCreationFailures => _ticketCreationFailures;
    public int TicketsFetched => _ticketsFetched;
    public int TicketFetchFailures => _ticketFetchFailures;
    public int CommentsCreated => _commentsCreated;
    public int CommentCreationFailures => _commentCreationFailures;
    public int CommentsFetched => _commentsFetched;
    public int CommentsFetchFailures => _commentsFetchFailures;
    public int StatusUpdates => _statusUpdates;
    public int StatusUpdateFailures => _statusUpdateFailures;
    public int StatusHistoryFetched => _statusHistoryFetched;
    public int StatusHistoryFetchFailures => _statusHistoryFetchFailures;
    public int TicketAssignments => _ticketAssignments;
    public int TicketAssignmentFailures => _ticketAssignmentFailures;

    public IReadOnlyDictionary<string, int> RequestsByEndpoint => _requestsByEndpoint;

    public void RegisterRequest(string endpoint, int statusCode)
    {
        Interlocked.Increment(ref _totalRequests);

        var normalizedEndpoint = string.IsNullOrWhiteSpace(endpoint) ? "unknown" : endpoint;
        _requestsByEndpoint.AddOrUpdate(normalizedEndpoint, 1, (_, current) => current + 1);

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

    public void RegisterTicketFetched()
    {
        Interlocked.Increment(ref _ticketsFetched);
    }

    public void RegisterCommentsFetched()
    {
        Interlocked.Increment(ref _commentsFetched);
    }

    public void RegisterStatusHistoryFetched()
    {
        Interlocked.Increment(ref _statusHistoryFetched);
    }

    public void RegisterTicketFetchFailure()
    {
        Interlocked.Increment(ref _ticketFetchFailures);
    }

    public void RegisterCommentsFetchFailure()
    {
        Interlocked.Increment(ref _commentsFetchFailures);
    }

    public void RegisterStatusHistoryFetchFailure()
    {
        Interlocked.Increment(ref _statusHistoryFetchFailures);
    }

    public void RegisterTicketCreationFailure()
    {
        Interlocked.Increment(ref _ticketCreationFailures);
    }

    public void RegisterCommentCreationFailure()
    {
        Interlocked.Increment(ref _commentCreationFailures);
    }

    public void RegisterStatusUpdateFailure()
    {
        Interlocked.Increment(ref _statusUpdateFailures);
    }

    public void RegisterTicketAssignmentFailure()
    {
        Interlocked.Increment(ref _ticketAssignmentFailures);
    }
}