---
id: FND-SOUND-056
title: DIGPAK descriptor staging clears slot state while direct submission has a distinct stack-cleanup caveat
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:1626..4842:1685
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:182E..4842:186B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:1285..4842:12D8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0F57..4842:0F92
tool: resident_listing.py at 790237e6; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-EXE-646 and FND-SOUND-052/053/055
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-055's two descriptor requests select 0079 and 0086.
Under FND-EXE-646's unchanged DIGPAK table they reach 1626
and 182E. Other driver tables and actual admission remain separate.

### Staging a slot

1626 first tests CS:0E5C. When zero, it calls stop body 18B3
with zero and then writes 0E5C one. Otherwise it skips that call.
It doubles incoming slot word BP+8 with word wrapping and loads
the descriptor far pointer at BP+0A. For doubled index d it writes:

| Descriptor field | Destination |
|---|---|
| Word +2 | CS:(0E54+d) |
| Pointer offset +4 | CS:(0E44+d) |
| Pointer segment +6 | CS:(0E48+d) |
| Count low word +8 | CS:(0E4C+d) |
| Count high word +0A | CS:(0E50+d) |
| Constant zero | CS:(0E58+d) |

There is no local slot bound or pointer/capacity check. For the
retained zero/one indexes of FND-SOUND-055 the destinations are
the two word slots in each array. The stores precede ordinary
register restoration; this body does not itself request audio data
submission. Its stop child has FND-SOUND-052's external effects.

The related 1285 start child doubles its incoming slot, publishes
two at CS:(0E58+d) and d at CS:0E60, passes the saved word at
0E54+d to 0F57 and publishes returned AX at CS:0E36. It then
requests 1045 with callback pointer CS:107E and passes the saved
pointer/count arrays to 12D8. These state publications precede the
submission child; they do not prove successful device consumption.
Actual slot retention, callbacks and downstream effects remain open.

### Direct descriptor submission and stack accounting

182E reserves four local bytes and saves DS, SI and DI. It loads
the descriptor pointer from BP+8, saves its count words +8/+0A
in those locals, then pushes only descriptor word +2 and calls
0F57 through PUSH CS plus near CALL. After return, instruction
184D adds four to SP, although the explicit argument was two bytes.

0F57 ends with ordinary RETF at 0F91. It clears BH in its input
word, computes 0100 minus that low byte, calls 05EF with four
constructed words and removes those eight bytes, then restores
its own saved registers and returns. Its local body does not add
two bytes to its incoming argument cleanup. Contrast 1285's call
of the same helper, which removes two bytes at 12A5.

182E next pushes its saved count, the descriptor pointer +4/+6
and returned AX to 0F92, removes ten bytes, and pops DI, SI, DS
before resetting SP from BP. Under unchanged frames, ordinary
balanced child returns and intact local stack contents, the extra
two-byte cleanup means those pops load entry SI into DI, entry DS
into SI, and the saved count's high word into DS. Resetting SP
from BP still selects the original saved BP and far return frame.
A matching PUSH DS and POP DS alone therefore does not prove
DS restoration on this path. Interrupt/nonlocal or modified-frame
behavior is not established by this conditional stack calculation.

FND-RNG-096 records the push/pop sites while its census-based
interpretation concerns DS at later reseeding callers. This finding
does not establish a native path from this submission to those
callers, but requires a frame-aware review before that local
push/pop pair is used as preservation evidence. Q-RNG-040 holds
that cross-area consequence. No actual reseeding divergence or
reachable native defect is inferred from this local condition alone.

## Interpretation

Q-SOUND-002 can remove the unread descriptor staging and direct
submission wrapper contracts. Staging copies metadata and clears
the selected slot's state; direct submission has separate children
and a nontrivial saved-register/frame condition. Actual descriptor
bounds, state/storage retention, 05EF/0F92/12D8 effects, interrupt
handling, selected driver identity and audible output remain open.
No complete reading or whole sound question closure follows.

## Alternatives

- Staging proves device submission: 1626 copies fields and marks
  the slot; submission is delegated through other bodies.
- Matching DS push/pop proves preservation: the intervening argument
  cleanup shifts the saved-register pops under ordinary child returns.
- The stack mismatch proves a reseeding bug: actual admission and
  routes to the reseeding callers have not been established here.

## How to reproduce

Read the four location ranges with resident_listing.py at the revision
above and retain sound-descriptor-consumers.txt under
GAME_DIR/analysis/handler-census-20261106/. Model SP relative to
182E's BP through the argument push, far call, RETF, ADD SP and
final pops; compare 1285's two-byte cleanup of the same helper.
Reuse FND-EXE-646 for conditional table binding. No original-game
run, emulator session or emulated call supplied evidence.
