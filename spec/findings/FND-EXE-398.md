---
id: FND-EXE-398
title: Game type-four slot retains a native word after unchecked positioning and carry-mapped zero-count requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:01C3..1425:02E1
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1425:01C3..1425:02E1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0009..1425:00AF
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1425:0009..1425:00AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0563..1425:0589
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1425:0563..1425:0589
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:014A..15F3:020F
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:014A..15F3:020F
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-391's type-four branch begins at 1425:0563. It forwards
the incoming quantity far pair, the selected record's word at offset two,
a far pointer to record offset four formed by word-width offset addition
without segment carry, and the incoming index far pair. It manufactures a
far return and calls local 01C3, then removes fourteen outgoing bytes and
uses the dispatcher's shared returned-word path.

Relative to 01C3's saved-BP frame, the index far pointer is at
SS:BP+0006, the record-content far pointer at +000A, the record word
at +000E and the quantity far pointer at +0010. The producer allocates
six local bytes and saves SI and DI. It initializes result word SS:BP-6
to zero, holds the record word in DI and clamps DI down to the
pointed-to quantity if that unsigned word is smaller. It then raises DI
to at least four. A supplied quantity below four therefore does not reject
the preliminary path.

### Prefix and preliminary word requests

It starts DX at zero and inspects the byte at offset one through the
record-content far pointer. If that byte is not a colon, DX stays zero.
Otherwise it sign-extends the first byte into AX at sixteen-bit width, copies
AX into DX, and subtracts 0040 when the word is unsigned at most
005A, or 0060 otherwise. There is no local alphabetic or length guard.
First bytes 40, 41, 5A, 5B, 61 and 80 give selector words
0000, 0001, 001A, FFFB, 0001 and FF20 respectively when the
second byte is the colon. Those arithmetic cases do not establish an admitted
native selector or readable input extent.

It forwards DX to far 15F3:01C1. That helper saves BP, BX,
CX and DX, puts the incoming word into DX and requests interrupt 21
with AX 3600. Returned AX FFFF maps to zero. Otherwise it multiplies
returned AX by current CX at word-input width, then multiplies only the
first product's low AX word by current BX. The first high product word
is discarded by that second multiplication. It shifts the second DX:AX
product left twice at thirty-two-bit width, copies the shifted high word to
AX and caps that unsigned result at 1000. It restores the saved registers
and returns far. No native carry test qualifies this arithmetic.

For local arithmetic cases, native AX/CX/BX values 0100/0100/0001
give zero, because the first low product is zero. Values 0100/00FF/0001
give three, and FFFE/FFFF/FFFF give seven because the first low
product is two. Combining the two multiplications in wider arithmetic would
change these results. The returned local word is at most 1000, but its
native units and producer reachability remain unadmitted.

The producer copies that AX into SI and initializes local SS:BP-2 to
FFFF. SI below four skips the next helper. Otherwise it forwards the
record-content far pointer to 15F3:01F3. That helper saves BP, BX,
CX, DX and DS, loads DS:DX from its incoming far pointer, sets CX
zero and requests interrupt 21 with AX 3C00. Carry set replaces AX
with FFFF; carry clear keeps the native AX. It restores its saved
registers and returns far. The producer retains that AX in local BP-2.

Local FFFF clears SI and selects the later no-publication path. Every
other local value passes this test, including zero. The producer caps current
SI at current DI unsigned. The helpers do not save SI or DI, so
the initial capacity word and quantity clamp do not establish later SI/DI
bounds across their native requests.

### Position, zero-count request and failure path

The continuing producer forwards its retained local word, current SI and a
zero word to 15F3:014A. This helper saves BP, BX, CX and DX.
From incoming words at SS:BP+0008 and +000A it forms CX:DX
as the first word multiplied by 4000 plus the second word: two paired
right shifts/rotates form the parts, and the final low-word addition carries
into CX. It loads BX from incoming +0006, sets AX 4200 and
requests interrupt 21. It restores its saved registers and returns the
native AX without a local carry mapping. The producer ignores that AX
and the native request's carry.

Next it forwards the retained local word, count zero and a far pointer
to the byte at SS:BP-3 to 15F3:0191. The byte is inside its
allocated local region but has no local initialization on this path. The
helper saves BP, BX, CX, DX and DS, loads DS:DX from its incoming
far pointer, CX from incoming +0008 and BX from +0006, and requests
interrupt 21 with AX 4000. It overwrites AX with one and subtracts
the native carry, returning one for clear or zero for set, regardless of
the native AX. It restores the saved registers and returns far. No
actual byte-count or full-transfer predicate survives that mapping.

The producer tests the mapped whole AX. Nonzero continues. Zero forwards
the retained word to 15F3:01B2, which saves BP and BX, requests
interrupt 21 with AX 3E00 and BX from its incoming word, restores
BX/BP and returns native AX unchanged. The producer ignores this result,
clears SI and sets local result BP-6 to five. Native request completion,
count-zero behavior and effects on the retained word remain unresolved; no
rollback is performed locally.

### Quantity and slot publication

Current SI below four skips publication. Otherwise the producer reloads the
quantity pointer and sets its current word to zero if unsigned below SI,
or reloads the pointer and subtracts SI. It loads the index far pointer,
reads the index word, multiplies it by fourteen at word width and writes
local BP-2 into current DS's indexed cleanup-argument field. The ordered
fields and values are:

| Order | Installed indexed field | Disc indexed field | Value |
| --- | --- | --- | --- |
| 1 | 3F4E | 3EC2 | Retained local word |
| 2 | 3F44 | 3EB8 | Modeled segment 1425 |
| 3 | 3F42 | 3EB6 | Offset 0009 |
| 4 | 3F48 | 3EBC | Modeled segment 1425 |
| 5 | 3F46 | 3EBA | Offset 0044 |
| 6 | 3F4C | 3EC0 | Modeled segment 1425 |
| 7 | 3F4A | 3EBE | Offset 007F |

Each pair reloads the index through the same ES and incoming offset before
multiplying by fourteen. It finally reloads the offset and increments that
index word. There is no local index-at-most-fifteen guard, and repeated reads
do not prove a stable index, valid DS storage or disjoint fields.

All exits return current local BP-6 in AX, restore DI/SI and the
frame, and return far without incoming cleanup. With admitted local storage
that result is zero except the mapped-failure arm's five. Zero includes
preliminary rejection, late SI-below-four and publication paths. A retained
local zero is not excluded from publication by the FFFF guard; FND-EXE-559's
cleanup dispatcher tests its stored argument for zero. Native input admission
and later aliases remain separate from that conditional branch case.

### Published targets

Targets 1425:0009 and 1425:0044 save BP, SI and DI and hold
incoming +0006 in SI and count +0010 in DI. Zero count returns
AX zero without their native calls. Nonzero count first calls 15F3:014A
and ignores its result, then calls a carry-mapped helper. Target 0009
uses incoming +0008/+000A for position and +000C/+000E as the
buffer pointer, calling 15F3:0170. Target 0044 uses +000C/+000E
for position and +0008/+000A as the buffer pointer, calling 15F3:0191.
Each forwards current SI and DI after the position call; neither verifies
that the native request preserved those held words.

0170 has the same input preparation, saves and one-minus-carry result mapping
as 0191, but requests interrupt 21 with AX 3F00. Both published
targets map a nonzero helper result to AX zero and zero to AX five.
Native AX counts are discarded; a carry-clear response alone does not locally
verify that the original requested count was completed.

Cleanup target 1425:007F allocates a local word and saves SI. It holds
the incoming word in SI, pushes a thirty-two-bit zero followed by SI
and calls 014A. Both offset words are consequently zero; it removes six
outgoing bytes. It forwards count zero and SS:BP-1 to 0191, then
forwards current SI to 01B2. It ignores the first two results, removes
their outgoing arguments and returns the last native AX without mapping or
a local slot-field clear. Its local byte is not initialized before the
zero-count request. The producer's earlier zero pushes are sixteen-bit; using
that width for cleanup's wide push would lose one position argument word.

### Relocations and remaining contracts

Producer relocation indices 11 at 0425:02A2, 10 at 0425:02B7
and 9 at 0425:02CC hold shipped 0425, giving modeled 1425.
Indices 16, 15, 14, 13 and 12 at 0425:0209, 021F,
0240, 0252 and 0261 hold shipped 05F3. The wrappers' indices
30, 29, 28, 27, 26, 25 and 24 at 0425:0026,
0034, 0061, 006F, 0090, 00A0 and 00A9 hold that same
segment. These all give modeled 15F3 with load segment 1000.

The helpers' final restores do not establish intermediate native preservation.
In particular SI, DI and, in several helpers, DS/ES are not saved;
native changes can alter later counts, selector words and publication storage.
Pointer extents, termination, buffer and frame aliases, native meanings and all
other callers/writers remain open. No original function is executed to obtain
these observations.

Both editions have identical bytes across the three published targets, the
dispatcher fragment and six native helpers. The producer has 99 corresponding
instructions, differing in the seven indexed field offsets listed above.
Targets 0009, 0044 and 007F contain 27, 27 and 22 instructions;
the dispatcher fragment has 15. Helpers 014A, 0170, 0191, 01B2,
01C1 and 01F3 contain 21, 19, 19, nine, 27 and 18.
Their final returns end each separately decoded body.

## Interpretation

This supplies type four's producer, immediate native helpers and all three
published target bodies. It records the distinct result-five failure arm,
unchecked positioning, carry-only result mapping, zero-count requests, retained
word publication and width-sensitive capacity arithmetic. It does not establish
native file effects, usable capacity or a successful cleanup. Q-EXE-007
retains native contracts, actual input/state writers and caller coverage,
pointer/quantity/index aliases and other startup dependencies. No complete-reading
declaration or game launch exclusion is made.

## Alternatives

Treating the capacity arithmetic as a wide three-factor product would preserve
the first high word that the second multiply discards. Treating the prefix
as a validated case-insensitive letter would invent length and character guards.
Treating returned zero as publication would erase the producer's bypasses.
Calling carry clear a complete transfer would reuse a native count that the
helper discards. Assuming the position request succeeded would add an ignored
result test. Treating the cleanup's zero push as one word would contradict
its operand width and six-byte cleanup. Treating the final native return as
the result of all three cleanup requests would invent their combination.

## How to reproduce

At revision e14bd5c2 require both identities in FND-EXE-350. Use MZ
header size 5200, modeled load segment 1000 and relative segments 0425
and 05F3. Split the low wrapper range at 0044 and 007F and
the native-helper range at 0170, 0191, 01B2, 01C1 and 01F3.
Decode each range separately in sixteen-bit mode, checking complete coverage,
instruction counts, the seven edition-specific fields and all listed relocations.
Header 0006 gives relocation count and 0018 its table offset.

Interpret the producer's byte-98 instruction as sixteen-bit CBW, despite
Capstone's CWDE label. Check the stated prefix and capacity cases at their
original widths, including the discarded first high product. Follow every
outgoing word and far pointer from the type-four dispatcher through the
native helpers and published targets. Distinguish the producer's word-size zero
pushes from cleanup's double-word zero, current versus saved registers, native
AX versus carry-mapped AX, all ignored results and the final local result.
Use FND-EXE-559 for the stored-argument consumer. No negative caller/writer
census is claimed. Licensed bytes remain outside Git; no game, DOSBox or
emulated call runs.
