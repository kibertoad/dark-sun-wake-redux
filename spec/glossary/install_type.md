# install_type

A `UINT8` the game keeps at `57E0:55BC` in BLD-GOG-EN-1.1: the installation type the switch `-W` gives, 0 to 7, 0 in the GOG build. Bit 1 makes the game delete `1.FLI` and `2.FLI` on entering region `0x43`, and bit 2 the speech files [FND-SOUND-006, FND-SOUND-010, FND-VIDEO-007]. No other build has been described.
