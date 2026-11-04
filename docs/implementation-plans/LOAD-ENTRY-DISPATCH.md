# Load-entry dispatch observation

Research tooling for Q-COMBAT-046; no rebuild behavior changes. The outcome
is an observed route from the verified two-character map checkpoint into
Load Game, without selecting a row or executing a numbered load.

Evidence: FND-SAVE-018's F2 route, FND-CONFIG-221/222's dispatch/count
contracts, FND-COMBAT-132's named UI fields, FND-COMBAT-081/085's mode and
selected-row consumers, and docs/RUNTIME.md's current Play checkpoint.

Restore only a strict pinned, executable-verified play-ready state whose
saved mapping and named fields admit the two-character map with leader 1.
Use the existing muted owned-session settings, private checkpoint drive,
read-only disc and session lock. Deliver only F2, with delivery recorded.
Break at resident 39D1:0752 and the guarded RNG entries; capture the 24-byte
by-value event at the dispatch entry's far-call argument frame. Observe the
named count/head, current event window/callback, global hooks, saved handler,
mirror, Save/Load pointer/mode, selected row/top, list pointer and exit byte.
All pointer reads use bounded real-mode reads and validate the window tag.
Never traverse the list graph, seed memory, invoke a function directly or
send a Load/Save confirmation. Unknown RNG callers and short/invalid reads
fail closed. State readiness, not elapsed time, admits Load window 18500.
Bound continuations and retain a diagnostic report on every failed predicate.
Original-derived events, frames, journals and checkpoints remain in GAME_DIR.

Synthetic controls: null and tagged event targets, bad tags, short reads,
real-mode wrapping, adjacent fields and wrong map/Load predicates. Reuse
the existing checkpoint evidence test suite. Run Test.ps1 with the affected
tooling suite before committing; runtime findings form a separate research
batch. A successful key operation alone proves delivery, not screen entry.

Exit: committed bounded probe and controls passing, with actual runtime
outcome retained locally. No queue closure, evidence promotion, native-saved
game capability, callback preservation or graph-membership proof follows
merely from a snapshot.

## Navigation delivery boundary

An optional fixed `down` stage may follow the admitted F2 entry. Capture
the outgoing 24-byte event at 39D1:083B (before the global far call), then
stop at the evidenced Load callback trampoline 5736:002F. Compare the event,
actual hook and far return frame to 39D1:083F. The callback-entry stop must
carry kind six and key word 5000, as FND-COMBAT-085 describes. This stage
stops before the callback executes navigation; it sends no confirmation.
Record whether the key operation is still pending; do not save a checkpoint
with pending input. Synthetic controls check the different entry/outgoing
stack offsets, equality checks and wrapping/short reads. No claim about
navigation outcome or the callback's later preservation follows from entry.
Remove the broad dispatch-entry breakpoint once F2 entry is admitted, keeping
the global-call/callback and RNG stops. Repeated kind-three idle dispatches
must not consume the Down observation budget before scheduled delivery.

## Navigation return

The fixed `down-return` stage may continue the matched callback to its
verified return at 39D1:083F. Before continuation, require top zero and row
0..9 and read only byte-zero markers of the ten requested 125-byte records
through the observed list pointer. Reject null, short or wrapping storage;
do not read names or descriptions. At return record AX, named UI fields
and the same ten markers, plus current input-operation state. Stop there:
do not confirm a load or claim completed key release. Snapshot equality
at entry/return does not prove absence of intermediate writes. Synthetic
controls cover marker stride, top/row admission, null and wrapping pointers.
Require matching SS and SP equal to callback-entry SP plus four at the
return stop; an inner call reaching the same instruction is not this return.

## Numbered-load call admission

The fixed `load-call` stage may deliver Enter after the admitted F2 entry.
FND-UI-037 maps event-six/1C0D to the action branch; FND-COMBAT-081/129
describes the subsequent +05ED call after window cleanup. Resolve only the
declared callback trampoline's EA transfer to overlay 192 +0785; require
the expected near-call instruction at +0BDF to target +05ED. Reject any
different mapping. Observe Enter at the global-call/callback boundary, then
stop at +05ED before its body runs. Verify far return +0BE2 in the same
overlay segment and read the one row argument. Capture named fields and
ten markers at callback entry and named fields at load entry; do not read
paths, invoke the loader directly, execute its body or send mouse input.
Pending release remains explicit and is never checkpointed. Controls cover
wrong trampoline opcode/offset, call displacement and Enter-event mismatch.
