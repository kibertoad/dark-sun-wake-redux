# show_drawn_page

A far routine at `3D72:0D83` in BLD-GOG-EN-1.1 that the end-of-move menu calls last [FND-COMBAT-026] and the edge-scroll pass calls first while `pointer_mode` is 5 [FND-EXPLORE-006]. It sets `DS:2FB8` and runs the page flip `4328:0B4C`, which shows the drawn page, between two calls of a hook that is never set, and returns zero or FFFF [FND-CONFIG-171, FND-EXE-588, FND-PARTY-195, FND-EXPLORE-011]. It writes neither view word. No other build has been described.
