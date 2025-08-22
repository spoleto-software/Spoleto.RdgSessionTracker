using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Text.RegularExpressions;
using Spoleto.RdgSessionTracker.Models;

namespace Spoleto.RdgSessionTracker
{
    public class RdgEventReader
    {
        private const string LogName = "Microsoft-Windows-TerminalServices-Gateway/Operational";

        // Regex works with both English and Russian formats
        private static readonly Regex _eventRegex = new(
            @"user ""(?<user>[^""]+)"", on client computer ""(?<ip>[^""]+)"", disconnected from the following network resource: ""(?<resource>[^""]+)""\..+?session duration was (?<duration>\d+) seconds.+?protocol used: ""(?<protocol>[^""]+)""",
            RegexOptions.Compiled
        );

        // Russian version support
        private static readonly Regex _eventRegexRu = new(
            @"Пользователь ""(?<user>[^""]+)"" на клиентском компьютере ""(?<ip>[^""]+)"" отключился от следующего сетевого ресурса: ""(?<resource>[^""]+)""\..+?Длительность сеанса клиента составила (?<duration>\d+) с\..+?Использован протокол подключения ""(?<protocol>[^""]+)""",
            RegexOptions.Compiled
        );

        public List<RdgEvent> GetEvents(DateTime since, DateTime to, string? machineName = null)
        {
            var events = new List<RdgEvent>();

            var query = $"*[System[(EventID=303) and TimeCreated[@SystemTime >= '{since.ToUniversalTime():o}' and @SystemTime <= '{to.ToUniversalTime():o}']]]";

            var eventQuery = new EventLogQuery(
                LogName,
                PathType.LogName,
                query
            );

            if (!string.IsNullOrEmpty(machineName))
            {
                eventQuery.Session = new EventLogSession(machineName);
            }

            using var reader = new EventLogReader(eventQuery);

            for (EventRecord record = reader.ReadEvent(); record != null; record = reader.ReadEvent())
            {
                try
                {
                    string msg = record.FormatDescription();
                    if (string.IsNullOrWhiteSpace(msg))
                        continue;

                    var match = _eventRegex.Match(msg);
                    if (!match.Success)
                        match = _eventRegexRu.Match(msg);

                    if (!match.Success)
                        continue;

                    var disconnectTime = record.TimeCreated ?? DateTime.MinValue;
                    var user = match.Groups["user"].Value;
                    var ip = match.Groups["ip"].Value;
                    var resource = match.Groups["resource"].Value;
                    var duration = int.Parse(match.Groups["duration"].Value, CultureInfo.InvariantCulture);
                    var protocol = match.Groups["protocol"].Value;

                    events.Add(new RdgEvent(
                        disconnectTime,
                        user,
                        ip,
                        resource,
                        duration,
                        protocol
                    ));
                }
                catch
                {
                    // Ignore bad events
                }
            }

            return Deduplicate(events);
        }

        /// <summary>
        /// Removes near-duplicate events (same user, same disconnect time +/- 5 sec, almost same duration)
        /// Keeps the first one.
        /// </summary>
        private static List<RdgEvent> Deduplicate(List<RdgEvent> events)
        {
            return events
                .OrderBy(e => e.ConnectTime)
                .GroupBy(e => e.UserName.ToLowerInvariant())
                .SelectMany(group =>
                {
                    var result = new List<RdgEvent>();

                    foreach (var current in group)
                    {
                        bool overlapFound = false;

                        for (int i = 0; i < result.Count; i++)
                        {
                            var existing = result[i];

                            // check time overlap
                            bool overlap =
                                current.ConnectTime < existing.DisconnectTime
                                && existing.ConnectTime < current.DisconnectTime;

                            if (overlap)
                            {
                                overlapFound = true;

                                // keep the longest
                                if (current.DurationSeconds > existing.DurationSeconds)
                                {
                                    result[i] = current;
                                }

                                break;
                            }
                        }

                        if (!overlapFound)
                        {
                            result.Add(current);
                        }
                    }

                    return result;
                })
                .ToList();
        }

        /// <summary>
        /// Gets for each user: earliest start (computed as last disconnect - total duration) and last disconnect.
        /// </summary>
        public List<UserSummarySession> GetSummarySessions(DateTime since, DateTime to, string? machineName = null)
        {
            var events = GetEvents(since, to, machineName);

            return events
                .GroupBy(e => e.UserName)
                .Select(g => 
                {
                    var totalDuration = TimeSpan.FromSeconds(g.Sum(e => e.DurationSeconds));
                    var lastDisconnect = g.Max(e => e.DisconnectTime);
                    var start = lastDisconnect - totalDuration;
                    return new UserSummarySession(g.Key, start, lastDisconnect, totalDuration);
                })
                .ToList();
        }
    }
}
