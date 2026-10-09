---
id: FND-EXE-399
title: Game type-one published targets make native requests before zero-count copies and map the full result word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:00AF..1425:0107
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:020F..15F3:02A9
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-391 publishes 1425:00DB and 1425:00AF as type-one targets.
The installed 1.1 bodies save BP and prepare fourteen outgoing bytes,
including an operand-size-32 push that copies a far pair. Both read the
current word at DS:3F40 for their last pushed argument. That field's
preliminary producer and zero test are recorded in FND-EXE-391/392; these
wrappers themselves make no segment-validity or zero test of the field.

00DB calls far 15F3:020F; 00AF calls far 15F3:025C. The
seven outgoing words have these bindings, relative to each routine's own
saved-BP frame and using SS for its BP-relative accesses:

| Callee incoming offset | Wrapper 00DB incoming offset | Wrapper 00AF incoming offset |
| --- | --- | --- |
| 0006 | Current DS:3F40 word | Current DS:3F40 word |
| 0008 | 0008 | 000C |
| 000A | 000A | 000E |
| 000C | 0006 | 0006 |
| 000E | 0010 | 0010 |
| 0010 | 000C | 0008 |
| 0012 | 000E | 000A |

MZ relocation index 22 at 0425:00F5 and index 23 at 0425:00C9
hold shipped segment 05F3, giving modeled 15F3 with load segment 1000.
After the calls each wrapper removes fourteen outgoing bytes and tests the
entire returned AX word. Zero returns AX zero; any nonzero word returns
AX six. Both return far without incoming cleanup. The native carry is
not used as their success predicate.

### Native prefix and intermediate preservation

Both resident callees save BP, BX, CX, DX, SI, DI, DS and ES,
then save flags and clear the interrupt-enable flag locally. They put the
incoming word at SS:BP+000C into DX and request interrupt 67 with
AX 4700. Returned AH nonzero selects final cleanup immediately; returned
AX otherwise is not separately checked or retained as a success value.

Returned AH zero continues by loading BX from incoming SS:BP+0008 and
requesting interrupt 67 with AX 4400. DX is not reloaded from the
incoming argument before this second request: it is the then-current native
DX. Again AH nonzero goes to final cleanup and AH zero continues.
No local rollback request follows either prefix failure.

BP/frame preservation and the native word's meaning remain unadmitted. In
particular the first request's AH-zero result does not prove that it preserved
DX for the second request or that either request established a usable memory
mapping. The eventual saved-register restores occur only at final cleanup.

### Opposite local-copy layouts

After both AH-zero tests, 020F loads ES:DI from its incoming far
pointer at SS:BP+0010. It sets DS from incoming +0006 and SI
from +000A. Thus its local copy reads through the supplied segment word
and offset and writes through the supplied far pointer. 025C instead loads
DS:SI from +0010, sets ES from +0006 and DI from +000A,
so those roles are reversed. Each obtains the byte count from +000E
after the native prefix; the wrapper's global segment word was captured into
the outgoing frame before either request, not freshly read here.

Each clears the direction flag, shifts CX right one and skips the repeated
word copy if that count is zero. Otherwise it copies that many words
forward. The following unsigned carry test reads the low bit shifted out
of the original count: neither the count-zero branch nor the word-copy
instructions replace those flags. An odd original count copies one further
byte, and an even count does not.

With admitted code, frame and memory accesses, counts zero, one, two and
three select respectively zero words/zero bytes, zero words/one byte, one
word/zero bytes and one word/one byte. A word input FFFF selects
7FFF word copies and one byte; the local output count is therefore
bounded by the captured byte-count word, independently of the native prefix.
The count is held in CX rather than reloaded between iterations. SI/DI
updates are word-width operations, but readable/writable extents, segment-boundary
behavior and source/destination aliases are not established by this bound.

The forward word-copy sequence is not an overlap-safe buffer contract. No
local range check or disjointness test selects a different direction. Native
prefix effects on the supplied buffers, frame or code remain separate from
the decoded instruction-level case.

### Post-copy result and cleanup order

After the copy path, including captured count zero, both set AX 4800
and request interrupt 67 without reloading DX. Current DX can still be
the second request's native output. They then copy returned AH into AL,
leaving AH unchanged. An AH-zero final result therefore returns AX zero;
a nonzero byte h returns the word whose two bytes are h. This is a
local result encoding, not an assertion about the native request's effect.

On either earlier failure exit, AL has not undergone that final mapping;
the helper retains the prefix request's native AX. Its nonzero AH nevertheless
makes the whole returned word nonzero. Thus all three tested nonzero-AH
positions map to wrapper AX six, while zero AH at all reached requests
maps to wrapper AX zero. A failure at the final request can follow the
completed local copy; the helper performs no local undo of copied bytes.

Every cleanup path restores the saved flags before ES, DS, DI, SI,
DX, CX, BX and BP and returns far without incoming cleanup. The
original interrupt-enable and direction flags are restored at that point, not
before the preceding native calls or copy. Restoring flags precedes restoring
the saved segment/general registers; no hardware timing or interrupt-observation
claim follows from that order. The wrappers subsequently adjust SP and test
AX, so their carry is not the helper's restored entry carry either.

An incoming zero count still reaches the two prefix requests and, if their
AH tests pass, the final request. Only the local copy is skipped. An
early prefix failure or a final failure can return six even for zero count.
Conversely, zero from the wrapper establishes none of the missing native
mapping, pointer, output-extent or lifecycle contracts on its own.

The installed wrappers contain 17 instructions each, ending at 00DA and
0106. The two resident callees contain 45 each, ending at 025B and
02A8. Each separately decoded body covers its full listed span. No disc
copy is read or added as an evidence location in this finding.

## Interpretation

This supplies the remaining type-one published target bodies and their resident
callees, including the shared-word binding, both copy directions, byte-count
bound, three native phases, whole-word caller mapping and flags-first cleanup.
It does not establish native mapping or cleanup effects or valid buffers.
Q-EXE-007 retains all caller/input/state writers, shared-segment provenance,
native preservation and response contracts, pointer/frame/code aliases and the
remaining startup dependencies. No complete-reading declaration or game launch
exclusion is made.

## Alternatives

Returning early for count zero would erase reached native requests. Treating
the second or final request's DX as the original incoming word would supply
reloads absent from the bodies. Testing only AL would lose the earlier
failure exits' nonzero AH. Testing native carry would replace the wrappers'
whole-word predicate. Restoring flags after registers would change the recorded
cleanup order. Treating a final failure as rollback would erase the preceding
copy. Treating the count bound as mapped memory extent or overlap safety
would add storage checks the helpers do not make.

## How to reproduce

At revision 0a9c405f require the installed DSUN.EXE identity from
FND-EXE-350: length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Use MZ header size 0x5200,
relative segments 0x0425 and 0x05F3 and modeled load segment 0x1000.
Split the wrapper range at 00DB and the resident range at 025C,
decode separately in sixteen-bit mode, and check full byte coverage and the
four instruction counts. Header 0006 gives relocation count and 0018 the
table offset; check indices 22 and 23 at the stated operands.

Track the seven outgoing words, current DS:3F40 capture, all native selector
and DX producers, AH tests, opposite segment/pointer assignments, captured
count, shift carry through the word-copy branch, trailing byte and final
AH-to-AL mapping. Check counts zero, one, two, three and FFFF without
executing the original or supplying native stubs. Follow every failure into
the same flags-before-registers cleanup and then the wrappers' full AX tests.
Use FND-EXE-391/392 for the shared field's known producer and
FND-EXE-559 for the cleanup consumer. No negative caller/writer census is
claimed. Licensed bytes stay outside Git; no game, DOSBox or emulated
call runs.
