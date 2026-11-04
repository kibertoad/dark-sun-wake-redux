---
id: SCR-UI-013
title: Load Game
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-036, FND-UI-037, FND-UI-038, FND-SAVE-005, FND-SAVE-007, FND-SAVE-026, FND-COMBAT-085, FND-COMBAT-131, FND-CONFIG-221, FND-CONFIG-222, FND-COMBAT-132, FND-COMBAT-133, FND-COMBAT-134, FND-COMBAT-135, FND-COMBAT-136, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-SAVE-001, RULE-SAVE-002, SCR-UI-006]
---

## Drawn elements

`RESOURCE.GFF#WIND/18500` is a 320 x 181 window with no image in its own
image field. Positions below are relative to the window; its screen position
and native widget chrome are not established (FND-UI-036).

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| LOAD title | `BUTN/18300`, changed to `ICON/18101` in Load mode | LOAD | (126, 0, 67, 23) control; image 70 x 23 | Load mode | FND-UI-036 |
| LOAD action | `BUTN/18301`, changed to `ICON/18102` in Load mode | LOAD | (231, 30, 44, 15) | Load mode | FND-UI-036 |
| EXIT action | `BUTN/18302`, `ICON/18109` | EXIT | (231, 50, 44, 15) | Window shown | FND-UI-036 |
| Ten list rows | `BUTN/18304` to `/18313`, `ICON/18100` | Description from `STXT/1` for a readable saved game; exact paint not established | (46, 31 + 11 * n, 163, 11), n = 0 to 9 | Window shown | FND-UI-036, FND-SAVE-007 |
| Name box | `EBOX/18400`, `BMP/10002` | Contents not established | (49, 147, 164, 12) control; image 161 x 12 | Window shown | FND-UI-036 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| A saved game's row | Window-local (46, 31 + 11 * n, 163, 11), n = 0 to 9 | Not known | Stores the selected row index and passes its name to the name box. | FND-UI-036, FND-UI-037 |
| LOAD | Window-local (231, 30, 44, 15) | Not known | Calls the Load Game routine with the selected row after closing the window (RULE-SAVE-002). | FND-UI-036, FND-UI-037, FND-SAVE-005 |
| Name box | Window-local (49, 147, 164, 12) | Not known | The callback's event-2 branch takes the same path as LOAD. | FND-UI-036, FND-UI-037 |
| EXIT | Window-local (231, 50, 44, 15) | Not known | Closes the window without directly calling Load Game; the later transition remains open. | FND-UI-036, FND-UI-037 |

The manual describes a list of saved-game names and an OKAY button
(SRC-MANUAL-1994, page 14). The shipped button art reads LOAD, rather than
OKAY (FND-UI-036).

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| Enter | Observed after F2 entry with selected row one, top zero and ten empty markers | Produces callback event six/key word 1C0D and reaches numbered-load entry with argument one after window cleanup. The loader body was not executed; no load outcome is established. | FND-COMBAT-136 |
| Down | One admitted row-one/top-zero Load state | Delivery and returned empty-list outcome are described below. | FND-COMBAT-134, FND-COMBAT-135 |

## Other input

| Device | Input | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Unknown | Callback event 6, word `0x4800` | Window shown | Decrements the selected row, searching past empty markers. Wraps from zero to nine and can continue searching after its first empty wrap; a second empty wrap stops at nine. Physical input mapping is unknown. | FND-UI-038, FND-COMBAT-085 |
| Down, observed on one admitted route | Callback event 6, word `0x5000` | Window shown; actual delivery observed at row one/top zero | Increments the selected row, searching past empty markers until it wraps from nine to zero. That wrap ends the search even when row zero is empty. One returned callback with all ten markers empty changed row one to zero; painted selection and other outcomes remain unconfirmed. | FND-UI-038, FND-COMBAT-085, FND-COMBAT-134, FND-COMBAT-135 |

These local search results assume an initial selected row within zero through
nine, valid list storage and unchanged mode, markers and selection through
the intervening calls. With all ten markers empty, the two branches leave
the selected-row word at nine and zero respectively, skip requesting the
new row's control state and skip the row-to-name-box helper. With initial
row five and only row two nonempty, `0x4800` reaches row two but `0x5000`
stops at empty row zero. These are conditional static results; native paint
and whether a subsequent LOAD action is delivered remain open
(FND-COMBAT-085, Q-COMBAT-046, Q-UI-005).

An inherited FFFF selection is a separate case: the character-list scan
can produce it, and Save/Load has no own setup reset. In Load mode the
navigation branches read the old indexed marker before their signed
movement or wrap. Actual inherited values, buffer validity and native
input remain unadmitted (FND-COMBAT-131, Q-COMBAT-046, Q-COMBAT-063).

Global registration alone does not admit these event-six branches. On the
resident dispatcher's zero registered-window-count route, it rewrites the
forwarded kind to three, which this handler rejects before any action.
The nonzero-count, absent-current-window route can instead retain kind
six. Actual count, handler and event state remain Q-COMBAT-046; a zero
window-callback result does not invoke a second global fallback
(FND-CONFIG-221).

The count is wrapped bookkeeping: registration and removal change it
before later failure paths. Some window walks compare it signed, while
dispatch tests zero only. Current count alone therefore does not establish
a live or usable Load window (FND-CONFIG-222, Q-COMBAT-046).

One recorded F2 entry from a verified two-character map state reached ready
window 18500 with Load mode one, selected row one and top zero. Its incoming
kind-six dispatch had count one and null current event-window target; final
count was two. This admits that entry only, not a subsequent Load action or
other initial values (FND-COMBAT-133, Q-COMBAT-046).

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Row text and markers are requested from records indexed by current `save_list_top` + row | Setup visits rows 0..9 of a requested ten-record list; actual top and storage must be admitted | The window closes | FND-SAVE-026 |

A nonzero inherited top does not establish valid later saved-game pages.
The consumer can index beyond the requested ten records; actual contents
and native paint remain unestablished (FND-SAVE-026).

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Nominal fixed restore payloads miss eight relevant UI state regions,
  but actual transfer bounds, pointer lifetime and other state writers
  remain unadmitted (FND-COMBAT-132, Q-COMBAT-046).
- Actual shared top/selection and buffer lifetime remain Q-COMBAT-046 and
  Q-COMBAT-063; native results when an index exceeds the requested list
  remain part of Q-UI-005 (FND-SAVE-026).
- How the ten rows paint descriptions or unavailable slots, which are selectable, how the window is positioned
  and painted, what physical input maps to callback event 6, and the later
  transition after EXIT remain open (FND-UI-036, FND-UI-037, FND-SAVE-006,
  Q-UI-005).
