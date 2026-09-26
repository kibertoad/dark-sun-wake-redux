# object_slot_kinds

A `UINT8[]` the game keeps at `4F49:0C33` in BLD-GOG-EN-1.1: one byte for each object slot, every 3 bytes, interleaved with `object_slot_combatants`. Kind 2 marks a slot that occupies cells and has a combatant record [FND-ACTOR-005, FND-EXPLORE-001, FND-EXPLORE-002]. No other build has been described.
