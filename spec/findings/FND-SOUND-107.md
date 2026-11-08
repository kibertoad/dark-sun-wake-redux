---
id: FND-SOUND-107
title: Fresh continuation after the first setup notice reaches a second BLASTER choice call while the original input remains pending
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py at 5331aeac; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1; retained FND-CONFIG-262 callsite listing
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private two-file C drive
---

## Observation

Q-SOUND-010 retains complete input delivery and continuation after
SOUND-106's first notice return. On2026-11-09 two fresh --notice-next
attempts used5331aeac. Each repeated104..106's fixed object, pre-poll,
single Enter and normal caller-return route. Each retained the original
input-operation handle, re-armed190F:0000, and continued only toward
19CF:009A with early-error/driver guards still armed. No second key,
mouse event or guest setup write was sent. Numbers below are hexadecimal.

The next stop in both runs was190F:0000, relocated0EC9:0000, rather
than the chooser entry. DS remained13F0 and SS:SP was22D0:0DE2.
Two paused reads of the incoming four-byte far return agreed. Both give
0C06:0C71, canonical164C:0C71. No arguments, object payloads or local
frame at this second invocation were read. Neither run continued through it.

The retained blaster-producers-instructions.txt from CONFIG-262 places
a far call to190F:0000 at164C:0C6C, returning at0C71. Its preceding
pushes statically supply DS:3204 as the first object and DS:D244 as the
second. Unlike the first call's AF36 object, this is a different object
binding. After the call the producer removes eight argument bytes, retains
AX in SI and chooses0F0F for zero or1144 for nonzero. Reuse262's
conditional card7A/card79 producer contracts; no native selection result
or outgoing record writes from this second invocation were observed.

This confirms which next callsite the fixed route reaches, not native
values for its type, membership tests, object contents or selection. The
earlier static reading's case structure does not supply missing native
argument snapshots. It is not justified to reuse104's one-choice profile
or send another Enter solely because the handler entry is the same.

At this second stop, observing the same original Enter-operation handle
again reported pending true and state pending in both runs. No operation
was restarted, replaced or replayed. The observed guest response and first
notice return from106 remain distinct from this unfinished delivery. The
probe ended without advancing merely to satisfy an emulated-time deadline.

The two next-boundary summaries are equal. Both first-notice traces again
contain232 FFFF polls followed by000D and caller return0000. All four
private source copies were reverified after execution. Both session records
report host_sound muted, empty writes and null run_failure/event_log.
Both owned contexts closed normally. No chooser entry/rows, second-notice
input, rendered pixels, continuous object/vector/heap lifetime or live
allocation ownership is established.

## Interpretation

The immediate continuation after the verified first notice reaches a
specific second BLASTER choice call under this profile. Q-SOUND-010's
next-boundary ambiguity is answered, while complete delivery and the second
invocation's object/frame/input admission remain open. The first notice's
return is insufficient to reach or admit the chooser. Neither format is
promoted; no complete reading or parity validation is claimed.

## Alternatives

Reaching the same shared handler does not establish the same objects or
choice count. Pending delivery at two later stops does not establish a
stuck input operation: no attempt here advances through a supported wait
state until delivery completes. Static card branches do not establish the
native second selection or hardware success.

## How to reproduce

Use5331aeac and SOUND-100's fixed licensed identities. Run checkout-local
evidence Python with -I, tools/research/rust-dos-runs/setup_parser.py,
a fresh run name, and --notice-next. The paired retained runs under
GAME_DIR/captures/rust-dos-setup-parser/ are
v181-notice-next-20261109-a/ and sibling b/. Compare notice_next,
notice_input and last_stop in setup-parser.json, plus session.json.
Report SHA-256 values are respectively
ba0ae7a88e3004d3bf94e8969863fd8f0bde9f58773c90cc8493482169cd79b4 and
80194154c83d04d138f665572a8324caa3508ebb82c6d7d41b46012bc98a8057.
Reuse262's retained listing at164C:0C54..0C88. Original-derived payloads
remain local. No emulated call was made.
