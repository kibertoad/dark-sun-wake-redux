---
id: FND-SOUND-105
title: Two fresh setup notices reach the first pre-poll stop with matching wrapped frame and one registered choice
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py at 350205cf; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private two-file C drive
---

## Observation

Q-SOUND-010 retains notice consumption and wrapped-frame admission after
SOUND-103/104. EXE-483..486 supply the construction and first-poll boundary.
On2026-11-09 two fresh --notice-poll attempts used350205cf. Each first
verified104's fixed entry profile, then removed only the190F:0000 guard
and continued to190F:054B, before the first poll's argument push. Early
error/driver and notice return/rejection guards remained armed. No input
or guest write was sent and neither run continued past054B.

Both stopped at relocated0EC9:054B with DS13F0, SS22D0, BP0DE0
and SPF676 (hexadecimal). This agrees with103's word-width wrapped
prologue calculation. The original incoming caller and argument span at
SS:BP+2 still agrees with104. Reads addressed each named local word
separately and the first row record at SS:F67A; no contiguous wrapped
frame read or arbitrary pointer traversal occurred.

All values in the following table are decimal, except the mode word:

| Named observation | Both runs |
| --- | --- |
| Current selector, BP-4 |0 |
| Expanded body height, BP-6 |11 |
| Choice length sum, BP-0A |9 |
| Spacing numerator, BP-14 |51 |
| Spacing quotient, BP-16 |25 |
| Retained poll-result local, BP-1C |0 |
| Registered-row local, BP-1E |1 |
| Collected body rows, BP-20 |3 |
| Collected choice rows, BP-22 |1 |
| Acceptance flag, BP-24 |1 |
| DS:18F6 input-mode word |FFFF |
| DS:18F8 registration count |1 |

The six words read at the first row-record address are0,35,13,44,15,0
in decimal. EXE-485 binds the first five to registration result, left,
top, right and bottom respectively; the sixth word's zero is observed
without assigning it a field meaning. These numbers and the three/one
row counts agree with104's conditional small-text construction profile.
The poll-result local zero is not evidence of an input event: the first
poll has not executed at this stop.

The first object's selector and second object's six words agree with
their entry snapshots. Both original body/choice prefixes, including their
terminators, also agree byte for byte. All named spans were read twice
while paused with no differences. The two complete pre-poll summaries
are equal. This establishes two endpoint comparisons, not continuous
absence of intervening writes or register/segment preservation by every child.

All four private source copies were reverified after execution. Both
session records report host_sound muted, empty writes and null
run_failure/event_log. Both owned contexts closed normally. No screenshot,
rendered-pixel correctness, keyboard/mouse delivery, notice return, chooser
entry or later vector/heap lifetime was observed. In particular,103's
intersection with an earlier negative-group allocation is not converted
into a simultaneous ownership or payload-preservation claim by these reads.

## Interpretation

The fixed notice can reach its first pre-poll boundary with the predicted
wrapped frame and constructed one-choice state. Q-SOUND-010 no longer
requires hypothetical construction reachability or unknown row/count values
at this endpoint. Later frame/object retention, input/return, simultaneous
allocation lifetime and chooser continuation remain open. Neither format
is promoted; no complete reading or parity validation is claimed.

## Alternatives

Matching text at entry and poll does not prove it was never changed in
between. Reaching a poll does not establish that any particular key is
delivered or dismisses the notice. Stable coordinate words do not validate
pixels or every underlying BIOS effect. The earlier numerical allocation
intersection remains distinct from current live ownership.

## How to reproduce

Use350205cf and SOUND-100's fixed licensed identities. Run checkout-local
evidence Python with -I, tools/research/rust-dos-runs/setup_parser.py,
a fresh run name, and --notice-poll. The paired retained runs under
GAME_DIR/captures/rust-dos-setup-parser/ are
v181-notice-poll-20261109-a/ and sibling b/. Compare notice_poll in
setup-parser.json, stopped registers, the entry/poll text prefix files,
and session.json. Report SHA-256 values are respectively
6180585121fa88224d844b3494b34e4c4fc5e0ec9a240b5cb97a4255540d789e and
46744b7ba8285b9d0b065e32b0d4993e88b76fc25ab66981bf879a5d2ac73e3c.
All original-derived payloads remain local. No emulated call was made.
