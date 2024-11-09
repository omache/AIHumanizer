using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AIHumanizer.Models;
namespace AIHumanizer.Models

{public static class SampleTracker
{
    private static readonly Dictionary<string, SampleUsage> _usageTracker = new();
    private static readonly object _lock = new();
    private const int INITIAL_SAMPLE_COUNT = 5;
    private const int HOURS_UNTIL_RESET = 24;

    public static SampleUsage GetOrCreateUsage(string ipAddress)
    {
        lock (_lock)
        {
            if (_usageTracker.TryGetValue(ipAddress, out var usage))
            {
                // Check if 24 hours have passed since last reset
                if (DateTime.UtcNow.Subtract(usage.LastReset).TotalHours >= HOURS_UNTIL_RESET)
                {
                    usage.RemainingCount = INITIAL_SAMPLE_COUNT;
                    usage.LastReset = DateTime.UtcNow;
                }
                return usage;
            }

            // Create new usage tracker for IP
            var newUsage = new SampleUsage
            {
                IpAddress = ipAddress,
                RemainingCount = INITIAL_SAMPLE_COUNT,
                LastReset = DateTime.UtcNow
            };
            _usageTracker[ipAddress] = newUsage;
            return newUsage;
        }
    }

    public static bool DecrementSample(string ipAddress)
    {
        lock (_lock)
        {
            var usage = GetOrCreateUsage(ipAddress);
            if (usage.RemainingCount <= 0) return false;
            usage.RemainingCount--;
            return true;
        }
    }
}}