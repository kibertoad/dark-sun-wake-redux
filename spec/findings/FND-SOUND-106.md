---
id: FND-SOUND-106
title: One Enter event produces the notice exit word and normal caller return in two fresh runs while delivery remains pending
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py at 5d8a1f7a and b866dcf2; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private two-file C drive
---

## Observation

Q-SOUND-010 retains notice input and return beyond SOUND-105's pre-poll
endpoint. EXE-486..488 supply its poll and exit contracts. On2026-11-09
three fresh --notice-enter attempts first verified104/105's fixed profile
and queued exactly one Enter event with hold_ms1. Every continuation used
its retained operation handle; no timeout caused input replay. There were
no guest setup writes, mouse events, second keys or chooser continuations.
Addresses and machine words below are hexadecimal; poll counts are decimal.

All three attempts observed232 successive poll returns ofFFFF followed
by000D at190F:0554, after03D7 returns and before argument cleanup.
Each stop had DS13F0, SS22D0, BP0DE0, SPF674 and incoming modeFFFF.
This is an observed guest poll sequence, not a timing guarantee or a claim
that every input operation returns such a sequence.

The first attempt, at5d8a1f7a, stopped in host tooling after the000D
observation: it incorrectly tried to remove the already consumed one-shot
0554 stop. The server reported no breakpoint at that address. The owned
session closed; no selected-return or caller-return result exists for that
attempt. This is not an original-game failure. Toolingb866dcf2 removes
that invalid request. The first run remains a retained partial diagnostic.

The two corrected attempts independently reached190F:0724 with DS13F0,
SS22D0, BP0DE0 and SPF676. Current selector at BP-4 was0000 and
poll local BP-1C was000D, each read twice. AX at this point was0001;
it is not the final notice result because EXE-486's selected-word load
has not executed yet.

After removing only the selected-return and rejection guards, both reached
the original caller at canonical164C:050D, relocated0C06:050D, with
AX0000, DS13F0, SS22D0 and SP0DE6. This is the incoming entry SP
plus four, matching consumption of the far return. The eight remaining
argument bytes still named13F0:AF36 and13F0:D244. No expired notice
local was read after the return. The probe stopped before caller cleanup
or later producer/driver/chooser work.

The same input-operation handle was observed again at that final stop.
Both corrected runs still reported pending true and state pending. Thus
the guest's000D and normal return are verified, while completion of the
whole queued press/release event is not. Neither run was resumed merely
to finish delivery. Both owned sessions then closed. No new event was sent.

Corrected runs have equal numeric poll sequences, selected-return state,
caller-return state and pending-delivery result. All six private input
copies were reverified after execution. All three session records report
host_sound muted, empty writes and null run_failure/event_log. No native
visual identity, pixel correctness, sound output, arbitrary input capability,
later object/frame lifetime or live allocation ownership is established.

## Interpretation

For the fixed notice profile, one queued Enter event can be observed as
000D and the normal selected-word return0000. Q-SOUND-010 no longer
has a hypothetical notice-return route under this exact attempt. Complete
input delivery, later continuation/retention and chooser admission remain
open. Pending release must not be silently treated as completed input when
designing a longer continuation. Neither format is promoted and no complete
reading or parity validation is claimed.

## Alternatives

A pending input operation does not negate the independently observed guest
poll and return, but that guest response does not prove full delivery.
Selected-boundary AX0001 is not the eventual caller result0000. The first
host refusal does not show native rejection, and its missing later endpoints
cannot be supplied by the corrected runs retroactively.

## How to reproduce

Useb866dcf2 and SOUND-100's fixed licensed identities. Run checkout-local
evidence Python with -I, tools/research/rust-dos-runs/setup_parser.py,
a fresh run name, and --notice-enter. Under
GAME_DIR/captures/rust-dos-setup-parser/ the corrected paired runs are
v181-notice-enter-20261109-b/ and sibling c/. Sibling a/ is the partial
5d8a1f7a diagnostic. Compare notice_input and last_stop in setup-parser.json,
plus session.json. Report SHA-256 values for a/b/c respectively are
36122c1adfeef0846b36c7029e2080d8427714e25982b0d9f5a29adf96c2f1b2,
0d9284623f4f2f1b6f05c8d35b22f9d48a81d5a7123972636a05387480fcc532,
250f3b0e81c3e4d44ae9436d51f52bcc9ee493d2d9bcfdfb9a544ac124110435.
Original-derived payloads stay local. No emulated call was made.
