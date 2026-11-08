---
id: FND-SOUND-102
title: Two fresh setup guard observations bind the intervening handler to the BLASTER producer notice call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py at 0993e110; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private two-file C drive
---

## Observation

FND-SOUND-101 left the shared190F:0000 guard's caller unidentified.
FND-EXE-483 supplies the entry-before-prologue contract; FND-CONFIG-262
already records the BLASTER producer's source, formatter and notice route.
On2026-11-09 two fresh fixed --chooser-rows attempts used0993e110's
four-byte incoming-return reader. Both first passed the existing parser,
record, label and post-label display checks, then stopped at that guard.
No input, guest write, argument read, pointer following or resumption past
the guard occurred. Numbers below are hexadecimal.

Both loaded at05BA:0000 and stopped at0EC9:0000, canonical190F:0000,
with DS13F0 and SS:SP22D0:0DE2. Two paused reads of the four bytes
at SS:SP agreed. The decoded far return is0C06:050D, canonical164C:050D.
Thus the two independently started attempts identify the same caller.

The retained blaster-producers-instructions.txt from FND-CONFIG-262
places a far call to190F:0000 at164C:0508, immediately followed by050D.
Its preceding pushes supply the first object at DS:AF36 and the second
at DS:D244; following return it removes eight argument bytes. The preceding
formatter/copy sequence uses a stack buffer and the first object's text
region. This matches262's type-caption notice route before its ordinary
type branches. Those are static bindings, not native argument or caption
observations: the new probe reads only the incoming return.

Both session records report muted host output, no guest writes and null
run_failure. Both owned contexts closed normally. Neither chooser entry
nor its row-adjustment endpoint was reached. No screenshot, caption text,
native type, dismissal action, input delivery or frame preservation is
established. A handler entry does not prove that its drawing or event loop
will complete. The four-byte return is not a trace of earlier source history.

## Interpretation

The caller-identification part of Q-SOUND-010's intervening-guard obligation
is answered for this fixed profile. Its input/object/frame contract and
continuation remain open, alongside later chooser/vector/heap conditions.
This removes the alternative that this observed guard is an unidentified
different190F invocation; it does not admit bypassing it. FMT-SOUND-004 and
FMT-CONFIG-002 remain supported, without a complete reading or parity claim.

## Alternatives

A shared handler stop alone could belong to another caller; the stable
incoming return and matching retained callsite distinguish this invocation.
An observed return address does not establish valid text objects, a rendered
notice or a usable completion event. Those alternatives remain untested.

## How to reproduce

Use the fixed licensed identities recorded by FND-SOUND-100 and0993e110.
Run the checkout-local evidence Python with -I and
tools/research/rust-dos-runs/setup_parser.py, a new run name, --chooser-rows.
The two retained runs are under GAME_DIR/captures/rust-dos-setup-parser/:
v181-setup-guard-20261109-a/ and sibling b/. Compare notice_return in
setup-parser.json, the last stop and session.json closure/configuration.
Report SHA-256 values are respectively
8a5dc6ea37b38bf4409bd7ed3421fa7981e991504cc14275cd1378372502fe21 and
6f11e961e82e9ffebf8f25a55c1ae95332d25a47fb7d3325818f111c6c40b134.
Reuse262's retained static listing at164C:04CE..0519; do not rerun an
unbounded caller search. Original-derived reports and private input copies
remain local. No emulated call or gameplay run was made.
