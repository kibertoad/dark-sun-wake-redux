---
id: FND-SOUND-057
title: Submission children do not compensate the descriptor wrapper cleanup and capability-disabled submission makes no interrupt
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:05EF..4842:063A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0F57..4842:1045
tool: resident_listing.py at 0a1f7320; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-056
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
Q-RNG-040 retains whether children can compensate the extra
argument cleanup in descriptor wrapper 4842:182E. This reading
settles the ordinary local child-frame component, not admission
or later reseeding routes.

### Arithmetic child

0F57 takes its incoming word, clears BH after loading it in BX,
and computes 0100 minus that low byte. It builds four words for
05EF: numerator low 4240, numerator high 000F, denominator low
the computed value, denominator high zero. Under unchanged
arguments this is unsigned 1,000,000 divided by 256 minus the
incoming low byte. The divisor is in 1..256, never zero.

05EF loads the numerator into DX:AX and denominator into CX:BX.
When CX is zero and either DX or BX is zero, it uses word DIV
BX and zeroes DX. Otherwise it performs 32 unsigned shift/subtract
iterations, leaving the quotient in DX:AX. For this caller DX is
000F, BX is nonzero and CX zero, so it takes the iterative path.
That path has no call, interrupt, memory write or changed SP beyond
its prologue/epilogue. Its temporary use of BP as divisor high word
does not select a new stack frame for any later memory operand;
POP BP restores the saved value. Both local return paths restore
DS/SI/DI/BP and use ordinary RETF, removing no arguments.

0F57 removes its four constructed words, restores its saved
DS/SI/DI/BP and uses ordinary RETF at 0F91. It does not remove
the incoming word. Its caller receives the low quotient in AX
and high quotient in DX; physical parameter units are not proved
by this arithmetic alone. Neither body compensates 182E's
four-byte cleanup of one incoming word.

### Capability-gated submission child

0F92 saves BP/DS/SI/DI and tests CS:0E2A bit one. When clear
it proceeds directly to the common register pops and ordinary
RETF at 1044, without another call, interrupt, argument write
or device request. This is a bounded no-interrupt return branch;
actual capability-word production and selected driver admission
remain requirements before claiming it occurs natively.

When the bit is set it publishes completion pointer CS:0E76 at
CS:0E3E/0E40 and its first argument at CS:0E42. It loads its
incoming buffer pointer and publishes it at CS:0E38/0E3A.
For a count greater than 8000 unsigned, it publishes chunk size
8000 at CS:0E3C, sets DS to CS and SI to 0E38, and issues
interrupt 66 with AX 068A. It saves/restores the stream DS/SI
around that request, advances the far pointer by 8000 using
linear arithmetic and paragraph normalization, subtracts 8000
from its incoming count words and repeats. A nonzero final low
count at most 8000 is published and requested once through the
same interrupt; zero skips that final request. External handler
register/frame preservation is required to apply these continuations.
There is no local test of interrupt success. No branch uses RETF
with an immediate argument cleanup or intentionally adjusts its
caller's saved frame. The count writes name its own incoming
argument slots, not the wrapper's saved locals under intact frames.

### Consequence for the wrapper

With the local arithmetic contract, unchanged frames and the
capability-disabled 0F92 branch, no unknown interrupt result is
needed to derive FND-SOUND-056's wrapper pops: entry SI into DI,
entry DS into SI and saved count high word into DS. The saved
return frame is selected again by MOV SP,BP. For capability-enabled
submission the same consequence remains conditional on the handler
and frame returning as declared. This does not establish a native
changed-DS route into any reseeding caller or a universal DS value.

## Interpretation

Q-RNG-040 can remove the ordinary child-cleanup ambiguity while
retaining actual descriptor/capability/driver admission, modified or
interrupt-affected frames and subsequent restoration/reseeding routes.
Q-SOUND-002 can reuse the finite arithmetic and chunk-request contract;
actual capacities, units, handler effects and output remain open.
The earlier push/pop census model is not frame-preservation evidence
for this wrapper without this additional analysis. No whole queue
closure, complete reading or observed native failure is claimed.

## Alternatives

- A child removes an extra incoming word and balances 184D:
  their local ordinary returns perform no such cleanup.
- Every submission requires an interrupt to reach its return:
  the capability-disabled branch returns directly.
- Quotient arithmetic proves a sample-rate unit or interrupt success:
  neither follows from the local calculation or unchecked requests.

## How to reproduce

Read both location ranges with resident_listing.py at the tool
revision above. Retain sound-submission-child-frames.txt under
GAME_DIR/analysis/handler-census-20261106/. Track pushed word
order, iterative division admission, both common epilogues,
capability-disabled control flow and incoming count-slot writes.
Combine with FND-SOUND-056's BP-relative stack calculation.
No original game, emulator session or emulated call was used.
