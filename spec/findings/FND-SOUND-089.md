---
id: FND-SOUND-089
title: ADV 11 registration and release preserve slot count conditionally, with event-dependent deferred removal
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00081DDF..0x00085DC4
tool: Ghidra 12.1.3 RecoverCitedFunctions.java, ReportDecompileWindow.java, ReportDecompileMatches.java, ReportInstructionWindow.java and ReportDataBytes.java; reused FND-CONFIG-237 and FND-SOUND-086/087/088
environment: null
---

## Observation

Q-CONFIG-014 retains the count/slot admission required by FND-SOUND-087's
registered-sequence scan. This connected assessment reads registration,
release and the timer's deferred-release consumer together. Addresses are
hexadecimal offsets within installed ADV 11, the 16357-byte resource with
XXH3-128 0faa31676d7d993a393da11bde9e7885 (FND-CONFIG-237).

### Initial relation and completed registration

Source-initialized table CS:293B..295B contains eight zero far pointers;
the following count word CS:295B is zero. This proves the initial source
relation, not its preservation after installation or callbacks.

FND-CONFIG-237 supplies registration 3852's complete local reading. It
checks exactly eight segment words for a free slot. A full table returns
FFFF without publishing a new slot. Its source-selection child at 3883
can also lead to FFFF before publication. For the admitted continuing
route, it loads the supplied control-block pointer at 389F and publishes
its offset/segment into the selected slot at 38A2/38A7. After its resource
walk and sequence-field initialization it increments count at 3973, calls
reset and returns the slot displacement.

At an ordinary completed call boundary, this adds one occupied slot and
one count if the supplied segment is nonzero, the selected slot remains
free until publication, the resource walk reaches the increment, and all
frames, code, data and children are admitted and nonaliasing. A zero
supplied segment does not establish an occupied slot merely because a
count increment follows. No local control-block segment guard supplies
that prerequisite. Publication also precedes the resource walk and count
increment: the partially completed interval is not an atomic count/slot
invariant. Entry saves flags and uses CLI; applying that exclusion requires
the actual call/child and asynchronous-execution contract.

### Release distinguishes no-op, pending and immediate removal

3998 reads its handle at SS:[BP+8]. FFFF or a current slot segment zero
skips all body writes. Other handles are trusted as table displacements;
the routine does not check alignment or the eight-slot range.

For an admitted occupied slot it loads the sequence through ES:DI. State
+1A equal to one writes pending word +1C to one and leaves both slot and
count unchanged. Any other state clears the slot's segment word at 39C4
and decrements count at 39CB. It leaves the slot's offset word in place;
occupancy is defined by the segment test, not by both words being zero.
A repeated release after that segment clear is a no-op under stable state.
No local count floor guard prevents decrementing an already inconsistent
count. No ordinary child is called apart from the saved-flags IRET control.

The seven-byte control at 39D0 verifies the offcut IRET at 39D1 followed
by the ordinary register restore and RETF at 39DB. Decompiler-inferred
extra flags arguments do not change the stack contract above.

### The pending marker has an event-dependent consumer

Timer function 336C checks its reentry word CS:295F, sets it for the
admitted iteration, sets current-slot displacement CS:295D to FFFC and
copies count to a frame-local word at 33AB..33B0. It advances displacement
by four until an occupied segment is found; this is a counted occupied
entry scan with no explicit table-end comparison, like FND-SOUND-087's
cleanup scan. It invokes event helpers only on the selected playing paths.

Its helper 3183 reads the current sequence event through the sequence's
far pointer +0C, including a variable-length field before dispatch. For
event selector 2F, the branch at 31E3 calls cleanup 2CF3, then stores
sequence state +1A equal to two at 31EC. If pending word +1C is nonzero,
it supplies current CS:295D to release 3998 at 3207. Thus the pending
path can become immediate removal because the state change precedes the
release call. The selection, event backing/termination, cleanup return,
current-slot correspondence and storage stability are prerequisites.
Setting pending alone does not prove that this event or removal occurs.

### Conditional preservation relation

Starting from a consistent eight-slot table, serialized admitted completed
registrations with nonzero segment add one to both occupancy and count;
valid immediate releases subtract one from both; no-op and pending releases
change neither. The reached selector-2F continuation reduces to that same
immediate-release transition under the conditions above. These transitions
preserve count equal to occupied segment words, and hence the count bound
zero through eight, at their completed boundaries.

For a timer snapshot, removal of the current visited slot does not itself
invalidate the remaining original-entry count: that iteration is still
consumed once. Additions, removals of future entries, changed current-slot
state or other writes during the scan require separate admission. The
relation is not an exhaustive writer census and is not established across
unexamined copied/computed/aliased writes, invalid handles, partially completed
registration, callbacks or arbitrary reentry. Stable backing for every
selected registered block remains FND-SOUND-087's requirement.

## Interpretation

Q-CONFIG-014 can remove unread local registration/release/deferred-removal
transitions from its count obligation. What remains is whether actual
operation histories meet the nonzero-block, valid-handle, completed-call,
serialization and writer/backing conditions. This supplies a finite
preservation argument for the known transitions rather than assuming the
count always agrees or that pending release completes promptly.

No complete reading, answered queue deletion, observed mismatch or native
playback result is claimed. No original game or emulated call was executed.

## Alternatives

- Release always decrements count immediately: playing state only marks
  pending, and an empty slot is a no-op.
- Empty slots have both words zero: removal clears only the segment word.
- Initial zero values prove a live invariant: completed operations and
  interference must satisfy the stated preservation conditions.
- Pending means removal on the next timer invocation: the known consumer
  requires its selector-2F path and the preceding event/cleanup admissions.

## How to reproduce

Process ADV11.bin in the existing isolated ADV11 project under
GAME_DIR/analysis/music-driver-contract-20261107 read-only without analysis.
Recover 3998, 336C and 3183. Decompile 3998 lines 1..100 and 336C lines
1..115 and 116..275. Inspect 32 instructions at 3998, 76 at 3183 and
46 at 336C; the release window prints adjacent code after its RETF.
Read 34 bytes at 293B and seven at 39D0. Reuse CONFIG-237's retained
registration reading, including 38A2/38A7 and 3973 ordering controls.

Outputs under GAME_DIR/analysis/slot-count-contract-20261107 include
release-and-count.log, timer-consumer.log, timer-consumer-tail.log and
lifecycle-controls.log. pending-release-consumer.log records the focused
3183 pattern result; its 30CB note-event context and 3285 no-match are
not an exhaustive negative search. lifecycle-controls.log shows the
selector-2F branch directly. This assessment does not claim a whole reading
of other tempo, event, interpolation or callback branches in the timer.
No original-derived code or data is committed.
