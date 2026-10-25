# script_state

The state of the script system: an `INT16` the game keeps at `4C0E:000B` in BLD-GOG-EN-1.1, which only overlay 169 writes, -1, 0, 1, or 2 once the trigger pool, the working buffers and the script buffers are set up; a script run, a region's script and every trigger walker need it to be 2 [FND-SCRIPT-007, FND-SCRIPT-013, FND-SCRIPT-028, FND-CONFIG-156]. No other build has been described.
