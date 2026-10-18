# master_sound_on

True while the game has sound at all: the `UINT8` the game keeps at `57E0:13F7` in BLD-GOG-EN-1.1 is not 0. While it is 0 the music and sound-effects settings are forced off [FND-CONFIG-218].
