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

## Open-to-provider and directory-row binding

For Q-COMBAT-064, a fixed `saved-origin` stage extends saved-payloads.
After successful native copying, observe resident 38FF:0066 for the fixed
rooted working filename. FND-COMBAT-110/127 describe its wrapper and
publication contract. Require return +0042 with the declared +003D call,
option two, reserve 04D4 and output DS:144E. Match the opener frame at
the +0096 I/O-open return and +02B1 success epilogue. Record its returned
I/O handle and constructed record pointer/+28 handle; require publication
as active/head and the working output handle at that stop. No arbitrary
path, direct open or setup write is introduced.

At both target selections require record/handle equality with that observed
publication. Read only the selected type/entry pointers from the core locals,
the provider's directory position/length and those bounded plain-table rows.
Require SAVE type, plain flag, table and row extents within the provider's
contiguous directory, aligned row index below count, and matching number,
offset and length. Compare the selected twelve-byte row with its corresponding
private-file span under unchanged whole-file identity. FMT-GFF-001/002/003
and FND-CONFIG-038 describe the file/directory layout. Reject indexed or
wrapping layouts; do not add an unbounded table or graph traversal.

Acceptance: matched fixed-path open/publication, both provider/row bindings,
and existing matched transfer/payload comparisons. Preserve diagnostics on
failure. This is one observed open-to-request record/content chain, not proof
against intervening close/reuse/aliasing, unique DOS handle ownership at all
times, all graph/fallback paths or completed loading. Synthetic controls
cover wrong open arguments/frames/results, record mismatch, indexed tables,
out-of-directory/misaligned rows, wrong metadata and unequal private rows.

## Reader-return preservation boundary

For Q-COMBAT-065/066, `saved-reader-return` extends saved-origin through
the full selector-zero reader's matched caller return +0F2F. FND-COMBAT-035/111
describe the loop and reconstruction; FND-EXE-562 and FND-COMBAT-142 describe
the overlay trap-return contract. Arm only that reader's saved return-segment
word after validating its selector-zero entry. Admit only the declared header,
loaded segment and return offset; remove the watch before trap interrupt-frame
reuse, and match the corresponding manager IRET before accepting relocation.
Require the original post-far-return SS/SP and caller POP CX / saved-AL store.
Record AL as the transfer-loop result, not proof every child succeeded.

Retain each already-compared target's reference bytes only in process memory.
At reader return, resample the same bounded destination spans against those
references and record equality or change. Also sample the two declared source
pointer slots for equality with the transfer destinations. Changes are evidence,
not a reason to discard the run. This establishes endpoint retention or change
for one route, not continuous nonaliasing, allocator capacity, absence of writes,
every child effect or completed loading. Stop before the caller consumes AL.
Keep existing sound/script RNG guards; any other draw rejects continuation.
Use a 1024-observation bound for this extended reconstruction route.

Synthetic controls cover allowed return encodings, wrong header/offset/frames,
trap watch removal, matched versus nested IRET and caller opcode rejection.
Tooling is committed after its affected gate; native findings remain separate.

## Actual map-child request and flag comparison

For Q-COMBAT-066, `saved-map-request` extends saved-reader-return using
FND-COMBAT-147 and FMT-REGION-003. After both admitted SAVE transfers,
observe only the fixed region-50 opener and GMAP/50 request. Admit the rooted
C:\RGN032.GFF open through the existing wrapper with option 0A, reserve 04D4
and output DS:145A; retain matched I/O and record-publication checks. Record
its counter state. GMAP must use 04AB's complete-transfer frame and the map
helper's declared +1D30 caller, with in/out slot DS:0538. Compare its selected
plain row against the corresponding row in the fixed private source file
whose record/handle tuple matches an observed publication. Admit only the
working archive or fixed region archive as candidates, never arbitrary paths.

At the matched successful core return, compare the 12544 destination bytes
with that file span under unchanged bounded whole-file identity. Read only
the named map and two bitmap pointers, with two 1568-byte planes. Bound each
memory request to 261 bytes, prohibit segment/real-mode wrapping, and record
physical overlap among map, planes and the two effect spans. Compute the
expected map from the transferred bytes and the two cached planes using
FND-COMBAT-147's column-order bits and row-order destination bytes. Keep
all bytes in local process memory; report only identities, digests, overlap
and comparison results. A nonstandard resource length stops the probe.

At the existing matched reader return, compare the same map span against
that prediction and record its current pointer binding. A mismatch or binding
change is evidence, not discarded output. Retain the existing effect-span
checks and RNG guards. This observes one map-child transfer and resulting
endpoint; it does not establish continuous lifetime, every child effect,
all source/failure cases or completed loading. Synthetic controls cover
GMAP frame/caller/slot rejection, region-open/publication contracts, fixed
file names, map/plane bounds, bit ordering and physical-overlap arithmetic.

## Actual object re-placement census

For Q-COMBAT-066/045, `saved-objects` extends saved-map-request through
the same matched reader return. FND-COMBAT-077/149 identify the flags-zero
loader caller, OJFF slot and resident children. After the matched GMAP
transfer and before object reconstruction, sample only the entry-index
words of all 320 resident presentation records. Record active slot indices
and slot 319's marker; do not infer universal save admission from one state.

Observe resident 31E0:0EFF entries only from overlay 187 +0C3F with its
declared +0C3A call, slot below 320, zero flags and resident DS. Match each
resident +1288 epilogue by original SS/BP, without interpreting it as an
observed far return. Match OJFF through the 04AB wrapper and loader +0F40
caller; require the local in/out slot and its stack destination. Observe
selected provider/length at core +094A and matched core +09D4. Admit the
documented sixteen-byte OJFF span; retain failures diagnostically.

Count only the declared child entries while a loader frame is active:
record initializer, replacement allocator, eviction helper, release helper,
script-chain insertion, image selector and animation setter. Record minimal
stop/frame metadata and bounded named arguments, not full repeated debugger
state or resource bytes. Reject nested loaders/unmatched core frames rather
than inventing an unrestricted tracing fallback. Compare the pre-loop slot
census with actual ordered loader calls at reader return and sample slot
319 again. A difference is evidence, not discarded output.

Use a 4096-observation cap: it covers the fixed 320-slot loop's ordinary
monitored paths with the admitted entry route while retaining a bound on
extra child activity. Keep existing RNG, source, map and effect checks.
This establishes one reached-call census and OJFF stack-span case, not all
free-list, allocation, alias, animation, save or failure paths. Synthetic
controls reject wrong callers/flags/slots/frames and OJFF bindings/lengths,
exercise matched epilogues and verify census/child-count reporting. Tooling
and native findings remain separate validated batches.

## Native reverse script-conversion comparison

For Q-COMBAT-066, `saved-script-conversion` extends saved-map-request.
FND-COMBAT-151 defines the converter's local prerequisites. Observe only
GPLI/1's resident size-wrapper entry with the declared converter +184C
caller, output SS:BP-08, initial null table slot and reader +0CC0 return.
Require the selector-zero low flag and DS=SS=relocated resident data.
At the common size-lookup +0548 stop match its nested frame and capture
selected provider/type/length, then match wrapper +0613. Capture the actual
data request through the converter +1879 caller and slot SS:BP-04, selected
provider/length at core +094A and matched successful +09D4 result/count.
Admit the nominal 7896-byte table; preserve refused other cases diagnostically.

After transfer and before remapping, cache that bounded table, the named
chain head and each node's four pair words/link, and the 2600-byte trigger
table. Bound the chain to the 48-record resident pool identified by
FND-PARTY-119, reject cycles/out-of-range links, and require nonwrapping
reads of at most 261 bytes each. Compute first-match reverse conversion
from actual captured table order, retaining unmatched pairs, trigger gates
and untouched fields. Cache raw bytes only in process memory; reports retain
metadata, counts, digests and comparisons.

At 444C:0092 entry from converter +1A27, match the original converter
frame and selected buffer argument, compare chain/trigger state with that
prediction before release, then continue to the existing matched reader
return and compare again. Matching queried/selected providers and lengths
is an endpoint observation, not continuous identity or file provenance.
Record sampled physical overlap of table, trigger, script fields and locals;
reject unsupported near-local segment relationships. A comparison difference
is evidence, not discarded output. Keep existing map/effect/RNG checks and
the 1024-observation bound; do not repeat the expensive object-entry census.

Synthetic controls cover caller/slot/frame and chain-range/cycle rejection,
first-match order, unmatched values, zero-script gates, unchanged bytes,
size narrowing and prediction comparisons. This admits one normal conversion
case, not every incoming route, alias, allocation, failure or diagnostic
behavior. Tooling and native findings remain separate validated batches.
Send the accumulated Python synthetic controls through standard input to
the existing isolated interpreter, preserving arguments and assertions;
their inline command now exceeds Windows' command-line length limit.
Also resample the retained queried length immediately after transfer and
the queried length/word scan bound before release, requiring 7896/1316
for this fixed case. This makes arithmetic-input retention explicit rather
than inferring it from a matching prediction.

## Native item clear and enumeration boundaries

For Q-COMBAT-066, `saved-items` extends saved-script-conversion, without
the object census. FND-PARTY-236 and FND-CONFIG-143/145 supply the static
attempt. At loaded reader +0CC1 match its original SS/BP and resident DS,
then cache the bounded 9200-byte kind-1 table. At resident 2D40:2196
require reader +0CEA, index equal to current DS:1A32, and fixed output
5072:0040 (relocated). Compare the table against a prediction clearing
only each of 400 words +0C. Record endpoint pointer identity, changed and
nonzero word counts; equality does not prove continuous pointer lifetime.

At matched enumeration +240D record AX, bounded output through ordinal 79
plus its sentinel/count bytes, and DS/SS relationship. Count reached
1AA0:0566/051C entries only while enumeration is active, without claiming
their individual writes or every internal path. Compare table and output
again at matched reader return. Record physical overlap with the existing
effect targets, output, item table and enumeration local frame. Reject
wrapping/null spans and wrong caller/frame/arguments. Keep bytes only in
process memory and licensed local reports; commit metadata and findings.

Acceptance: synthetic controls reject invalid bounds, wrong frames and
arguments, and distinguish exact clear prediction from unrelated writes.
The native run records differences as evidence. This admits one normal
boundary case, not helper termination for all inputs, continuous lifetime,
failure paths or complete loading. Tooling and research remain separate
validated commits; Exit is the matched reader return with retained guards.

## Enclosing restore caller epilogue

For Q-COMBAT-066, `saved-caller-epilogue` extends saved-items beyond the
matched reader return. FND-COMBAT-043 supplies the static attempt. Preserve
the existing reader-return comparisons, then match the parent SS/BP at its
+0FA1 post-region-setup boundary and +0FB5 epilogue. Capture saved reader
and open-result bytes, current reuse/working-handle/archive fields and AL.
At resident close 38FF:02B5 admit only the parent's +1C3A helper return,
matching current DS:144E; match the resident close epilogue before recording
its result at +0427. Confirm exact caller/epilogue offsets from the complete retained
static bodies, not byte-pattern searches. Refuse unsupported overlay reloads.

At parent +0FB5 repeat item/output, script, map and effect endpoint checks
using the same cached references and report differences. This observes an
epilogue, not a far return or completed visible loading. Do not infer every
placed-record write, intermediate callback effect, handle lifetime or failed
reader result from this normal case. Keep the existing RNG admission and
1024-continuation bound. Synthetic controls reject mismatched frames and
close arguments; Exit is the matched parent epilogue with retained report.
