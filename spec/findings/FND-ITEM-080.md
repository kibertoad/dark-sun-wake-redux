---
id: FND-ITEM-080
title: Common slot cleanup decrements descriptor counts before deletion and fills the object after release requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F7E..1000:3FC1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:044C..31E0:05BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0092..444C:00F6
tool: tools/research/exec-census/resident_listing.py at 0a9862d3; Python 3.12.10, Capstone 5.0.7; composition with FND-PARTY-119/213, FND-CONFIG-144 and FND-ITEM-077/079
environment: null
---

## Observation

Q-PARTY-116 retains the full effects of common cleanup 31E0:044C after
FND-ITEM-079's call binding. FND-PARTY-213 establishes deletion before
slot-release requests, and FND-PARTY-119 describes the latter's separate
free-list limits. This reads the complete local cleanup, fill and resource
release wrapper, retaining their child/storage obligations. Addresses,
field offsets and masks are hexadecimal; lengths are decimal.

### Enable, event and redraw gates

044C first tests the byte at DS:265B. Zero returns before object/metadata
access or any child call. Nonzero computes the 37-byte object pointer
DS:67BB+37*slot and reads metadata kind at 4F49:0C33+3*slot, with word
arithmetic and no own slot bound.

Kind two with a nonzero second argument byte requests 573B:002F(slot)
and 371D:0002(slot,FFFF,zero_double_word,1), in that order. Results are
not tested here. Event-removal/handler effects and preservation remain
separate from this local argument binding.

A nonzero third argument byte requests later redraw, except object
word +10 equal to FFFF clears that local argument byte. Otherwise the
routine captures rectangle words from object +3/+5 minus DS:1408/140A,
with zero-extended bytes +13/+14 added for the far bounds. Arithmetic
keeps word width. The captured rectangle is not recomputed after deletion.

### Descriptor count and resource result

For signed object word +16 positive, it selects a nine-byte descriptor
at DS:6577+9*that_word and decrements its first byte at byte width. There
is no own upper index bound or 00FF exception in this cleanup, unlike
FND-ITEM-079's separate frame-walk gate. A zero count becomes 255, not
zero, and takes the nonzero-result branch. A count of one becomes zero.

Only a zero resulting byte takes descriptor cleanup. If its far pointer
at +3 is nonnull, it calls 444C:0092 and writes returned DX:AX back to
that pointer field. It then fills the entire nine-byte descriptor with
zero regardless of whether a nonnull pointer existed. A nonzero resulting
count skips both resource release and descriptor fill. Nonpositive object
word +16 skips this whole descriptor branch.

444C:0092 reads a word at supplied pointer offset minus four before its
null check. For nonnull input it derives a marker offset from that word
times sixteen, added at word width, then minus five; no far carry is
added. A marker byte 77 is cleared. Null input or a different marker sets
byte 55CD:0000 to one instead. All these routes still call 1000:149B
with the supplied pointer. If that child returns to the wrapper, DX:AX
is explicitly zeroed. Consequently a null result cannot by itself prove
valid header/marker storage or successful underlying release. The cleanup
caller avoids null input to this wrapper, but supplies no own header check.

### Deletion, release and object fill ordering

The routine writes FFFF to object word +10, requests 2D40:2410(slot),
then 2D40:03C1(slot), and finally fills all 37 object bytes with FF.
It does not inspect either child's result before filling. FND-PARTY-213
describes the chunk deletion dispatcher; FND-PARTY-119 retains slot-kind,
free-list and low-slot distinctions. These calls are not proof that
every requested object was deleted or placed on a free list.

FND-CONFIG-144's fixed fill helper forwards only the low fill byte and
word count, clears direction, handles an odd starting offset and writes
exactly the count bytes. With valid disjoint nonwrapping storage it thus
provides the nine-zero-byte descriptor fill and 37-FF-byte object fill
above. It returns the destination far pointer. No implicit preservation
of the newly filled object is inferred across other children.

If the retained third argument byte is nonzero, the routine next performs
the existing DS:143F presentation/timing-child sequence when that byte
is nonzero, clears DS:143F after those calls and requests 31E0:128C with
the captured rectangle, current origin, fourth argument as page and final
literal one. FND-ITEM-077 supplies its own gates. This redraw occurs after
the object fill; the routine has no explicit rollback branch. It restores
SI/DI/BP on return, not a defined success Boolean or preserved ES.

## Interpretation

This removes common cleanup's own enable/descriptor/fill/redraw gates,
count widths and child ordering from Q-PARTY-116. Remaining effects are
native object/descriptor/header/metadata identity, count producers and
aliases, event/handler deletion, underlying heap release, chunk deletion,
slot-list and graphics children and their preservation. No source-side
constructor publication or native slot release follows from these local
requests or from a resource wrapper returning null.

## Alternatives

Treating descriptor decrement as saturating would change the zero case.
Treating an FF-filled object as evidence that deletion/release succeeded
reverses the dependency: the fill follows their untested returns. Treating
marker failure as an early return omits the underlying release call.
No original was run, native defect inferred, complete reading declared
or parity status promoted.

## How to reproduce

Use approved DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py with -I and
the three location ranges above in order. Under
GAME_DIR/analysis/item-removal-20261104/, effect-common-cleanup-contract.txt
and effect-common-cleanup-contract-repeat.txt both have SHA-256
944ea8ea792782e4d28df3cb1814e6e10749a9fc5f2c8e5e9597394e3d4bc2a4.
Check byte versus word gates, decrement before zero testing, descriptor
and object fill counts, wrapper return versus child release, and redraw
after fill separately. Keep original bytes/listings/memory only locally.
