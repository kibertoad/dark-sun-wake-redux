---
id: FND-PARTY-321
title: Sound teardown has a nonzero-ready wrapper and a conditional nonreturning error route
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:010F..4734:0128
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49DE:000F..49DE:00A7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388..1000:0400
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067902..0x00067916
tool: direct_callers.py, resident_listing.py and overlay_listing.py at 8697c8df; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with ReportDecompileMatches.java; FND-SOUND-016, FND-EXE-551 and FND-PARTY-314/320
environment: null
---

## Observation

Q-PARTY-136 retains teardown admission because FND-PARTY-320's
ready byte can be cleared there. Installed DSUN.EXE is 634416 bytes,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Offsets are hex.

### The two encoded teardown callers have different gates

The direct-caller report for 4734:0591 recovers a near call at
4734:0123 after PUSH CS and a far call at file 3F062,
canonically 49DE:0082. Its printed paragraph alias 49E6:0002
is not another function entry. These are the wrapper and error
routine already distinguished by FND-PARTY-320/FND-SOUND-016.
Other transfer forms remain outside this caller search.

The complete shutdown wrapper 4734:010F saves BP and reads
byte 4E71:0B53. Zero skips the teardown; every nonzero value
calls 0591, then restores BP and returns far if that call returns.
It does not locally normalize AX. Its gate is nonzero, unlike the
initializer's exact equality with one. A ready value two therefore
passes this teardown gate but does not bypass initialization through
4734:00B0. This distinguishes two predicates, not an observed value.

The encoded caller search for 010F finds overlay 180 +0731,
inside the established root +0722. That root tests DS:13F7:
zero jumps past the sound calls; nonzero first calls 4734:0087,
then 010F. Actual mode, frame and segment retention through that
first call remain prerequisites. The root is not proven reachable
during a particular getter interval by this encoded caller alone.

### Error-route teardown is selected by the later DI test

49DE:000F saves SI and DI, loads the first argument into SI and
the second into DI, and initially tests DI against 0069. Equality
can invoke DS:3475 and print diagnostics. It then tests the current
DI against 0069 again at 0059. Only equality at this later test
reaches the route containing teardown at 0082. The initial test
alone does not prove the later selection across reached children.

On the selected route, tests at 4E71:0C22 and 0B67 can invoke
4734:0087 and 4AAB:0084 before teardown. After teardown it
may call the pointer DS:3479, then calls 1000:03DF with current
SI. The error body's epilogue after that call is not proof the
callee returns. The unequal later-DI branch reaches the epilogue
without this body's own teardown or exit call.

### The zero-mode exit wrapper has the same forced-zero return barrier

03DF passes zero, zero and its incoming code word to near 0388;
the latter receives code at BP+4, zero at BP+6 and zero mode
at BP+8. Unlike FND-PARTY-314's 03EE mode-one wrapper, this
mode-zero route runs the initial exit-callback loop, 0163 and the
stored callback before the common cleanup. That callback behavior
and any reentrant reads are not excluded here.

Every ordinary local continuation still reaches 0388's BP+6 test
at 03BF. Keeping that argument zero prevents its direct epilogue
bypass. Depending on current SI, it invokes two further stored
callbacks or skips them, then calls 019E. FND-EXE-551 and
FND-PARTY-314 establish that 019E has no return instruction;
returning from its native termination request follows the diagnostic
fallback into another forced-zero 03EE invocation. That fallback
has no finite ordinary base return under the recorded conditions.

Consequently 03DF likewise has no finite ordinary return when
its zero argument, code and frames survive, children return with
balanced frames, and no nonlocal control transfer changes the route.
Additional mode-zero callbacks may not return; their ordinary return
does not supply a base return from 019E. This proves no native
termination or loop termination. Ghidra recovers the error-to-03DF
edge and prints a following return; the instruction continuation and
argument binding decide the conditional nonreturn result.

## Interpretation

A getter that returns through its ordinary continuation cannot have
completed this selected error teardown and 03DF route under the
intact-zero/frame model. It therefore needs no blanket preservation
contract for this route as an ordinarily returning child. Changed
zero arguments, code/frames, SS/SP and nonlocal rejoining remain.
Reentrant reads during exit callbacks remain access-census obligations;
they are not excluded by absence of an outer ordinary return.

The ordinary 010F wrapper remains independently relevant: its
mode/ready/root admission and backing effects are not dismissed by
the error-route argument. Q-PARTY-136 retains those concrete
conditions, excluded transfers and FND-PARTY-320's state writers.
No queue closure or complete reading follows; FMT-PARTY-007
remains supported.

## Alternatives

- Both wrappers test ready equal to one: teardown tests nonzero.
- Every call of the error routine performs teardown: the later DI test
  selects that route, after any earlier diagnostic children.
- A visible epilogue after 03DF proves normal error-route rejoining:
  its zero-argument path has no finite ordinary base return.
- No ordinary outer return excludes nested exit-callback readers:
  those callbacks can still run before termination.

## How to reproduce

Reuse FND-SOUND-016, FND-EXE-551 and FND-PARTY-314/320.
Run direct_callers.py for 4734:0591 and 4734:010F; decode
the resident locations above and overlay 180 67902..67916.
Track both DI tests, the cleanup call, the general exit wrapper's
two zeros and 0388's post-cleanup zero-word test separately.

Reports under GAME_DIR/analysis/handler-census-20261106/ are
getter-sound-teardown-callers.txt, getter-sound-teardown-gates.txt,
getter-sound-shutdown-wrapper-callers.txt, getter-sound-shutdown-root.txt,
getter-sound-exit-arguments.txt, getter-sound-exit-worker.txt and
getter-sound-teardown-ghidra reports. The shutdown-root report's
exploratory prefix before 67902 is not entry evidence. No original
game, emulator session or emulated call supplied evidence here.
