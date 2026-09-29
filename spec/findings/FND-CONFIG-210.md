---
id: FND-CONFIG-210
title: The runtime allocation wrapper clears a wrapped product in bounded chunks after a nonnull heap return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:18D0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:15A5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:18BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0583
tool: Python 3.14.7 and Capstone 5.0.7 bounded resident instruction readings with explicit operand widths and effective segments
environment: null
---

## Observation

FND-CONFIG-209 calls 1000:18D0 with its wrapped product plus one
and double-word one. The complete wrapper spans file
0x00006AD0..0x00006B58, offsets 18D0..1958. It multiplies its two
arguments through near helper 07D9, whose complete reading at
07D9..07EF has the same low-product and cross-product arithmetic
as FND-CONFIG-209's far helper. The low 32-bit result replaces the
first argument in the stack frame, without overflow rejection.

It supplies that product as one double-word argument to far-return
routine 15A5, using a push-CS and near-call frame. It retains returned
DX:AX in a separate local far pointer. Zero combined pointer skips
clearing and returns zero. A nonzero pointer is copied into a second
local far cursor; a zero product also skips clearing, but returns that
original pointer rather than forcing it to null.

For a nonzero remaining product, the loop chooses a word-sized chunk:
FA00 hexadecimal (64,000) when the high word is nonzero or the low
word exceeds FA00, otherwise the low word. It passes that chunk,
byte value zero and the current far cursor to near helper 18BC.
The helper, 18BC..18CD, loads ES:DI from its far argument and performs
a repeated byte store for its word count, then removes eight argument
bytes on return. It has no direction-flag initialization. Consequently
forward zero filling requires the inherited direction flag to be clear;
a set flag stores backward within the same segment. Neither wrapper
nor fill helper establishes that precondition locally.

After each fill, the wrapper advances the local cursor through 0583
with nonnegative double-word amount equal to the chunk, subtracts
that chunk from the remaining double word and repeats until zero.
The selected positive path through 0583..05C6 updates the local far
pointer at its SS address: it accounts for offset carry in the segment
and normalizes the resulting offset to 0..15, with word-sized segment
wrap. The first fill still uses the allocator's unnormalized original
pointer. The wrapper returns the original allocation, not the advanced
cursor. Pointer advancement does not prove the preceding fill's capacity
or correct direction.

The complete local entry body of 15A5 spans offsets 15A5..1621,
file 0x000067A5..0x00006821. It stores current DS in CS:1361 and
restores DS from that location on its common return. A zero request
bypasses its own size and heap branches, returning the zero request
already loaded into DX:AX. A nonzero request adds 19, rejects addition
carry or a high word with any upper twelve bits set, then converts
the accepted total into a paragraph count by shifting right four.
Thus the own admission bound is request at most 1,048,556; it does
not admit an arbitrary nonzero 32-bit allocation size.

After admission, CS:135B and CS:135F choose lower calls 14C4 or
1528, or a linked free-block search. The search reads block extent at
DS:0 and follows DS:6 until its starting block recurs, selecting 1528
if no adequate block is found. An oversized block selects 1582; an
exact-size block calls 143B, copies its word at +8 to +2 and returns
offset four. Those lower callees and heap producers remain unread in
this batch. Rejection explicitly returns DX:AX zero; 0x99 here is an
unprefixed 16-bit sign extension of zero AX, despite Capstone's label.
Heap-search completion, returned segment, header validity and allocated
extent are not established by this entry-body reading alone.

## Interpretation

The allocation chain locally attempts to clear the requested product
before FND-CONFIG-209 writes its header-derived marker. The chunk
bound limits one fill count, not total allocation capacity. Requested
length, paragraph admission, returned header extent, first-pointer
normalization, direction flag and resource transfer count are separate
contracts. None is proved solely by a nonnull allocator return.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain heap-state and header producers,
14C4/1528/1582/143B effects, direction-flag provenance, admitted count
ranges, first-pointer offset and segment-wrap conditions, storage aliases
and lifetime. A valid allocation and clear direction flag can support
forward initialization; a null result skips it. A changed flag or an
incompatible nonzero pointer changes the stores without an own error
branch. Caller and lower-runtime readings would distinguish these cases.
No native or emulated result is claimed.

The reading that a 64,000-byte chunk proves total capacity is ruled
out by the separate allocation and cursor contracts. Unconditional
forward zero initialization is unsupported because neither local body
clears the direction flag. The entry-level size gate does not establish
all lower allocator outcomes or eliminate wrapping in the preceding
product-plus-one wrapper.

## How to reproduce

Read 18D0..1958, 07D9..07EF and 18BC..18CD completely. Track the
stack arguments, product replacement, original pointer versus local
cursor, zero-product path, unsigned chunk selection, callee cleanup,
remaining-count subtraction and returned original pointer. Follow only
the selected nonnegative cursor path through 0583..05C6, retaining its
other branch as outside this call form. Read 15A5..1621 through every
local branch; check carry, high-word mask, paragraph conversion, heap
state, lower calls and common DS restore without assigning their unknown
outputs. Keep fill direction and allocation capacity as dependencies.
