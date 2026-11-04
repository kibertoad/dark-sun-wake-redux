---
id: FND-ITEM-051
title: Six encoded placement calls separate five message-disabled queries from one message-enabled action prefix
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00075560..0x00075A62
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000762AB..0x00076440
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000768C4..0x00076A49
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00075C9B..0x00075CAF
tool: tools/research/exec-census/direct_callers.py and overlay_listing.py; tools/evidence/report.mjs table at ee0e1ce3; Python 3.12.10, Capstone module 5.0.7
environment: null
---

## Observation

Q-ITEM-003 retains actual caller/position admission after FND-ITEM-048/049/050.
This reads all six encoded call candidates returned for overlay 189 +2C6B,
with complete local enumeration/frame-consumer bodies and bounded action
prefixes. It does not claim an exhaustive native incoming census.

### Encoded candidates and argument bindings

The fingerprinted direct_callers.py query for 189+2C6B returns these six near
calls, each after push CS. Inspecting their argument pushes gives the
following table; offsets and event keys are hexadecimal.

| Call | Caller entry | Logical position L | Message-enable byte |
|---|---|---|---:|
| +0D8B | +0BB0 | Identifier minus 2BCC, low word | 0 |
| +0F99 | +0BB0 | Same caller transformation | 0 |
| +107F | +0BB0 | Same caller transformation | 0 |
| +19F4 | +1991 | Loop positions 4..29 | 0 |
| +1A4A | +1991 | Loop index 0..5 plus 36 or 42 | 0 |
| +2005 | +1F14 | Identifier minus 2BCC, low word | 1 |

Each pushes message, position and item-index words before push CS/call,
matching the far-return frame of FND-ITEM-048. The item argument is reread
from graph-entry word +4 at 5072:0040 plus ten times current DS:1A34 for
every call. The action prefixes require DS:1A34 other than FFFF but do not
independently bound it against a graph count at these call sites. A message
byte of zero does not make the getter/traversal/copy route read-only.

As independent call controls, the separate query for 189+2B64 includes the
known +30D5 call from FND-ITEM-048 and the far calls 184 +2239 and 213 +131F
of FND-EXE-610/615. All six placement candidates are actual instructions in
the inspected entry/dispatch prefixes; the raw search by itself is not that
instruction-boundary proof. It searches encoded direct calls, not computed
segments, stored handlers, jumps or changed code. It also names trampoline
5713:0025 for the placement entry; no universal incoming absence is inferred.

### Enumeration and immediate frame consumers

The complete +1991 body first constructs the DS:1A32 graph at 5072:0040.
Starting at index zero, it scans signed indices below the returned count
for the first entry whose kind word +2 is one, then stores that index in
DS:1A34. If none is found it still stores the stopping index; there is no
local missing-item exit before later indexed reads. It then calls +1688
with current character 4E71:0B44, the projection of FND-PARTY-217.

The first loop queries 26 logical positions, four through 29 inclusive.
Nonzero placement AL calls +18FB; zero calls +1946. Both consumers request
coordinates through +30E2 (FND-PARTY-222), passing the same local output
address twice for the last two rectangle words. For positions at least
four, +18FB requests frame four and +1946 frame five through 2D40:3BEC,
using the image pointer 4E71:002F, the returned x/y words, page one and
fourth argument zero. FND-COMBAT-038 binds that final word to frame index.
No colour, actual pixels or successful rendering is inferred here.

If either DS:6187 or DS:6188 is nonzero after the main loop, a second loop
queries six positions. Each iteration rereads DS:6187: nonzero chooses
base 36, zero base 42. With stable flags, the first flag therefore has
priority when both are set. A successful query calls +18FB and a failed
query +1946, again selecting frame four or five. Each drawing branch
rereads the base flag after the placement query; changed flags can make
the drawn position differ from the position just queried. Do not assume
a stable container mode merely because the source loop has six iterations.

These iteration bounds assume the returning callees and intact saved
frames/registers. The placement gate and immediate consumers save/restore
SI, which holds this caller's loop index. Storage aliases or nonreturning
children remain outside this local loop contract. Each query rereads the
source entry; even an unchanged loop index does not prove the same selected
item survives every query and frame request.

### Action-prefix dispatch and position origin

+0BB0 forms L from its identifier argument before requesting its rectangle.
Its complete five-key table maps 02 to +0BFF, 04 to +0E18, 20 to +0FD1,
40 to +0FC9 and 80 to +10B2. No key match exits. The bounded prefixes for
the first three keys contain +0D8B, +0F99 and +107F respectively, subject
to their geometry/window/selection gates and intervening calls.

The immediate nonzero/zero result consumers request frames 8/7 for +0D8B,
4/5 for +0F99 and 8/7 for +107F. The first two use overlay 190's drawing
wrapper; the third calls 2D40:3BEC. These requests do not prove colour,
window acquisition, clipping or actual pixels. The remainder of the action
handler and its other dispatch branches are not completely read here.

The +1F14 prefix independently forms the same L from its own identifier,
projects the current character and may close/update container state before
+2005. A FFFF source index exits before that call. Its nonzero AL continues
to +209C; zero takes the inspected rejection drawing/call continuation.
Its complete transfer and later effects remain outside this reading.

FND-UI-063 independently records +0BB0's container registrations through
5713:0052, establishing a stored-handler route missed by direct-call search.
Identifiers 11248..11253 produce L=36..41, and 11254..11259 produce L=42..47
under the caller transformation. Actual dispatch/registration retention and
other identifier producers remain prerequisites. No local general upper or
lower bound on the identifier is added by the transformation itself.

## Interpretation

This removes the local message-mode and logical-position producer ambiguity
for the six encoded sites. The complete enumeration path and three bounded
action prefixes distinguish repeated message-disabled queries and their
frame selection from the message-enabled +1F14 continuation. The conditional
registration composition admits both container logical ranges at the source
level without asserting an observed native event.

Q-ITEM-003 retains incoming event/identifier admission, source selection and
current-character stability, complete action/transfer and snapshot/removal
children, actual graph/near-state/descriptor/width/sample lifetime and aliases.
Other incoming mechanisms remain visible. This does not close item mutation
coverage, prove every slot is usable, or promote parity/complete readings.

## Alternatives

Treating every call as a placement mutation loses the immediate frame-only
consumers and ignored transfer continuation. Treating message-disabled calls
as side-effect-free loses the gate's getter/traversal effects. Caching the
source item or container flag across calls adds preservation not established
by these bodies. A direct-call search cannot exclude the independently known
stored-handler registration route.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run direct_callers.py for 189+2C6B and
189+2B64. Read descriptor 189 file ranges 75560..75A62, 762AB..76341,
76341..76440 and 768C4..76A49 with overlay_listing.py. Resolve the action
table with report.mjs table at 75C9B hex, one 20-byte row: five two-byte
keys at offsets 0,2,4,6,8 and five two-byte targets at 10,12,14,16,18.
The count comes from the entry's CX=5 scan, not a guessed event vocabulary.

Private reports placement-entry-callers.txt, placement-highlight-caller.txt,
placement-highlight-children.txt, placement-action-prefix.txt,
placement-transfer-caller.txt and placement-action-dispatch.json, with its
-controls.json configuration, remain under
GAME_DIR/analysis/item-removal-20261104/. placement-caller-incoming.txt is a
candidate report only; its extra incoming bodies were not accepted here.
Reuse FND-UI-063 for the positive stored-handler registration. No original
code, image, table fixture or payload is committed or executed.
