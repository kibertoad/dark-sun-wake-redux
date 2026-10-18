# music_level

The music level the game asks the sound library for: the `UINT8` the game keeps at `57E0:26B4` in BLD-GOG-EN-1.1, saved at `PREF/100` offset `0x02`. The only instructions found that write it are the Load Game copy and the Preferences screen's entry [FND-CONFIG-012, FND-CONFIG-029, FND-CONFIG-219].
