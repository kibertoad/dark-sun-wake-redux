---
id: FND-EXE-267
title: Resident header reader gates stack-buffer consumption and preserves a distinct wrapped offset calculation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0050..4AE5:00E7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0135..4AE5:0140
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:028B..4AE5:029B
tool: scientific-method-engine 13.6.0, executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

The ten-instruction, sixteen-byte helper at `4AE5:028B` saves DS, forms
DX from BP minus twenty, and explicitly loads DS from SS before selecting
AH `0x3F` and invoking interrupt `0x21`. Thus the outgoing buffer pointer
is SS:BP-minus-twenty under the admitted frame. It restores DS before
testing carry. Carry set returns directly; carry clear compares returned
AX with current CX and returns the comparison flags. An unsigned smaller
count sets carry; equality or a larger count clears it. There is no equality
requirement and no restoration of CX from a saved request value.

FND-EXE-175's root retains the file result in BX and state word `0x0128`.
It requests twenty bytes through the helper, loads CX `0xFFFD` without
altering flags and branches on carry before consuming the local buffer.
Its first buffer comparison tests word BP-minus-twenty against `0x5A4D`.
A mismatch selects CX `0xFFFC` and the common failure path. The request
fits the root's twenty allocated local bytes; initialized output still
depends on the external read contract and preserved frame/register state.

On a matching header it loads AX from BP-minus-sixteen and CX from
BP-minus-eighteen. If CX is nonzero, AX is decremented at word width.
Unsigned MUL by `0x0200` produces DX:AX. ADD AX,CX follows, then ADD AX,15,
then ADC DX,0 and masking AX with `0xFFF0`. The first ADD's carry is
overwritten by the second ADD: only the latter carry reaches DX. This is
not an unrestricted full-width addition of both terms followed by rounding.
The decrement also wraps at sixteen bits; no local normalization is shown.

The root saves that DX:AX, supplies it as CX:DX to service `0x4200`, and
invokes interrupt `0x21`. It does not test the seek carry before requesting
sixteen bytes through the same stack-buffer helper. POP AX, POP DX and
MOV CX `0xFFFD` preserve the read-helper flags, which the following branch
tests before buffer consumption. It then adds sixteen to the saved pair
with carry. The buffer's first two words must equal `0x4246` and `0x564F`;
the first mismatch fails, while a second-word mismatch adds the next two
buffer words to the pair with carry and repeats the seek/read sequence.
No iteration limit is present in this local loop.

On both signature matches it publishes the pair to DS-relative words
`0x0114` and `0x0116`, and the next two buffer words to ES-relative words
`0x356C` and `0x356E` after loading ES through CS-relative word seven.
The common failure path calls close service AH `0x3E`, then sets AX
`0xFFFF` and enters the restore suffix. That suffix restores DI, SI, DS,
SP and BP and far-returns with eight bytes of argument cleanup. The CX
values selected before close are not established as preserved external
results; AX's failure value is written after the interrupt.

## Interpretation

This supplies the local initialized-buffer admission gates and return
consumption relevant to Q-EXE-001 and Q-EXE-010. It separates the requested
extent, returned count, currently tested CX, buffer fields and arithmetic
widths. The explicit DS-from-SS operation supports the outgoing buffer's
stack identity rather than assuming all equal offsets share storage.
Seek success, actual initialized bytes, external register preservation,
input field writers, loop termination, root admission and later consumers
remain open. No complete_reading or inventory replacement follows.

## Alternatives

A carry-clear helper return is not an exact-count assertion. Ignoring the
intervening ADD would incorrectly carry the final-byte addition into the
high word. A failed seek is not locally rejected before the next read.
Conditional CFG closure does not establish the external read contract or
bound the repeated header search. Selected CX diagnostics cannot be called
stable return values without reading the close service's preservation.

## How to reproduce

At revision `7cabb58`, independently decode the installed source identity
in FND-EXE-236 in sixteen-bit mode: shipped `0x000400A0..0x00040137`
at initial IP `0x0050`, `0x00040185..0x00040190` at IP `0x0135`, and
`0x000402DB..0x000402EB` at IP `0x028B`. Model segment `0x4AE5`, MZ
header `0x5200`, load segment `0x1000`. Read FND-EXE-175's frame setup
as the candidate frame, not as native admission.

Run the committed x86-bounds wrapper with entry and sole region entry
`0x000402DB`, region `0x00040050..0x00042050`, segment `0x4AE5`, IP zero,
sourceKind mz and the same hash. Supply no seeds or summaries; defaults
are 512 steps, 64 paths and depth eight. Retain the returning-interrupt
assumption and the near-return exit. Track flags through the intervening
MOV/POP instructions and each ADD separately. Source and reports remain
in GAME_DIR; no original instructions or bytes are committed.
