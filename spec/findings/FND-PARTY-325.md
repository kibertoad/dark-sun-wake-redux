---
id: FND-PARTY-325
title: Three raw sound-mode write candidates begin inside operands of established instruction streams
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0AFA..2C5F:0B25
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006ABBF..0x0006ABF3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00072376..0x000725C0
tool: resident_listing.py, overlay_listing.py, direct_callers.py and inventory_boundaries.py at 64b11d69; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; FND-PARTY-324
environment: null
---

## Observation

This classifies FND-PARTY-324's three raw write-like candidates for
Q-PARTY-136. Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets are hexadecimal.

| Raw candidate | Containing instruction on the established owner stream | Following access |
|---|---|---|
| Resident paragraph alias 2D0F:0007, canonical 2C5F:0B07 | Last immediate byte of the DI-equals-one assignment at 0B05..0B08, within root 2C5F:0AFA. | 0B08 reads byte DS:13F7 into AL. |
| Overlay 182 +2383, file 6ABD3 | Last displacement byte of the near jump +2381..+2384, within root +236F. | +2384 reads byte DS:13F6 into AL on the separate continuing branch. |
| Overlay 187 +277E, file 725AE | Last immediate byte of the six-byte dword push +2779..+277F, within root +2546. | +277F reads byte DS:13F4 into AL. |

Starting a decoder at each operand byte combines that byte with the
following instruction bytes into an apparent write. Following the owner
stream instead consumes the operand and decodes a read at the next
instruction boundary. The near jump's sequential successor is reached
by a different local branch, not by fall-through from that jump.

The direct-call query finds no encoded call to any of the three raw
starts. Its independent root control 2C5F:0AFA recovers far calls
from overlay 187 +20DC and overlay 204 +22F2. The overlay owners
have declared trampolines: 56BD:00E8 for 182 +236F and
56EF:00FC for 187 +2546. Their caller query also recovers a
near call at 182 +117C, plus resident file 1CC68 and overlay
204 +0302 far calls to the latter root. These controls establish
the queried call forms and owner entries, not all incoming transfers.

The resident inventory-range branch-pattern query finds no near call,
jump, conditional jump or loop targeting 2C5F:0B07. Its positive
control 0B16 recovers the established conditional branch at 0B0F.
That branch query covers only resident rows with the same canonical
segment. Overlay jump targets, other segment spellings, indirect or
computed entries, table transfers and changed code are not excluded.
The direct-call query likewise does not search jumps or dynamic targets.

## Interpretation

The three candidates are not unclassified writes on their known owner
streams. Q-PARTY-136 can remove that instruction-identity question.
Their possible alternate entry through excluded mechanisms remains
distinct, as do FND-PARTY-324's broader adjusted-pointer, copied,
aliased and nonliteral mode writers. No negative whole-program writer
claim, actual runtime invariant, queue closure or complete reading follows.
FMT-PARTY-007 remains supported.

## Alternatives

- The census's write-like decode is the owner's instruction: each starts
  inside an operand consumed by the owner stream.
- No direct call proves a candidate unreachable: jumps, computed targets
  and alternate streams remain outside that call search.
- Identifying the operand removes every writer obligation: it only
  classifies these three candidates on the established streams.

## How to reproduce

Reuse FND-PARTY-324's getter-sound-mode-writers.txt. Read the listed
resident range and overlay 182 6ABBF..6ABF3, overlay 187
72376..725C0, starting at their established owner entries. Run
direct_callers.py for 2C5F:0B07, 2C5F:0AFA, 182+2383,
187+277E and the two owner roots 182+236F/187+2546.
Run inventory_boundaries.py with the installed inventory and targets
2C5F:0B07/0B16, preserving its raw-pattern and range limitations.

Reports under GAME_DIR/analysis/handler-census-20261106/ use prefix
getter-mode-raw-: resident-owner.txt, overlay182-owner.txt,
overlay187-owner.txt, call-controls.txt, overlay-owner-callers.txt and
resident-branches.txt. No original game, emulator session or emulated
call supplied evidence here.
