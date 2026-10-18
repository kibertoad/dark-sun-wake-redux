# voice_on

True while the Preferences voice button is on, which only matters in the CD version: the `UINT8` the game keeps at `57E0:14E4` in BLD-GOG-EN-1.1, true when not 0. Speech plays only while it and the saved byte `57E0:1439` are both not 0 [FND-CONFIG-010, FND-CONFIG-014].
