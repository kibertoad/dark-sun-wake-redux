---
id: FND-PARTY-302
title: Five known case-four callers bypass the word client while two cursor callers read the same stored number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059CA7..0x00059EA8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A051..0x0005A179
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A4B5..0x0005A575
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00096F04..0x000973B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009785F..0x00097A5B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084E94..0x00085044
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085044..0x00085052
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092A83..0x00092B36
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092EA2..0x00092F2E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092F81..0x00092FF1
tool: direct_callers.py retained census, overlay_listing.py at b3d9442f, Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with packaged bounded instruction/data/decompile reporters; FND-RNG-040/095 and FND-CONFIG-109/110
environment: null
---

## Observation

FND-PARTY-301 leaves seven encoded producer candidates from its shared
census unassessed: five callers of 197+1A94 and two of 208+05B2.
This checks their actual outgoing argument frames and selected own
branches. Offsets are hex; resource number 1001 is decimal. The
source is the installed DSUN.EXE identity of FND-PARTY-301.

### Five fixed case/number pairs

| Call of 197+1A94 | Case argument | Number argument |
|---|---|---|
| 172+0A73, file 59EA3 | 4 | 0 |
| 172+0D44, file 5A174 | 4 | 0 |
| 172+1140, file 5A570 | 4 | 0 |
| 211+159B, file 973AB | 4 | 0 |
| 211+1C46, file 97A56 | 4 | 0 |

Each call has a literal-zero push, the slot push and a literal-four
push, with no intervening child call before the verified relocated
far call. These match FND-RNG-040's independently recorded five
bindings. They do not supply an unknown number word at entry.

The callee first fills a local effect block through 193+1BA3. Its
early block rejection returns before dispatch. Otherwise its unsigned
case bound admits 0..6 and indexes seven words at CS:1C44. The
fifth word, for case four, is 1AE7. Its own case-four branches read
state and can call the random/level helpers, then converge at +1B75,
which jumps directly to the epilogue at +1C40. They do not fall
through to +1B78 or its +1B83 call of 208+07F2.

Under retained case/frame, code-table binding and ordinary child
continuation, all five known case-four routes therefore bypass that
word client, independently of their slot state or chosen random result.
This is stronger than needing to preserve their zero number in SI
through that whole case. Changed dispatch inputs/tables or unknown
indirect, jump and bypass entries remain outside this path exclusion.

The seven table words are independently read in the Ghidra project.
Its bounded caller contexts recover three known call controls but miss
the decoded instruction at two others, and miss the common case return.
Its decompiler does not expose the requested case-four literal. Those
misses supply no negative proof. Source decoding from each established
containing entry verifies all five frames; the retained complete callee
reading verifies the direct case return. This is a limit of that analysis
view, not a finding that the missing instructions do not exist or a
reason to silently change the joined inventory.

### Two consumers of one stored number

208+129E pushes current DS:9BE2 at +1295, followed by DS:9BE0.
208+170F pushes the same number field at +1706, followed by the same
caster field. The other arguments are coordinate pointers. Neither
push sequence narrows or changes the number; neither provides an
independent producer. FND-CONFIG-109/110 already locate the cursor
setup and event routes that reach these consumers.

The known setup at 208+1612 retains its third argument in DI, calls
the getter and copies its prefix, then publishes current DI to DS:9BE2
at +1655. Thus getter/copy DI retention is required to identify the
stored field with the incoming argument. It is not an unconditional
direct reload from BP+0A at the store. Its own later writes do not
replace the number field before the setup's selected continuation.

FND-RNG-095 retains the bounded literal-writer census and six known
setup callers, correcting FND-RNG-064's copy count to 49 bytes,
73 decimal. Its producer families are two retained SI inputs, three
guarded stored/item-word inputs and literal 306. Literal 306 and the
recorded less-than-328 guarded inputs exclude 1001 at their tested
production points; retained words, effective source identity and other
writers remain conditions. The two SI families still need their actual
producer/lifetime admission. The general setup argument remains a word.

## Interpretation

All ten candidates of FND-PARTY-301's three-interface census now have
an explicit bounded assessment. Its three range-wrapper callers have
that finding's producer map; five case-four routes bypass their later
client; two cursor routes share DS:9BE2 rather than unrelated unknown
number sources. This closes no entire client or queue item.

Q-PARTY-134's remaining work is actual cursor-field producer histories,
the other cast/object number routes and alternate incoming transfers.
Q-PARTY-136 retains dispatch/frame admission, setup and later number
preservation and computed/wider/aliased field writers. Q-PARTY-135/137
retain their separate getter-pair and queue obligations. The eighty
other direct sites remain Q-PARTY-124. FMT-PARTY-007 stays supported,
with no complete reading, answered deletion or changed unresolved-site count.

## Alternatives

- The local callee can accept arbitrary cases even when every known
  direct caller passes four. Unknown entry routes are not excluded.
- Reading a common field at two sites is not proof it still holds a
  setup argument, nor that an earlier guarded value remains unchanged.
- A missed Ghidra instruction/control cannot establish an absent route;
  retain the source-bound positive checks and analysis-view limits.

## How to reproduce

Reuse word-selector-producer-callers.txt in
GAME_DIR/analysis/handler-census-20261106/. Decode the five caller
contexts from their inventory entries with overlay_listing.py; the
case4-source reports use 59CA7..59EA8, 5A051..5A179, 5A4B5..5A575,
96F04..973B0 and 9785F..97A5B. Require all five literal frames.
Reuse the complete 197+1A94 report in the class-table closure store.

Read fourteen table bytes at mapped fbov197::08fe44, and the explicit
caller/case contexts with ReportInstructionContext.java. Keep every
missing-control diagnostic from word-selector-case4-ghidra logs.
Check the original case-four target and +1B75 jump against the source.
For cursor consumers/setup use overlay_listing.py descriptor 208 at
92A83..92B36, 92F81..92FF1 and 92EA2..92F2E. Reports
word-selector208-cursor-first/second and word-selector208-setup are in
the same local store. Apply FND-RNG-095's caller/writer exclusions.
No original game, emulator session or emulated call ran.
