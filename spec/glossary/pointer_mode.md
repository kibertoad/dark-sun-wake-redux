# pointer_mode

The pointer mode on the map: an `INT16` the game keeps at `57E0:1440` in BLD-GOG-EN-1.1, 1 for Walk, 4 for Attack, 2 for Look and 3 for a mode whose pointer is an object's image; a right-button press steps it 1, 4, 2, then 3 or 1, and 3 and 5 step to 1 [FND-INPUT-013]. Overlay 208 stores 5 and the pan routine `28C9:34B1` stores 0 [FND-INPUT-013]. A direction key sets it to 1 through `fn_28C9_2535`, and key 5 passes whether it is 1 to a redraw [FND-COMBAT-013, FND-EXPLORE-004, FND-EXPLORE-005]. No other build has been described.
