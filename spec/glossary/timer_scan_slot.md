# timer_scan_slot

The slot the timer interrupt handler is serving: a `UINT16` the game keeps at `4842:011E` in BLD-GOG-EN-1.1, which the handler counts from 0 to 16 as it scans the slots; `choose_timer_period` and the routine that restores the old interrupt vector set it to `0xFFFF`, so a change made during a callback makes the scan start again from slot 0 [FND-TIME-007, FND-TIME-008]. No other build has been described.
