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
