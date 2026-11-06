---
id: FND-PARTY-285
title: Seven overlay alias expressions finish the fixed-writer classification while retaining coordinate, counter and flag admission
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089833..0x00089AD7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008AC6F..0x0008AFAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092A83..0x00092E92
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092F3B..0x00092F5E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009304D..0x00093088
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00099260..0x00099287
tool: tools/research/exec-census/overlay_listing.py at 6d5fccb9; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-UI-044, FND-CONFIG-109/110/190 and FND-EXE-615
environment: null
---

## Observation

Q-PARTY-124 retains seven unclassified expressions in the fixed-writer
census after FND-PARTY-284. Installed DSUN.EXE is 634416 bytes,
XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`. Offsets and arithmetic
below are hexadecimal; decimal resource number 1001 is word 03E9.

### Image-centering word

Overlay 201 +018E writes DS:9B04. It subtracts current word 9B00
from 008B with word wrap, logically shifts right one and stores the
result. Therefore that result is 03E9 exactly when the sampled
9B00 is F8B8 or F8B9. The instruction has no signed clamp.
FND-UI-044 and FND-CONFIG-190 already identify the preceding frame-zero
dimension helper 1BF3:771C; its returned AX is stored at 9B00 at
+0176. Under retained storage that return supplies this arithmetic.
Actual picture selection, valid dimensions and intervening writers remain
conditions. An intended small image height is not a proved input bound.

### Counter-byte overlap

Overlay 202 +0A33 increments word DS:9B71. Selected word 9B70's
low byte is untouched; its high byte becomes the incremented counter's
low byte. The store leaves selected 03E9 exactly when byte 9B70
is E9 and the prior counter low byte is 02. The prior counter high
byte does not affect that condition. The immediate zero initializer
was separately excluded by FND-PARTY-276; it does not exclude the
counter later reaching three. Loop/counter provenance and adjacent-byte
preservation remain obligations.

### Four coordinate publications

All four stores below write selected word DS:9BDC directly:

| Overlay 208 store | Source and exact 03E9 condition |
|---|---|
| +1260 | Current local word BP-8 equals 03E9. |
| +1278 | Current local word BP-8 equals 03E9 on the stored-gate-zero branch. |
| +16B7 | First word argument BP+6, loaded into DX at +16B1, equals 03E9. |
| +17DE | The word loaded at +17D7 from DS:(67C5 + 25 * current word DS:9BE0), with word-width multiplication/address wrap, equals 03E9. |

In the +11F3 entry, +1212..+1219 initializes BP-8 to the word sum
of first argument and DS:1408. It calls +0885 before reaching
+1260/+1278. Under retained local state the two stores therefore
require that modular sum to be 03E9; the reached helper's frame/alias
effects must be admitted before using the initialization as their value.
FND-CONFIG-110 already identifies the coordinate and stored-gate routes.

The +16AB entry loads its arguments and stores the two coordinates
before its child call, as FND-PARTY-109 describes. It applies no own
range check to the first coordinate before +16B7.

The +17BD entry clears DS:9BE4 and requires stored-record byte +0C
to equal seven. It multiplies DS:9BE0 by 25 at word width, reads
the word at the resulting DS offset plus 67C5, and copies it first
to 9BD8 then 9BDC. FND-CONFIG-110 already identifies the intended
37-byte records. There is no own index bound here. Admitted record,
index, source-word and segment state remain necessary before applying
an intended coordinate bound; no shipped-table or all-indices exclusion
is claimed.

### Flag-byte overlap

Overlay 213 +0024 writes only byte DS:9D8C, selected word 9D8C's
low byte. At +0013 the entry calls 28C9:2522 and compares AX with
two. A result other than two clears byte argument BP+0C; either
branch then loads that byte and stores it. There is no local boolean
normalization of the preserved argument on the equal-two branch.
FND-EXE-615 already records this gate and its four direct callers.

At the store, selected word 03E9 requires loaded AL E9 and byte
9D8D equal to 03. Under retained argument state, a helper result
other than two excludes that result by the explicit zero store to
the argument. The other branch requires admitted argument E9 and
the neighboring byte 03. Caller arguments, helper/frame effects and
neighboring-byte provenance remain open; the parameter's flag meaning
alone does not prove its value.

## Interpretation

All 63 fixed inventory writer matches now have classified immediate
expressions: 36 literal exclusions in FND-PARTY-275/276, one canonical
segment-store exclusion in FND-PARTY-282, and 26 conditional expressions
in FND-PARTY-279/281 through 285. The latter count is 1 + 6 + 5 + 3
+ 4 + 7. None of these seven adds an unconditional exclusion.

This ends the unknown-expression component of that bounded census,
not the actual input, state-writer or lifetime obligations. The 34 raw
instruction-interior candidates retain FND-PARTY-277/278's classifications
and transfer exclusions. Larger/block/indexed/copied/alias mechanisms,
segment identity, other word-index inputs, 83 other direct getter sites
and repeated-hook preservation remain. FMT-PARTY-007 stays supported;
no queue item closes or complete reading is declared.

## Alternatives

- Logical right shift after wrapped subtraction does not clamp a large
  or invalid dimension to a small centering result.
- Incrementing an overlapping field can supply the selected high byte
  even when its own full word is not 03E9.
- Coordinates and flags remain numeric inputs until their actual domains
  and preservation are proved; semantic names do not exclude 1001.

## How to reproduce

Run overlay_listing.py on the fingerprinted executable with descriptor
and half-open file ranges 201:89833..89AD7, 202:8AC6F..8AFAD,
208:92A83..92E92, 208:92F3B..92F5E, 208:9304D..93088 and
213:99260..99287. Descriptor bases come from the retained overlay map,
not segment-order inference. Reports have final-alias201, final-alias202,
final-alias208-coordinate/point/record and final-alias213-prefix stems
under GAME_DIR/analysis/class-table-closure-20261106/.
Intersect exact store bytes with the selected words before deriving
each value condition. Reconcile 36 + 1 + 26 with the 63 inventory
matches separately from the raw candidates. No original game ran.
