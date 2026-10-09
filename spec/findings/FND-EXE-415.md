---
id: FND-EXE-415
title: Identity lookup starts at record zero and supplies distinct first-slot and second-slot callback layouts
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0B26..1425:0BB4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0BB4..1425:0C04
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-414's deeper helper at 0B26 saves BP and SI,
allocates four local bytes and copies incoming word SS:BP+6
to SI. Local BP-2 and BP-4 both start at zero. It does
not initially call 060B to derive these coordinates from the input.

While SI is unsigned at least 0800, it reads current DS:39CE
indexed by local BP-2 times 0108 plus BP-4 times four,
using word arithmetic. Zero returns AX eleven without the final
caller-output stores. Nonzero is re-read and passed to 060B,
with local far output pointers SS:BP-2 and SS:BP-4. The
body removes ten outgoing bytes and tests the full returned AX
through DX. Nonzero returns that AX. Zero subtracts 0800
from SI and repeats the unsigned test using the updated local outputs.
No local check rejects link one or admits the link range before 060B.

When SI is below 0800, it reads the current indexed DS:39CC
word and writes it through incoming far output pointer SS:BP+8.
It then shifts current SI left by three at word width and writes
that result through incoming far output pointer SS:BP+0C.
It returns AX zero. With preserved SI and local storage, the residual
is in 0000..07FF, its shifted output in 0000..3FF8, and
the initial word input permits at most 31 successful link steps.
Those arithmetic bounds do not independently admit callback preservation
or prove the traversed storage and links are valid.

An input below 0800 consumes the initial record-zero/index-zero
DS:39CC word directly. Larger inputs start by consuming that same
coordinate's DS:39CE link. Each successful 060B may have the
age/output/transfer/tag effects described in FND-EXE-409 before a
later error. The final output pointers are re-read in store order;
an alias of the first destination with the second pointer's storage can
change the later destination. This body supplies no rollback or pointer
extent check.

### Second-slot callback wrapper

The wrapper at 0BB4 saves BP and SI and allocates four local
bytes. It calls 0B26 with incoming word SS:BP+6 and far
local outputs SS:BP-2 and SS:BP-4, removes ten outgoing bytes
and tests full returned AX through DX. A nonzero result returns
that AX without the indirect call.

Zero splits local BP-2 into its high four bits in SI and
low twelve bits retained in BP-2. It pushes word eight, local
BP-4, local BP-2, the incoming doubleword at SS:BP+8,
then current DS:3F4E indexed by SI times 000E. It calls
the current far pair at similarly indexed DS:3F46/3F48,
removes twelve outgoing bytes and returns the target's AX.

FND-EXE-414's 0C04 wrapper instead uses DS:3F42/3F44
and pushes word eight, the incoming doubleword, local BP-4,
local BP-2 and the indexed slot argument. The pointer therefore
occupies a different outgoing position in the two wrappers. The shared
lookup and request word eight do not establish interchangeable callback
signatures, read/write meanings or successful eight-byte effects.

### Preservation and admission

Neither listed body saves or restores DS. Saving SI for final return
does not save it around 060B or a callback. Current DS, SI,
local coordinates and caller pointers are consumed after calls; register,
segment and alias admission remains necessary. The encoded output word
comes from current DS:39CC storage, not from the input's high
bits alone. Its producer and lifetime, the initial record-zero links,
slot argument/target writers and the callback's effects remain separate
dependencies.

## Interpretation

This reads 0B26 and 0BB4 requested by FND-EXE-414 and
distinguishes their output and callback layouts. Q-EXE-007 retains
record-zero and encoded-word producers, identity/head/age/dirty writers,
complete callers and input/output admission, computed/cross-region writers
and current-segment/register/native contracts. No complete reading,
interchangeable callback contract, observed original bug or successful
native transfer is declared.

## Alternatives

Deriving the initial coordinates from the incoming word contradicts their
explicit zero initialization. Using one callback argument layout for both
wrappers contradicts their pointer positions and different far pairs.
Treating an input bound as admitted native storage or preserved registers
goes beyond the local arithmetic. Treating a later error as side-effect
free ignores the earlier lookup-helper effects.

## How to reproduce

At revision f8a95bd9 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus each Locations span separately in sixteen-bit mode.
Require 142/80 bytes and 54/34 instructions. Follow zero local
coordinates, the unsigned 0800 test, link re-reads, each 060B
output/result binding, residual shift and ordered caller-pointer stores.
Compare the second-slot wrapper's outgoing widths and order with
FND-EXE-414's first-slot wrapper. Preserve producer, alias, preservation
and native-effect gaps. Licensed bytes remain outside Git; no game,
DOSBox or emulated call runs.
