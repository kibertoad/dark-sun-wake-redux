# character_class

One of a character's classes, which the game shows by name: cleric, druid, fighter, gladiator,
preserver, psionicist, ranger or thief. Rules write it as a code from 0 to 7 in that order, the
order of the executable's labels at `5000:908C` in BLD-GOG-EN-1.1 [FND-PARTY-016]. The character
record keeps one to three classes as codes from 1 to 17 (FMT-PARTY-001 `classes`), where
Cleric, Druid and Ranger each have four codes [FND-PARTY-056, FND-PARTY-057]; a code's class in this
numbering is byte 1 of the code's pair in the table at `4E4F:009C`, less 1: codes 1 to 4 are
cleric, 5 to 8 druid, 9 fighter, 10 gladiator, 11 preserver, 12 psionicist, 13 to 16 ranger and
17 thief [FND-PARTY-083, FND-PARTY-057].
