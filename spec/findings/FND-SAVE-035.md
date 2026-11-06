---
id: FND-SAVE-035
title: Resource updates can return pending metadata when write-through is absent or an error bypasses preparation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0066..38FF:02B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:00E8..37FC:077C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:08FB..37FC:09C1
tool: connected evidence review at 7e324b04; retained resident_listing.py reports from FND-COMBAT-121/122/124/127
environment: null
---

## Observation

This answers RULE-SAVE-001/Q-SAVE-007's local pending-state return
contract. It composes the existing opener, writer, removal and preparation
readings; it does not repeat their allocation and I/O investigations.
The retained source is DSUN.EXE,634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets below are hexadecimal.

### The write-through flag is constructed from open options

FND-COMBAT-127 shows that opening clears local header +14 and adds
flag masks4/8 from the supplied option word. Copying that header to
record+10 places these runtime flags at record+24. The original option
word is separately stored at record+6. Consequently the immediate
preparation gate is record+24 mask8, not a direct test of record+6 or
an unchanged on-disk flag. Under unchanged construction and fields,
open option8 sets that gate; option2 alone does not.

### The payload-error branch skips preparation even with that flag

FND-COMBAT-121's writer publishes directory metadata before its payload
seek/write. A mismatching seek or write count sets global error15, then
the joined continuation sets the current active record's pending byte1.
The test at0511 skips further bookkeeping for nonzero global error.
The common test at0591 again requires global error0 before testing
record+24 mask8. Nonzero error goes directly to the failure return,
without calling08FB through this tail. Enabling write-through is therefore
not an unconditional guarantee of preparation after a failed payload.

With zero global error, mask8 clear returns AX0 without preparation;
mask8 set calls08FB and forwards its AX. The free-list helper result
retains FND-COMBAT-123's separate qualifications; AX0 does not validate
all directory extents or all earlier helper results.

### Removal has a separate, simpler preparation tail

After its admitted successful removal and directory compaction,
FND-COMBAT-122's body sets pending byte1, updates its metadata fields
and tests record+24 mask8. Clear returns AX0 without08FB; set calls
08FB and forwards its AX. This tail has no intervening global-error
gate equivalent to the writer's0591 test. Earlier rejections and child
effects remain as recorded by FND-COMBAT-122/123.

### Conditional pending state at the update return

For a stable same record, valid distinct storage and admitted returning
helpers, the own publication/preparation continuations compose as follows:

| Continuation after setting pending1 | Local ending |
| --- | --- |
| Writer has nonzero global error | AXFFFF; skips its optional preparation regardless of flag8 |
| Writer error0, or removal tail, with flag8 clear | AX0; no own clear of pending1 |
| Either tail calls preparation and it returns AX0 | Preparation has cleared pending to0 |
| Either tail calls preparation and it returns AXFFFF | Preparation has not performed its success clear; no local rollback |

FND-COMBAT-124 supplies the preparation checks and success clear;
FND-SAVE-039 observes one controlled native success. Early update
rejection does not independently establish that the incoming record was
clean, and a later reader, explicit close or another update may prepare
the record before Quit. These are return-boundary claims, not continuous
state or ordinary shutdown histories.

## Interpretation

The ordinary producer investigation can focus on write-through-clear
updates, writer errors that bypass preparation, failed preparation, other
dirty producers and later state changes. A successful checked preparation
on the same stable record is not itself an unexplained source of pending1.
FND-SAVE-039's observed options000A include option8, but that run did
not sample record+24 or establish its full producer history. Its explicit
dirty seed cannot substitute for finding one of the ordinary histories.

This removes the unknown local option/error/return relationship from
Q-SAVE-007. It leaves actual callers, field changes, handle/file bindings,
native failures and other mechanisms open. No whole-entry complete
reading, queue closure or parity promotion follows.

## Alternatives

- Successful payload transfer alone is not preparation or a dirty-byte clear.
- Option8 does not bypass the writer's nonzero-error gate.
- A successful update with flag8 clear can return with pending metadata.
- Runtime flag construction does not prove that no later writer changes it.
- A known pending return does not prove the byte stays nonzero until Quit.

## How to reproduce

Reuse the controlled retained reports under
GAME_DIR/analysis/hit-incoming-20261104/: restore-archive-open-contract.txt,
restore-save-directory-writer.txt, restore-save-resource-removal.txt and
restore-archive-flush.txt. Regenerate with resident_listing.py using the
listed half-open ranges and source fingerprint. Follow the writer's error
store through0508,0511,0591 and its flag/preparation tail; compare removal's
tail and the opener's option-derived header copy. Reuse the complete
connected readings above for children. Keep listings and original data local.
