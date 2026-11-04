---
id: FND-SOUND-021
title: Setup header flags select combined or separate card records while list-choice admission remains a distinct dependency
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0001..1AF6:0171
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0189..1AF6:024F
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:026F..1AF6:038E
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1429:0262..1429:042D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1429:063A..1429:0799
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:01B0..17FB:023A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:0346..17FB:03D3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 18F7:0003..18F7:0180
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 19CF:000D..19CF:0109
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 19CF:0AD2..19CF:0ADD
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1ABD:000A..1ABD:0264
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1BD4:00A9..1BD4:0244
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000C4D1..0x0000C4E9
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000C5AF..0x0000C5CF
tool: tools/research/exec-census/resident_listing.py at 05d904c6; tools/evidence/report.mjs table/incoming; ReportPhysicalBytePattern.ps1; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0
environment: null
---

## Observation

FND-SOUND-020 binds parsed name fields to settings copies and one initial
loader pointer. This settles the local header actions and three record-source
routes behind those copies. Dialog row/index and child-state admission are
kept apart rather than inferred from a caller multiplying an index by 254.
Addresses, offsets and flag masks below are hexadecimal; counts and lookup
indices are decimal.

### Header actions and mode are separate

The complete local header parser 1AF6:0189..024F reads a line, extracts its
tag and uses FND-SOUND-020's lookup. Nonpositive lookup, failed reads/value
extraction and a tag absent from its eight-key dispatch return zero. Its
bounded value/target table gives these actions:

| Original header key | Local action |
| --- | --- |
| CdEnabled | OR word DS:ECCA with 02. |
| MultipleInstallDisabled | OR that word with 10. |
| MidiNotEnabled | OR that word with 08. |
| EndOfHeader | Return one if DS:ECCC is nonzero, zero otherwise. |
| GameName | Extract a bracketed value and copy it to DS:EC7A. |
| DriverPath | Extract a bracketed value and copy it to DS:EB7E. |
| ProtectedMode | Continue parsing; no own field write. |
| RealMode | Continue parsing; no own field write. |

The two value paths use the same bracket reader and NUL-copy helper as
FND-SOUND-020. Flags are ORed into prior state, not cleared here. A later
failure does not undo earlier flag or string writes. This is a complete
local header action graph with the reader/copy/storage conditions retained,
not a complete file parser or I/O proof.

The file-opening routine first assigns one to DS:ECCC. A whole-file exact
CC EC byte-pair scan returns six candidates: that assignment, the header
nonzero test and the four equality-to-one tests in the two settings-copy
routines. Entry-based readings classify all six as those instructions.
Neither recognized mode tag switches the word. This bounds exact literal
references only; overlapping wider, computed or bulk writes and effects
of children remain outside that search.

The fingerprinted installed SOUND.INI header contains CdEnabled,
MultipleInstallDisabled and MidiNotEnabled. With its parsed flags retained,
the application route below takes the combined-card selection. No live
selection or guest flag value was observed.

### Group and display-row bindings

The six-key top-level dispatch binds MainGroup, MusicOnlyGroup and
DigitalGroup to the following distinct storage. The group reader uses
254-byte parsed records; the display-label preparation uses 80-byte slots.

| Group | Record far pointer | Signed count word | Label far pointer |
| --- | --- | --- | --- |
| MainGroup | DS:188C | DS:EC74 | DS:18A0 |
| MusicOnlyGroup | DS:1890 | DS:EC78 | DS:189C |
| DigitalGroup | DS:1894 | DS:EC76 | DS:1898 |

18F7:0003 allocates each label array from the low word of count times 80,
checks the returned far pointer for zero and stops with zero on a failed
allocation; earlier successful allocations are not released in that tail.
Its three preparation loops format record n into a local 80-byte buffer
then copy that text to label slot n, under signed n<count. The caller ignores
those preparation routines' register residue. There is no local index
filter or reorder. Allocation, formatter bounds, termination, actual group
counts/bases, aliases and preservation remain admitted inputs, not facts
established by matching these strides.

### Combined and separate selection

In 1ABD:000A the file reader and label preparation precede the choice routes.
Word DS:ECCA mask 10 skips the separate-card prompt and calls 1429:0262.
When that mask is clear, a prompt result of zero calls 1429:02F2; nonzero
calls 1429:0262. The prompt's input/device and returned state remain child
contracts. This is the exact local branch, not an inferred UI caption.

The combined routine 1429:0262 obtains an initial index from the MainGroup
card-number lookup, or zero when the current number is zero. It calls
19CF:000D with the MainGroup count and labels. A result exactly FFFF skips
its initial settings copy; any other result n forms the record pointer
with low-word n*254 added to the MainGroup base offset, retaining its
segment. It passes that pointer to 1429:077C, which calls +042D and then
+063A with the same argument. Thus the two settings writers have a concrete
same-card source route. The caller does not add a general n<count check.

1429:02F2 similarly selects from MusicOnlyGroup and calls +042D for that
record, then selects from DigitalGroup and calls +063A. Each initial copy
is skipped only for exact FFFF. Later settings adjustment calls through
14A2:0087 follow a separate +0006 decision and recompute a record from
the saved index without a second local FFFF test. Their admission and
effects are not inferred from the initial-copy guard.

The complete local +063A body changes the second inline output name at
DS:E03F: record +009A for mode one, +00B8 otherwise, in both of its
settings-source branches. Its explicit stores do not replace the first
name or the music-resource number at DS:E053. Copy lengths, aliases and
callee effects still condition preservation. FND-SOUND-020 already reads
+042D's corresponding two-name and chunk-number copies.

### Initial indices, fallback and dialog limits

The three card-number lookup loops start at zero, scan their own signed
group count and return the first matching record +0028 index, or zero
when none matches. For a retained positive count and valid records, that
initial value is in range. For a nonpositive count they still return zero;
that is not an admitted element of an empty group.

The error handler 1BD4:00A9 tests MainGroup membership of card number 108.
If absent, it finds number 113's index and calls the combined writer on
that MainGroup record. It then tests membership of 108 again; if present,
it finds 108's index and calls the same combined writer. The repeated test
is not replaced by an assumed if/else under mutable state. A failed index
lookup supplies zero, not a not-found sentinel. The later handler requests
the existing settings-file writer and the exit path. No native fallback
was invoked, and successful writes/device initialization are not claimed.

19CF:000D does not validate a positive count before copying the first label
into its 1024-byte local buffer. It appends the remaining counted labels
and separator text, then forwards the built string and initial index to
+009A. That dialog clamps a negative initial index to zero at entry but
has no upper-bound clamp there. Its normal return is the current index;
a separate layout-rejection return is FFFF. Its row construction, navigation,
completion-key child and state preservation must establish the actual
returned-index contract. In particular the row-count adjustment at
+022B..0258 compares rows through 1000:3A4A with a far 0000:0000 argument;
that helper does not supply a null-string bypass. The actual storage read
there is a separate evidence requirement, not an empty-string assumption.
These observations do not demonstrate an invalid native index or a failure.

## Interpretation

Q-SOUND-007 no longer needs the local combined/separate group-to-writer
mapping or the header's flag/mode-tag actions. Actual choice-row/index
admission is independently Q-SOUND-010. Remaining setters/callers, mode and
pointer preservation across device children stay Q-SOUND-007; group/count
and full file/error parsing remain Q-CONFIG-006. No complete-reading,
hardware, successful load, parity or bug claim follows.

## Alternatives

Treating the two mode tags as assignments contradicts their no-op branches.
Treating label and record strides as equal would select different storage.
Treating exact FFFF rejection as a full range check omits other values.
Treating a well-formed source count as the dialog's returned-index bound
skips its string/row construction and children. Treating the nullable-string
comparison as a literal empty string would substitute unsupported memory.

## How to reproduce

Use approved SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57 and installed SOUND.INI length 54669,
XXH3-128 aecc7469098e0c188e0e3d056c5fd530. The new resident listings
and bounded configs/reports remain under
GAME_DIR/analysis/disc-adv-survey-20261104/ with setup-selection-caller,
setup-main-choice-route, setup-selection-controls, setup-choice-wrapper,
setup-choice-dialog, setup-initial-choice, setup-failure-record/continuation,
setup-choice-label-preparation and setup-music-label-preparation stems.
The setup-choice-labels listing is hardware-preselection code, not the
label builder; do not use its filename as a semantic identification.

The setup-top-dispatch table reads six value words and their six targets
from file C4D1; the setup-header-dispatch reads eight and eight from C5AF.
Counts come from the corresponding bounded value scans. Repeated report
SHA-256 values are 2483e9a7d416ca7a6fa6341d101dc277169dc03b9ebac4d08538c56df467b83f
and 5619185412437439a79e5326361f82c0bc4d969e799f390a04f68f1437420d8c.
Use the existing full tag-slot report of FND-SOUND-020 to name their keys.

The three setup-selected/both-writers/separate-choice incoming queries use
the independently read far call at file 5285 as control, resolving to DEA5.
Their declared-relocation far-call candidate counts are zero, two and one,
respectively; all are untruncated without unresolved candidates. Confirm
the positive sites in the entry-based listings. They exclude near/computed
calls and unrelocated pointers and cannot prove complete callers. The
six mode-reference candidates are retained in setup-mode-candidates.txt.
No original program is executed; original tables/listings stay outside Git.
