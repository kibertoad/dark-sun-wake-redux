---
id: FND-ITEM-034
title: Two explicit effect-handler outputs write the API's final word rather than its code argument
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F9B0..0x0007F9DC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008249D..0x000824A3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082547..0x0008254D
tool: tools/research/exec-census/operand_range.py and overlay_listing.py at e2f31b41; locked evidence Python and Capstone
environment: null
---

## Observation

FND-ITEM-032 leaves preservation of the API's positive signed code across
its children open. Q-ITEM-013 needs the actual supplied output spans before
treating any child output as a possible code-argument write. This reading
classifies two explicit outputs in overlay 195 +0AF9, not its whole effect
or memory contract. Frame offsets below are hexadecimal.

At overlay 193 +16C0 the API pushes SS and the near offset BP+12, then
the sign-extended code, second slot and first slot before calling overlay
195 +0AF9 at +16CC. Cleanup removes ten argument bytes. In the handler's
ordinary far frame, the supplied far pointer is at BP+0C, with its segment
word at BP+0E. The code word is a separate argument at BP+0A.

The declared-range operand locator finds two explicit far loads of that
output argument, at overlay 195 +0D2D and +0DD7. At each, LES loads the
pointer into ES:BX and the next instruction stores AX as one word at that
exact address. Neither sequence adds an index or offset before the store.
These are two-byte writes, not byte writes merely because a caller later
reads the low byte.

For a preserved pointer and intact nonwrapping frames, both destination
spans are the API wrapper's own SS:[BP+12,BP+14). Its signed code byte occupies
SS:[BP+0C,BP+0D), so neither explicit output overlaps it. The API later
reads the low byte at BP+12 for insertion's final argument at +16D9,
and rereads the distinct code byte at BP+0C at +16E1. The output therefore
can intentionally change the former without changing the latter through
these two stores. This far-pointer relationship does not require DS=SS.

## Interpretation

These two output stores are no longer unresolved code-argument alias
candidates on the admitted call frame. They support a separate final-word
output contract. They do not prove that every handler branch returns,
that the supplied pointer or frames survive other children, or that no
other alias changes the code. FND-ITEM-032's 1..127 bound remains conditional.
Other handler effects, bitmap-builder admission, copies and restored state
remain Q-ITEM-013; segment preservation remains Q-ITEM-011. No rule status
or complete-reading declaration changes.

## Alternatives

- A low-byte consumer does not reduce the producer's word write to one byte.
- An explicit output parameter need not alias every caller argument.
- Disjoint direct output spans do not exclude writes through another alias.
- A located far load does not establish complete native path or caller coverage.

## How to reproduce

Use installed DSUN.EXE resolved through tools/game-dir.mjs and locked
evidence Python. Readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Copy the header and inventory row starting
0x00082269 at e2f31b41 to a local metadata TSV. Run operand_range.py on
that row with hexadecimal bounds C 10, retaining all output. The handler
row declares 82269..824CB and 824CE..82724; linear candidate decoding
and restart behavior do not establish missing or overlapping paths.

Run overlay_listing.py with descriptor 195 and ranges 8249D 824A3,
82547 8254D. Reuse the verified descriptor-193 caller range
7F9B0 7F9DC from FND-ITEM-032's retained API body. The stores' widths,
immediate use of the loaded pointer, push order and cleanup are the controls.
No whole-handler negative writer search is claimed.

Reports stay in GAME_DIR/analysis/hit-incoming-20261104/:
effect-handler-row.tsv, effect-handler-output-sites.txt,
effect-handler-output-one.txt, effect-handler-output-two.txt and
effect-admission-prefix.txt. Some windows include following instructions;
claims use the stated ranges. No original listing or bytes are committed.
