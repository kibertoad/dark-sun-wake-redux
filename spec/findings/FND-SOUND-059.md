---
id: FND-SOUND-059
title: Refill uses post-submission DS and SI before its own return and does not restore DS
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4611:0051..4611:0177
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:03BC..4842:040C
tool: resident_listing.py at 7f06e5ca; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-055/056/057/058 and FND-RNG-096
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-056/057 provide the conditional descriptor wrapper
return: entry DS becomes SI, the descriptor count high word
becomes DS, and entry SI becomes DI under intact local frames
and the specified ordinary child returns. This finding follows
that condition through the immediate refill caller, not to a
native reseeding outcome.

4611:0051 saves BP and SI at entry, but never saves DS and has
no local DS assignment. At 0122 it requests selector 0086 through
4842:0C6A with current DS:3436 and descriptor DS:A424. Under
FND-EXE-646's unchanged DIGPAK dispatch this is the 182E body.
After its return the caller removes six argument bytes, increments
current SI and compares it with two signed. It does not reload
its loop index before that comparison.

Therefore the earlier two-slot-loop description requires the
retention condition stated in FND-SOUND-055. If the wrapper's
entry DS is 57E0 under the local mismatch conditions, current
SI becomes 57E0, increments to 57E1 and takes the signed
not-less-than-two exit. It does not visit a second slot on that
continuation. This is conditional arithmetic, not native admission.

At 0133 it reads DS:3436 using the post-submission DS and
requests 4842:0C88. It then reads dword DS:A438. Neither read
locally restores the previous data segment. If that dword is zero,
additional queries and possibly the stop body use the current DS
binding as described in FND-SOUND-055. If nonzero it assigns
AX one and reaches the common epilogue. That epilogue pops its
own saved SI and BP and returns far; it does not restore DS.
Thus the caller can restore its external SI contract without
restoring the segment under which its final operations ran.

### A bounded non-restoring continuation

Dispatch 03F4 obtains its driver slot from SS:[entry SP+4] and
requests lookup 03BC. For an unsigned slot at least sixteen,
03BC assigns DX:AX zero and returns without dereferencing a
driver table. 03F4 then returns directly, without loading DS or
calling a driver. These instructions have no interrupt request.

Consequently, under the preceding wrapper/frame conditions,
entry DS 57E0, a word at post-submission DS:3436 at least sixteen
and a nonzero dword at that DS:A438, the immediate caller can
reach AX one and its ordinary return without any later child
restoring DS. This finite continuation still requires actual backing
memory and histories for those fields. It is not a native fixture,
an established reachable state or proof of a later reseeding effect.
Other admitted handles can select children whose effects remain open.

### Relation to the old census

FND-RNG-096 explicitly records that its second walk does not
decode 182E and that neither of its two censuses flags a call
of 03F4. The first includes the driver-table bodies as inventory
rows. Its reported push/pop sites in 182E do not prove restoration
through the shifted frame. The report's separate model statement
must not be promoted into an exhaustive runtime propagation proof
through this far-return dispatch. Actual callers, admission and
later segment restoration remain Q-RNG-040; this finding does
not revise the historical census counts or infer native failure.

## Interpretation

Q-RNG-040 can remove ambiguity about the immediate refill caller's
local restoration: there is no DS save/reload, and post-submission
SI affects its loop before the epilogue restores SI. The bounded
invalid-slot continuation supplies one conditional return route
without adding an unknown restoring driver child. Actual selected
mask/driver/frame, memory admission and routes from this return to
reseeding callers remain unresolved. No complete reading, whole
queue closure, universal register value or native RNG defect follows.

## Alternatives

- The caller reloads its loop index after submission: it increments
  the returned SI directly before its signed loop test.
- Its epilogue repairs the changed DS: it pops SI and BP only.
- The later request necessarily restores DS: lookup can reject an
  unsigned slot at least sixteen and return without a driver call.
- The old census establishes this propagation: neither census flags
  the dispatcher and the second excludes the target body.

## How to reproduce

Read both location ranges with resident_listing.py at the tool
revision above. Retain sound-refill-postsubmission-frame.txt under
GAME_DIR/analysis/handler-census-20261106/. Combine the known
wrapper frame with the caller's 0127..0176 continuation, tracking
current versus entry DS and SI separately. Check the unsigned slot
rejection and DX:AX-zero dispatch path. Reuse FND-RNG-096's own
model/exclusion statements rather than rerunning unchanged censuses.
No original game, emulator session or emulated call was used.
