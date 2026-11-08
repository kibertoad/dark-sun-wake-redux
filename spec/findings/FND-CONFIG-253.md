---
id: FND-CONFIG-253
title: Forwarded P takes the signed-turn rejection and leaves the Game Menu ready after input delivery
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/preferences_rejection.py and preferences_readback.py at 76779199; reused preferences_key.py, strict restore/history/RNG helpers; initial diagnostics at 6cc05dae and a78131f5; dinorefurb-dosbox-session 1.0.0; checkout-local evidence Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-252 stops before the signed turn comparison. This finding
continues that fixed rejecting route through the known key epilogue and
matching menu return, then requires completed P delivery and the same ready
Game Menu. FND-CONFIG-243 and FND-INPUT-012 supply the branch reading;
the keyboard prologue reserves 92 bytes after saving BP and then saves SI,
and its ordinary epilogue restores SI, leaves the BP frame and returns far.
Addresses, stack values and key words below are hexadecimal; mode/turn
values, counts and nanosecond times are decimal.

On 2026-11-08 two independent corrected-tooling runs restore the checked
play-ready state from v181-q046-play-ready-20261104. They verify the pinned
state/executable identity, exact instruction/time, full named snapshot,
frame hash and input-map readiness before input. The embedded approved
DSUN.EXE is 634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
The fixed gestures are Tab held 100ms and, after ready Game Menu 10500,
one lower-case P held 1ms. No guest setup write, supplied seed, mouse gesture
or input retry is used.

### Baseline and actual rejecting continuation

At strict restore, 2F8B:097D, sequence 6 and time 195166973150ns, the sampled
mode DS:0DAB is 1 and signed turn DS:426D is 9999. This predates Tab and the
Game Menu. No writer or first origin of 9999 is inferred, and no continuous
mode/turn watch is installed. The later key samples have mode2 and turn 9999.

Both completed runs retain these identical branch/frame observations:

| Phase | Actual stop | Sequence | Time ns | SP | BP |
|---|---|---|---|---|---|
| State query call | 5D9C:1A82 | 45 | 195480438350 | FE6E | FECC |
| State query return | 5D9C:1A87 | 47 | 195480438750 | FE6E | FECC |
| Before signed turn comparison | 5D9C:1A8E | 49 | 195480438883 | FE6E | FECC |
| Rejection epilogue | 5D9C:1EC9 | 51 | 195480439083 | FE6E | FECC |
| Matching menu return | 614A:0039 | 53 | 195480439283 | FED2 | FEF2 |

All five have reason breakpoint and real mode, with DS/SS 4D9A. Before
return, the keyboard frame retains event kind 6, key word 1970, callback
4C73:002A and handler return 614A:0039. The state query's actual full AX is
two, equal to the sampled mode and nonzero. The actual comparison input is
signed 9999, and the resumed original reaches the known rejection epilogue.
This goes beyond the observer refusal before comparison in FND-CONFIG-252.

At comparison and epilogue, the active keyboard frame's saved menu BP is
FEF2. Its event/key/callback, stack, menu pointers and sampled mode/turn
agree. At the matching native return, SP is keyboard BP+6 and BP is the
saved menu BP. The observer stops reading the keyboard frame after its
lifetime ends; it reads only active registers and named globals there.

Both known menu pointers remain 81CF:0004. The declared callback test/call,
callback trampoline/helper entries, post-callback reload, P opener, resident
Preferences trampoline and music getter are still guarded; none is reached.
The readback array remains empty. This is a fixed-site result, not a census
of alternative transfers or a global absence claim about every callback.

### Completed delivery and ready-menu endpoint

After the matched return, only fixed one-millisecond sampling continuations
are permitted, retaining those execution guards. Completed input and actual
menu readiness determine the endpoint; the duration alone never admits it.
Both runs finish at a time stop 05BA:1E92, sequence 114 and time 195510000016ns,
with ready WIND 10500, flags 8, mouse enabled one, mode 2, leader one and
slot kinds [0,2,0,2]. The registered handler is 56B9:0034; selected control
is null. Both menu pointer fields still agree with the earlier samples.

The completed input records show Tab delivered at 195290000016ns and P at
195510000016ns. Thus P delivery, including its release, is now admitted on
this exact gesture/route, rather than inferred from the earlier key-down
word alone. There are two input operations and 38 total outer stops.
Overall passed is true with explicit result keyboard-turn-rejected;
keyboard.complete and value_history.complete are also true. These markers
mean completed rejection, not entry to the positive Preferences path.

### Unchanged named fields and RNG

The restored nine-field watch set remains fixed: master/music gates, music
byte, configuration pointer, slot/source/getter handles, library mode and
the card word under its admitted configuration binding. It reports no
value-change event; initial and final values match. Music remains 100,
slot/source/getter handles FFFF, library mode 4, card 122, configuration
68D0:0004 and both enable bytes one.

This rejection mode has a distinct no-observed-change predicate and
positive_mutation_control false. Armed but untriggered watches supply no
positive mutation-delivery control. Same-value writes, intra-instruction
effects, unmonitored fields and continuous frame/segment/target preservation
remain outside the observation. Owned watches are removed before completion.

The initial RNG state is 1718646187. One checked RULE-SOUND-003 draw has
bound 3, result 0 and next seed 2185262696. No other known-generator draw or
reseed is admitted under the existing guards. No rebuild replay, native
visual/audible comparison or parity promotion is performed.

### Repetition, diagnostics and closure

Parsed keyboard, value_history, draws and empty readback records are
identical between the two completed runs. Volatile request/input metadata
and whole-file hashes are not part of that comparison.

The earlier a diagnostic reaches epilogue and menu return, but its repeated
builder-only sampling exhausts 512 stops with P still pending; it remains
incomplete. The b attempt stops at the matched return when deletion of an
already-removed builder breakpoint is refused. It is also incomplete.
Corrected tooling uses post-return time sampling and removes that duplicate
deletion. These diagnostics do not count as successful endpoints.

All four owned muted session contexts close normally with run_failure null
and no event journal. Emulator PIDs 13220, 14716, 12228 and 27192 are absent
after closure, as is the machine run lock. Derived records remain local.

## Interpretation

Q-CONFIG-014 can remove the missing actual rejection/epilogue/menu-return
and P-completion observations for this fixed checked menu route. The native
branch rejects the sampled signed turn, leaves the menu pointer bindings
intact and returns to a ready Game Menu after complete P delivery. The
restored baseline already contains 9999 before the menu is opened; assigning
its first creation solely to this opening is unsupported.

Positive callback/reload/opener admission still needs a naturally admitted
turn state below four and its producers. Other keys/callers, mode-zero and
other turn cases, reentrant/nonlocal entries, between-stop changes and
remaining driver/sequence/storage/extent mechanisms remain open. This does
not establish universal keyboard behavior or complete the whole rule.
RULE-CONFIG-003 remains supported without complete_reading; no whole queue
item or parity status closes.

## Alternatives

- An observer rejection alone supplies the outcome: the original now runs
  past the comparison to its epilogue and matching caller return.
- P remains undelivered at the endpoint: both input records are done and
  the same ready menu is independently checked before completion.
- A completed history proves positive watch delivery: this mode explicitly
  has no observed mutation and a separate no-change completion predicate.
- The menu opening first introduces turn 9999: the strict pre-input baseline
  already has that value, without proving its earlier writer/history.

## How to reproduce

Use checkout-local evidence Python with -I and
tools/research/rust-dos-runs/preferences_readback.py, arguments
v181-q046-play-ready-20261104, a new run name and keyboard-rejection.
Completed runs are GAME_DIR/captures/rust-dos-preferences-readback/
v181-q014-rejection-c-20261108/ and sibling v181-q014-rejection-d-20261108/.
The same root's a/b attempts are retained incomplete diagnostics.

Require the strict restore baseline, five accepted branch/frame samples,
matching menu return, completed Tab/P deliveries and ready-menu endpoint,
explicit rejected result, empty readback, unchanged final named values and
positive_mutation_control false. Compare parsed keyboard, value_history,
draws and readback objects. Review exact pins, muted output, run_failure
and actual process/lock cleanup separately; reconstruct no event journal.
The observation plan's Native turn-rejection completion section documents
the scope and controls. Keep original-derived output local.
