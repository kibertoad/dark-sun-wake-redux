---
id: FND-EXE-395
title: Game type-two published targets reorder six words and stage odd counts around native requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:02E1..1425:031B
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1425:02E1..1425:031B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0039..15F3:0076
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:0039..15F3:0076
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0423..15F3:04DA
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:0423..15F3:04DA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:04DA..15F3:05C8
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:04DA..15F3:05C8
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-393 publishes 1425:02E1 and 1425:02FE beside its cleanup
target. Both wrappers save BP and forward twelve bytes from their incoming
SS:BP frame, including one operand-size-32 push that copies two words.
They call far 15F3:0423 and 15F3:04DA respectively, remove twelve
outgoing bytes, restore BP and return far without incoming cleanup or an
AX test. The callee's AX passes through unchanged.

The table names offsets relative to each routine's own saved-BP frame; it
does not assume that DS equals SS or that pointed-to storage is disjoint.

| Callee incoming offset | Wrapper 02E1 incoming offset | Wrapper 02FE incoming offset |
| --- | --- | --- |
| 0006 | 0006 | 0006 |
| 0008 | 0008 | 000C |
| 000A | 000A | 000E |
| 000C | 0010 | 0010 |
| 000E | 000C | 0008 |
| 0010 | 000E | 000A |

MZ relocation index 8 at 0425:02F7 and index 7 at 0425:0314
each hold shipped segment 05F3, producing modeled segment 15F3 with
load segment 1000. The wrappers' incoming argument producers and complete
callers remain unread; publishing their addresses is not evidence of a call.

### Shared native-request helper

Near 15F3:0039 clears carry and immediately returns when incoming CX is
zero. That path makes no native request and does not initialize the descriptor
or scratch storage. AX and the direction flag are not assigned on that path.

For nonzero CX it saves that count, clears 24 words at ES:0004 through
ES:0033 using a zero word and a cleared direction flag, and restores CX.
It doubles CX at word width, writes the resulting word to current DS:0014
and DS:001C, then shifts CX right one. Thus CX's original high bit is
lost: incoming 8000 would write length zero and reach the request with
CX zero, rather than taking the earlier zero-count bypass. Incoming 0001,
7FFF and FFFF give stored lengths 0002, FFFE and FFFE and final
counts 0001, 7FFF and 7FFF respectively. These are local arithmetic cases.

It writes byte 93 to DS:0019 and DS:0021. The first descriptor's
low word at DS:0016 comes from SI and high byte at DS:0018 from BL;
the second's low word at DS:001E comes from DX and high byte at
DS:0020 from BH. It sets SI four and AX 8700, invokes interrupt 15,
and returns near without mapping the interrupt's AX or carry. ES and DS
are not locally checked for equality, and the helper does not restore the
native request's other register changes. Its clear region ends before the
scratch pair at offsets 0034/0035 and the address fields at 0036/0038.

### Common outer setup

Both far callees save BP, BX, CX, DX, SI, DI, ES and DS and
initially assign DS and ES from CS. They assemble a low word and high byte
from incoming words at SS:BP+0006, +0008 and +000A. Starting with AX
from +000A, BX from +0006 and DX from +0008, they rotate DX
right twice. They add DH into AH at byte width, discarding that addition's
carry; then add BL into AH and add its carry plus DL into BH at byte
width. This is the actual staged arithmetic, not an assumed ordinary far
pointer conversion. In 0423 the resulting low word goes to SI and high
byte to BL; in 04DA they go to DX and BH.

They separately convert the far pointer at SS:BP+000E/+0010 by rotating
the segment left four, using its low nibble as the high byte, masking that
nibble from the low word and adding the offset with carry into the high byte.
In 0423 this forms DX/BH, and in 04DA it forms SI/BL. They load
CX from SS:BP+000C and shift it right one. The shifted-out bit selects
the odd-count path; the remaining word is the bulk request count.

An even incoming count makes one shared-helper call. CX zero therefore
reaches the helper's carry-clear bypass. A returned carry set selects AX
seven; carry clear selects AX zero. These outer routines do not inspect
the native AX to choose their result. They restore their saved registers
and return far without incoming cleanup.

### Callee 0423 odd-count paths

For an odd count, 0423 tests the assembled SI low bit while retaining it
in flags across a shift back to an even low word. It saves SI, DX, BX
and the bulk CX count. It obtains a scratch address from current DS:0036
and DS:0038, the fields written by FND-EXE-394's conditional query path.
The scratch address's validity and that query's execution are not locally checked.

For an odd assembled low word, it calls the shared helper with CX one
and the even low word as the first descriptor, with scratch as the second.
It restores the saved four registers before testing carry. Carry set returns
AX seven. Carry clear advances the first low word by two with carry into
BL and the second low word by one with carry into BH, then makes the
bulk call. A carry-clear bulk result reads byte 0035 through current DS
and writes it through the far pointer at SS:BP+000E, then returns AX zero.

For an even assembled low word, it doubles the bulk CX count and advances
the first low word by that amount with carry into BL. It adds the same
word to its incoming SS:BP+000E offset without advancing the accompanying
segment, sets CX one and makes the scratch request. After restoring the
four saved registers it tests carry and, on clear, makes the bulk call
with those restored values. A carry-clear bulk result reads current DS:0034
and writes that byte through the now-advanced incoming far pointer. Every
tested carry-set result selects AX seven; earlier pointer, descriptor or native
effects are not undone.

### Callee 04DA odd-count paths

For an odd count, 04DA similarly tests the assembled DX low bit and
saves SI, DX, BX and the bulk CX count. Its initial scratch request
uses the assembled even low word/high byte as the first descriptor and the
address fields at current DS:0036/0038 as the second, with CX one.

On the odd-low-word path it restores those registers and tests carry. A
clear result loads a byte through the incoming far pointer at SS:BP+000E,
sets DS from CS and writes it to offset 0035. It saves the same four
registers again and makes another count-one request with scratch as the first
descriptor and the restored assembled address as the second. After restoring
the four registers and testing carry, it advances assembled DX by two with
carry into BH and SI by one with carry into BL before the bulk call.

On the even-low-word path it doubles the bulk count, advances the first
descriptor low word by that amount with carry into BL and adds that word
to its incoming SS:BP+000E offset without segment carry. It makes the
initial scratch request with CX one, restores the four registers and tests
carry. A clear result reads a byte through the advanced incoming far pointer,
sets DS from CS and writes offset 0034. It saves the four registers,
selects scratch as the first descriptor, advances the second descriptor low
word by twice the bulk count with carry into BH, and makes another
count-one request. It restores the four registers and tests carry before the
bulk call, which uses the restored bulk count and addresses.

All tested carry-set results select AX seven. A carry-clear bulk result
selects AX zero. The explicit scratch-byte write and the even-low-word
incoming-offset change precede later failure tests and have no local rollback.
For incoming count one the bulk helper receives zero, so its final carry-clear
bypass does not erase the earlier native requests or local scratch write.

Both callees initially establish DS=ES=CS, but reached interrupts have no
admitted segment/register-preservation contract. The scratch reads, descriptor
stores, ES clear region and later calls cannot silently inherit that equality
after a native request. Restoring the outer saved registers on final return
does not prove that intermediate accesses used the intended storage. The
odd-path four-register restores do not restore DS or ES. Explicit far-pointer
loads and the 04DA CS-to-DS assignments remain distinct operations.

Both editions have identical bytes across the five bounded bodies. Each
wrapper has 11 instructions; the shared helper has 24, callee 0423 has
92 and callee 04DA has 123. Their final returns are at 02FD,
031A, 0075, 04D9 and 05C7 respectively.

## Interpretation

This supplies the two previously unread published targets and their shared
native-request body, including word reordering, count conversion, both alignment
paths, local writes and carry-based AX results. Descriptor construction and
an interrupt request are not evidence that bytes were transferred. Q-EXE-007
retains native semantics and preservation, scratch/descriptor writers, argument
producers, bounds and aliases, complete callers and the other slot types.
No complete reading, usable storage contract or game launch exclusion is claimed.

## Alternatives

Treating the two wrappers as identical forwarding would lose their different
word orders. Returning the native AX would ignore the outer carry mapping.
Treating a zero bulk count as a side-effect-free operation would ignore the
odd-count scratch requests and explicit byte write. Assuming rollback on AX
seven would ignore earlier descriptor writes and incoming-offset changes.
Combining the first two byte additions into an ordinary wide sum would retain
a carry that the first addition drops. Treating every later descriptor access
as CS-relative would supply an unverified native preservation contract.

## How to reproduce

At revision cc885eea require both identities from FND-EXE-350. Use MZ
header size 5200, modeled load segment 1000 and relative segments 0425
and 05F3. Decode each listed body separately in sixteen-bit mode, dividing
the wrapper range at 02FE, and check full byte coverage, instruction counts
and edition equality. Stop the shared helper at 0076; subsequent bytes are
not part of that body. Check relocation table indices 7 and 8 using
the count at header 0006 and table offset at 0018.

Track each wrapper's operand-size-32 push and six outgoing words, each callee's
own SS:BP frame, both byte-addition carries, far-pointer conversion, the count
shift and every odd/alignment branch. Follow all saves/restores and the exact
position of every carry test, scratch-byte write and incoming-offset change.
Check the stated shared-helper count cases at word width and the count-one
outer paths without assuming a native result. Use FND-EXE-393 for publication
and FND-EXE-394 for the scratch-address writer. No negative caller or writer
census is claimed. Licensed bytes stay outside Git; no game process, DOSBox
or emulated call runs.
