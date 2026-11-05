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
evidence: [FND-CONFIG-003, FND-CONFIG-213, FND-CONFIG-006, FND-CONFIG-007, FND-SOUND-020, FND-SOUND-021, FND-SOUND-022, FND-SOUND-023, FND-SOUND-024, FND-SOUND-025, FND-SOUND-026, FND-SOUND-027, FND-SOUND-037, FND-SOUND-029, FND-SOUND-030, FND-SOUND-031, FND-SOUND-032, FND-SOUND-033, FND-SOUND-034, FND-SOUND-035, FND-SOUND-036, FND-SOUND-038, FND-SOUND-039, FND-SOUND-040, FND-SOUND-041, FND-SOUND-042, FND-SOUND-043, FND-SOUND-044, FND-SOUND-045, FND-SOUND-046, FND-SOUND-047]
conflicting: []
split_with: []
related: []
---

## Layout

ASCII text in lines ended by CR LF, read by the sound setup program and never by the game
[FND-CONFIG-213, FND-CONFIG-005]. A `;` starts a comment that runs to the end of the line, and a
line that starts with `//` is a separator. Every other line that is not blank is a key, a name in
square brackets, followed by spaces (tabs in the disc's copy) and an optional value: a decimal
integer, which may be -1, a hexadecimal integer written with `0x`, or a string in square brackets.
The keys come in a fixed order: a header from `[Header]` to `[EndOfHeader]`, three groups that
each open with the group's key and `[NumberOfRecords]` and end with `[EndOfGroup]`, and
`[EndOfFile]`. Each record of a group runs from `[StartRecord]` to `[EndRecord]`. A count key
gives how many of the keys after it follow [FND-CONFIG-007]. The recorded
ASCII tag path is case-insensitive (FND-SOUND-020). The native outer record
dispatch does not enforce source field order or required-field completeness;
nested alternate, flag and jumper contexts have distinct consumption and
rejection rules (FND-SOUND-037). Complete malformed/file-error admission
remains open.

Inside a counted jumper list, a nonpositive tag lookup consumes the line
without assigning a bit. A positive lookup outside the eight jumper
selectors fails the record; inside Flags, nonpositive lookup instead fails.
The installed source includes MidiDmaJumper as a counted jumper even though
the native tag table lacks it. Under intact lookup/context/storage it is
ignored rather than rejected, and supplies no jumper bit (FND-SOUND-037).
All 26 installed records fit the measured counted source bands: 940
interior keys comprise 649 outer keys, 27 flag children, 66 jumper children
and 198 further alternate children. Two jumper children use the ignored
spelling. This accounts for conditional installed source consumption and
group-counter composition, without native storage/lifetime admission
(FND-SOUND-038).
Two fresh native setup-prefix runs returned parser success, counts 13,
13 and -1 and nonzero group pointers; this observes the normal endpoint
without verifying every record byte or later allocation/chooser state
(FND-SOUND-039).
The normal setup group allocation has a four-byte prefix before the
offset-four payload. Its first word, at offset zero of the returned
segment, stores the block's size in paragraphs including that prefix.
The second word is not interpreted here. Count 13 requests 3302 bytes
and expects 207 paragraphs; count -1 requests low word 65282 and expects
4081 paragraphs. Native header values and record contents remain separate
from that local binding (FND-SOUND-040).
Two further fresh runs match every byte of both populated 3302-byte
groups against the documented source projection, with zero differences.
All three first-prefix words match the predicted sizes, and the modeled
extents are disjoint at return. Negative-count payload and slack remain
unread; later lifetime and intermediate writers are not established
(FND-SOUND-041).

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
| `CardGroup` | `INT16` | `card_group` | Copied to settings word +32; value four substitutes FFFF for a non-FFFF DMA summary argument. Wider effects remain open. | supported | FND-CONFIG-007, FND-SOUND-047 |
| `RmMidiDriverName` | `char[]` | `rm_midi_driver` | The real-mode music driver's file name, with its extension. | supported | FND-CONFIG-003, FND-CONFIG-006, FND-CONFIG-007 |
| `RmDigitalDriverName` | `char[]` | `rm_digital_driver` | The real-mode digital sound driver's file name, with its extension. | supported | FND-CONFIG-003, FND-CONFIG-006, FND-CONFIG-007 |
| `PmMidiDriverName` | `char[]` | `pm_midi_driver` | The protected-mode music driver's file name. | supported | FND-CONFIG-007 |
| `PmDigitalDriverName` | `char[]` | `pm_digital_driver` | The protected-mode digital sound driver's file name. | supported | FND-CONFIG-007 |
| `MidiDriverChunkNumber` | `INT16` | `midi_chunk` | Copied to the corresponding settings chunk word; complete consumer effects remain open. | supported | FND-CONFIG-003, FND-CONFIG-007, FND-SOUND-047 |
| `DigitalDriverChunkNumber` | `INT16` | `digital_chunk` | Copied to the corresponding settings chunk word; complete consumer effects remain open. | supported | FND-CONFIG-003, FND-CONFIG-007, FND-SOUND-047 |
| `SamplePlaybackMode` | `INT16` | `sample_playback_mode` | Native parser accepts word zero or one; handoff copies its low byte to settings +3A. Wider effects remain open. | supported | FND-SOUND-037, FND-SOUND-047 |
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

The recorded name paths match ordinary ASCII tags without case distinction
using the shipped character table. Real-mode and protected-mode name tags
write separate inline record fields; the selected-record routine copies
real-mode names to the output when its mode word is one and protected-mode
names otherwise. The chunk-number tags populate record words later copied
to output offsets 0x38 and 0x36, respectively (FND-SOUND-020). This does not
establish every parser arm, count/error path or actual setup selection.

Source numeric spelling is distinct from the setup helper's parsed word.
The helper copies digits, minus and x/X into its token but stops before
other hexadecimal letters. Under intact input/context/storage conditions,
thirty-two installed alternate-address fields consequently scan as the
single hexadecimal digit two or three after the base prefix. They produce
word two in eight cases and word three in twenty-four, rather than their
complete source magnitudes. This is a conditional parsing contract, not
an observed driver configuration. The measured installed line, tag and
numeric token spans fit their mapped gaps/local token array; the helpers
do not thereby gain general bounds or native storage admission
(FND-SOUND-034).

Numeric failure has separate channels. Finding no digit or minus returns
helper failure without writing its destination. A short token that makes
the scanner assign nothing can still produce helper success and copy an
unassigned local word; no fixed numeric default is specified for that
word. A bare zero/x prefix takes a successful zero assignment instead.
Repeated count tags have no local seen-count rejection, and the count is
published before allocation. Local record/group failure does not roll
back preceding stores (FND-SOUND-035). General malformed-input and native
storage contracts remain open.

Within the bounded retained token/storage conditions, magnitude overflow
above 4294967295 is an assigned result: the plain numeric path writes
word FFFF for positive overflow and 0000 for negative overflow. Smaller
magnitudes use their signed result's low word. The bracketed-value helper
accepts an empty value, checks source position rather than destination
capacity, and can leave partial copied bytes on failure. These are local
parser contracts, with native spans and lifetime still open (FND-SOUND-036).

## Enumerations and flags

The header's CdEnabled, MultipleInstallDisabled and MidiNotEnabled tags OR
flags 0x02, 0x10 and 0x08 respectively into setup's header state. Retained
0x10 forces combined-card selection from MainGroup. Without it, the prompt
chooses the combined route or separate MusicOnlyGroup/DigitalGroup records.
The recognized ProtectedMode and RealMode tags make no assignment in this
binary's local header parser; its file-opening routine initializes the
separate mode word to one. This is not a complete writer/preservation census
(FND-SOUND-021).

## Differences between builds

None known. The disc's copy uses tabs between keys and values and lists 16 cards in the main
group and 14 in the digital group, where the installed copy lists 13 and 13 [FND-CONFIG-007].

## Coverage

Every line of the installed file and of the disc's copy [FND-CONFIG-007].

## Open questions

- Remaining group/numeric/count and malformed token/value parsing beyond
  the recorded local tag/field/output bindings and installed small tokens
  (FND-SOUND-020, FND-CONFIG-213, FND-CONFIG-007, FND-SOUND-025,
  FND-SOUND-037, FND-SOUND-029, FND-SOUND-034, FND-SOUND-035,
  FND-SOUND-036, FND-SOUND-038, Q-CONFIG-006).
- Native stream/storage lifetime, file/error contracts and OS outcomes
  (Q-CONFIG-011). FND-SOUND-030/031/032 record local byte, lower-read,
  translation and flush dispatch. FND-SOUND-033 binds the actual binary
  mode, startup free-slot writes and conditional 512-byte construction;
  retained successful regular-file conditions bypass text translation
  and read-side flush scanning. Changed state, failed allocation/device
  results and other callers remain competing native alternatives.
  FND-SOUND-039 observes the native normal count/pointer return twice;
  FND-SOUND-041 verifies populated record bytes and modeled group bounds
  at that endpoint; intermediate storage and later lifetime remain open.
  FND-SOUND-042 records the local exhausted-slot ordering and retained
  initial-slot allocation-failure cleanup: close is attempted, active
  fields cleared and its result discarded, without proving DOS success.
  FND-SOUND-043 records the lower write's raw/translated branches and
  word arithmetic for short translated writes. Native negative-flush
  reachability, source/stack bounds and DOS outcomes remain open.
  FND-SOUND-044 binds initial error-map values and normalization categories;
  FND-SOUND-045 identifies the exact-literal selector consumer in a
  create-mode branch skipped by retained rb flags. Computed/implicit
  accesses and preservation remain Q-CONFIG-011. FND-SOUND-046 excludes
  supported direct-transfer entry into the two boundary candidates;
  excluded entry mechanisms remain open without a global unused-word claim.
- Full consumer effects of CardGroup, the driver chunk numbers and
  SamplePlaybackMode beyond their recorded local stores/output copies
  (FND-SOUND-020, FND-SOUND-037, Q-CONFIG-012). FND-SOUND-047 binds
  handoff widths, digital-copy alternatives and the CardGroup summary
  argument; its literal census retains pointer/implicit-access and boundary
  limits. These local obligations do not require another unchanged reading.
- Actual list-choice row/index, storage and continuation admission between
  parsed card records and the settings writers, beyond the conditional local
  small-label profile. Its producers, retained vector and child/frame state
  remain distinct from the local arithmetic (FND-SOUND-021, FND-SOUND-022,
  Q-SOUND-010).
