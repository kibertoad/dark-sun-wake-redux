# set_pointer_mode

The far routine at `28C9:2535` in BLD-GOG-EN-1.1, which stores its argument in the word at `57E0:4596` and in `pointer_mode`, and calls `2C5F:0C8D` with it, which picks the pointer image for that mode and shows it when it differs from the one shown [FND-EXPLORE-004, FND-INPUT-013]. No other build has been described.
