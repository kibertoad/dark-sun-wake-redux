---
id: FMT-UI-005
title: Edit box resource (EBOX)
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: 168
text: false
definition: fmt_ui_005.ksy
evidence: [FND-UI-001, FND-UI-005, FND-UI-006, FND-UI-016, FND-UI-017]
conflicting: []
split_with: []
related: [RULE-UI-001]
---

## Layout

An `EBOX` resource of `RESOURCE.GFF`: a box a window places that shows text [FND-UI-001,
FND-UI-005]. The generic child dispatcher tests `event_mask` [FND-UI-006]. In the dialogue
captures the image `RESOURCE.GFF#EBOX/12400` names is drawn at the box's place in its window
[FND-UI-016, FND-UI-017].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `char[4]` | `tag` | `EBOX`. | supported | FND-UI-001 |
| `0x04` | 4 | `UINT32LE` | `size` | The resource's size in bytes, 168. | supported | FND-UI-001 |
| `0x08` | 4 | `UINT32LE` | `number` | The resource's number, equal to its number in the GFF directory. | supported | FND-UI-001 |
| `0x0C` | 4 | `UINT32LE` | `unk_0C` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x10` | 2 | `UINT16LE` | `unk_10` | Purpose unknown. 1 in every record. | supported | FND-UI-005 |
| `0x12` | 2 | `UINT16LE` | `unk_12` | Purpose unknown. 1 in every record. | supported | FND-UI-005 |
| `0x14` | 2 | `UINT16LE` | `unk_14` | Purpose unknown. 1 in every record. | supported | FND-UI-005 |
| `0x16` | 2 | `UINT16LE` | `unk_16` | Purpose unknown. 16, 64, 200 or 11,226 in the shipped records. | supported | FND-UI-005 |
| `0x18` | 4 | `UINT32LE` | `number_copy` | The resource's number again. | supported | FND-UI-005 |
| `0x1C` | 6 | `BYTE[6]` | `unk_1C` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x22` | 2 | `UINT16LE` | `width` | Box width in pixels. | supported | FND-UI-005 |
| `0x24` | 2 | `UINT16LE` | `height` | Box height in pixels. | supported | FND-UI-005 |
| `0x26` | 20 | `BYTE[20]` | `unk_26` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x3A` | 4 | `UINT32LE` | `image` | Number of a `BMP ` resource of the same file drawn under the box's text, or 0 for none. | supported | FND-UI-005, FND-UI-016, FND-UI-017 |
| `0x3E` | 26 | `BYTE[26]` | `unk_3E` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x58` | 2 | `UINT16LE` | `unk_58` | Purpose unknown. Equal to `event_mask` of the button `unk_5A` names. | supported | FND-UI-005 |
| `0x5A` | 4 | `UINT32LE` | `unk_5A` | Purpose unknown. The number of a `BUTN` resource: 2082, 2099, 15309 or 18313. | supported | FND-UI-005 |
| `0x5E` | 16 | `BYTE[16]` | `unk_5E` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x6E` | 1 | `UINT8` | `unk_6E` | Purpose unknown. 128 in every record. | supported | FND-UI-005 |
| `0x6F` | 3 | `BYTE[3]` | `unk_6F` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x72` | 2 | `UINT16LE` | `unk_72` | Purpose unknown. 65,535 in every record. | supported | FND-UI-005 |
| `0x74` | 4 | `BYTE[4]` | `unk_74` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x78` | 1 | `UINT8` | `unk_78` | Purpose unknown. 2 in every record. | supported | FND-UI-005 |
| `0x79` | 5 | `BYTE[5]` | `unk_79` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x7E` | 2 | `UINT16LE` | `unk_7E` | Purpose unknown. 0, 229 or 251. | supported | FND-UI-005 |
| `0x80` | 2 | `UINT16LE` | `unk_80` | Purpose unknown. 0, 127 or 199. | supported | FND-UI-005 |
| `0x82` | 20 | `BYTE[20]` | `unk_82` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0x96` | 2 | `UINT16LE` | `event_mask` | Event bits. The box is considered when the game looks for the control under the pointer only when the value 2 is set (RULE-UI-001). 0, 4 or 10 in the shipped records. | supported | FND-UI-005, FND-UI-006 |
| `0x98` | 16 | `BYTE[16]` | `unk_98` | Purpose unknown. 0 in every record. | supported | FND-UI-005 |
| `0xA8` | | | | Total size 168 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 7 `EBOX` resources of `RESOURCE.GFF` and of `CD:RESOURCE.GFF` in BLD-GOG-EN-1.1, which are
byte for byte the same: each is 168 bytes, and each nonzero `image` names a `BMP ` resource of the
file [FND-UI-001, FND-UI-005]. The placement of `image` was compared with two dialogue captures
[FND-UI-016, FND-UI-017].

## Open questions

- What `unk_16`, `unk_58`, `unk_5A`, `unk_7E` and `unk_80` do. `unk_58` and `unk_5A` repeat
  fields of a button, as the copied bytes of an FMT-UI-001 window do. `unk_7E` and `unk_80` are
  not 0 only in `RESOURCE.GFF#EBOX/12400`, where they are 229 and 199, and in
  `RESOURCE.GFF#EBOX/12402`, where they equal its width and height (FND-UI-005, Q-UI-001).
- What each bit of `event_mask` means (FND-UI-006, Q-UI-001).
- Which font the box draws its text in, and where in the box (Q-UI-002).
