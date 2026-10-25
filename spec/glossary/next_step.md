# next_step

A `UINT8[]` the game keeps at `4F49:0374` in BLD-GOG-EN-1.1: for each slot, one byte every 28 bytes, the direction, 0 to 7, of the slot's next step, or `0xFF` for none [FND-EXPLORE-014]. The 28-byte records start at `4F49:0363`. No other build has been described.
