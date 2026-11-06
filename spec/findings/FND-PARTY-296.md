---
id: FND-PARTY-296
title: Known details-pointer allocation and release representations exclude the loader selector indexed alias by address residues
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007304A..0x00073098
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073251..0x0007330C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000866A1..0x00086715
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0140..4AE5:016B
tool: Existing-evidence review of FND-COMBAT-072/083, FND-CONFIG-184/209/211/223 and FND-PARTY-295; window_operands.py and overlay_listing.py at 1e1aa28f; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; 16-bit segmented-address arithmetic
environment: null
---

## Observation

FND-PARTY-295 identifies one established explicit-window writer which
could affect the loader vector selector: overlay 197 +32D4 reloads
ES:BX from current DS:19C5 and writes a word at offset BX+0110.
The selector load uses offset 0111 in its current state segment.
Offsets below are hexadecimal and effective offsets are word-sized.

FND-COMBAT-083's controlled literal-MOV writer census identifies the
known allocation and release publications to DS:19C5/19C7. The
allocation request is 0462 bytes with argument one. The wrapper adds
one and forwards request 0463, with no product or increment wrap in
this request (FND-CONFIG-209). FND-CONFIG-223's own allocator entry
and FND-CONFIG-211's lower return paths use offset 0004 for ordinary
nonnull allocated results. The clearing wrapper returns its retained
original pointer, rather than its normalized cursor; the outer wrapper
also returns that pointer unchanged. Frame and retained-pointer
preservation remain conditions of applying those return contracts.

### Nonnull allocation representation

For a retained pointer with offset 0004, the indexed word store's
effective offset is 0114. Its two bytes have linear-address residues
four and five modulo sixteen, for every possible ES segment. A
selector addressed at offset 0111 has residue one modulo sixteen,
for every possible state segment. These cannot be the same bytes.
Word offset wrapping and a twenty-bit address wrap preserve those
residues. This exclusion needs neither a particular heap segment nor
a proof that the heap lies above the resident image.

More generally, a two-byte store at BX+0110 can meet a selector at
segment-relative 0111 only when BX modulo sixteen is zero or one.
This is a necessary condition, not sufficient segment/alias admission.
The known allocated offset four satisfies neither case.

### Normal release representation

FND-CONFIG-184's release wrapper returns DX:AX zero after its runtime
call returns normally. FND-COMBAT-083 records publication of those
returned words to the primary details pointer. A retained 0000:0000
representation makes the indexed store address 0000:0110..0111.
It is disjoint from the loader's admitted initial 55CE:0111 backing.
Unlike the allocation exclusion, this null-pointer exclusion needs that
loader-state binding; it is not true for every possible state segment.
It proves neither successful release nor safety of using a null pointer.

### Explicit low-word writer census

The current inventory window 19C2..19C6 returns 402 operands and seven
raw candidates outside instruction starts. Its four established writes
are word stores at 19C3 and 19C5 in the known allocation and release
helpers. The 19C3 writes end at 19C4 and do not overlap the pointer low
word. Both 19C5 publications are positive controls from FND-COMBAT-083.
There is no other established writer among the reported operands.
The two raw SBB writes begin in the displacement byte of the preceding
segment-half store at each publication. Decode from the established
allocation/release paths, not from those interior bytes. The other five
raw candidates are reads. Unknown transfers to those interiors remain
outside the established-path result.

## Interpretation

The known nonnull allocation representation cannot overwrite the selector
through this indexed store, regardless of the allocated segment. Normal
release also excludes it under the loader-state binding above. The
next admission question is therefore whether the actual loaded pointer
retains one of those representations, rather than where every heap block
lies. FND-COMBAT-083 leaves non-MOV, wider, indexed, copied and aliased
pointer writers outside its census. DS identity, retained-result/frame
lifetime and any alternate pointer history still need evidence.
The new census also covers other access mnemonics with encoded
displacements in its window, but not arbitrary earlier-starting wider
writes or other computed effective addresses. It is not an exhaustive
alias census.

This removes the segment-bound dependency for the known offset-four
case; it does not declare every possible DS:19C5 value harmless. Other
selector-write mechanisms and routing conditions stay Q-PARTY-134.
FMT-PARTY-007 remains supported, without a complete reading or queue
closure. No allocation, runtime or original-platform success is asserted.

## Alternatives

- A later writer can change the pointer's low offset bits and invalidate
  the residue exclusion. The known publication paths alone do not exclude it.
- A different loader-state binding can invalidate the null representation
  comparison, while the offset-four residue exclusion still holds.

## How to reproduce

Reuse the publication census and checked contexts of FND-COMBAT-083,
the wrapper return contracts of FND-CONFIG-209/223, and all four lower
allocation paths in FND-CONFIG-211. Check FND-PARTY-295's reload and
store width, including the discarded earlier record index. Compare the
two destination-byte residues with the selector residue, retaining the
normal-return and pointer-lifetime conditions. For the null case compare
0000:0110..0111 with the admitted loader-state backing explicitly.
For the new low-word census run window_operands.py with the current
DSUN.EXE inventory and 19C2..19C6; require both known 19C5 write controls.
Check overlay 188 file 7304A..73070 and the release's established entry
7325A..7330C. Reports details-pointer-low-window, allocation-context and
release-context remain in GAME_DIR/analysis/handler-census-20261106/.
No original run or emulated call is needed.
