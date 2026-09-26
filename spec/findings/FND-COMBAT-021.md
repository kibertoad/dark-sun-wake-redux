---
id: FND-COMBAT-021
title: The owner saw combat start and end with no transition screen, and a click on an enemy made the active character walk up and strike
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: Owner's observation while taking the captures of FND-COMBAT-018 to FND-COMBAT-020
environment: GOG DOSBox 0.74-2 with dosbox_darksun2.conf and dosbox_darksun2_single.conf (machine=svga_s3, memsize 16, cycles fixed 15000, sbtype sb16), game started by RAVAGER.BAT
---

## Observation

While playing the first fight in Tyr of a game started with START GAME, the owner saw the
following, and recorded no capture of it beyond those of FND-COMBAT-018 to FND-COMBAT-020:

- When combat began, the map and the characters were drawn as before, with the status panel added
  at the top right. The first steady frame of combat showed the panel of the character whose turn
  it was.
- Clicking an enemy made the active character walk up to it and strike it. There was no separate
  step of selecting the target or confirming the attack.
- Nothing marked the change from one character's turn to the next beyond the panel's contents.
- When combat ended, there was no closing screen: the game went straight back to exploring, and
  only the leader was drawn.

## Interpretation

Combat happens on the map screen. A plain click on an enemy is the attack command. When the party
is set to show only the leader, all four members are drawn during combat and the others vanish
again when it ends.

## Alternatives

The owner reported these from memory: the frames of the transitions were not captured, and the
owner's display setting for the party (SRC-MANUAL-1994, page 15, Collapse Party) was not recorded.

## How to reproduce

Start a game with START GAME, walk into the first fight in Tyr, click an enemy during a party
member's turn, and watch the start and end of the fight.
