---
id: FND-SOUND-046
title: Setup error-word boundary candidates have no supported direct-transfer entries and overlap a driver-function stub
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:0C90..1C08:0C9C
tool: tools/research/exec-census/sound_ds_driver_function_census.py, setup_stub_transfers.py and resident_listing.py at 697e811f; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Q-CONFIG-011 retains two instruction-boundary candidates from
FND-SOUND-045: a possible memory read at 1C08:0C95 and an immediate move
at 0C96. FND-EXE-497 already identifies 0C96 as the driver-function
007F stub, without a caller or stored far pointer found by its census.
That earlier search checked stub starts, not the preceding interior byte.
This extends its fixed utility census to both candidate addresses.

The identity-checked load image is file 1400..1E5E0. The added search
checks every possible five-byte far-call/jump start in that image,
resolving raw segment:offset aliases by linear segment*16+offset, without
20-bit/A20 wrap modeling. It also
checks every byte of the complete 65536-byte 1C08 frame for 16-bit
relative calls/jumps, short jumps/conditions/loop forms and near
conditional jumps. Relative instruction fetch and target IP wrap within
that frame. These are raw candidates, not established instructions.

Independent controls retain the two known far callers of the 0065 stub
at load offsets 3EE2 and 4065, and the near dispatcher call at
1C08:0BFB. Synthetic controls cover far aliases, each relative form,
cross-boundary fetch/target wrap, unrelated destinations and image bounds.
The complete fixed census was repeated with identical output.

Neither candidate address has any far or relative transfer candidate
in that supported search. This includes the preceding byte 0C95,
which the earlier stub-start census did not test. No instruction-boundary
interpretation is needed to reject incoming raw candidates when none
exist in those representations.

The separate bounded listing shows the normal stub layout: an immediate
function-number move at 0C90 followed by a near jump at 0C93, then the
007F immediate move at 0C96 and another jump at 0C99. Both jumps target
the dispatcher at 03F6. The memory-read candidate at 0C95 overlaps the
previous jump and the following immediate move. No runtime entry into
that overlapping decode was established.

## Interpretation

This removes supported direct-transfer admission into these two starts
from Q-CONFIG-011. It corroborates the stub interpretation under those
representations, without declaring either raw candidate reachable or
promoting an unused-error-word claim. Remaining admission would require
an excluded entry mechanism, such as a computed transfer or operand-size
override, with its own evidence. The broad error-state alias/implicit
access and preservation obligations remain.

## Alternatives

The search excludes operand-size overrides and prefixed instruction
starts as separate forms, computed transfers, returns, interrupts,
stored pointers and runtime-written code/entries. A prefixed transfer
whose unprefixed opcode computes the same 16-bit target is still seen
at that opcode byte, but no general prefix-equivalence proof is claimed.
FND-EXE-497's stored-pointer result concerns the stub start, not every
possible interior-entry encoding. No complete reading or parity status
is promoted, and no original program was executed.

## How to reproduce

Use approved installed SOUND_DS.EXE, length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Run
`python -I tools/research/exec-census/sound_ds_driver_function_census.py <sound>`.
The command accepts only that fixed source identity and adds no CLI targets.
Repeat it and inspect the two boundary rows and retained control rows.
Run resident_listing.py with 1C08:0C90..1C08:0C9C independently.

Under GAME_DIR/analysis/disc-adv-survey-20261104/, retained
setup-error-boundary-transfers.txt and its -repeat pair both have SHA-256
e3e730e7e6695a50e71070cbd5d2d8f0bc09d825503e76a3f0940e6da67909eb;
setup-error-boundary-stubs.txt and its -repeat pair both have SHA-256
f9b75a48b6456f89535fc42b83b1a4377e713241ac53f934a5646d67b8f263d5.
Original-derived listings/reports remain only in that approved local store.
