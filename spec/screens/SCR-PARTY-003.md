---
id: SCR-PARTY-003
title: Choose a new class
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-PARTY-161, FND-PARTY-077, FND-PARTY-154, FND-PARTY-232]
conflicting: []
split_with: []
related: [RULE-PARTY-004, SCR-UI-002, SCR-UI-015, SCR-UI-005]
---

## Drawn elements

The opener requests `RESOURCE.GFF#WIND/17502` with position words
73 and 41. The source window is 172 by 117. Rectangles below are
window-relative source placements; actual loaded placement, painting
and native control frames remain unvalidated (FND-PARTY-232).

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Window picture reference | image word 17002 of `WIND/17502` | Native background drawing remains open | Window request (73,41), source size 172 by 117 | The class-window creation branch is taken | FND-PARTY-232 |
| Cleric choice | `BUTN/17303`, `ICON/17102` | Logical cleric family | (89,25,79,8) | Window requested; closure depends on its flag | FND-PARTY-161, FND-PARTY-232 |
| Druid choice | `BUTN/17304`, `ICON/17101` | Logical druid family | (89,34,79,8) | As above | FND-PARTY-161, FND-PARTY-232 |
| Fighter choice | `BUTN/17305`, `ICON/17105` | Logical fighter class | (89,43,79,8) | As above | FND-PARTY-161, FND-PARTY-232 |
| Gladiator choice | `BUTN/17306`, `ICON/17107` | Logical gladiator class | (89,52,79,8) | As above | FND-PARTY-161, FND-PARTY-232 |
| Preserver choice | `BUTN/17307`, `ICON/17103` | Logical preserver class | (89,61,79,8) | As above | FND-PARTY-161, FND-PARTY-232 |
| Psionicist choice | `BUTN/17308`, `ICON/17108` | Logical psionicist class | (89,70,79,8) | As above | FND-PARTY-161, FND-PARTY-232 |
| Ranger choice | `BUTN/17309`, `ICON/17106` | Logical ranger family | (89,79,79,8) | As above | FND-PARTY-161, FND-PARTY-232 |
| Thief choice | `BUTN/17310`, `ICON/17104` | Logical thief class | (89,88,79,8) | As above | FND-PARTY-161, FND-PARTY-232 |
| Exit button | `BUTN/17302` | `EXIT` | (126,99,42,12) | Window requested | FND-PARTY-154, FND-PARTY-232 |

RULE-PARTY-004 supplies the class eligibility test. The opener tests
original class codes 1 through 17 and writes one of eight flags for
each result. A final zero flag requests closure of that family's button
through code one; a nonzero flag skips closure and does not explicitly
enable an already-closed reused button. The separate any-success flag
selects whether to request this window at all. If every test returned
zero, the opener requests the no-other-classes message and bypasses this
window's creation branch (FND-PARTY-161, FND-PARTY-232).

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Class choice | Its source rectangle above | Its loaded button must admit delivery; zero availability flags request closure | Event kind 2 for its ID selects the corresponding original class family, calls the class-change entry and requests close. Cleric, druid and ranger first request the sphere menu. | FND-PARTY-161, FND-PARTY-232 |
| Exit | (126,99,42,12), window-relative | Native delivery remains open | Event kind 2 for ID 17302 requests close without the local class-change call. | FND-PARTY-161, FND-PARTY-232 |

The handler itself does not repeat the class eligibility test when a
button event reaches it. Native dispatch and child validation are
separate from this local event mapping (FND-PARTY-232).

## Keyboard input

None known.

## Other input

| Device | Input | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Unknown | Event kind 6, code `0x011B` | Handler receives the event | Requests close without the local class-change call. | FND-PARTY-161, FND-PARTY-232 |
| Unknown | Event kind 1 | Handler receives the event | Calls the close helper, clears a shared byte and returns FFFF. | FND-PARTY-232 |

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Class-window request | At least one class test returned nonzero | The local event loop ends when its current stored window pointer is null; successful live activation remains open | FND-PARTY-232 |
| Sphere-menu request | A delivered cleric, druid or ranger choice | Returned values 1..3 adjust the base class code; other values retain it, before the class-change and close requests | FND-PARTY-161, FND-PARTY-232 |

## Timing

The local loop polls for an event, dispatches a returned 24-byte event
when the poll returned zero, and repeats while its stored window pointer
is nonnull. No fixed duration or native input latency is established
(FND-PARTY-232).

## Differences between builds

None known.

## Open questions

- Remaining setup, event-dispatch, close and restoration children, loaded
  control admission, actual class-window paint and native frame/input states
  (Q-PARTY-101).
- Actual loaded origin beyond the requested position words (Q-UI-003).
- How the window picture reference is drawn and how its other control masks
  affect presentation (Q-UI-001, Q-UI-007).
