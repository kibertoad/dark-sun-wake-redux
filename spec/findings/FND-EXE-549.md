---
id: FND-EXE-549
title: Game request publisher changes its upper bound on native failure and commits a pair on sentinel success
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:177C..1000:1831
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:177C..1000:1831
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:298E..1000:29AA
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:298E..1000:29AA
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-548's near callee 177C saves BP and SI and reads
incoming high word SS:BP+6 into SI. It increments SI, subtracts
DS:0090, adds 003F at word width and shifts right logically
by six. None of the arithmetic steps tests carry or borrow. The
result is at most 1023 independently of the original word. It compares
that result with DS:3908 installed or DS:387C on disc.

Equality skips the native request and commits the incoming pair: it reads
SS:BP+6 into AX and SS:BP+4 into DX, stores AX
at DS:00A4 and DX at DS:00A2, then returns AX one.
These ordered stores do not prove the incoming pair denotes valid storage.

Inequality shifts SI left six at word width, reads DS:00A8
into DX, forms AX as SI plus DS:0090 and compares AX
unsigned with DX. AX at most DX retains SI; otherwise SI becomes
DX minus a fresh read of DS:0090 at word width. There is
no underflow test on this clamp. It pushes SI then DS:0090
into far-returning 298E, manufacturing the far return with push CS and
a near call, and removes four argument bytes into CX.

Returned AX becomes DX. Exactly FFFF computes AX from retained
SI shifted right six, stores that value into the edition's cache word
3908/387C and joins the incoming-pair commit path. Every other
result instead adds DX to a fresh DS:0090 at word width,
stores the sum at DS:00A8, clears DS:00A6 and returns
AX zero. That failure path does not locally update the current pair
at 00A4/00A2, but does change the upper comparison pair used
by FND-EXE-548. It does not roll back native or error-helper effects.

Every local return restores SI and BP and near-returns with four-byte
incoming cleanup. Callee/native preservation, segments, aliases and accessible
storage remain conditions; the retained SI relies on them across the call.
The wrapper does not locally change DS or SS.

Far helper 298E saves BP, sets AH to 4A, loads BX
from SS:BP+8 and ES from SS:BP+6, then requests
interrupt 21. Carry clear replaces AX with FFFF. Carry set
pushes returned BX, pushes returned AX into near 06BA, then pops
the held BX into AX. FND-EXE-502 records 06BA's error
mapping and two-byte incoming cleanup; its fixed FFFF return is overwritten
by this pop. The carry-set path therefore returns the native BX word,
not the error helper's AX, while retaining that helper's shared error stores.

Both paths restore BP and return far without incoming cleanup. There is
no local distinction if a carry-set native BX happens to be FFFF:
177C still takes its sentinel-success branch. No native BX range,
segment/register preservation or storage effect is established by these local
instructions. ES is assigned before the interrupt and not restored locally.

Adjacent near caller 17F2 saves BP and compares its incoming pair
with DS:00A0/009E and DS:00A8/00A6 using 07F0.
The candidate is reloaded for the second comparison. Carry below the
first or unsigned above the second returns FFFF. Otherwise it passes
incoming high then low into 177C; its four-byte cleanup is supplied
by that callee. Nonzero returned AX becomes zero here, while zero
becomes FFFF. It restores BP and returns near without incoming cleanup.
FND-EXE-548 records the comparison helper's normalization and wrapping;
this adjacent caller adds no independent storage admission.

Both editions share local control flow; only the cache word offset differs.
FND-EXE-548's 1831 caller treats zero 177C result as failure,
and on nonzero returns its captured prior pair rather than the new pair
just stored here. Its four outgoing argument bytes are consumed by 177C,
before the later SP reset; they are not deferred cleanup on normal return.

## Interpretation

This resolves the remaining immediate request publisher and its native wrapper,
including a concrete writer of the current and upper pairs. Q-EXE-007
retains original shared-pair/cache producers, actual segments and extents, native
AH=4A effects and register contracts, aliases and lifetime, other callers
and remaining startup dependencies. No complete backing-storage or launch
contract is claimed.

## Alternatives

Treating failure as state-preserving ignores the upper-pair and error stores.
Treating 298E's failure return as the mapped error ignores the held
native BX pop. Treating FFFF as unconditional native success ignores
the absence of a carry-set BX range contract. Calling the cache comparison
proof of valid storage ignores wrapped arithmetic and absent admission.
Treating 1831's outgoing words as surviving the call ignores RET 4.

## How to reproduce

At revision 303b9fc require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
697C..6A31 and 7B8E..7BAA in sixteen-bit mode. Track wrapped
quantity formation, cache equality, clamp reloads, manufactured far return,
native carry branches, held BX versus error-helper AX, every shared store
and both callers' return conversion and cleanup. Use FND-EXE-502 for
06BA and FND-EXE-548 for 07F0 and 1831. Keep native
contracts and storage admission explicit. Licensed bytes remain outside Git;
no original process, DOSBox or emulated call runs.
