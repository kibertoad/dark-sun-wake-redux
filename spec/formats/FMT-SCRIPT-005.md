---
id: FMT-SCRIPT-005
title: Record with two script entry points in the 19-byte list
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: []
byte_order: little
size: 19
text: false
definition: fmt_script_005.ksy
evidence: [FND-SCRIPT-016, FND-SCRIPT-017, FND-SCRIPT-038, FND-RNG-068, FND-COMBAT-062, FND-PARTY-098, FND-SCRIPT-036]
conflicting: []
split_with: []
related: []
---

## Layout

A record of the table at `4F49:08A3` in BLD-GOG-EN-1.1, kept only in memory, on a list whose head
is the word at `57E0:5AF5` [FND-SCRIPT-016, FND-SCRIPT-017]. Segment `2D40` runs the scripts its
entry points name.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `unk_00` | Target cell x on view-click walk setup; complete producer contract open. | supported | FND-RNG-068 |
| `0x02` | 6 | `BYTE[6]` | `unk_02` | Word +02 is target cell y on view-click setup; +04 is walk mode; +06 selects the second-entry gate (1 or 2) and zero disables it. Complete producer/lifetime contract open. | supported | FND-RNG-068, FND-COMBAT-062, FND-PARTY-098 |
| `0x08` | 2 | `UINT16LE` | `entry_offset_0` | Offset of the first entry point, or its `GPLI` entry number while converted. | supported | FND-SCRIPT-016, FND-SCRIPT-017 |
| `0x0A` | 2 | `UINT16LE` | `entry_offset_1` | Offset of the second entry point, the same way. | supported | FND-SCRIPT-016, FND-SCRIPT-017 |
| `0x0C` | 2 | `UINT16LE` | `entry_script_0` | `GPL ` number of the first entry point. | supported | FND-SCRIPT-016, FND-SCRIPT-017 |
| `0x0E` | 2 | `UINT16LE` | `entry_script_1` | `GPL ` number of the second entry point. | supported | FND-SCRIPT-016, FND-SCRIPT-017 |
| `0x10` | 1 | `UINT8` | `unk_10` | Low seven bits supply the second-entry distance threshold; bit seven is cleared and can skip one slot visit in the walker sweep. Other uses remain open. | supported | FND-PARTY-098, FND-COMBAT-062 |
| `0x11` | 1 | `INT8` | `previous` | Index of the previous record on the list, or -1. | supported | FND-SCRIPT-016 |
| `0x12` | 1 | `INT8` | `next` | Index of the next record on the list, or -1. | supported | FND-SCRIPT-016, FND-SCRIPT-017 |
| `0x13` | | | | Total size 19 | | |

## Enumerations and flags

The second-entry condition word at +06 admits the recorded helper/threshold
gates for values one and two. Other nonzero values still perform the helper
calculations but do not run the second entry on this route. See FND-PARTY-098. Registration
opcodes 1B/1C write that condition word, not walk mode. Their radius-byte
update is `(old & 0x80) | UINT8(p3)`: the old high bit cannot be cleared,
and the incoming high bit can set it. The consumer separately masks the
radius to seven bits (FND-SCRIPT-038, FND-PARTY-261).

## Differences between builds

None known.

## Coverage

Read from the code that walks, keeps and converts the records [FND-SCRIPT-016, FND-SCRIPT-017];
no record was observed in memory.

## Open questions

- Remaining admitted producers, other readers/writers, caller phase/order and
  storage lifetime for the target, mode, condition and threshold fields. Existing
  findings give the map-sweep idle and condition/radius entry gates; the list-wide
  idle runner's own continuation reads next after mutation and its independent
  invocation remains open. For admitted zero-height cell coordinates, the geometry
  helper checks only interior raster cells for bit seven and skips both endpoints;
  use-time coordinate/grid bounds, aliases and live admission remain distinct from
  that local contract (FND-SCRIPT-037, FND-SCRIPT-036, FND-PARTY-098,
  FND-COMBAT-062, FND-RNG-068; Q-SCRIPT-017).
- Whether the single nonliteral registration radius can set the sweep-skip high
  bit. The other 381 decoded radius operands are small literals; the GPL/223
  local-word operand has a conditional small-radius sequence for counter zero
  through three; other counter values have no default assignment. Admitted
  values outside that class, other entries and preservation remain open
  (FND-SCRIPT-039, FND-SCRIPT-040, Q-SCRIPT-023).
