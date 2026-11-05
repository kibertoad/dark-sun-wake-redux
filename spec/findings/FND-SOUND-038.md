---
id: FND-SOUND-038
title: All installed setup records fit the counted source bands, including two consumed unrecognized jumper keys
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
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:026F..1AF6:0A00
tool: tools/research/exec-census/sound_ini_profile.py --contexts at 595c7602; Python 3.12.10, xxhash 4.0.1; composition with FND-SOUND-024/034/037
environment: null
---

## Observation

This measures the exact installed source's remaining record-context
obligation in Q-CONFIG-006. The first stricter tool profile and its
excluded unknown jumper are retained in FND-SOUND-037. The corrected
profile explicitly admits that one proven table-absent spelling as a
consumed no-assignment jumper; it does not admit arbitrary unknown tags.
The source remains unchanged, length 54669, XXH3-128
aecc7469098e0c188e0e3d056c5fd530. Counts below are decimal.

### Complete installed record-key accounting

All 26 source records pass the declared bounded consecutive-key profile.
There are thirteen MainGroup and thirteen DigitalGroup records and none
in MusicOnlyGroup, matching FND-SOUND-023/029. Terminal LF, line/control
and source-ignored-bracket conditions are the measured ones of
FND-SOUND-034. The new profile rejects a mismatch rather than silently
continuing or labeling unsupported input as native success.

The record interiors contain 940 source keys, partitioned as follows:

| Consumption role | Keys |
| --- | ---: |
| Outer record keys, including each first alternate and each Flags/Jumpers count key | 649 |
| Nested flag keys | 27 |
| Nested jumper keys | 66 |
| Further alternate keys consumed after the first alternate | 198 |
| Total | 940 |

StartRecord and EndRecord boundaries are separate from this interior
count. First alternate keys belong to the outer dispatch; only the
subsequent count-minus-two keys belong to their nested loop. The report
contains 155 bands, including zero-child bands. Its complete band totals
and largest additional-child counts are:

| Band | Bands | Nested child keys | Maximum children in one band |
| --- | ---: | ---: | ---: |
| Flags | 26 | 27 | 2 |
| Jumpers | 26 | 66 | 4 |
| NumberOfMidiAddresses | 23 | 53 | 7 |
| NumberOfDigitalAddresses | 19 | 43 | 7 |
| NumberOfMidiIrqs | 22 | 44 | 6 |
| NumberOfIrqs | 23 | 46 | 6 |
| NumberOfDmas | 16 | 12 | 1 |

Every encountered count is a short positive decimal for alternate
families, or a nonnegative short decimal for Flags/Jumpers. Every
alternate band has its earlier corresponding count and exactly that
count minus one consecutive alternate source keys, including its first
outer key. No additional same-family band or duplicate array-count key
passes this profile. Count-one families have no alternate band. These
are source-profile observations, not required-field rules added to the
native parser. Missing/repeated primary-key exceptions remain exactly
FND-SOUND-029's missing DigitalAddress and duplicate Dma.

### Ignored nested jumper occurrences

Two of the 66 jumper children are MidiDmaJumper, once at global record
ordinal eleven in MainGroup and once at global ordinal twenty-three in
DigitalGroup. Ordinals here count all records from zero, as in the
report; the latter is local DigitalGroup ordinal ten. The other 64
children use one of the eight recognized jumper names. Every flag child
belongs to the profile's five known positive flag names, so neither an
unknown/nonpositive flag nor a positive non-jumper is hidden in these
bands. FND-SOUND-037 supplies the actual lookup and contrasting failure
gates; the profiler supplies source positions and count conformance.

Under intact line/tag/table/record state, each unrecognized jumper
therefore consumes one declared child without ORing a jumper bit. Its
absence from the native table does not shift the next source key into
the wrong context. The earlier strict profiler's rejection must not be
treated as a native record failure or as a successful complete report.

### Composition with the group and numeric readers

Given those source deliveries, retained count words and valid record
storage, the counted flag/jumper/alternate loops of FND-SOUND-037 consume
exactly their measured nested keys and leave each following outer key
and EndRecord in its expected context. FND-SOUND-034 retains the thirty-two
alternate numeric values truncated before successful scanning; conformance
does not restore their source magnitudes or change their assignments.

Each positive group has one declared count 13 before its thirteen
StartRecord boundaries under the existing structural admission. If its
record calls succeed and loop state is preserved, FND-SOUND-024's tag
counter advances once for the count and once for each record, reaching
14 and leaving EndOfGroup for the outer reader. Its record counter
separately reaches 13. The negative-count empty group's allocation and
early-exit conditions remain FND-SOUND-024/026; this source accounting
does not convert that branch to a zero-allocation success.

## Interpretation

Exact installed nested-key consumption and the positive group-counter
composition are removed from Q-CONFIG-006's remaining source uncertainty.
All source records are accounted for by this bounded profile, including
the two unknown-but-consumed jumper children and the separate numeric
truncation exceptions. Native source delivery, heap/storage identities,
aliases, intervening writers and child preservation remain conditions,
not results supplied by a source validator.

## Alternatives

Counting the first alternate twice would give the wrong outer/nested
partition. Treating an ignored jumper as a failed line or a missing
consumed child contradicts the native loop. Treating passing source
conformance as actual setup success would omit native stream, record,
heap, caller and chooser admission. Those remain Q-CONFIG-006/011 and
Q-SOUND-010. Other input profiles and outer error/close children also
remain open. No original process, complete reading or parity validation
is claimed from this static source measurement.

## How to reproduce

Use Python -I with committed sound_ini_profile.py, approved installed
SOUND.INI and --contexts. The reader verifies the exact size/hash and
source boundaries, then checks the restricted source bands. Retain
setup-source-contexts.json and setup-source-contexts-repeat.json under
GAME_DIR/analysis/disc-adv-survey-20261104/. Their SHA-256 is
7cf751057e97e9a643b408ab02a9e1945df7b7a02e313e4cf50e856cfba8f39c.
Sum each record's sourceKeys, outerKeys, flagKeys, jumperKeys and
alternateKeys independently; sourceKeys must equal the latter four.
Group bands by kind, retaining zero-child bands, and sum ignoredJumperKeys
separately from the total consumed jumper children. Check all per-record
line positions rather than extrapolating the first eleven records.

Reuse FND-SOUND-037's native dispatch/lookup tables and corrected branch
reading, FND-SOUND-034's numeric/line profile and FND-SOUND-024's retained
group-counter listing. The source-conformance tool is bounded to its
documented profiles and is not a native parser or allocation model.
Canonical original files, reports and earlier exception diagnostics stay
in the approved local store; no source text or record fixture is committed.
