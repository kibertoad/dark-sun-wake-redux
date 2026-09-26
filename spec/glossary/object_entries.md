# object_entries

The object entries: a list the game keeps, of 8-byte entries at the far pointer at `57E0:67B7` in BLD-GOG-EN-1.1, from which a slot takes its position. This spec reads an entry with the field names of FMT-REGION-006, whose first two words match it; byte 5 is `flags`, whose bit 3 marks a party member keys 5 and 6 may hide [FND-ACTOR-003, FND-EXPLORE-005]. No other build has been described.
