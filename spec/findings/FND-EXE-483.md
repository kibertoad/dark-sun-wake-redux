---
id: FND-EXE-483
title: Sound utility optional consumer collects two text regions with signed traversal and unchecked row counts
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 190F:0000..190F:019F
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3AE4..1000:3B03
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x000009B2..0x000009D6
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-480's optional 190F:0000 consumer establishes BP, reserves
1766 bytes and saves SI and DI. It reads the first word through its first
incoming far pair at SS:BP+6 into BP-4. It initializes SI and DI zero,
words BP-6/-8/-0A/-0C/-0E/-10/-12/-18/-1A/-1C/-1E/-20/-22
zero and BP-24 one. It calls 1BC3:00DE, 1A7C:040F and
1A7C:0195, then calls 1000:2033 with six words pushed in order:
SS, offset BP-1028, 0019, 004E, 0001, 0001. It removes twelve
bytes and tests full AX against one. Any other value calls 1000:167E
with current DS:D472 and removes four bytes; both paths continue.
These callees' effects and earlier DI preservation are not admitted here.

It adds two to the incoming pair's offset at word width without segment
adjustment, passes that pair to 1000:3AE4, removes four bytes and stores
AX into BP-8. The length helper returns zero for an all-zero pair.
Otherwise it scans for zero with AL zero, CX FFFF and forward byte
comparisons, then returns the complemented remaining count minus one.
It preserves DI and BP, returns far without incoming cleanup and reports
no exhaustion separately. A stable terminator among the scanned bytes
produces its preceding count, zero through FFFE; exhaustion also returns
FFFE. It performs no allocation check or segment adjustment at offset wrap.

The first collector resets SI zero, then tests SI against BP-8 signed,
continuing while less than or equal. Each iteration reloads the incoming
far pair, adds SI to its offset and reads the byte at displacement two.
Opcode 98 sign-extends AL into AX at sixteen-bit operand width; the
resulting word is stored in BP-1A. Byte 0A, signed DI at least 0046,
or zero word selects row completion. Otherwise its low byte is appended
at SS:BP+DI-106E and DI increments. Every path increments SI.

Row completion increments BP-6, writes zero at the current scratch cursor
and compares DI signed with retained BP-18. Only when larger does it
store DI plus six into BP-18; this comparison does not retain a simple
maximum raw length. It uses the old BP-20 value, increments that word,
multiplies the old value signed by 0046, discards the high product and
adds the low product to BP-15E6 as an offset in SS. It passes that
destination and scratch SS:BP-106E to FND-EXE-480's 3A7A copy,
removes eight bytes, ignores the result and resets DI zero.
There is no row-index limit or high-product/offset-wrap check.

With admitted initial DI zero, helper preservation and nonaliased stable
state, a first scratch row can contain 70 data bytes. The next source byte
then selects completion and is consumed without being appended, whether
or not it is a delimiter. The copy includes the scratch terminator and
can therefore write 71 bytes into a destination whose row stride is 70.
The stride does not establish allocation capacity or permissible overlap.

The second collector first resets DI zero, obtains length through 3AE4
from the incoming pair plus 0202, stores it in BP-8 and resets SI zero.
It traverses with the same signed less-than-or-equal test. Its scratch
append limit is 0028 instead of 0046. Completion writes a scratch zero,
stores DI plus one at SS:BP-16C6 plus low signed BP-22-times-002C,
then copies scratch into SS:BP-16EE plus that same low stride product
using the old BP-22 while incrementing it. It ignores the copy result and
resets DI. There is no row limit, capacity or product-overflow check.
Under analogous admitted state its scratch copy is at most 41 bytes,
but neither that local limit nor stride 44 bounds the number of rows.

For either collector, an admitted stable length N from zero through 7FFE
selects N plus one iterations, including the terminator position. Length
words 8000..FFFF skip the initial traversal because zero is greater signed.
Length 7FFF has no terminating SI value: every sixteen-bit signed index
is at most 7FFF, including after wrap. This conditional loop fact does
not predict a native infinite run where row writes can alter live state.
Continuation at 019F, second incoming pair consumers and cleanup remain
outside this reading.

All nine far-call segment words in the interval are MZ relocation records
613 through 605 in descending call order. Encoded 0BC3, 0A7C,
0A7C and six zero segments bind at modeled load 1000 to the callees
named above, including repeated 3AE4 and 3A7A calls.

## Interpretation

This traces the optional consumer into two local text collectors without
assuming a UI outcome or safe frame layout. Per-row scratch limits, row
strides, row counts and signed traversal limits are independent obligations.
Q-EXE-007 retains producer and frame/region extents, actual DS, prior-call
preservation, aliases and state lifetime, the initialization/error callees,
019F's continuation, second input and final cleanup. No complete consumer
reading or launch exclusion is claimed.

## Alternatives

Treating the row stride as capacity ignores unchecked indices and the
first collector's terminator. Treating the full scratch boundary as automatic
line wrapping ignores the consumed source byte. Treating the inclusive
signed traversal as a bounded unsigned length loop ignores high-bit lengths
and the 7FFF case. Treating BP-18 as the maximum raw row length ignores
the extra six applied before later comparisons.

## How to reproduce

At revision 0fbdd80 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Use Capstone 5.0.7 in sixteen-bit mode, MZ header size 1400 and
modeled load segment 1000. Decode shipped A4F0..A68F at IP 0000,
CS 190F, and 4EE4..4F03 at IP 3AE4, CS 1000. Check opcode 98
at 190F:00BB and 013C as sixteen-bit byte-to-word sign extension;
the tool's displayed mnemonic alone is not operand-width evidence.
In the relocation table at 003E with 958 records, check records
613..605 against segment words three bytes after call starts
005C, 0061, 0066, 0081, 0093, 00A4, 0108, 0124 and 018F.
Track scratch and row addressing through SS, low products, consumed
boundary bytes and signed comparisons. Compare symbolic lengths
0000, 7FFE, 7FFF and 8000, and scratch cursors 0046 and 0028.
Licensed bytes stay outside Git; no original process, DOSBox or emulated call runs.
