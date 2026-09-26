---
id: SCR-COMBAT-001
title: Combat status panel
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-COMBAT-004, FND-COMBAT-006, FND-COMBAT-007, FND-COMBAT-008, FND-COMBAT-018, FND-COMBAT-019, FND-COMBAT-020, FND-COMBAT-021, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-025, FND-COMBAT-026, FND-COMBAT-027]
conflicting: []
split_with: []
related: [RULE-COMBAT-001, RULE-COMBAT-004, RULE-COMBAT-005, RULE-COMBAT-006, RULE-COMBAT-008, RULE-COMBAT-009]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Panel | `RESOURCE.GFF#BMP/19003`, 98 x 32 | None | (215, 4) | `combat_state` is neither 0 nor 1, each time the map view is redrawn | FND-COMBAT-004, FND-COMBAT-006, FND-COMBAT-007, FND-COMBAT-018 |
| Name | The interface font, in the colours at `57E0:2D14` and `57E0:2D16` | `combatants[b].name`, where `b` is the record `2D40:3E64` gives for `turn_combatant` | centred on the panel at y 6 | With the panel | FND-COMBAT-018, FND-COMBAT-022 |
| Hit points | The same font | `combatants[b].hit_points` and `combatant_details[a].max_hit_points` with `%d/%d`, or `???/???` when `turn_combatant` is above 4 | centred at y 12 | With the panel | FND-COMBAT-018, FND-COMBAT-019, FND-COMBAT-022 |
| Condition | The same font | `Okay`, or the `name` of the record of `effect_names` that RULE-COMBAT-009 gives when `combatants[b].combat_mark` is 1 | centred at y 18 | With the panel | FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-027 |
| Movement | The same font | `movement_tenths[b] / 10`, truncated toward zero, after `Move : ` | centred at y 24 | With the panel | FND-COMBAT-018, FND-COMBAT-019, FND-COMBAT-022 |
| Damage number | Not known | The damage of a hit, as a white number on a red and black splash | over the figure struck | After a hit | FND-COMBAT-019 |

Each line starts at x = 215 + (98 - text width) / 2, truncated toward zero. The panel sits on the
map view itself, with no window of its own; the rest of the map, the party and the enemies are
drawn as in exploration, with all four party members (RULE-COMBAT-001).

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| An enemy | Its figure | A party member's turn | The member walks to it and attacks (RULE-COMBAT-006) | FND-COMBAT-021 |

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| `G` | A party member's turn | The member guards (RULE-COMBAT-004) | FND-COMBAT-025 |
| `W` | A party member's turn | The member waits (RULE-COMBAT-004) | FND-COMBAT-025 |
| `Q` | A party member's turn | Opens the end-of-move menu (RULE-COMBAT-005) | FND-COMBAT-025, FND-COMBAT-026 |
| `F1` | Always | Refused with a message (RULE-COMBAT-008) | FND-COMBAT-023 |

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Party member's turn | The turn passes to one of the party; the pointer is the Walk arrow and the panel shows the member's hit points | The member's turn ends | FND-COMBAT-018, FND-COMBAT-022 |
| Enemy's turn | The turn passes to an enemy; the pointer is the hourglass and the panel shows `???/???` | The enemy's turn ends | FND-COMBAT-019, FND-COMBAT-020, FND-COMBAT-022 |
| Conversation | A conversation opens during combat; the panel is not drawn | The conversation closes | FND-COMBAT-020 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- What `2D40:3E64` maps a combatant to, why a `turn_combatant` above 4 shows question marks
  while the value 4 does not, and whether 4 is ever a combatant's number (Q-COMBAT-002).
- Where the damage number is drawn from and for how long, what starts and ends a combat, and how
  the turn passes from one combatant to the next (Q-COMBAT-001).
- Whether the frame FND-COMBAT-020's owner label describes as an enemy moving is the conversation
  frame it shows (Q-COMBAT-001).
