---
id: FND-EXE-062
title: Typed metadata reader separates guarded width dispatch from zero bypass, base adjustment and one indirect read
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4DD0..0x005F4E96
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003515C0..0x003515F3
tool: Ghidra 12.1.3 PUBLIC and bounded physical dispatch-table reading
environment: null
---

## Observation

FND-EXE-060 prepares the marker, modifier result and input cursor in three
registers, with output-plus-four as the first outgoing stack argument. This
helper saves the marker's low byte, modifier word and original cursor.
It obtains the destination address from its first original stack argument
when storing the result. It has no local source-length or destination-size
argument or guard.

An exact low-byte marker of 80 takes a separate path: add three to the
cursor at 32-bit width, clear its low two bits, read a full word at the
resulting aligned address, store that word through the destination, and
return aligned address plus four. It skips the ordinary zero test, base
adjustment and indirect-read stages. Address addition can wrap; no local
range validation is performed.

All other markers use their low nibble as an index. Values above twelve
reach the abort import established by FND-EXE-041. For zero through twelve,
the guarded table at loaded address `0x007543C0` maps through the physical
PE bytes to these direct targets:

| Low nibble | Target | Direct read/consumption |
|---|---|---|
| 0, 3, 11 | `0x005F4DF4` | Read 32-bit word; advance cursor four |
| 1 | `0x005F4E54` | Call FND-EXE-059's byte reader into a local word |
| 2 | `0x005F4E65` | Read 16-bit word, zero-extend; advance two |
| 4, 12 | `0x005F4E40` | Read 32-bit word; advance eight |
| 5, 6, 7, 8 | `0x005F4E47` | Reach abort import |
| 9 | `0x005F4E75` | Call `0x005F4D70` into a local word, unread callee |
| 10 | `0x005F4E4C` | Read 16-bit word, sign-extend; advance two |

The table mapping is from the exact guarded input index and shipped bytes,
not analyzer symbol order. The four/eight branches read only the first four
bytes while advancing eight; no second word read occurs in those branches.
The helper-call branches use the callee's returned pointer as the continued
cursor and then read the prepared local word. FND-EXE-059 establishes the
first callee's writer on normal termination; the nibble-nine output and
consumption contract remain unread. These direct branches all join the
same post-decode test.

A full decoded word of zero goes straight to the destination store. It
bypasses both base adjustment and indirection regardless of marker bit seven.
For a nonzero word, marker bits masked by 112 are compared with sixteen.
Equality adds the original input cursor; every other mask class adds the
prepared modifier word. Addition is at 32-bit width. FND-EXE-061 establishes
that the modifier supplied by the studied metadata caller is zero on its
ordinary admitted branches, but this helper's direct input contract is not
restricted to that one producer.

After adjustment it tests marker bit seven. Clear stores the adjusted word;
set reads one full word through the adjusted address and stores that read
result. It does not recheck adjusted zero before that indirect read, so a
nonzero decoded word that wraps to zero during adjustment does not gain the
initial zero bypass. It performs no recursive dereference or later adjustment
of the indirectly read result.

The ordinary result store is one full word through the first stack argument.
It then returns the continued cursor, not the stored result or adjusted
address, and restores its frame and saved registers. The indirect read occurs
before that output store. Source/destination aliases can affect later caller
reads and do not establish a validated storage layout.

## Interpretation

The guarded type dispatch, width/advance distinction and ordered post-decode
stages are now bounded. FND-EXE-060's first non-255 marker selects this reader,
whose cursor return supplies the next metadata-byte position while its stored
word is separate. Q-EXE-009 retains nibble-nine consumption, marker provenance
and allowed combinations, source bounds, modifier/result consumers, alignment
admission, alias contracts and remaining matching classification. No complete
schema or replacement reader is implemented.

## Alternatives

Treating eight-byte advance as an eight-byte read, treating zero after
adjustment as the initial decoded-zero bypass, returning the stored target,
or applying ordinary stages to the exact aligned marker are ruled out by the
instructions. Low-nibble bounds do not validate source bytes or indirect
targets. Inferring signedness from the table's neighboring targets would
miss the explicit zero-extension and sign-extension branches. The callback's
zero modifier does not justify deleting this helper's original-cursor branch.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005F4DD0` and
read eighty instructions there, restricting claims to the cited body and
excluding the following function. Hash-check the source and map the thirteen
32-bit table words from loaded address `0x007543C0` to the cited file-data
range through PE sections. Follow the exact-marker bypass, low-nibble mask
and unsigned bound before assigning table targets. Track read versus advance
width, local-word callee outputs, pre-adjustment zero, masked base choice,
wrapped addition, one indirect read, output stack argument and cursor return.
Use FND-EXE-041 for abort, FND-EXE-059 for the known variable-length reader,
and FND-EXE-060/FND-EXE-061 for the studied caller. Keep unread callee,
source-bound and alias contracts conditional. Keep rich reports local and
execute no interpreter or game.
