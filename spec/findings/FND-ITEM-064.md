---
id: FND-ITEM-064
title: The final area filter accepts selector forty-two by default without child calls or persistent writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092797..0x0009299E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009299E..0x000929DA
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00091B91..0x00091C06
tool: tools/research/exec-census/overlay_listing.py and tools/evidence/report.mjs table at 3e9dec43; composition with FND-ITEM-063
environment: null
---

## Observation

FND-ITEM-063 leaves the final area-list predicate and compaction children
open for canonical selector D=42. This verifies the complete fifteen-key
table and predicate body, then composes that selected path with +0301's
single-candidate loop. It removes rejection and persistent mutation by
this filter as alternatives for Q-PARTY-109's retained canonical route.

### Full local dispatch contract

208 +0F07 takes target, caster and selector. Before testing the selector,
it reads the target's word from 4F49:0C34 plus three times target, scales
it by 49 and forms a far combatant pointer using DS:19C9. This prefix
does not dereference that computed target pointer yet. It starts its
comparison word at eleven. For unsigned caster below 48 it resolves the
caster's combatant and zero-extends its byte +15 into that word; otherwise
it retains eleven. When DS:45AA is nonzero it replaces the word with the
full word at DS:43D9, not just its low byte.

It searches fifteen selector words at CS:110E, with paired destination
words thirty bytes later. The complete key/branch grouping is:

| Selectors | Local accepting condition |
| --- | --- |
| 2, 30, 136 | Target details byte +12 unsigned in 1..7 |
| 31, 82, 306 | Target details byte +12 equals 9 |
| 60, 165 | Target details byte +12 equals 14 |
| 131 | Target details byte +12 in 1..7 or equal to 12 |
| 29 | Target combatant byte +15 equals the comparison word |
| 34, 36, 116 | Target combatant byte +15 differs from the comparison word |
| 115, 127 | 197 +31AA(target) returns AL zero and the target combatant byte +15 equals the comparison word |
| Every other selector | Accept |

These are local conditions with valid retained field/frame identities.
The target-details branches resolve its details index through combatant
word +04 and the 66-byte table at DS:19C5. The role of +31AA's children
is not established by the Boolean test above. The listed field comparisons
do not validate indices or the preceding metadata/combatant reads.

### Selector forty-two's closed path

D=42 matches none of the fifteen keys. After all comparisons fail, the
body reaches the default AL-one return. No selected case, target-details
read or pointed-to target-combatant field read occurs on that path. The
prefix's target metadata read and caster/script reads still happen before
the default and must be readable. The values used solely to form the
unused target pointer and comparison word do not decide this default result.

This selected path has no child call, interrupt, port operation or own
persistent-data store. Its stores are confined to its stack frame under
disjoint valid storage; it restores SI and DI, while ES and other scratch
registers are not a preservation claim. Consequently it does not itself
change the selected row, target record, table count, DATA buffer or
equipment flag under those storage conditions. AL is one; AH need not be
zero until the caller explicitly clears it.

### Composition with the single-candidate loop

208 +0301 starts at index zero and passes target, owner and D to +0F07.
FND-ITEM-063's FF/mode-one branch capped the input count at one before
this helper. With count one and D=42, returned AL one skips its rejection
and compaction call, increments the index to one and returns count one.
With count zero, the helper makes no predicate call and returns zero.
Neither admitted selected path writes the output entries or calls the
compaction child. It does not select a replacement candidate.

Thus, given the preceding scanner's valid count/list and retained D=42,
the final filter preserves that zero-or-one result rather than introducing
another eligibility test. A retained single candidate reaches the outer
post-hit loop's first 371D:050F(target, zero) request without an earlier
per-target call on this list. This removes the filter and earlier list
iterations as independent row-mutating children on this narrow route.
It still does not supply the removal flag missing from the scanner's test:
FND-PARTY-262 and FND-ITEM-063's row-flag/slot/table production and preservation
conditions remain. No actual target list or unload was observed.

## Interpretation

Q-PARTY-109 no longer requires the default predicate or compaction child
to explain canonical D=42 target loss or persistent mutations. Actual
range/coordinate setup, scanner children and row flags, metadata and
near-list/frame admission, removal/redraw/deferred effects and flag values
remain. Q-PARTY-077 retains the broader DUAL slot-lifetime question.
This is not a proof for other selectors or a complete-reading promotion.

## Alternatives

A target-origin or side restriction for D=42 would add a key absent from
the complete table. Treating a default result as permission to omit the
prefix's reads would change instruction order. Treating script comparison
state as a byte would change its recorded word load. A valid pointer
calculation is not proof of valid pointed storage on cases that dereference
it. A filter that preserves the list does not establish a later removable row.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for descriptor 208,
92797..9299E hex, following the established entry, fifteen-comparison loop,
paired table jump and all return branches. The full listing is retained as
placement-area-filter-complete.txt under
GAME_DIR/analysis/item-removal-20261104/. The earlier partial
placement-area-filter.txt remains diagnostic only. Reuse FND-ITEM-063's
placement-area-list.txt for +0301 and the preceding cap.

Run report.mjs table on the same DSUN.EXE, start 9299E hex, count thirty,
stride two, unsigned word at zero. The first fifteen words are keys and
the next fifteen their paired destinations. Count and pairing evidence
are +0F70's fifteen, +0F73's CS:110E and +0F86's displacement 1E hex.
Check that every destination is an instruction reached in the full body;
key 60 pairs with +0F8A and key 29 with +10E8 as distinct branch controls.
Compare all fifteen keys with 42 and trace the failed-search jump to +1108.

placement-area-filter-table-controls.json, placement-area-filter-table.json
and its -repeat.json remain beside the complete listing. Repeats are
byte-identical, SHA-256
a9dbe10faa572d55a6b6a94bc9c549a854c700e2008609a6bd90a1b3529ed0ec.
No original code, memory or fixture is committed or executed.
