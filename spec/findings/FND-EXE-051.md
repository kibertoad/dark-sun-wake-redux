---
id: FND-EXE-051
title: Recovered stored handlers adjust the incoming frame and select distinct forwarding paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FE6A0..0x005FE6C5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6D20..0x006D6D41
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6D64..0x006D6D73
tool: Ghidra 12.1.3 PUBLIC, targeted cited-entry recovery and bounded instruction reading
environment: null
---

## Observation

FND-EXE-050 records two stored handler addresses. Initially neither address
had an instruction in the saved analysis, although both were mapped in
executable memory. Targeted recovery decoded both entries. This distinguishes
an analyzer gap from absence of shipped code; no game or interpreter ran.
The regenerated function inventory records the recovered starts and body
sizes, including changed ownership of part of the existing field helper.
Those analyzer body sizes are not contiguous spans or a completeness claim.

The object handler at `0x005FE6A0` begins by adding twelve to its incoming
frame register at 32-bit width. It has no conventional frame-establishing
prologue in the cited range. Relative to that adjusted register it reads a
word at offset minus 56, saves it at offset minus 72, and reads another
word at offset minus 68 into a saved register. It reserves twelve outgoing
stack bytes and pushes the latter word before calling `0x005FD000`.
That callee's effects are outside this reading.

On normal return it pops one outgoing stack word, rereads the saved word at
adjusted-frame offset minus 72, and pushes it. It then writes all ones at
adjusted-frame offset minus 60 and calls `0x00600EB0`. The write follows
the first call and precedes the second. The cited prefix does not perform
a direct call to the ordinary record-cleanup helper from FND-EXE-049.
Whether either callee cleans up records or transfers control is unestablished.

The field handler at `0x006D6D20` adds twenty-four to its incoming frame
register. Relative to that adjusted register it reads words at offsets
minus 116 and minus 112, then compares the word at minus 120 with one.
Equality selects the first forwarding path regardless of the minus-112 word.
Otherwise it increments the minus-112 word's loaded copy at 32-bit width
and tests for zero. An original all-ones word therefore selects the second
path; any other original word selects the first. The increment does not
locally store back into the source field.

The first path reserves twelve outgoing stack bytes, pushes the saved
minus-116 word, writes all ones at adjusted-frame offset minus 120, then
calls `0x00600EB0`. The second path at `0x006D6D64` likewise reserves
twelve bytes and pushes the saved minus-116 word, but writes two at offset
minus 120 and calls `0x005F55E0`. These are distinct callees and local
state writes. Their effects, return behavior and later continuations are
outside this bounded finding.

These reads use frame-relative addressing after an explicit adjustment.
They are not yet identified as particular constructor locals: doing so
requires the dispatcher to establish the incoming register, segment and
frame identity. Likewise, a passed word is not yet a confirmed exception
object, record pointer or cleanup token. The direct handler prefixes do
not establish either callee's complete argument contract.

## Interpretation

The stored targets now have concrete decoded entry behavior and separate
branch conditions. Q-EXE-009 retains dispatcher admission, incoming frame
provenance, field producers, both forwarding callees, the earlier object
callee, exceptional continuation and higher caller return consumption.
The recovered instructions do not establish a complete handler lifecycle
or permit mapping unverified incoming frame words to known local fields.

## Alternatives

Treating an undisassembled entry as absent code is ruled out by targeted
recovery. Treating either entry as an ordinary independent stack-frame
function is unsupported by its incoming-register adjustment. The field
handler does not always forward through one callee: its one-state test
and full-width all-ones test select different paths. A narrow byte test,
a stored increment, or identical local state values on both paths are
ruled out by the instructions. Naming the object helper a destructor or
assuming the forwarding calls never return requires further evidence.

## How to reproduce

Use FND-EXE-011's verified PE and image base. First request instruction
windows at both stored entries; retain the explicit mapped-undisassembled
diagnostics as local analysis metadata. Run RecoverCitedFunctions with
only the two stored addresses, then read eighteen instructions from each
entry and five from `0x006D6D64`. Restrict claims to the listed ranges;
exclude later instructions and callee effects. Track the incoming frame
adjustments, read widths, saved word, branch priority, wrapped increment,
outgoing stack writes and branch-specific local stores. Export the allowed
start/size inventory, preserving its address formatting and reviewing every
changed row. Keep rich reports local and execute no interpreter or game.
