# psionic_ranks

The `PSST` bytes the game keeps at `4E71:09CD` in BLD-GOG-EN-1.1, 34 for each party slot, one per psionic power at `4E71:09CD + 34 * slot + power`: bits 1 to 7 are the power's rank and bit 0 is set while a use of it is in effect [FND-PARTY-104, FND-PARTY-210, FND-RNG-046]. No other build has been described.
