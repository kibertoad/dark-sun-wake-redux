---
id: FND-EXE-060
title: Callback metadata reader uses independent byte markers and returns a cursor separately from stored relative targets
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4EA0..0x005F4F78
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A90..0x00600A97
tool: Ghidra 12.1.3 PUBLIC, bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-058 prepares three register inputs for this helper: the callback's
sixth argument, the nonzero offset-28 reader result, and a local output
address. This body saves those register values as context, input cursor and
output address. It reads no original stack-argument slot for those inputs.
It starts a base-query result at zero. For a nonzero context it passes that
context as one outgoing argument to `0x00600A90`. That local callee clears
its return register, restores its frame and returns; it reads no argument,
calls nothing else and accesses no additional memory. Thus both context
branches store zero at output offset zero on normal direct completion.

It reads one byte from the input cursor and advances it by one. A marker
of 255 stores the base-query result, hence zero under the direct callee,
at output offset four. Any other marker is zero-extended and prepared with
the original context for `0x005F4CC0`. Its return is then prepared with the
marker and current cursor for `0x005F4DD0`, with output-plus-four in an
outgoing stack slot. Both callee contracts remain unread. On normal return
it uses the latter return as the new cursor. The non-255 branch therefore
does not have a proven byte length or offset-four writer contract yet.

It next reads a byte at that current cursor, advances by one, and stores
the original byte at output offset 20. It increments a register copy at
byte width and tests for zero, so original marker 255 selects the bypass
branch. This is independent of the first marker. A second marker other
than 255 calls FND-EXE-059's byte reader with the current cursor and a local
word address. On normal termination it stores returned cursor plus the
decoded word, wrapped at 32-bit width, at output offset twelve. It retains
the returned cursor itself for the next byte read; it does not jump its
parsing cursor to that stored target. Marker 255 instead stores zero at
output offset twelve without this first offset decode.

Both arms read another byte at the retained cursor, advance by one, and
store that byte at output offset 21. They then call FND-EXE-059's reader
with the advanced cursor and a local word address. On normal termination
they store returned cursor plus decoded word at output offset sixteen,
again at 32-bit width, and return the returned cursor itself through ordinary
frame restoration. The stored offset-sixteen target is not the function's
return value. This second offset decode occurs even when the earlier markers
were 255, subject to valid input and normal callees.

The body performs full-word stores at output offsets zero, four on the
first-marker bypass, twelve and sixteen, and byte stores at offsets 20 and
21. It has no direct store at output offset eight or to the other bytes
beside those two byte fields. This is not a complete output-write bound for
unread callees, nor a claim that unfilled fields are initialized. It has no
local input-length, cursor-wrap, target-bound or output-size validation.
Source and output aliasing remain conditional; earlier output writes can
change later input where storage overlaps.

For FND-EXE-058, normal reader completion establishes the output byte later
loaded at frame offset minus 36, and the full word later loaded at frame
offset minus 40, by their relative positions in the prepared local region.
The return provides the starting cursor for its subsequent repeated reads.
It does not establish the size, provenance or contents of the stream behind
that cursor, or the role of the two stored targets.

## Interpretation

This narrows metadata production and distinguishes continued cursor movement
from relative-target calculation. The zero base-query helper is a local
constant result, not an imported runtime lookup. Q-EXE-009 retains the typed
reader and modifier helper, first-marker output effects and input length,
relative-target admission, other local output fields, source/output aliases,
remaining matching classification and dispatcher provenance. No complete
metadata schema or safe runtime format reader is implemented.

## Alternatives

Treating the two markers as one guard, advancing the cursor to the first
stored target, returning the final stored target, or interpreting the local
base query as an unknown imported operation are ruled out by the instructions.
The second marker's byte increment does not store an incremented marker in
the output. Two byte stores do not establish a fully initialized word at
offset 20, and direct store absence does not exclude unread callee writes.
A decoded offset alone does not validate its destination.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005F4EA0`,
read eighty instructions there, eight from `0x005F4F5D`, five from
`0x005F4F73`, and seven from `0x00600A90`. Restrict claims to the listed
bodies and exclude the later functions printed. Use FND-EXE-058 for register
writers and output consumers, and FND-EXE-059 for decoded words and cursors.
Track all three saved inputs, local zero-query contract, independent marker
branches, byte versus word stores, typed-reader preparation, both offset
sums, retained cursor, final return and output last-writer gaps. Keep callee,
source bounds and aliases conditional. Keep rich reports local and execute
no interpreter or game.
