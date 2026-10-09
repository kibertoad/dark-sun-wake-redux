---
id: FND-EXE-290
title: Buffer setup candidate reaches hook setup after publishing shared state
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4464:014E..4464:01F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0002F083..0x0002F088
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-INPUT-005 independently names entry 4464:014E. With the MZ load
segment 1000, that entry maps to shipped 0x0003998E. Decoding from it
through exclusive end 0x00039A33 reaches FND-EXE-289's setup-call
candidate on an ordinary instruction path. The relocated segment immediate
at 0x00039997 supplies DS=57E0.

The procedure saves BP, DS, ES, BX, CX and DX. If word DS:33CC is one,
it writes 0802 to word DS:33BE and returns AX=FFFF. Otherwise it takes
the size word from SS:BP+0A. A zero size writes 000D to DS:33BE and
returns AX=FFFF. It loads the supplied far pointer from SS:BP+06 into
ES:BX and copies ES to DX. Either nonzero pointer word bypasses allocation.

For a zero pointer, a temporary BP frame replaces the pushed AX word
with zero; the subsequent push of CX supplies the other outgoing word.
The relocated call at 4464:0199 reaches 1000:15A5, with low request CX
and high request zero. It removes four argument bytes and copies returned
AX to BX. It writes one to word DS:33CE, then tests BX and DX separately.
Only when both are zero does it clear DS:33CE, write 0008 to DS:33BE,
and return AX=FFFF. These post-call DS and CX values are not reloaded.

The shared continuation stores CX to DS:33C0 and DS:33C2, DX to
DS:33CA, BX to DS:33C8, and one to DS:33CC, in that order. The
supplied-pointer arm does not write DS:33CE. It calls 44B6:0018
(FND-EXE-288), then 44D0:0006. The second call's segment operand at
shipped 0x00039A22 resolves to 44D0:0006. Only the second result is
tested: AX=FFFF is returned unchanged, and every other AX becomes zero.
There is no local rollback of the preceding shared publications. The
saved DX, CX, BX, ES, DS and BP are restored before the far return.

A controlled incoming query for shipped 0x0003998E returned one relocated
call-byte candidate at 0x0002F083, with segment operand 0x0002F086
resolving to 4464:014E. Limit 100 did not truncate it and no candidate
was unresolved. The independent MZ control 0x00040DA9 resolved to
1000:15A5. This query does not verify the candidate's instruction path.

## Interpretation

The earlier ungrounded decode is replaced by a path from the entry named
in FND-INPUT-005; this connects the local setup body to FND-EXE-288.
It does not prove native entry admission or every caller. Q-EXE-010 still
needs the incoming candidate's path and arguments, allocation-call DS/CX
preservation, the second setup callee, state writers and admitted lifetime.
FND-EXE-272's allocator restores DS through a shared CS-relative slot;
that local suffix alone does not settle its intervening-writer obligations.
In particular, the caller's original size cannot automatically be attributed
to the shared stores after allocation, and the unchanged allocation flag on
the supplied-pointer arm cannot be assumed clear on repeated entry.

The query excludes near calls, computed calls, unrelocated pointers and
instruction-boundary verification. The positive control covers MZ-relocated
calls, not FBOV fixups. No caller-absence claim, code-range addition or
complete-reading promotion follows.

## Alternatives

Assuming a zero pointer always produces usable storage ignores the explicit
two-word zero-result test. Assuming supplied storage resets ownership ignores
the absent flag store on that arm. Assuming an error leaves state untouched
ignores publication before the final call. Assuming CX is the incoming size
after allocation requires a callee-preservation reading not supplied here.

## How to reproduce

At revision 610cef9 require the installed DSUN.EXE identity recorded in
FND-EXE-176, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
Decode shipped half-open 0x0003998E..0x00039A33 with locked Capstone
5.0.7 in sixteen-bit x86 mode, initial IP 014E and modeled CS 4464.
Run the committed operand reporter with loadSegment 1000 at sites
0x00039997/targetOffset 0000, 0x000399DC/15A5 and
0x00039A22/0006. Run the incoming reporter with the same loadSegment,
target 0x0003998E, limit 100 and controls [0x00040DA9]. Source bytes
and reports remain outside Git; no original execution is involved.
