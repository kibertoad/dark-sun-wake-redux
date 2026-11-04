---
id: FND-COMBAT-141
title: A native saved-game source normalizes, opens and repopulates the truncated working file before copy returns one
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py at 7fee9313; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe restored FND-COMBAT-140's independently
verified saved-map checkpoint. It checked the executable and emulator
identity, instruction/time, named state and native SAVE02.SAV digest.
A11D remained null; the map head/global-hook/input-loop predicate was
checked independently, as that finding describes. No file was injected,
no setup memory was written and no synthetic pathname was supplied.

F2 delivery completed and reached ready Load window 18500 in Load mode.
Enter then reached the admitted callback and numbered-load caller with
row argument one and top zero. At callback entry only row one's marker
was nonzero; the other nine markers were zero. The observed source at
copy entry was the terminated thirteen-character path `C:\SAVE02.SAV`.
The working destination was terminated `C:\darkrun.GFF`, fourteen
characters. The row-one/top-zero fields and list pointer were retained
through preparation. The copy frame and loaded wrapper/call matched
FND-COMBAT-137's declared contracts; optional buffer/capacity were null/zero.

Before copying, the source matched the native writer's retained file
digest. Both fixed private host paths were regular bounded files, and
their filesystem identities were distinct. The debugger stopped at the
following declared copy-body sites; offsets are in resident 4544:

| Stop | Observation |
| --- | --- |
| 004F, source normalization return | AX zero; output `c:\save02.sav`, terminated within the bound |
| 0067, destination normalization return | AX zero; output `c:\darkrun.gff`, terminated within the bound |
| 007D, source-open return | AX 000B |
| 0091, destination-open return | AX 000C; private working file existed with length zero |
| 00A6, length-helper return | DX:AX represented 49,739 |
| Matched overlay-187 +0EFC copy return | AX 0001; original SS and entry SP plus four |

The source and working file had both been 49,739 bytes with equal
SHA-256 before copying. The intermediate destination-open snapshot
therefore matters: the working file was observed empty before transfer,
then was 49,739 bytes and matched the source digest at copy return.
The source's length and digest were unchanged. Host reads covered only
those two fixed private files; the installed originals were untouched.

The script did not stop at every transfer or close operation. Those
individual return values, source position and transient effects beyond
the measured snapshots are not independently claimed. FND-COMBAT-113
supplies the bounded copy-body count/error contracts, separately from
this observed input/output case.

The final run used 46 continuation observations, with five dispatch
captures. Both key operations completed. RNG guards recorded no draw;
the observed generator state stayed unchanged. Execution stopped before
the wrapper tested the copy result or opened/restored archive resources.
The owned muted session then closed.

An earlier run of the same case reached copy return one and equal file
hashes, but lacked the intermediate file-state read. It did not by itself
distinguish unchanged contents from truncation/repopulation. Both reports
are retained; the final run adds that direct observation.

## Interpretation

This supplies an actual F2/Enter, nonempty rooted-source normalization,
open, length and copy-output case for Q-COMBAT-063. It differs from
FND-COMBAT-138's empty-input rejection: destination open was reached and
the observed working file was truncated before being repopulated with
the native source's contents. A11D null did not prevent this F2 route.

This is not a successful whole saved-game restoration. No supplying
SAVE resource, archive selection, fallback or reader destination was
observed after copy return. Other source strings, filesystem aliases,
source-open failure, partial I/O, close failures and later continuation
remain open. A rooted explicit-drive case does not admit every relative
path, directory state or character-class-table input.

## Alternatives

- Equal entry/return hashes alone prove copying changed the destination:
  the first run could not establish that; the final run directly observed
  the zero-length intermediate state and subsequent source-matching output.
- The source and working names necessarily alias: their fixed private
  filesystem identities were distinct in this run.
- Copy return one proves archive resources were restored: the run stopped
  before the wrapper's result test and archive/reader work.

## How to reproduce

Prepare and independently verify the native saved-map candidate as
FND-COMBAT-140 describes. Run the project-local evidence Python with `-I`
and `tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q063-native-copy-truncate-20261104 saved-copy-return`.
Choose a new output name on repetition. Reports, journals, earlier
Load-entry checkpoint and private files remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q063-native-copy-truncate-20261104/.
The earlier net-equality run is v181-q063-native-copy-20261104 under the
same parent. The fixed stage rejects other source/destination names,
changed native-file identities, unsupported frames, missing terminators
and unadmitted RNG callers. Synthetic path-pair and shared admission
controls establish tooling behavior only.
