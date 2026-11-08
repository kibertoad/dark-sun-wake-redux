---
id: FND-CONFIG-250
title: The menu event and outer Preferences frame retain their sampled values across close and readback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:07D2..39D1:0810
tool: tools/research/rust-dos-runs/preferences_parent.py and shared preferences_readback.py at d550b49d; preferences_history.py; resident_listing.py for return-site cross-check; dinorefurb-dosbox-session 1.0.0; checkout-local evidence Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-249 supplies the static menu/event and nested-frame contract.
FND-CONFIG-248 supplies the separate full fresh named-value history. This
finding observes actual parent/event samples on the existing strict restored-
map route. It does not extend the earlier fresh run's frame coverage or join
the two sessions into a continuous parent-frame history. Addresses, stack
values and unlabelled offsets are hexadecimal; counts, event/button values
and nanosecond times below are decimal.

On 2026-11-08 two independent owned sessions restored the same checked
play-ready state from v181-q046-play-ready-20261104. The observer verified
the pinned state/executable identity, exact instruction/time, full named
snapshot, frame hash and input-map readiness before supplying input. The
embedded approved DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Restored time is 195166973150ns and RNG
state 1718646187. No guest setup write or supplied seed was used.

The existing Tab/move-to-virtual(160,101)/left-click route completes all
three input deliveries. Native menu readiness and selected BUTN10303 are
required before the click. At that point the observer verifies the loaded
menu-handler trampoline 56B9:0034, derives menu segment 614A from its far
jump to offset 0 and verifies the known call at that segment's +0281 to
the relocated Preferences trampoline. Only the three declared parent
breakpoints are armed, in addition to the existing getter/RNG/history stops.

### Five paired samples

All five stops have DS and SS equal to 4D9A. Event kind is 2 and control 10303
at every sample, read from the admitted parent BP frame's words+6 and+8.
The handler's own sampled return pair at that frame's +2 is 2F8B:0807
throughout. Actual stops and fields are:

| Sample | Actual instruction | Reason | Sequence | Time ns | SP | BP |
|---|---|---|---|---|---|---|
| Menu Preferences arm | 614A:025E | breakpoint | 107 | 195604680883 | FEEE | FEF2 |
| After close, before opener call | 614A:0281 | breakpoint | 109 | 195620449950 | FEEE | FEF2 |
| Resident Preferences trampoline | 4D3C:0048 | breakpoint | 111 | 195620450016 | FEEA | FEF2 |
| Actual music getter entry | 4028:011F | breakpoint | 113 | 195622785350 | FEE4 | FEE8 |
| Paired music-byte store | 641F:0041 | watchpoint | 118 | 195622786816 | FEE8 | FEE8 |

At the arm, both DS:1431 and relocated 4E46:0000 contain 81CF:0004.
The live backing reads as WIND 10500 with flags 8. At the pre-opener call
and every later sample both pointer fields are null. The old window is
not dereferenced after close; no retained allocation or object lifetime
is inferred from the earlier readable backing.

Across the frame-five setter, flush and close children, the sampled SS,
SP, BP, event/control words and handler return pair agree before/after.
This is actual sampled agreement, rather than a conclusion from the
balanced stack arithmetic in FND-CONFIG-249 alone.

At the resident Preferences trampoline, the actual far return is 614A:0286
at SS:FEEA; SP is four bytes below the pre-call sample and BP is still FEF2.
The observer does not require the initially entered trampoline to be patched
to its final jump before that first call. At the actual getter and store,
the outer return pair is still 614A:0286, and the word at SS:FEE8 saves
parent BP FEF2. The actual SP/BP relationships match the static nested-frame
contract. The parent's event and handler return words also still match.

### Handler return-site cross-check

The sampled handler return 2F8B:0807 maps to resident 39D1:0807 using the
verified load relocation. Reuse FND-PARTY-078's event-by-value delivery and
FND-PARTY-044's dispatch reading. A bounded instruction cross-check confirms
that 39D1:0802 calls the far handler at window-record+F5 and 0807 removes
the 24-byte copied event. This is the window-specific call arm, distinct
from the later global-hook arm. It is consistent with the observed parent
return and the menu handler reached on the fixed route.

The probe does not stop before that indirect call or read its actual
dispatcher window/target binding. A matching saved return word is not by
itself an execution trace of that instruction or an exclusion of alternate
entry/frame construction. That precise caller/target obligation remains.

### Existing getter, history and RNG controls remain admitted

Readback verifies the actual Preferences trampoline, resident getter call,
return before the AL store and post-store frame. As in FND-CONFIG-245,
the old music byte is 100, actual returned AX is 255 and the byte becomes 255.
The configuration is 68D0:0004 with sampled card ID 122, library mode 4 and
sentinel slot/getter handles FFFF. No later Preferences interaction or
eventual menu return is executed by this observer.

The restored nine-field history has its single positive music-byte change,
at the post-store watch above. It includes the derived card watch and omits
the fresh observer's internal driver-enable word. Its caller binding remains
fixed and its completion requires actual byte mutation and watch cleanup.
The known generator/seed-setter guards stay active. One RULE-SOUND-003 draw
has bound 3, result 0 and seed 1718646187 to 2185262696, with checked native
caller/result and next seed; no other draw or reseed is admitted. There is
no rebuild replay, continuous seed watch or parity comparison.

Both reports have passed, parent.complete and value_history.complete true.
Their parsed parent, value_history, draws and readback objects/arrays are
identical. Whole-file metadata and input request IDs are not compared.
Both owned muted sessions close normally with run_failure null and no event
journal. Emulator PIDs 27816 and 13888 are absent, as is the machine run lock.
Original-derived reports and runtime records remain local.

## Interpretation

Q-CONFIG-014 can remove the missing actual menu-arm/post-close and nested
Preferences event/frame samples for this exact restored route. Event 2/
control 10303 and the named stack/return pairs are admitted at the five
stops, with actual pointer clears across the known children. This supplies
neither continuous frame/segment preservation nor all incoming root paths.

Retain the actual dispatcher call/window/target binding, other parent and
keyboard post-child key/gate states, exit-time reentrant readers, changed-
frame/nonlocal entry, between-sample writes and all unqueried indirect or
constructed mechanisms. Existing driver/sequence/storage/extent and other-
profile obligations also remain. RULE-CONFIG-003 stays supported without
complete_reading; no whole queue item or parity status closes.

## Alternatives

- The pre-click selected button alone proves post-child retention: the
  observation independently rereads the actual parent slots and return pair
  after the children and inside the nested Preferences frame.
- Null pointers prove the released menu backing remains valid: the observer
  follows it only at the pre-close arm and never after the pointer clears.
- The handler return proves execution of the earlier indirect call: that
  call/target binding was not directly observed and remains an obligation.
- Equal samples prove no intervening change: same-value writes, transient
  changes restored between stops and unmonitored fields remain outside them.

## How to reproduce

Use checkout-local evidence Python with -I and
tools/research/rust-dos-runs/preferences_readback.py, arguments
v181-q046-play-ready-20261104, a new run name and parent. Completed outputs
are GAME_DIR/captures/rust-dos-preferences-readback/v181-q014-parent-a-20261108/
and sibling v181-q014-parent-b-20261108/. Retain preferences-readback.json,
session.json and the owned private runtime records.

Require strict restore controls, all five parent samples, event/control,
stack/return and pointer invariants, completed inputs, paired getter/store,
one checked draw, no guest writes and all three completion markers. Compare
the parsed parent, value_history, draws and readback fields. Review pins,
muted output, run_failure and actual process/lock cleanup separately.

For the return-site cross-check, use committed source-guarded resident_listing.py
with 39D1:0752..39D1:0810; retain event-dispatch.txt in
GAME_DIR/analysis/preferences-parent-frame-20261108/. The entry and branch
reading is independently established by FND-PARTY-078/044. The observation
plan's Menu parent and event frame section documents the fixed bindings
and synthetic controls. Keep original-derived outputs local.
