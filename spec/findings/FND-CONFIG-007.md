---
id: FND-CONFIG-007
title: SOUND.INI is a list of sound cards written as bracketed tags with values
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND.INI
    offset: 0x00..0xD58C
  - build: BLD-GOG-EN-1.1
    file: CD:SOUND.INI
    offset: 0x00..0x86ED
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The installed `SOUND.INI` is 54,669 bytes (XXH3-128 `aecc7469098e0c188e0e3d056c5fd530`) of ASCII
text in 1,500 lines, each ended by CR LF.

- A `;` starts a comment that runs to the end of the line, on a line of its own or after a value.
  The comments at the top of the file give its rules: a card name is at most 40 characters, a
  driver name at most 15 and must include its extension, square brackets are forbidden in a
  comment, and a list of addresses, IRQs or DMA channels has at most 10 entries. 28 further lines
  start with `//` and hold only a row of dashes.
- Every other line that is not blank is a tag in square brackets, such as `[CardId]`, then spaces
  and an optional value: a decimal integer, which may be -1; a hexadecimal integer written `0x`;
  or a string in square brackets. 535 values are decimal, 185 hexadecimal and 131 strings, and 157
  tags have no value.
- The file opens with `[Header]` and a game name, three tags with no value, of which
  `[MidiNotEnabled]` is one, and `[EndOfHeader]`. Three groups follow, `[MainGroup]`,
  `[DigitalGroup]` and `[MusicOnlyGroup]`, each opening with `[NumberOfRecords]`, 13, 13 and -1,
  holding that many records, 13, 13 and none, and ending with `[EndOfGroup]`. The file ends with
  `[EndOfFile]`.
- A record runs from `[StartRecord]` to `[EndRecord]`. It gives, in this order, `[CardName]`,
  `[CardId]`, `[CardGroup]`, `[RmMidiDriverName]`, `[RmDigitalDriverName]`,
  `[PmMidiDriverName]`, `[PmDigitalDriverName]`, `[MidiDriverChunkNumber]`,
  `[DigitalDriverChunkNumber]`, then for each of music addresses, digital addresses, IRQs, music
  IRQs and DMA channels a count tag (`[NumberOfMidiAddresses]` and so on) followed by the first
  value and the alternatives (`[MidiAddress]`, `[MidiAltAddress]`; `[DigitalAddress]`,
  `[DigitalAltAddress]`; `[Irq]`, `[AltIrq]`; `[MidiIrq]`, `[MidiAltIrq]`; `[Dma]`, `[AltDma]`),
  then `[Flags]` with a count and that many flag tags with no value (`[DspEnabled]` or
  `[DspNotEnabled]`), then `[Jumpers]` with a count and that many jumper tags with no value
  (`[AddressJumper]`, `[IrqJumper]`, `[DmaJumper]`, `[MidiAddressJumper]` and the like). A record
  with no digital address gives its count and no address.
- Card IDs run from 102 to 131; two records hold 122.

The disc's `CD:SOUND.INI` (34,542 bytes, XXH3-128 `8dcbd9c58a54517946ca5cbc1203bbd3`) has the same
syntax with tabs in place of the spaces, and 16 records in its main group and 14 in its digital
group.

## Interpretation

`SOUND.INI` is the setup program's list of sound cards (FND-CONFIG-213): for each card, its drivers
for real mode and protected mode, the numbers of the driver chunks, and the addresses, IRQs and DMA
channels it can use, the first of each being the default. The game itself does not read it
(FND-CONFIG-005). The installed copy was written for this release with fewer cards than the
disc's.

## Alternatives

The setup program's parser was not read, so what it does with an unknown tag, a missing tag or a
malformed line, and whether case matters, are not known. None of the tag names occurs as a string
in `SOUND_DS.EXE`.

## How to reproduce

Read both files as ASCII, strip comments, and list each tag with the form of its value.
