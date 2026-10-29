---
id: FND-COMBAT-090
title: The zero-style APFM paint path returns without frame writes, while flagged styles use a working rectangle below the callback region
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0068..3F96:0097
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3CFA:00DA..3CFA:0423
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0808..3A8E:08AA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000303C3..0x000303DB
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table at 853eb28e; Capstone 5.0.9
environment: null
---

## Observation

Record offsets, addresses and flag masks below are hexadecimal.

The child loop of resident 3A8E:060D computes each 30-byte child entry
from the window's byte +F2 and index, then skips entries whose word
+1C has bit 8000 set. The four-tag scan at 0842 pairs these tags and
targets in order: APFM to 087F, BUTN to 086C, MENU to 085E and EBOX
to 088D. The APFM branch passes the child's loaded far pointer to
3F96:0068 at 0886. The common continuation removes four argument
bytes and advances the loop without testing this child's return value.

The count is four from CX and the bounded LOOP. With tag index i,
the matching BX is 08E3 + 4i and remaining CX is 4-i. The doubled-CX
addition and final +8 displacement therefore select word 08F3 + 2i.
The tag bytes occupy file 303C3..303D3 and target words 303D3..303DB.
This table was read separately from code, with the source fingerprint
and explicit count/stride through the bounded-table reader.

3F96:0068 has a stack-limit guard, then passes its frame far pointer
advanced by 0C and word three to 3CFA:00DA. Nonzero AX would map to
FFFF; zero maps to zero. The shared helper also has a stack-limit guard.
Its complete local body has one normal return tail, which sets AX zero
before restoring DI, SI and BP and returning far. Thus, on a normal
return through that tail, 0068's nonzero-result branch is not taken.
This does not guarantee that a guard or graphics child returns.

Let P be the shared helper's supplied pointer, the APFM frame plus 0C.
Its first rendering call is guarded by double word P+20 (frame+2C).
It then tests style word P+2 (frame+0E): bit 01 selects local 061F
with argument zero; bit 02 selects working-rectangle preparation and
local 061F with argument two; bit 10 selects local 046F; either bit
04 or bit 08 selects the remaining graphics-call branches. These
tests re-read the record in execution order, rather than snapshotting
one style value across external calls.

Every own write through P in this body is within P+14..P+1C, that is
frame+20..frame+28. In the bit-02 branch it copies eight bytes from
P+4 through 1000:0452 to P+14. That primitive takes CX as byte count,
copies forward, restores DS/SI/DI, clears the direction flag, and removes
its two far-pointer arguments. Here CX is eight, giving a disjoint copy
from frame+10..frame+18 to frame+20..frame+28 when storage is valid
and addresses do not wrap.

Subsequent word additions/subtractions adjust working-rectangle offsets
P+14, +16, +18 and +1A. DI and SI start at three on this branch. The
initial adjustments depend on style bits 01 and 10 and a font-height
result; another set follows the local 061F call. No own write targets
the mask or callback region at frame+58 and above. External rendering
and text children on nonzero-style paths are not bounded by this own
write footprint and remain separate dependencies.

For valid disjoint frame/stack storage, no address wrap, both guards
bypassed, frame+2C zero and frame+0E zero, no rendering or other child
call is reached after the guards. The shared helper performs no frame
store on that path and returns zero; 0068 then returns zero. FND-UI-005
records those zero fields in every shipped APFM resource, and
FND-COMBAT-088 shows the intact zero-image constructor preserves them.
This conditional local path therefore preserves the callback bytes as
well as the other frame bytes through this paint call.

## Interpretation

The rebased frame pointer in this operation does not hide an own write
to a callback field: its only frame-write range ends at 28, well below
5A and 5E. More strongly, the intact zero-default case reaches no such
write or rendering child at all. This excludes that local paint operation
as a callback-byte producer under its explicit conditions. It does not
exclude other window callbacks, other children or later writes.

The style word at frame+0E and the conditional working rectangle at
20..27 gain concrete shared-helper roles. This does not identify every
writer or assign the working rectangle's values for all live frames.

## Alternatives

FND-UI-041 retains the window's separate before/after-child callbacks
and other activation work. No absence of visible window pixels or
whole-window no-effect result follows from the zero-default child case.
FND-COMBAT-089 retains acquisition and live archive/storage conditions;
Q-COMBAT-049 retains intervening style/image changes, record identity,
aliases and other callback-field producers. A modified nonzero style can
reach children whose complete effects are not established here.

The raw table positions alone do not prove tag-to-target indexing; that
binding uses the count, remaining CX and indexed jump read above. The
zero return is the shared helper's own tail assignment, not an assumption
that every graphics child succeeds. No native rendering experiment or
complete-reading declaration is claimed.

## How to reproduce

Read 3F96:0068..3F96:0097, 3CFA:00DA..3CFA:0423 and
1000:0452..1000:046E with resident_listing.py on the licensed installed
DSUN.EXE. Enumerate the shared helper's own stores separately from
its child calls and translate all offsets through the 0C advance.
Trace the zero image/style case to the sole zero-result return tail.

Read 3A8E:0808..3A8E:08AA and use report.mjs table for two bounded
tables: file 303C3, four unsigned four-byte tags at stride four; file
303D3, four unsigned two-byte targets at stride two. Use the DSUN.EXE
size/XXH3 baseline in BLD-GOG-EN-1.1 and count evidence from 0842's
four-entry scan and 0856's index transformation. The local configurations
are GAME_DIR/analysis/apfm-life-20261029/activation-tags.json and
activation-targets.json. Record the table values and the input-to-index
derivation separately; do not disassemble the table as code.
