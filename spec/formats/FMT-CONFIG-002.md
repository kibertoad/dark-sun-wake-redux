---
id: FMT-CONFIG-002
title: Sound card list SOUND.INI
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["SOUND.INI", "CD:SOUND.INI"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-CONFIG-003, FND-CONFIG-004, FND-CONFIG-006, FND-CONFIG-007]
conflicting: []
split_with: []
related: []
---

## Layout

ASCII text in lines ended by CR LF, read by the sound setup program and never by the game
[FND-CONFIG-004, FND-CONFIG-005]. A `;` starts a comment that runs to the end of the line, and a
line that starts with `//` is a separator. Every other line that is not blank is a key, a name in
square brackets, followed by spaces (tabs in the disc's copy) and an optional value: a decimal
integer, which may be -1, a hexadecimal integer written with `0x`, or a string in square brackets.
The keys come in a fixed order: a header from `[Header]` to `[EndOfHeader]`, three groups that
each open with the group's key and `[NumberOfRecords]` and end with `[EndOfGroup]`, and
`[EndOfFile]`. Each record of a group runs from `[StartRecord]` to `[EndRecord]`. A count key
gives how many of the keys after it follow. Whether case matters, and what the setup program
does with a missing key, an unknown key or a malformed line, are not known [FND-CONFIG-007].

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| `Header` | none | `header` | Opens the header. | supported | FND-CONFIG-007 |
| `GameName` | `char[]` | `game_name` | The game's name. | supported | FND-CONFIG-007 |
| `CdEnabled` | none | `cd_enabled` | A flag; present in the shipped files. | supported | FND-CONFIG-007 |
| `MultipleInstallDisabled` | none | `multiple_install_disabled` | A flag; present in the shipped files. | supported | FND-CONFIG-007 |
| `MidiNotEnabled` | none | `midi_not_enabled` | A flag; present in the header, and as a record flag in one record. | supported | FND-CONFIG-007 |
| `EndOfHeader` | none | `end_of_header` | Closes the header. | supported | FND-CONFIG-007 |
| `MainGroup` | none | `main_group` | Opens the first group. | supported | FND-CONFIG-007 |
| `DigitalGroup` | none | `digital_group` | Opens the second group. | supported | FND-CONFIG-007 |
| `MusicOnlyGroup` | none | `music_only_group` | Opens the third group. | supported | FND-CONFIG-007 |
| `NumberOfRecords` | `INT16` | `record_count` | The number of records in the group; -1 in the shipped third group, which has none. | supported | FND-CONFIG-007 |
| `StartRecord` | none | `start_record` | Opens a record. | supported | FND-CONFIG-007 |
| `CardName` | `char[]` | `card_name` | The name of the sound card, at most 40 characters by the file's own comment. | supported | FND-CONFIG-007 |
| `CardId` | `INT16` | `card_id` | The card's number, 102 to 131 in the shipped file; the one `SOUND.CFG` records. | supported | FND-CONFIG-003, FND-CONFIG-007 |
| `CardGroup` | `INT16` | `card_group` | Purpose unknown. 0, 1 or 4 in the main group. | supported | FND-CONFIG-007 |
| `RmMidiDriverName` | `char[]` | `rm_midi_driver` | The real-mode music driver's file name, with its extension. | supported | FND-CONFIG-003, FND-CONFIG-006, FND-CONFIG-007 |
| `RmDigitalDriverName` | `char[]` | `rm_digital_driver` | The real-mode digital sound driver's file name, with its extension. | supported | FND-CONFIG-003, FND-CONFIG-006, FND-CONFIG-007 |
| `PmMidiDriverName` | `char[]` | `pm_midi_driver` | The protected-mode music driver's file name. | supported | FND-CONFIG-007 |
| `PmDigitalDriverName` | `char[]` | `pm_digital_driver` | The protected-mode digital sound driver's file name. | supported | FND-CONFIG-007 |
| `MidiDriverChunkNumber` | `INT16` | `midi_chunk` | Purpose unknown; `SOUND.CFG` repeats it. | supported | FND-CONFIG-003, FND-CONFIG-007 |
| `DigitalDriverChunkNumber` | `INT16` | `digital_chunk` | Purpose unknown; `SOUND.CFG` repeats it. | supported | FND-CONFIG-003, FND-CONFIG-007 |
| `NumberOfMidiAddresses` | `INT16` | `midi_address_count` | How many of `MidiAddress` and `MidiAltAddress` follow. | supported | FND-CONFIG-007 |
| `MidiAddress` | `INT16` | `midi_address` | The music port address to try first. | supported | FND-CONFIG-007 |
| `MidiAltAddress` | `INT16` | `midi_alt_address` | Another music port address. | supported | FND-CONFIG-007 |
| `NumberOfDigitalAddresses` | `INT16` | `digital_address_count` | How many of `DigitalAddress` and `DigitalAltAddress` follow. | supported | FND-CONFIG-007 |
| `DigitalAddress` | `INT16` | `digital_address` | The digital sound port address to try first. | supported | FND-CONFIG-007 |
| `DigitalAltAddress` | `INT16` | `digital_alt_address` | Another digital sound port address. | supported | FND-CONFIG-007 |
| `NumberOfIrqs` | `INT16` | `irq_count` | How many of `Irq` and `AltIrq` follow. | supported | FND-CONFIG-007 |
| `Irq` | `INT16` | `irq` | The IRQ to try first. | supported | FND-CONFIG-007 |
| `AltIrq` | `INT16` | `alt_irq` | Another IRQ. | supported | FND-CONFIG-007 |
| `NumberOfMidiIrqs` | `INT16` | `midi_irq_count` | How many of `MidiIrq` and `MidiAltIrq` follow. | supported | FND-CONFIG-007 |
| `MidiIrq` | `INT16` | `midi_irq` | The music IRQ to try first. | supported | FND-CONFIG-007 |
| `MidiAltIrq` | `INT16` | `midi_alt_irq` | Another music IRQ. | supported | FND-CONFIG-007 |
| `NumberOfDmas` | `INT16` | `dma_count` | How many of `Dma` and `AltDma` follow. | supported | FND-CONFIG-007 |
| `Dma` | `INT16` | `dma` | The DMA channel to try first. | supported | FND-CONFIG-007 |
| `AltDma` | `INT16` | `alt_dma` | Another DMA channel. | supported | FND-CONFIG-007 |
| `Flags` | `INT16` | `flag_count` | How many flag keys follow. | supported | FND-CONFIG-007 |
| `DspEnabled` | none | `dsp_enabled` | A record flag. | supported | FND-CONFIG-007 |
| `DspNotEnabled` | none | `dsp_not_enabled` | A record flag; by the file's comment, required for the choice with no sound. | supported | FND-CONFIG-007 |
| `Jumpers` | `INT16` | `jumper_count` | How many jumper keys follow. | supported | FND-CONFIG-007 |
| `AddressJumper` | none | `address_jumper` | A setting the card lets the player choose; by the file's comment, the jumpers decide what the manual installation asks for. | supported | FND-CONFIG-007 |
| `IrqJumper` | none | `irq_jumper` | A setting the card lets the player choose; by the file's comment, the jumpers decide what the manual installation asks for. | supported | FND-CONFIG-007 |
| `DmaJumper` | none | `dma_jumper` | A setting the card lets the player choose; by the file's comment, the jumpers decide what the manual installation asks for. | supported | FND-CONFIG-007 |
| `MidiAddressJumper` | none | `midi_address_jumper` | A setting the card lets the player choose; by the file's comment, the jumpers decide what the manual installation asks for. | supported | FND-CONFIG-007 |
| `MidiIrqJumper` | none | `midi_irq_jumper` | A setting the card lets the player choose; by the file's comment, the jumpers decide what the manual installation asks for. | supported | FND-CONFIG-007 |
| `MidiDmaJumper` | none | `midi_dma_jumper` | A setting the card lets the player choose; by the file's comment, the jumpers decide what the manual installation asks for. | supported | FND-CONFIG-007 |
| `DigitalAddressJumper` | none | `digital_address_jumper` | A setting the card lets the player choose; by the file's comment, the jumpers decide what the manual installation asks for. | supported | FND-CONFIG-007 |
| `DigitalIrqJumper` | none | `digital_irq_jumper` | A setting the card lets the player choose; by the file's comment, the jumpers decide what the manual installation asks for. | supported | FND-CONFIG-007 |
| `DigitalDmaJumper` | none | `digital_dma_jumper` | A setting the card lets the player choose; by the file's comment, the jumpers decide what the manual installation asks for. | supported | FND-CONFIG-007 |
| `EndRecord` | none | `end_record` | Closes a record. | supported | FND-CONFIG-007 |
| `EndOfGroup` | none | `end_of_group` | Closes a group. | supported | FND-CONFIG-007 |
| `EndOfFile` | none | `end_of_file` | Closes the file. | supported | FND-CONFIG-007 |

## Enumerations and flags

None.

## Differences between builds

None known. The disc's copy uses tabs between keys and values and lists 16 cards in the main
group and 14 in the digital group, where the installed copy lists 13 and 13 [FND-CONFIG-007].

## Coverage

Every line of the installed file and of the disc's copy [FND-CONFIG-007].

## Open questions

- How the setup program parses the file and what it does with keys it does not expect, and what
  `CardGroup` and the chunk numbers mean (FND-CONFIG-004, FND-CONFIG-007, Q-CONFIG-006).
