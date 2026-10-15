---
id: FND-RNG-021
title: Second clock seed caller precedes a conditional random port-offset loop
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
tool: tools/evidence resident and FBOV bounded readers; tools/research direct_callers.py; report.mjs table
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BDC0..0x0008BF21
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF21..0x0008BF29
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004F9F7..0x0004F9FF
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004FA00..0x0004FA08
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E8B0..0x0008E948
---

## Observation

The trampoline 5787:006B reaches overlay 204 +0000. This is the
second seed caller recovered by FND-RNG-009, rather than the adjacent
request dispatcher. Its three word arguments are magnitude, mode and
signed iteration count. Before checking either mode or count it calls
the clock wrapper with optional output zero and supplies AX to the seed
setter at overlay +001E. Thus a nonpositive count still reseeds.

It initializes horizontal and vertical offset words to zero, a selector
word to zero and a polarity byte to one. A geometry helper receives
320 and 200. A signed index/count comparison controls the loop; positive
counts through 32767 admit that many iterations on normal helper returns.
Modes 1 through 4 select four local branches. Other mode words retain
the common offset and delay calls without a local vector update.

Mode 4 draws directly from the generator at overlay +00E1 only when
both retained offsets are zero. Otherwise it clears both and skips the
draw. The nonnegative generator result is scaled by eight and divided
by 32768 using signed wide arithmetic, selecting one of eight vectors.
The shipped horizontal vector is 0, 1, 1, 1, 0, -1, -1, -1; the vertical
vector is -1, -1, 0, 1, 1, 1, 0, -1. Each component is multiplied by
the magnitude, retaining the low word. A negative horizontal product
becomes 320 minus magnitude and decrements the vertical word. Remaining
signed negative offsets are clamped to zero.

Consequently the draw schedule depends on the resulting offsets, not
just iteration parity. With magnitude zero every mode-4 iteration can
draw. Even a positive magnitude can leave both offsets zero after the
first vector is clamped. No unconditional one-draw-per-two-iterations
contract follows from this body.

Modes 1 and 2 alternate their retained vertical or horizontal offset
and zero; their polarity byte selects the magnitude or its opposite
case. Mode 3 advances a selector modulo eight when both offsets are
zero, using the same vector products, and otherwise clears them. These
branches contain no direct generator call.

Every admitted iteration passes the two offsets to overlay 205 +0000,
then calls resident delay 1000:12FA with word 50. After the loop a final
zero-offset call precedes return. The geometry helper at overlay 205
+005E sets ES to 0040 and updates geometry fields. The offset helper
reads its port base through that ES, polls status bit 08 and writes
display registers under interrupt masking. These are hardware-dependent
operations: this static reading establishes neither their eventual
return nor pixels, visible motion or elapsed delay units.

## Interpretation

This answers the local second-caller role and conditional direct draw
site. It does not establish its admitted callers, DS/SS buffer identity,
all seed writers, hardware behavior or a gameplay rule. FND-RNG-022
separates the recovered script wrapper and direct caller argument widths.
No native run, emulated call, rule promotion or implementation is made.

## Alternatives

An iteration-only draw schedule conflicts with the zero-offset guard.
A visible-motion interpretation still needs hardware and presentation
evidence; port writes alone do not establish it.

## How to reproduce

Use the project evidence Python and licensed DSUN.EXE, XXH3
e296af55ba2ecde7e77f555c90f33d0b. Resolve 5787:006B through the FBOV
map, read overlay 204 +0000 through +0161, and qualify the clock, seed
and generator calls against their resident entries. Read overlay 205
+0000 through +0098 for the geometry and offset helper boundaries.
Use report.mjs table with count/limit four, stride two and an unsigned
word field at file 0x8BF21 for the mode targets. Read each vector with
count/limit eight, stride one and a signed byte field, starting at
0x4F9F7 and 0x4FA00. Keep query files and derived reports in GAME_DIR.
