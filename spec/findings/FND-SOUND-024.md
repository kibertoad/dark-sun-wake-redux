---
id: FND-SOUND-024
title: Setup group counts drive low-word allocations and a tag counter rather than a parsed-record equality check
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
    address: 1AF6:026F..1AF6:038E
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0B26..1AF6:0BB3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0C8F..1AF6:0CDE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 18F7:0003..18F7:0072
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 18F7:0126..18F7:0180
tool: tools/research/exec-census/resident_listing.py at b5bd8e13; Python 3.12.10, Capstone 5.0.7; retained report.mjs tag table from FND-SOUND-020
environment: null
---

## Observation

This answers the local group/count arm of Q-CONFIG-006 and the negative-count
allocation alternative of Q-SOUND-010. Hexadecimal notation identifies code
and fields; allocation sizes and counters below are decimal unless prefixed.
All child success/preservation conditions remain explicit.

The retained controlled eighty-slot tag report of FND-SOUND-020 gives
NumberOfRecords index 1, StartRecord 2 and EndOfGroup 25. Group reader
1AF6:026F recognizes exactly those positive lookup results. Other results,
failed line/tag reads and failed record reads return a zero far pointer.
Its local declared count, tag counter SI, record counter DI and return
pointer start at zero. It attempts another tag while signed SI is at most
the declared count; each completed count or record arm increments SI.

NumberOfRecords calls 1AF6:0B26 with the address of its local count.
If that helper returns nonzero, the count is copied to DS:EC74 for group
selector DS:D8FC=1, DS:EC76 for selector 2, or DS:EC78 for selector 4.
The reader multiplies the signed count by 254 and passes only the low word
to 1000:1BD6. A zero returned far pointer fails; a nonzero pointer is
retained and passed with zero and the same low-word length to 1000:3133.
There is no local negative-count rejection before these calls.

StartRecord passes the retained pointer's segment and its offset plus the
low word of DI*254 to 1AF6:038E. A nonzero result increments DI. EndOfGroup
returns the retained pointer immediately, without comparing DI to the
declared count. Exiting the signed SI comparison also returns that pointer.
Thus the loop bound is not an equality check against successfully parsed
records. For one successful count tag returning 13 followed by thirteen
successful record tags, SI reaches 14 and exits before consuming EndOfGroup.
The outer reader recognizes EndOfGroup separately and continues.

The outer MusicOnlyGroup arm stores the group reader's far result at
DS:1890 and tests only whether it is null. It has no special local -1 bypass.
If the numeric helper returns -1, the group arm stores that count, requests
65282 bytes (0xFF02, the low word of -254), and, if allocation and clearing
return normally preserving loop state, exits with SI=1 because 1 > -1.
It does not read a record or consume EndOfGroup on that continuation.

The separate label allocator 18F7:0003 also has no local negative-count
bypass: it requests the low word of each count times 80, in Digital,
MusicOnly, Main order, failing on a null result. For a retained music-only
count of -1 that request is 65456 bytes (0xFFB0), not zero. The music-only
label builder 18F7:0126 starts SI=0 and enters its body only while signed
SI < DS:EC78. It therefore produces no label for a retained count of -1.

## Interpretation

The source-level -1 in FND-SOUND-023 cannot be interpreted as a native
zero-allocation shortcut from these callers. Conditional on native numeric
conversion producing -1, the group and label allocation requests are large
low-word values, while the signed label loop produces no first label.
That removes the caller-level special-sentinel alternative; allocator
semantics, actual continuation and chooser reachability remain unproved.
The positive source profile likewise does not prove exact native record
cardinality: the reader's local tag counter is distinct from DI.

## Alternatives

The numeric extractor accepts a token beginning at a classified digit or
minus and copies digits, minus and x/X to a local string. Its conversion
helper 1AF6:0C8F chooses a format based on an initial 0x/0X, calls
1000:39C4, ignores that child's return and reads its local output word.
This reading does not establish the format strings, conversion child,
classification-table retention or all token bounds. Consequently source -1
is not asserted to become native -1 here. A different conversion result,
an allocation failure or later state change remains an alternative to the
conditional continuation above. No native failure, display, corruption or
successful chooser outcome is inferred. General repeated-count, malformed
record and file/error behavior remains Q-CONFIG-006; row/storage/child
admission remains Q-SOUND-010. No complete-reading promotion is claimed.

## How to reproduce

Use SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57 and the fixed 1000 load model of
resident_listing.py. Retain output only under
GAME_DIR/analysis/disc-adv-survey-20261104/. setup-open.txt,
setup-group-parser.txt, setup-value-helpers.txt and
setup-choice-label-preparation.txt are the earlier retained listings.
Regenerate 1AF6:026F..1AF6:038E and 18F7:0126..18F7:0180 into
setup-group-count-and-empty-label.txt; the former starts at the actual
entry, avoiding the earlier header-table linear decode. The conversion
entry through its far return is 1AF6:0C8F..1AF6:0CDE in
setup-number-conversion.txt; later listing text is outside this claim.
Reuse setup-tags.json and its controls/repeat from FND-SOUND-020 rather
than rebuilding the unchanged tag table. Compare the three lookup indices,
every group-reader branch and the two signed loop predicates. The request
sizes above follow 16-bit low-word multiplication, not a runtime trace.
