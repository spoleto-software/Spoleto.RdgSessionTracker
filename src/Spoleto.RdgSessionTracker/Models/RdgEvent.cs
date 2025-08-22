namespace Spoleto.RdgSessionTracker.Models
{
    public record RdgEvent
        (
            DateTime DisconnectTime,
            string UserName,
            string ClientIp,
            string Resource,
            int DurationSeconds,
            string Protocol
        )
    {
        public DateTime ConnectTime => DisconnectTime.AddSeconds(-DurationSeconds);
    }
}
