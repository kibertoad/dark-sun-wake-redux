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

## Native saved-game prerequisite

For Q-COMBAT-063's nonempty-source case, native_save.py may restore only
the verified two-character map checkpoint, deliver F1, admit Save window
18500 with mode zero, selected row one and top zero, then deliver Enter.
No text, mouse action or memory seed is supplied. FND-SAVE-004/008 and
FND-UI-037 describe the native writer and action. Resolve the callback's
declared +0785 trampoline and verify the +0BEF near call to +03FB; observe
that Save entry with row argument one and return +0BF2. Track only that
declared return-segment word as overlays move and verify original SS/SP
and the caller's POP CX at return. The native game may write its owned
private files; the probe never edits a save or the installed originals.

Bound the route to 1024 observations with existing sound/script RNG guards.
Compare only the fixed private SAVE02.SAV metadata, rejecting links and
oversize files. A new nonempty file and matched Save return are evidence
of this route, not proof of every save resource. After return, require
confirmed Enter delivery and the admitted map state at the native input
builder before saving a file-checked saved-map checkpoint. Preserve all
failures locally. A separate new-process strict restore must match named
state, frame and native-save digest before this checkpoint admits loading.
Synthetic controls cover the Save call mapping, wrong mode/row/top, return
offset, fixed-file names and bounded metadata. No proprietary fixture or
rebuild behavior changes. Run affected tooling gates before its tooling
commit; record runtime findings separately.

The first post-Save attempt returned to mode one/global map handler with
A11D null, so the earlier current-window predicate was not met. A revised
attempt must retain that null value and independently read the bounded list
head's WIND tag, number 19501 and ready flags. Require count one, null event
target, map global hook, no Save/Load window/list, zero exit byte, expected
party/leader and completed Enter delivery at the native input builder.
This is a candidate keyboard-loop checkpoint, not proof of graph membership
or F2 delivery. A later actual F2 entry is required before using it for the
nonempty-source experiment. Reject unexpected tags, fields or live windows.
Cache each terminal input-delivery observation. Do not keep querying a
completed operation while later game work runs: old operations may leave
the bounded debugger journal. Controls require pending handles to be polled
and terminal handles to be reused locally without a second server query.

## Native nonempty-source copy

The fixed `saved-copy-return` stage may use FND-COMBAT-140's independently
verified saved-map checkpoint and native SAVE02.SAV. Verify its pinned state,
executable, named post-Save keyboard-loop predicate and native-file digest.
Require actual F2 delivery and ready Load window before Enter, then retain
the existing event, caller, copy-frame and terminated-input guards. Accept
only the fixed rooted source SAVE02.SAV and working darkrun.GFF on C:,
with distinct private host files; no file is injected or edited by the probe.
Record source/working metadata before/after, both normalization returns,
direct open returns and length-helper DX:AX. Stop at the matched copy return
before archive restoration. Matching file hashes establish observed output,
not every I/O call or later resource provenance. Reuse bounded named-file
and terminal-delivery helpers. Controls cover source-kind admission, fixed
path-pair rejection and preserved legacy source admission. Required tooling
and separate research gates still apply.
Also sample only the fixed working file at the direct destination-open
return (+0091), before transfer. This distinguishes a net-equal copy from
observed truncation/repopulation when source and destination initially match.

## Successful-copy reader admission

The fixed `saved-reader-entry` stage may continue only a nonzero native
copy result. Observe the declared region-preparation call through resident
56BD:00C0, requiring its caller +0F26, original wrapper frame, expected
arguments and loaded +0F21 call. Watch only that call's saved return-segment
word while overlays move. At return, validate cleanup and the near call
to overlay 187 +0A6C, then stop at that reader entry before its body runs.
Require return +0F2F and selector zero. Retain copy-return, region-entry,
region-return and reader-entry snapshots of named DS fields: 167A, 144E,
145A, 0DAB, 140C, 9D9B, 9D9F, 9DA3 and 9D8E. Read only each nonnull
active/head record's evidenced +28 handle, with real-mode bounds; those
snapshots alone do not establish allocation lifetime or resource provider.
No graph traversal, direct reader invocation or setup write is admitted.
Controls cover region/reader frame offsets, selector, near-call and record
field bounds. Stop before any SAVE payload restoration or provider claim.
FND-EXE-562's trap-form return must be handled explicitly: an unloaded
overlay uses resident header 56EF:0000 and header word +2 retains +0F26.
Accept only that declared encoding (or its documented loaded-segment
transition with offset zero). At the header stop require the original
post-far-return SS/SP, validate trap form and saved offset, then remove
the stack-word watch before INT 3F reuses that storage. At manager
4AE5:0559 require the corresponding interrupt frame and loaded header
segment/return +0F26; ignore nested IRET frames without admitting them.
Then stop at the reloaded caller and retain the original frame/code checks.
Controls cover trap/loaded encodings and wrong header/IRET offsets/segments.

## Request-time provider observation

For Q-COMBAT-064, extend the verified native-source route with a fixed
`saved-providers` stage. After selector-zero reader admission, observe resident
38FF:07B1 requests. FND-CONFIG-151 and FND-COMBAT-120 describe its complete
transfer arguments and provider-local selection; FND-COMBAT-035 describes the
selector-zero SAVE request order. Accept target requests only through the
04AB wrapper's return +04D0, zero start, full-length sentinel and nonnull
in/out pointer slot. No guest writes or direct calls are introduced.

For SAVE/24 and SAVE/26, stop at +094A after destination selection but before
archive preparation. Match SS/BP to the admitted core entry; retain provider,
recorded length/offset, selected count and destination from the named locals,
the provider's +28 handle and +4 I/O handle, and active/head fields. Bound all
reads; no directory or graph traversal. Record SAVE/24's matched +09D4 return,
AX/error/count and then continue to SAVE/26's pre-preparation stop. A rejected
or failed SAVE/24 stops the experiment. Earlier ordinary requests may run;
they do not prove their transfers or lifetime preservation. This stage proves
only observed provider/length selection, not payload provenance, full loading,
destination capacity or all fallback/missing-resource paths.

Synthetic controls reject wrong wrapper frames, ranges/counts, target IDs,
core stack frames, invalid selected lengths/offsets and wrapping field reads.
Run the affected tooling gate before committing the probe, then record the
native result in a separate research batch. Count every loop continuation
independently of the legacy generic-observation `polls` field.

## Matched target payload comparison

For the same Q-COMBAT-064 route, `saved-payloads` may continue each target
to its matched successful core +09D4 stop, including SAVE/26. Retain the
provider selection/frame guards. Compare only the selected destination span
with the selected offset/count in the fixed private darkrun.GFF. Admit lengths
1920 for SAVE/24 and two for SAVE/26, as observed in FND-COMBAT-143; reject
other sizes rather than introducing an arbitrary memory/file reader. Bound
each memory read to 261 bytes and prohibit segment/real-mode wrapping. Verify
the regular, bounded, link-free private file's whole-file identity still
matches copy return before and after each comparison. Persist only comparison
results and digests in the local report, never payload bytes in Git.

Acceptance: both matched core results/counts and both byte comparisons pass;
stop at SAVE/26's return before subsequent reader work. A mismatch remains
diagnostic and ends the run. Byte equality proves this observed span's content
matches the copied private file, not DOS handle ownership, every directory
entry, capacity/lifetime, all fallback paths or complete loading. Synthetic
controls cover unequal bytes, exact size admission, short reads, segment
wrapping and file-span bounds. Tooling and findings remain separate batches.
