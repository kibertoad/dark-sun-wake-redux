---
id: RULE-COMBAT-005
title: The end-of-move menu offers GUARD, WAIT and END TURN beside the character whose turn it is
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-COMBAT-026, FND-COMBAT-024, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-025, FND-COMBAT-008, FND-ACTOR-003, FND-PARTY-013, FND-COMBAT-047, FND-COMBAT-048, FND-COMBAT-049, FND-COMBAT-050, FND-COMBAT-051]
conflicting: []
split_with: []
related: [RULE-COMBAT-004, FMT-COMBAT-001, FMT-ACTOR-004]
---

## Summary

A menu titled with the character's name offers to guard, wait or end the turn. It is placed to
the right of the character's figure, or to its left near the right edge of the screen.

## When it runs

When the player presses `Q` during a party member's turn in combat (RULE-COMBAT-004).

## Parameters

- `x`: the horizontal place of the character's figure on the screen.
- `y`: the vertical place of the character's figure on the screen.

## Inputs

`turn_combatant`, `combatants`, `object_slots`, `view_left` and `view_top`.

## Procedure

```text
define name_length(name: char[16]):
    let n = 0
    while n < 16 and name[n] != 0:
        n = n + 1
    return n

define menu_left(x, width):
    if x + width + 146 > 320:
        return x - 146
    return x + width

define menu_top(y, height):
    if y - 34 + height / 2 < 0:
        return 0
    if y + 34 + height / 2 >= 199:
        return 199 - 68
    return y + height / 2 - 34

let c = turn_combatant
let title = message_end_named_move
if name_length(combatants[c].name) + 11 > 20:
    title = message_end_move
let slot = object_slots[c]
if fn_28C9_000A(c) == 0:
    fn_2C5F_018C(c, 1)
    x = slot.x - view_left
    y = slot.y - view_top
let choice = fn_566A_0025(menu_left(x, slot.width), menu_top(y, slot.height), title, combatants[c].name, label_guard, label_wait, label_end_turn)
if choice == 1:
    fn_5671_0093(c, 1)
else if choice == 2:
    fn_5671_0093(c, 2)
else if choice == 3:
    fn_5671_0098(c, 0)
fn_182_19F8()
fn_3D72_0D83()
```

## Outputs

No return value. Shows the menu, then runs the action chosen: `fn_5671_0093` with 1 for GUARD
or 2 for WAIT, `fn_5671_0098` with 0 for END TURN, and nothing for any other result. Then runs
`fn_182_19F8` and `fn_3D72_0D83` whatever was chosen.

The menu producer records choices separately from its handler's return:
event type two maps control identifier 2844 to zero, 36B1/36B5 to one,
36B2/36B6 to two and 36B3/36B7 to three, or stores FFFF for no match.
That handler returns zero even after storing a choice. Type six tests
the three labels' nonzero first bytes in order, sign-extending the stored
byte and ORing 20 hexadecimal before comparing with the zero-extended
input low byte ORed with 20. A match stores its one-based index; otherwise
the exact input word 011B stores zero only when the cancellation gate is
zero, and every other unmatched case stores FFFF. Type six returns FFFF
for a recorded choice and zero for FFFF. Other event types leave the
choice unchanged and return zero. These event numbers do not yet establish
native device delivery.

The producer loops while its shared choice word is FFFF and ignores its
direct handler call's return. Before the loop it saves the result of
`fn_3A8E_060D`; only for zero does it replace it with `fn_3A8E_0D9A`'s
result. After handler restoration and cleanup, it returns that saved result
if nonzero, otherwise rereads the shared choice word. Thus a native
selection-to-action claim still needs zero admitted child results and
choice preservation through cleanup (Q-COMBAT-015).

## Edge cases

`message_end_named_move` holds `%Fs` in place of the name and 11 other characters, so a name of
more than 9 characters gives the shorter title `message_end_move`. The menu is 146 pixels wide
and 68 high as far as the placement goes; its left edge can be negative for a figure within 146
pixels of the left edge that is also near the right one, which cannot happen on a 320-pixel
screen for a figure narrower than 28 pixels. `/ 2` truncates toward zero.

## What the sources say

SRC-MANUAL-1994 does not describe this menu; page 77 has `Q` quit the turn.

## Differences between builds

None known.

## Open questions

- The admitted state, later consumers and gameplay meaning of `fn_5671_0093`
  and `fn_5671_0098`'s command
  stores (Q-COMBAT-006). FND-COMBAT-047 answers their local branches,
  resolver gate and distinct zero-mask/OR routes.
- What the common postprocessing through `fn_182_19F8` and `fn_3D72_0D83`
  leaves for later consumers (Q-COMBAT-016).
- What initialization, event delivery, control admission and complete child/
  storage-preservation contracts of `fn_566A_0025`, `fn_3A8E_060D` and
  `fn_3A8E_0D9A` make the local choice and final return
  branches apply to native selection or closing (Q-COMBAT-015).
  FND-COMBAT-051 answers the local handler mappings, ignored handler result,
  saved child-result precedence and cleanup-before-choice-reload order.
- What admitted state and complete child/preservation contract let
  `fn_2C5F_018C` supply the menu's later coordinate rereads and final
  presentation (Q-COMBAT-014). FND-COMBAT-049 answers its local
  calculation and child sequence; FND-COMBAT-050 identifies distinct
  bound and coordinate producers. FND-COMBAT-048 answers the separate
  local anchor-position test made by `fn_28C9_000A`.
