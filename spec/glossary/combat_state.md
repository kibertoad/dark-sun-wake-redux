# combat_state

A `UINT16` the game keeps at `4C10:0019` in BLD-GOG-EN-1.1: 0 outside combat and not 0 during it. Direct stores give it 0, 1 and 4, and the status panel is drawn only when it is neither 0 nor 1 [FND-COMBAT-023].
