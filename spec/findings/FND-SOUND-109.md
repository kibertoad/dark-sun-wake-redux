---
id: FND-SOUND-109
title: The second fixed Enter returns selector one and reaches the generic chooser entry after completion of the first event
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py at 01c33a8a and da2f94a1; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private two-file C drive
---

## Observation

Q-SOUND-010 retains second selection/return and later continuation after
SOUND-108's verified two-choice state and completed first input. EXE-486..488
provide the input/return contracts; CONFIG-262 supplies the caller's next
branch. Fresh --second-select attempts on2026-11-09 used the same licensed
inputs and fixed route. Numbers below are hexadecimal, except poll counts.

The initial01c33a8a attempt ended in host WinError10048 while observing
an existing continuation during the first event's completion wait. Its last
retained phase had1572 completed second-notice wait observations. It never
sent the second input and supplies no second-return evidence. The owned
session closed. This is a host transport diagnostic, not native rejection.
Toolingda2f94a1 adds bounded recovery only for this error on read-only
observations/named memory reads; it never replays a mutation or input.

### Second event and caller return

Two subsequent fresh attempts first completed108's original-event wait with the
same fixed two-choice/selector-one state. Only after the first input handle
reported done, pending false did the probe send one further Enter held1ms.
Thus the route has exactly two key events, one per notice, with no mouse
event or guest setup write. Their operation identities remain distinct.

The second input produced745 observedFFFF poll results followed by000D
at190F:0554. Each read required DS13F0, SS22D0, BP0DE0, SPF674
and incoming modeFFFF. No timeout caused a second send or continuation
restart. The next selected-return stop at190F:0724 had selector local1,
poll local000D and SPF676. AX at that boundary was0001; its final
meaning was checked separately at the caller.

The far return reached canonical164C:0C71, relocated0C06:0C71, with
AX0001, DS13F0, SS22D0 and SP0DE6, four bytes above the original
entry SP. The eight remaining argument bytes still bound13F0:3204
and13F0:D244. No expired notice locals were read after return. This
verifies the selected word rather than inferring it from the initial selector.
The second input operation remained pending at that caller stop.

### Next guarded boundary

With the early/driver guards retained and190F:0000 re-armed, continuation
from AX1 reached19CF:009A, relocated0F89:009A, before its prologue.
DS was13F0, SS22D0, SP0BD2 and BP0FE4. Two paused reads of
the four incoming return bytes agreed on0F89:008F, canonical19CF:008F.
This matches SOUND-021's generic wrapper-to-chooser call.

Only the incoming return was read at this endpoint. No wrapper caller,
source count, group/text pointer, initial index, row construction, vector
or backing allocation was read. Therefore this establishes generic chooser
entry, not its particular source list or MainGroup admission. The older
chooser-row profile's nonwrapping frame assumptions are not validated here.

CONFIG-262's conditional branch maps this second-notice return to its
card79 producer. Reaching the next boundary does not prove device success,
all producer stores or immutable group/heap state; those were not traced.
The second input still reported pending at chooser entry. The run stopped
without operating the chooser or resuming merely to finish delivery.

The paired successful attempts have equal numeric second-poll sequences,
selected/caller states, generic-entry binding and pending-delivery results.
The b attempt logged one bounded WinError10048 read retry; c logged none.
Both completed without replaying guest operations. All six private input
copies were reverified after execution and all three owned contexts closed.
Session records report muted output, no guest writes and null
run_failure/event_log. No
native visual identity, pixels, general input capability, chooser-row/index
admission, continuous retention or live allocation ownership is established.

## Interpretation

The fixed second notice can return1 after one Enter, and its admitted next
route reaches the generic chooser entry. Q-SOUND-010's second-return and
generic entry reachability are no longer hypothetical for this profile.
Second-event completion, actual wrapper/source/frame admission, row/vector
conditions and later lifetime remain open. Neither format is promoted;
no complete reading or parity validation is claimed.

## Alternatives

Returning the initial selector and observing it at the caller are different
claims. A generic wrapper return does not identify the outer caller or its
source list. Pending second delivery is not completed input. A host socket
diagnostic is not a native rejection, and read-only recovery does not
authorize replaying a mutation whose outcome is unknown.

## How to reproduce

Use da2f94a1 and SOUND-100's fixed licensed identities. Run checkout-local
evidence Python with -I, tools/research/rust-dos-runs/setup_parser.py,
a fresh run name, and --second-select. Under
GAME_DIR/captures/rust-dos-setup-parser/, v181-second-select-20261109-a/
is the01c33a8a host diagnostic; siblings b/ and c/ are the successful pair.
Compare second_notice.completed, second_selection and last_stop in
setup-parser.json, the retained local retry log and session.json.
Report SHA-256 values for a/b/c respectively are
41a6781a982fe8cbe819edfcc1917c1c814688f7b0a6c014fd8eebd89848f7cf and
b7c315f2cb22ad0eb3e619024c0f8315116cdde2457f9c9236dfd6908cc88cb7 and
9c7d5a863ea9fe5cbbe9a1b4add0f1f511bcda62f2c4038267158915571573e8.
Original-derived payloads remain local. No emulated call was made.
