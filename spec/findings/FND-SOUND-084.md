---
id: FND-SOUND-084
title: The two alternate known sequence setup calls share the playback payload and publish a separate handle
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4654:056A..4654:09A9
tool: resident_listing.py at bacdf20e; pinned evidence Python with Capstone 5.0.9; FND-SOUND-050/069/082/083, FND-RNG-082 and FND-CONFIG-236/237
environment: null
---

## Observation

Q-CONFIG-014 asks what Preferences can obtain from music-level readback.
This connected assessment classifies both remaining known direct calls to
47B9:0002 from FND-RNG-082, reusing the existing producer, setup and getter
readings. It does not claim an exhaustive census of indirect calls or writers.
Installed DSUN.EXE has 634416 bytes and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hexadecimal.

The MZ header is 5200 bytes in hexadecimal. File call sites 0003C002 and
0003C0C8 therefore correspond to 4654:08C2 and 4654:0988 in the relocated
load image. Both belong to the dispatcher beginning at 4654:056A, rather
than separate initialization routines. FND-SOUND-050 already establishes
its table/type selection and local record bindings.

| Caller arm | Setup arguments | Result consumer |
| --- | --- | --- |
| Type 7, 08C2 | Zero-extended record byte +0E and far control block DS:3446 | Stores AX to DS:3442 at 08CA, can perform the existing parameter child, then joins 0993. |
| Type 8, 0988 | Zero-extended record byte +0E and far control block DS:3446 | Stores AX to DS:3442 at 0990, then joins 0993. |

The common continuation reloads DS:3442 and DS:343E and requests
4842:0CF4 at 099B. Neither arm directly stores its returned handle to
DS:3464, which the getter in FND-CONFIG-236 reads. Child effects and
computed/copied writers are not excluded by that local observation.

FND-SOUND-082 establishes the shared helper's full request: it supplies
current payload pointer DS:3458 separately from the incoming control block.
Neither alternate caller constructs or publishes a replacement payload
pointer in this dispatcher. FND-RNG-082 and FND-SOUND-082 identify the
allocation of DS:3446 separately from the allocation of DS:3458. Thus these
two calls do not themselves supply evidence of a distinct built-in
initialization sequence. The known song loader's third call instead passes
DS:344A and later stores DS:3440 (FND-SOUND-069/082/083).

Both alternate arms have an earlier local exclusion already identified by
FND-SOUND-050: after loading configuration into ES:BX, the type-7 arm
requires word +32 equal to zero at 0870 and equal to three at 087A. The
type-8 arm imposes the same pair at 093E and 0945. No local write or child
call occurs between either pair. With that word and binding unchanged,
neither arm reaches its setup request. This excludes both routes under
that condition; it does not establish continuous state preservation or
exclude independent entry after the checks.

## Interpretation

All three known direct setup callers now have classified argument and
result-field contracts. The two alternate sites are not two additional
unread payload producers, and their explicit result stores do not populate
the getter's handle. Q-CONFIG-014 can remove classification of those sites
from its unread work. Actual entry/configuration admission, shared payload
history, child effects and indirect or copied transfers remain unresolved.
FND-SOUND-083's installed archive restriction is unchanged; other media and
runtime-modified archive records remain outside it.

This is a bounded caller classification, not a whole reading of
RULE-CONFIG-003, a complete writer census, an observed runtime failure or
an answered deletion of Q-CONFIG-014. It establishes no audible outcome.

## Alternatives

- Each remaining direct site creates an independent initialization payload:
  both reuse the shared helper and its DS:3458 payload argument.
- These explicit stores supply the getter's handle: their destination is
  DS:3442; the getter reads DS:3464. Possible later propagation is separate.
- The contradictory local tests prove unconditional unreachability: only
  unchanged configuration/binding and entry through the checks support it.

## How to reproduce

Run the pinned evidence Python with -I and
tools/research/exec-census/resident_listing.py, passing the licensed installed
DSUN.EXE and range 4654:056A..4654:09A9. The reader validates identity and
applies MZ relocation at load segment 1000. The retained report is
GAME_DIR/analysis/alternate-sequence-setup-20261107/dispatch.txt.
Compare the call sites with FND-RNG-082's three-site list and reuse
FND-SOUND-050/069/082 and FND-CONFIG-236 for the connected interfaces.
No original code or data is committed, and no original process was run.
