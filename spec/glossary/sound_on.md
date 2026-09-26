# sound_on

A `UINT8` the game keeps at `57E0:13F7` in BLD-GOG-EN-1.1: not 0 while the game makes any sound. It starts at 1, and the command-line switch `-M` sets it to 0 [FND-SOUND-010]. No other build has been described.
