# scroll_step_x

An `INT16` the game keeps at `57E0:9BF6` in BLD-GOG-EN-1.1: the pixels one scrolling step moves the view across, 16 after every call of its setter but one, which uses 32 while it pans the view to an object [FND-EXPLORE-006]. No other build has been described.
