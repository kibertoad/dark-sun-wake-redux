---
id: FMT-UI-001
title: Window resource (WIND)
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: null
text: false
definition: fmt_ui_001.ksy
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-008, FND-UI-041, FND-UI-042, FND-UI-011, FND-UI-016, FND-UI-018, FND-UI-045]
conflicting: []
split_with: []
related: []
---

## Layout

A `WIND` resource of `RESOURCE.GFF`: one window, a fixed part of 261 bytes followed by the list
of the controls it places [FND-UI-001, FND-UI-002]. The generic window code finds a window by the
number at `0x08` and resolves each child by its tag and number [FND-UI-008]. Two captures show
controls drawn at the window's origin plus the child's `x` and `y` [FND-UI-016, FND-UI-018].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `char[4]` | `tag` | `WIND`. | supported | FND-UI-001 |
| `0x04` | 4 | `UINT32LE` | `size` | The resource's size in bytes, equal to the size in the GFF directory. | supported | FND-UI-001 |
| `0x08` | 4 | `UINT32LE` | `number` | The resource's number, equal to its number in the GFF directory. | supported | FND-UI-001, FND-UI-008 |
| `0x0C` | 138 | `BYTE[138]` | `unk_0C` | Purpose unknown in the file: bytes copied from another record (see the Open questions). In the loaded record the child dispatcher reads a list of rectangles here, a count word followed by 8-byte rectangles. | supported | FND-UI-003, FND-UI-011 |
| `0x96` | 2 | `INT16LE` | `unk_96` | Copied bytes in the file. The child dispatcher adds the loaded record's value to the x of each child's rectangle, so there it is the window's left edge on the screen. | supported | FND-UI-003, FND-UI-011 |
| `0x98` | 2 | `INT16LE` | `unk_98` | Copied bytes in the file. The child dispatcher adds the loaded record's value to the y of each child's rectangle, so there it is the window's top edge on the screen. | supported | FND-UI-003, FND-UI-011 |
| `0x9A` | 4 | `BYTE[4]` | `unk_9A` | Purpose unknown. Copied bytes, 0 in every record. | supported | FND-UI-003 |
| `0x9E` | 2 | `UINT16LE` | `flags` | Window flags; see Enumerations and flags. 256 in windows 14000 to 14002, 8,192 in 19500, 2,048 in 19501 and 0 in the others. | supported | FND-UI-003, FND-UI-011, FND-UI-045 |
| `0xA0` | 8 | `BYTE[8]` | `unk_A0` | Purpose unknown. Copied bytes, 0 except byte `0xA4`, which is 1 in windows 14001 and 14002. | supported | FND-UI-003 |
| `0xA8` | 22 | `BYTE[22]` | `unk_A8` | Purpose unknown. 0 in every record. | supported | FND-UI-002 |
| `0xBE` | 2 | `UINT16LE` | `width` | Window width in pixels, 91 to 320. | supported | FND-UI-002 |
| `0xC0` | 2 | `UINT16LE` | `height` | Window height in pixels, 28 to 200. | supported | FND-UI-002 |
| `0xC2` | 2 | `UINT16LE` | `image` | Number of a `BMP ` resource of the same file whose size is within one pixel of the window's, or 0. Not 0 in 6 windows. | supported | FND-UI-002 |
| `0xC4` | 42 | `BYTE[42]` | `unk_C4` | Purpose unknown. 0 in every record. | supported | FND-UI-002 |
| `0xEE` | 4 | `FARPTR<FMT-UI-001>` | `next_window` | 0 in every record. In the loaded record, the next window in the game's list of registered windows. | supported | FND-UI-002, FND-UI-008, FND-UI-011 |
| `0xF2` | 1 | `UINT8` | `unk_F2` | Purpose unknown. 0 in every record. The child dispatcher adds it to the address of each child record. | supported | FND-UI-002, FND-UI-011 |
| `0xF3` | 2 | `UINT16LE` | `child_count` | Number of child records that follow the fixed part. | supported | FND-UI-002, FND-UI-011 |
| `0xF5` | 4 | `BYTE[4]` | `unk_F5` | Purpose unknown. 0 in every record. | supported | FND-UI-002 |
| `0xF9` | 4 | `FARPTR<BYTE>` | `after_children` | 0 in every record. In the loaded record, a handler the activation routine calls after it dispatches the children, or null. | supported | FND-UI-002, FND-UI-041 |
| `0xFD` | 4 | `FARPTR<BYTE>` | `before_children` | 0 in every record. In the loaded record, a handler the activation routine calls before it dispatches the children, or null. | supported | FND-UI-002, FND-UI-041 |
| `0x101` | 4 | `BYTE[4]` | `unk_101` | Purpose unknown. 0 in every record. | supported | FND-UI-002 |
| `0x105` | `child_count * 30` | `FMT-UI-002[child_count]` | `children` | The controls the window places, in stored order. | supported | FND-UI-002, FND-UI-008, FND-UI-011, FND-UI-016, FND-UI-018 |
| | | | | Total size `261 + child_count * 30` | | |

## Enumerations and flags

### `flags`

The word in the loaded record [FND-UI-011, FND-UI-045]. The low five bits are a state the window
routines set, replacing the file's value; the other bits are flags.

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| `0x001F` | `WINDOW_STATE` | The state bits. Registration, making the window current and moving it back each keep the other bits and replace these. | supported | FND-UI-045 |
| `0x0002` | `WINDOW_STATE_REGISTERED` | State once the window is registered. | supported | FND-UI-045 |
| `0x0004` | `WINDOW_STATE_SKIPPED` | State once the window is moved back to the end of the list. The child dispatcher, the redraw loop and the edit box pass over the window; making it current clears the bit. | supported | FND-UI-011, FND-UI-045 |
| `0x0008` | `WINDOW_STATE_CURRENT` | State once the window is made current and drawn. A change to one of its buttons or frames is repainted at once. | supported | FND-UI-045 |
| `0x0010` | `WINDOW_STATE_10` | Tested with `0x0008` before a repaint. No writer is known. | supported | FND-UI-045 |
| `0x0040` | `WINDOW_HAS_MNEMONICS` | Set at registration when a button child has a nonzero byte `0x6C` once loaded. The key handler searches the buttons' mnemonics only when it is set. | supported | FND-UI-045 |
| `0x0080` | `WINDOW_HAS_ACCELERATORS` | Set at registration when the window has an `ACCL` child. The key handler searches the accelerators only when it is set. | supported | FND-UI-045 |
| `0x0100` | `WINDOW_LAST_DISPATCHED` | From the file. The child dispatcher visits no window after this one. | supported | FND-UI-011 |
| `0x0800` | `FLAG_0800` | Purpose unknown. Set in the file record of window 19501; the state writes keep it. | supported | FND-UI-003, FND-UI-045 |
| `0x2000` | `FLAG_2000` | Purpose unknown. Set in the file record of window 19500; the state writes keep it. | supported | FND-UI-003, FND-UI-045 |
| `0x4000` | `WINDOW_RELEASES_A0` | Removing the window first passes its word `0xA0` to a release routine. No writer is known. | supported | FND-UI-045 |

## Differences between builds

None known.

## Coverage

All 28 `WIND` resources of `RESOURCE.GFF` and of `CD:RESOURCE.GFF` in BLD-GOG-EN-1.1, which are
byte for byte the same: each is `261 + child_count * 30` bytes, and each of the 402 children
names a resource that exists [FND-UI-001, FND-UI-002]. The placement of the controls of
`RESOURCE.GFF#WIND/12500`, `#WIND/12501` and `#WIND/3020` was compared with captures
[FND-UI-016, FND-UI-018].

## Open questions

- What the copied bytes `0x0C` to `0xA7` are for. Apart from `flags`, they repeat the bytes of an
  edit box or a button, which may be what the tool that wrote the file left in its buffer, and the
  game uses the same places for a rectangle list and the window's screen position once it has
  loaded the window. Whether it overwrites them on loading is not known. Earlier notes read the
  value at `0x3A` as the window's image; it is the `image` field of the copied edit box
  (FND-UI-003, FND-UI-011, Q-UI-001).
- Whether the game draws `image` as the window's background, and where. Its sizes match the
  windows, but no code that reads it is known (FND-UI-002, Q-UI-001).
  FND-UI-042 finds no qualified literal +C2 memory operand after
  classifying the three raw candidates, with neighboring dimension reads
  as controls. An unused source field and a copied/rebased/computed
  consumer both remain possible: the literal absence supports the former
  only circumstantially and does not exclude the latter. A concrete
  producer-to-consumer trace, including copies and callbacks, is needed.
- What bits `0x800` and `0x2000` of `flags` do. FND-UI-045 finds no
  instruction with a literal `0x9E` or `0x9F` operand that tests, sets or
  clears them; a copied, rebased or computed consumer is not excluded
  (Q-UI-001).
- What sets `unk_96` and `unk_98` of a loaded window, which place it on the screen. The Look panel
  is drawn at (67, 44), where the file holds 0 and 0 (FND-UI-011, FND-UI-018, Q-UI-003).
