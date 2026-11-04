---
id: FND-SOUND-020
title: Setup driver-name tags populate inline fields and the selected names have a concrete first-loader pointer binding
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:038E..1AF6:0A00
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0A74..1AF6:0B26
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0BB3..1AF6:0C8F
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0EC9..1000:0EF5
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3A4A..1000:3AA3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3AE4..1000:3B03
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1429:042D..1429:063A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 13D0:0005..13D0:0152
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x00010364..0x00010FE4
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000CD70..0x0000CDD4
tool: tools/research/exec-census/resident_listing.py at 05d904c6 and tools/evidence/report.mjs table; Python 3.12.10; Capstone 5.0.7; Node.js 24.15.0
environment: null
---

## Observation

Q-SOUND-007 retains the name producers behind FND-SOUND-019's inline
settings fields. Q-CONFIG-006 owns the wider SOUND.INI parser. This binds
the four name tags to their actual record stores and reads the selected
record's two output-copy alternatives and one concrete initial loader binding.
It does not establish which UI selection supplies that record or preservation
through later driver children.

### Tag identity and record dispatch

The tag extractor at 1AF6:0BB3 writes the tag text to DS:EA56, leaving
the bracketed value separate. Its character helper 1000:0EC9 returns
minus one for input minus one; otherwise it zero-extends the low character
byte and subtracts 32 when classification bit eight at DS:DA9B is set.
The fingerprinted shipped table sets that bit exactly for ASCII a through z.
With that table retained, ordinary ASCII tag matching is case-insensitive.

1AF6:0A74 scans eighty forty-byte slots at DS:0C04. It skips empty strings
and compares the remaining strings with DS:EA56 through 1000:3A4A, returning
the matching slot index or zero. All eighty shipped slots were read in
full; 54 are nonempty, at indices one through 54, and every slot has a NUL
within its forty bytes. The string-length and compare helpers explicitly
clear direction and scan NUL-terminated storage; valid spans and retained
segments are prerequisites, not a runtime table-writer census.

The record parser at 1AF6:038E calls the line reader, tag extractor and
lookup. It rejects a nonpositive lookup result. For positive results it
subtracts three, admits unsigned indices zero through 49, doubles that
index and jumps through the fifty-word table at CS:0A10. The following
bindings come from that table and the reached branch instructions, not
from the ordering of SOUND.INI lines:

| Original key | Lookup index | Branch | Destination relative to supplied record |
| --- | ---: | --- | --- |
| RmMidiDriverName | 20 | +0704 | +00A9 |
| RmDigitalDriverName | 21 | +072C | +009A |
| PmMidiDriverName | 51 | +0754 | +00C7 |
| PmDigitalDriverName | 52 | +077C | +00B8 |
| MidiDriverChunkNumber | 26 | +07A4 | word +00D8 |
| DigitalDriverChunkNumber | 27 | +07BF | word +00D6 |

Independent controls are EndRecord at lookup index three, branching to
+03EB and then success at +09F4, and CardName at index five, branching to
+03EE. The number paths use 1AF6:0B26; its complete numeric conversion
and malformed/count behavior are not established here.

### Name value and copy contract

Each of the four name branches calls 1AF6:0AC1. It scans the current line
past the tag's closing bracket, finds the value's opening bracket, copies
until the closing bracket to DS:EA06 and appends NUL. Missing delimiters
take its zero return after the scan passes the recorded line length;
the name branch then takes the record parser's failure path. A closing
bracket immediately after the value opener yields a successful empty name.

On success the branch supplies the record's far segment/offset plus its
listed field offset as destination to 1000:3A7A, with DS:EA06 as source.
That helper explicitly clears direction, scans the source for NUL with
initial CX FFFF and copies through that terminator. It saves/restores DS
and returns the destination pointer. It does not receive a destination
capacity. Thus the observed adjacent fifteen-byte name slots do not impose
a copy cap. Valid termination, nonwrapping readable/writable spans, frame
and source/destination identity remain conditions; no malformed or oversized
input was constructed and no native overwrite is claimed.

The parser loops back for the next line after a successful name copy.
An unrecognized/unsupported tag, failed line/tag read or failed value
extraction takes its zero return; there is no whole-record rollback in
that local failure tail. Earlier field writes remain unless some later
caller replaces or frees the record. This is the local name-path result,
not a complete statement about every group/file failure or other field arm.

### Selected record to output names

The complete local 1429:042D..063A body has two settings-source branches
according to record word +00EC. Both choose names the same way: when
DS:ECCC equals one, they copy record +00A9 to DS:E031 and record +009A
to DS:E03F. Other mode-word values copy +00C7 and +00B8 instead. Each
copy uses 1000:3A7A and neither result is used as a success condition.
FND-CONFIG-213's 59-byte output buffer begins at DS:E01B, so these
destinations are its inline fields +16 and +24 hex, matching the corrected
consumer offsets of FND-SOUND-019 only when that consumer's current-record
pointer is bound to this buffer.

The same body copies record words +00D6 and +00D8 to DS:E051 and DS:E053,
output offsets +36 and +38 hex, and record +002A to output +32. The
parser's CardGroup tag reaches +042B and the numeric destination +002A;
this binds its stored value without deciding the later device effect of
the game-side comparison. FND-CONFIG-214's previously recorded output
stores remain unchanged. Pointer lifetime, successful file write and
subsequent resource selection remain independent.

### Initial loader pointer has an explicit producer

The complete local 13D0:0005 routine passes DS:E01B to 13D0:00AA, which
forwards that far argument to 13D0:00D7. The latter copies the argument
segment and offset words to DS:E019 and DS:E017 respectively at +00E3
and +00E6, then calls 13D0:00FB immediately. No child separates those
stores from that call. With the declared DS and disjoint valid argument/
stack/global storage, this supplies a concrete current-pointer producer.

The +00FB prefix before its first filename helper has no child or own
current-pointer write. If the settings word +14 hex mask eight is clear,
it forms the name at low-word current offset plus 16 hex, retaining the
current segment, and calls 13D0:0427 at +014F. On this admitted path the
name therefore comes from DS:E031, the first output-name destination
above. If that mask is set, the first file request is skipped. This proves
the binding at that first helper entry, not a successful file open, install
or later digital call. Other callers of the pointer setter, intervening
driver/timer effects and aliases still need their own admission. The
00AA routine's later delay happens after the 00D7 call returns and cannot
be used as evidence of earlier or later preservation.

## Interpretation

Q-SOUND-007 no longer needs the local tag-to-name writer mapping or the
selected record's mode-controlled output-name copies or the initial
DS:E01B pointer producer and conditional first-name binding. It retains
which record is passed, mode/other pointer producers, aliases and
preservation through later driver use. Q-CONFIG-006 retains the remaining
numeric/group/count arms, full file/read/error behavior, other writers and
complete effects of its settings fields. No actual setup selection, driver
load, device output, complete-reading or parity validation is claimed.

## Alternatives

Inferring real/protected-mode choice from the file order ignores the mode
test. Treating these fields as far pointers skips the inline string copies.
Treating a parser failure as undoing preceding name stores adds rollback
absent from its tail. Treating the demonstrated initial pointer binding as
universal would omit other incoming setters and later child effects.

## How to reproduce

Use SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. The private directory
GAME_DIR/analysis/disc-adv-survey-20261104/ retains setup-record-parser,
setup-value-helpers, setup-tokenizer, setup-string-primitives and
setup-selected-record listings produced by resident_listing.py for the
locations above. Bytes +0A00..0A74 are data tables, not instructions;
the linear listing there is diagnostic only.
setup-current-binding-entry.txt and setup-current-binding-helper.txt retain
the initial argument forwarding and explicit pointer stores; use
setup-name-consumers.txt of FND-SOUND-019 for the conditional first helper.

Run report.mjs table twice for eighty forty-byte tag slots from file
10364 hex, reading ten doublewords per slot, and fifty target words from
CD70 hex. Bounds come from +0A74's eighty-slot loop and +03D6's
subtract-three/unsigned-49 dispatch gate. Read the 256 one-byte case
classifications at file 1D1FB hex; the low-byte zero extension bounds them.
The setup-tags, setup-record-jumps and setup-case controls/reports/repeats
remain private. Their SHA-256 values are respectively
55de7ad6cab4369b6273768987426399721490192be1fbff5a7ff2672bebeaa6,
1f101ef80e47b402e48b9e4cd8cf79a84ae1d8c4da400470e46aa248dee78ce1 and
1ceb2e4574f31a486f808054091bc4c1afcbea6772b1da72ee8850a2c5e4d1fc.
Every report repeats byte for byte. Keep source-derived tables, names and
code listings in GAME_DIR; no original program is executed or committed.
