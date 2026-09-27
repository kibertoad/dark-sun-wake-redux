---
id: FND-CONFIG-018
title: A WIND resource return value gates the message-delay wait
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0048
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV ranges; ReportFbovOverlayMap.ps1
environment: null
---

## Observation

The overlay 172 routine behind `566A:002A` calls the overlay 182 entry
`56BD:0048` at `DSUN.EXE+0x00059AC6` with `0x2905` (10501) as its first
argument. It stores the returned `DX:AX` far pointer at `0300:0007` at
`0x00059AD3..0x00059AD8`, then tests that pointer at `0x00059B18` before
the calls and `DS:26B7` wait described by FND-CONFIG-011. The overlay 172
cleanup routine at `0x00059B76` returns immediately when the pointer is
zero; otherwise it passes the pointer to another far routine and clears
`0300:0007` at `0x00059BD0`.

Overlay 182's entry begins at `DSUN.EXE+0x000689BB`. It calls its local
routine at `0x00068914` with the 10501 argument. That routine passes the
four-byte tag `WIND`, the ID and an output far-pointer address to a resource
call, then returns the output pointer in `DX:AX`. The entry checks that
pointer for zero. On a nonzero return it calls three further setup routines;
only if each returns zero does it return the pointer. Its failure path
returns a zero far pointer. `WIND/10501` is present in the installed
resource inventory (FND-UI-001). FND-CONFIG-030 follows the three setup
calls: callback storage always returns zero, while registration and
activation retain failure paths.

## Interpretation

The second `0300:0007` test is a success gate for acquiring and setting up
`WIND/10501`. A nonzero return reaches the message-delay wait; a zero return
skips it. The pointer is subsequently cleared by the same overlay's cleanup
path.

## Alternatives

This reading does not identify registration and activation's full effects or
prove that they succeed in a particular live state. It does not establish
whether every text-message caller uses this same resource state, what the
window looks like on screen, or whether another path changes `DS:26B7`.
The far pointer is the value returned after loading `WIND/10501`; its exact
runtime object type beyond that path is not established.

## How to reproduce

Use `tools/ghidra/ReportFbovOverlayMap.ps1` on the approved `DSUN.EXE` to
place overlay 172 code at `0x00059430` and overlay 182 code at
`0x00068850`. In the resident header of overlay 182, trampoline
`56BD:0048` selects offset `0x016B` of its code. Disassemble bounded
physical windows `0x00059AB7..0x00059B75`, `0x00059B76..0x00059BE0`,
`0x00068914..0x00068946` and `0x000689BB..0x00068A3F` in 16-bit mode.
