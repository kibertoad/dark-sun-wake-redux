# scroll_hold

A `UINT8` the game keeps at `57E0:0DA8` in BLD-GOG-EN-1.1: while it is not 0, the pointer at the screen's edge does not scroll the view, and each main-loop pass with the pointer away from the edge stores 0 in it [FND-EXPLORE-006]. It is 1 in the load image, and no instruction stores another value [FND-EXPLORE-010]. No other build has been described.
