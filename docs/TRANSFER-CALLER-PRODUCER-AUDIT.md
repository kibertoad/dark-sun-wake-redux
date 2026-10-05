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

The committed inventory represented the Save/Load callback by its initial
fragment and separate interior fragments, leaving the confirmed call outside
its declared search ranges. The Start Game callback was missing altogether.
Verified callback endpoints precede the documented external dispatch tables.
The numeric inventory now expands the first callback's range and adds the
second; existing interior entries remain. These rows contain no original code,
bytes, strings or names. They describe known source ranges, not complete
execution or established analysis coverage.

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

The next stops are actual unknown indirect targets, a further computed dispatch,
an unmodeled interrupt and unresolved loop/step/path bounds. These retain the
unknown native resource/control state. Neither full callback-root stream reaches
the allocation entry. Ordinary routes and stop sites exactly match the previous
reports; every added stream retains its declared dispatch assumptions.

Source-local reports: GAME_DIR/analysis/reporter-audit/issue5-parent-callees100,
including successive source-mapped callee stages. Ignored drivers and controls:
artifacts/engine100/parent-callees*. No query limit was increased. Next evidence
is the helper dispatch inputs, indirect target producers and required service
effects; repeating the same capped roots would not answer those questions.
Whole Gap 37 remains open.
