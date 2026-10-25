# step_index

A `UINT8[]` the game keeps at `4F49:0373` in BLD-GOG-EN-1.1: for each slot, one byte every 28 bytes, the index in `step_queue` of the next queued direction [FND-EXPLORE-014]. The 28-byte records start at `4F49:0363`. No other build has been described.
