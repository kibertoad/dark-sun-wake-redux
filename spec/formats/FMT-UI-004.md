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
evidence: [FND-UI-001, FND-UI-005, FND-UI-006, FND-UI-007]
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
| `0x0C` | 28 | `BYTE[28]` | `unk_0C` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x28` | 2 | `UINT16LE` | `width` | Frame width in pixels. | supported | FND-UI-005 |
| `0x2A` | 2 | `UINT16LE` | `height` | Frame height in pixels. | supported | FND-UI-005 |
| `0x2C` | 44 | `BYTE[44]` | `unk_2C` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x58` | 2 | `UINT16LE` | `event_mask` | Event bits. The frame is handed an event when this shares a bit with the event's bits (RULE-UI-001). The code can set, clear and zero bits of it while it runs. | supported | FND-UI-005, FND-UI-006, FND-UI-007 |
| `0x5A` | 26 | `BYTE[26]` | `unk_5A` | Purpose unknown. 0 in every record. The dispatcher requires the 32-bit value at `0x62` of the loaded record to be nonzero, so the game writes some of these bytes while it runs. | supported | FND-UI-005, FND-UI-006 |
| `0x74` | | | | Total size 116 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 97 `APFM` resources of `RESOURCE.GFF` and of `CD:RESOURCE.GFF` in BLD-GOG-EN-1.1, which are
byte for byte the same: each is 116 bytes, and apart from the header only `width`, `height` and
`event_mask` vary [FND-UI-001, FND-UI-005].

## Open questions

- What each bit of `event_mask` means. The shipped values are 0, 32, 38, 70, 110, 160, 224, 230,
  486 and 494 (FND-UI-005, Q-UI-001).
- What the game stores in `unk_5A` while it runs (FND-UI-006, Q-UI-001).
