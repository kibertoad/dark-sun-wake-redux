---
id: FND-PARTY-312
title: Both encoded menu-selection admission routes are excluded by unchanged shipped window graphs and matching payload tags
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0364..3D72:03BE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:03C1..3D72:03D8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00032E11..0x00032E29
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3BA6:12B8..3BA6:13A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3BA6:13A4..3BA6:1428
tool: direct_callers.py, alias_far_words.py and resident_listing.py at 9e10a110; tools/evidence/report.mjs table; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with ReportDecompileMatches.java; FND-COMBAT-095, FND-UI-001/002/068, FND-CONFIG-216 and FND-PARTY-311
environment: null
---

## Observation

FND-PARTY-311 identifies a concrete null-selection history in menu
helper 3BA6:0FA4. This checks its encoded caller admission against
the existing shipped-graph census, rather than treating that static
history as a reachable cast-window event. DSUN.EXE is the same
licensed installed build, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets are hexadecimal.

### Two encoded calls into the null-clearing helper

direct_callers.py finds 3BA6:0FA4 called at 3D72:03CA, file 32CEA,
and at same-segment 3BA6:1374, file 31FD4, after PUSH CS. There
is no encoded direct call to FND-PARTY-311's other menu selector
caller 3BA6:0B5C. The alias-aware census finds only the known far
call to 0FA4 and no nearby relocated/fixed-up pair for 0B5C.
The independently known 3D72:0635 far-call sites are controls.
These results retain runtime-built pointers, more distant producers,
unrelocated targets, near jumps, alias-relative and bypass transfers.

### Pointer route requires a MENU payload

The pointer routine's continuing input branch calls 3A8E:0D9A with
the containing window, then tests current A125 nonnull. It reads
that payload's tag at +0, not merely a child-table tag, and dispatches
through four counted tag/target entries. The bounded data reader
verifies this mapping independently of instruction decoding:

| Payload tag | Target in 3D72 |
|---|---|
| APFM | 0476 |
| BUTN | 0404 |
| MENU | 03C1 |
| EBOX | 03D8 |

Only the MENU target pushes current DS:A12D and DS:A125 and calls
0FA4 at 03CA. Unknown tags bypass these four branches. The shared
lookup/hit record and intervening helper effects remain conditions
from FND-UI-068 and FND-CONFIG-095/096. A changed or misbound
payload can defeat the tag argument; it is not excluded by the
child table's source tag alone.

### Keyboard route requires a MENU child-table entry

The other call, 3BA6:1374, belongs to recursive helper 12B8. It
passes a word selection index and the menu pointer after matching
a case-folded key byte. The encoded callers of 12B8 are its own
recursive site 1304 and 3BA6:13A4's site 1408. The same alias-aware
census finds no nearby relocated/fixed-up pointer to 12B8, while
recovering the 0FA4 far-call control.

13A4 scans its supplied window's child table with the F2 byte,
unsigned F3 word count and 30-byte stride. Its full tag comparison
at 13E5 requires MENU at child-entry+4 before reading that entry's
payload pointer and calling 12B8. A mismatch increments the index.
Exhaustion returns zero. FND-CONFIG-216 records the event builder's
call to this root; this is distinct from its later button/accelerator
and generic-key routes. The recursive call is not an independent
initial MENU producer or a proof of finite traversal.

### Shipped source exclusion and actual remaining admission

FND-COMBAT-095, reusing FND-UI-001/002, establishes all 28 shipped
WIND child tables: 402 entries, consisting only of APFM, BUTN and
EBOX. Their referenced payloads exist in RESOURCE.GFF and have the
matching embedded tags recorded by FND-UI-001. There is no shipped
MENU child to enter 13A4's guarded call. Under complete unchanged
table/payload binding, nonwrapping address formation and preserved
fields, pointer selection likewise supplies no MENU payload for
the 03C1 branch. A selected unchanged window payload is WIND and
does not match MENU either.

Consequently these two encoded input routes do not admit 0FA4's
null-selection history from those unchanged graph/payload bindings.
Its admission requires a changed/constructed table or payload,
different record binding, an intervening relevant write/helper effect,
or a transfer outside the tested forms. No actual menu insertion,
native null-record read or stale callback is asserted.

## Interpretation

The unchanged-shipped-graph case is removed from the menu caller
admission in Q-PARTY-134. The remaining conditions name the actual
MENU-producing table/payload/record or alternate transfer needed;
FND-PARTY-311's null-state coordinate/tag/mode conditions remain
for any route admitted that way. Q-PARTY-136 retains guard, frame,
segment and intervening-helper preservation.

This is a conditional exclusion of two encoded routes, not a complete
menu subsystem, whole-client exclusion, queue answer or complete
reading. FMT-PARTY-007 remains supported. It does not expand this
payload claim to unrelated menu behavior or presentation success.

## Alternatives

- Finding 0FA4 proves a normal cast-window null-selection event:
  both encoded input routes require a MENU selection absent from
  the unchanged shipped graph/payload set.
- A MENU child-table tag alone proves the pointer route: that
  branch rereads the selected payload's own tag instead.
- Shipped graph absence excludes all runtime MENU records: it
  does not cover changed/constructed graphs or alternate inputs.

## How to reproduce

Use direct_callers.py for 3BA6:0FA4, 0B5C and 12B8. Use
alias_far_words.py for 0FA4/0B5C with control 3D72:0635, and
for 12B8 with control 0FA4. Read 3D72's established pointer body
through 03BE and its separate 03C1..04BB range, preserving the
inventory gap, and 3BA6:12B8..13A4 and 13A4..1428.

For the table reader, use DSUN.EXE's fingerprint above. Read four
four-byte unsigned tag fields at file 32E11 with stride four, then
four two-byte targets at 32E21 with stride two. Count evidence is
CX four and the bounded scan at 039E..03BA. The table result does
not replace the separate input-to-target instruction reading.
Reuse FND-COMBAT-095's precise provider and source-scope exclusions.

Bounded Ghidra matches recover the 12B8 call in 13A4; its inferred
pointer/string comparison is not evidence of the tag type. Original
instructions establish the full MENU comparison. The same query
misses the known 0FA4 call in the pointer root and supplies no
negative proof; the bounded table and instruction control decide it.
Retain cast-menu-selection-callers/alias-pairs/caller-context,
cast-menu-key-callers/alias-pairs/window-root, cast-menu-pointer-tags
and targets configurations/reports, and cast-menu-admission-ghidra
logs in GAME_DIR/analysis/handler-census-20261106/. No original game,
emulator session or emulated call ran.
