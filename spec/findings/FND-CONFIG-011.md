---
id: FND-CONFIG-011
title: Overlay 172 gates the message-delay wait on a nonzero far pointer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of a bounded shipped-file range
environment: null
---

## Observation

Overlay 172's far routine begins at `DSUN.EXE+0x00059A0B`. It first checks the
far pointer stored at `0300:0007`. When that pointer is zero, control jumps
to `DSUN.EXE+0x00059A65`. When it is nonzero, the routine calls through a far
pointer, processes a local 24-byte buffer, and may repeat while the word at
`DS:40CD` is nonzero. After this loop it calls several other routines and
stores a far pointer back at `0300:0007`.

At `DSUN.EXE+0x00059B18` it tests that pointer again. A zero pointer skips
to the routine's return at `0x00059B74`. A nonzero pointer leads through two
far calls, two calls with that pointer as an argument, and then the read of
`DS:26B7` at `0x00059B62`. The routine multiplies that word by 100 with
16-bit arithmetic and passes the result to the millisecond wait, as
FND-TIME-004 describes. The wait is followed by a local call before the
routine returns.

## Interpretation

The message-delay wait in this routine is conditional on the pointer at
`0300:0007` being nonzero at the second test. The ordinary Preferences arrow
branches are not the wait itself (FND-CONFIG-010).

## Alternatives

The pointer's exact runtime object type, the far calls' full effects, and
the player-visible timing have not been identified. FND-CONFIG-017 identifies
several text-message callers; FND-CONFIG-018 identifies the later test as a
`WIND/10501` acquisition and setup success gate. The earlier pointer test
alone does not determine whether the later test succeeds,
because this routine can replace the pointer between them. This reading also
does not establish whether another path caps or initializes `DS:26B7`.

## How to reproduce

Use `tools/ghidra/ReportFbovOverlayMap.ps1` on the approved `DSUN.EXE` to
confirm that file offsets `0x00059A0B` through `0x00059B75` lie in overlay
172. Disassemble only that interval in 16-bit mode. Follow the branch at
`0x00059A1D`, the pointer store at `0x00059AD8`, the branch at
`0x00059B1F`, and the wait call at `0x00059B69`. Resolve the overlay fixup
for that far call as in FND-TIME-004.
