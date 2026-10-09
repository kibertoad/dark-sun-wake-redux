---
id: FND-EXE-551
title: Game startup failure selects direct cleanup before a native termination request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0176..1000:01B0
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0176..1000:01B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:01F3..1000:0220
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:01F3..1000:0220
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:02A5..1000:02C4
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:02A5..1000:02C4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388..1000:0400
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0388..1000:0400
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The startup failure target 02AD sets CX to 001E, DX to
0056 and DS from saved CS:02C4, then calls near 02A5.
That helper sets AH to 40 and BX to two, requests interrupt 21
and returns near without cleanup. It does not locally replace CX/DX
or test native carry, count or other result. The caller then replaces
AX with three, pushes it and manufactures a far return into 03EE.
It does not inspect the write result before that next call. No diagnostic
text is retained here.

Far wrapper 03EE saves BP, pushes one, zero and its incoming
word SS:BP+6, then calls near 0388. The near helper saves
BP and SI and reads its third incoming word SS:BP+8 into
SI. In this recorded chain that word is one, so it skips the
zero-SI callback-count loop, 0163 and the stored far call that follow
that loop. Other callers can select those paths; their target/state contracts
are not established by this selected-chain reading.

The selected path manufactures a far return into 01F3, then into
0176. It tests the second incoming word SS:BP+6 for zero.
Here zero continues. It tests held SI; one skips the two further
stored far calls and pushes its first incoming word into far-returning
019E. With intact frames and callee/native preservation, that word comes
from 03EE's incoming value three. 0388 itself does not locally
change SI after loading it.

01F3 restores four vector pairs through native requests. For each pair
it pushes DS, sets AX to 2500, 2504, 2505 or
2506 respectively, loads DX and DS together from current DS far
pointer at 0074, 0078, 007C or 0080, requests interrupt
21, then pops DS. These are paired offset/segment loads, not offset-
only targets. FND-EXE-509 records the earlier native getters and stores
at those locations. Native behavior and intact stack/segment preservation are
not proved by reading the saves. The helper then returns far without cleanup.

0176 saves SI and DI, loads ES from CS:02C4, zeros
AX and SI and captures CX 002F. Its loop adds each of
47 consecutive ES bytes into AL, carries into AH, increments SI
and decrements CX. It subtracts 0D5C from the resulting word and
skips its next write request only on equality. Otherwise it sets CX
0019 and DX 002F and calls 02A5. That helper's source
uses current DS, while the preceding sum reads through ES; their equality
is not locally established. It restores DI and SI and returns far
without incoming cleanup. The numerical byte sum is bounded by 11985;
this does not admit the ES buffer or diagnostic source extent.

019E replaces BP with current SP, sets AH to 4C and
loads AL from SS:BP+4 before requesting interrupt 21. With the
recorded argument/frame binding that low byte is three. There is no local
return instruction after this request: if it returns, execution falls into
01A7, which sets CX 000E and DX 0048 and jumps to
02B3, reusing the diagnostic-and-wrapper continuation. A native nonreturn
contract is therefore required to call the observed chain unconditional termination.
The fall-through is a local alternative, not evidence that it occurs natively.

If 019E nevertheless returned to its far caller, 0388 would pop
one argument word into CX, restore SI and BP and near-return with
six-byte incoming cleanup. 03EE would restore BP and far-return without
incoming cleanup. These instructions describe the caller's continuation only;
the actual 019E body supplies no such far return. Frame integrity,
native preservation and writable aliases remain unadmitted.

Both editions share the selected-chain instructions. Their unselected zero-SI
callback paths use distinct globals, but this reading does not establish those
targets or their producers. FND-EXE-529/550 record startup paths into
02AD, including a far transfer from the first-priority target.

## Interpretation

This resolves startup failure's direct write, selected cleanup and native-request
chain without assuming the termination request cannot return. Q-EXE-007
retains native write/vector/termination contracts, actual segments and frames,
aliases and extents, other 0388 callers and stored targets, and remaining
startup dependencies. No complete general cleanup or launch exclusion is claimed.

## Alternatives

Treating failed diagnostic writing as stopping cleanup ignores absent result
tests. Treating vector restoration as offset-only ignores LDS. Treating the
checksum source as the write source ignores ES versus DS. Calling 019E
an ordinary returning far helper contradicts its absent return and fall-through.
Calling this selected path all cleanup behavior ignores the skipped zero-SI
callback paths and other callers.

## How to reproduce

At revision 81a95307 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode the
resident offsets in Locations in sixteen-bit mode. For the possible native
return continuation additionally decode 01A7..01B0, which jumps into the
recorded 02B3 body. Track 02AD's counted write and value three,
03EE's three push sources, 0388's selected flags/arguments, paired vector
loads, checksum register widths and every manufactured far return. Keep the
native nonreturn dependency separate from the caller's unreachable-without-return
cleanup. Use FND-EXE-509/529/550 for producers and incoming paths.
Licensed bytes remain outside Git; no original process, DOSBox or emulated
call runs.
