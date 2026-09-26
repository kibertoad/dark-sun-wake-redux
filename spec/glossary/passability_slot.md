# passability_slot

A `INT16` the game keeps at `57E0:19B6` in BLD-GOG-EN-1.1: the object slot whose own cell test the movement cell test applies, or -1 for none. It is -1 in the file, and the movement routine sets it around its route search [FND-EXPLORE-001, FND-EXPLORE-003]. No other build has been described.
