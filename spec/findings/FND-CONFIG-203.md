---
id: FND-CONFIG-203
title: The formatter literal-output path appends a terminator without a destination-capacity gate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3150:000E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3150:05D6
tool: Python 3.14.7 and Capstone 5.0.7 bounded resident formatter entry, literal-output and common-exit instruction reading
environment: null
---

## Observation

FND-CONFIG-202 supplies destination SS:BP-18 and a format pointer
to resident 3150:000E. The entry at file 0x0002670E reserves
0230 hexadecimal frame bytes, saves SI/DI and initializes word
BP-0A to zero. It retains an extra-argument cursor derived from
BP+0E and initializes private formatting fields before reaching
the format-byte test. This bounded reading does not assign the
full percent-conversion parser or its numeric helpers.

At the shared byte test, file 0x00026D51, it loads the supplied
far format pointer from BP+0A, reads its current byte, stores the
character word at BP-0C and tests that word for zero. The
unprefixed extension instruction was checked in the 16-bit operand
context: it sign-extends AL into AX before the word store. A nonzero
byte reaches the comparison with percent at file 0x00026763.
A non-percent byte goes directly to the literal-output block at
3150:05D6, file 0x00026CD6.

That block loads the supplied far destination from BP+06, adds
word BP-0A to its offset without carrying into the segment, stores
the low character byte there and increments BP-0A as a word.
It then increments the low word of the format pointer and repeats
the byte test. No output-length or destination-capacity comparison
occurs on this literal path, and no callee is invoked there.

A zero format byte selects file 0x00026D64. It loads the same
far destination, adds current BP-0A to its offset, stores one zero
byte there, copies BP-0A to AX and restores DI/SI and the frame
before far-return at file 0x00026D74. The terminator is an
additional write, not included in the returned word count.

The adjacent common output blocks also write destination-plus-count
for padding and copied source bytes, incrementing the same word.
Their admission conditions and conversion producers remain outside
this bounded path contract. No general formatter capacity or valid
source/argument guarantee is established by this reading.

### Conditional literal case

With valid nonaliasing destination and a terminated format of six
nonzero, non-percent bytes, starting at the entry and without offset
wrap, the local path writes those six bytes and a zero at destination
plus six, returning count six. It therefore requires seven bytes of
storage. Five such bytes and their zero require six bytes. These
are deductions from the literal path, not tests of the native caller's
format. They do not establish the actual DS:291E contents or the
output of any percent conversion in FND-CONFIG-202.

## Interpretation

The six-byte destination area in FND-CONFIG-202 is not sufficient
proof of bounded formatter output. The literal path has an explicit
extra terminator write and word-offset arithmetic. Actual fit depends
on the caller's format, arguments and conversion output. The returned
count alone does not reserve the terminator or prove safe storage.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain the caller's format/DS
provenance, conversion parser and helper branches, argument widths,
source termination and frame aliases. One reading supplies output
of at most five bytes plus a terminator to the six-byte area; another
produces more. Complete format and admitted-input readings would
separate those cases. No native overwrite, original-game defect or
emulated result is claimed.

The reading that the literal path enforces the caller's destination
capacity is ruled out by its complete write/test sequence. A reading
that returned count includes the terminating zero is ruled out by the
extra zero store followed by the unchanged-count return.

## How to reproduce

Read 3150:000E's count initialization and jump to code 0651,
then the byte test, non-percent branch to 05D6, literal write and
pointer/count increments. Follow the zero-byte exit at 0664 through
far return 0674. Check the segment/offset operations and absence
of a capacity predicate along that path. Keep percent conversion
and native caller format contents outside the claim. Derive the
six-literal-byte case with valid nonaliasing storage and no wrap
explicitly assumed; compare FND-CONFIG-202's frame area.
