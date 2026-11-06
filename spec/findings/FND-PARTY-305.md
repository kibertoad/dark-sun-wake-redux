---
id: FND-PARTY-305
title: Intended cursor frame registrations bound list indices to 0..33 and shipped last-cast indices to 0..5
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000962B2..0x000962DA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0515..3D72:059E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A051..0x0005A0BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00096F04..0x00096F36
tool: retained overlay listings; trampoline_target.py, direct_callers.py and resident_listing.py at 68065feb; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; FND-CONFIG-074/075/104/180, FND-MAGIC-023 and FND-UI-006
environment: null
---

## Observation

This assesses existing registration evidence against FND-PARTY-303's
effective-index obligations before extending its source-history reading.
DSUN.EXE is the same licensed installed build, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Code offsets are hexadecimal;
index bounds are decimal.

211+02B5 initializes SI to 2BCD at +04A2. Its registration body
pushes mask 0066, callback 57C1:0061, current SI and the window
pointer from 55C9:0000 into 182+01EF. The own loop increments SI
and repeats while signed SI is below 2BEF. Its intended identifiers
are therefore exactly 2BCD..2BEE, 34 values. The trampoline resolver
independently maps 57C1:0061 to 211+10F4, file 96F04. This agrees
with FND-MAGIC-023's correction of the earlier callback attribution.

FND-CONFIG-180 establishes the registration wrapper's actual argument
layout and its two independent lookup/setter calls. Thus these are
registration attempts, not proof of successful callback installation.
The loop-domain conclusion requires SI and its stack arguments to
survive those calls as supplied; the literal loop does not establish
all intermediate storage against arbitrary aliases.

For last-cast selection, FND-CONFIG-074 records seven attempted
identifiers 2BCD..2BD3 for callback 566A:0043. FND-CONFIG-075 reads
the shipped WIND/14002 graph: exactly the first six APFM identifiers
are present; the seventh is absent. The resolver maps 566A:0043 to
172+0C21. On the unchanged shipped graph, successful intended
registration therefore covers precisely indices zero through five
after the handler's subtraction of 2BCD, not an extra seventh table
word. This graph conclusion reuses the recorded resource inspection;
no new resource bytes or native state were obtained.

The own frame dispatcher 3D72:0515 checks the APFM tag, nonnull frame
and callback, and a positive signed mask intersection. At +055B it
loads the selected frame's identifier word at +8, then stores it in
local event word +2 at +055F. Event kind five and the supplied event
value occupy its adjacent words. After copying the rest of the event
and calling the bracket helper, it copies that local event onto the
outgoing stack and calls through the frame's current +62 pointer.
The identifier comes from the frame record, not from the registration
loop's SI at dispatch time. Local/frame/target preservation through
these intervening children remains required.

Under these unchanged bindings, the list handler's DI is 0..33, and
its additional signed DI<count gate restricts it further. The shipped
last-cast route's SI index is 0..5. Neither subtraction wraps or yields
a negative index in those domains. A fresh list prefix of count entries
with the same retained count then supplies an emitted entry, whose
numeric exclusion is already established in FND-PARTY-303.

The raw relocated direct-call census finds no encoded direct call to
either handler, only its trampoline. The known positive controls are
the independently resolved callback literals and the retained setup
loop; these indirect calls are intentionally outside that direct-call
search. Absence of direct sites does not exclude other pointer users,
jumps, unrelocated targets, bypass entries or changed runtime mappings.

## Interpretation

Q-PARTY-134 no longer needs an unspecified intended registration-index
domain. The remaining out-of-range admission must involve changed
frame identifiers, different callback registrations/targets, preserved
event/frame/stack state, or alternate transfers rather than those
unchanged intended routes. Actual list/table source, member and count
histories still require their own evidence; these registration bounds
do not settle freshness or all writes to the backing words.

This removes a bounded index component, not an entire client. It adds
no complete reading or queue closure and leaves FMT-PARTY-007 supported.

## Alternatives

- The list loop intends identifiers below 2BCD: contradicted by its
  initializer and bound under its stated preservation conditions.
- The seventh last-cast registration necessarily admits index six:
  the unchanged shipped graph lacks that frame.
- Any invocation of the exported handlers has these bounds: unsupported;
  direct or changed indirect inputs can have other word values.

## How to reproduce

Reuse mode-caller211-first.txt for the loop and cursor-si-lastcast/list.txt
for both subtract/gate paths under GAME_DIR/analysis/handler-census-20261106/.
Run trampoline_target.py on 57C1:0061 and 566A:0043. Reuse the exact
registration/resource inspections in FND-CONFIG-074/075/180 and
FND-MAGIC-023; retain their lookup, graph and preservation limits.
Use resident_listing.py for 3D72:0515..3D72:059E, retained locally as
cursor-frame-dispatch.txt. Run direct_callers.py for 211+10F4 and
172+0C21, retained as cursor-handler-direct-callers.txt. No original
game, emulator session or emulated call ran.
