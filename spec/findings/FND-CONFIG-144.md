---
id: FND-CONFIG-144
title: Overlay 188 setup assigns the iterator stored index and clears its three-byte table
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3FA2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F7E
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; declared FBOV fixup mapping
environment: null
---

## Observation

Overlay 188 entry 5702:00D4 maps to code offset 0CD7,
file offset `0x00073B77`, with return at `0x00073CFF`.
After initial byte assignments, nonzero DS:193E skips to the
common return without reaching the DS:1A32 assignment.
The zero branch continues through several helper calls and loops.

At `0x00073C14..0x00073C28`, this branch passes far pointer
4F49:0C33, fill word zero and count 1572 to resident
1000:3FA2. The segment argument at `0x00073C1B` is a declared
FBOV fixup: raw 0388 names descriptor 113, mapped segment 4F49.
The call's segment operand at `0x00073C23` is a declared fixup
naming descriptor zero, mapped segment 1000.

Resident 1000:3FA2 (`0x000091A2..0x000091C1`) forwards the
far pointer, count and fill byte to 1000:3F7E and returns the
pointer. The complete fill body (`0x0000917E..0x000091A2`)
clears direction, handles an odd starting offset with a leading
byte when count permits, repeats a word containing two copies
of the fill byte, and writes a trailing byte for an odd remainder.
It has no further call. For the stated inputs it writes zero to
1572 consecutive bytes from 4F49:0C33. This spans 524
three-byte positions, including index 523's byte and paired word.

Later, `0x00073CBF` assigns word 523 to DS:1A32 and the next
instruction assigns minus one to DS:1A34. The later call to resident
2D40:2196 at `0x00073CD5` passes DS:1A32 and far output
pointer 5072:0040. The immediate operand at `0x00073CCC`
is a declared FBOV fixup: raw 03A0 names descriptor 116,
mapped segment 5072. The following pushed word 0040 is its
offset. FND-CONFIG-143 reads the callee's BP-relative input
index and far-pointer arguments and its bounded local effects.
This corrects FND-CONFIG-134's interpretation of the raw segment
as a third numeric argument. The complete
containing entry has one early bypass, conditional loop backedges
and one return; there is no other direct DS:1A32 assignment in it.

A raw operand-candidate search over the resident MZ image and
all 49 declared overlay code-and-data ranges found this one
candidate whose first operand writes displacement 1A32 with a
selected instruction mnemonic. Its boundary was confirmed from
the exported entry. This is a qualified inventory, not proof
that no other write exists: indirect or block writes, different
encodings, aliased addresses and incidental data remain outside
that claim.

## Interpretation

The iterator's stored index has a concrete local producer of 523.
That value lies within the three-byte span cleared by this path.
If it remains 523 and the iterator's byte checks pass, returning
it does not itself repeat index three: the following iterator
call starts at 524 and returns sentinel (FND-CONFIG-133).
This conditional result does not establish the actual later value,
table activity or termination of the rest caller's two passes.

## Alternatives

Q-CONFIG-008 retains this setup entry's incoming routes and timing,
intervening helper effects, DS:1A32's indirect or block writers,
three-byte table producers and the 49-byte record's offset-six
word producers. The table clear alone does not establish its
later active bytes; calls after the clear can modify it. The
local 523 assignment rules out a claim that this particular
instruction supplies three, while other reachable writers and
states remain unread. No gameplay identity is assigned to the table.

## How to reproduce

Resolve overlay 188 trampoline 00D4 to code target 0CD7.
Read its bounded entry `0x00073B77..0x00073D00`, checking
the DS:193E bypass, table fill arguments, loops, assignment and
later call. Verify the fill operands, output-pointer segment operand
at `0x00073CCC` and 2D40 call operand at `0x00073CD8`.
Compare the callee argument reads in FND-CONFIG-143. Read the complete fill wrapper
and body at the stated bounds and follow their reordered words.
For the qualified writer inventory, search raw little-endian
1A32 operands in each resident or overlay range, locally decode
candidate starts up to four preceding bytes, and select first
memory operands with displacement 1A32 for mov, inc, dec, add,
sub, and, or, xor, xchg or pop. Validate the positive instruction
from the exported boundary; do not linearly decode whole ranges
or turn that candidate search into an exhaustive negative finding.
