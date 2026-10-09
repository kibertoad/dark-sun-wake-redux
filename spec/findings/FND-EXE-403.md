---
id: FND-EXE-403
title: Registration caller prepares stack records and narrows a wrapped quantity without assigning DS locally
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 167B:003F..167B:0138
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The installed full body containing FND-EXE-558's registration call saves BP,
allocates 019C local bytes and saves SI. It obtains SI from incoming
SS:BP+6. No instruction in this body assigns DS, saves it locally or
restores it between calls. Current DS therefore remains dependent on entry
and each returning callee, rather than established by the local frame setup.

Before the first call it writes these local words through SS:

| BP-relative offset | Value |
| --- | --- |
| -019C | 3 |
| -019A | 0 |
| -0158 | 3 |
| -0156 | 0 |
| -0114 | 2 |
| -0112 | 0040 |
| -0110 | 0 |
| -00D0 | 1 |
| -00CE | 0 |
| -008C | 4 |

It reads a byte through current DS:SI into AL, without first clearing AH,
pushes the entire AX word and calls 1425:13D2. On return it removes
two bytes, copies AX into CX, divides unsigned DX:AX by ten with DX
cleared, subtracts the quotient from CX at word width, and stores CX
at SS:BP-008A. This describes use of the returned word, not its meaning
or the first callee's input width and preservation contract.

It next pushes current DS, current SI, SS and BP-0088, then calls
1000:406D and removes eight bytes. It pushes 0040, current DS,
0230, SS and BP-0088, calls 2D40:3DC2 and removes ten bytes.
Neither call's result is tested locally. The source pointer after the first
callee uses then-current SI and DS, not independently restored entry values.
The calls' memory effects may affect earlier local records; the body does
not reload or reinitialize all their words afterwards.

After those calls it writes zero at SS:BP-0048 and initializes the
doubleword at SS:BP-4 to 00008000. It adds incoming SS:BP+8's
doubleword and then 00004000, both at 32-bit width. It shifts a copy
of the resulting doubleword right fourteen bits, copies the low word into
DX and increments DX at word width if any low fourteen bits of the
stored sum are nonzero. Thus, with admitted frame and incoming reads,
S is the incoming doubleword plus 0000C000 modulo 2^32; the outgoing
word is floor(S/16384) modulo 65536, incremented modulo 65536 when
S modulo 16384 is nonzero. This is not an unbounded rounded quotient.

It pushes SS, BP-019C and that current DX word for 1425:13FE.
The preceding record writes and count production supply FND-EXE-558's
previously partial predecessor inputs. The remaining return branches, cleanup
registration and error calls are recorded there. Calls can still alter local
storage, registers or control flow; no valid record extent or native contract
is inferred from the local stores or supplied far pointers.

The complete body decodes as 77 instructions covering 249 bytes, ending
with the far return at 0137. It restores saved SI and discards its
frame on the shared return. Those final restores do not preserve SI across
the earlier calls that consume its current value.

## Interpretation

This supplies concrete local input producers and the count's intermediate widths.
It leaves entry DS and SI provenance, callees' preservation and memory effects,
input extent, aliases and manager consumption unresolved under Q-EXE-007.
It establishes neither the shared DS:3F40 storage identity nor a complete
reading of registration or the manager.

## Alternatives

A local frame does not establish DS equals SS. Zeroing AH before the
first argument push invents an instruction. Restoring SI at final return
does not restore it before the pointer calls. An unbounded rounded count
would erase 32-bit addition and word narrowing; a sum of FFFFFFFF gives
outgoing zero after the final word increment, not 262144. Treating local
record values as unchanged at manager entry ignores intervening call effects.

## How to reproduce

At revision 84e55877 use only installed DSUN.EXE: length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. With header 0x5200,
relative segment 0x067B and modeled load segment 0x1000, decode
file base 0xB9B0 plus 0x003F through exclusive 0x0138 in
sixteen-bit mode. Require full byte coverage and 77 instructions.

Read the relocation count at header 0x0006 and table offset at 0x0018.
Indices 57..64 respectively locate 067B:0132, 0123, 011B, 0115,
00FE, 00BD, 00A9 and 008C, with shipped segments 3448, 0000,
067B, 3448, 0425, 1D40, 0000 and 0425. Add the modeled
load segment to interpret these relocated operands; keep the call offsets
from the decoded instructions. Follow each outgoing word, source segment,
stack cleanup, local write, arithmetic width and conditional continuation.
Use FND-EXE-558 for the registration suffix. Keep licensed bytes outside
Git; no game, DOSBox or emulated call runs.
