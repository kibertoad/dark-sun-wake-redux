---
id: FND-EXE-548
title: Game common request checks normalized pairs before returning a captured prior pair
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1831..1000:18BC
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:1831..1000:18BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0562..1000:0583
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0562..1000:0583
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:060B..1000:066B
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:060B..1000:066B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07F0..1000:0811
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:07F0..1000:0811
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-547's common near callee 1831 saves BP and reserves
eight local bytes. It reads DS:00A4 into AX, zeros DX and
calls 0562 with CL four. That helper converts the near return into
a far return by replacing the popped return offset with CS and that
offset. For this fixed count it returns the two-word input shifted left
four, modifies BX and CL, and makes no memory accesses except its
stack operations. Other callers' count contracts are not established here.

1831 adds DS:00A2 into AX with carry into DX, then adds
incoming low SS:BP+4 with carry plus incoming high SS:BP+6
into DX. It compares DX signed with 000F: below continues,
above rejects. Equality compares AX unsigned with FFFF and continues
when at most that word, which every AX value satisfies. There is no
separate final carry rejection. Thus this is not an unconditional unsigned
nonoverflowing twenty-bit bound check; negative signed DX passes locally.

Continuation freshly reads DS:00A4 into DX and DS:00A2 into
AX, loads incoming high/low into CX/BX and calls 060B. This
helper also manufactures a far return from its near call, leaving ES
holding the original return offset. It tests CX signed. Nonnegative adds
BX into AX, adds 1000 into DX on low-word carry, then adds
the low byte of CX shifted left four into DH at byte width.
It adds resulting AX shifted right four into DX and returns AX
masked to its low nibble. All segment arithmetic wraps at word width.

Negative CX first complements BX and CX, adds one into BX with
carry into CX, then enters the shared subtraction path at 064D.
That path subtracts BX from AX, subtracts 1000 from DX on
borrow, forms BX as the low byte of CX shifted left four at byte
width in BH with BL zero, and subtracts that BX from DX. It then
adds residual AX shifted right four into DX and returns AX's low
nibble. The adjacent entry at 063A selects the same paths with opposite
initial sign dispatch; it is not called by the bounded wrapper. Neither
arithmetic path calls another helper or changes DS/SS. Actual pointer
identity and accessible storage do not follow from the normalized numbers.

1831 saves the returned DX/AX at SS:BP-2/-4. It loads
CX/BX from DS:00A0/009E, calls near 07F0 and rejects
carry set. It then loads CX/BX from DS:00A8/00A6,
reloads the saved candidate DX/AX, calls 07F0 again and rejects
unsigned above. Both stored comparison pairs are read after candidate formation.

07F0 holds incoming CX on the stack and normalizes DX:AX by
adding AX shifted right four into DX and retaining AX's low nibble.
It similarly adds BX shifted right four into restored incoming CX and
retains BX's low nibble. Both additions wrap at word width. It compares
DX with CX, and only on equality compares the two nibble offsets.
Its final comparison flags therefore determine the wrapper's unsigned branches.
It returns near without cleanup, changes AX/BX/CX/DX locally and
does not alter segments or call other helpers. These comparisons do not
establish ordering of unbounded linear addresses across segment wrap.

After both comparisons pass, 1831 freshly captures DS:00A4 at
SS:BP-6 and DS:00A2 at SS:BP-8. It pushes the
candidate high then low words into near 177C and tests returned AX.
Zero joins rejection. Nonzero returns the captured prior pair with DX
from BP-6 and AX from BP-8; it does not return 177C's
result or reload the shared pair afterward. Every rejection returns
DX:AX FFFF:FFFF. All paths reset SP from BP, dropping
locals and any remaining outgoing words, restore BP and return near without
incoming cleanup. 177C's own cleanup and state effects remain unread.

The wrapper does not save SI/DI or ES locally and contains no
interrupt itself. Nested 177C behavior, DS/frame preservation, shared-word
producers, aliases and storage extent remain unadmitted. Both editions share
identical instructions in the cited bodies. FND-EXE-547 traces how its
callers test AX only, discard selected preliminary results, retain earlier DX
through padding and publish headers; those callers do not establish the
request's backing-storage contract.

## Interpretation

This resolves the common request wrapper's local arithmetic, comparison flags,
capture order and pair return. Q-EXE-007 retains 177C, shared pairs
and their producers, actual segments, units/extents, aliases and lifetime,
other callers and remaining startup/native dependencies. No complete allocation
or launch contract is claimed.

## Alternatives

Calling the first bound unsigned ignores signed DX branches. Treating the
returned pair as the new candidate ignores the captured prior pair.
Treating comparisons as unlimited linear-address checks ignores word wrapping.
Assuming ES preserved ignores the arithmetic helper's manufactured far return.
Calling failure transactional ignores 177C's unresolved effects and discarded
preliminary results in the recorded callers.

## How to reproduce

At revision 99fced7 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode the
resident offsets in Locations in sixteen-bit mode. Track near-to-far return
conversion, fixed CL four, low/high arithmetic widths, signed branches,
segment-normalization wrap and final comparison flags. Track every local word
and outgoing argument from wrapper entry through SP reset, distinguishing
the candidate from the later captured prior pair. Use FND-EXE-547 for
result disposal and publication. Keep 177C and storage admission explicit.
Licensed bytes remain outside Git; no original process, DOSBox or emulated
call runs.
