---
id: FND-SOUND-028
title: Setup record dispatch maps scalar fields and counted alternate, flag and jumper sublists
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-SOUND-037]
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:038E..1AF6:0A00
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000CD60..0x0000CDD4
tool: tools/research/exec-census/resident_listing.py at b5bd8e13 and tools/evidence/report.mjs table; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0
environment: null
---

## Observation

Q-CONFIG-006 retains record parsing beyond FND-SOUND-020's name/chunk
bindings. This reads all fifty outer dispatch targets and both nested
flag/jumper paths, including the five alternate-setting loops. It describes
local stores and control flow, not successful native file admission.
Offsets, masks and instruction locations below are hexadecimal; list
indices and counts are decimal. A record address is its supplied far base.

### Outer record dispatch and fields

1AF6:038E repeatedly reads a line, extracts its tag and looks it up through
the helpers already recorded in FND-SOUND-020/025. Nonpositive lookup,
failed line/tag/value/numeric helpers and rejected selectors return zero.
Positive lookup indices 3 through 52 dispatch through CS:0A10. EndRecord
returns one immediately. There is no required-field, field-order or
declared-count equality check before that return. This reader does not
clear the record on entry; the separate group allocation/clear contract
is FND-SOUND-024/026. Earlier stores are not rolled back on failure.

The complete field mapping of its active outer arms is:

| Tag | Branch | Record offset | Local action |
| --- | --- | --- | --- |
| CardName | 03EE | 0000 | Bracketed value copied through NUL |
| CardId | 0411 | 0028 | Numeric word |
| CardGroup | 042B | 002A | Numeric word |
| NumberOfMidiAddresses | 0446 | 002C | Numeric word |
| MidiAddress | 0461 | 002E | Numeric word |
| MidiAltAddress | 047C | 0030 | Numeric word, then alternate loop |
| NumberOfDigitalAddresses | 04F5 | 0042 | Numeric word |
| DigitalAddress | 0510 | 0044 | Numeric word |
| DigitalAltAddress | 052B | 0046 | Numeric word, then alternate loop |
| NumberOfMidiIrqs | 0901 | 0058 | Numeric word |
| MidiIrq | 091C | 005A | Numeric word |
| MidiAltIrq | 0937 | 005C | Numeric word, then alternate loop |
| NumberOfIrqs | 05A4 | 006E | Numeric word |
| Irq | 05BF | 0070 | Numeric word |
| AltIrq | 05DA | 0072 | Numeric word, then alternate loop |
| NumberOfDmas | 0653 | 0084 | Numeric word |
| Dma | 066E | 0086 | Numeric word |
| AltDma | 0689 | 0088 | Numeric word, then alternate loop |
| RmDigitalDriverName | 072C | 009A | Bracketed value copied through NUL |
| RmMidiDriverName | 0704 | 00A9 | Bracketed value copied through NUL |
| PmDigitalDriverName | 077C | 00B8 | Bracketed value copied through NUL |
| PmMidiDriverName | 0754 | 00C7 | Bracketed value copied through NUL |
| DigitalDriverChunkNumber | 07BF | 00D6 | Numeric word |
| MidiDriverChunkNumber | 07A4 | 00D8 | Numeric word |
| SamplePlaybackMode | 09AF | 00DA | Assign one or zero only; any other converted word fails |
| Flags | 07DA | 00DC | Count to local word, then nested flag processing |
| Jumpers | 084D | 00DE | Count to local word, then nested jumper processing |

EndOfFile, the three group-opening tags, EndOfGroup, CdEnabled,
DspEnabled, DspNotEnabled, MultipleInstallDisabled, MidiNotEnabled,
Header, EndOfHeader, GameName, DriverPath and the eight jumper tags
dispatch directly to failure when encountered as outer record tags.
Tags outside indices 3 through 52 also fail there. In particular a flag
accepted inside its counted sublist is not thereby an accepted outer tag.

### Five alternate-setting loops

Each alternate arm first writes its current value to index one, even if
the associated count is at most one. It then sets SI=2 and, while the
signed count word is greater than SI, reads one more line. Its extracted
tag must exactly equal the expected uppercase alternate tag in the
retained lookup table. On success it writes a numeric word at the
array's base plus 2*SI, increments SI and repeats. Missing/wrong tags or
helper failure return zero from the record reader.

| Alternate tag | Count word | Array base | Further-line loop |
| --- | --- | --- | --- |
| MidiAltAddress | 002C | 002E | 0499..04F2 |
| DigitalAltAddress | 0042 | 0044 | 0548..05A1 |
| MidiAltIrq | 0058 | 005A | 0954..09AD |
| AltIrq | 006E | 0070 | 05F7..0650 |
| AltDma | 0084 | 0086 | 06A6..0701 |

The base value is index zero and the first alternate is index one. The
following count-field boundaries leave ten word slots in each array;
these instructions impose no ten-slot bound. Low-word multiplication
and offset addition do not carry into the record segment. Consequently
neither a source count nor the 254-byte record allocation alone proves
that arbitrary alternate lists stay inside their field or record.

### Nested flags and jumpers

Flags converts its count into a local word, starts SI=0 and consumes
that many nested lines under a signed SI<count comparison. A nonpositive
lookup or failed helper fails the record. DspEnabled ORs 01 into word
00DC. MidiNotEnabled ORs 08 there. DspNotEnabled makes no assignment.
Any other positive lookup result also makes no assignment but consumes
one nested line. Thus EndRecord encountered inside this sublist is not
an immediate record return. Nonpositive counts consume no nested lines.

Jumpers similarly uses a local count and signed loop, but accepts only
lookup indices 38 through 45. Its controlled eight-word CS:0A00 table
has these targets and OR masks on word 00DE:

| Tag | Target | OR mask |
| --- | --- | --- |
| MidiAddressJumper | 0899 | 08 |
| MidiIrqJumper | 08A4 | 10 |
| DigitalAddressJumper | 08AF | 20 |
| DigitalIrqJumper | 08BA | 40 |
| DigitalDmaJumper | 08C5 | 80 |
| AddressJumper | 08D1 | 01 |
| IrqJumper | 08DC | 02 |
| DmaJumper | 08E7 | 04 |

Any other nested jumper lookup fails, even if positive. No clearing
operation accompanies these ORs. A spelling absent from the lookup
table cannot become a recognized jumper merely because a source comment
names it. This parser does not validate hardware effects of any mask.

## Interpretation

The complete local record-arm mapping, including consumption of nested
lines and local failure/no-op alternatives, is removed from Q-CONFIG-006's
remaining work. Numeric conversion and capacity admission are distinct:
the arms call the same numeric extractor, whose bounded installed-count
reading is FND-SOUND-025, rather than enforcing per-field numeric ranges.
The five unbounded NUL copies and five count-controlled arrays mean that
Q-SOUND-010 still needs actual source/profile spans and retained record
storage before its conditional label proof can be applied natively.
Source ordering is an observation of the file, not an order enforced by
this outer dispatch. SamplePlaybackMode has a concrete local word store;
its wider consumer effect remains a separate question.

## Alternatives

A syntactically known tag may be accepted, ignored or rejected depending
on the current nested context. Treating every known tag as globally
accepted contradicts these dispatch paths. Assuming DspNotEnabled clears
a flag contradicts its no-op arm. Assuming the array count enforces ten
slots adds a comparison the loop does not make. Actual file delivery,
token/value limits, general numeric failures, field consumers and
heap/alias/retention conditions remain open. This is not a complete native
parser, hardware or complete-reading promotion, and no original was run.

## How to reproduce

Use SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57 with resident_listing.py's fixed load
model. Keep source-derived output only under
GAME_DIR/analysis/disc-adv-survey-20261104/.
Read setup-record-parser.txt from the established entry 1AF6:038E
through its far return at 09FF. Text from 0A00 onward is data decoded
linearly, not more record instructions. Reuse setup-tags.json and
setup-record-jumps.json with their controls/repeats from FND-SOUND-020;
join each outer selector with table index selector minus three, then
read the reached branch and helper argument destinations.

Run report.mjs table twice with setup-jumper-jumps-controls.json. Its
report and repeat compare exactly, SHA-256
4807faf76b3bd0526b48fa563880ff303f1ad41945c8d4f26215bf2f978ee95f.
The eight-entry bound comes from the selector range check; compare all
targets to the OR instructions. As positive controls, CardName reaches
03EE in the outer table and generic AddressJumper reaches 08D1 in the
nested one. Track the signed loop conditions, SI initial values and
both failure and successful EndRecord exits without substituting a host parser.
