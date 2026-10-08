---
id: FND-EXE-087
title: Shared-head selection negates before dispatch and freshly chooses saved-state publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FAF40..0x005FAF90
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600FF0..0x00601101
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading
environment: null
---

## Observation

FND-EXE-086's terminal arm calls `0x005FAF40`. This helper obtains context
through `0x005FD2F0` (FND-EXE-067), saves it and reads its first full word
as the head without a prior context-null guard. A zero head goes directly
to `0x005FD120`, whose mutable target and final abort boundary are recorded
in FND-EXE-041. A nonzero head is checked against full signature words
`0x432B2B00` at offset 48 and `0x474E5543` at offset 52, also recorded in
FND-EXE-025. A match negates the full word at head offset 20 before the
selected-state call. This is modulo-32-bit negation, not a Boolean toggle.
A mismatch clears the saved context's first word, but retains the old head
pointer for the same selected-state call. Both nonzero routes supply old
head plus 48 to `0x00600FF0`.

If that call returns normally, the preserved pointer is supplied next to
`0x005FABA0` and its result is not admitted or combined before the call to
`0x005FD120`. The finalizer's unexpected normal return would physically
fall through into the head-clearing route. There is no local ordinary
return in the cited helper body. Normal callee return and preserved
register assumptions must remain separate from the observed branch order.

The selected-state helper `0x00600FF0` first reads full word 12 through its
incoming pointer, with no initial pointer-null check. A zero field calls
`0x00600B50` with that pointer and restores its own frame, passing through
the callee's raw EAX on return. A nonzero field reads shared pointer
`0x0242F640`. A zero shared pointer calls `0x006005B0` and reloads it; a
negative signed mode at shared offset 48 calls `0x00600860`, reloads the
shared pointer and rereads the mode. FND-EXE-043 and FND-EXE-200 record
those helpers' separate boundaries.

Mode zero obtains its initial candidate from shared offset 40. Nonzero
mode saves shared offset 44, calls `0x00602490`, then calls `0x00602580`
with the saved field, retains its result, and calls `0x00602440` with the
first call's result. The retained second result becomes the candidate.
External/helper outcomes and stack-cleanup contracts are not newly proved
by this caller reading. The candidate is copied to locals -16 and -20;
the incoming pointer and address of local -20 are supplied in EAX and EDX
to `0x00600CC0` (FND-EXE-054). Only a full return of seven is admitted.
Any other return calls `0x006020C0`, the abort boundary in FND-EXE-041.

After seven, it freshly reads shared pointer `0x0242F640` and selected
local -20. It again handles a zero shared pointer by initialization and
a negative mode by the mode helper, with fresh pointer and mode reads.
It does not reuse the first mode decision. Fresh mode zero stores the
selected pointer to shared offset 40 before loading its saved-state words.
Fresh nonzero mode calls `0x00602590` with shared offset 44 first and the
selected pointer second; two further slots receive EDX without established
semantic input meaning. A nonzero full return proceeds to transfer; zero
calls `0x00602490` and also proceeds. Neither return gate rolls back the
selection or requires a newly successful publication result.

The transfer freshly loads local -20, takes target from selected offset
36, restores EBP from selected offset 32 and ESP from selected offset 40,
then jumps to the target without adding a return address. No local target
or selected-pointer null guard precedes those accesses. The saved-state
layout matches the separately recorded forwarding route in FND-EXE-052;
native target/frame validity is not established by this static transfer.

## Interpretation

Signature match and mismatch retain distinct state effects before a common
selection call. The exact-seven admission and second mode decision govern
the observed saved-state transfer. Q-EXE-009 still needs callers, shared
writers, aliases, valid selected records, external effects and admission of
the native saved frames; this finding does not claim a complete reading or
successful unwinding. A callee that jumps away prevents later normal calls.

## Alternatives

Clearing the head on a signature match, negating after selection, discarding
the old head on mismatch, treating any nonzero selector result as seven,
reusing the original mode at publication, or adding a fresh return address
at the saved-state transfer is ruled out locally. Negating zero yields
zero and negating the minimum signed word wraps to itself; neither proves
the field's intended state meaning. Publication-helper zero does not locally
cancel the transfer.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-086's direct call.
Read thirty-five instructions at `005FAF40`, retaining only its cited
region. Read seventy-eight at `00600FF0` and twenty-two at `006010C8`,
restricting claims before `00601110`; the earlier forty-five-instruction
prefix alone does not cover every branch. Follow every direct branch,
normal-return assumption, saved pointer, full comparison and fresh shared
read. Use FND-EXE-025/041/043/046/052/054/067 for the specifically cited
callee and signature limits. Keep reports local and do not execute the
original or infer externally provided stack/frame state.
