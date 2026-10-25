# timer_accumulators

The time each of the 17 timer slots of RULE-TIME-002 has run since its callback last ran, in microseconds: a `UINT32[17]` the game keeps at `4842:008E` in BLD-GOG-EN-1.1. The interrupt handler adds `timer_period` to a running slot's entry and calls the slot when it reaches the slot's period; `choose_timer_period` clears them all when the period changes, and setting a slot's period clears its entry [FND-TIME-008]. No other build has been described.
