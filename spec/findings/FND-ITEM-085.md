---
id: FND-ITEM-085
title: Dimension pointer helper normalizes signed displacement arithmetic without reading or validating data
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:060E..1000:063A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:064D..1000:066B
tool: tools/research/exec-census/resident_listing.py at 3d9625bb; Python 3.12.10, Capstone 5.0.7; composition with FND-ITEM-077/084
environment: null
---

## Observation

Q-PARTY-116 retains the pointer-helper effects used by redraw and image
refresh. FND-ITEM-077/084 describe their pointer-result dereferences.
This reads both owned ranges of 1000:060E, including its shared negative
arithmetic tail. The intervening 1000:063A entry is a different entry,
not another branch reached from 060E. Register words and constants below
are hexadecimal unless explicitly described as decimal.

The input pointer is DX:AX and the displacement is CX:BX. Signed CX
nonnegative takes addition. Negative CX forms the two's-complement
doubleword magnitude in CX:BX and takes subtraction; the signed minimum
retains magnitude 80000000. This path does not recursively negate it.

On addition, AX plus BX keeps its low word and a carry adds 1000 to DX.
The low nibble of the displacement's high word contributes that nibble
times 1000 to the segment at word width. It then adds the resulting
offset divided by sixteen to DX and retains only the offset's low nibble
in AX. The high displacement bits make no further contribution.

Subtraction subtracts the magnitude's low word from AX. A borrow subtracts
1000 from DX. It subtracts the magnitude high word's low nibble times
1000, then normalizes the remaining offset in the same way: its quotient
by sixteen is added to DX and its low nibble becomes AX. All segment
arithmetic keeps word width.

Thus, writing input segment s, offset o and signed doubleword displacement
d as integers, the returned offset is in decimal 0 through 15 and the
returned segment/offset representation satisfies

`16 * returned_segment + returned_offset = (16*s + o + d) mod 1048576`.

This equation describes the produced register representation. It is not
a claim about A20, an emulator's mapping, object membership or readable
host/guest memory. A negative displacement, carry, segment wrap or null
input has no separate failure return or validity test.

The body has no data-memory operand, child call, interrupt or port access.
Its far return consumes the ordinary return address from the stack. It
leaves DS, ES, SI, DI and BP unchanged. AX/DX are the output; CX and flags
are scratch. BX is unchanged on the nonnegative path but is scratch on
the negative path. There is no separate success indicator.

FND-ITEM-084 supplies each displacement from the frame table's two words
and each base from the bitmap-cache buffer pointer. This helper neither
changes those tables nor supplies their bounds. Its two returning calls
cannot themselves mutate the frame/cache state, but their callers still
need valid pointers, correct ownership, admitted aliases and readable
spans for the subsequent dimension-byte reads. FND-ITEM-077's redraw
pointer uses have the same arithmetic-only child contract.

## Interpretation

This removes 1000:060E's own address arithmetic, data-side effects and
register preservation from Q-PARTY-116. It replaces an unspecified child
effect with a bounded normalization contract, while retaining native
cache/frame/buffer identity, caller inputs and other child effects.
No allocation, pointer validity, completed bitmap load or native input
admission follows from a normalized returned offset.

## Alternatives

Treating this as a plain word-offset addition misses carry/borrow and
normalization. Treating it as an unbounded linear address misses segment
wrap and discarded high displacement bits. Treating the result as checked
or successfully dereferenced adds operations absent from the helper.
No original was executed, complete-reading declaration added or parity
status promoted by this local reading.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. With project-local evidence Python -I,
run resident_listing.py on the two half-open location ranges in order.
Under GAME_DIR/analysis/item-removal-20261104/,
effect-pointer-normalization-contract.txt and its -repeat pair both have
SHA-256 f06ee18d58eb8667561d1376b6a6bf581cb266a3094698357e91e45d6c5edd51.
Check both sign routes, carry/borrow and nibble/word widths. Original-derived
listings remain local; no source bytes or translated implementation are added.
