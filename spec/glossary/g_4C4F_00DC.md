# g_4C4F_00DC

A table of four-byte entries the game keeps at `4C4F:00DC` in BLD-GOG-EN-1.1, one per combatant, of two `INT16` words written `first` and `second`: the fight set-up writes both, and the routine at overlay 173 `+1098` keeps the combatant with the greatest first word, and on a tie the greater second word [FND-RNG-041, FND-RNG-042]. What follows that pick is (unknown). No other build has been described.
