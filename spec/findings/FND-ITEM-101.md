---
id: FND-ITEM-101
title: Target distance uses wrapped deltas, fixed-point scaling and shared coordinate scratch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:382F..2D40:38E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1B0B:000C..1B0B:00A4
tool: tools/research/exec-census/resident_listing.py at 61325486; Python 3.12.10, Capstone 5.0.7; composition with FND-PARTY-098, FND-COMBAT-060 and FND-ITEM-099/100
environment: null
---

## Observation

Q-PARTY-116 retains distance effects at rectangle enumeration and the
immediate handler's strict target comparison. This reads 2D40:382F and
its complete 1B0B:000C/0088 child, refining FND-PARTY-098's approximate
distance description. Addresses/offsets are hexadecimal; arithmetic
constants below are decimal unless marked otherwise.

### Delta widths and wrapper scaling

The wrapper subtracts first coordinates from second coordinates at word
width, then conditionally negates each negative signed result. Negation
also keeps word width: signed minimum -32768 retains bit pattern 8000.
Thus these are wrapped coordinate differences, not unbounded subtraction
of the original input words.

If both resulting words compare signed at most 127, it passes them as
the second position against origin zero to 1B0B:000C without scaling.
For ordinary magnitudes 0..127 this is the small-delta path. It does not
independently reject the signed-minimum bit pattern at this comparison.

Otherwise it orders the two words by signed comparison, retaining the
larger as a. For ordinary magnitudes this is max(a,b)>=128 and the other
is b. It computes signed dword factor q=trunc(32512/a) and output scale
t=trunc(256*a/127), replaces a by 127 and b by the low word of
arithmetic-right-shift(b*q,8), and calls the child against origin zero.
The two divisions and products use the signed/dword operations of the
body; low-word and shift rounding are retained, not floating-point lengths.

For ordinary magnitudes 0..32767 this child input remains within 0..127
on both axes. The child result r is returned directly on the small path;
the scaled path returns low16(arithmetic-right-shift(signextend(r)*t,8)).
Large ordinary results can consequently have their word sign bit set.
FND-ITEM-099 compares that returned word signed; no nonnegative-distance
guarantee may be silently imported into its strict target gate.

### Child arithmetic and bounded ordinary domain

1B0B:000C saves DS and selects 4C49. Its near 0088 helper records each
first-coordinate word, forms the wrapped second-minus-first difference,
conditionally negates it and records the magnitude plus sign word one or
FFFF. Each magnitude is checked unsigned against 255; exceeding it returns
999 from the parent. Earlier scratch writes are retained on that exit.
After both checks it sums the two byte-sized squares, returning 999 on
word carry. No native service or other child is called.

For the wrapper's ordinary 0..127 child domain, neither check nor the
square-sum carry fails. With M the larger and m the smaller magnitude,
the initial estimate is M plus floor(m/4) when m+floor(m/2)>M;
otherwise it uses floor(m/8) when twice that threshold exceeds M, and
zero otherwise. It then increments the estimate until s*(s+1) is at least
the computed square sum. In this bounded domain the estimate and byte
multiplications fit their intended widths; this gives the integer rounded
root rather than exact real Euclidean distance. No equivalent claim for
every arbitrary direct child input is needed for this wrapper composition.

As an explicit signed-minimum case, a wrapper delta bit pattern 8000
with the other magnitude at most 127 takes the unscaled path. The child's
unsigned 255 gate rejects that magnitude and returns 999. The wrapper
then returns 999 unchanged. Other high-bit arithmetic cases retain the
machine-width paths above; no native coordinate admission is inferred.

### Persistent footprint and preservation

The child writes 4C49:0017 and 0019 with the first position's words,
000B and 000D with magnitudes, and 0011 and 0013 with sign words.
FND-COMBAT-060 already identifies this segment's physical overlap with
4C13 control storage; scratch is shared state, not an invisible local.
An early first-axis failure need not update the second-axis scratch.

The child restores DS/SI/DI/BP; the wrapper restores SI/BP. Neither
changes ES in its body. AX is the result; other scratch registers/flags
are not preservation promises. Valid stack/code/shared scratch and
nonaliasing with caller state remain conditions. Under those conditions
the explicit shared stores do not directly write the placement table or
DS:264E; aliases and later consumers remain separate obligations.

## Interpretation

This removes the distance child’s local numeric paths and direct scratch
footprint from Q-PARTY-116. Actual coordinates, scaled-result signedness
at callers, shared-state admission, accepted population, list/frame spans
and casting-child preservation remain. Calling this approximate distance
does not authorize replacing it with an exact square root or assuming it
is pure or nonnegative for every word input.

## Alternatives

Unbounded absolute differences erase word wrap and signed minimum.
Floating-point scaling erases the fixed-point truncations. Treating scratch
as untouched contradicts explicit stores, and treating an error as no
mutation ignores the write-before-check ordering. No native defect,
complete-reading declaration or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with resident_listing.py on the two half-open location ranges in order.
GAME_DIR/analysis/item-removal-20261104/ holds
effect-target-distance-contract.txt and its -repeat pair, both SHA-256
5b88ae3f5bc3ecc876e2a695d7654cc037b4e58539759d0aef1bce88a8ef47f6.
Keep ordinary bounded-domain conclusions separate from direct-child and
signed-minimum inputs. Reports remain local; no original or isolated function ran.
