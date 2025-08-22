namespace Spoleto.RdgSessionTracker.Models
{
    public record UserSummarySession
        (string UserName,
        DateTime Start,
        DateTime End,
        TimeSpan TotalDuration);
}
