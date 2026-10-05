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
issue5-poll-callers100. Ignored drivers and logs: artifacts/engine100/
poll-callers.*, poll-caller-roots.* and poll-root-controls.*.

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
Ignored driver/log: artifacts/engine100/poll-progress.mjs and poll-progress.log.
