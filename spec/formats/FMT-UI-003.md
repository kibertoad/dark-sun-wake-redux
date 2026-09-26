---
id: FMT-UI-003
title: Button resource (BUTN)
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: null
text: false
definition: fmt_ui_003.ksy
evidence: [FND-UI-001, FND-UI-004, FND-UI-006, FND-UI-016, FND-UI-018]
conflicting: []
split_with: []
related: [RULE-UI-001]
---

## Layout

A `BUTN` resource of `RESOURCE.GFF`: one button, a fixed part of 110 bytes followed by a tail
whose length the fixed part gives [FND-UI-001, FND-UI-004]. The generic child dispatcher tests
`event_mask` [FND-UI-006], and the captures show a button's `icon` drawn at its place in the
window [FND-UI-016, FND-UI-018].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `char[4]` | `tag` | `BUTN`. | supported | FND-UI-001 |
| `0x04` | 4 | `UINT32LE` | `size` | The resource's size in bytes, equal to the size in the GFF directory. | supported | FND-UI-001 |
| `0x08` | 4 | `UINT32LE` | `number` | The resource's number, equal to its number in the GFF directory. | supported | FND-UI-001 |
| `0x0C` | 1 | `UINT8` | `unk_0C` | Purpose unknown. 0 in every record. | supported | FND-UI-004 |
| `0x0D` | 1 | `UINT8` | `unk_0D` | Purpose unknown. 64 in `RESOURCE.GFF#BUTN/2046`, 0 in every other record. | supported | FND-UI-004 |
| `0x0E` | 1 | `UINT8` | `unk_0E` | Purpose unknown. 1 in buttons 14005 to 14007, 0 in every other record. | supported | FND-UI-004 |
| `0x0F` | 25 | `BYTE[25]` | `unk_0F` | Purpose unknown. 0 in every record. | supported | FND-UI-004 |
| `0x28` | 2 | `UINT16LE` | `width` | Button width in pixels. | supported | FND-UI-004 |
| `0x2A` | 2 | `UINT16LE` | `height` | Button height in pixels. | supported | FND-UI-004 |
| `0x2C` | 44 | `BYTE[44]` | `unk_2C` | Purpose unknown. 0 in every record. | supported | FND-UI-004 |
| `0x58` | 2 | `UINT16LE` | `event_mask` | Event bits. A button with the value 4 or 2 set is passed over when the game looks for the control under the pointer (RULE-UI-001). 0 in 106 records. | supported | FND-UI-004, FND-UI-006 |
| `0x5A` | 4 | `UINT32LE` | `number_copy` | The resource's number again. | supported | FND-UI-004 |
| `0x5E` | 6 | `BYTE[6]` | `unk_5E` | Purpose unknown. 0 in every record. | supported | FND-UI-004 |
| `0x64` | 4 | `UINT32LE` | `icon` | Number of the `ICON` resource of the same file drawn for the button, or 0 for none. | supported | FND-UI-004, FND-UI-016, FND-UI-018 |
| `0x68` | 5 | `BYTE[5]` | `unk_68` | Purpose unknown. 0 in every record. | supported | FND-UI-004 |
| `0x6D` | 1 | `UINT8` | `tail_length` | Number of bytes after the fixed part. | supported | FND-UI-004 |
| `0x6E` | `tail_length` | `BYTE[tail_length]` | `tail` | Purpose unknown. Printable bytes followed by zeros. | supported | FND-UI-004 |
| | | | | Total size `110 + tail_length` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 139 `BUTN` resources of `RESOURCE.GFF` and of `CD:RESOURCE.GFF` in BLD-GOG-EN-1.1, which are
byte for byte the same: each is `110 + tail_length` bytes, and each nonzero `icon` names an `ICON`
resource of the file [FND-UI-001, FND-UI-004]. Buttons of three windows were compared with
captures [FND-UI-016, FND-UI-018].

## Open questions

- What each bit of `event_mask` means beyond the values 4 and 2, which keep the pointer search
  from choosing the button. The shipped values are 2, 4, 80, 84, 144, 160 and 208 (FND-UI-004,
  FND-UI-006, Q-UI-001).
- What `unk_0D`, `unk_0E` and `tail` do. The tail's text is not copied here (FND-UI-004,
  Q-UI-001).
- Why the art of some buttons is a pixel or more larger or smaller than `width` and `height`,
  and which frame of a many-frame `icon` the game draws in which state (FND-UI-004, Q-UI-002).
