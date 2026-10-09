---
id: FND-EXE-287
title: Cleanup nested wrappers preserve selected registers around interrupt requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45B9:0006..45B9:0018
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:046D..44DE:0482
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-286's first nested target resolves through the declared MZ
operand at shipped 0x00039F4D to 45B9:0006. That body saves BX,
CX and DX, sets AX to zero and reaches INT 33h. On ordinary continuation
it tests returned AX. Zero is retained; nonzero is replaced by the
post-interrupt BX. It restores saved DX, CX and BX and far-returns.
It does not locally save or restore DS, ES or flags. The outer callee
subsequently sets AX to zero independently of this wrapper's result.

The second nested target resolves through declared MZ operand
0x00039E01 to 44DE:046D. It establishes BP, saves DS, AX and DX,
sets AH to 0x25 and loads AL from SS-relative BP plus six. It loads
DX and DS from the far pair at SS-relative BP plus eight and ten,
then reaches INT 21h. On ordinary continuation it restores saved DX,
AX and DS, restores BP and far-returns without removing incoming
argument bytes. It makes no local carry/status test after the interrupt.

FND-EXE-286's corrected first argument is word nine, so this call supplies
AL nine while AH is 0x25. Its following words remain the values pushed
from CS-relative 0x000C and 0x000E, selecting the outgoing DX and DS
respectively. The caller removes those three argument words separately.
The caller's DS-loading immediate at shipped 0x00039DD8 resolves to
57E0:0000 through its declared MZ relocation. Its saved incoming DS
and this temporarily selected data segment are distinct roles.

## Interpretation

The second wrapper's ordinary stack restoration hides the interrupt's
AX/DX/DS results from its caller. The first wrapper preserves selected
general registers but has no equivalent local DS restoration. Thus the
second cleanup callee restores the DS it received after the first nested
interrupt path, not a proven copy of the original outer data segment.
Local register restoration also does not undo external interrupt effects.

Q-EXE-001 and Q-EXE-010 retain interrupt returns and effects, pointer-word
writers, admitted target identity, DS preservation across INT 33h,
physical aliases and lifecycle. The code establishes requests and ordinary
continuations, not installed services, changed vectors or completed cleanup.
No complete reading or original execution is claimed.

## Alternatives

Treating the second wrapper's restored AX as an interrupt status ignores
its saved-register pop. Treating a zero outer result as proof of nested
success ignores the outer overwrite. Treating the CS-relative pointer
words as the first service-selector argument repeats the stack-position
error corrected in FND-EXE-286. Register saves do not prove harmless
interrupt effects or absence of asynchronous changes.

## How to reproduce

At revision 04534e2, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. Run the
committed operand reporter with loadSegment 0x1000 and site/targetOffset
pairs (0x00039F4D, 0x0006), (0x00039E01, 0x046D) and
(0x00039DD8, zero). With locked Capstone 5.0.7 in sixteen-bit x86
mode, decode shipped half-open ranges 0x0003AD96..0x0003ADA8 and
0x0003A44D..0x0003A462 at initial IPs 0x0006 and 0x046D.
Follow each saved register, argument read, interrupt boundary and ordinary
return; compare the last argument writers in FND-EXE-286. Keep interrupt
effects and pointer contents unresolved. No complete caller or writer
search is claimed; source bytes, queries and reports stay outside Git.
