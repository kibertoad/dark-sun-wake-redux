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
artifacts/engine100/poll-cfg-bounds.mjs, poll-cfg-reading.py,
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
