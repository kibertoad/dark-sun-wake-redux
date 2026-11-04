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

## Copy input binding

The fixed `copy-inputs` stage may continue the admitted +05ED entry through
its preparation children and working-path construction, stopping at resident
4544:0000 before normalization or copy I/O. Verify its return at overlay
187 +0EFC against the declared 56EF:0089 trampoline and +0EF7 far call.
Read only the four declared arguments: source, destination, optional buffer
and capacity. Require the known null buffer/zero capacity contract. Read
source as a NUL-terminated path bounded to 80 bytes and destination to the
wrapper's 82-byte local buffer, one byte at a time and rejecting missing
terminators, wrapping or short reads. Record actual strings only locally;
spec records lengths/identity facts in its own words. Capture named fields
after preparation; no file-copy success or filesystem identity is inferred.
Controls cover early termination, unterminated/null/wrapping paths and
argument layout; no arbitrary path reads or additional inputs are admitted.

## Copy return and rejection

The fixed `copy-return` stage may continue the declared copy invocation to
its verified wrapper return, with SS/SP identity checked. Observe its source
and destination normalization returns at 4544:004F/0067 and direct open-call
sites at 0078/008C. Require the original copy BP/SS at internal stops.
Capture AX and bounded normalized output; a missing destination-normalization
stop is not itself proof of no filesystem effects in other helpers.
Before/after, inspect only the owned run's drive-c/darkrun.GFF: refuse links,
junctions, directories or files above 128 MiB and record existence, length
and a bounded SHA-256. Do not modify the file or inspect the installed game.
Stop at the copy return before wrapper cleanup/reader work. Compare net
file state only; equal hashes do not exclude transient writes. Controls
cover frame identity and file absence/content/link/size bounds. No synthetic
game state or forced error is introduced.

## Rejected-copy outer continuation

The fixed `load-return` stage may continue only an observed copy AL-zero
return. Record the wrapper return at overlay 192 +0659, CHARSAVE open
continuation +0661, PREF request/return +066F/+0674, GREQ request/return
+0743/+0748 and close continuation +077D. Verify original numbered-load
SS/BP for internal stops, capture only the declared outgoing tag/ID/output
pointer and AX at returns, and stop at its verified caller +0BE2 with
matching SS/SP. Do not continue outer action cleanup or infer settings
application from resource requests alone. No raw resource payload is read.
Unknown RNG/entry/frame paths fail closed; only the admitted failed-copy
branch may continue. Synthetic controls cover resource request layout and
tag/ID mismatch; reused stack controls reject nested return confusion.
Before resuming the wrapper after copy return, reread its declared caller
return at wrapper BP+2 and numbered-load's saved return frame. Require
offsets 0659 and 0BE2 in one current overlay segment, and verify the loaded
0659 argument-cleanup/CHARSAVE-call instructions. Use this validated mapping
for continuation stops; a segment observed before preparation is insufficient.
Controls reject mismatched frames, offsets and loaded instructions.
At the final-helper call, a capability attempt may watch only the two-byte
saved segment of the declared numbered-load return frame. A change requires
rereading that same far return, retaining offset 0BE2 and updating only its
breakpoint. Never write the frame or generalize this to arbitrary watches.
Validate the loaded caller's jump to the shared cleanup and original SS/SP at the eventual
return. Record each relocation and retain failed attempts; a settings-return
observation does not establish completion of the later helper.
Bound this extended stage to 1024 observations, retaining the 512 bound
for earlier stages. The prior mapped attempt reached the settings stops
but exhausted 512 observations in the later helper; the current-pin startup
route also required more than 512 observations. More time alone does not
admit success: the matched frame and caller stop remain required.
The final opcode guard must check +0BE2's short jump to +0BF2, not an
argument-cleanup instruction at +0BE2. Retain observed bytes locally before
validation so a rejected guard remains diagnosable.
