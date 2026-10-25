# wall_follow_diagonals

A `UINT8` the game keeps at `57E0:0443` in BLD-GOG-EN-1.1: 1 when the route search's wall followers may also step diagonally between two turns. `plan_walk` sets it to 1 for its first search and 0 for its second [FND-EXPLORE-014]. No other build has been described.
