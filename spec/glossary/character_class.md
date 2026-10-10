# character_class

One of a character's classes, which the game shows by name: cleric, druid, fighter, gladiator,
preserver, psionicist, ranger or thief. Rules write it as a code from 0 to 7 in that order, the
order of the executable's labels at `5000:908C` in BLD-GOG-EN-1.1 [FND-PARTY-016]. The character
record keeps one to three classes as codes from 1 to 17 (FMT-PARTY-001 `classes`), where
Cleric, Druid and Ranger each have four codes [FND-PARTY-056, FND-PARTY-057]; how a code maps to
this numbering in rules is (unknown).
