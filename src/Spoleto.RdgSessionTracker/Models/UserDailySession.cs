namespace Spoleto.RdgSessionTracker.Models
{
    public record UserDailySession
        (string UserName,
        DateTime Start,
        DateTime End,
        TimeSpan TotalDuration);
}
