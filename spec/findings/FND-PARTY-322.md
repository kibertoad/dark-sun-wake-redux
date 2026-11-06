---
id: FND-PARTY-322
title: Known ordinary sound shutdown is selected by an exact-one mode test in the registered exit callback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067902..0x00067942
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000679EE..0x00067A06
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067321..0x00067338
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047BE2..0x00047BEC
tool: direct_callers.py, alias_far_words.py and overlay_listing.py at c3414212; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; existing FND-CONFIG-059/060/061, FND-SAVE-012/019 and FND-PARTY-321
environment: null
---

## Observation

This narrows Q-PARTY-136's ordinary shutdown-root admission.
Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets are hexadecimal.

### Exact mode test in the known caller

The encoded caller census for overlay 180 +0722 recovers one near
call, +0816 after PUSH CS, and identifies its trampoline 56B2:005C.
It finds no encoded far caller through that trampoline. The same run
recovers the independently known resident far call at file 1CC2F
through 56B2:0020 to setup root +0141, providing the overlay-far
control. FND-SAVE-019's independently recorded +0722 call provides the near control.
Register/memory calls, jumps, computed segment forms and alternate streams
remain excluded; raw patterns alone do not prove caller admission.

The +0816 call is inside the separate exit-cleanup root +0762,
trampoline 56B2:0025. At +080E it compares current byte DS:13F7
with one. Equality invokes +0722 and then joins at +0826.
Every unequal byte instead calls 4842:071A with double word zero,
then joins at +0826. FND-SAVE-019 already identifies these two
callees; this reading retains the exact selector rather than interpreting
it as a generic zero/nonzero mode branch.

The complete +0722 body independently tests DS:13F7 against zero.
Zero skips its calls; nonzero invokes 4734:0087, then the shutdown
wrapper 4734:010F. Its continuing local path separately tests
DS:3458 and DS:3420 and releases each nonnull pointer through
444C:0092 before returning far. The second pointer is reloaded
after the first release. Neither pointer is cleared locally. This is
an effect classification, not a successful-release or disjointness claim.

Under unchanged mode one from the caller's test to the root's read,
the known call takes its nonzero sound path. Mode two is a useful
predicate distinction: it would pass +0722's own gate, but does not
select the known caller's +0816 edge. It is not an observed native
mode value. Changed mode between reads and other incoming routes remain.

### Reuse the exit registration, not an assumed gameplay caller

FND-CONFIG-061 establishes that setup root +0141 passes the
56B2:0025 pointer as separately pushed words to 1000:030B.
Registration succeeds only with room in its callback table and the
caller does not test its result. The runtime exit loop in 0388
invokes the registered pointers. FND-SAVE-012 supplies the known
main-loop return into 03DF; FND-SAVE-019 supplies the exit sequence.
These are existing connected evidence, not new proof of every callback
or table history.

On that known intact registration and exit route, ordinary sound
shutdown is an exit callback, before the native termination request.
FND-PARTY-321's conditional absence of a finite ordinary return from
03DF therefore applies to its outer caller. A returned getter does
not need to preserve its number across this whole route as an ordinary
returning child under the same intact-zero/frame model. Reentrant
getter/client reads during cleanup and changed-frame/nonlocal rejoining
remain; no blanket exclusion of exit-time accesses follows.

### Supported pointer forms leave one unclassified candidate

alias_far_words.py inspects 12965 declared relocation/fixup segment
words with zero unreadable segment mappings. For 56B2:005C it
finds one nearby offset-word candidate: relocated segment word at
file 47BEA can form alias 528F:428C using the word eight bytes
earlier at 47BE2. The separated words are not an established far
pointer or transfer. Their common linear target does not prove use
as a pair; their actual consumption remains unclassified here.

The control 56B2:0025 recovers the known split registration at
6732A with the offset three bytes later, plus nearby candidates at
67338 and overlay 175 +00E0. This supports the mechanism search;
it does not promote every nearby word pair to an incoming reference.
Segments constructed dynamically or words farther apart than the
reporter's twelve-byte neighborhood remain outside that search.

## Interpretation

Q-PARTY-136 can remove an unspecified meaning and caller identity
for the known +0816 shutdown route, applying its exact-one gate and
conditional outer nonreturn. It retains actual mode and callback-table
histories, the one unclassified alias-pattern candidate, excluded incoming
mechanisms, reentrant exit readers and changed-frame/nonlocal rejoining.
The root's two releases retain their actual backing/lifetime conditions.
No queue closure or complete reading follows; FMT-PARTY-007 stays
supported. FND-SAVE-019's broader subsystem summary is reused only
with this exact mode predicate and its stated partial-callee limits.

## Alternatives

- Every nonzero caller mode invokes sound shutdown: +080E requires one.
- The root's own nonzero test proves the caller admits every nonzero mode:
  the two tests occur at distinct locations with different predicates.
- No encoded far call makes the root unused: the near call and registered
  exit route establish a concrete use.
- The nearby alias words prove another caller: their pair consumption has
  not been established.

## How to reproduce

Reuse FND-CONFIG-059/060/061 and FND-SAVE-012/019 first. Run
direct_callers.py for 180+0722 and control 180+0141; decode
overlay 180 67902..67942 and the established exit-root path at
67942..67A27. That latter report ends before the final far return;
it is evidence for the selected branch, not a new complete root reading.
Run alias_far_words.py for 56B2:005C and control 56B2:0025.

Retained reports under GAME_DIR/analysis/handler-census-20261106/
are getter-shutdown-root-callers.txt, getter-shutdown-root-complete.txt,
getter-shutdown-exit-root.txt and getter-shutdown-root-pointer-controls.txt.
No original game, emulator session or emulated call supplied evidence.
