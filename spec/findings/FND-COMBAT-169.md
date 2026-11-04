---
id: FND-COMBAT-169
title: A paused native saved-map checkpoint has canonical FNFO buffers, distinct scratch and selected objects, and finite disjoint free lists
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF48..0x0002037E
tool: tools/research/rust-dos-runs/native_save.py and getter_metadata.py at c28627da; compare_archive at 83593ed7; dinorefurb-dosbox-session 1.0.0; Python 3.12.10; xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-140/145/150/153/154 identify getter tables and the scratch-object
converter, but leave actual populated state open. FND-PARTY-119 and
FND-ITEM-015 identify free-list operations without admitting every native
chain. This observation samples those named inputs at FND-COMBAT-140's
already verified post-Save checkpoint, not during temporary-object cleanup.

### Strict paused restoration

On 2026-11-04 the owned muted session restored saved-map from
v181-q063-native-save-input-20261104. Executable identity, checkpoint
configuration, stopped instruction/time, map/input predicates, native-file
digest and frame matched the source metadata. The stop was 2F8B:097D at
197002506750 emulated nanoseconds, the relocated 39D1:097D input builder.
The admitted mapping delta is minus 0A46 hex from analysis load segment
1000. The probe sent no input, wrote no guest memory and made no gameplay
continuation. Its draw and relocation records were empty.

The probe read only fixed fields through the checked bounded reader.
Its existing same-process checkpoint roundtrip preserved all sampled
metadata, map state, native-file digest and frame. The successful report
retains the sampled bytes, addresses and hashes under getter_metadata.
This records endpoint equality, not intermediate writer absence.

### Getter input state

The 980 bytes at analysis DS:5AF7 and 98 bytes at DS:60ED exactly equal
the canonical FNFO/1 and FNFO/2 spans of FND-CONFIG-153. The comparison
reader verified OBJEX.GFF length 6816516 and XXH3-128
e08d3cac153c547f6804ca27b155915d before selecting 1FF48..2037E.
It separately compared the two buffers, rather than accepting a matching
length or inferring the archive originally selected by the runtime.

DS:60EB was 520, while DS:1A32 was 523. Object-table rows at analysis
4F49:0C33 plus three times index were:

| Index | Kind | Paired record word |
|---|---:|---:|
| 319 | 0 | 182 |
| 520 | 4 | 73 |
| 523 | 0 | 0 |

Thus scratch-object metadata was nonempty while the distinct selected
object was empty. Neither its kind nor the scratch index can be inferred
from the other's value. Kind zero at 319 does not prove an empty
presentation entry or a cleared paired word; presentation emptiness is
not measured by this capture.

The five selector rows match FND-CONFIG-150's assignments. Kind one has
positions 0/1/2 equal to 5/2/4 and start 2; kind two has positions
0/2/3/4 equal to 15/16/17/4 and start 4; kind five has position 1 equal
to 2 and start 1. Kinds three and four have no nonzero position among
zero through 15 and start zero. All other positions in those five rows
are zero. The stride words for kinds one through five are 23,49,66,15,23.
Width bytes zero/one are one, four/five are four, and seven/eight/nine/
twelve/thirteen are two. These are sampled inputs to the earlier static
getter interpretation, not an observation of a particular getter call.

### Free-list state

The bounded analysis followed links within the captured 524 three-byte
rows, detecting repeats before rereading a node. It found:

| Head word | Head index | Nodes | Minimum index | Maximum index |
|---|---:|---:|---:|---:|
| DS:56ED | 14 | 10 | 5 | 14 |
| DS:56EF | 453 | 134 | 320 | 453 |
| DS:56F1 | 267 | 181 | 48 | 267 |

Each chain ended at 9999, all visited kind bytes were zero, no index was
out of the captured range and no node belonged to two lists. Minimum and
maximum do not assert that every intervening index was a free node.
Counter words DS:55C6/55C8 were 66/90. The observed sums 66+134=200 and
90+181=271 match the two corresponding initial capacity intervals in
FND-PARTY-119, without proving their invariants on other paths.

## Interpretation

This admits one actual source-equal metadata state, width/selector state,
distinct scratch/selected values and finite disjoint free-list snapshot.
Those are removed from Q-COMBAT-067 and Q-CONFIG-008's unobserved state
at this specific paused boundary. It supplies no populated temporary
root, getter result, cleanup graph or deletion invocation: slot 319's
sampled kind is zero.

Native cleanup root/state admission, intervening writes, map values,
record/base/stack extents, source-provider identity, pool ownership over
time and later release effects remain open. A full source-equal buffer
does not establish which runtime request populated it. The same snapshot
need not hold after loading begins. No entry confidence, parity or
complete-reading promotion follows.

## Alternatives

Executable initial metadata and a failed or bypassed resource load do not
describe the captured buffers: both match the canonical resources at this
boundary. That does not rule out earlier failure followed by replacement.
A zero selected-object kind does not imply a zero scratch-object kind;
the sampled distinct indices and rows disprove that equality here.
Finite endpoint chains do not exclude transient cycles or aliased writes
between checkpoints, or guarantee that another caller preserves them.

## How to reproduce

With the pinned project-local Python, run native_save.py at c28627da:

```text
native_save.py v181-q063-native-save-input-20261104 v181-q067-getter-metadata-20261104 verify
```

Use a fresh second run name; never overwrite the retained observation.
Output is GAME_DIR/captures/rust-dos-native-save/<new-run>/native-save.json.
The original accepted run uses the second name above. The script verifies
the approved executable and strict state; its fixed spans and mapping
checks are in getter_metadata.capture. Pool traversal uses its bounded
pools function on exactly the captured rows and three head words.

For the source comparison, load report['getter_metadata'] from that JSON
and pass it with GAME_DIR/OBJEX.GFF to getter_metadata.compare_archive at
83593ed7. It verifies the full archive fingerprint and compares exactly
980 and 98 bytes at the named source offsets. All licensed bytes, frames
and checkpoint/private-drive data remain only in GAME_DIR. The session
closed and released its owned run lock after observation.
