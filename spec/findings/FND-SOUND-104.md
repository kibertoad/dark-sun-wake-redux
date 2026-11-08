---
id: FND-SOUND-104
title: Two fresh setup notice observations bind fixed objects to a small terminated body and one choice
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py at 664e5a12; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private two-file C drive
---

## Observation

Q-SOUND-010 retains object admission at FND-SOUND-102's identified
BLASTER notice, before any attempt to continue the handler. EXE-483..486
supply its object consumers and SOUND-103 its wrapped-frame constraint.
On2026-11-09 two fresh --notice-objects attempts used664e5a12. Both
passed the parser/post-label checks and stopped at190F:0000 before its
prologue. There was no input, guest write or continuation past this guard.
Addresses and words below are hexadecimal; text sizes are decimal.

Both incoming return/argument frames give canonical164C:050D, first
object13F0:AF36 and second object13F0:D244, with DS13F0 and
SS:SP22D0:0DE2. This directly corroborates102's previously static
argument bindings at this endpoint. The first object's selector word is
0000. The second object's six words, in order, are000F,0001,0007,
0000,000F,0002. EXE-484..486 describe their separate consumers;
these observations do not assign visible color meanings.

The body at first-object+2 has99 bytes before NUL. It has two LF bytes
and split line lengths44,53,0, the last empty line following the final
LF. Every other nonzero byte is printable ASCII. The choice text at
first-object+0202 has eight printable bytes before NUL and no LF.
Reads stopped at each NUL. Each prefix was read twice, and selector,
style words, incoming frame and return were rechecked while paused.
Neither512-byte body nor128-byte choice observation cap was exhausted;
the caps are limits of this probe, not measured object capacities.

Both body prefixes, including NUL, have SHA-256
ab917c16918260a53f77c2b5dfec74918b6edd3dc43b25671891a5a4d207cf20.
Both choice prefixes have SHA-256
8dd2ef8f57e5a6e0a5962b95e753008196363cda10bcdb5b2d5cad5916b2fad7.
Original text remains in the local run stores. No semantic caption or
key label is assigned from a screenshot or inferred from these lengths.

Under EXE-483's unchanged-input, nonaliasing and helper-preservation
conditions, the observed short strings produce three body rows and one
choice row. Their lengths stay below its70/40-byte collector thresholds;
the choice length contribution is nine. Selector zero is within that
one-choice domain. This is a conditional application of the static reading,
not an observation of native row construction. EXE-484's spacing divisor
would be two for that domain, removing the zero-choice/divisor alternative
for these observed objects if retained through collection. Earlier or
intervening writers, aliases and child preservation remain unproved.

The two complete object summaries are equal. All four private source copies
were reverified after execution. Both session records report host_sound
muted, empty writes and null run_failure/event_log. Both owned sessions
closed normally. No handler drawing, poll, return, deeper-stack use,
chooser entry or later vector/allocation lifetime was observed.

## Interpretation

Q-SOUND-010's native object uncertainty is narrowed to retention and
consumption after this endpoint. The fixed route no longer has hypothetical
incoming pointers, selector or unterminated/large choice text at entry.
Wrapped-frame and simultaneous allocation lifetime/alias admission from103,
input delivery and continuation remain open. Neither format is promoted;
no complete reading, successful dismissal or parity validation is claimed.

## Alternatives

A valid incoming return alone did not prove its arguments; this reader
checks those separately. Short terminated text at entry does not prove
stable text during children, valid wrapped local storage or successful
drawing. One observed choice does not validate other notice profiles or
prove a particular input completes this notice.

## How to reproduce

Use664e5a12 and the fixed licensed identities recorded by SOUND-100.
Run the checkout-local evidence Python with -I,
tools/research/rust-dos-runs/setup_parser.py, a fresh run name, and
--notice-objects. The retained paired runs under
GAME_DIR/captures/rust-dos-setup-parser/ are
v181-notice-objects-20261109-a/ and sibling b/. Compare notice_objects in
setup-parser.json, local notice-body-native.bin/notice-choices-native.bin,
the stopped state and session.json. Report SHA-256 values are respectively
ec061983276d57d37350b7ec6cb62d656a0dca589007a441753f3a81a03b7e29 and
ed48496b0deb73ff2f77d018b2d9638e0649a6b31902555a6f0b0888f2f3a436.
No original-derived payload is committed. No emulated call was made.
