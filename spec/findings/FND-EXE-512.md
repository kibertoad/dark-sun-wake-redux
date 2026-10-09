---
id: FND-EXE-512
title: Game diagnostic initialization masks a returned handle bit and changes record state before allocation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0705..1000:0716
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0705..1000:0716
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:364E..1000:3726
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:364E..1000:3726
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-511's far helper 0705 saves BP, sets AX 4400 and
BX to incoming SS:BP+6, then requests interrupt 21. It exchanges
returned DX with AX and masks AX with 0080, restores BP and
returns far without incoming cleanup. It does not test returned carry or
route returned error AX through an error helper. Locally its AX result
can only be zero or 0080; its meaning still depends on the native
request and register preservation. FND-EXE-511's caller clears flag 0200
only for the zero result, regardless of native carry.

The far helper 364E saves BP, SI and DI, holds incoming record
offset SS:BP+6 in SI and quantity SS:BP+12 in DI. It
requires record word fourteen equal to SI, incoming mode at SS:BP+10
not greater than two signed, and quantity at most 7FFF unsigned.
Failure returns FFFF before later stores. Negative mode words pass this
signed guard; it is not an unsigned zero-through-two validation.

On the admitted branch it tests current DS:394E installed or DS:38C2
on disc. Zero with SI equal to diagnostic offset 3692/3606 stores
one there and skips the next marker test. Otherwise it tests current
DS:394C/38C0; zero with SI equal to base record 3682/35F6
stores one there. These markers precede later allocation failure.

If record word zero is nonzero it calls 302B with push order one,
zero, zero, record offset, removes eight argument bytes and ignores AX.
It next reloads record flags word two. Bit 0004 set passes record
word eight to 20A3, removes two argument bytes and ignores AX.
It then clears bits 0004 and 0008 in current flags, clears word
six and stores record offset plus five at word width into words eight
and ten. It does not locally clear record word zero at this point.

Mode exactly two or quantity zero returns zero after these changes, without
allocation. Other modes and nonzero quantity first store zero at DS:3678
installed or DS:35EC on disc and 3F14 at DS:3676/35EA.
Those two word stores alone do not establish how later consumers interpret
them. Nonzero incoming source offset SS:BP+8 skips allocation. Zero
passes held quantity to 2172, removes two argument bytes and stores
returned AX in that incoming frame word. Returned zero takes the FFFF
return without reversing earlier record or marker stores. Nonzero sets current
record flag 0004 before continuation.

The continuation reloads incoming source offset into AX, stores it into
record words ten and eight, and stores held quantity into word six.
Mode exactly one sets flag 0008; every other admitted mode leaves that
bit as cleared by the earlier mask. It returns zero. All local exits
restore DI, SI and BP and return far without incoming cleanup. The
record pointer and quantity rely on SI/DI preservation across callees;
native effects, callee contracts and aliases remain unadmitted.

For FND-EXE-511's diagnostic call, mode two therefore skips allocation
after resetting the fields. Mode zero with source zero and quantity 0200
reaches 2172. The caller ignores this helper's returned AX. Neither branch
locally restores a cleared failure-bypass flag from the preceding 0705 test.
Both editions share local control flow with the distinct globals above.

## Interpretation

This resolves the local return producer and record-writing helper below the
startup caller, including signed mode admission, ignored preliminary results and
failure after state changes. Q-EXE-007 retains native 4400 results and
preservation, 302B/20A3/2172 and their record/allocation contracts, actual
segments, storage extents, aliases and lifetime, the earlier startup target,
remaining writers/callers and broader launch coverage. No complete initialization
contract, allocation success or whole-game launch exclusion is claimed.

## Alternatives

Treating 0705 as a checked native success ignores its absent carry test.
Treating mode admission as unsigned ignores negative accepted words. Treating
allocation failure as leaving state unchanged ignores marker, flag and pointer
stores before 2172. Treating mode two as preserving shipped pointer fields
ignores its resets. Treating ignored preliminary AX values as successes ignores
the caller's absent result tests.

## How to reproduce

At revision 7e18301 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
5905..5916 and 884E..8926 in sixteen-bit mode. Track 0705's
AX/BX request, post-interrupt exchange and mask without a carry branch.
Follow 364E's signed mode and unsigned quantity guards, marker stores,
ignored results, resets before mode/quantity tests, allocation failure and shared
cleanup. Compare modes FFFF, zero, one, two and three, quantities zero,
7FFF and 8000, and allocator results zero/nonzero as static branches.
Keep actual segment, native and storage admission separate. Licensed bytes stay
outside Git; no game process, DOSBox or emulated call runs.
