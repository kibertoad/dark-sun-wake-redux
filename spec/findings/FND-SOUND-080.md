---
id: FND-SOUND-080
title: Local-variable backup uses one fixed snapshot and GPL21 contains no explicit nested backup or cross-script call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DD05..0x0008DDE9
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: overlay_listing.py at 01976df8; retained script_decode.py reports from 211ea53c; FND-CONFIG-137/182 copy interface; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

This connected save/call/restore assessment follows FND-SOUND-079's
GPL/18 health-input route. Installed DSUN.EXE is 634416 bytes,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Script identities and
retained decoder gaps are those in FND-SOUND-077. The whole GPLDATA
location names the inspected resource census, not a native function.

### Fixed snapshot and current pointer binding

FND-SCRIPT-026 locates request 29 at overlay 204 +1F45 and request 30
at +1FB7. Reading both complete helpers confirms four unconditional
calls to the existing copy service in this order:

| Local block pointer in 4C13 | Count | Fixed snapshot interval in 4E71 |
|---|---|---|
| 033B, flags | 9 | 0244..024D |
| 0337, numbers | 64 | 0204..0244 |
| 0333, big numbers | 40 | 01DC..0204 |
| 032F, strings | 420 | 0038..01DC |

Intervals are half-open. Together they cover 533 bytes, 0038..024D.
Request 29 copies from each current variable pointer to the fixed area;
request 30 copies from that area to each current variable pointer. Each
pointer is loaded at its own call. Neither helper saves or restores the
pointer values themselves, allocates another snapshot, indexes a depth,
checks pointer nullness, or tests a copy-success return. They have no
other child calls. The helper epilogues do not normalize a separate
success flag.

FND-CONFIG-137/182 read 1000:3F5A's whole copy body: it copies the
supplied byte count forward, restores DS/SI/DI, leaves ES changed and
direction clear, and provides no capacity or general overlap guarantee.
Thus valid nonwrapping source/destination intervals, intact frames and
physically independent backing remain prerequisites. The absence of a
copy error result is not proof those prerequisites hold.

Kind-two local number nine occupies bytes 18..20 decimal of its
64-byte block under RULE-SCRIPT-004's word layout. Its snapshot is
therefore 4E71:0216..0218. Successful restoration of its saved bits
requires that interval to retain the desired snapshot and the current
0337 pointer to designate the intended writable local-number block.
A changed pointer makes the helper copy to the new block; the helper
does not recover the old address. Cross-copy aliasing can also change
later pointers or snapshot bytes and is not dismissed.

For stable independent ordinary buffers, two successive request-29 calls
saving different local states A then B, followed by request 30, copy B
back. There is no snapshot stack to recover A. This is a synthetic
composition of the read helper contracts, not an observed nested call.
The snapshot does not include 4C0E's local-clear flag or interpreter
current-script metadata (FND-SCRIPT-026).

### Explicit GPL21 instructions do not overwrite that snapshot

The two GPL/18 sequences identified by FND-SOUND-079 call GPL/21 at
0001 and 0315 between requests 29 and 30. Inspection of all decoded
GPL/21 instructions, including nested expressions and its local-call
bodies, finds zero opcode-14 cross-script calls and 29 opcode-22 function
requests. All 29 function selectors are literal: 5, 9, 12, 15, 18, 21,
22, 33, 34, 37 or 38. None is request 29 or 30. GPL/21 has no decoder
stop; FND-SOUND-077's only known undecoded interval remains in GPL/37.
The resource-wide scan is broader than either particular entry's reach.

The independently retained GPL/18 request-29 at 0752, cross-script call
at 075B and request-30 at 0760 are positive controls for those searched
forms. The full report's dynamic function selector is the separate
GPL/27 site recorded in FND-SOUND-077, not an unclassified GPL/21 selector.
This excludes an explicit nested local backup or direct script call in
GPL/21's decoded script text. Native instructions can call services,
register work or reach callbacks; their effects, non-script callers,
changed scripts, restored frames and writes by other mechanisms remain
outside this syntactic exclusion.

## Interpretation

The save/restore interface is now finite: one fixed snapshot, four current
pointer loads per operation, forward copies and explicit backing/lifetime
conditions. For the carried GPL/18 health value, Q-SOUND-004 need not
assume an unexamined explicit request-29 instruction in GPL/21. It retains
actual snapshot/0337 backing and other writer/callback effects, successful
return to the restore, and the other entry/scheduling obligations already
recorded. RULE-SCRIPT-005 remains supported, without a whole-entry complete
reading or a guarantee that this snapshot survives every native call.

## Alternatives

A nesting-safe local backup is contradicted by the fixed destinations and
absence of a depth slot. Automatic pointer restoration is contradicted by
the current pointer loads. A GPL/21 explicit nested request 29 is absent
within the decoded resource scope and controlled instruction forms; a
universal no-write conclusion would omit native and indirect mechanisms.

## How to reproduce

Run tools/research/exec-census/overlay_listing.py at 01976df8 on the
installed DSUN.EXE, descriptor 204 file 8DD05..8DDE9. Retain
music-health-local-backup-helpers.txt under
GAME_DIR/analysis/handler-census-20261106/. Reuse FND-CONFIG-137/182's
copy-body evidence and FND-SCRIPT-026's dispatcher and pointer bindings.

Reuse music-health-all-scripts.txt and music-health-script-sweep.txt
from FND-SOUND-077, regenerated by committed script_decode.py at 211ea53c.
Select GPL/21 lines, then match every decoded position, including nested
ones, for opcode 14 and opcode 22's first parameter. Count the 22 requests
and retain their full literal-selector set and the no-stop result. Verify
the GPL/18 control sequence separately. No original game or emulated call
supplied new evidence.
