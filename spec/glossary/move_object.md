# move_object

The far routine at `31E0:4129` in BLD-GOG-EN-1.1, which takes a slot and a position and, when the position changes, frees the slot's cells at the old position, stores the new position in `position_x` and `position_y`, updates the slot's entry and occupies the cells at the new position [FND-EXPLORE-003]. Read except `31E0:435D`, which it calls with the change of position. No other build has been described.
