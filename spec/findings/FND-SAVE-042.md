---
id: FND-SAVE-042
title: A successful reused-open Save copies unprepared working metadata and later Quit updates only the working file
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: failed_save.py and retry_metadata.py at 0b88cb7d; shutdown_run.py at c8d925e3; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic15000 cycles; SB16 base220 IRQ5 DMA1; no Ultrasound; muted host output
---

## Observation

This answers Q-SAVE-008 for RULE-SAVE-002. FND-SAVE-037 reads the
ownership-zero close bypass before a numbered copy; FND-SAVE-035 and
FND-COMBAT-110/113/124 supply the open flag, copy and preparation
contracts. FND-SAVE-041 supplies an actual still-open pending working
record after a controlled failed Save. This observation retries Save
without applying another fault, then follows ordinary Quit.

### Admitted preexisting-open state and successful retry

On 2026-11-06 the fixed observer restored the checked failed-map from
v181-q009-native-write-failure-c-20261106. Executable, mapping, stopped
instruction/time, map state/frame and four private file states matched.
The working reuse flag was1, logical working handle10 and record8161:0004
retained native handle10, options2, runtime flags0 and pending byte1.
FND-SAVE-041 binds that record to the native open of C:\darkrun.GFF.
SAVE02.SAV was absent. The observer delivered normal F1 and Enter;
both completed. It acquired no host file lock and seeded no guest field.

The first SAVE/1 payload wrote its full11840-byte count with carry clear.
Its resource writer returned AX0/error0 while the same record remained
pending1. The numbered writer's saved success byte was1, again with that
record pending1. A fixed watch recorded no pending-byte value change
through this retry. No new working-file open was observed.

At the actual file-copy helper entry3AFE:0000, the checked return frame
identified the Save wrapper and its saved ownership byte was0. The
working record still matched the source record. Its metadata differed
from the working file's on-disk metadata:

| Domain at copy entry | Directory offset | Directory size |
| --- | --- | --- |
| Live working header and directory | 37108 | 800 |
| DARKRUN.GFF on disk | 959 | 32 |

The bounded metadata reader hashed the header and directory bytes
separately; both domains differed. This reader checks header identity
and extents but does not validate directory records or infer a valid
old snapshot from the old extent. In particular, an unchanged header
need not mean its referenced bytes were preserved by new payload writes.

### Completed numbered copy and return

Eight hundred forty-four bounded observations reached the checked input-map
return and retry-map checkpoint. Save returned at614A:0BF2. All362
monitored native write requests had recorded carry-clear results. There
were no RNG draws or guest setup writes. Twenty-nine preparation entries
belonged to662E:0004,6855:0004 and81B4:0004; none belonged to the
working record8161:0004.

DARKRUN.GFF and SAVE02.SAV were each37108 bytes with identical SHA-256
`c422a8c1f196710850d73f879f1159bab8c6095b2c7c77741aa65846a7ff997f`.
Both retained the on-disk offset959/size32 metadata, with equal header
and directory hashes matching the copy-entry disk observation. The live
working record still held offset37108/size800 and pending1. Thus a
successful numbered writer and completed copy did not first prepare
this working record's current header/directory.

CHARSAVE.GFF retained length11735 but changed hash to
`4c8b19bb4515bb2a258a0008dce0b2b2e774ccdb61dc03168e150797dc507ee2`.
RGN032.GFF retained its prior length/hash. These observations distinguish
the numbered copy from the subsequent character-archive work; they do
not declare a valid or loadable numbered snapshot.

### Later ordinary Quit prepares only the working metadata

A second owned process restored retry-map with equal instruction/time,
state/frame and four file states. Normal F3/Q deliveries completed;
ordinary exit status and both flags were0, with no draws or guest writes.
The same working record entered close-all preparation pending1. Native
handle10 wrote28 bytes from8162:0004, returning AX001C/carry clear,
then800 bytes from8164:0005, returning AX0320/carry clear. AH68 on
handle10 returned AX6820/carry clear immediately after the interrupt.
The paired preparation return was AX0/pending0 with handle10 unchanged.
The fixed watch recorded one1-to0 change at2DB6:09BC. Five other
preparations entered clean. Forty-seven observations reached05BA:01A5
before executing DOS termination.

DARKRUN.GFF then held37908 bytes and SHA-256
`262ea0d0a6bf323c055e7869ae8ef6b5551101d99ebcd00ad85d98a7475d5a0b`.
Its header and directory hashes now exactly matched the retry's live
working metadata, including offset37108/size800. SAVE02.SAV retained
its37108-byte length, full-file hash and offset959/size32 metadata.
CHARSAVE.GFF and RGN032.GFF also retained their pre-Quit states.
Both owned sessions closed; their emulators were absent and machine
run locks released afterward.

## Interpretation

The reused-open branch can successfully copy the working file before its
current pending metadata is prepared. In this concrete history it copies
the old on-disk header/extent rather than the live directory; later
ordinary Quit prepares the working file but does not update the already
copied numbered file. This answers Q-SAVE-008's state, copy, preparation
and later-close conditions. It does not establish that the old extent
still describes a coherent old directory, that this numbered save loads,
physical durability, all possible Save histories or a complete reading.
RULE-SAVE-002 remains supported; parity is unchanged.

## Alternatives

If a mandatory intervening preparation always preceded the ownership-zero
copy, the two metadata domains would agree at that boundary. They did
not, and no working-record preparation or pending-byte change occurred
before the completed copy and return. A later close repairing the
numbered copy would change its file state; the observed Quit changed only
the working metadata. The initial controlled failure is part of this
admitted history, not evidence that every ordinary Save takes this path.

## How to reproduce

Use the pinned evidence Python and the scripts at the revisions above:

1. `failed_save.py v181-q009-native-write-failure-c-20261106 <new-retry-run> retry`.
2. `shutdown_run.py <new-retry-run> <new-quit-run> retried-save`.

Retained: GAME_DIR/captures/rust-dos-save-retry/
v181-q008-reused-save-b-20261106/retry.json and states/retry-map;
GAME_DIR/captures/rust-dos-shutdown/
v181-q008-reused-save-quit-20261106/shutdown.json. Both roots contain
session.json, emulator configuration/logs and private files. No exported
request/event journal is claimed. Require the success flags, source/restore
guards, zero guest writes, ownership-zero copy, same pending record,
metadata comparisons and later native results above. The earlier
v181-q008-reused-save-20261106 attempt stopped at copy entry because its
new reader exceeded the existing single-read bound; it supplies no
completed-copy result. Keep all original-derived reports and files local.
