# Transfer caller and coordinate producers, 2026-10-05

Read-only inputs: FND-CONFIG-183/186/191/192. Published reader 2.1.0 and
engine 10.0.0 incoming-call censuses confirm the transfer wrapper's primitive
call and resident/overlay callers of that wrapper. Searches remain partial;
these are positive callers, not exhaustive caller coverage.

A fresh actual entry query follows FND-CONFIG-186's constant-coordinate
caller into the real handle request and slot scanner. Documented CS:IP
coordinates preserve near-call semantics. No SP/BP, memory, returned handle,
call model or selected branch outcome is supplied. Conditional DS/SS values
and the existing bounded trace limits are explicit.

The request reads coordinate words 10,10,38,26 with provenance from the
caller's actual two double-word pushes. All four consumed words and their
respective producers are checked; the prologue is rejected as their producer.
Omitting the scanner or one-step limiting removes its entry witness.
The initial handle argument and slot flags/roots remain unknown.

Some paths return before transfer, some reach the undeclared wrapper call,
and scanner repeats and dropped paths remain. No transfer, port programming,
slot admission, complete capacity or native pixels are established by these
input witnesses. Gap 37 remains open for the wrapper/primitive continuation,
slot/segment/count/mask/alias producers, later writers and hardware controls.
No original game was run and no game spec/parity claim changed.

Source-local configs/reports: GAME_DIR/analysis/reporter-audit/
issue5-transfer-callers100, issue5-transfer-wrapper-callers100 and
issue5-transfer-producers100. Ignored drivers/logs: artifacts/engine100/
transfer-callers.*, transfer-wrapper-callers.* and transfer-producers.*.

## Connected slot-write and getter control

Adding the actual transfer wrapper, coordinate validator, all coordinate
getters and primitive reaches the first validator getter at unchanged limits.
A conditional scanner route produces handle zero; the caller forwards that
actual result through SI into the wrapper and validator. Its native admission
is unknown, not supplied or inferred from the initializer's separate control.

The getter reads coordinate 10 from the request's actual slot assignment.
Origin controls retain both the caller push and request-store producers;
last-writer controls identify the request store rather than the argument push.
Wrong push-as-direct-slot-writer is rejected. Missing scanner and one-step
controls remove downstream witnesses. Whole controls remain undecided.

The primitive is declared but not reached. Unknown second handle, slot flags
and fields, scanner repeats, path drops and step stops retain incomplete
validation/transfer coverage. This is a connected producer-store-consumer
witness, not hardware presentation or an admitted native handle proof.
Source-local reports: GAME_DIR/analysis/reporter-audit/
issue5-transfer-continuation100. Ignored drivers/logs: artifacts/engine100/
transfer-continuation.* and transfer-continuation-controls.*.

## Actual second-handle producer found

A fresh literal-field census finds overlay allocation-result assignments to
the transfer caller's two handle fields and later cleanup rewrites. The search
remains partial; implicit/aliased writes and excluded source ranges remain.

A fresh narrow entry at the first allocation argument boundary traces the
real slot allocator and scanner into the first handle-field store. The
preceding overlay prefix and its frame are not established by that entry.
No memory, return value, call model or concrete stack register is supplied.
The accepted allocator routes write coordinates 0,0,27,15 and paragraph count
7, then store actual returned handles zero or one in the first field. Other
routes store allocator-produced FFFF. Slot/pool admission remains conditional
on unknown flags and pool words, not proved by the separate initializer test.

Missing allocator and one-step controls remove the field assignment witness.
Accepted continuations retain scanner-repeat stops. Rejection cleanup reaches
a final frame mismatch because the narrow entry omitted its prologue; no
second-field completion, joined transfer state or valid native slot is claimed.
These are actual producer instructions and outputs, not substituted handle
fixtures. Complete prefix, initialization, intervening writers and transfer
continuation remain required before Gap 37 can close.

Source-local reports: GAME_DIR/analysis/reporter-audit/issue5-second-handles100
and issue5-second-handle-producers100. Ignored drivers/logs:
artifacts/engine100/second-handle-writers.* and second-handle-producers.*.

## Allocation parent references and stop correction

The final frame mismatch belongs to overlay rejection cleanup after a rejected
first request, not the second allocator return. The narrow allocation window
cannot prove the omitted prologue's saved state. Retain that query limitation;
there is no evidence of a native frame failure.

A new incoming census retains no caller within its partial inventory domain.
An independent relocated-pair search against the source-derived resident
trampoline finds two references outside that declared inventory. Exact-site
instruction checks decode both as far calls, and published incoming reports
resolve each to the actual allocation body. These declared call-boundary
entries prove target identity only, not routine starts or preceding reachability.
Overlay analysis coordinates were rejected as a runtime pointer query; the
correct query uses the descriptor-resolved resident trampoline. No validation
was disabled to obtain the references.

The next evidence is the two actual caller prefixes and their initialization
order. Neither supplies state to the earlier allocation or transfer query.
Source reports: GAME_DIR/analysis/reporter-audit/issue5-allocation-parent-callers100.
Ignored drivers/logs: artifacts/engine100/allocation-parent-* and
allocation-trampoline.mjs. Whole Gap 37 remains open.

## Parent ownership and inventory repair

FND-UI-037 identifies the first parent as the Save/Load callback; FND-UI-035
and FND-CONFIG-028 identify the second as the Start Game callback. The latter
finding already records its call to the allocation entry with zero. The
Save/Load action continuation likewise has an actual zero push before its
confirmed call; its earlier mode test/callees remain conditions.

The old acceptance search treated body-byte counts as contiguous ranges,
leaving confirmed calls outside its declared search domain. The Start Game
callback also lacked an inventory entry. Verified source spans precede the
documented external dispatch tables and are declared independently in queries.
Original analyzer measurements and interior entries remain; the new callback
entry uses the published reporter's measured partial body count. These rows
contain no original code, bytes, strings or names and claim no complete reading.

Future caller searches must include these ranges and preserve unresolved
computed dispatch. The next work is their actual prefixes and initialization
order, not a repeated old partial census. Whole Gap 37 remains incomplete.

## Actual callback roots and declared dispatch continuations

Fresh traces from both documented callback prologues retain their actual
frames and unknown event/control inputs. Ordinary paths stop at computed
dispatch, bounded selector loops or unread callback callees. Allocation is
not reached on those ordinary paths.

The delivered indirectJumps mechanism reads the documented source word tables.
Declarations are non-exhaustive and name both table layout and consumer
evidence (FND-UI-035/037); they assign no event, control ID, register or memory.
Separate declaredContinuationPaths now follow the documented event-two
branches from the real root streams, preserving each table assumption.
The Start Game continuation stops at its first unread resident pair call;
the Save/Load action continuation stops at its first unread control call.
Other branch dependencies remain separate. No assumed continuation is
presented as ordinary reachability, complete effects or native input evidence.

Ordinary instruction paths and stop sites exactly match the undeclared-root
reports. Disabling continuation paths leaves them unchanged and removes all
conditional witnesses; a one-step cap removes the conditional witnesses too.
All controls pass without increased limits or state stitching. The toolkit
already supplies this conditional dispatch feature; no new issue was opened.
Issue 151 separately documents that entryFrame does not follow declared tables.

Next: actual pre-allocation resident/control callees, followed by frame and
state producers through callback continuation. Source-local reports:
GAME_DIR/analysis/reporter-audit/issue5-parent-prefixes100 and
issue5-parent-dispatch100. Ignored drivers/logs: artifacts/engine100/
parent-prefixes.*, parent-dispatch.* and parent-dispatch-controls.*.

## Connected pre-allocation callees

Actual parent relocation mappings and committed numeric inventory bounds now
carry both callback-root conditional streams into the pre-allocation resident
callees. The Save/Load action control helper returns into its actual caller;
both parents then reach the same resident helper and its further callees.
No returning-call model, selected input, supplied frame or memory was added.

The next stops include an undeclared direct near-call target, computed dispatch,
an unmodeled interrupt and unresolved loop/step/path bounds. These retain the
unknown native resource/control state. Neither full callback-root stream reaches
the allocation entry. Ordinary routes and stop sites exactly match the previous
reports; every added stream retains its declared dispatch assumptions.

Source-local reports: GAME_DIR/analysis/reporter-audit/issue5-parent-callees100,
including successive source-mapped callee stages. Ignored drivers and controls:
artifacts/engine100/parent-callees*. No query limit was increased. Next evidence
is the helper dispatch inputs, actual dependency bodies and required service
effects; repeating the same capped roots would not answer those questions.
Whole Gap 37 remains open.

## Static producer map and focused selector verification

Static source readings now separate the actual Start Game argument boundary
from the Exit branch reached first by the broad conditional trace budget.
Both Start Game and Save/Load supply selector four to the resident helper.
Focused traces retain its two-byte read and the actual push producer, without
register or memory seeds or returning-call models. Omitting the argument prefix
loses the concrete selector; a one-step cap loses the read altogether.

The helper compares the unsigned selector with five before indexing its six
source word targets. The source entry for index four selects the field-update
branch. This is a source-table reading, not proof that runtime code never
changes the table, that callback events occur, or that lookup succeeds.

The previously described unknown indirect target is corrected: the diagnostic
helper's stopping call is a relative near call with a source-derived target
outside the declared regions. Its inventory row is only the initial fragment;
a source jump immediately leaves that fragment. No unknown callback pointer
was demonstrated at that stop. Do not reuse the old classification.

The actual Start window opener supplies resource 19500 to the overlay window
acquisition entry documented by FND-CONFIG-018/030/034, then stores both words
of its returned far pointer in the storage the callback loads. Existing findings
identify acquisition, registration and activation failure gates. A focused
argument-boundary report resolves the acquisition call to that actual body;
it does not model success or connect separate runtime states. The resource
reader, registration writes and subsequent writers to that pointer/child graph
are the concrete next producers, rather than another broad root expansion.

Source-local reports: GAME_DIR/analysis/reporter-audit/issue5-parent-selector100.
Ignored drivers: artifacts/engine100/parent-selector.mjs and
window-acquisition-boundary.mjs. Original instruction context stays local; no
new spec status or gameplay behavior is asserted. Whole Gap 37 remains open.

## Window acquisition and registration dependency map

The actual opener's acquisition call resolves to the overlay helper already
recorded by FND-CONFIG-018/030/034. Its resource-request helper initializes a
local far result, passes the requested WIND number and its output address to
the resident reader wrapper, and returns the two result words. Acquisition
then tests that pointer before position registration, callback storage and
activation. Those recorded failure gates remain conditions, not modeled success.

The local position-registration caller's actual relocated far call is confirmed
by the published incoming reporter against the full resident registrar. This
search is deliberately partial and cannot prove no other callers. The numeric
inventory body-byte count does not supply an end address. The query uses the
full source span identified by FND-CONFIG-031, excluding its external dispatch
tables; the original analyzer count is preserved. No original code or table
data enters the inventory.

Static reading connects the registrar's child resolver output to its two
child-pointer stores, using the same child offset byte, word count and stride
that the later lookup consumes (FND-UI-008/011). Window list/position writes
precede completion of child registration. FND-CONFIG-030/031 retain failure
gates and later callback initialization, so the pointer store alone cannot
prove a complete graph or rollback. The resident resource reader's archive,
allocation, read and fallback producers remain the next specific dependencies.
No ordinary runtime state or unconditional acquisition success is asserted.

Source-local cross-reference report: GAME_DIR/analysis/reporter-audit/
issue5-window-registration100/incoming.report.json. Ignored driver:
artifacts/engine100/window-registration.mjs. The actual acquisition argument
boundary report remains under issue5-parent-selector100. Whole Gap 37 and
the other four connected-evidence gaps remain open.

## Archive-root census reconciled with existing findings

The fresh literal-field census initially retained four direct archive-root
stores. Reconciliation with FND-CONFIG-037/068 showed that both the archive
initializer and archive-open routine were absent from the committed inventory.
Their existing documented ranges and final source returns now supply two
numeric inventory rows. A fresh published operand-candidate census includes
all six literal stores listed by FND-CONFIG-068, with their four-byte widths
and DS accesses retained. The pre-repair report remains local for comparison.

This is corrected search coverage, not a no-other-writers claim. The known
address-taking record-growth call remains a separate producer route, and the
source-container exclusions and partial domains remain explicit. FND-CONFIG-039
connects startup archive open to the resource name selection and graphics
handoff; FND-CONFIG-068 records the initial option gate that excludes growth
on the newly opened resource record. Later selection, close, options, aliases
and externally supplied read/allocation outcomes still need their own bounds.

Source-local reports: GAME_DIR/analysis/reporter-audit/issue5-resource-root100.
Ignored driver: artifacts/engine100/resource-root-writers.mjs. This map connects
previously recorded producer findings to the acceptance search domain without
promoting a spec entry or supplying native state. Whole Gap 37 remains open.

## Reader publication and body-count controls

FND-CONFIG-151 records output-pointer publication before later archive/seek/read
gates. Fresh bounded source context agrees: the acquisition helper reads and
returns its local pointer after the reader call without branching on its status.
The older generic failure wording in FND-CONFIG-038 cannot support an assumption
that every reader error leaves a null output. This acceptance record uses the
more detailed recorded ordering and retains intervening callee, alias/storage
and native failure-admission questions; it does not establish an observed defect.
Spec reconciliation remains research work outside this tooling-only scope.

The published bounds report independently reproduces FND-CONFIG-151's reached
body bytes and full source span. It retains the far return beyond start plus
body bytes, the internal hole, and returning-callee assumptions. A query that
uses the count as its end loses the return and is incomplete; a one-instruction
control is also incomplete. Complete here describes only the local CFG under
those assumptions, not original resource delivery or whole Gap 37.

Earlier inventory expansions incorrectly substituted spans for measurements.
The registrar's original analyzer count is restored and independently matches
the source CFG with its unresolved dispatch. Save/Load retains its original
analyzer count rather than a span or a different decoder's partial count.
The newly added Start Game entry uses the published partial body-byte count;
its unresolved dispatch remains explicit. Both new archive rows independently
match complete local body-byte measurements. Source-span query regions remain
separate from these inventory metrics. Old size-as-end searches are partial
candidate searches and cannot support absence or full coverage.

Source-local reports: GAME_DIR/analysis/reporter-audit/issue5-resource-reader-
bounds100. Ignored drivers/controls: artifacts/engine100/resource-reader-bounds*,
archive-body-counts.mjs and callback-body-counts.mjs. No original content or
new gameplay/spec status is committed. All five whole exits remain open.

## Gap 37 actual transfer hardware placement, 2026-10-05

The published engine 10.0.0 `bounds` query now reads the complete resident
primitive documented by FND-CONFIG-192, with source identity guarded by the
published reader. Its local CFG covers the documented span without holes,
calls or unresolved branches and reaches the final far return. Every reported
port access has an explicit continuation assumption. This is local CFG
completion, not complete input evidence or a native hardware execution.

An independent bounded Capstone CFG census agrees with the published port
instruction sites and directions. Removing each site in turn and checking
whether the return remains reachable identifies two common port writes; the
other port sites have bypass routes. This checks syntactic return-path
placement only, not the feasibility of every branch, device success or native
pixels. It agrees with the existing finding's common plane setup and optional
scratch/plane-update paths. The report does not substitute RAM effects for VGA
output, claim accepted scratch/storage, or supply original memory values.

The published `callees` query connects the actual FND-CONFIG-191 transfer
wrapper to both validator calls, all four coordinate getters and the
FND-CONFIG-192 primitive. Each declared node has complete local CFG coverage;
the primitive is the hardware-bearing child. The wrapper's guards still decide
whether that child is called. Graph connectivity is not proof that an original
invocation passes those guards or that the original caller's slot/reference,
segment, mask and alias conditions are admitted.

Nonvacuous controls pass: a one-instruction bound and a region truncated before
the transfer expose no hardware sites and remain incomplete. Omitting the
primitive from the wrapper graph or limiting the graph to its root loses the
hardware-bearing child. None is accepted as evidence of hardware absence.
No old unresolved symbolic traversal bound was raised or repeated.

Configs, complete numeric reports and the placement summary are local under
GAME_DIR/analysis/reporter-audit/issue5-transfer-boundaries100. Drivers and
assertion logs are artifacts/engine100/transfer-boundaries.mjs,
transfer-boundary-placement.py, transfer-boundary-graph.mjs and
transfer-boundary-controls.mjs with their corresponding logs. The placement
classification derives from the independently checked CFG; it is not a new
field claimed to exist in the published bounds API.

This supplies actual source-local hardware placement and connected
wrapper/callee classification for Gap 37. The initializer remains a separate
control. Whole startup/caller/input and later-writer acceptance, finite valid
reference chains, storage/mask/alias admission and native hardware output remain
unverified. Existing game findings and parity are unchanged; all five full
connected exits remain open. Next identify which slot or mask producer is
required for a specific missing transfer control, rather than treating every
archive dependency as a placement prerequisite.

## Gap 37 mask-byte dependency and numeric-bound limitation, 2026-10-05

A focused source reading and published trace of FND-CONFIG-192's destination
mask prefix now separate its inputs: a current scratch word is masked to two
bits, a byte is loaded through explicit CS-relative indexed addressing, AL is
set to the indexed-port selector, and DX is set to the destination-mask port.
The byte therefore comes from the named four-byte mask table, not from an
ordinary DS-relative archive field. Its current contents and intervening writers
remain unknown. This does not establish accepted mask values or VGA output.

The narrowly entered trace supplies no original memory values, root-slot state,
register inputs or hardware model. Reached port-number, selector-byte and
mask-read-origin occurrences hold. Omitting the table load rejects the claimed
byte origin, and the one-step control loses every anchor. Whole controls remain
undecided: the query starts inside the primitive and its later repeated copy
has an unresolved count. Port output is recorded as leaving the instruction
model; conditional continuation does not execute or validate the device.
This source-local prefix does not establish the omitted frame, caller or slot
history and is not substituted for a whole connected transfer positive.

The masked-index unsigned upper-bound control remains undecided even locally.
A separate wholly synthetic complete returning query isolates this reporting
limitation: an unknown word ANDed with three returns with a bound-of-three
control undecided, while known-input and zero-mask controls hold. The unmasked
unknown negative stays undecided. No hardware, callee, original data or stopped
path explains the synthetic result. This is conservative expressiveness, not
an unsound accepted bound. Duplicate-checked toolkit [issue 290](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/290)
requests sound constant-mask unsigned bounds or a supported equivalent recipe,
with width/signedness, omitted-mask and incomplete-query controls. Delivery is
pending; do not claim the engine proves the index bound already.

Original-derived configs/reports stay under
GAME_DIR/analysis/reporter-audit/issue5-transfer-mask100. Source reading,
assertions and logs are artifacts/engine100/transfer-mask-reading.py,
transfer-mask-controls.mjs and transfer-mask-controls.log. The independent
synthetic reproducer is masked-index-controls.mjs, with fixtures/reports in
artifacts/engine100/masked-index-controls and masked-index-controls.log.

Next producer evidence is the current CS mask table and its writes, alongside
the particular slot/reference inputs needed by a missing whole transfer control.
Do not infer mask admission from a bounded index, hardware success from the
port event, or full startup coverage from a prefix. Game findings/parity remain
unchanged. All five full connected exits remain open; the new upstream request
addresses this specific control rather than replacing their other evidence.

## Gap 37 mask-index scratch producer placement, 2026-10-06

Toolkit issue 290 remains open with no response or delivery evidence at this
check. The unchanged numeric-bound query was not repeated. A fresh full-body
source reading instead follows the scratch word consumed by the mask prefix.

The primitive reads the second stacked handle, doubles its word slot index,
loads that slot's horizontal-coordinate word, copies it into intermediate
scratch and then into the mask-index scratch word. Its entry selects DS from
CS before these accesses. The later mask prefix explicitly loads the scratch
through CS and applies the two-bit mask before the indexed table read. This
follows the original field path already described by FND-CONFIG-192; it supplies
no native handle, field value or mask-table contents.

Independent full-entry CFG controls show that removing any of the named
handle-load, slot-read, scratch-copy, index-load/mask or DS-selection instructions
disconnects the actual table-read site. The second-handle argument reading also
rejects a first-handle alternative. All source-local DS assignments that can
precede the mask-scratch store reduce to the entry CS selection; later transfer
DS changes cannot reach that earlier store on this CFG. The source is call-free
and interrupt-free, with ports retained as explicit hardware boundaries.

These controls establish placement of the primitive's own explicit producers,
not complete runtime last-writer coverage. Dynamic slot addresses, stack aliases,
reference admission and actual transfer ranges remain unverified; copied ranges
may alias shared scratch or the mask table. Later rows reuse scratch after
transfers. CFG domination cannot prove native memory stability, mask admission,
finite malformed-input behavior or hardware output. The unresolved published
numeric-mask control remains separate from this producer reading.

Private reports/readings remain under issue5-transfer-mask100. Ignored drivers:
transfer-mask-scratch-reading.py, transfer-mask-scratch-segments.py and
transfer-mask-scratch-placement.py under artifacts/engine100. Next is the specific
second-handle slot producer/admission and the transfer-alias dependency of a
whole control, rather than unrelated archive history. No game specification or
parity status changes; all five full connected exits remain open.

## Gap 37 second-handle admission and field-writer candidates, 2026-10-06

Published caller-order controls retain the actual primitive call behind both
signed-handle gates and both coordinate-validator result gates. The capped
negative loses usable ordering. Independent source assertions follow the
wrapper's second word through DI into the primitive's second stacked argument,
and distinguish word-sized signed handle checks from byte-sized AL result
checks. A full-AX validator interpretation is rejected. Callee and saved-frame
preservation remain conditions on that argument handoff.

FND-CONFIG-191's coordinate getters and validator do not bound a nonnegative
handle index or validate its slot flags/storage. The specific second-handle
input in FND-CONFIG-186 comes from a current DS-relative destination field;
matching it to the primitive's admitted slot needs that field's producers and
preservation, not a broader claim that coordinate acceptance validates storage.

A function-boundary-independent literal census, followed by bounded linear
candidate decoding, identifies direct word-store candidates in overlay 182.
Published operand reports now verify their entry-path instruction ownership and
word width; instruction-limit controls lose ownership. Raw literal hits and the
separate indexed byte candidate do not establish uses of the same field. This
is not an exhaustive writer search: computed, aliased and cross-segment writes
remain outside the literal census, and current DS identity is unverified.

The first candidate saves an allocator return before testing the word against
the all-ones failure value. Its child mapping resolves to FND-CONFIG-183's slot
allocator. The second passes the current field to FND-CONFIG-184's cleanup
wrapper and replaces it with that service's returned word. Their bounded source
readings establish these local producer roles, not a successful allocation,
preserved field, valid later handle or primitive invocation. In particular,
store-before-failure-test is not an admitted-input positive. FND-CONFIG-028
records a caller of the first candidate; its native route and intervening effects
remain unverified.

Private reports/readings remain under issue5-transfer-mask100. Ignored drivers:
transfer-mask-handle-guards.mjs, transfer-mask-handle-widths.py,
transfer-destination-handle-candidates.py, transfer-destination-handle-reading.py,
transfer-destination-handle-filter.py and transfer-destination-writer-controls.mjs
under artifacts/engine100. Next is the specific allocation-return/field handoff
and later cleanup-write dependency, retaining current DS and slot/alias limits.
No game specification or parity status changes. All five full connected exits
remain open.

## Gap 37 destination allocation and later replacement routes, 2026-10-06

Published caller-order analysis now confirms the allocation call feeding the
first destination-field store within the candidate's entry CFG. The capped
negative loses usable ordering. The documented allocator and free-slot scan
also resolve as a separate connected call graph with usable complete local
bodies. This does not complete the parent, whose earlier callees and current
state remain unverified.

FND-CONFIG-183 bounds the successful allocator's returned slot-derived handle;
its all-ones failures remain separate. The candidate copies that word into its
current DS-relative field before testing failure. Neither a resolved call edge
nor a bounded successful return proves that this invocation succeeds, that the
field preserves it, or that the later transfer sees the same physical storage.
The scanner does not reserve the selected slot on its own.

The later writer's child is the cleanup wrapper documented by FND-CONFIG-184.
Fresh full-wrapper decoder controls, starting after its parameter load with
explicit register hypotheses, cover the signed-at-most-one bypass for zero,
one, the failure sentinel and the most negative word. Each returns the supplied
word with whole conditional controls held. A wrong unconditional-sentinel
control is rejected, and the one-step cap loses all value anchors. These are
conditional decoder cases, not native field inputs or a joined producer history.

The positive service hypothesis reaches the actual release call and stops there
unmodeled. The wrapper's source assigns the failure sentinel after that call
returns, discarding its returned AX; service success and effects are not tested.
Thus replacement must distinguish unchanged bypass words from the wrapper's
own sentinel assignment after a returning hardware-bearing service. The earlier
field's allocator origin cannot be assumed to survive this replacement.

Private reports/readings remain under issue5-transfer-mask100. Ignored drivers:
transfer-destination-allocator-connection.mjs, transfer-cleanup-word-controls.mjs
and transfer-cleanup-word-negative.mjs under artifacts/engine100. No native field
memory was supplied, no old capped traversal bound was increased, and no stopped
positive-service path is accepted as completion. Next is current DS and the
specific field-to-consumer preservation/order, alongside slot/transfer aliases.
No game specification or parity status changes. All five full exits remain open.

## Callback-root dispatch to the destination producer, 2026-10-06

The previous branch-local positive now has a published callback-root ordering
control. Private source checks corroborate FND-UI-035/FND-CONFIG-028's button
consumer, subtraction, unsigned bound, word-index shift and documented targets.
They also bound the preceding selector scan and its parallel target-word array.
The actual FBOV code start supplies the source-table offset base; the historical
location label is not used as that base. All table targets remain within the
declared callback code. Original table words and instruction contexts stay local.

With both exhaustive source-derived declarations, the published call-order query
confirms the destination-producer call from the callback root and retains usable
local ordering without a root dispatch gap. Exhaustiveness here covers the read
source tables and their bounded consumers. It does not establish native selector
inputs, unchanged table memory, overlay load address or effects of the returning
callees. The declarations and continuation assumptions remain in the reports.

Omitting either declaration loses usable root ordering and leaves its computed
transfer unresolved. The producer call remains owned through the other declared
route: in particular, the preceding selector table contains a target to the same
branch. Thus the omission negative is incomplete ordering, not call absence.
The one-instruction control loses confirmed ownership. Do not claim that the
button/event gate is necessary for every source route to this branch, or that a
table-declared path proves an input gesture or native execution.

This removes the specific callback-root dispatch gap for source-local ordering.
It does not join the allocator result/DS field with the later transfer consumer,
resolve intervening callee effects or satisfy a full dynamic connected control.
Other callback-root candidates retain their prior gaps. Private reading and
reports are under issue5-transfer-field-incoming101; ignored drivers/assertions
are artifacts/engine101/start-dispatch-reading.py and start-dispatch-root-order.mjs,
with corresponding logs. No memory seeding, window stitching or increased old
traversal bounds. All five full exits remain open; no game spec or parity change.

## Destination-field producer incoming references, 2026-10-06

A new published-reader census follows the verified destination-field writer
candidates and the transfer-field consumer through all declared MZ relocations
and FBOV fixups. It retains allocator-writer references from overlay callers,
a separate cleanup-writer reference from resident code, and resident consumer
references. The known primitive call remains a positive mapping control. The
source-derived reports retain near, computed, unrelocated and instruction-path
exclusions; no result is an exhaustive absence claim or shared DS identity.

Inventory-adjacent bounded linear contexts align the retained candidates with
far-call instructions. Published entry-path checks retain dispatch/undecoded
gaps in the allocator's full callback roots. The documented Start Game branch
in FND-CONFIG-028 supplies a separately justified branch start: its published
call-order query confirms the call to the allocator-field writer with usable
local ordering. A one-instruction negative loses that ownership and leaves a
raw candidate. This is branch-local source acceptance, not proof that native
callback dispatch reaches that branch. The query still lists undeclared child
and target-body edges; local ordering does not establish their effects.

The resident cleanup-writer and consumer calls also have published entry-path
instruction ownership. The consumer callers retain usable local ordering;
the cleanup caller's unresolved jump prevents full usable ordering. Their
capped controls lose the confirmed calls. This
distinguishes those references from the allocator roots' dispatch-limited
candidates, but still supplies no joined producer-to-consumer execution history.

The other inventory-window reports retain their own candidate, undecoded and
unmapped limitations. Do not use linear alignment, inventory membership or an
empty confirmed-call list to infer native callers or absence. All resident
views were checked against the MZ header; overlay analysis segments are explicit
coordinates, not load addresses. The separate documented-branch entry is not a
stitched trace or a supplied native state.

Private census, contexts and published reports are under
GAME_DIR/analysis/reporter-audit/issue5-transfer-field-incoming101. Ignored
drivers/logs are transfer-field-incoming.mjs, transfer-field-caller-windows.py,
transfer-field-caller-order.mjs and transfer-field-start-branch.mjs under
artifacts/engine101. The next dependency is actual dispatch/segment producers
and effects between these producer candidates and the transfer consumer;
the census does not join those histories. No original-memory seed or higher
unresolved traversal bound was used. All five full exits remain open; no game
specification, parity status or other research goal changes.

## Connected caller, request, transfer and cleanup graph, 2026-10-06

The published engine 10.1.0 callee query now starts at the actual caller in
FND-CONFIG-186 rather than the transfer wrapper alone. It joins that caller
to FND-CONFIG-191's request and validator, FND-CONFIG-183's free-slot scan,
the coordinate getters, FND-CONFIG-192's primitive, FND-CONFIG-184's cleanup
wrapper and FND-CONFIG-194's release service. Complete local bodies and all
encoded edges resolve inside this declared graph. The graph's explicit
completeWithinDeclaredGraph result is true; there are no unresolved edges or
unchecked entries. The generated report holds the node/edge inventory and
both primitive and release hardware boundaries.

Removing the request leaves its caller edges unresolved and the disconnected
scanner unchecked. Removing the release leaves the cleanup service edge
unresolved even though remaining nodes retain usable local bodies. The latter
control demonstrates why node-boundary usability alone cannot prove complete
callee coverage. The one-instruction control retains only an incomplete caller
and unchecked children. Each negative loses completeWithinDeclaredGraph.

This completes source-local direct-callee coverage for this caller scope, not
its dynamic paths or native input admission. The read-only findings describe
normal request/primitive/release restoration of saved DS/SI/DI, but also retain
unverified reference chains and dynamic writes/copies that may alias metadata
or saved stack storage. The release can move buffers and change unsaved ES/DF
and graphics state. Therefore missing callees are no longer the blocker in
this declared caller graph; native segment/field identity, preservation against
aliases and admitted finite transfer/count producers remain unproved. Do not
replace these dependencies with scoped preservation models or infer a native
successful transfer from graph completion.

No native memory was seeded or trace windows joined, and no old traversal
bound was raised. Private reports are under
GAME_DIR/analysis/reporter-audit/issue5-transfer-caller-graph101; the ignored
driver/assertions and log are artifacts/engine101/transfer-caller-connected-graph.mjs
and transfer-caller-connected-graph.log. All five full exits remain open.
No game specification, parity status or other research goal changes.

## PR 293 released mask bounds, 2026-10-06

Engine 10.1.0 delivers toolkit issue 290 through PR 293. The exact released
wheel is now adopted; tag and artifact integrity are recorded in
LATEST-RELEASE-GAP-AUDIT.md. The published regression suite passes.

The same complete returning synthetic unknown-word AND-three query changes
from undecided to held. Known-word and zero-mask controls still hold; the
unmasked unknown remains undecided. The actual documented mask-prefix query,
with unchanged bounds and no invented memory, now retains a held unsigned
index-bound occurrence. Port number, selector and table-read origin occurrences
also hold. Removing the index mask leaves the numeric occurrence undecided;
removing the table load rejects the producer assertion; one step loses every
anchor. The actual whole controls remain undecided because the later string
copy count is unresolved. A local numeric hold does not complete the transfer.

Private actual configs/reports are under
GAME_DIR/analysis/reporter-audit/issue5-transfer-mask101. Ignored drivers and
logs are artifacts/engine101/masked-index-controls.mjs and
transfer-mask-controls.mjs, alongside summarize.mjs. Previous engine 10.0.0
reports are retained. No native table values, caller admission, storage aliases
or rendered output are established by this fix. All five full exits remain open;
the next Gap 37 dependency is the field-to-consumer segment/preservation chain
and admitted transfer/count producers, rather than the repaired mask arithmetic.
