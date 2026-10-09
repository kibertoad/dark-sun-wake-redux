---
id: FND-EXE-392
title: Game slot request writes quantity on every exit and handle only after native AH zero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:02A9..15F3:02B9
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:02A9..15F3:02B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:02C3..15F3:0325
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:02C3..15F3:0325
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-391's far helper 15F3:02C3 has the same local body in
both game editions. It saves BP, BX, CX, DX and DS, sets
DS to current CS, supplies DX 02B9 and CX zero, and requests
interrupt 21 with AX 3D00. Carry set skips to 02EB. Otherwise
it copies returned AX into BX and requests interrupt 21 with AX
4400. That carry set skips to 02E6; carry clear tests bit
0080 in returned DL. Bit clear also skips to 02E6; bit set
increments current CX. 02E6 requests interrupt 21 with AX 3E00
and current BX, without testing its result before reaching 02EB. It
does not reload BX from the earlier returned value before that request.

At 02EB current CX zero goes to the common quantity store at
031A. Nonzero clears CX and requests interrupt 67 with AX 4200.
Returned AH nonzero also goes to the common quantity store. AH zero
subtracts the incoming word at SS:BP+6 from current BX at word
width; borrow goes to that store. Otherwise it caps BX at the
incoming word at SS:BP+8 using an unsigned comparison. BX below
four then goes to the common quantity store. At least four requests
interrupt 67 with AX 4300 and that BX. Returned AH nonzero
goes to the common quantity store.

Only AH zero after the latter request copies current BX into CX,
loads DS:BX from the incoming far pointer at SS:BP+0A and
stores current DX through it as a word. All paths then load DS:BX
from the incoming far pointer at SS:BP+0E and store current CX
through it as a word. These are two independently loaded output pointers;
the handle store precedes the quantity-pointer load and quantity store.
Aliasing can therefore affect the later pointer or output unless the caller's
bindings and native preservation exclude it. No local pointer or extent check
precedes either store.

The common exit restores DS, DX, CX, BX and BP and returns
far without incoming cleanup. It does not assign a unified AX result;
that word retains the latest native request's result on the selected path.
In particular, it does not clear the handle output on any early exit.
The quantity store uses current CX: its earlier zero assignments are separated
from that store by native requests. Zero after every failed request is not
established without the relevant native register contract. The same applies to
BX used after requests, DX at the handle store and BP/SS used
for frames and output pointers.

For FND-EXE-391's type-one caller, the incoming SS:BP+6 word is
the record's word-two input, SS:BP+8 is the request quantity, the
first far output pointer names the caller's local word BP-2, and
the second names its local word BP-4. The caller supplied both segment
words from SS and formed both offsets with LEA, so the LDS operations
bind these stores to those caller-stack locations under intact caller and native
frames. The output at BP-4 is read by the caller's below-four test;
BP-2 is read only on its later slot-publication path. They were not
locally initialized before this helper. The caller ignores returned AX and
removes twelve outgoing bytes. This binding is not a default value for an
output whose store was skipped, nor proof that the later publication path
always has an initialized handle.

The subsequent far helper 15F3:02A9 saves BX, requests interrupt 67
with AX 4100 and tests returned AH. AH nonzero clears BX;
AH zero keeps current BX. It copies BX to AX, restores incoming
BX and returns far without incoming cleanup. Thus the caller's stored result
is zero on AH nonzero and the native BX word on AH zero;
a zero BX on that latter branch also supplies zero. It makes no
pointer-output stores or other local calls. FND-EXE-391 stores AX into
its shared word and uses a word-zero test before publication. Native result
meaning, preservation, all callers and the pointed-to source at CS:02B9
remain unadmitted.

## Interpretation

This resolves the local output writers used by one slot producer, including
distinct early-exit effects, native AH tests, output order and the companion
word-return producer. Q-EXE-007 retains native request/register contracts,
input/frame/storage admission, aliases, complete caller/writer coverage and the
other type producers and published targets. No complete allocation, slot
publication or game launch exclusion is claimed.

## Alternatives

Treating either early exit as clearing both outputs contradicts the skipped
handle store. Treating the quantity output as always zero on failure ignores
native calls between zeroing CX and storing it. Returning a single checked
AX success flag contradicts the common exit's absent assignment. Treating the
second helper's zero as only AH failure ignores the AH-zero/BX-zero path.
Treating equal output offsets as their storage identity ignores the separately
loaded far segments; this caller's SS bindings require its own intact frame.
Replacing the actual output writers with guessed initial values would change
the caller's publication admission.

## How to reproduce

At revision b3a7215d require both DSUN.EXE identities from FND-EXE-350.
Use MZ header size 5200, relative code segment 05F3 and modeled
load segment 1000. Its shipped-file base is B130. Decode each address
range in Locations separately in sixteen-bit mode, ending after its final far
return; do not decode the intervening bytes as part of either body. Trace
the carry and DL-bit request branches, current-CX gate, AH tests, subtraction
borrow, unsigned cap and below-four gate, conditional handle store and common
quantity store. Track which native request last supplies each outgoing register
and which frame pointer supplies each output. Use FND-EXE-391 for the
caller's push widths, SS/LEA output bindings, cleanup and word tests. Native
results/preservation are unresolved, not seeded or simulated. No negative
caller/writer census is claimed. Licensed bytes stay outside Git; no game
process, DOSBox or emulated call runs.
