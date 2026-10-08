# Actual poll caller-root acceptance, 2026-10-05

Read-only inputs: FND-CONFIG-162/164 and the committed function inventory.
Published reader 2.1.0 and engine 10.0.0 ran the existing inventory-batch
incoming-call census against the poll wrapper. It confirms resident and overlay
callers, but reports partial searches and unresolved sites. It proves no absence
of other callers; the older caller sites remain part of the required coverage.

A fresh query from the first aliased caller's real entry traces its own prologue,
setter and both pointer argument producers into the actual resident wrapper.
It supplies neither SP/BP nor entry memory. The interrupt return and frame-word
preservation remain explicit conditional models, not native driver evidence.
At the reached actual loop predicate, the checkpoint proves the second (DX)
store is the aliased scratch word's last writer, and the branch origin is the
interrupt's BX. The wrong first-store writer is rejected. Omitting the wrapper,
unmodeling the interrupt and one-step limiting remove both control witnesses.

A newly confirmed small overlay caller also produces its own frame and two
distinct output pointers before the wrapper. Its subsequent undeclared call
stops the route; it is not credited as complete downstream coverage.

The aliased caller retains repeat-step and dropped-path gaps. Whole controls
remain undecided. Gap 31 stays open for full required caller/callee coverage,
interrupt/result sequences, admitted storage and downstream input producers.
No budgets were increased, no state was stitched, no original game was run,
and no game spec claim or parity status changed.

Source-local configs/reports: GAME_DIR/analysis/reporter-audit/
issue5-poll-callers100.

## Connected result-dependent continuation

Separate explicit interrupt cases supply BX zero and BX one. The actual
prologue-root trace takes the caller's exit branch for zero and reaches the
real state-clear entry; one takes the repeat branch. Both retain the root's
actual pointer formation and last-writer/predicate controls. The downstream
state-clear and status bodies are declared, not substituted returning calls.
Removing state clear or using a one-step cap removes its entry witness.

The zero case stops inside state clear at the unchanged step bound; pointer
wrapper and registration alternatives retain dropped paths and an unmodeled
interrupt. The one case retains a poll repeat stop. Neither is a driver
observation, whole termination proof or inherited downstream state fixture.
Source-local reports: GAME_DIR/analysis/reporter-audit/issue5-poll-progress100.
Driver: tools/research/engine100/poll-progress.mjs.

## Both aliased callers and repeating argument structure, 2026-10-05

Published engine 10.0.0 bounds reports now cover both complete documented
FND-CONFIG-161/162 caller bodies and the FND-CONFIG-164 interrupt wrapper.
Joint declared regions resolve the outer helper's actual near call to its
first local callee, rather than leaving that target undeclared. Each local CFG
reaches its documented far return without holes or unresolved branch gaps.
Calls and the interrupt still carry explicit continuation assumptions. This
is local CFG coverage, not proof that every external call returns normally.

An independent bounded instruction-structure check verifies both callers'
prologues and allocation of the local scratch word. Immediately before each
poll call, each pushes current SS, forms BP minus two in AX, pushes that offset,
then pushes the same SS and AX for the other output. Neither pair is formed
from DS or from the scratch word's previous contents. After the call each
removes the argument words, tests returned AX bit zero, and branches back to
the beginning of that same argument-formation sequence. Thus repetition does
not locally change which pair is passed or replace the register predicate
with the overwritten scratch word. This conditional structural check covers
both documented callers, not just the previously traced first caller.

The complete wrapper has no own branch or further call around the two output
stores. It loads the first supplied far pointer and writes CX, then loads the
second and writes DX, then copies BX into AX before restoring saved registers.
Under unchanged valid argument storage and the stated external preservation
conditions, equal output pointers therefore retain DX and the caller's bit
test follows driver BX. Saving/restoring caller BP does not prove that an
interrupt leaves the wrapper's current BP, output argument words or other
memory intact. Prior callees can also affect caller-frame state; those inputs
and effects remain acceptance conditions, not a newly established ABI promise.

One-instruction controls for either caller lose its poll-call witness; the
wrapper control loses its interrupt witness. They remain incomplete and are
not counted as absent calls or hardware. Existing wrong-first-writer, unread,
unscoped and symbolic-cap controls are retained. No symbolic traversal bound
was increased, no result sequence or original memory was supplied, and no
function/game was executed.

Configs, complete numeric reports and the argument/back-edge summary are local
under GAME_DIR/analysis/reporter-audit/issue5-poll-cfg100. Drivers/assertions:
tools/research/engine100/poll-cfg-bounds.mjs, poll-cfg-reading.py,
poll-cfg-controls.py and poll-cfg-joint-controls.mjs; corresponding logs are
ignored artifacts. The independent structure check does not claim that its
summary is a new published API field.

Gap 31 now has verified local CFG and repeating argument/predicate structure
for both named aliased callers. Whole connected frame/callee preservation,
admitted storage, driver result sequences and downstream producer acceptance
remain unverified. Static back-edge coverage is not a finite-termination or
whole symbolic-control pass. Existing game findings/parity stay unchanged;
all five full connected exits remain open. Next trace the particular prior
callee or interrupt preservation condition needed by a missing whole control,
rather than repeating an unknown-result loop with a larger budget.

## Exact stopped routes and downstream input guards, 2026-10-05

A census of the current root, zero-BX and one-BX reports distinguishes their
actual stops. The retained route reaching the poll has returned through the
wrapper and then reaches the ordinary instruction limit: after the zero case
it reaches the state-clear entry, and after the set-bit case it starts another
argument-formation sequence. Other retained routes stop in the setter's DOS
guard or at the instruction limit in the setter. Dropped paths remain in the
pointer/runtime and setter regions. Those reports do not establish a lost
frame on the reached poll route. Do not describe all these distinct limits as
one unresolved frame-preservation failure, or rerun them with larger budgets.

Fresh published call graphs identify the state-clear and status bodies' local
inputs and unread services, using FND-CONFIG-166 as a read-only input. The
state-clear graph resolves its local forwarding call, while its service calls
and the forwarder's external child remain unread. Local CFG completion and
boundary usability do not make those callee effects complete. An initial query
also declared the unrelated status entry without reaching it; the reporter
correctly retained unchecked-entry dependencies. The clear-only query removes
that unused declaration, not an actual required callee, and retains unresolved
external-service edges. The broader status graph and its exclusions are kept.

The published caller-order report verifies necessary guard edges before the
clear-forwarder call. It reports a normalized-register bit test followed by a
word-width DS-relative state test. The first is not a test of bit zero in the
raw pointed configuration word: the body first converts that word to a
zero/nonzero result. Reading only the adjacent register test would lose the
input's actual meaning. The status body uses the same normalization and state
gate. The pointer and current pointed word remain native input conditions;
FND-CONFIG-020 supplies a known configuration-pointer producer, not a complete
history of this later buffer, its extent or intervening writers.

Focused post-load register hypotheses verify this distinction through the
actual source instructions. For zero, both local bodies return completely,
with observed prologue frames and no external call on those conditional paths.
Nonzero, high-bit-only and all-bits-set hypotheses reach the state gate; unread
active-service paths retain incomplete reports. No configuration memory value
is supplied. These AX hypotheses are not observed native buffer values or a
connected producer witness: the frame-prefix observation does not establish
that the original load produced them. They cannot be substituted for the
missing caller/buffer/state provenance or a whole Gap 31 control.

An instruction-limit caller-order control loses the required accepted guard
ordering. Existing wrong-writer, unread, unscoped and poll-cap controls remain.
No old poll budget was increased, native driver sequence invented, original
function/game executed, or game spec/parity changed.

Original-derived configs/reports stay under
GAME_DIR/analysis/reporter-audit/issue5-poll-state-inputs100. Drivers/logs:
tools/research/engine100/poll-blocker-census.py, poll-state-inputs.mjs,
poll-state-guards.mjs, poll-state-prefix.py and poll-state-normalization.mjs.
The normalization and caller-order assertions are recorded in their logs.

Next connected inputs: current configuration-pointer/pointed-word and state-word
producers and intervening writes, alongside the setter's DOS condition. Keep
instruction/path limits, external return/preservation conditions and missing
input evidence separate. Whole connected controls for Gap 31, and all five
full exits, remain unverified.

## Configuration-pointer producer and state-write candidate, 2026-10-06

The current licensed configuration still matches FND-CONFIG-003's recorded
identity and field description. Its relevant pointed-word source value is
nonzero. The earlier post-load AX-zero tests are therefore conditional decoder
controls, not evidence of the shipped-file input or a native zero-guard path.
FND-CONFIG-019's loader does not establish a full-length read by comparing a
non-error read count with the request, and later buffer writes remain unresolved.
File identity alone does not prove the later resident field's value.

A fresh published bounds report covers the receiving window documented by
FND-CONFIG-020 through its local return. Its near initialization callee is
outside the declaration; the local CFG's return-continuation assumption does
not prove that callee's effects. The actual trace retains a reached four-byte
DS-relative pointer store copied from the argument load. Source-origin,
full-width last-writer and store-before-initializer-preparation controls hold
at that reached store/preparation boundary. A wrong argument-load-as-writer
control is rejected, and the one-step control removes all anchors. Whole
controls remain undecided at the unread initialization call. No original
pointer, configuration memory or field value was seeded.

The pointer probe is before the next stack write. It establishes the field's
writer at that boundary, not preservation across that stack write or callee,
physical DS/SS disjointness, valid native pointer storage, successful file load,
or producer-to-status joining. Returned bypass paths do not establish the
pointer-store route's completion.

A function-boundary-independent literal census identifies state-word candidates
across the shipped executable. Literal equality is not an instruction use or a
runtime-storage identity. In the already documented windows, decoded operands
retain the status/clear word reads and the clear's word write. A separate
inventory-entry candidate has a matching DS-relative word write verified by
the published CFG/operand reporter. Its source query window ends at the adjacent
inventory entry, rather than at start plus body-byte count; the bounded CFG
independently reaches the final return. The instruction-limit negative loses
that writer's accepted instruction ownership.

The additional write remains a candidate: its caller reachability, DS formation
and preservation, aliases and connection to the consumer's current field have
not been established. Other raw candidates and indirect/aliased writes are
unread. Neither the literal census nor the local candidate report proves an
exhaustive producer list or that the state word is stable. No new native state
claim or game finding/parity status is added by this tooling acceptance.

Reports/configs and the licensed configuration summary remain local under
GAME_DIR/analysis/reporter-audit/issue5-poll-sound-producers100. Drivers/logs:
tools/research/engine100/poll-sound-producers.mjs, poll-sound-write-reading.py,
poll-sound-pointer-controls.mjs, poll-state-word-literals.py,
poll-state-writer-candidate.mjs and poll-state-writer-negative.mjs.

Next: the candidate writer's actual incoming route and segment provenance,
initialization/later buffer writers and the full read-to-consumer handoff.
Do not use the shipped file as entry memory, equate DS offsets without segment
proof, or turn a zero-register hypothesis into a native continuation. All five
full connected exits remain unverified; issue 290 remains open without delivery
information at this check.

## State-write candidate's incoming callers, 2026-10-06

A relocation-aware incoming-call search finds two direct far-call candidates
for the additional state-write window. The known poll call is a positive
search control. The search reports no unresolved relocation candidates, but
excludes near, computed and unrelocated callers; it is not an exhaustive caller
claim.

Published call-order analysis now confirms both candidates as entry-path
instructions within the source spans documented by FND-SOUND-007 and
FND-SOUND-008. Both caller boundaries and ordering are usable at the configured
bounds, with no unchecked declared entries. The one-instruction negative loses
both confirmed calls and usable ordering. Necessary CFG edges remain structural
conditions, not observed runtime values or proof that earlier callees preserve
the tested fields. Return-continuation assumptions remain explicit.

An independent whole-span instruction reading verifies that both calls push
the filename-buffer offset already described in those findings. Neither caller
directly writes DS. That scan does not prove transitive DS preservation: earlier
calls, including the second caller's conditional status/clear route, remain
relevant. The argument offset alone establishes neither initialized buffer
contents nor valid storage, and cannot join the candidate's DS-relative write
to the poll consumer's physical field.

This replaces the candidate's wholly unverified incoming route with two verified
direct caller paths and their immediate argument formation. It does not promote
the candidate to an exhaustive native producer or complete the read-to-consumer
handoff. The existing source findings remain read-only; no native behavior or
parity status changes. All five full connected exits remain open.

Private reports/configurations: GAME_DIR/analysis/reporter-audit/
issue5-poll-state-incoming100. Drivers:
tools/research/engine100/poll-state-incoming.mjs, poll-state-caller-order.mjs
and poll-state-caller-reading.py.

Next: resolve the specific DS-preservation and buffer-production dependencies
on these verified routes, keeping relocation-search exclusions and callee-return
assumptions. Do not expand unrelated sound behavior or seed native memory to
manufacture a connected positive.

## Buffer consumer's guarded helper handoff, 2026-10-06

The additional state-write candidate's bounded instruction reading follows the
argument into a saved register and three helper calls. The state-word store
writes the immediate value one only on the route passing their result gates.
This is source-local producer structure, not evidence that native services
succeed or that its DS-relative field is the poll consumer's physical storage.

The final helper has independently verified bounds inside its inventory-adjacent
query window. Its reached interval ends at the decoded far return; the inventory
body-byte count was not used as the window endpoint. The local report is complete
only under the explicit return assumption for its unread external callee.

Published caller-order controls now retain the helper call and its required
configuration-bit, configuration-byte, prior error-word and prior nonzero-result
edges. The caller's argument cleanup is observed after assumed return, not
callee success or preservation. The one-instruction control loses confirmed
ordering. An initial undeclared-target query was rejected; adding a declaration
only after the independent bounds check makes that failure explicit rather than
treating missing code as a negative caller result.

Independent helper reading shows the actual filename-offset parameter is paired
with the current DS for its external call, alongside the caller's mode argument.
Its return is copied at word width and tested against the all-ones error value.
Neither a near offset argument nor the helper's explicit DS argument proves that
DS was preserved along earlier calls, that the buffer is initialized and bounded,
or that the external service returned successfully. These are now identifiable
dependencies of this connected route, rather than reasons to read unrelated
sound behavior.

Private source readings and reports remain under issue5-poll-state-incoming100.
Drivers: poll-state-consumer-reading.py,
poll-state-helper-bounds.mjs and poll-state-consumer-guards.mjs under
tools/research/engine100. No original instructions enter Git; no source finding or
parity status changes. Full connected acceptance remains unverified.

## Filename service and segment-restoration structure, 2026-10-06

The buffer helper's external filename service now has independent published
bounds over the inventory-adjacent source window. The reporter reaches its
shared far return and explicitly records a DOS interrupt and three optional
unread near calls as continuation assumptions. The one-instruction negative
loses complete return and interrupt coverage.

A connected helper/service call graph resolves their actual far-call edge.
Both local node boundaries are usable, while the three undeclared near-call
edges remain unresolved. Node-local completeness is not graph completeness.
Caller-order controls independently confirm the helper's service handoff and
argument cleanup after assumed return; their capped negative loses usable
ordering. No native field memory, external results or pointer values were seeded.

The private instruction reading identifies an entry DS save, a load of the far
filename argument into DS and the offset register, and a shared DS restore before
the far return. The service uses the supplied mode to select optional routes,
and handles the DOS carry-result paths separately. Its error paths may change
DS temporarily and write error storage before the shared cleanup. The helper
continues to distinguish the full-word all-ones error value from other returns.

This establishes the service's save/load/restore structure, not unconditional
preservation across unknown interrupt/callee writes or aliases of saved stack
storage. The connected caller supplies mode one; the source-local initial mode
tests skip optional preparation on that value. Post-interrupt register tests
must still retain the interrupt's preservation conditions. Do not infer an
actual successful open, a valid filename buffer, or native DOS register effects
from this structural reading.

Private reports/readings remain under issue5-poll-state-incoming100. Ignored
drivers/logs: poll-filename-service.mjs, poll-filename-handoff.mjs and
poll-filename-service-negative.mjs under tools/research/engine100. Next dependencies
are buffer construction and its earlier callee effects, plus the explicit
saved-frame/interrupt conditions on this now-resolved service edge. This tooling
record changes no native claim, specification or parity status; all five full
connected exits remain open.

## Filename construction routes and intervening calls, 2026-10-06

Independent whole-span readings of FND-SOUND-007 and FND-SOUND-008 retain the
actual destination arguments at their four formatting call sites. The published
relocation reader resolves all four to the resident formatter documented by
FND-CONFIG-203. These are instruction-owned sites from the caller readings;
relocation candidates alone would not establish executable caller paths. Search
controls and exclusions remain in the private report; the driver checks the
reader's callSite field explicitly.

Independent CFG omission controls show that removing the sound caller's sole
formatter disconnects its consumer call. Removing all three formatter sites in
the speech caller also disconnects its consumer. Removing any single speech
formatter still leaves an alternative route. Backward traversal identifies all
three as possible last declared formatting calls before the consumer, including
the fallback rewrite route. This is a structural union of alternatives, not a
claim that one conversion dominates every native route.

The sound route has a file-check call between formatting and consumption. The
speech routes additionally retain conditional check, stop and status/clear
calls. Their exact sites remain private. These intervening calls are now a
bounded preservation-dependency list for the connected buffer handoff. A last
declared formatter call is not a last memory writer: any intervening callee,
changed DS or aliased write could alter the buffer used by the consumer.

FND-CONFIG-203's existing formatter reading separates the output count from its
extra terminator and leaves conversion admission/capacity unresolved. The sound
filenames use conversions outside that entry's literal-only path; the selected
plain decimal bound in FND-CONFIG-205 cannot establish their general output
length. The caller's destination offset does not supply a capacity contract.
Keep current format/source contents, argument-segment identity, output bounds,
termination, aliases and callee preservation unverified.

Private readings and controls remain under issue5-poll-state-incoming100.
Drivers: poll-buffer-producer-reading.py,
poll-buffer-route-controls.py and poll-buffer-formatter-relocations.mjs under
tools/research/engine100. The CFG controls assume calls return and do not prove native
branch feasibility. No native claim, specification or parity status changes;
all five full connected exits remain open.

## Intervening file-check helper's connected service graph, 2026-10-06

The three instruction-owned file-check calls in FND-SOUND-007/008 resolve through
the same declared FBOV trampoline to one overlay entry. A fresh bounds report
over its inventory-adjacent window reaches its far return; the one-instruction
negative loses complete return and both call sites. Its explicit analysis segment
is a decoding view, not a native load address. The resident-only emulator cannot
execute this overlay, and no native run was attempted.

Private instruction reading shows that the helper inspects the supplied far
filename and has a conditional state-reader loop before forwarding that pointer
with a local stack-buffer pointer to another service. Neither the helper nor its
call-free state-reader child directly writes DS or the filename buffer. Loop
termination and unchanged state are not established by a complete local CFG.

The published connected graph now resolves the overlay helper, the conditional
state reader, the filename-status service and its state getter/setter. Their
local boundaries are usable, but the status service retains an unread external
validation edge and explicit DOS-interrupt continuations. Omitting the state
reader restores an unresolved edge; the one-instruction graph loses usable
completion. Complete local nodes do not make this a whole connected pass.

The status service saves DS and other registers, calls the state getter,
conditionally supplies its caller's stack-buffer pointer to the state setter,
loads the far filename, and reaches a DOS interrupt. Its shared cleanup calls
the setter again and restores saved registers after both result paths. The state
setter has its own DS save/load/restore structure. These are observed source
operations, not proof of native DOS effects or saved-frame preservation.

The getter copies interrupt-produced ES:BX into DX:AX, then restores its incoming
ES:BX from the stack. Its caller immediately saves ES and BX into the local pair
later supplied to the cleanup setter. It does not save the getter's returned
DX:AX pair there. Conditional on intact saved storage, these are different
producer routes; calling the getter does not establish that the cleanup pair
contains the queried state. This is relevant to the original discarded-result
and snapshot-provenance contracts. Native equality, service-state corruption or
an intended bug fix is not inferred.

The previously opaque intervening call now has specific remaining dependencies:
the external validation result/effects, interrupt results and writes, validity
of the local buffer, and aliases with filename or saved-frame storage. Do not
treat the local allocation as an established service-output bound, or classify
the filename as preserved merely because the helper has no direct buffer store.
Current DS, terminator/output bounds and later writers remain unverified.

Private reports/readings remain under issue5-poll-state-incoming100. Ignored
drivers/logs: poll-file-check-resolution.mjs, poll-file-check-bounds.mjs,
poll-file-check-child-resolution.mjs and poll-file-check-connected.mjs under
tools/research/engine100. This tooling acceptance changes no native claim, source
finding or parity status. All five full connected exits remain open.

## Resolved validation edge and caller carry provenance, 2026-10-06

The file-check status service's previously unread validation edge is now included
in the published connected graph. Its bounded instruction reading tests the two
argument words for zero and sets its return/carry state; it does not validate a
storage range, access filename bytes or write DS. The graph now has usable local
bodies and no unresolved call edges in this declared scope. DOS interrupts and
callee-return assumptions remain explicit; this is not complete native behavior,
loop termination or preservation across interrupt/alias effects.

Omitting the validator restores the unresolved edge. The one-instruction graph
loses usable local completion. These controls replace the earlier unread-edge
qualification with a resolved source-local dependency, without changing the
remaining storage and hardware qualifications.

The caller adjusts SP after the validator returns and before branching on carry.
That adjustment produces the branch flags; the validator's returned carry is not
the tested carry. Actual two-instruction trace controls, using explicit post-call
SP hypotheses and otherwise unknown returned flags, retain the adjustment as
flag producer. The non-wrapping hypothesis rejects the branch and the wrapping
hypothesis takes it. Omitting the adjustment leaves both branch outcomes
unresolved, while the one-instruction cap reaches neither branch outcome.

These are conditional decoder/provenance controls. They seed no native memory,
do not prove the caller's actual SP, and stop at their stated step limit. No
native failure, admitted wrap, service success or bug is inferred. A caller's
branch after a service is not evidence that it tested the service's return flags;
the intervening flag producer must be retained.

Private reports/readings remain under issue5-poll-state-incoming100. Driver/log:
tools/research/engine100/poll-file-check-validation.mjs. The source-local
validation dependency is resolved; next are formatter bounds, other intervening
calls and this service graph's explicit interrupt/saved-storage conditions.
No specification or parity status changes. All five full connected exits remain
open.

## Connected queried-state versus caller snapshot controls, 2026-10-06

The resolved file-status service graph supplies a fresh trace from its actual
prologue through the real getter and both caller snapshot stores. It reuses the
existing poll profile's step/path/total/visit bounds. SS/DS remain explicit
hypotheses; distinct incoming ES/BX and queried ES/BX values are added solely
to distinguish their producer routes. The getter's DOS interrupt has an explicit
conditional return model and saved-memory scope covering its saved words, call
frame and the caller's saved frame through BP. No SP/BP value or entry memory
is supplied, and no native service-state guarantee is inferred.

At the caller snapshot checkpoint, AX/DX equal the distinct hypothetical queried
pair while each actual local store receives the corresponding restored incoming
ES/BX word. All four relations hold locally. The claim that the snapshot's
segment word instead equals the queried segment is rejected. Removing the
memory scope stops at the getter return with unknown return-target provenance,
before every witness. Removing the interrupt model stops at the getter's real
interrupt; one step also reaches no requested anchor. Thus saved-register and
frame preservation are explicit conditions, not incidental defaults.

The connected query then reaches the actual state setter and stops at its
unmodeled interrupt. Path-limit gaps around the validator/carry branch remain;
all whole verdicts are undecided and completeWithinModel is false. This provides
a connected conditional producer control for the already recorded distinction,
not native snapshot equality, correctness, state corruption, successful cleanup
or complete filename preservation. Neither the handler nor the original game
was executed. No native claim, game specification or parity status changes.
All five full exits remain open.

Private configs/reports: UserContent/analysis/reporter-audit/file-state-snapshot101.
Driver: tools/research/engine101/file-state-snapshot.mjs.
Bootstrap facts, offline pinned rules, hash-verified published source queries,
positive relations and false/scopeless/unmodeled/cap negatives passed.

Full assetless `tools/Test.ps1 -NoRestore` passed on 2026-10-06, including
documentation base comparison.
Original-dependent tests skip with GAME_DIR absent.

## Actual file-check root to service argument handoff, 2026-10-06

The resolved overlay file-check entry supplies a fresh connected source query
from its real prologue into the actual resident file-status service. All prior
poll step/path/total/visit limits remain unchanged, with explicit DS/SS
hypotheses and no call/interrupt model, entry-frame preset or supplied memory.
The source's own local allocation and argument formation are executed by the
published symbolic reader; no original function or game is run.

At each reached service-entry checkpoint, all ten argument bytes retain their
actual individual writers: the four-byte filename forwarded from the caller's
argument storage, the locally formed stack-buffer offset, its SS word and the
zero mode word. The stack-buffer offset's own address-formation origin holds
locally. Naming the mode push as a buffer-segment byte writer is rejected.
Omitting the resident service removes both checkpoint controls; one step reaches
neither. These controls verify the connected local argument layout and producer
route, not filename contents, unchanged root arguments, valid storage capacity
or successful service output.

The full query stops at the real getter's unmodeled DOS interrupt on the reached
service routes. It also retains a path-limit gap and a separate root-return
frame/balance stop. Neither is suppressed or described as native failure.
Every whole control remains undecided; no higher bound or preservation model
is introduced. The prior conditional queried-state snapshot query remains
separate and is not imported into this caller state. Service/alias outcomes,
formatter bounds, later filename writes and original caller admission remain
required for the complete handoff. No game specification or parity changes;
all five full connected exits remain open.

Private configs/reports: UserContent/analysis/reporter-audit/file-check-handoff101.
Drivers: tools/research/engine101/file-check-handoff.mjs and
file-check-handoff-controls.mjs. Bootstrap facts, offline pinned rules,
hash-verified source queries, positive byte-writer/origin and false/omitted/cap
controls passed.

Full assetless `tools/Test.ps1 -NoRestore` passed on 2026-10-06, including
documentation base comparison.
Original-dependent tests skip with GAME_DIR absent.

## File-check root return-width correction, 2026-10-06

The earlier handoff trace inherited a graph query with no returnBytes field.
The reader therefore declared the root near-return width, while this actual
entry ends in a far return. Its reported frame/balance stop occurred with SP
already equal to the entry SP: the failed condition was query return width.
The earlier stop is withdrawn as an unresolved source frame/balance dependency;
it demonstrates neither native stack corruption nor a LEAVE implementation
failure. Historical configs and reports remain intact.

A corrected query explicitly declares the source's far-return width. No other
register, memory, model, region or traversal limit changes. The retained bypass
path now returns; all argument-byte writer and stack-buffer origin occurrences
still hold locally. The wrong segment writer is still rejected, and omission
and one-step controls still remove both witnesses. Service routes continue to
stop at the real getter's unmodeled DOS interrupt, and the existing path-limit
gap remains. Whole verdicts stay undecided and completeWithinModel stays false.
No state from the standalone conditional snapshot query is imported.

A wholly synthetic one-instruction far-return source independently reproduces
the diagnostic ambiguity: default near width stops with the combined frame/
balance reason, while explicit far width returns with no stack mutations.
After duplicate searches, [toolkit issue 300](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/300) requests distinct failed-predicate
fields or reasons. It asks for no default change or relaxed rejection; this
was a consumer query error, not an execution defect. Native argument admission,
service/alias conditions, filename preservation and formatter bounds remain
open; all five full exits remain open. No game specification or parity changes.

Private corrected reports remain under file-check-handoff101 with far-prefixed
names. Driver: tools/research/engine101/file-check-far-handoff-controls.mjs.
Synthetic driver/reports: tools/research/engine101/return-width-repro.mjs and its
directory. Bootstrap facts, offline rules, hash-verified published queries and
positive/negative assertions passed. No original execution or emulation occurred.

Full assetless `tools/Test.ps1 -NoRestore` passed on 2026-10-06, including
documentation base comparison.
Original-dependent tests skip with GAME_DIR absent.
