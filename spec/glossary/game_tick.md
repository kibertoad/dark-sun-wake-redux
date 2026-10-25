# game_tick

The game's tick count: a `UINT32` the game keeps at `57E0:145E` in BLD-GOG-EN-1.1, which the 96 Hz timer callback `31E0:31A7` adds 1 to and nothing else writes, so it counts 1/96 s ticks from start-up [FND-TIME-012, FND-TIME-010]. Waits and the animation frame timing compare against it [FND-EXE-602, FND-EXPLORE-014, FND-EXPLORE-019, FND-TIME-010]. No other build has been described.
