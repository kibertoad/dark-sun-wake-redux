---
id: FND-PARTY-290
title: Cache-miss lookup and allocation interfaces narrow DI preservation to saved words, guards and transfer services
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0045..39A9:0157
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0157..39A9:01A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:01A2..39A9:0289
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38DC:0005..38DC:0066
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0615..38FF:07B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0008..444C:008E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:08FB..37FC:09C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0A72..38FF:0AA1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:01AF..44DE:022C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:041A..1000:0431
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0583..1000:05C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07D9..1000:07F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:15A5..1000:1622
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:18BC..1000:1959
tool: tools/research/exec-census/resident_listing.py at aa8a8321; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-PARTY-289, FND-CONFIG-163/223 and FND-COMBAT-120
environment: null
---

## Observation

Q-PARTY-134 retains the cache-miss DI effects named in FND-PARTY-289.
Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Offsets below are hexadecimal.
This follows only register/interface effects needed for the bounded
reflection route, not unrelated resource-service behavior.

### Lookup and traversal interfaces

| Helper | DI result on its ordinary continuation |
|---|---|
| 39A9:0045, type search | Saves DI after its four local bytes and saved SI, then restores it at 012A on the common own return. The saved DI is SS:BP-8. Live DI changes in pointer arithmetic children need not retain the incoming value. |
| 39A9:012E, signature | No own DI/EDI write or child after its stack guard. The guard-skipping path preserves DI. |
| 39A9:0157, traversal | No own DI/EDI write or child after its stack guard. The guard-skipping path preserves DI. |
| 39A9:01BC, plain-number search | No own DI/EDI write. Apart from its guard it calls only 38DC:0005, whose guard-skipping body has no DI write or other child. |
| 38FF:0615, indexed lookup | No own DI/EDI write; its reached children are the type/plain-number searches, preparation 37FC:08FB, positioning 38FF:0A72 and transfer 44DE:01AF, plus its guard. |

The type search's normal register restoration requires its SS:BP-8
and frame to survive record accesses, child effects and aliases.
Guard-skipping is a stated path condition, not proof every runtime
guard skips or returns normally. FND-CONFIG-163 retains the stack
guard's interrupt and tail continuation; it is not assumed harmless.

### Allocation and clearing have restoration boundaries

444C:0008 has no own DI/EDI operand. Its multiplication child
1000:041A is a call-free body with no DI writes. It then reaches
1000:18D0 as FND-CONFIG-223 describes; the wrapper's own subsequent
header-derived marker writes still require admitted separate storage.

18D0 does not change DI itself. Its multiplication helper 07D9
has no DI write or child. The called allocator 15A5 saves DI at
15B3 to SS:BP-4 and restores it at 161E on the common return,
including zero-request, rejection and ordinary allocated returns.
Thus lower allocator live-register changes do not independently
need to preserve the incoming DI; its saved word, stack continuation,
heap/metadata aliases and shared state still need admission.

The fill primitive 18BC saves DI at 18BF to SS:BP-2, loads the
destination into ES:DI and executes repeated byte stores. It restores
DI at 18CB before its near return that removes eight argument bytes.
Actual fill destination/extent/direction must not overwrite that saved
word or its frame. This is distinct from proving the fill's bytes or
capacity. The wrapper's selected nonnegative cursor path through
0583/05A1 has no DI write or child; its negative tail to 05E5 is
outside this caller's chunk form, as FND-CONFIG-223 qualifies.

This resolves the ordinary allocation/clear register mechanism with
specific saved-memory conditions, without reopening every lower
allocator's unrelated behavior. The rereading includes closing returns;
FND-CONFIG-223 supersedes 210's truncated reproduction endpoints.

### The transfer-side remainder is explicit

Preparation 37FC:08FB has no own DI/EDI operand. Besides its guard,
it calls positioning 38FF:0A72, write 44DE:022C and final handle
operation 44DE:02F8. Positioning 0A72 has no own DI operand and
calls 44DE:0064 after its guard. The read wrapper 44DE:01AF
has no own DI operand, calls near 44DE:0002 and issues DOS INT
21h on its two transfer paths. Absence of an own write does not
establish these reached services' register or saved-memory effects.

The remaining transfer-side interface audit is therefore
44DE:0002/0064/022C/02F8 and the reached interrupt contracts, plus
actual guard continuations. This is a concrete boundary, not a claim
that an arbitrary successful I/O call preserves the caller's registers.

## Interpretation

The guard-skipping lookup/traversal/pointer-arithmetic DI questions are
answered; type search and allocation/fill restore DI from specific
saved words. Q-PARTY-134 retains those named saved spans and actual
guard behavior, the explicit transfer-side remainder, effect helper
SS:BP-4, queue state and other number producers. Register restoration
does not prove lifetime or source/key/buffer coherence.

No whole getter site or question closes: eighty direct sites under
Q-PARTY-124 and three under Q-PARTY-134 remain 83 total. Indirect
inputs and repeated-hook preservation remain. FMT-PARTY-007 stays
supported; no complete reading is declared.

## Alternatives

- Lower callees need not preserve live DI when a caller restores its
  incoming value, but overwriting that saved word defeats restoration.
- A nonwriting wrapper does not prove an interrupt's register behavior.
- A guard-skipping result cannot be extended to an admitted guard call
  merely because ordinary paths reach the same source suffix.

## How to reproduce

Run resident_listing.py over the owned locations above. Preserve the
39A9:01A0..01A2 inventory gap and the selected cursor-path exclusion.
Retained reports di-miss-lookup-helpers, di-miss-services,
di-miss-zero-allocation, di-miss-allocation-boundaries and
di-miss-allocation-arithmetic remain under
GAME_DIR/analysis/class-table-closure-20261106/.
Compare save addresses with BP and trace every ordinary own return;
do not infer a callee-saved convention from a function name.
No original game ran.
