# view_right_limit

An `INT16` the game keeps at `57E0:14DB` in BLD-GOG-EN-1.1: one past the greatest map column the view may show: a scrolling step stops before `view_left + 319` reaches it [FND-EXPLORE-006]. The view-centring routine clamps to it and script request 33 sets it [FND-COMBAT-049, FND-COMBAT-050]. No other build has been described.
