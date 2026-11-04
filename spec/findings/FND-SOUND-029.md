---
id: FND-SOUND-029
title: Installed setup record strings and list counts fit the mapped field gaps, with one missing and duplicate primary-key exception
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND.INI
    offset: 0x00000000..0x0000D58D
tool: tools/research/exec-census/sound_ini_profile.py --spans at f3bcd060; Python 3.12.10, xxhash 4.0.1
environment: null
---

## Observation

The installed SOUND.INI is 54669 bytes, XXH3-128
aecc7469098e0c188e0e3d056c5fd530. Its 26 source records comprise thirteen
MainGroup and thirteen DigitalGroup records; MusicOnlyGroup has none
(FND-SOUND-023). This measures source lengths and occurrences against
FND-SOUND-028's local field mapping, without replaying the native parser.
Lengths and counts below are decimal; record offsets are hexadecimal.

Each of the five string fields occurs exactly once in every source record.
Maximum byte requirements include the terminating NUL that the native
copy contract would append:

| Source field | Record offset | Maximum bytes including NUL | Gap to next mapped field |
| --- | --- | ---: | ---: |
| CardName | 0000 | 27 | 40 |
| RmDigitalDriverName | 009A | 12 | 15 |
| RmMidiDriverName | 00A9 | 12 | 15 |
| PmDigitalDriverName | 00B8 | 13 | 15 |
| PmMidiDriverName | 00C7 | 13 | 15 |

Each of the five setting-count keys also occurs exactly once per record.
Their source values are positive integers in these ranges:

| Count field | Minimum | Maximum | Primary and alternate source-key correspondence |
| --- | ---: | ---: | --- |
| NumberOfMidiAddresses | 1 | 9 | All 26 have one primary and count minus one alternates |
| NumberOfDigitalAddresses | 1 | 9 | 25 match; the first MainGroup record has count one and no primary or alternate |
| NumberOfMidiIrqs | 1 | 8 | All 26 match |
| NumberOfIrqs | 1 | 8 | All 26 match |
| NumberOfDmas | 1 | 3 | 25 match; the first MainGroup record has count one, two primary keys and no alternate |

Both exceptions belong to the same record, ordinal zero in the report.
The missing key is DigitalAddress and the repeated key is Dma. The
source has 22 through 45 measured keys per record. No source label,
driver name or numeric setting value is reproduced here.

## Interpretation

The five source string lengths do not require crossing the next mapped
field boundary. If native conversion preserves these count values and
the alternates are admitted in their measured source families, the local
contracts of FND-SOUND-028 need no array index above eight: each mapped
array has ten word slots before the next field. The repeated primary Dma
key uses the same fixed index-zero store, not a new array element.
Source value magnitudes alone do not establish each native numeric
conversion; FND-SOUND-025's bounded count reading remains distinct.

This removes uncertainty about source lengths, count magnitudes and the
missing/repeated-primary pattern. It does not prove the native parser
receives every source line in the same context or that actual record
storage is valid and retained. Under the separate successful group-clear
and record-store conditions, the absent DigitalAddress supplies no own
primary write and the repeated Dma can replace its earlier primary value.
Neither a required-field rejection nor automatic repair follows from the
source: FND-SOUND-028's outer reader has no such checks.

## Alternatives

Treating every count as matched by exactly that many source keys would
erase the two measured exceptions. Treating the measured span envelope
as native setup success would omit byte delivery, nested context/order,
general value conversion, heap/alias/retention and chooser-child obligations.
Those remain Q-CONFIG-006 and Q-SOUND-010. The disc copy and arbitrary
modified source files are outside this exact installed-file measurement.
No runtime, complete-reading or parity status is promoted.

## How to reproduce

Run Python with -I and tools/research/exec-census/sound_ini_profile.py,
passing the approved installed SOUND.INI path followed by --spans. The
reader checks the exact identity and structural profile, then emits field
lengths, declared source values and key occurrences without source strings.
Its numeric/key limits are tool bounds, not native parser limits.

Retain both independent invocations under
GAME_DIR/analysis/disc-adv-survey-20261104/ as
setup-source-record-spans.json and setup-source-record-spans-repeat.json.
The reports compare exactly, SHA-256
0a05cd202f6ecf3826388c7ab8ab892ef2cde443bb8cd449af2c57182d0eea7b.
For each field aggregate every record, keeping missing and repeated keys
visible; compare gaps to FND-SOUND-028, not guessed capacities. No original
program was run or source-derived fixture committed.
