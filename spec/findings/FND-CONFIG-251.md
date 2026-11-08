---
id: FND-CONFIG-251
title: The window-specific dispatcher reaches the Game Menu Preferences arm with its copied button event
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: DispatchTrace in tools/research/rust-dos-runs/preferences_parent.py and preferences_readback.py at 48a968ff; existing strict restore, readback, value-history and RNG helpers; dinorefurb-dosbox-session 1.0.0; checkout-local evidence Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-250 observes the menu/Preferences parent samples, but only
cross-checks the saved handler return against a known dispatcher call.
This finding additionally stops at that actual call before execution,
reads its copied event, window and handler target, and pairs it with the
following menu-arm frame. FND-CONFIG-249 and FND-PARTY-078 supply the
static far-frame/event-by-value contracts. It is not a census of every
incoming mechanism. Addresses and stack values below are hexadecimal;
counts, event/control values and nanosecond times are decimal.

On 2026-11-08 two independent owned sessions restore the same checked
play-ready state from v181-q046-play-ready-20261104. They use the unchanged
fixed Tab/move-to-virtual(160,101)/left-click route. Executable identity,
instruction/time, named snapshot, frame hash and input-map readiness are
verified before input, with the same pinned muted runtime and private media
contract as FND-CONFIG-250. The embedded DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. No guest setup write or supplied seed occurs.

### Direct call, copied event and actual target

Before arming, the observer verifies the declared resident instruction as
the far call through ES:BX+F5 described by FND-CONFIG-250. It also retains
the parent observer's loaded menu-trampoline and known Preferences-call
checks. After native selection of BUTN10303 and before the existing click,
it adds only that fixed pre-call breakpoint.

Both runs stop there at 2F8B:0802, sequence 108 and time 195591684816ns,
with reason breakpoint and real mode. DS and SS are 4D9A, and SP is FEF8.
The first two words of the already-copied event at SS:SP are kind 2 and
control 10303. This reads the actual outgoing argument copy, not merely
the earlier selected-control state or a later saved return word.

The actual operand window ES:BX is 81CF:0004, equal to both DS:1431 and
relocated 4E46:0000 at this stop. Its readable record has tag WIND, number
10500 and flags 8. Its far handler field at +F5 is 4C73:0034: the known
resident menu trampoline 56B9:0034 after load relocation. The trampoline
was admitted as a far jump to loaded menu 614A:0000 before the click.
No different target is followed and no alternate window is substituted.

### Paired menu and nested Preferences frames

After resuming the actual call, all five existing parent observations
complete. The six observations in this mode are:

| Boundary | Actual instruction | Sequence | Time ns | SP | BP |
|---|---|---|---|---|---|
| Window-specific call | 2F8B:0802 | 108 | 195591684816 | FEF8 | not used |
| Menu Preferences arm | 614A:025E | 110 | 195604680883 | FEEE | FEF2 |
| Post-close Preferences call | 614A:0281 | 112 | 195620449950 | FEEE | FEF2 |
| Resident Preferences trampoline | 4D3C:0048 | 114 | 195620450016 | FEEA | FEF2 |
| Actual music getter | 4028:011F | 116 | 195622785350 | FEE4 | FEE8 |
| Paired music-byte store | 641F:0041 | 121 | 195622786816 | FEE8 | FEE8 |

The menu arm has the same SS and live menu pointer as the observed call.
Its SP is ten bytes below the pre-call SP and BP six bytes below it,
matching the far call and menu prologue. Its actual handler return pair
is 2F8B:0807, the continuation after the directly observed instruction.
The event words in the admitted parent frame are again kind 2/control 10303.
These checks connect the call's actual target and copied arguments to
the following menu frame, instead of inferring the call from that frame alone.

The subsequent event/frame samples and pointer-clear behavior agree with
FND-CONFIG-250. Both menu pointers are null before Preferences, and the
observer never follows released menu backing. The outer Preferences return
is 614A:0286 at SS:FEEA; saved parent BP at SS:FEE8 is FEF2. The getter and
store retain those pairs and sampled event words. Only the final stop has
reason watchpoint; the earlier five are execution breakpoints. DS/SS are
4D9A at all six stops. These remain sampled, not continuous frame histories.

### Completion and existing controls

Both runs complete all three input deliveries and the actual paired getter
return/store. Old music byte 100 becomes 255 from returned AX 255, with
sentinel slot/getter handles FFFF, sampled card ID 122 and library mode 4.
The restored named-value history has its single positive music-byte event.
One caller-checked RULE-SOUND-003 draw has bound 3/result 0 and seed 1718646187
to 2185262696; the existing generator/seed guards admit no other draw/reseed.
No rebuild replay, audible observation or parity comparison occurs.

Both reports have passed, parent.complete and value_history.complete true.
Parsed parent, value_history, draws and readback objects/arrays match exactly.
Volatile input/session metadata and whole-file hashes are not compared.
Both owned muted sessions close normally, with run_failure null and no event
journal. Emulator PIDs 16864 and 6260 are absent, as is the machine run lock.
All original-derived reports and runtime records remain local.

## Interpretation

Q-CONFIG-014 can remove the previously unobserved window-specific dispatcher
call/window/target admission on this exact restored route. The copied event,
live menu, actual handler pointer and following stack/return join are observed
together. The local source reading, saved return and actual pre-call stop
now agree; the result no longer relies only on a sampled return address.

Other dispatcher windows or handlers, global-hook routes, keyboard gates/
post-child key retention, exit-time reentrant reads, changed-frame/nonlocal
entry and unqueried jump/indirect/table/return or constructed-pointer
mechanisms remain open. Neither this call-to-arm pair nor the later samples
exclude between-stop changes or continuous segment/target/frame mutation.
Retain the remaining unmonitored state, driver/sequence/storage/extent and
other-profile obligations. RULE-CONFIG-003 stays supported with no
complete_reading; no whole queue item or parity status closes.

## Alternatives

- A matching handler return alone is the new evidence: the actual pre-call
  instruction, copied event and window/target operand are now observed too.
- A different menu window or handler supplies the result: this mode rejects
  a window differing from either known menu pointer, a different tag/number/
  state or any target other than the admitted resident menu trampoline.
- This establishes all callers or continuous preservation: it covers only
  this fixed route and its explicitly sampled field/frame relationships.

## How to reproduce

Use checkout-local evidence Python with -I and
tools/research/rust-dos-runs/preferences_readback.py, arguments
v181-q046-play-ready-20261104, a new run name and dispatch. Completed outputs
are GAME_DIR/captures/rust-dos-preferences-readback/v181-q014-dispatch-a-20261108/
and sibling v181-q014-dispatch-b-20261108/. Require strict restore controls,
all six parent samples, the outgoing event/window/target and menu-frame join,
all prior parent/getter/store invariants, completed inputs, one checked draw,
no guest writes and all three completion markers.

Compare parsed parent, value_history, draws and readback fields. Review
pins, muted output, run_failure and actual owned process/lock cleanup
separately; no event journal is reconstructed. The observation plan's
Direct dispatcher admission section gives the fixed bindings and controls.
Keep original-derived outputs local.
