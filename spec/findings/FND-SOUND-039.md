---
id: FND-SOUND-039
title: Two fresh setup utility runs return successful parse counts and group pointers with distinct data and stack segments
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py at 4cbc8a86; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private C drive without additional media
---

## Observation

Q-CONFIG-006/011 and Q-SOUND-010 retain native admission beyond the static
readings. This is the first fixed utility-specific capability attempt,
followed by a fresh-process repeat. FND-SOUND-024/033/038 and
FND-EXE-491/572 supply the prior static attempt and documented boundaries.
Only SOUND_DS.EXE's parser entry and return are observed; DSUN.EXE,
gameplay, setup UI and driver loading are not entered.

On 2026-11-05 setup_parser.py admitted only installed SOUND_DS.EXE
(length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57) and SOUND.INI
(length 54669, XXH3-128 aecc7469098e0c188e0e3d056c5fd530). It verified
and copied those files to each new private C drive. RustDosSession used
RUST_DOS_PINNED_SHA256, default muted host sound and the machine run lock,
without other media. The script accepts no target, arguments or addresses.

From program entry it installed declared early-error and driver-loader
guards, then continued to canonical 1AF6:0001. It checked the relocated
DS and incoming far return to 1ABD:0021 before named field reads, then
continued to that return and checked its stop and DS again. Each existing
continuation handle was observed without fixed-time advancement. Neither
run hit an error/driver guard. Both session contexts closed; subsequent
process inspection found no owned emulator still running.

The package records expected/actual emulator digests as equal, host_sound
muted, the configured SB16, no agent input/guest-write operations and no
run failure. Its event_log is null; no missing event/RNG journal is
reconstructed. This utility capability observation supplies no gameplay
rule recording, RNG replay or parity claim. The utility's ordinary
parser/heap writes are separate from agent setup writes.

### Named endpoint fields

Both program-entry stops were 05BA:0000, with DS=05AA and SS=22D0.
The declared parser entry maps to 10B0:0001 and its incoming far return
is 1077:0021. At parser entry all three counts, group pointers, stream
pointer and mode word were zero; AX was 0100. Its far frame matched.

At the declared return both runs observed:

| Named observation | Value |
| --- | --- |
| Parser AX | 0001 |
| MainGroup count | 13 |
| DigitalGroup count | 13 |
| MusicOnlyGroup count | -1 |
| MainGroup pointer | 2415:0004 |
| DigitalGroup pointer | 24E4:0004 |
| MusicOnlyGroup pointer | 25B3:0004 |
| Retained stream pointer | 13F0:DC0C |
| Mode word | 0001 |
| DS / SS at return | 13F0 / 22D0 |

Pointer/register values are hexadecimal; counts use decimal. This matches
the static normal count/pointer continuation of FND-SOUND-024/025/026/033.
In particular the observed negative count is not replaced by zero or
paired with a null group result. The stream pointer identifies the
index-five slot predicted by the static initializer/open reading; its
contents were not followed after parser return.

The complete phase field objects compare equal between fresh runs, as do
the complete returned register objects. Full stop objects and report files
are not byte-identical; no whole-journal or timing equivalence is claimed.
Both private drives retained the exact input identities after the run.

## Interpretation

The utility can start and reach these fixed parser stops with named
field/frame reads under current pins. Its successful return with all
three counts and nonzero group pointers is observed twice, rather than
inferred solely from source shape or allocation arithmetic. DS and SS
are distinct at return: DSUN's startup equality is not a utility invariant.

This removes untested normal parse-return uncertainty from the selected
questions. It does not establish complete record contents, allocation
headers/extents, absence of aliases, intermediate DOS results or buffer
identity through every child. Label allocation, retained vector, chooser,
driver and other error/input paths remain Q-CONFIG-006/011 and Q-SOUND-010.
No spec or parity status is promoted.

## Alternatives

A successful AX and count/pointer endpoint is not byte-for-byte record
verification or a general heap lifetime proof. That needs further bounded
named-field/content observations. Continuing beyond the stop could expose
other allocation/input/driver conditions, which this probe does not admit.
No original content, save, screenshot, raw record or executable is committed.

## How to reproduce

Use the project-local evidence Python and run
`python -I tools/research/rust-dos-runs/setup_parser.py <new-run-name>`.
Names allow letters, digits, underscore and hyphen and must be new. Keep
the two exact licensed inputs, emulator pin and device settings.

Retained attempts are GAME_DIR/captures/rust-dos-setup-parser/
v181-setup-parser-20261105-a and v181-setup-parser-20261105-b. Each keeps
setup-parser.json, the private drive, configuration and package records.
The setup-parser.json SHA-256 values are respectively
6daa0d517c9d79482cd9bc6d91554a7fc6a890f5d7e80ff7e087b6ce8ffdc3e4
and 423ce597dd4ee3e17e9ba36fa2cb1a57c274fb120954ff4c2d8841c1c5431695.
Compare phases and returned registers semantically while preserving each
original stop. No absent event log, draw sequence or elapsed time is
inferred. Input copies can be rechecked with the committed verified_input
helper and INPUTS identities; derived reports remain local.
