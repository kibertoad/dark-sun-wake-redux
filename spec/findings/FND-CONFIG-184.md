---
id: FND-CONFIG-184
title: Initializer failure cleanup rewrites record state and clears handles after unchecked release calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5773:046F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0092
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3BD1
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete local 16-bit readings and declared FBOV/MZ operand mapping
environment: null
---

## Observation

FND-CONFIG-183's checked failure branch calls overlay
200 code 046F before its error helper. This complete
local cleanup spans `0x000895CF..0x00089725`, ending
with far return at `0x00089724`. Its initial byte test
skips everything below when current DS:265C is zero.
Other values enter the full cleanup sequence; it does not
require equality with one. DS-relative addresses below
mean DS at each instruction, not assumed preservation
across external calls.

The first loop compares an unsigned word index against
current word DS:264E, re-reading the count at its later
condition. It accesses stride-eight records through far
field DS:67B7. A record's signed word at +6 below zero
skips that record's state handling. Other indices select
word-wrapped near offset DS:67BB plus 37 times that
index. Signed word +16 of the selected structure greater
than zero selects word at current DS:6578 plus nine
times that value; its negation is stored back at +16.
Zero or negative +16 performs no own write. No pointer,
index range, multiplication overflow or storage-capacity
check protects these accesses. These producer and alias
conditions remain open, not a native corruption claim.

It next tests far fields current DS:2652, then DS:6573.
Each nonzero field is passed to 444C:0092, and the
returned DX:AX is assigned back to that field without an
AX failure test. It then repeats the same nonzero-pointer
release and returned-pointer assignment for exactly 64
stride-nine records, whose pointer fields begin at DS:657A.
The loops do not turn the preceding record mutations or
these calls into a transaction or rollback guarantee.

444C:0092's complete resident span is
`0x00039752..0x000397B6`. It first reads a header word
at supplied far offset minus four, before testing whether
the pointer is zero. With a nonzero pointer, that word
selects a word-shifted extent whose byte at pointer plus
extent minus five is compared with marker 77 hexadecimal.
A matching byte is cleared; null or mismatching paths set
byte 55CD:0000 to one, with the segment verified by MZ
relocation. All paths then pass the original far pointer
to runtime 1000:149B. After that call returns, the wrapper
explicitly returns DX:AX zero. Runtime, allocation-header,
validity and actual release effects remain open. Thus the
cleanup's normal stored zero reflects the wrapper's local
return, not an observed successful release or accepted marker.

The cleanup then walks an unsigned word count DS:265D,
re-read at its loop condition, and clears bit one of each
byte at mapped 52A2:0CB6 plus sixteen times its word
index. There is no own array-capacity or wrapped-offset
rejection. It handles three word fields in this order:
DS:9BFC, DS:9BF8, then DS:9BFA. Each value except
FFFF is passed to resident 2D40:3BD1; the returned AX
is ignored, and the field is set to FFFF afterward.
A skipped sentinel remains FFFF. A returning failed or
no-op service therefore has the same own field clear.

3BD1's complete resident span is
`0x000261D1..0x000261EC`. A signed supplied word at
most one returns that word without the later service;
other words call 1BF3:28C5, ignore its AX and return
FFFF. The cleanup does not distinguish these paths.
The complete 28C5 body spans
`0x000139F5..0x00013A8F`. It saves DS/SI/DI, selects
DS=CS, performs VGA port reads/writes before examining
the supplied word's doubled slot index, then may update
slot flags, paragraph count and subsequent buffers with
forward copies. Its final VGA port reads/writes occur
before saved registers are restored. It has no general
input index bound. Even paths whose slot handling skips
still touch ports; static reading or an interrupt-free
emulated call cannot establish the hardware outcome.

Finally, cleanup passes current far DS:6577, fill byte
zero and count 576 (0240 hexadecimal) to runtime
1000:3FA2, then clears current DS:265C. The fill spans
exactly 64 nine-byte records under valid storage and no
intervening changes. Its count alone does not establish
that the earlier pointer fields were valid. The cleanup
restores saved SI at its local return and does not provide
an overall success normalization. The caller ignores that
result and proceeds to its error helper.

All overlay segment operands, including 52A2, both
release wrappers, 3BD1 and the final fill helper, were
checked through descriptor-200 declared fixups. Resident
runtime, marker and 28C5 segments were checked through
header-derived MZ relocations. No own DS:0DAB store
occurs in cleanup, but aliases, segment/state changes and
unread runtime/service effects remain separate.

## Interpretation

Failure cleanup can mutate record state before releasing
pointers, marking handles and zeroing a table. Result
ignoring and explicit field writes do not prove successful
release or rollback to a prior state. A field can be
cleared after a returning service without establishing its
hardware or runtime outcome. The parent mode re-read
remains conditional on these calls and its later error
helper returning with the required state.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain the cleanup's
callers, byte/count/index/pointer producers, allocation
headers, valid extents, aliasing, word-wrap and DS/SS
conditions, runtime 149B and VGA service outcomes, and
later field writers. One reading supplies valid finite
records and handles with completed releases; another has
a skipped byte, a sentinel/no-op handle or a returning
runtime failure. Their local writes differ, but native
reachability and successful release require more evidence.

The interpretation that this path always restores an
unchanged pre-initialization state is unsupported: own
record writes precede external release calls and no result
protects the later handle clears. The interpretation that
3BD1 calls its VGA service for every nonsentinel word is
ruled out by its signed at-most-one gate. No native or
emulated result is claimed, and Q-SCRIPT-007 cannot
execute this overlay or validate VGA port behavior.

## How to reproduce

Read descriptor-200 code 046F through 05C4 from its
entry. Follow the byte-zero bypass, signed record fields,
unsigned count re-reads, stride-nine releases, mapped
byte-mask loop, handle order and ignored results, final
576-byte fill and byte clear. Verify every declared fixup.
Read resident 444C:0092 through 00F5, including the
pre-null header access and marker segment relocation;
retain runtime 149B as an external dependency. Read
2D40:3BD1 through 3BEB and 1BF3:28C5 through
295E, including both port phases and conditional buffer
compaction. Keep capacity, alias, signed index, release
and hardware outcomes distinct from explicit field clears.
