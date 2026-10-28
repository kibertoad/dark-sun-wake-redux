---
id: FND-COMBAT-078
title: The three explicit temporary-slot presentation references are loader copy and clear operations, which do not exclude indexed or restored writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:10CD..31E0:1112
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1241..31E0:1253
tool: tools/research/exec-census/immediate_search.py and resident_listing.py at 2be8d56a; Capstone 5.0.9
environment: null
---

## Observation

The 37-byte presentation record for slot 319 begins at DS:95D6:
0x67BB + 319 * 37 = 0x95D6 (FND-ACTOR-003). The immediate/displacement
candidate search for 95D6 returns exactly three candidates, displayed as
32ED:0004, 32F0:0007 and 3304:0008. Their linear load-image addresses equal
31E0:10D4, 31E0:1107 and 31E0:1248 respectively. All three are instruction
starts in the previously read loader 31E0:0EFF, not additional functions.

At 31E0:10D4 the temporary record is the destination of a 37-byte fill
with 0xFF, on the allocation-failure path. At 31E0:1107 it is the source
of a 37-byte copy into the allocated object's presentation record. At
31E0:1248 it is again the destination of a 37-byte fill with 0xFF on the
normal completion path. Thus the two fills leave the temporary record's
entry-index word +1 at -1; the copy reads it. The known fill and copy sites
from FND-PARTY-119's loader path provide independent positive controls
for the searched immediate operand form.

## Interpretation

These three explicit references account for the observed fixed-address copy
and clearing of the temporary presentation record. They do not establish
that a numbered saved game can never contain a populated slot 319.

## Alternatives

The search decodes every byte and initially returns candidates; the three
returned sites were checked in the loader context. Its scope covers encoded
word immediates and displacements in the resident image and FBOV code. It
does not cover computed or indexed record addresses, bulk table copies,
saved-table restoration, indirect pointer aliases, or arbitrary data bytes
as instructions. No absence claim for those mechanisms follows. In
particular, FND-SAVE-022 restores the presentation table before examining
its slots. Q-COMBAT-045 retains the admission question and its consequences.

## How to reproduce

With the installed licensed DSUN.EXE, run immediate_search.py with word
95D6. Read resident_listing.py ranges 31E0:10CD..31E0:1112 and
31E0:1241..31E0:1253 and compare the candidate addresses as linear MZ
addresses. These are bounded static reads, not a run of the original.