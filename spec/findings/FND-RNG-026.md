---
id: FND-RNG-026
title: Two coordinate samplers qualify the other six recovered range-helper sites
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000642E0..0x00064461
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064461..0x0006453D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006453D..0x000647DA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E907..0x0007EAE1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3A03..2D40:3A43
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py, overlay_listing.py and resident_listing.py; installed executable identity, MZ and FBOV fixup checks
environment: null
---

## Observation

The remaining six relocation/fixup-backed range candidates are overlay
179 +01D0, +01E1, +01F1, +0384, +0395 and +03BA. Trampolines
56A7:0020, 56A7:0093 and 56A7:0098 establish entries +0000, +0181
and +025D. The wrapper at +0000 calls +0181 on two branches and
+025D on another shared branch. Reading from those entries to their
returns verifies all six range calls and their loop order. They are
coordinate-sampling consumers of the known generator through
FND-RNG-012's range helper; they do not supply dice damage in these
local bodies.

### Sampler at overlay 179 +0181

This entry receives eight word-sized arguments. The first four act as
two coordinate bounds, the fifth supplies a scale, the sixth supplies
an upper factor, the seventh is a near output offset and the eighth is
a signed word capacity. Its prologue computes a signed 32-bit fixed-point
step from 25600 divided by the signed word difference of the fourth and
second arguments. It derives a second fixed-point quantity from that
step and the fifth argument, and initializes a coordinate accumulator
from the first argument shifted left by eight.

For each entered iteration, its three range calls occur in this order:

| Site | Lower word | Upper word | Local use |
|---|---|---|---|
| +01D0 | 0 | Low word of the derived fixed-point quantity shifted right arithmetically by eight | Offset added to the current first coordinate. |
| +01E1 | Second argument | Fourth argument | Numerator source for the second coordinate. |
| +01F1 | 256 | Sixth argument | Divisor source for that coordinate's scaling. |

The code stores the first range result, then obtains the second and
third results before writing an output record. It shifts the signed
second result left eight bits in 32-bit arithmetic, divides by the
signed third word and keeps the quotient word. It writes the first
coordinate accumulator's shifted value plus the first result, and that
quotient, as two words at DS:output + four times the record index.
It then advances the accumulator and increments the record index.

The loop tests signed record index against signed capacity and shifted
coordinate accumulator against the third argument before entering an
iteration. A skipped loop reaches none of these range sites. This is
not an early guard on the prologue's division: equal second/fourth
arguments can reach that division before either loop test. No total
admitted argument or termination claim is made here.

Each entered iteration therefore makes three local range calls, with
zero to three generator draws according to their signed endpoint gates.
Equal or reversed bounds still return the lower word and can affect
the generated pair. They are not silently normalized, resampled or
treated as a request for an ordinary positive random interval.

### Sampler at overlay 179 +025D

This separate entry takes ten word-sized arguments and writes paired
words through its ninth argument, a near output offset, bounded by a
signed tenth-argument capacity. Its prologue derives fixed-point scales
from the third/fourth dimensions, adjusts a step and initializes a
byte-wrapped angular span from its first two arguments. A loop indexes
paired signed words at 4DA1:0060 + four times the current byte index,
scales them, and calls 2D40:382F on the resulting pair before the
following three range calls:

| Site | Lower word | Upper word | Local use |
|---|---|---|---|
| +0384 | 0 | Returned word from the preceding pair helper | Starting sample for a radial adjustment. |
| +0395 | 32 | 48 | Divisor for the first sample after its word-width left shift by five. |
| +03BA | 256 | Eighth argument | Further fixed-point divisor. |

The literal pair 32/48 is a 32-bit push whose two little-endian
argument words are 32 then 48. This range always costs one generator
draw. The other two calls draw only when their signed lower is below
their signed upper. All three calls occur in that order before the
paired output stores, assuming returning callees and valid storage.
Thus each completed iteration reaches three local range calls and
costs one to three direct generator draws through those calls.

The arithmetic after the second call keeps the first result's left
shift by five at word width before signed division. Later adjustments
use both word and 32-bit fixed-point arithmetic. Widening that shift
or treating the two tables as unsigned coordinates would change this
local reading. Full geometry, helper semantics, table producers and
caller argument domains are not established here.

The loop can skip output when its remaining angular span or capacity
test fails. Its prologue and pair-helper work precede those tests;
a zero output capacity does not establish a draw-free whole function
or bypass its earlier arithmetic dependencies. Output aliases can also
change later parameters/locals unless their separation is established.

### Caller and scope of the census

Overlay 193 +0617, exported through 573B:008E, copies a source record
into its local frame, returns before generation when its byte at +17
is FF, and otherwise selects an 18-byte profile through that byte.
At +0709 it calls overlay 179 +0000 with a near local-buffer offset
and capacity 250. The selected mode is bits 2..3 of profile byte 5;
other profile fields and mapped values supply dimensions and factors.
The wrapper selects its samplers by mode and another byte flag, and
passes the buffer and count through later processing calls.

The caller's buffer offset is formed from SS, whereas the sampler's
near output stores use DS. Neither instruction sequence establishes
that these are the same storage on every admitted path. No native
buffer, rendered pixels, timing, effect success or damage result was
observed. The read establishes paired-word generation and its caller
connection, not a validated visual-effect specification.

Combined with FND-RNG-025, all eleven candidates returned by this
canonical range-helper incoming query now have qualified entries and
local consumption order. This is the query's bounded direct-call scope:
relocation-backed resident far calls, matching overlay fixups and
same-segment relative near calls. It does not exclude stored/computed
targets, noncanonical segment aliases, jump transfers used as calls or
other generators. Per-call argument admission and a complete rule/draw
map remain Q-RNG-001. RNG state access through computed pointers or
bulk/overlapping writes is independently retained by Q-RNG-007.

## Interpretation

The range helper serves both psionic checks and coordinate sampling.
The fact that two callers use the same generator is not evidence that
their samples have the same gameplay meaning. Record generator draws
according to each reached helper's endpoint gate and preserve their
order, including samples whose later output is not presented.

This removes the six remaining overlay-179 instruction-qualification
and local draw-order obligations from Q-RNG-001. It does not settle the
profile values, full caller input/state identity, geometry, every callee
or native presentation. No complete-reading declaration follows.

## Alternatives

- Three range-call instructions per sample do not mean three generator
  draws: the helper's equal/reversed-bound branches remain active.
- The constant 32..48 sample is independent of the following 256-based
  factor sample. Reusing one draw for both changes the state sequence.
- No shipped profile census, mode-default admission or renderer reading
  is supplied here. Do not infer that every mode produces coordinates or
  that the output buffer is visible merely because it was written.
- The delivered resident harness cannot execute these overlays. No native
  run, emulated-call fixture or gameplay implementation was added.

## How to reproduce

Use the project evidence Python and approved installed DSUN.EXE.
Run direct_callers.py for 2D40:3A03, 179+0000, 179+0181,
179+025D and 193+0617. Resolve 56A7:0020, 56A7:0093,
56A7:0098 and 573B:008E with trampoline_target.py. Read the four
listed file ranges with overlay_listing.py from their established
entries, and the resident reducer with resident_listing.py.

Trace the wrapper branches to both samplers, each loop's initial test
and back edge, argument push order/cleanup, result stores and widths.
Check the constant pair's two stack words and count draws using the
signed endpoint predicate in FND-RNG-012, rather than counting call
sites alone. Retain the prologue arithmetic, callee, near-pointer and
alias prerequisites. Keep all source-derived output only in GAME_DIR.
