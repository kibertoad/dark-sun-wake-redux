# origin

A character's people, which the game shows as its race: human, dwarf, elf, half-elf, half-giant,
halfling, mul or thri-kreen. Rules write it as a code from 0 to 7 in that order, the order of the
executable's labels at `5000:9050` in BLD-GOG-EN-1.1 [FND-PARTY-017]. A value the game keeps for
each character, of a type and at a place that are (unknown); the character record does not hold
the code in a header byte or aligned word (FND-PARTY-018).
