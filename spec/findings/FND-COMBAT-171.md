---
id: FND-COMBAT-171
title: The paused native shared item-type buffer exactly matches all 102 canonical ALL/4 rows
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001F291..0x0001F88B
tool: tools/research/rust-dos-runs/native_save.py; getter_metadata.item_types and compare_item_types at 61962e51; dinorefurb-dosbox-session 1.0.0; Python 3.12.10; xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-ACTOR-017 identifies the offered native ALL/4 binding and canonical
1530-byte span; FND-ACTOR-018 compares all known shipped inline copies.
FMT-ITEM-002 already describes the fifteen-byte structural partition.
FND-COMBAT-170 measures a compatible native kind-four header extent but
does not read the payload. This observation compares that live payload
with the complete canonical resource at the same paused saved-map boundary.

### Paused capture and roundtrip

On 2026-11-04 the fixed owned muted session restored saved-map from
v181-q063-native-save-input-20261104. The existing executable/configuration,
instruction/time, map/input state, native-file digest and frame checks
passed. It made no gameplay continuation, input or guest-memory write;
the draw and relocation records were empty. The session closed normally.

After capturing the named getter bases, the probe checked all five
record allocation headers as FND-COMBAT-170 describes. Kind four's base
was 76E3:0004; the header described 1531 payload bytes before marker 77
hex. It required the base read from DS:615F to agree with that checked
header before reading exactly 1530 bytes, in bounded nonwrapping chunks.
It did not read the remaining payload byte or the contents of other blocks.

The captured bytes' SHA-256 was
7235f6856602cde2751e31cdb2d10bbcb3eb0ed02e3c20606d553f906cd6989e.
All 1530 bytes were identical after the existing same-process checkpoint
roundtrip; the getter snapshots and allocation headers also matched.
The report retains both the bytes and their named address/extent locally.

### Canonical-source comparison

The committed comparator read at most 6816517 source bytes and required
OBJEX.GFF length 6816516 and XXH3-128
e08d3cac153c547f6804ca27b155915d before selecting file span 1F291..1F88B.
It required captured and reference lengths both to be 1530, then compared
every byte. The result was equal. This covers the entire 102-row resource
at stride fifteen, not a matching subset or just scratch row 73.

## Interpretation

At this specific paused boundary, the live kind-four generic-accessor
base points to source-equal shared item-type bytes in the measured
compatible extent. Its complete payload comparison is now admitted
alongside FND-COMBAT-169's FNFO/width/selector observations and
FND-COMBAT-170's header separation. This removes the unobserved live
ALL/4 payload at this boundary from the shared setup-state obligation
in Q-CONFIG-008.

The result does not identify the runtime request/provider that populated
the buffer, prove its original allocation arguments, or establish its
preservation through later original execution. No particular getter,
trigger, item action or cleanup consumer was executed by this probe.
Actual use-time record/index/alias admission remains Q-SCRIPT-022 and
semantic reader/writer coverage remains Q-ITEM-005. No new field meaning,
entry confidence, parity or complete-reading promotion follows.

## Alternatives

The live buffer is not merely a nonnull pointer or a same-sized allocation:
all canonical bytes match here. That does not distinguish an original
resource transfer from another producer of identical bytes. Checkpoint
roundtrip equality does not prove continuous lifetime or absence of
intermediate writes during gameplay. Source equality also supplies no
unread field semantics or guarantee that every consumer uses this base.

## How to reproduce

Use the pinned project-local Python and native_save.py at 61962e51:

```text
native_save.py v181-q063-native-save-input-20261104 v181-q067-item-types-20261104 verify
```

Use a fresh second name. The accepted native-save.json is under
GAME_DIR/captures/rust-dos-native-save/v181-q067-item-types-20261104/.
Its item_types member holds address, exact size, digest and retained bytes;
record_allocations holds the checked kind-four header. Load item_types
from that report and pass it with GAME_DIR/OBJEX.GFF to the committed
getter_metadata.compare_item_types function. It gives the fixed source
fingerprint/span and exact comparison. Licensed bytes, frames and checkpoint
data remain only in GAME_DIR.
