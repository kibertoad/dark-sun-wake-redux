---
id: FND-PARTY-314
title: Getter stack guards have no finite ordinary return through the forced termination chain when their zero argument and frames survive
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2E48..1000:2E5A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:03EE..1000:0400
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388..1000:03DF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:019E..1000:01B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:02A5..1000:02AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:02B3..1000:02C4
tool: resident_listing.py at a4305661; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with ReportDecompileMatches.java; FND-CONFIG-163, FND-EXE-551 and FND-PARTY-289/290/291
environment: null
---

## Observation

Q-PARTY-136 retains guard continuation while applying the getter-miss
register contracts. This reuses the already read termination chain
and checks its ordinary return edges, rather than asking every cleanup
callee to preserve the getter's live DI. Installed DSUN.EXE is size
634416, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Offsets are hex.

### Forced wrapper and its only local returning bypass

FND-CONFIG-163 establishes the stack guard 1000:2E48: it requests
diagnostic output, restores DS from BX if that interrupt returns,
then jumps to 03EE. There is no local guard return before that jump.
The diagnostic result is not tested. This does not establish BX/DS
preservation or an actual diagnostic outcome.

03EE pushes three words into near helper 0388: the incoming code,
literal zero and literal one. In 0388's frame those are BP+4, BP+6
and BP+8. It initially loads the last word into SI; one bypasses
the initial exit-callback loop and its following stored callback.
This argument binding is recorded in FND-CONFIG-163/FND-EXE-551
and directly checked in the retained source instructions.

After cleanup calls 01F3 and 0176, 03BF tests the second word at
SS:BP+6. Nonzero branches to 03DA's returning epilogue without
the termination call. Zero continues to 03D6's call of 019E.
The SI test can skip or enter two more stored callbacks, but both
ordinary returning paths then reach that same 019E call. Therefore
the proof below needs the forced zero word and normal frame/control
preservation; it does not need to assume that cleanup preserves SI.
Changed callback or interrupt effects on the zero word, frame or
control transfer remain explicit conditions, not harmless callees.

### A returning termination request does not return from 019E

019E loads AH=4C and the low byte of its supplied code, then issues
interrupt 21 at 01A5. Its body has no return instruction. If that
request returns to its next instruction, 01A7 sets two diagnostic
arguments and jumps to 02B3. This is not the caller continuation
at 03D9 after the call of 019E.

02B3 loads DS from its saved CS-relative word, calls diagnostic
writer 02A5, then pushes code three and calls 03EE at 02C1.
The writer sets AH=40 and handle two and near-returns if its
interrupt returns normally. Its result does not gate the nested
03EE call. That nested wrapper again supplies zero as 0388's
second argument and one as its initial mode. Bytes at 02C4 and
following are not an ordinary return sequence for this prefix;
reaching them by normal fallthrough would first require the nested
03EE to return.

### Finite ordinary-return argument

Take the observed entry/branch bindings, unchanged code, valid
frames, the forced zero argument remaining zero through each
cleanup, and child/native calls either not returning or returning
to their encoded successors with balanced frames. A taken guard
cannot have a finite ordinary return to its getter caller:

- A nonreturning diagnostic, cleanup or termination request already
  prevents that return.
- If they return normally, the forced wrapper reaches 019E.
- If 019E's request returns, its fallback calls another instance
  of the same forced wrapper before any possible return.
- A finite return would therefore require a deeper wrapper to
  return first, with no base case in this chain under the zero-word
  condition. The caller epilogues do not create that missing base case.

This is a conditional nonreturn result, not a claim of successful
DOS termination or termination of the fallback recursion. It excludes
the observed guard path from a finite ordinary returning getter
continuation under those conditions. It does not apply if the zero
argument becomes nonzero, an active handler changes SS/SP or a
return frame, code is replaced, or another nonlocal transfer rejoins
the caller outside the encoded call/return model.

## Interpretation

Q-PARTY-136 no longer needs a live-DI preservation contract for
this forced guard path as though it were an ordinary returning
helper. Under the stated conditions a getter that returns normally
has bypassed that guard route. Actual guard state, stack/frame
backing and interrupt/alias/nonlocal effects remain relevant where
they defeat those conditions; specifically, 0388's SS:BP+6 zero
word controls its returning bypass after cleanup.

The ordinary-file source/release/vector/handle conditions in
FND-PARTY-292 and the saved-DI/frame conditions in 290/291 remain.
No native termination, whole getter/client exclusion, queue closure
or complete reading is established. FMT-PARTY-007 remains supported.

## Alternatives

- A returning AH=4C request returns from 019E: its actual successor
  tail-jumps into the diagnostic/forced-wrapper fallback.
- A visible epilogue after the call proves an ordinary guard return:
  reaching it requires the unproved callee return; the forced chain
  has no finite base return under its zero-word condition.
- All guard rejoining is impossible: changed argument/frame/control
  conditions are outside this conditional exclusion and remain open.

## How to reproduce

Reuse FND-CONFIG-163 and FND-EXE-551 before further cleanup reading.
Use resident_listing.py for the locations above, keeping the guard's
tail jump, 0388's post-cleanup zero-word test, 019E's absent return,
01A7's jump and 02C1's nested forced call separate. The first guard
code argument is not a fixed result code; its value is immaterial
to this local control-flow argument. Do not decode 02C4's data as
the fallback's returning continuation.

Bounded Ghidra matches recover the 0388-to-019E and 03EE-to-0388
call controls. Its printed caller return statements do not prove
that the invoked termination function returns. Instruction control
flow and the explicit argument binding decide the result.
Retain getter-guard-continuation-contract and getter-guard-ghidra
reports under GAME_DIR/analysis/handler-census-20261106/. The
exploratory termination-fallback report extends past 02C4; that
suffix is not evidence for the claimed instruction path. No original
game, emulator session or emulated call supplied evidence here.
