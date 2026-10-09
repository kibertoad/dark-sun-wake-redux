---
id: FND-EXE-405
title: Registration pointer calls copy without a capacity and append with signed length gates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 167B:009E..167B:00C2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:406D..1000:4096
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3DC2..2D40:3E34
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-403's first pointer call supplies destination SS:BP-0088
and source current DS:SI to 1000:406D. The full installed body
agrees with FND-CONFIG-181: it scans up to FFFF source bytes,
copies the consumed count including a reached zero, and copies FFFF
bytes if none is found. No capacity is supplied or checked. It
returns the original destination in DX:AX and restores DS, SI and
DI under an intact frame. ES remains the destination segment and
the direction flag is clear. The caller discards the outgoing eight
bytes and does not test the returned pointer.

The next call supplies that same far destination, source current DS:0230,
and limit 0040 to 2D40:3DC2. Its installed body agrees with
FND-CONFIG-182's input bindings, signed gates, word arithmetic and callees.
Call destination length D, source length L and supplied limit N=64.
Signed D at least N skips the source. Otherwise signed word D+L
below N selects the full append helper. The remaining branch computes
word C=N-D-1, copies C source bytes at destination offset plus D,
and writes zero at the original destination offset plus C-1.
It does not place that explicit zero relative to the advanced copy destination.

For disjoint valid short strings without offset or word wrap, D=0/L=64
copies 63 bytes then zeroes destination byte 62. D=3/L=61 copies
60 bytes beginning at byte three then zeroes byte 59. D=32/L=32
copies 31 bytes beginning at byte 32 then zeroes byte 30 within
the earlier prefix. D=64 skips all source processing. These are conditional
local cases, not observed registration inputs or a declared bug.

FND-CONFIG-182 records the length helper's null-pointer zero result and
FFFF-byte scan bound; absence of a terminator gives FFFE, which the
append caller treats as signed negative. Its full-append callee has separate
FFFF-byte scans rather than the limit argument. Therefore neither the signed
gate nor N=64 establishes a 64-byte write bound for arbitrary inputs.

The append helper returns the original destination pair, restores SI and DI,
and discards its local frame. Its known direct callees preserve DS with
intact frames; its body makes no local DS assignment. The caller ignores
the returned pointer. No native call is needed within these copy/append
bodies, but writable aliases can still invalidate their saved-register frames
and the caller's earlier record words. A local restore sequence alone is
not storage admission or an unconditional preservation guarantee.

The standalone copy body covers 41 bytes in 23 instructions; the
append body covers 114 bytes in 43 instructions. Their final far returns
are 4095 and 3E33 respectively. FND-EXE-403's later stores at
BP-0048 and BP-4 do not repair all potentially changed record storage.

## Interpretation

This supplies the two remaining immediate pointer-call bindings of the
registration input producer using the existing helper readings. Q-EXE-007
retains entry DS/SI provenance, valid terminated sources, destination and frame
extents, aliases, native selector-helper preservation and actual manager consumption.
The local limit does not admit all previously initialized records as unchanged.
No complete reading or runtime outcome is claimed.

## Alternatives

Inferring capacity from BP-relative spacing adds a check the first helper
does not make. Giving the append helper a conventional end-of-buffer zero
store changes its operand. Treating its signed length gate as an unsigned
capacity check discards negative scan results and word wrap. Assuming successful
copies from ignored pointer returns adds a predicate absent from the caller.

## How to reproduce

At revision f2872894 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. With header 0x5200
and load segment 0x1000, decode each Locations span in sixteen-bit
mode. Check caller pushes and eight/ten-byte cleanups, complete copy and
append spans and instruction counts, pointer returns and segment restores.
Use FND-CONFIG-181/182 for the subordinate helper bodies and relocation
admission, and FND-EXE-403/404 for the connected earlier input producers.
Substitute N=64 into the documented signed branches and check the four
listed D/L cases without executing original code. Preserve frame and alias
conditions rather than assuming them. Licensed bytes remain outside Git;
no game, DOSBox or emulated call runs.
