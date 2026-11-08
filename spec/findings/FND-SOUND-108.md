---
id: FND-SOUND-108
title: Second setup notice constructs two choices and allows the original Enter operation to complete without another selection
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py at a562fc64 and 3d54de7b; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private two-file C drive
---

## Observation

Q-SOUND-010 retains SOUND-107's second invocation and completion of the
original Enter operation. EXE-483..488 supply the shared construction/poll
contracts and CONFIG-262 the distinct caller. Fresh --second-wait attempts
on2026-11-09 repeated the verified first-notice route, retained its original
input handle and stopped at the second190F:0000 entry. They sent no second
key, mouse event or guest write. Addresses/words below are hexadecimal;
text sizes, counts, coordinates and spacing values are decimal.

### Distinct objects and construction

The incoming caller is164C:0C71 with first object13F0:3204 and second
object13F0:D244, directly verifying107's static argument bindings. The
selector word is0001. Style words are000F,0001,0007,0000,000F,0002.
Body text at first-object+2 has55 bytes before NUL, one LF and split
line lengths54,0. Choice text at+0202 has22 bytes before NUL, one LF
and line lengths13,8. All other nonzero bytes are printable ASCII.
Each prefix stopped at its NUL and was read twice, with fixed words and
incoming arguments rechecked. Original text remains in local run stores.

The body prefix including NUL has SHA-256
cb0e34bf4e6363b51b9de29cfddd281aea29d5aa5af067be0a2826114d52d331;
the choice prefix has SHA-256
1390826d464922a4e4b671e2034340ec128af1abb147fe5daf02b1b2d4e99f5e.

The original reached190F:054B before polling with DS13F0, SS22D0,
BP0DE0 and SPF676. Separately bound wrapped local reads give selector1,
expanded body height10, choice length sum23, spacing numerator38,
spacing quotient12, poll local0, registered-row local2, body rows2,
choice rows2 and acceptance flag1. DS:18F6 isFFFF and registration
count DS:18F8 is2. The first row record's six observed words are
0,22,13,36,15,0. No second row record was read; the last word is
not assigned a field meaning. Original selector/style/text prefixes agree
at entry and pre-poll. Duplicate paused reads of all named spans agree.

Thus the first notice's selector-zero/one-choice profile does not apply
here. These are two endpoint observations, not continuous preservation or
admission of every intervening child and deeper stack use.

### Same input handle completes during ordinary waiting

The initial a562fc64 attempt reached512 second-notice poll returns, all
FFFF, with original delivery still pending. It stopped on its explicit
observation budget at emulated385494883ns and closed. This is retained
as a bounded diagnostic, not failed native input or an original crash.

Two expanded3d54de7b attempts used a2048-poll budget and retained
delivery detail/emulated timestamps. Every observed second-notice return
wasFFFF, with DS13F0, SS22D0, BP0DE0, SPF674 and incoming modeFFFF.
In each, the original operation remained pending for1594 observations and reported
done, pending false at observation1595. Its input_delivered event has
emulated timestamp400000016ns; the stopped poll reports400008883ns.
No second input operation or selection was made. The run stopped there,
without continuing through the second notice's selected return or caller.

The paired expanded runs have equal object summaries, pre-poll state and
numeric poll/delivery sequences including emulated timestamps. All six
private source copies were reverified after execution. All three owned
contexts closed, with session metadata reporting muted output, no guest
writes and null run_failure/event_log. No chooser entry, rendered pixels, second selection,
continuous heap/vector/frame lifetime or live allocation ownership is
established. Completion is the session API's delivery state, not proof of
all downstream hardware/OS effects or general keyboard behavior.

## Interpretation

This admits the fixed second object profile and its pre-poll construction,
and demonstrates completion of the original Enter event while the second
notice still waits. Q-SOUND-010's pending-event and second entry/count
uncertainty can be removed for this route. Its second selection/return,
later continuation/retention and live allocation conditions remain open.
Neither format is promoted; no complete reading or parity validation is claimed.

## Alternatives

Budget exhaustion with pending status does not establish a stuck operation.
Increasing the observation budget did not replay its event or restart a
live continuation. API delivery completion does not select the second row:
the guest still returnedFFFF. Endpoint prefix agreement does not prove
absence of intervening writes or valid allocation lifetime.

## How to reproduce

Use3d54de7b and SOUND-100's fixed licensed identities. Run checkout-local
evidence Python with -I, tools/research/rust-dos-runs/setup_parser.py,
a fresh run name, and --second-wait. Retained runs are under
GAME_DIR/captures/rust-dos-setup-parser/: v181-second-wait-20261109-a/
is the512-poll diagnostic; siblings b/ and c/ are the expanded successful pair.
Compare second_notice.objects/pre_poll/polls/completed and last_stop in
setup-parser.json, local second-entry/poll prefix files and session.json.
Report SHA-256 values for a/b/c respectively are
8013ad255f009bdf90258c96906e1d5e949efb275588532d6aa371852a8d909d and
54121d16eb769f43c1854fd577b0605d0bd3d6d7ffaf328bf17c7a6776f02f81 and
81f462c525e48b95bc4291a5a23e5bc145c516f231e5e775f0dd6961682da030.
All original-derived payloads remain local. No emulated call was made.
