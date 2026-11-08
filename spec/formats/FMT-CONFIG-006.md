---
id: FMT-CONFIG-006
title: Sound configuration SOUND.CFG
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["SOUND.CFG"]
byte_order: little
size: 59
text: false
definition: fmt_config_006.ksy
evidence: [FND-CONFIG-257, FND-CONFIG-213, FND-CONFIG-005, FND-CONFIG-236, FND-CONFIG-019, FND-CONFIG-020, FND-CONFIG-021, FND-CONFIG-214, FND-CONFIG-023, FND-CONFIG-024, FND-SOUND-007, FND-SOUND-008, FND-SOUND-013, FND-SOUND-020, FND-SOUND-037, FND-SOUND-047, FND-SOUND-092, FND-CONFIG-258, FND-CONFIG-259, FND-CONFIG-260, FND-CONFIG-261]
conflicting: []
split_with: []
related: []
---

## Layout

The corrected replacement for FMT-CONFIG-001. FND-CONFIG-257 distinguishes actual writer fields from equal shipped values. The whole of `SOUND.CFG`, which the sound setup program writes in one piece of 59 bytes
[FND-CONFIG-213] and the game reads into a whole-file buffer through its sound library
[FND-CONFIG-005, FND-CONFIG-019]. The loader does not validate the 59-byte layout. The meanings below
come from the shipped file, its matching record and the parser-to-output handoff
[FND-CONFIG-257, FND-SOUND-020, FND-SOUND-037, FND-SOUND-047].

The constructor words are not necessarily the final settings written to the
file. The later adjustment can replace address, IRQ and DMA words according
to eight record masks and a separate mode word. It selects list results
before applying them, and specialized results follow shared results in the
apply order (FND-CONFIG-258). Actual dialog/count/index and storage admission
remain conditions. This does not change the byte offsets or widths below.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `unk_00` | Installed 0x220. Full constructor copies record MidiAddress at +2E when record +EC is zero, otherwise word +EE. Passed with the next three words to initialization calls; downstream role remains open. | supported | FND-CONFIG-257, FND-CONFIG-021 |
| `0x02` | 2 | `UINT16LE` | `unk_02` | Installed 5. Full constructor copies record MidiIrq at +5A when +EC is zero, otherwise word +F0. Downstream role remains open. | supported | FND-CONFIG-257, FND-CONFIG-021 |
| `0x04` | 2 | `UINT16LE` | `unk_04` | Installed 1. Full constructor copies Dma at record +86 when +EC is zero, otherwise word +F2. | supported | FND-CONFIG-257, FND-CONFIG-021 |
| `0x06` | 2 | `UINT16LE` | `unk_06` | Installed 1. Full constructor copies Dma at record +86 again when +EC is zero, otherwise word +F4. This is not the source flag count; the duplicate DMA value caused the former inference. | supported | FND-CONFIG-257, FND-CONFIG-021 |
| `0x08` | 2 | `UINT16LE` | `unk_08` | 122, the matching record's card ID. Values `0x77` and `0x79` select two hardware port writes using `unk_0a`; `0x69` selects a runtime byte value of 90 rather than 100. The game plays no sound effect, speech or song while it is 113, the card ID of "No Sound", unless `unk_14` asks for disc music. Other uses unknown. | supported | FND-CONFIG-257, FND-CONFIG-020, FND-CONFIG-021, FND-SOUND-007, FND-SOUND-008, FND-SOUND-013 |
| `0x0A` | 2 | `UINT16LE` | `unk_0a` | Installed 0x220. Full and partial constructors copy DigitalAddress at record +44 when +EC is zero, otherwise word +F6. Used as an I/O base for card IDs 0x77/0x79 and passed with the next three words to selector-one setup calls. | supported | FND-CONFIG-257, FND-CONFIG-020, FND-CONFIG-024 |
| `0x0C` | 2 | `UINT16LE` | `unk_0c` | Installed 5. Full and partial constructors copy Irq at record +70 when +EC is zero, otherwise word +F8. | supported | FND-CONFIG-257, FND-CONFIG-024 |
| `0x0E` | 2 | `UINT16LE` | `unk_0e` | Installed 1. Full and partial constructors copy Dma at record +86 when +EC is zero, otherwise word +FA. | supported | FND-CONFIG-257, FND-CONFIG-024 |
| `0x10` | 2 | `UINT16LE` | `unk_10` | Installed 1. Full and partial constructors copy Dma at record +86 again when +EC is zero, otherwise word +FC. It is not a flag count. | supported | FND-CONFIG-257, FND-CONFIG-024 |
| `0x12` | 2 | `UINT16LE` | `unk_12` | Installed 122. Both constructors copy CardId at record +28, as the full constructor also does at output +08. Remaining game-side uses are unknown. | supported | FND-CONFIG-257 |
| `0x14` | 2 | `UINT16LE` | `unk_14` | Installed 11. Both constructors copy packed record Flags at +DC and OR bit 0x02/0x08 when the same bit of setup override word DS:ECCA is set. This is not MidiDriverChunkNumber, which is copied to +38. Bit 0x08 clears the library enable word during initialization; bit 0x02 gates disc audio; bit 0x01 gates selector-one setup. Other effects remain open. | supported | FND-CONFIG-257, FND-CONFIG-020, FND-CONFIG-021, FND-CONFIG-024, FND-SOUND-013 |
| `0x16` | 14 | `char[14]` | `music_driver` | Mode-selected MIDI driver name. Setup uses the record real-mode name when DS:ECCC is one and the protected-mode name otherwise. The installed name occupies ten bytes followed by four NULs; arbitrary copy length is not capped by this field. | supported | FND-CONFIG-257 |
| `0x24` | 14 | `char[14]` | `digital_driver` | Mode-selected digital driver name, selected by the same mode condition as the MIDI name. The installed name occupies ten bytes followed by four NULs; copy storage admission remains required. | supported | FND-CONFIG-257 |
| `0x32` | 2 | `UINT16LE` | `unk_32` | 1 in the shipped file; setup copies the selected record's CardGroup word `+0x2A`. Initialization replaces 1 or 2 with 4 in the loaded buffer when `DS:3417` equals 1, and those values also gate later string and file calls. A value of 3 caps a music-level request at 90 instead of 100. Setup's summary substitutes -1 for the first block's DMA argument when this word is 4. Other uses unknown. | supported | FND-CONFIG-257, FND-CONFIG-236, FND-CONFIG-020, FND-CONFIG-021, FND-CONFIG-214, FND-SOUND-020, FND-SOUND-037, FND-SOUND-047 |
| `0x34` | 2 | `UINT16LE` | `unk_34` | 4 in the shipped file; setup writes the literal 4. The library compares it with a runtime word using a signed greater-than branch; the compared quantity's purpose is unknown. | supported | FND-CONFIG-257, FND-CONFIG-214, FND-CONFIG-023 |
| `0x36` | 2 | `UINT16LE` | `unk_36` | 8 in the shipped file. Setup copies the selected record's DigitalDriverChunkNumber word `+0xD6`; the library uses it as the `ADV ` number paired with the second settings block. This source-field identity does not prove driver installation or device effects. | supported | FND-CONFIG-257, FND-CONFIG-214, FND-CONFIG-023, FND-CONFIG-024, FND-SOUND-020, FND-SOUND-037, FND-SOUND-047 |
| `0x38` | 2 | `UINT16LE` | `unk_38` | 11 in the shipped file. Setup copies the selected record's MidiDriverChunkNumber word `+0xD8`; the library pairs it with the first settings block. This does not prove driver installation; the unchanged installed flag at `0x14` can bypass that path. | supported | FND-CONFIG-257, FND-CONFIG-214, FND-CONFIG-023, FND-CONFIG-024, FND-SOUND-020, FND-SOUND-037, FND-SOUND-047, FND-SOUND-092 |
| `0x3A` | 1 | `UINT8` | `unk_3a` | 0 in the shipped file. Setup copies only the low byte of the selected record's SamplePlaybackMode at `+0xDA`, despite its parser's word store. Other library effects remain unread. | supported | FND-CONFIG-257, FND-CONFIG-214, FND-CONFIG-023, FND-SOUND-037, FND-SOUND-047 |
| `0x3B` | | | | Total size 59 | | |

## Enumerations and flags

None.

## Differences between builds

None known. The disc has no `SOUND.CFG`; the setup program writes it [FND-CONFIG-257,
FND-CONFIG-213].

## Coverage

The one installed file [FND-CONFIG-257].

## Open questions

- Remaining selected-record producer and incoming storage
  admission, game-side block roles and field effects (Q-CONFIG-005). FND-CONFIG-257 settles the local full/partial block writes; retain source/override producers and storage admission.
  FND-SOUND-020/037/047 already bind CardGroup, both driver-number fields,
  SamplePlaybackMode and name-copy alternatives; do not repeat those local
  mappings. A digital source-field name is compatible with an actual digital
  block but does not settle its receiving driver calls or device effects.
  Conditional source settings and loaded-buffer rewrites remain distinct.
  The finite setup roots are SOUND_DS.EXE 1429:042D, 1429:063A and
  1BD4:0003 in coverage/BLD-GOG-EN-1.1/SOUND_DS.EXE.tsv. The latter writes
  the whole buffer; the former two share selected-record state with
  FMT-CONFIG-002 and the name/loader contract in FND-SOUND-020. Record and
  output span, aliasing, incoming selection and child preservation remain
  conditions, not established lifetime claims.
  The later14A2:0087 adjustment and14A2:0AA5 initial-index helper are also in
  that inventory. FND-CONFIG-258 supplies their local mapping and order;
  actual returned indices, changed masks and label/frame lifetime remain open.
  FND-CONFIG-259 adds17FB:00B0's conditional group-based EC marker and its
  inventory-backed membership/index helpers. It populates no extended fields.
  FND-CONFIG-260 assigns two of that census's other EC writes and two EE
  writes to card7C's paired extended-field producer. MainGroup F0 and F8
  take different source fields; this is not a universal card-field meaning.
  FND-CONFIG-261 supplies that route's decimal-prefix/sign/modulo conversion;
  the caller retains AX, while actual source/table/frame admission stays open.
  Twenty other EC and18 other EE writes, excluded mechanisms and actual
  group/count/pointer preservation remain obligations, not immutable state.
  DSUN.EXE roots 4734:0063, 47B9:0236 and initializer4734:0133 are in its
  corresponding inventory. Resource selector47B9:0334 and digital setup
  49EC:015F from FND-CONFIG-024 share sound-driver helpers; their canonical
  inventory ownership and unresolved indirect targets remain unassigned
  here. No CONFIG ownership JSON or complete-reading declaration is claimed.
  Existing inventory coverage does not establish missing branches/callers;
  embedded ADV code still lacks independent inventories. File layout does
  not answer loader short-read/release outcomes, computed/block access,
  changed configuration bindings or native driver behavior.
