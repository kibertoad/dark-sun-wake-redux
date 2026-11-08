---
id: FND-SOUND-103
title: The observed setup notice entry implies a wrapped frame whose upper interval intersects the earlier negative-group allocation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 190F:0000..190F:0009
tool: tools/research/exec-census/resident_listing.py; Capstone 5.0.7, xxhash 4.0.1; retained FND-SOUND-100/102 endpoint metadata
environment: null
---

## Observation

Q-SOUND-010 retains frame admission before continuing FND-SOUND-102's
identified BLASTER notice. FND-EXE-483 describes the handler frame, and
FND-SOUND-100 supplies the earlier modeled allocation extents. This checks
their intersection rather than assuming that an incoming readable return
also admits the handler's later stack. All numbers below are hexadecimal.

The bounded instruction reading confirms that190F:0000 saves BP, sets BP
from SP, subtracts1766 from word SP, then saves SI and DI. There is no
stack-segment adjustment or local wrap test in those nine bytes. The
two native guard observations in102 give SS22D0 and incoming SP0DE2,
before any of this prologue executes.

If that prologue executes from the observed register state with ordinary
sixteen-bit stack arithmetic, saved BP occupies offsets0DE0..0DE2 and
BP becomes0DE0. Subtracting1766 sets SP toF67A modulo10000; saving
SI and DI then sets SP toF676. The reserved local interval is therefore
two intervals, F67A..10000 and0000..0DE0 in SS, rather than a single
nonwrapping interval ending at BP. Including saved SI/DI extends the
upper interval toF676..10000. The physical intervals are respectively
32376..32D00 and22D00..23AE0. Saved BP remains just above the latter.
These are half-open address intervals, not observations of written locals.

At100's earlier post-label endpoint, the modeled MusicOnlyGroup record
block begins at physical25B30 and has0FF1 paragraphs, ending at35A40.
The entire upper frame interval32376..32D00 lies inside that earlier
modeled extent. Its saved SI/DI positions32376..3237A do also. This is
an exact numerical intersection with the earlier allocation model, not a
claim that the block still belongs to the group at the later guard.
The negative group's payload was never read or compared. Neither100
nor102 traces allocator lifetime, stack limits or every intervening writer.

EXE-483..486's BP-relative locals must consequently be interpreted at
word width under this entry profile. For example BP-1028 isFDB8 and
BP-1766 isF67A. A future observer cannot require BP>=1766, read a
single contiguous physical interval below BP, or silently normalize SS
while retaining these offsets. Each needed span requires its own named
binding and wrap/extent analysis. Child calls and their deeper stack use
remain outside this prologue-only interval calculation.

No original continuation, native frame write, negative payload read, input
or emulated call was performed. No heap damage, visible defect or runtime
failure is inferred from the intersection.

## Interpretation

The nonwrapping-frame alternative is ruled out for the prologue applied
to102's observed entry state. Q-SOUND-010 now requires explicit wrapped
frame admission and allocation lifetime/alias evidence, rather than an
unspecified frame check. Actual guard objects, input, helper preservation
and chooser continuation remain open. Neither named format is promoted
and this does not establish a complete handler reading.

## Alternatives

A low incoming SP is compatible with a valid four-byte return read while
the larger later frame wraps. Numerical overlap with an earlier allocation
does not establish simultaneous live ownership or an overwrite of meaningful
data. Treating a skipped negative-count payload as initialized or unused
would assume what the existing evidence explicitly did not read.

## How to reproduce

Verify SOUND_DS.EXE length204593 and XXH3-128
236c2dc23c071eca421eb5b427caee57. With the checkout-local evidence Python,
run -I tools/research/exec-census/resident_listing.py on the licensed file
for190F:0000..190F:019F. The retained report is
GAME_DIR/analysis/config-block-construction-20261108/notice-entry-frame-instructions.txt.
Read the first nine bytes' instructions, reuse EXE-483..486 and100/102,
and calculate word-width0DE2-2-1766-4 modulo10000. Add SS22D0 times10
to each resulting offset interval. Compare with25B3 times10 plus0FF1
times10. All original-derived output stays local; no original execution.
