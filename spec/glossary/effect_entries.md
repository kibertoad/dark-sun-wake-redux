# effect_entries

The 192 ten-byte effect entries the game keeps at `4E71:024D` in BLD-GOG-EN-1.1. The word `+0`, `slot`, is the object slot the entry belongs to and the byte `+6`, `code`, its effect code; the word at `DS:2100` counts the entries in use [FND-RNG-042, FND-RNG-046]. What the other bytes hold is (unknown); `g_4E71_024F` is the word `+2`. No other build has been described.
