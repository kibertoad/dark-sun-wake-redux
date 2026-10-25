# step_queue

A `UINT8[16][]` the game keeps at `4F49:0363` in BLD-GOG-EN-1.1: for each slot, 16 bytes every 28 bytes, the directions queued after `next_step`, 0 to 7, ended by a value outside that range [FND-EXPLORE-014]. The 28-byte records start at `4F49:0363`. No other build has been described.
