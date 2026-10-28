---
id: FND-COMBAT-077
title: Saved-object re-placement passes zero load flags and skips the RDFF reload branch; its final coordinate helper writes only two presentation words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000709A8..0x00070AE7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0FFA..31E0:1040
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:444A..31E0:4474
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at e3f4181e; Capstone 5.0.9
environment: null
---

## Observation

Overlay 187 has code base file 0x6FE30. After its successful saved-resource
reads, +0A6C walks object slots 0 through 319. It skips a slot whose 37-byte
record's word +1 equals -1. For another slot it retains seven fields of that
record, then calls 31E0:0EFF with the record's word +0x1B as object number,
word +1 as entry index, the loop slot, byte +0 masked with 7, and flags 0.
The flags word is pushed at file 0x70A49; the far call is at 0x70A6A.
If the call returns -1 it skips the field restoration and coordinate helper.
Otherwise it restores the seven retained fields and calls 31E0:444A with
the same record's far pointer at file 0x70AD5.

At 31E0:1000 the callee tests mask 0x04 of its flags byte. When clear it jumps
to 31E0:1040. This skips the object-number checks and the RDFF request
through 2D40:000A at 31E0:1028 (FND-ACTOR-003). Therefore the reader's
flags-zero call cannot take that particular resource-load branch.

31E0:444A writes record word +3 as word +0x0A less signed byte +7, and
word +5 as word +0x0C less word +8 less signed byte +0x0E. Both arithmetic
results are words. It returns at 31E0:4473, with no child call and no other
store. The caller supplies a presentation object record, not a combatant
or details record (FND-SAVE-022).

## Interpretation

The saved-object re-placement loop does not take 31E0:1028's fresh RDFF
load and its associated difficulty scaling (FND-COMBAT-075). Its subsequent
coordinate adjustment changes only presentation words. These local paths
cannot by themselves establish whether all later save-load children preserve
restored current and maximum hit points.

## Alternatives

Q-COMBAT-044 retains SAVE/5 and SAVE/6 admission, the remaining children
of 31E0:0EFF, the reader's other children and later load continuations,
including failure paths. A global claim that a numbered load never resets
or rescales a creature does not follow from this one skipped branch.

## How to reproduce

With the installed licensed DSUN.EXE, run overlay_listing.py for descriptor
187 and file range 709A8 70AE7. Run resident_listing.py for
31E0:0FFA..31E0:1040 and 31E0:444A..31E0:4474. Compare the pushed fifth
argument with the callee's flags test, retaining the far-call frame described
by FND-ACTOR-003. FND-SAVE-022 supplies the successful reader continuation.