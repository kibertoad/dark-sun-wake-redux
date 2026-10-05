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
The second request retains return-frame/stack and scanner-repeat stops; no
second-field completion, joined transfer state or valid native slot is claimed.
These are actual producer instructions and outputs, not substituted handle
fixtures. Complete prefix, initialization, intervening writers and transfer
continuation remain required before Gap 37 can close.

Source-local reports: GAME_DIR/analysis/reporter-audit/issue5-second-handles100
and issue5-second-handle-producers100. Ignored drivers/logs:
artifacts/engine100/second-handle-writers.* and second-handle-producers.*.
