---
id: FND-COMBAT-083
title: Explicit combatant-table pointer stores occur in allocation and release, while allocation aliases are not refreshed by that release helper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007304A..0x00073098
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073251..0x0007330C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067942..0x0006796B
tool: tools/research/exec-census/immediate_search.py, direct_callers.py and overlay_listing.py at 2150d36f; Capstone 5.0.9
environment: null
---

## Observation

The explicit-operand candidate search for words 19C5, 19C7, 19C9 and
19CB, retaining MOV instructions with one of those literal memory
destinations, returns eight word-store candidates. Each was checked in
its overlay-188 instruction context:

| Pointer | Allocation segment, offset stores | Release segment, offset stores |
| --- | --- | --- |
| DS:19C5 | file 0x7305B, 0x7305F | file 0x732E2, 0x732E6 |
| DS:19C9 | file 0x73082, 0x73086 | file 0x732C6, 0x732CA |

Allocation stores the returned DX and AX words from 444C:0008, then
checks their OR for zero. The details request is 0x462 bytes with the
other double-word argument 1; the combatant request is 0x80A with 1.
After each successful check it copies the far pointer to DS:615B and
DS:6157 respectively (FND-COMBAT-072).

The release routine is overlay 188 +03BA. It tests and releases, in
order, DS:19D1, DS:55BE, DS:19CD, DS:19C9, DS:19C5 and DS:19C1.
Each nonnull pointer is passed to 444C:0092 and replaced by returned
DX:AX, with no result test. It clears byte DS:193C at the end. It makes
no own store to DS:615B or DS:6157. The release wrapper explicitly
returns zero after its runtime call returns (FND-CONFIG-184), so those
normal-return assignments clear the two primary pointers, without proving
successful release or validity of their aliases.

The initializer's failure exit +03B1 calls +03BA, then returns AL zero;
its success exit stores DS:193C = 1 and returns AL one. The direct-call
candidate search for 188+03BA finds that near call at file 0x73253 and
one far call at overlay 180 file 0x67966. The latter is an instruction
in the routine starting at file 0x67942. The initializer 188+0000 has a
selected far-call candidate at overlay 180 file 0x676AB, already read
in FND-CONFIG-149. These checked sites do not establish all indirect
incoming routes or unconditional startup identity.

## Interpretation

Allocation and normal-return release have distinct effects on the primary
pointers and their copied aliases. The local release helper clears the
primary fields through the free wrapper's explicit zero result but leaves
the aliases unchanged. A saved-table lifetime proof must account for which
of those representations each later consumer uses.

## Alternatives

The explicit search covers the four literal displacement/immediate words
and the selected MOV destination form, not indexed writes, wider stores
beginning before the fields, bulk copies, pointer aliases or other write
mnemonics. Known allocation stores from FND-COMBAT-072 supply controls
for both halves of both pointers. Raw candidates were checked in the
listed instruction context; the search is no proof of absent other writers.
Q-COMBAT-047 retains those mechanisms, admitted lifetimes, actual archive
provenance and whether later setup replaces any relevant pointer.

## How to reproduce

Run immediate_search.py on the installed licensed DSUN.EXE with 19C5
19C7 19C9 19CB and retain MOV memory destinations exactly at those
four displacements. Read descriptor 188 ranges 7304A 73098 and
73251 7330C, and descriptor 180 range 67942 6796B with overlay_listing.py.
Run direct_callers.py for 188+0000 and 188+03BA; retain its encoding and
indirect-transfer exclusions. FND-CONFIG-149 supplies the initializer
call's existing instruction-boundary control.