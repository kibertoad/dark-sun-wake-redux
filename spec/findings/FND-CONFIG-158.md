---
id: FND-CONFIG-158
title: Three pre-setup calls restore DS and have no direct archive-state write
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2A4A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2973
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:271B
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared FBOV fixup mapping
environment: null
---

## Observation

FND-CONFIG-157's calls at file offsets 0006765C,
0006766D and 00067679 target resident entries 1BF3:2A4A,
2973 and 271B. All three segment operands are declared
FBOV fixups naming descriptor 11, mapped segment 1BF3.
FND-VIDEO-003 identifies the first two as the graphics
library's mode query and mode setter.

The complete query, `0x00013B7A..0x00013B91`, saves
DS, SI and DI, assigns CS to DS and reads word CS:295F.
When it differs from FFFF, it returns that word. When it
is FFFF, it requests the BIOS current mode with INT 10h,
AH 0F, then clears AH. Both local paths pop the saved
registers and far-return. The cached word's loaded-image
value is FFFF; its actual value at this call remains
state-dependent. This routine has no own memory store or
ordinary call. BIOS outcomes are not observed here.

The caller stores the returned word at 4E71:0001, then
calls 2973 with word 0113. The complete setter has one
return at `0x00013AE8` for argument seven and another at
`0x00013B79` for other arguments. The supplied 0113 takes
that second path. The entry saves BP, DS, SI and DI,
clears direction and sets DS to CS before storing the
full argument at CS:295F.

On the supplied path it sets DS to 0040, replaces bits
four and five of byte 0040:0010 with value 20h, and
requests BIOS mode 13h through INT 10h with AH zero.
The following unsigned comparison reads the original word
argument again; 0113 is at least 0100 and enters the
VGA register path described in FND-VIDEO-003. Its port
operations use 3C4/3C5, 3CE/3CF and 3D4/3D5. It sets
ES to A000 and writes 32768 zero words starting at
ES:0000, then completes the port operations and restores
the saved registers. ES and flags are not saved. Its own
code has no ordinary call or archive-list access. The
other argument branches were checked for exits and local
write targets, but no additional hardware behavior is
claimed from an unobserved interrupt or port response.

The entry's direction clear precedes its BIOS request.
There is no later direction clear before its repeated
store. This reading does not assign the BIOS handler's
returned flags, nor infer a later caller direction flag
from the earlier clear alone. In particular it does not
settle FND-CONFIG-154's width-prefix condition.

The third body, `0x0001384B..0x000138D8`, saves BP,
DS, SI and DI and sets DS to CS. It reads none of the
four zero words supplied by its caller and makes no call
or interrupt request. Its direct writes are in CS:

| Offset or span | Local value |
|---|---|
| 0E4C | AFFB |
| 0004 and 0006 | A000 and A400 |
| 0204 and 0206 | 1000 |
| 0404, 0406, 0604 and 0606 | zero |
| 0804 and 0806 | 319 |
| 0A04 and 0A06 | 199 |
| 0C04 and 0C06 | zero |
| 0E4E | A7E8 |
| low bytes at 0C08 through 0E02, step two | 80h, 254 positions |
| 118E and 1050 | zero |

It then restores the saved registers and far-returns.
These are local graphics-library fields; no complete
field schema or rendered-page behavior is assigned here.
The explicit CS writes lie apart from the DS archive
head, active pointer and mode fields in FND-CONFIG-037
and FND-CONFIG-040. This body does not change direction.

## Interpretation

The three own-code paths do not open, close or select an
archive, assign the archive traversal mode, or populate
FNFO buffers. Their temporary DS changes are restored by
the local returns. The third helper has no transitive
callee to account for; the first two still depend on
external BIOS and hardware effects. This narrows the
intervening-state question without proving an uninterrupted
startup or a successful resource load.

## Alternatives

Q-CONFIG-008 retains external interrupt effects and the
later pre-setup helpers, setup callees, script execution
and archive I/O. One reading leaves the registered archive
list intact through these own-code paths; another changes
it through external or later activity. The local instructions
rule out these bodies as direct archive-state producers,
but not the second reading's remaining mechanisms.
FND-CONFIG-159 reads the other pre-setup mode helper's
local branch and function-pointer store separately.

## How to reproduce

Resolve the three declared segment operands at file offsets
0006765F, 00067670 and 0006767C. Read the query through
its far return, both setter return paths including the
branch past the earlier return, and the complete 271B body.
Track DS changes and restores, CS-relative fields, the
0040 and A000 segments and the repeated-store inputs.
Compare direct destinations with the archive-state fields.
Do not equate no ordinary call with no external effect:
retain INT 10h, port operations and returned flags as
unobserved dependencies.
