---
id: FND-RNG-098
title: A loaded offset-loop trampoline does not rebind DS before the clock helper forms its near stack-buffer offsets
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BDC0..0x0008BDE4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0963..1000:09B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:046E..1000:0494
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0811..1000:0822
tool: resident_listing.py and overlay_listing.py at 963aa90e; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FMT-EXE-004, FND-RNG-009/021/053/096 and FND-SOUND-059/060
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-060 identifies the service-return repeat edge from
201 +0767 to the call at +075F of 204 +0000 through 5787:006B.
This finding composes the retained caller and callee contracts
without assuming actual changed-segment admission.

### Transfer and entry prefix

FMT-EXE-004 records two trampoline forms. When the overlay is
already loaded and the trampoline retains its rewritten far-jump
form, the transfer changes CS:IP, not DS. The trap form instead
enters the overlay manager; its segment/frame effects are not
excluded by this finding. Actual residency and absence of intervening
unloading or rewriting remain necessary to apply the far-jump case.

204 +0000 saves BP, reserves six local bytes and saves SI/DI.
It clears SI/DI and initializes local words/bytes. At +0015 it
pushes zero and at +0017 calls 1000:0963. It makes no local DS
assignment before that call and has not tested its incoming mode,
count or magnitude. After the clock helper returns, it passes only
AX to 1000:0811 at +001E. Reuse FND-RNG-009/021 for this
low-word seed forwarding, not a new independent seed observation.
Thus mode/count predicates cannot alone exclude the initial clock
call after an admitted entry. The caller's three arguments influence
later work, not the prefix's decision to request the clock.

### Near offsets retain separate segment identities

0963 reserves twelve local bytes on SS and holds its incoming
optional output offset in SI. It forms BP-4 as a near offset for
046E and BP-8 for 0481; neither pointer contains an SS segment.
It does not load DS before either call. 046E loads its offset
argument, requests INT 21/AH=2A and stores returned CX/DX at
DS:offset and DS:(offset+2). 0481 similarly requests AH=2C and
stores through DS. Their BP/SI and arguments remain SS-based;
they make no local DS assignment or DS save/restore.

Under an unchanged DS across the external requests, these stores
therefore use the inherited DS, not necessarily the segment of the
frame whose offsets were passed. Equal near offsets alone do not
identify the same backing bytes. This is the conditional distinction
already noted by FND-RNG-053, now applied to the concrete service
return route; it does not establish an actual mismatch or successful
clock input stores. The interrupt handler could affect registers,
frames or bindings, and those external guarantees remain separate.

0963 next passes both near offsets to 386D and retains its DX:AX
result in SS locals. Its optional output offset is zero for this
entry, so that additional output store is skipped under preserved SI.
It returns the saved result. The converter's state/segment effects
remain Q-RNG-003 and Q-RNG-040; the absence of a DS assignment
in the wrapper does not prove every child preserves it.

0811 loads its seed argument through SS:BP+6, writes zero to
word DS:384C and that argument to DS:384A, then returns. There
is no local DS rebinding. Consequently identifying the write as an
update of the intended game's RNG state also requires the actual
DS binding at this later call. A changed entry DS does not by itself
prove that either the clock destination or RNG state was changed
in the intended data segment; conversion and external effects may
intervene. No actual seed value is inferred here.

## Interpretation

Q-RNG-040 can remove the local entry-prefix/near-pointer ambiguity:
there is no DS reload before the first clock request on the loaded
far-jump route, and no mode/count gate precedes that request.
The remaining transfer obligation names actual residency or trap-path
effects; remaining input obligations name SS-frame versus DS-buffer
backing, service guarantees, conversion effects and final seed-store
binding. The service return remains conditional on FND-SOUND-059's
admission. No whole queue closure, complete reading, native mismatch
or RNG failure is claimed.

## Alternatives

- The loaded trampoline repairs DS: its far-jump form has no DS load.
- Zero or rejected mode/count prevents the clock prefix: arguments
  are tested only after the clock/seed calls.
- Near stack-buffer offsets ensure stores reach the SS frame:
  the two service wrappers explicitly use inherited DS.
- A changed DS necessarily corrupts the real RNG state: actual
  intervening effects and final state-segment identity remain open.

## How to reproduce

Read overlay 204 file range 8BDC0..8BDE4 and the three resident
location ranges with the approved readers at the tool revision above.
Retain reseed-offset-entry-segment-contract.txt and
reseed-clock-segment-contract.txt under
GAME_DIR/analysis/handler-census-20261106/. Reuse FMT-EXE-004
for the two trampoline forms and FND-SOUND-060 for the repeat edge.
Track SS-relative offset formation separately from DS-relative stores,
and preserve every external-service and residency condition.
No original game, emulator session or emulated call supplied evidence.
