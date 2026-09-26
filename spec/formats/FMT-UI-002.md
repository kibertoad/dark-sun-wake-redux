---
id: FMT-UI-002
title: Child record of a window
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: 30
text: false
definition: fmt_ui_002.ksy
evidence: [FND-UI-002, FND-UI-008, FND-UI-011, FND-UI-016, FND-UI-018]
conflicting: []
split_with: []
related: []
---

## Layout

One entry of the `children` list of an FMT-UI-001 window: a control the window places
[FND-UI-002]. The generic window code resolves each child by its tag and number [FND-UI-008], and
the child dispatcher reads the loaded record [FND-UI-011].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `FARPTR<BYTE>` | `control` | 0 in every record. In the loaded window, the address of the control's record, or null; the child dispatcher passes over a child whose pointer is null. | supported | FND-UI-002, FND-UI-011 |
| `0x04` | 4 | `char[4]` | `tag` | The control's resource tag: `BUTN` (FMT-UI-003), `APFM` (FMT-UI-004) or `EBOX` (FMT-UI-005). The child dispatcher also handles `MENU` and passes over `ACCL`, which no shipped record uses. | supported | FND-UI-002, FND-UI-008, FND-UI-011 |
| `0x08` | 4 | `UINT32LE` | `number` | The control's resource number in `RESOURCE.GFF`. | supported | FND-UI-002, FND-UI-008 |
| `0x0C` | 2 | `INT16LE` | `x` | Pixels from the window's left edge to the control's left edge, 0 to 305 in the shipped records. | supported | FND-UI-002, FND-UI-016, FND-UI-018 |
| `0x0E` | 2 | `INT16LE` | `y` | Pixels from the window's top edge to the control's top edge, 0 to 181 in the shipped records. | supported | FND-UI-002, FND-UI-016, FND-UI-018 |
| `0x10` | 12 | `BYTE[12]` | `unk_10` | Purpose unknown. 0 in every record. | supported | FND-UI-002 |
| `0x1C` | 2 | `UINT16LE` | `flags` | 0 in every record. The child dispatcher passes over a child with the value `0x8000` set in the loaded window. | supported | FND-UI-002, FND-UI-011 |
| `0x1E` | | | | Total size 30 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 402 child records of the 28 `WIND` resources of `RESOURCE.GFF` in BLD-GOG-EN-1.1: 209 name an
`APFM`, 184 a `BUTN` and 9 an `EBOX`, and every one names a resource that exists [FND-UI-002].
The controls of three windows are drawn at the positions `x` and `y` give [FND-UI-016,
FND-UI-018].

## Open questions

- Whether `x` and `y` can be negative. No shipped record has a negative value (FND-UI-002,
  Q-UI-001).
