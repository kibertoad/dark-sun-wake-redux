---
id: FND-COMBAT-103
title: GPL 37's rejected expression byte has a local continuation after its native error call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00202969..0x0020298A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3278..172C:3585
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0000FA63..0x0000FB29
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:367B..172C:36CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1DCF..172C:1DF7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:00A1..172C:00EE
tool: tools/evidence/report.mjs table and tools/research/exec-census/resident_listing.py at 0f0ab029; pinned executable-reader and locked evidence Python with Capstone 5.0.9
environment: null
---

## Observation

FND-SCRIPT-024 identifies opcode 0x77 at GPL/37 offset 0x0D37 and the
rejected first expression byte at 0x0D38. The retained decoder index stops
at the next offset, 0x0D39, and resumes at a literal target, 0x0D58. This
reading checks the original expression dispatch and enclosing handler;
it does not change the script or extend the decoder by guessing an operand.

### The shipped byte selects the actual default branch

A bounded read of the 33 bytes from the opcode through the byte before
0x0D58 confirms the rejected byte is 0xB3. FND-SCRIPT-024's possible
intended 0xB1 is not the shipped value.

The expression reader fetches a byte, zero-extends it and applies its
extended-variable adjustment only when the relevant 0x40 bit is set.
For 0xB3 that bit is clear. The table index is the resulting value minus
0x80, bounded by 0x62 at 172C:3317..172C:332C. The table therefore has
99 two-byte targets at CS:35A3, shipped file 0x0000FA63..0x0000FB29.

The 0xB3 cell selects 172C:3416. Controls distinguish it from valid cells:
0x80 selects 332C, the accumulator route; 0x8B selects 336C, the two-word
literal reader; 0x8C selects 3351, the nested-instruction route; and 0xB1
selects 33A8, which calls the field reader. These targets agree with the
entry-based instruction reading and FND-SCRIPT-010's expression classes.

At 3416 the function calls 5702:00B1, the error entry of FND-SCRIPT-023.
If that call returns, the next instruction is the ordinary continuation
at 341B. There is no immediate return or newly supplied fixed result at
this call site. The following code combines the current intermediate
state, checks the next byte for operators or a closing parenthesis, and
can reach the ordinary expression return at 3577..3585. Its values and
effective storage still depend on the current frame, segments and error
callee effects; a successful numeric result is not inferred here.

### Error reporting does not directly unwind the parameter reader

The complete parameter reader at 367B..36CE calls the expression reader
once per parameter, saves the returned double word and operand far address,
increments its counter and compares it with the requested count. Its own
body contains no script-stop-byte test between these calls.

Opcode 0x77's complete handler at 1DCF..1DF7 requests two parameters.
After that call returns, it reads the byte through the first saved operand
address, subtracts the low byte of the second parameter and writes the
byte result through the saved address. It contains no separate stop-byte
check or target validation before this write. No target, value or alias
preservation across the earlier error path is established by this fact.

The interpreter loop at 00A1..00EE tests the script-stop byte between
dispatched instructions. FND-SCRIPT-023 records that the error entry's
returning path calls its stop setter only after a shared helper and optional
message. Those child effects and returns remain conditional. A returning
error, parameter completion, the handler's write and the next interpreter
stop check are distinct boundaries, rather than one proven immediate abort.

## Interpretation

The undecoded span begins with a concrete rejected expression branch, not
an unknown opcode layout that can be repaired by substituting 0xB1. The
native caller has a local continuation after the error request. The decoder's
recovery at a later literal target is an analysis convenience, not the
original interpreter's demonstrated error recovery.

Q-COMBAT-059 holds the remaining gap-specific admission, error-effect,
operand-address and stop-preservation question. Q-COMBAT-057 keeps the
106 decoded nonliteral first-operand domains. This reading does not establish
execution of another 0x5E, a zero selector, the malformed site's admission or
any end-to-end original-game outcome. No complete-reading or parity claim follows.

## Alternatives

- Treating the suspected intended byte as shipped would choose a different
  dispatch target and erase the error call. The original resource is unchanged.
- Treating a decoder exception as an immediate native abort conflicts with
  the error call's local continuation and the parameter reader's count-only
  loop. FND-SCRIPT-023 still leaves child return and state effects unresolved.
- A preserved stop byte may prevent another top-level instruction after the
  handler finishes. An error callback, changed stream/frame, operand-address
  alias or independent incoming transfer needs its own evidence before
  drawing a broader absence or execution conclusion.

## How to reproduce

Use the supported DSUN.EXE identity in FND-COMBAT-099 and GPLDATA.GFF,
size 2191945, XXH3 2bd1d2b989eef2dcb0545feb95975fca. Reuse the retained
script index of FND-COMBAT-100 and its GPL/37 error record.

With report.mjs table, use the explicit source fingerprint and read 99
rows of stride two from file 0xFA63, one unsigned two-byte target at row
offset zero. The count evidence is the native 0x62 index bound. Check
cells 0, 11, 12, 49 and 51 against the control branches above. Independently
read 33 one-byte rows from GPLDATA.GFF file 0x202969, bounded by the known
GPL/37 offsets. Keep both configurations and reports outside Git.

Use resident_listing.py on 172C:3278..172C:36CE, reading the dispatch
table separately as data; follow only the selected code branches, not linear
decoding of the inline tables. Also read 172C:367B..172C:36CE,
172C:1DCF..172C:1DF7 and 172C:00A1..172C:00EE. Follow the local continuation
after the error call independently from assumptions about its children.

Configurations and reports are retained in
GAME_DIR/analysis/script-gap-20261103/: expression-dispatch.json and its
report, rejected-span.json and its report, handler-and-loop.txt and
parameters-and-main-loop.txt. The expression listing is retained in
GAME_DIR/analysis/reader-context-20261103/expression-reader.txt.
No script bytes, instruction listings or original-derived fixtures are committed.
