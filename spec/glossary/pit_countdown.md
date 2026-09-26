# pit_countdown

A value from outside the game: a `UINT16`, the complement of the count of channel 0 of the timer chip, which the original reads by writing 0 to port `0x43` and reading the low and high byte from port `0x40` with interrupts off, in the routine at `1000:12BF` in BLD-GOG-EN-1.1 [FND-TIME-004]. It changes all the time, going up as the chip counts down.
