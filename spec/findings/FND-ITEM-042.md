---
id: FND-ITEM-042
title: The malformed GPL thirty-seven instruction targets the accumulator under preserved returning-error context
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00202969..0x0020296D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3278..172C:3498
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:34F2..172C:3585
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:367B..172C:36CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1DCF..172C:1DF7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:00A1..172C:00EE
tool: retained resident_listing.py readings and tools/evidence/report.mjs table at 8182a5b4; composition with FND-COMBAT-103/109, FND-SCRIPT-010/023/025 and FND-CONFIG-144
environment: null
---

## Observation

Q-PARTY-115 retains GPL/37's decoder gap as a possible source of a name
write. FND-COMBAT-103 already follows its rejected expression and confirms
that error reporting does not immediately unwind the parameter reader.
This reading resolves that instruction's own target under explicit context
conditions; it does not decode or repair the whole gap.

### Conditions

Enter the unchanged opcode at GPL/37 offset 0D37 hex with the canonical
handler table. Script/frame/cursor mappings and the following source bytes
remain intact. Stack frames, parameter arrays, operand-pointer fields,
accumulator, stop byte and item records occupy valid disjoint storage.
The expression clear helpers return with the documented fill effects.
The error entry and its children return while preserving these mappings,
register/frame inputs and operand pointer; their unrelated effects are not
excluded. Subsequent helpers preserve these inputs and the accumulator
between the second expression's read and the handler's store. To apply the
final stop boundary, preserve the error entry's stop assignment as well.
These are prerequisites, not newly observed native guarantees.

### The first address is initialized before the rejected byte

Expression entry 3278 clears its value/operator arrays through the known
fill helper and sets operand address 4C13:024D/024F to accumulator
4C13:031B at 32AE/32B5. The rejected B3 expression byte selects the
default error call at 3416, as FND-COMBAT-103 verifies independently.
Its returning continuation does not select an item field or replace the
operand address on this path under the stated preservation conditions.

With level and pending operator still zero, the continuation uses the
ordinary default-value path through 3523. The scalar temporary at BP-6
has not been assigned on this rejected branch; no fixed successful first
parameter value is claimed. That indeterminate numeric value is distinct
from the initialized operand address.

The next source byte, at GPL/37 offset 0D39, is the accumulator expression
code 80 hex. It is neither an operator nor a closing parenthesis, so the
first expression returns without consuming it. Parameter reader 367B
saves both the numeric result and the still-accumulator far address.
It then reads its second parameter without testing the script-stop byte.

### The second parameter and own byte store

The second expression resets the same operand address, consumes the 80
code and loads the accumulator dword through 335F..3369. The following
byte at 0D3A is not an expression operator, so it returns with cursor
0D3A. The decoder's recovery at 0D58 is not this native cursor result.

Handler 1DCF then loads the first saved far address, reads its byte,
subtracts the low byte of parameter two and writes through the first
saved address at 1DF3. Under the stated accumulator/array preservation,
both subtraction inputs are the same accumulator low byte. The own store
therefore clears that byte, leaves its other three bytes unchanged and
does not address an item-name word. The first numeric parameter's
indeterminate value is unused by this subtraction handler.

FND-SCRIPT-023's returning error path sets stop byte 4C13:0326 to one.
Once this handler returns, interpreter 00A1's continuation either returns
on its frame-depth test or checks the stop byte at 00E3. A retained one
prevents the next opcode fetch. No immediate abort before the byte store
is inferred. Error children may themselves change other state before this
boundary; neither their write footprint nor actual return is proven here.

## Interpretation

The admitted unchanged 0D37 route does not need a guessed field-eleven
selector or a guessed recovery layout to identify its own store. With the
listed context preserved it writes the accumulator, then stops before
fetching the remaining gap bytes. This removes that route's local
parameter/target decoding uncertainty from Q-PARTY-115.

The remaining gap obligation is preservation/admission of this context,
the error-child effects held by Q-SCRIPT-003 and Q-CONFIG-008, aliases and
independent incoming entries or changed dispatch/stream state. In
particular, this conditional target is not a global exclusion of name
writes in the gap, a proof of native reachability or an error rollback.
No original function ran and no evidence status or parity changed.

## Alternatives

A returning error does not inherently suppress the current handler's
write. Conversely, an undefined first numeric result does not inherently
make its saved address undefined: the entry initialized that address.
An error child that replaces the address/frame or aliases an item changes
the prerequisites and needs its own evidence. A different incoming offset
is not the 0D37 route examined here.

## How to reproduce

Reuse FND-COMBAT-103/109's fingerprint-checked retained reports under
GAME_DIR/analysis/script-gap-20261103/: rejected-span.json and its report,
expression-dispatch.report.json, handler-and-loop.txt and
parameters-and-main-loop.txt. The expression listing remains under
GAME_DIR/analysis/reader-context-20261103/expression-reader.txt. Follow
only inventoried instruction ranges, keeping its inline tables as data.
The older directory cited by FND-SCRIPT-024 is absent; these newer retained
readings supply the evidence used here.

Repeat node tools/evidence/report.mjs table with rejected-span.json.
The 33-row byte query starts at GPLDATA.GFF file 202969 hex and checks
size 2191945 and XXH3-128 2bd1d2b989eef2dcb0545feb95975fca.
Its first four positions identify opcode 77, rejected B3, accumulator 80
and the next nonoperator byte. The new repeated report is
GAME_DIR/analysis/item-removal-20261104/name-gap-span-repeat.json.
Use DSUN.EXE size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b for regenerated listings. Follow the
pointer stores, both parameter saves, accumulator load and handler store
independently of the undefined scalar, then the post-dispatch stop test.
No original byte sequence, listing or fixture is committed.
