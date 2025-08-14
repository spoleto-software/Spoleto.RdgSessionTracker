# Spoleto.RdgSessionTracker

**Spoleto.RdgSessionTracker** is a .NET library for reading **Remote Desktop Gateway** (RD Gateway) session disconnect events from the Windows Event Log and calculating **user daily working time** based on session durations.

It is designed for scenarios where you need to track when users connect/disconnect through an RD Gateway and store the calculated working periods in a database or reporting system.

---

## Features

- Reads RD Gateway events (e.g., Event ID **303**) from **Microsoft-Windows-TerminalServices-Gateway** log.
- Filters events by date range (`since` / `to`).
- Parses the event message to extract:
  - **Username** (`DOMAIN\user`)
  - **Client IP address**
  - **Remote resource name**
  - **Session duration (seconds)**
- Groups events by user per day.
- Calculates:
  - **Total working time** for the day (sum of all session durations).
  - **Start of the working day** = `Last disconnect - Total duration`.
  - **End of the working day** = `Last disconnect`.
- Handles duplicate events for the same disconnect (different protocols, same time) — processes only the first unique event.
- Supports reading logs **locally** or from a **remote computer** (via `EventLogSession`).

---

## Example

```csharp
var reader = new RdgEventReader();

// Load yesterday's events from the local or remote server
var events = reader.GetEvents(
    since: DateTime.Today.AddDays(-1),
    to: DateTime.Today,
    machineName: "sv-server" // set to null for local machine
	);
	
foreach (var session in events)
{
    Console.WriteLine($"{session.Date:yyyy-MM-dd} | {session.UserName} | Start: {session.Start} | End: {session.End} | Total: {session.TotalDuration}");
}	

// Calculate per-user daily sessions (start, end, total duration)
var dailySessions = reader.GetDailySessions(
    since: DateTime.Today.AddDays(-1),
    to: DateTime.Today,
    machineName: "sv-server" // set to null for local machine
	);

foreach (var session in dailySessions)
{
    Console.WriteLine(
        $"{session.UserName}: Start={session.Start}, End={session.End}, Total={session.TotalDuration}"
    );
}
```

## Event Source
The events come from:

Log Name: `Microsoft-Windows-TerminalServices-Gateway/Operational`
Event ID: `303` — User disconnected from a network resource.

Example Event Text:

```csharp
The user "DOMAIN\user", on client computer "10.0.0.10", disconnected from the following network resource: "sv-term.domain.com". Before the user disconnected, the client transferred 2403328 bytes and received 35761296 bytes. The client session duration was 2552 seconds. Connection protocol used: "UDP".
```

## Requirements
- Windows Server 2016+ / Windows 10+
- RD Gateway role enabled
- .NET 8.0 (Windows only)
- Event log access permissions

## Installation
```powershell
dotnet add package Spoleto.RdgSessionTracker
```

## License
MIT License