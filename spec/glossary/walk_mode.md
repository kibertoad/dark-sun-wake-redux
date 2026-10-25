# walk_mode

A `INT16[]` the game keeps at `4F49:08A7` in BLD-GOG-EN-1.1: for each slot, one word every 19 bytes, what the slot moves toward. 15 is the cell in `walk_target_x` and `walk_target_y`, set by a direction key or a Walk click; 1, 10, 11, 13, 18 and 19 are the object whose slot `walk_target_x` holds; with any other value `plan_walk` plans nothing [FND-EXPLORE-004, FND-RNG-068, FND-EXPLORE-014]. No other build has been described.
