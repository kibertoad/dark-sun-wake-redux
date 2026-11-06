---
id: FND-SCRIPT-051
title: GPL37 entry0001 has no listed literal route to entry0A41's pre-gap postamble
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: script_reach.py at 4c5171a0 over retained script_decode.py index from FND-SOUND-077; FND-SCRIPT-043/044 controls
environment: null
---

## Observation

FND-SCRIPT-044 distinguishes GPLI entry167's pair GPL/37 offset0001
from entry168's pair GPL/37 offset0A41. FND-SCRIPT-045's MAS/1 TALK
registration for key -99 selects the latter; the same finding identifies
a separate GPL/48 registration selecting the former for key -96. This
assessment tests whether the former supplies a listed literal control-flow
route to the latter routine's postamble, rather than treating every entry
in GPL/37 as equivalent.

The retained all-script index is SHA-256
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
Its sources are GPLDATA.GFF size2191945, XXH3-128
2bd1d2b989eef2dcb0545feb95975fca, and OBJEX.GFF size6816516,
XXH3-128 e08d3cac153c547f6804ca27b155915d (FND-SOUND-077).

The committed graph reporter walks both conditional outcomes, listed
literal local calls/jumps, menu targets and literal cross-script calls,
and retains the continuation after a local call as if it returned.
It therefore overapproximates the listed paths under intact streams,
frames and returning child conditions; it does not prove conditions
feasible or native calls harmless.

Using roots GPL/37@1 and GPL/37@2625 (decimal offsets), its pre-gap
query gives these controlled results:

| Decoded site | Root0001 | Root0A41 |
| --- | --- | --- |
| 03A6 stop | reached | not reached |
| 0A41 portrait instruction | not reached | reached |
| 0AF9 stop | not reached | reached |
| 0D29 closing marker | not reached | reached |
| 0D2A local postamble call | not reached | reached |
| 0D2D and 0D32 output instructions | not reached | reached |

No unresolved-transfer diagnostic is emitted for these two roots in
this decoded graph. The positive entry0A41 controls agree independently
with FND-SCRIPT-043; the entry0001 stop and its six reported local-call
sites show that the first root was actually traversed. Its final adjacent
local routine returns at 0A40; physical adjacency is not a fallthrough
edge into 0A41.

The graph reader does not follow control transfers nested inside numeric
expressions. The GPL/37 index has three nested instruction sites: request48
calls at 0095 and 02B2, and opcode80 at 02A2. None is a script-level jump,
call, return or menu instruction. Actual native request/handler effects,
callbacks and changed frames remain outside the literal-graph exclusion.

### The rejected instruction is not a graph control

An initial opcode77 query found no hit even from root0A41. It cannot
support any absence claim: the retained decoder stops at the malformed
expression before emitting an opcode77 instruction row. Its next row is
the resumed literal target0D58. FND-COMBAT-103/109 and FND-SCRIPT-042,
not that missing row, establish the actual rejected-atom mechanism.

The replacement query uses the decoded postamble nodes above, entirely
before the omitted instruction. A graph continuation across the omitted
interval is not used as evidence for execution through it. The preserved
gap and the initial failed positive control are explicit limits, even
though the reporter itself emits no unresolved-transfer diagnostic.

## Interpretation

Converted entry167 and its known offset0001 registration are not an
alternative route to the pre-gap postamble through the listed literal
graph. Q-SCRIPT-024 can retain entry168/map lifetime and independent
computed, restored or changed entry mechanisms without treating entry167
as an unresolved literal connection to 0A41. No actual MAS/1/TALK event,
error continuation or whole-entry complete reading is established, and
the remaining queue question is not closed.

## Alternatives

A shared GPL resource does not make its distinct entry offsets equivalent.
A failed opcode77 positive control is not proof that its instruction is
unreachable. Conversely, a positive postamble graph route from 0A41 does
not establish actual frame, condition, native-child or callback histories.

## How to reproduce

On the fingerprinted retained music-health-all-scripts.txt under
GAME_DIR/analysis/handler-census-20261106/, run script_reach.py with
opcodes13,31,4F,54,67 and roots GPL/37@1 GPL/37@2625. Retain
gpl37-two-entry-pre-gap-routes.txt and require the controls in the table,
the traversed entry0001 local-call sites, and no PROBLEM diagnostics.
Select GPL/37's nested opcode markers in that same index; retain
gpl37-nested-opcode-domain.txt, requiring two22 markers and one80 marker,
then inspect their sites named above. Do not infer native-child purity.

The earlier opcode77-only report is
gpl37-two-entry-opcode77-routes.txt; it contains only the entry count,
not a successful instruction control. Preserve it as a rejected query,
alongside the decoder's GPL/37 gap notice. Reuse FND-SCRIPT-043/044 for
the independent local-route and entry-index controls. No original game
or emulated call was run and no original-derived output is committed.
