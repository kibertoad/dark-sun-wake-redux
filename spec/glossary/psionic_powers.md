# psionic_powers

A table the game keeps at `51F1:07E0` in BLD-GOG-EN-1.1, three bytes for each psionic power 0 to 33 at `51F1:07E0 + 3 * power`. Byte 0 holds `score_code`, the score number in bits 0 to 2 that the power check reads, and `adjust`, the signed byte shifted right by three that it adds; byte 1 is `cost`, the points a use takes; byte 2, `byte_2`, keeps a use from marking the power in effect when it is 99 [FND-RNG-046, FND-PARTY-210]. What the values are and where they come from is (unknown). No other build has been described.
