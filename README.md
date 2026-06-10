```markdown
# PriceBlotter

## Running the app
```
cd PriceBlotter.Api && dotnet run
cd priceblotter-ui && npm install && npm run dev
```

## Architecture
SignalR WebSocket was chosen to minimise traffic. The full list of 10 items is sent once on subscribe. Only 3-5 changed items are pushed per second after that.

## AG Grid
Industry-standard data grid for financial UIs. getRowId enables delta row updates so only changed rows re-render.

## Scalability note
Client state uses a simple array (O(n)), which is fine for 10 items. For thousands of instruments this would be replaced with Map<number, PriceItem> for O(1) merge performance.

## Potential improvements
- Starting prices use realistic hardcoded midpoints. A random number generator could initialise each pair within a small spread around its midpoint, making each session start differently.
- The SignalR hub has no authentication. Production would require JWT or cookie-based auth.
- No reconnection logic on the client. Production would use SignalR's built-in withAutomaticReconnect().
- No unit tests. PriceService tick logic and the usePriceSocket hook behaviour would be the first things to cover.
```