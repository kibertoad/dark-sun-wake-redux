---
id: FND-SOUND-063
title: All four exact music-filename candidates begin inside unrelated established instructions
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0615..38FF:063B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CE64..0x0005CE80
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A788..0x0007A7CD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A191..0x0008A1E3
tool: resident_listing.py, overlay_listing.py and direct_callers.py at 015e35a6; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-062, FND-CONFIG-234/065, FND-COMBAT-065/120 and FND-EXE-603
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-062's exact filename-offset window 08C2..08C3 has
one positive-control loader operand and four interior candidates.
This finding classifies those four against their containing entries.

| Candidate | Containing stream | Why its 08C2 access is not an established instruction |
|---|---|---|
| 3961:0009 | 38FF:0615 | The alias is 38FF:0629, the final byte of the BP+0A argument load at 0627. The next instruction at 062A adds eight to DX. Decoding from 0629 combines the old displacement and following instruction bytes into a different memory operand. |
| 173 +2558 | 173 +2544 | The candidate is the displacement byte of the signed branch at +2557. The following instruction at +2559 adds eight to DX. The established path takes either the branch or that ADD, not a new access at +2558. |
| 190 +2486 | 190 +2448 | The candidate is the final address byte of the DS:6273 load at +2483. The next instruction at +2487 adds 5208 to DX. The false memory displacement spans these unrelated encodings. |
| 201 +0A1C | 201 +09D1 | The candidate is the operand byte of the register ADD at +0A1B. The following +0A1D instruction adds eight to DX. No 08C2 data operand occurs in those established instructions. |

The first entry is the resource-index helper of FND-CONFIG-234/065
and FND-COMBAT-120. The 173 entry is the direction helper used
by FND-COMBAT-065. FND-EXE-603 records the 201 code area. The 190 root resolves
through trampoline 571F:00D4 and has direct callers in overlays
172/173/211/213. The inventory rows identify containing ranges at file
7A788..7A843 and 8A191..8A368. The candidate classification
uses the reviewed entry streams, not an absent function row or
an assumption that a raw matcher always finds real instructions.

The known loader PUSH at 2834:044D remains the independent
positive control from FND-SOUND-061/062. No new filename access
is established by these four decodings. This does not exclude an
alternate entry, changed code or another pointer computation that
reaches the filename; those are separate mechanisms rather than
unclassified forms of the four reported sites.

## Interpretation

Q-SOUND-011 can remove the four exact-name candidates from its
unclassified-instruction component. Together with FND-SOUND-062,
the known field-window and exact-name raw candidates are now
classified. The remaining census obligations are computed/aliased
filename and table addresses, overlapping writes, indirect accesses,
alternate-entry mechanisms and actual extent/field bindings outside
the reviewed streams. The wider name-window report retains its
original scope and count. No universal filename-access exclusion,
complete reading or whole queue closure follows.

## Alternatives

- An exact literal match is a real memory access: all four reported
  cases begin inside earlier instructions and combine neighboring bytes.
- Rejecting these candidates proves no other opener exists: pointer
  construction, aliases and unresolved transfers remain outside the query.

## How to reproduce

Reuse music-table-exact-name-window.txt from FND-SOUND-062.
Read the listed resident range and overlay ranges with descriptors
173, 190 and 201 using the approved readers at the tool revision
above. Retain music-name-resident-interior.txt and
music-name-overlay173-interior.txt, music-name-overlay190-interior.txt,
music-name-overlay201-interior.txt under
GAME_DIR/analysis/handler-census-20261106/. Start at the containing
entries, not the candidate offsets, and compare each established
instruction extent with its candidate. No original game, emulator
session or emulated call supplied evidence. The root query
direct_callers.py for 190+2448 is retained as
music-name-overlay190-root-callers.txt in the same local store.
