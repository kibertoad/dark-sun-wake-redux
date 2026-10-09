---
id: FND-EXE-484
title: Sound utility optional consumer derives coordinates and signed spacing without local admission checks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 190F:019F..190F:02DF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 190F:072A..190F:0730
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000099A..0x000009B2
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-483's continuation sets SS:BP-2 to retained BP-18 plus
one and adds eight to collected-row word BP-6, both at word width.
For a word interpreted signed, its local halving sequence sign-extends AX
into DX, subtracts DX from AX and arithmetic-shifts AX right once.
This gives division by two rounded toward zero. With H denoting that
operation, it computes BP-0E as 12 minus H(BP-6) plus two,
BP-0C as 40 minus H(BP-2), and BP-10 as 40 plus H(BP-2).
It then overwrites BP-0E with 12 minus H(BP-6), discarding the
earlier extra two, and sets BP-12 to 12 plus H(BP-6).
Arithmetic stores wrap at word width; no coordinate-range check occurs.

It pushes ten words in order: 0001, 0000, current DS, D486,
word zero and word two from its second incoming far object, BP-12,
BP-10, BP-0E and BP-0C. Each object word uses a separately
reloaded pair from SS:BP+0A. Push-CS followed by near call 08E5
enters that local far-returning interface with current CS; the caller removes
twenty bytes and ignores its result, then calls 1A7C:0195.
Object meaning, callee effects and preservation remain outside this reading.

The first-row consumer resets SI zero and continues while SI is less
than BP-20 signed. It holds BP-0E plus SI plus two on the stack,
derives row SS:BP-15E6 plus low signed SI-times-0046 and calls
FND-EXE-483's 3AE4 length helper. After removing that pair, it halves
AX logically, subtracts it from 40 and pushes that word. It calls
1000:1FEA with these two words, removes four bytes, then derives
the same row pair afresh and calls 1000:167E, removing four bytes.
It ignores both results, increments current SI and rechecks the loop.
Native preservation and row extents are not admitted by the local stride.
The logical length halving is distinct from the earlier signed halving.

Next it resets SI zero and, while SI is less than BP-22 signed,
adds the word at SS:BP-16C6 plus low signed SI-times-002C into
BP-0A. Each addition wraps. SI increments and the signed limit is
rechecked; there is no local row extent, capacity or sum-overflow check.
It then compares accumulated BP-0A with BP-2 signed. A sum greater
than BP-2 selects a call to 1000:2089 with six pushed words:
SS, BP-1028, 0019, 004E, 0001, 0001. It removes twelve bytes,
calls 1A7C:0164, overwrites AX with FFFF and jumps to 072A.
That cleanup restores DI and SI, resets SP from BP, restores BP and
returns far without incoming argument cleanup. Callee results do not
change this local returned word.

The other branch stores word-width BP-2 minus BP-0A into BP-14,
forms BX as BP-22 plus one at word width, sign-extends BP-14
into DX:AX and divides that signed pair by BX. It stores the signed
quotient into BP-16. There is no local divisor-zero or quotient-range
test before division. This identifies missing admission checks, not native
reachability of a failing divisor or quotient. Continuation at 02DF remains
outside this reading.

The six far-call segment words in the interval are MZ relocation records
604 through 599 in descending order. Encoded segments 0A7C, zero,
zero, zero, zero and 0A7C bind at load segment 1000 to the named
callees. The local 08E5 call uses push-CS rather than a relocated segment.

## Interpretation

The collected counts feed word-width coordinate arithmetic, signed row
iteration, an accumulated size test and signed spacing division. Their
local expressions do not establish admitted geometry, valid object fields,
safe arrays or a nonzero divisor. Q-EXE-007 retains the object and count
producers, frame extents and aliases, downstream interfaces and preservation,
02DF's continuation and remaining return paths. No complete consumer
reading, visible layout or launch exclusion is claimed.

## Alternatives

Keeping the initial extra two in the outgoing BP-0E ignores its later
overwrite. Treating the length half as signed repeats a different arithmetic
sequence. Treating the sum as monotonic ignores word overflow and signed
comparison. Treating the spacing division as admitted assumes missing
count, divisor and quotient bounds.

## How to reproduce

At revision 158c762 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Use Capstone 5.0.7 in sixteen-bit mode, MZ header size 1400 and
modeled load segment 1000. Decode shipped A68F..A7CF at IP 019F
and AC1A..AC20 at IP 072A, CS 190F. Verify opcode 99 at
01AD, 01C0, 01D0, 01DE, 01EE and 02D9 as word-to-pair
sign extension; the tool's displayed mnemonic alone is not width evidence.
In the relocation table at 003E with 958 records, check records
604..599 against segment words three bytes after call starts
0226, 0247, 0256, 026C, 02B6 and 02BE. Track outgoing words
in push order, the last BP-0E writer, logical versus signed shifts,
sum wrap, signed branches and division inputs. Licensed bytes stay outside
Git; no original process, DOSBox or emulated call runs.
