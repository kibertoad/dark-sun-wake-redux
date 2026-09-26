# TIME

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `RULE-TIME-001` | The game waits a number of milliseconds by reading the timer chip until enough counts have passed | supported | missing | None | None | supported | The rebuild has no fixed pauses of the original. Its runtime advances on host time through a fixed-step accumulator with bounded catch-up and never busy-waits on hardware. |
| `RULE-TIME-002` | The timer interrupt runs at the shortest period any of 17 timer slots asks for, given in microseconds | supported | missing | None | None | supported | The rebuild does not program a timer or keep timer slots; the modern runtime supplies frame timing. |
