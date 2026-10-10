# origin

A character's people, which the game shows as its race: human, dwarf, elf, half-elf, half-giant,
halfling, mul or thri-kreen. Rules write it as a code from 0 to 7 in that order, the order of the
executable's labels at `5000:9050` in BLD-GOG-EN-1.1 [FND-PARTY-017]. The character record keeps
it as one byte counting from 1 in the same order, one more than this code (FMT-PARTY-001
`origin`) [FND-PARTY-055, FND-PARTY-056].
