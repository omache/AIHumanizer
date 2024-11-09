using AIHumanizer.Models;

public static class SampleTracker
{
    private static readonly Dictionary<string, SampleUsage> _usageTracker = new();
    private static readonly object _lock = new();
    private const int NON_AUTHENTICATED_SAMPLE_COUNT = 2;
    private const int AUTHENTICATED_SAMPLE_COUNT = 5;
    private const int HOURS_UNTIL_RESET = 24;

    public static SampleUsage GetOrCreateUsage(string identifier, bool isAuthenticated)
    {
        lock (_lock)
        {
            if (_usageTracker.TryGetValue(identifier, out var usage))
            {
                // Check if 24 hours have passed since last reset
                if (DateTime.UtcNow.Subtract(usage.LastReset).TotalHours >= HOURS_UNTIL_RESET)
                {
                    usage.RemainingCount = isAuthenticated ? AUTHENTICATED_SAMPLE_COUNT : NON_AUTHENTICATED_SAMPLE_COUNT;
                    usage.LastReset = DateTime.UtcNow;
                }
                return usage;
            }

            // Create new usage tracker entry
            var newUsage = new SampleUsage
            {
                IpAddress = isAuthenticated ? null : identifier, // Only store IP for non-authenticated users
                RemainingCount = isAuthenticated ? AUTHENTICATED_SAMPLE_COUNT : NON_AUTHENTICATED_SAMPLE_COUNT,
                LastReset = DateTime.UtcNow
            };
            _usageTracker[identifier] = newUsage;
            return newUsage;
        }
    }

    public static bool DecrementSample(string identifier)
    {
        lock (_lock)
        {
            if (_usageTracker.TryGetValue(identifier, out var usage) && usage.RemainingCount > 0)
            {
                usage.RemainingCount--;
                return true;
            }
            return false;
        }
    }
}
