---
id: FND-PARTY-264
title: The class-table accessor and three resource-reader roots have no additional relocated pointer-pair route in a controlled segment-alias census
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005FC0C..0x0005FE11
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000790C9..0x00079254
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F640..0x0006F6F9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:029E..1425:02AA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00050676..0x00050682
    kind: file-data
tool: tools/research/exec-census/alias_far_words.py, direct_callers.py, overlay_listing.py and resident_listing.py at b4f09e77; Python 3.12.10, Capstone wheel 5.0.9 (binding version 5.0.7), xxhash 4.0.1; Ghidra 12.1.3 with scientific-method-engine 19.10.0 ReportReferences.java and ReportDecompileMatches.java
environment: null
---

## Observation

This tests one excluded mechanism in Q-PARTY-124, not all indirect calls.
The input is the installed DSUN.EXE, 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Addresses use load segment 1000;
overlay locations below are offsets in the shipped file.

The alias census examines all 12,965 MZ-relocated or FBOV-fixed segment
words, with none unreadable. For each target it tests offset words within
twelve bytes of each segment word, including equivalent segment:offset
aliases. The four targets are overlay 185's accessor trampoline
`56E6:0020`, and the resource-reader roots `38FF:04AB`, `38FF:0438` and
`38FF:07B1`. This is a segment/offset candidate census; instruction context
decides whether a reported pair actually constructs a pointer.

| Target | Direct far-call candidates | Other pair candidates |
|---|---:|---:|
| `56E6:0020` | 12 | 3 |
| `38FF:04AB` | 110 | 0 |
| `38FF:0438` | 11 | 0 |
| `38FF:07B1` | 1 | 0 |

The independent direct-call reporter reproduces those far-call counts and
also finds the two known push-CS/near calls of `38FF:07B1`, from its
same-segment wrappers. The twelve accessor calls agree with
FND-PARTY-058/068; the 110 direct full-resource calls agree with FND-RNG-091.
This does not newly classify every direct caller's DATA number.

The three non-call accessor candidates are not pointer constructions:

- At file `0x5FDB3` and `0x5FDC6`, the segment immediates belong to
  separate pushed callbacks. The matching offset word is the fixed-up
  segment operand of an intervening call to overlay 190's registration
  routine. It is not either callback's offset.
- At `0x7923B`, the segment operand belongs to the call of overlay 184
  `+07D8`; the apparent matching offset eleven bytes later is the
  segment operand of a different call.
  Entry-based instruction context separates these operands.

Two independent controls exercise the non-call forms. FND-EXE-584's
backend registration at `1425:029E` supplies the segment and offset of
`1425:0009` through separate stores; its pair is found six bytes apart.
FND-EXE-579's three initialized far pointers to `1000:0387` at
`DS:3676`, `DS:367A` and `DS:367E` are all found as adjacent offset/segment
pairs. Other nearby-word candidates for these controls are not treated as
additional pointer definitions.

A read-only query of the reconciled Ghidra overlay view independently
reports the twelve accessor call references through its trampoline and
the mapped jump into its body. A bounded decompiler literal query gives
the same DATA-1001 signed-byte index expression as FND-PARTY-058.
The reference query reports no reference to the backend registration
control, although the byte census finds its separate stores. Therefore
Ghidra's empty reference result is not used to exclude indirect routes.

## Interpretation

No additional stored or nearby-immediate pointer route to these four
roots is present among the relocated/fixed-up segment-plus-offset forms
tested. The three accessor alias candidates are accounted for as unrelated
operands. That removes this specific mechanism from Q-PARTY-124's unknown
list; it does not settle the question or establish FMT-PARTY-007.

## Alternatives

- Targets assembled at runtime from memory, arithmetic, unrelocated
  segment words, or offset/segment producers more than twelve bytes apart
  are excluded. Copies of such targets and dynamic overlay-body entry
  addresses are excluded too. Near indirect transfers, returns and entries
  bypassing the named roots are not excluded by this census.
- Computed resource numbers at separate DATA-request sites, alternate
  readers/writers, index producers and resource-byte mutations remain open.
- A raw candidate count does not prove every call executable; known calls
  are controls, and only the three additional candidates are newly classified.
- The Ghidra reference control failure precludes using its negative result.
  Its positive calls and short decompiler match are navigation corroboration,
  not a complete caller/input or branch reading.

## How to reproduce

At b4f09e77, use the project-local locked evidence Python. Run
`alias_far_words.py <dsun> 56E6:0020 38FF:04AB 38FF:0438 38FF:07B1 1425:0009`
and, separately, `alias_far_words.py <dsun> 1000:0387` in
`tools/research/exec-census/`. Compare with
`direct_callers.py <dsun> 185+0000 38FF:04AB 38FF:0438 38FF:07B1`.
Use `overlay_listing.py <dsun> 175 5FC0C 5FE11` and
`overlay_listing.py <dsun> 190 790C9 79254` for anchored candidate context;
use `resident_listing.py <dsun> 1425:0291..1425:02D4` for the registration.
The stored control pointers are the file-data range cited above, as
FND-EXE-579 records them.

For Ghidra, follow docs/GHIDRA.md's reconciled mapped-image procedure and
use the read-only INST-overlay-20261029g project, program INST-MAPPED.EXE.
Run `ReportReferences.java 7A44:0000 56E6:0020 1425:0009` and
`ReportDecompileMatches.java 7A44:0000 0x3e9 0x3e8 0x48 0x9` with no analysis
or project writes. The mapping reporter converts `7A44:0000` to shipped
file `0x6F640`; mapped addresses are not evidence locations. Retained
local reports are under GAME_DIR/analysis/class-table-closure-20261106/.
No original game was run, no content enters Git and no parity is validated.
