# character_class

One of a character's classes, which the game shows by name: cleric, druid, fighter, gladiator,
preserver, psionicist, ranger or thief. Rules write it as a code from 0 to 7 in that order, the
order of the executable's labels at `5000:908C` in BLD-GOG-EN-1.1 [FND-PARTY-016]. A value the
game keeps for each of a character's one to three classes, of a type and at a place that are
(unknown); the character record does not hold the code in a header byte or aligned word
(FND-PARTY-018).
