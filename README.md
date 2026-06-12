Open http://localhost:5173 in a browser.

## Solution

The backend is ASP.NET Core Web API using SignalR to stream price updates over WebSocket. 
10 FX pairs are simulated with realistic per-tick deltas
 - smaller for majors like EURUSD
 - scaled up for JPY crosses which trade at a different magnitude. 
 - Starting prices are randomised around realistic midpoints so each session begins differently.

On subscribe, the full list of 10 items is sent once to the client. After that, only the items that changed on each tick are pushed, not the full list. Keeping client-server traffic to a minimum.

The frontend is React + TypeScript using AG Grid Community for the blotter. AG Grid's getRowId prop enables delta row updates so only changed rows re-render. The usePriceSocket custom hook manages the SignalR connection lifecycle, keeping data logic separate from the presentation.

The service currently serves mock data. Ultimately, we need to have a working service, and a dev env mock service which we can switch to for development and testing.

The UI is very lightweight and could be considerably improved including tooltip like transient popups with the original Price so that the user can briefly see what it was and what it changed to.

## Swagger

Swagger is included even though there are no REST controllers yet. SignalR hubs do not appear in the Swagger spec as they use WebSocket rather than HTTP. Swagger is only enabled in the Development environment for security reasons. Exposing API documentation in production is a risk. Better to add it now than later as an afterthought because any production service will eventually need REST endpoints alongside the hub, and retrofitting API documentation later is disruptive.

## Scalability note

Client state uses a simple array (O(n) lookup), which is fine for 10 items. For thousands of instruments this would be replaced with a Map keyed by ID for O(1) merge performance.

## Potential improvements

- The SignalR hub has no authentication. Production would require JWT or cookie-based auth on the hub connection.
- No reconnection logic on the client. Production would use SignalR's built-in withAutomaticReconnect().
- No unit tests. PriceService tick logic and the usePriceSocket hook would be the first things to cover.
- Logging currently uses string interpolation for readability. Switching to structured logging placeholders (e.g. `_logger.LogDebug("Pushed {Count} updates", count)`) would make individual fields queryable by name in tools like Splunk or Application Insights, which matters at scale when filtering thousands of log entries.