# alignment

A character's alignment, which the game shows by name: lawful good, lawful neutral, lawful evil,
neutral good, true neutral, neutral evil, chaotic good, chaotic neutral or chaotic evil. Rules
write it as a code from 0 to 8 in that order, the order of the executable's labels at
`5000:90EB` in BLD-GOG-EN-1.1 [FND-PARTY-017]. The character record keeps it as one byte counting
from 1 in the same order, one more than this code (FMT-PARTY-001 `alignment`)
[FND-PARTY-055, FND-PARTY-056].
