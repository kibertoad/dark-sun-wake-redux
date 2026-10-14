---
id: FND-PARTY-213
title: Defeat cleanup unloads placement entry zero repeatedly and clears a combatant identifier through the chunk deletion dispatcher
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
tool: Python 3.12 with Capstone 5.0.9, tools/research/exec-census/resident_listing.py and direct_callers.py
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0330..277B:0353
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:044C..31E0:05BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:074B..31E0:0783
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2410..2D40:253B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:0094..289B:00F0
---

## Observation

The installed executable is the input; resident addresses use load segment 1000.
FND-PARTY-212's watchpoint stack reports return addresses, including 2D40:2488,
rather than entries of the routines being called.

When DS:13FB is nonzero, 277B:0330..0353 repeatedly calls 31E0:05BB with
placement index 0 and the following three words 1, 0, 0, while unsigned
DS:264E is greater than zero. It leaves this loop only when the count is zero.

The unload's 31E0:074B..0763 passes index DI, the placement-table far pointer
DS:67B7, the near address DS:264E, the far address DS:67BB and the word 320
to 289B:0094. That helper reads the count through its incoming DS, returns
without changing it when zero, and otherwise decrements it before loading
another DS. When entries follow the removed index it copies the remaining
eight-byte entries forward with the direction flag cleared, then calls its
near helper 007A to update related indices. It restores the incoming DS.
The unload then clears byte +5 of the entry at the new count and clamps
DS:656F down to that count if necessary (31E0:0763..0783).

The unload's later call at 31E0:0AC9 reaches 31E0:044C as FND-PARTY-138
describes. Provided DS:265B is nonzero, 31E0:044C calls 2D40:2410 with its
slot at 054F, then the slot release 2D40:03C1 at 0556. DS:265B zero skips
this whole body. The chunk deletion precedes release.

2D40:2410 rejects unsigned slot values of 9999 or above and slots whose
kind byte at 4F49:(0C33 + 3 * slot) is zero. Otherwise it calls the chunk
writer 2D40:2196 with the slot and an 800-byte SS-local buffer, then calls
2D40:2452 with that far buffer and root index zero. 2452 constructs a
160-byte local index map through 27B7, then calls 248D with the root index,
the far buffer and the map's near address. The return from this call is
2488, explaining the dynamic stack.

248D dispatches on chunk type minus one, accepting indices zero through
four. The five CS-relative target words at 27AD are 253B, 251B, 24B6,
27A9 and 2551. Thus chunk type 2 reaches 251B: it reads the chunk's word
+4, selects that index in the 49-byte combatant table through DS:19C9,
and clears word +6, character_id, at 2533. The next instruction is 2539,
the stop FND-PARTY-212 observed. Chunk type 3 instead scans 42 combatant
records for a nonzero identifier with the same details index; only when
none matches does it clear word +0E in the 66-byte details table through
DS:19C5 (24B6..2519).

## Interpretation

Defeat cleanup removes placement entry zero repeatedly, with compaction
and decrement before the slot's chunk deletion. This explains the observed
one-member identifier clearing without treating 2488 as a function entry.
The loop is over all placed objects, rather than an explicit four-box loop.

## Alternatives

- This is not a complete reading of unload, chunk serialization or recursive
  deletion. Their other callees, alias conditions and input admission remain
  unproved, including whether every party slot of a larger defeated party
  is necessarily unloaded. Q-PARTY-078 retains that question.
- The near map in 2452 is passed without an explicit segment. The recursive
  deletion reads it through DS; this reading does not establish DS equals SS
  or justify that storage identity.
- The compactor's near helper 007A and the chunk writer's output are not read
  here. Count decrement is observed before those calls, not proof they return.
- No new runtime run was made; the one-member comparison is FND-PARTY-212.

## How to reproduce

Use the repository's hash-locked evidence Python. Run resident_listing.py
with the installed DSUN.EXE and ranges 277B:0330..277B:0353,
31E0:044C..31E0:05BB, 31E0:05BB..31E0:0610,
31E0:0719..31E0:0830, 31E0:09A8..31E0:0AFC,
2D40:2410..2D40:27B7 and 289B:0094..289B:00F0.
Read 27AD..27B7 as five little-endian target words, not instructions.
Run direct_callers.py with targets 2D40:2410, 2D40:2452 and 2D40:248D;
its byte-pattern candidates require instruction-boundary checking and do not
establish absence of indirect callers.
