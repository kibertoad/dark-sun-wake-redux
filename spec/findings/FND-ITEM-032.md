---
id: FND-ITEM-032
title: The qualified direct effect-insertion path admits positive signed codes but does not enforce the bitmap's fifteen-byte bound
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F95F..0x0007F9E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F9E6..0x0007FB29
tool: tools/evidence/report.mjs x86-incoming; tools/research/exec-census/overlay_listing.py at 16b73ac4; locked evidence reader
environment: null
---

## Observation

FND-ITEM-031 identifies the block builder's code-byte destination bound.
FND-PARTY-096 records the general effect API's signed-code split. This
connects that split to the actual insertion store without assuming it is
the only way the table can be populated or changed.

Overlay 193 +166F tests the low code byte at BP+0C. Zero returns. A
negative signed byte requests +192F with the negated byte and returns.
The positive route returns when DS:2100 equals 192; this is an equality
test, not a general unsigned capacity check. It requests DATA and may
request removal of an existing effect, then requests overlay 195 +0AF9.
Only a nonzero low-byte result from that handler reaches insertion +16F6
at +16EC. The code byte is reread from BP+0C for the insertion argument.
Preservation across those intervening children is an explicit dependency.

The insertion body obtains an index from 3143:000D and requests +1CB6.
If the signed index is below DS:2100, it requests a move of ten times
the count-minus-index bytes before incrementing DS:2100. It does not
locally establish a nonnegative index or valid copy extent. At +175A it
stores the low code byte from its own BP+0C into the indexed effect row's
code field, at 4E71:0253 plus ten times the index. This store adds no
code-range check. Other direct fields are written separately; the later
duration branches and queue request do not rewrite this code field in
the body's own instructions. Copy and child aliases remain unproved.

A controlled incoming query targeting canonical file 7F9E6 confirms one
direct caller, +16EC at file 7F9DC, with no additional matching raw or
contested calls. The call is also a required control. This qualifies the
known direct route, not all computed or runtime-constructed callers.

## Interpretation

For a preserved positive code argument on this route, the inserted byte
is 1 through 127. That excludes FND-ITEM-031's conditional flags-byte
overlap from codes 200 through 207 for this producer alone. It does not
establish the narrower bound 119 needed to keep every bitmap store within
the fifteen-byte clear span: codes 120 through 127 select byte offset
15. Neither range is a claim that each code occurs in native gameplay.

Q-ITEM-013 no longer needs the local signed-gate/store connection read
again. It still needs argument preservation, row/index/count admission,
other code writers, moved or restored row contents, table lifetime and
the actual admitted code set. Insertion copies can carry preexisting codes
without passing their values through the API's gate again. Q-ITEM-010
retains unresolved incoming mechanisms. No native defect, complete reading
or confidence promotion follows.

## Alternatives

- The insertion body itself does not reject zero, negative-byte or large
  codes; its qualified direct caller supplies the conditional bound.
- An equality test against capacity does not reject every larger count.
- The API rereads its argument after children; the entry test alone does
  not establish its value if an alias can change that argument.
- One direct incoming match is not universal producer completeness.
- Excluding one conditional flags overlap does not establish the whole
  frame or fifteen-byte bitmap contract.

## How to reproduce

Use installed DSUN.EXE through tools/game-dir.mjs and locked evidence
Python. The readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py with descriptor
193 and ranges 7F95F 7F9E6, then 7F9E6 7FB29.

Reuse FND-ITEM-027's full controlled x86-incoming mapping. Change target
to 0x7F9E6 and add call control 0x7F9DC to the eight existing controls.
Keep source, regions, roots, source-table controls and all scan/instruction/
result limits unchanged. Run node tools/evidence/report.mjs x86-incoming
with that configuration. The result retains 896 unresolved calls, is
untruncated and has partialSearch false. Its computed-transfer and other
coverage exclusions remain; no negative completeness claim is made.

Keep inputs and reports only in GAME_DIR/analysis/hit-incoming-20261104/:
effect-admission-prefix.txt, effect-row-insert-prefix.txt,
effect-row-insert-tail.txt, effect-insert-incoming.json and
effect-insert-incoming.report.json. The retained tail includes the next
function; the insertion claim ends at its return before file 7FB29.
No original listing, bytes or extracted rows are committed.
