---
id: FND-CONFIG-176
title: Region initialization returns zero and append rejects count sixteen before writing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0003
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:00A6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0027
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3FA2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F7E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4072:0078
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4400:015C
tool: Python 3.14.7 and Capstone 5.0.7 complete local 16-bit entry readings and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-173's list helper initializes current DS:9F06
through 4237:0003 and checks 4237:0027 for returned
FFFF in its later pass. The complete local 0003 span is
`0x00037573..0x00037597`. After its stack-limit guard
through 1000:2E48, it passes its far pointer, word fill
zero and word count 138 to runtime 1000:3FA2. It then
explicitly returns AX zero, regardless of the runtime
helper's returned pointer. No own pointer-null check or
capacity parameter protects the supplied range.

Runtime 3FA2's complete span is
`0x000091A2..0x000091C1`. It forwards that pointer,
word count and fill byte to local far 3F7E, then returns
the supplied pointer in DX:AX. The complete 3F7E span,
`0x0000917E..0x000091A2`, fills through ES:DI, handling
an odd initial offset with one byte, then words and any
last byte. It clears the direction flag, restores saved
DI, does not write DS or SI, and leaves ES changed.
The named request zeros 138 bytes with word offset wrap.
Its nonnull/capacity validity remains a caller condition.

The complete 4237:00A6 append body occupies
`0x00037616..0x0003765D`. After its guard, it reads
the unsigned word at the supplied destination. A value
at least 16 returns FFFF immediately, without the later
copy or increment. Lower values select wrapped offset
destination offset plus 0A plus eight times that count,
retaining the destination segment. It calls 4072:0078
with the second far pointer as source and that selected
address as destination, then re-reads and increments
the destination word and returns zero. It does not test
the copy helper's AX, validate capacity beyond this count,
or check either pointer for null before the accesses.
State changed across callees remains a separate condition.

The complete 4072:0078 body at
`0x00035998..0x000359B8` has its own stack guard and
copies eight bytes through runtime 1000:0452, whose
saved-register and forward-copy contract is bounded in
FND-CONFIG-175. It makes no pointer-null test and does
not normalize AX. Thus 00A6's zero result is its own
continuation result, not the copy's success signal.

The complete 4237:0027 body occupies
`0x00037597..0x000375E6`. After its guard it calls
0003 on its destination and tests AX FFFF. The ordinary
0003 body always clears AX to zero on its normal return,
so that test has no local normal-return error origin.
The continuation writes word one to the destination,
copies eight source bytes to destination+0A through
4072:0078, then passes the destination to 4400:015C
and returns that callee's AX. It does not test the copy
result. The later list caller checks specifically FFFF,
not every nonzero result.

The complete 4400:015C normalizer spans
`0x0003935C..0x00039408`. It saves DS, SI and DI and
loads the supplied region into DS:SI. A zero count calls
4237:0003 on that region and returns its AX, normally
zero. A nonzero count seeds words at region+2/+4/+6/+8
with 7FFF/7FFF/8001/8001 through local 0139, then scans
records at region+0A plus eight times a word index.
It uses signed comparisons to take minima at record
+0/+2 and maxima at +4/+6, stores those four results
at region+2/+4/+6/+8, and returns one. The extrema
include those seeds: upper seed 8001 is signed -32767,
so an upper record word 8000 alone cannot reduce it to
-32768. No unrestricted full-signed-range extrema claim
is assigned. The starting
word index is count minus one and the decrement loop
uses a sign test; the body does not itself cap the count
at 16. Local 0139's complete span is
`0x00039339..0x0003935C` and writes the four supplied
words while restoring DS and SI.

For stable valid count one, as assigned by 0027 before
its nonoverlapping record copy, the normalizer processes
one record and returns one. A finite valid 0027 path
therefore does not supply FFFF through either normal
callee. This resolves one of FND-CONFIG-173's checked
edges without claiming valid inputs, successful fill/copy,
or a native presentation outcome. All named external
segments were verified at declared MZ relocation operands.
DS and supplied-pointer segments retain their stated
instruction-time provenance.

## Interpretation

Initialization and one-record setup have local zero/one
normal-return contracts. Append has a concrete pre-write
capacity rejection at unsigned count 16, providing a real
FFFF origin for callers that use it. The latter must be
kept distinct from an encoded FFFF test around a helper
whose normal return cannot originate that value. Copy
and initializer returns do not prove pointer validity or
accepted contents.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain counts, capacity and
pointer producers, field and buffer/stack aliasing, all
callers, guard outcomes and runtime input preservation.
One append reading supplies a stable count below 16 and
valid copy addresses; another supplies count at least 16
and takes the pre-write rejection. These are directly
encoded alternatives, but native count reachability remains
unestablished. Invalid count/pointer state or a nonreturning
guard is not an observed original failure.

One 0027 reading supplies a distinct eight-byte source
and valid destination, maintaining its assigned count one;
another changes or aliases those fields. The local setup
order is known, but complete producer/callee coverage is
needed for actual inputs and successful content. Resident
cases are retained under Q-SCRIPT-007 once supported
layouts and the harness exist. No native or emulated
observation is claimed.

## How to reproduce

Read 4237:0003 through 0026, 1000:3FA2 through 3FC0
and 3F7E through 3FA1. Verify the prefixed packed word
arguments, fill count and returned AX rewrite. Read
4237:00A6 through 00EC, checking the unsigned capacity
gate, count-based word offset, ordered copy/increment
and returned zero. Read 4072:0078 through 0097,
4237:0027 through 0075, 4400:015C through 0207 and
0139 through 015B. Check signed extrema/index tests
and saved registers, then compare the caller's FFFF
predicate in FND-CONFIG-173. Resolve every declared
MZ segment and retain pointer, capacity, alias and guard
conditions separately from native outcomes.
