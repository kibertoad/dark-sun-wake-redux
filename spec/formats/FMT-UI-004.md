---
id: FMT-UI-004
title: Application frame resource (APFM)
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: 116
text: false
definition: fmt_ui_004.ksy
evidence: [FND-UI-001, FND-UI-005, FND-UI-006, FND-UI-007, FND-COMBAT-088]
conflicting: []
split_with: []
related: [RULE-UI-001]
---

## Layout

An `APFM` resource of `RESOURCE.GFF`: a rectangle a window places that has no image of its own
[FND-UI-001, FND-UI-005]. The code tests and changes `event_mask` [FND-UI-006, FND-UI-007].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `char[4]` | `tag` | `APFM`. | supported | FND-UI-001 |
| `0x04` | 4 | `UINT32LE` | `size` | The resource's size in bytes, 116. | supported | FND-UI-001 |
| `0x08` | 4 | `UINT32LE` | `number` | The resource's number, equal to its number in the GFF directory. | supported | FND-UI-001, FND-UI-007 |
| `0x0C` | 28 | `BYTE[28]` | `unk_0C` | Zero in every resource; registration fills placement subfields below. Remaining purposes unknown. | supported | FND-UI-005, FND-COMBAT-088 |
| `0x28` | 2 | `UINT16LE` | `width` | Frame width in pixels. | supported | FND-UI-005 |
| `0x2A` | 2 | `UINT16LE` | `height` | Frame height in pixels. | supported | FND-UI-005 |
| `0x2C` | 44 | `BYTE[44]` | `unk_2C` | Zero in every resource; includes the runtime parent link below. Remaining purposes unknown. | supported | FND-UI-005, FND-COMBAT-088 |
| `0x58` | 2 | `UINT16LE` | `event_mask` | Event bits. The frame is handed an event when this shares a bit with the event's bits (RULE-UI-001). The code can set, clear and zero bits of it while it runs. | supported | FND-UI-005, FND-UI-006, FND-UI-007 |
| `0x5A` | 26 | `BYTE[26]` | `unk_5A` | Purpose unknown. 0 in every record. The dispatcher requires the 32-bit value at `0x62` of the loaded record to be nonzero, so the game writes some of these bytes while it runs. | supported | FND-UI-005, FND-UI-006 |
| `0x74` | | | | Total size 116 | | |

Registration interprets these subranges of the otherwise opaque resource
fields. They are runtime values, not pointers or positions shipped in the
resource, whose corresponding bytes are zero (FND-COMBAT-088):

- `0x10`, two bytes: child placement x from the window child entry.
- `0x12`, two bytes: child placement y from the window child entry.
- `0x14`, two bytes: x + width - 1, retaining the low word.
- `0x16`, two bytes: y + height - 1, retaining the low word.
- `0x54`, four bytes: far pointer to the parent window supplied by registration.

The shared setup helper sees image-number fields at frame offsets `0x2C`
and `0x34`; both are zero in all shipped frames. With those fields intact,
valid disjoint storage, no address wrap and both stack guards bypassed,
the APFM setup branch returns zero without requesting images and preserves
the mask and callback region. This is a local construction result, not a
proof of later callback-field values or of successful resource acquisition
(FND-COMBAT-088, Q-COMBAT-049).

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 97 `APFM` resources of `RESOURCE.GFF` and of `CD:RESOURCE.GFF` in BLD-GOG-EN-1.1, which are
byte for byte the same: each is 116 bytes, and apart from the header only `width`, `height` and
`event_mask` vary [FND-UI-001, FND-UI-005].

## Open questions

- What the remaining subfields of `unk_0C` and `unk_2C` do beyond the mapped
  registration placement and parent link (FND-COMBAT-088, Q-UI-001).
- What each bit of `event_mask` means. The shipped values are 0, 32, 38, 70, 110, 160, 224, 230,
  486 and 494 (FND-UI-005, Q-UI-007).
- What the game stores in `unk_5A` while it runs (FND-UI-006, Q-UI-007).
