# party_shown

A `UINT8` the game keeps at `57E0:13FA` in BLD-GOG-EN-1.1: not 0 while the whole party is shown on the map and 0 while only the leader is. 0 in the file; the switch `-A` sets it to 1, and the party loader shows every member when it is not 0 [FND-EXPLORE-005, FND-PARTY-013, FND-SOUND-010]. No other build has been described.
