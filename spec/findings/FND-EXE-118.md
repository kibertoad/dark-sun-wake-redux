---
id: FND-EXE-118
title: Shared flag counters precede a latch-setting tail insertion with no local rollback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A43D0..0x005A4462
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4360..0x005A4366
tool: Ghidra 12.1.3 PUBLIC bounded shared counter and latched tail-insertion reading
environment: null
---

## Observation

FND-EXE-113 records local byte F at current ESP plus fifteen and the
first callee's three saved registers and sixteen-byte reservation. Entry
at `0x005A43D0` zero-extends current F and ORs its low byte into O plus
`0x0125` before continuing at `0x005A43DB`. Other paths enter that
later address directly, including the current-byte-zero path in FND-EXE-116;
the earlier OR is not unconditional for every suffix entry.

At `0x005A43DB` it freshly zero-extends F. Mask four present increments
full O plus `0x012C` modulo thirty-two bits. Mask two present then
increments full O plus `0x0130`; only in that mask-two path it tests mask
two of current byte `0x0075B205`, and absence additionally increments
full O plus `0x0138`. At `0x005A4410` it freshly zero-extends F again.
Mask eight in this second value increments full `0x0128`; mask sixteen
then increments full `0x013C`. Each increment is modulo thirty-two bits,
and the order is `0x012C`, `0x0130`, optional `0x0138`, `0x0128`,
`0x013C`. F's two reads and the intervening global-byte read are separate
observations; aliases and producers remain unresolved. No mask-one counter
increment or full `0x0134` increment occurs in this bounded suffix.

Only after those updates it tests byte O plus `0x0126`. Nonzero restores
the frame at `0x005A4360` and returns EAX retaining the second
zero-extended F read; no success normalization is made. Zero publishes byte
one to `0x0126`, freshly reads full `0x010C`, and replaces the original
three argument slots with target `0x005A4BB0`, full bits `0x447A0000`
for insertion's single-precision second input, and `0x010C` OR mask eight
as full third input. It restores the local stack and all saved registers and
tail-transfers to `0x004F2490`, preserving the entry return address.

FND-EXE-105 records the insertion helper's empty-free-list normal exit.
This tail transfer has no local result test or rollback path, so the direct
latch publication precedes that helper even when it creates no node. This
statement concerns local stores and ordinary control flow; aliases, exceptional
access and other actors' mutations remain unproved. FND-EXE-109 records a
separate value-two normal-return path that clears the byte and these fields;
that does not establish their complete lifetime or meaning.

## Interpretation

The shared suffix has ordered flag-dependent counters followed by a latch
gate and one tail insertion. The gate does not guard the preceding counter
updates. Q-EXE-009 in FMT-EXE-006 still requires all suffix admissions,
field/global producers and aliases, prior flag paths and callback effects.
No complete first-callee reading, successful scheduling guarantee or actual
PATH outcome follows.

## Alternatives

- Counter updates occur even when a nonzero latch suppresses insertion.
- The `0x0125` OR belongs to the earlier entry, not every suffix admission.
- Global mask two affects `0x0138` only within the local mask-two arm.
- F is reread before the mask-eight/sixteen counters and return value.
- Setting the latch before insertion does not guarantee a node was inserted.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
FND-EXE-113 supplies the frame and local F; FND-EXE-116 independently
supplies the direct entry at `0x005A43DB`. Use the saved Ghidra program
with -noanalysis and ReportInstructionWindow.java at `0x005A43D0` limit
40, restricting observations through `0x005A4462`. The earlier
`0x005A41F0` limit-140 window in FND-EXE-113 supplies the ordinary return
at `0x005A4360`. Track both F reads, ordered full increments, conditional
global test, latch publication and original argument slots after frame
restoration. Keep reports in GAME_DIR/analysis/exe-batches; commit no original
listings or bytes and execute neither interpreter nor game.