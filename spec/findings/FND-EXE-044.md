---
id: FND-EXE-044
title: Shared-record local names append a thirty-three-byte shipped tail and terminator
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00352050..0x00352071
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE section mapping
environment: null
---

## Observation

FND-EXE-043 copies thirty-four bytes from the loaded address `0x00754E50`
into each of its two local name buffers. Bounded physical section mapping
places those bytes at the cited shipped-file range. The verified file has
thirty-three consecutive nonzero bytes there followed by NUL at relative
index 33. A bounded analyzer read of the same loaded range agrees.
No substantial writing or byte dump is retained in this finding.

The initializer's lookup buffer begins with thirty-two bytes each 65; its
registration buffer begins with the thirty-two encoded pointer bytes in
FND-EXE-043, each 65 or 97. Each tail is copied immediately after that prefix
using eight 32-bit loads/stores and one two-byte load/store. Therefore, under
unchanged tail storage and normal reads, each local buffer contains sixty-five
nonzero bytes and one NUL, at relative index 65. The final two-byte copy supplies
the last nonzero tail byte and the terminator. FND-EXE-042 requests 66 bytes
from the name import; this observation establishes the initializer-produced
name extent, not all inputs that can reach that reader.

The analyzer's references to the tail start show the two initializer reads
already described by FND-EXE-043. This is only a lead for producer/writer
coverage: the query does not exclude interior, overlapping, indexed, aliased,
computed or externally modified writes. No immutable-storage claim is made.

## Interpretation

This narrows FND-EXE-043's copied-tail and local-termination dependency: its
shipped bytes supply the terminator within the copied extent. It does not
establish runtime immutability, atom API behavior, admitted existing names,
record identity or complete lifetime. Q-EXE-009 retains those dependencies.
The shared tail connects the two local name constructions without assigning
an external meaning to its text.

## Alternatives

A shipped tail with an earlier NUL, no NUL within the copied extent, or a
terminator outside the final two-byte copy is ruled out by the verified bytes
and cited construction. Assuming every reader input has this extent merely
because this producer does is not justified. Treating the reference listing
as a complete absence-of-writers result is also not justified.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read thirty-four bytes from
`0x00754E50` and independently map that loaded address through the PE section
raw ranges to the cited file offset. Bound both reads to thirty-four bytes;
check that the first NUL is at relative index 33 and every earlier byte is
nonzero. Follow FND-EXE-043's two prefix and tail copy sequences, especially
the final two-byte copy. A reference query at `0x00754E50` locates the known
reads but is not used as a negative writer proof. Keep runtime storage and
other caller extents conditional. Keep rich reports local and execute no
interpreter or game.
