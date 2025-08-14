namespace Spoleto.RdgSessionTracker.Tests.Models
{
    public record TestOption
    {
        public DateTime Since { get; set; }
        public DateTime To { get; set; }
        public string? MachineName { get; set; }
    }
}
