---
id: FND-SOUND-023
title: Installed setup source has two small printable label groups and an empty music-only group
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
tool: tools/research/exec-census/sound_ini_profile.py at b5bd8e13; Python 3.12.10, xxhash 4.0.1
environment: null
---

## Observation

The installed SOUND.INI has 54669 bytes and XXH3-128
aecc7469098e0c188e0e3d056c5fd530. Its declared and observed source record
counts agree. Every observed CardName consists of printable ASCII.
Lengths and footprints below are decimal bytes, not original strings.

| Group | Declared count | Observed records | Maximum name length | 80-byte label slots | Joined labels, LF per label and final NUL |
| --- | ---: | ---: | ---: | ---: | ---: |
| MainGroup | 13 | 13 | 26 | 1040 | 244 |
| DigitalGroup | 13 | 13 | 25 | 1040 | 249 |
| MusicOnlyGroup | -1 | 0 | 0 | 0 | not applicable |

## Interpretation

MainGroup and DigitalGroup satisfy the source count, length and printable
conditions of FND-SOUND-022. Their joined-text footprints fit its 1024-byte
wrapper buffer if the native producers preserve those names and counts.
The 1040-byte figures describe separate label arrays, not that joined buffer.
MusicOnlyGroup does not satisfy the positive-count condition: no first label
exists in this source group. This makes its possible separate-choice route
a distinct admission obligation, not a successful zero-choice case.

## Alternatives

This removes uncertainty about the installed source's label profile; it
does not remove native group/count parsing, formatting, allocation,
retention, vector or child/frame obligations in Q-SOUND-010 and Q-CONFIG-006.
One reading admits the two positive groups once those conditions are
proved. A competing reading permits producer/state changes before the
chooser, which this text measurement cannot exclude. The empty music-only
source cannot be passed through the positive-list proof. FND-SOUND-021's
forced combined route is conditional on retained header state; the source
measurement alone does not prove the separate route unreachable.
No native setup process was executed and no parity status is promoted.
The disc copy is outside this exact installed-file measurement.

## How to reproduce

Run Python with -I and tools/research/exec-census/sound_ini_profile.py,
passing the approved installed SOUND.INI path. The reader checks the exact
identity before parsing and reports metadata only. Synthetic controls
cover boundaries and rejected inputs.
Its caps are tooling limits, not native parser limits.

Two independent invocations are retained under
GAME_DIR/analysis/disc-adv-survey-20261104/ as
setup-source-label-profile.json and setup-source-label-profile-repeat.json.
The UTF-8 reports compare exactly; SHA-256 is
624d7373a0f9d19f6002b6b56499ba596345a00b4e1be5789023548ac1494d9c.
No label text or licensed fixture is committed.
