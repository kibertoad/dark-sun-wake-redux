# view_left

The map's horizontal place of the left edge of the view: an `INT16` the game keeps at `57E0:1408` in BLD-GOG-EN-1.1, which the combat code subtracts from an object slot's `x` to place a menu on the screen [FND-COMBAT-025, FND-COMBAT-026].

The adjustment child at `2C5F:01F3` writes an aligned anchor-relative
value before ordered signed boundary tests [FND-COMBAT-049]. Its final
value through all later calls remains Q-COMBAT-014.
