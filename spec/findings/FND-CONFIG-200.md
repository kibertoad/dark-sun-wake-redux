---
id: FND-CONFIG-200
title: A text-output caller forwards its accumulated rectangle and ignores the transfer result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 401B:0172
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:0BD5
tool: Python 3.14.7 and Capstone 5.0.7 bounded resident entry/call-continuation reading and MZ relocation/FBOV descriptor mapping
environment: null
---

## Observation

A query over declared MZ relocated far-call operands to 4328:0BD5
finds one encoded resident site, file 0x0003573D. Descriptor 47
identifies its resident segment as 401B. The containing routine
starts at 401B:0172, file 0x00035522, and far-returns at file
0x0003574A. Its following selector and target tables are data,
not an extension of the returning body.

The entry reserves 26 frame bytes, saves SI/DI and loads two word
arguments following the first far-pointer argument into SI/DI.
After the runtime stack-limit guard, a null first pointer returns
FFFF without the text-output or rectangle-transfer calls. A nonnull
pointer passes current DS:A03D by far address to 1BF3:5945,
then supplies wrapped sums of its record words +96/+98 and the
coordinate arguments to 1BF3:5E2E. These callee outcomes remain
external dependencies.

After that returning call, it re-reads the record. The wrapped
record+96 plus current SI becomes SI and local BP-14. Wrapped
record+98 plus current DI becomes local BP-16; that value plus ten
becomes local BP-18. It requests 1BF3:599E with word 0048 and
sets DI to the returned AX plus one, with word arithmetic.
The later far input pointer comes from the stacked argument at
BP+0E. These reads occur after preceding calls and do not freeze
all input state at entry.

The routine traverses that far input until a zero byte. Ordinary
nonzero bytes call 1BF3:59C1 and add current DI to SI. A percent
byte selects further dispatch after advancing the low offset word.
The ten-word selector table and computed-target table dispatch to
local formatting, character, string and coordinate call paths.
Their complete rendering semantics and argument/storage provenance
are not assigned by this bounded caller reading. Some paths advance
SI by word products with current DI; the string path repeats its
character call until its own zero byte. No general input-capacity
or iteration bound is established here.

Once the main zero-byte exit is reached, the caller compares local
BP-14 with current SI. There is no conditional branch after that
comparison: it pushes BP-18, SI, BP-16 and BP-14, calls
4328:0BD5, removes eight argument bytes, clears AX and returns.
Thus the helper's parameter order is (BP-14, BP-16, SI, BP-18).
An equal first/third coordinate does not locally suppress the call.
The returned transfer AX has no own test and is not propagated.
The call's segment operand at file 0x00035740 is a declared MZ
relocation to relative segment 3328.

FND-CONFIG-199 reads that helper's gates, frame readers, two
reference requests, sentinel-only cleanup and conditional second
frame-word assignment. This caller establishes one encoded incoming
route into that helper after text-loop termination; it does not
initialize the callee's second handle slot or establish a native
first-request failure. Its own prior frame storage and calls are
not evidence of the residual word in the callee's new frame.

## Interpretation

The rectangle helper has a concrete text-output continuation caller.
Returning completion is normalized to zero after the transfer call,
independently of that helper's release or rendering outcomes. The
local coordinate comparison is not a branch gate. Input termination,
accepted record/image state, formatting effects and native output
remain separate conditions rather than implied by the zero return.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain incoming text callers, accepted
records and zero-terminated inputs, formatting targets and callees,
coordinate/slot producers, DS/SS aliases and the cleanup frame word.
One reading completes the text traversal and admits the transfer
helper's handle requests; another bypasses those requests through
its shared-state or rectangle gates, or never completes traversal.
Complete callers and input/callee provenance would distinguish the
code-decided parts. Owner evidence remains necessary for hardware
output. No native or emulated result is claimed.

The reading that the comparison immediately before the transfer call
rejects equal coordinates is ruled out by its unbranched continuation.
A reading that this caller forwards the helper's overall result is
ruled out by the explicit AX clear. The declared-call inventory is
not a complete incoming-reference inventory.

## How to reproduce

Use the MZ relocation table to select far-call operands with offset
0BD5 and relative segment 3328. Resolve the caller's CS through
resident descriptor 47. Read 401B:0172's entry, record-coordinate
reads, input-loop zero exit and the call continuation through its
far return. Keep the selector/target tables separate from code;
follow their local targets only as needed to identify traversal and
coordinate changes, leaving external rendering effects unassigned.
Check the unbranched coordinate comparison, four-word push order,
relocated call and common AX clear. Compare FND-CONFIG-199
without inferring its unassigned frame word from caller history.
