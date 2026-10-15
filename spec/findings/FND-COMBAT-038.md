---
id: FND-COMBAT-038
title: The effect-icon list retains null-request slots and passes frame zero to its draw path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008AC6F..0x0008AE72
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078340..0x000783D2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3BEC..2D40:3D4C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3B67..2D40:3BD1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:01A6..3F96:0211
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
tool: tools/research/exec-census/direct_callers.py, overlay_listing.py and resident_listing.py; installed-build fingerprint and relocation/fixup checks
environment: null
---

## Observation

Exported overlay 202 +07CF, trampoline 577D:0039, is the effect-name
ICON-number consumer qualified by FND-COMBAT-029/030. This finding
follows its list construction and effect-icon loop through +09D2,
where a separate later list begins. It does not claim the remaining
function or every image backend as a complete reading.

### The other direct collector caller

The direct-call candidate query for overlay 193 +1D00 returns four
far-call sites: two in overlay 190's selector, qualified by
FND-COMBAT-036, and two in this exported icon consumer. This is a
raw direct-call query with fixup/trampoline checks; computed calls,
jumps and alternate transfer kinds are not excluded by it.

The icon consumer reserves 146 local bytes. Its output starts at
BP-146 and its bitmap at BP-32, again leaving a 114-byte span before
the bitmap. It prepares the bitmap for the owner read from
4E71:0B44, collects category zero, stores the returned word in DS:9B73,
then collects category one into output + that first count. The sum
is stored in DS:9B75. Both collections reuse the bitmap offset.
Neither call receives a remaining destination capacity.

Only afterward does it clamp each global count to at most 35 using
signed word comparisons. As in FND-COMBAT-036's exported two-category
path, the two individual 113-byte collector caps do not establish
that earlier aggregate writes fit the 114-byte pre-bitmap span.
Admitted aggregate counts, matching DS/SS, nonwrapping frame storage
and independent rows remain required under Q-COMBAT-012/013.

For the clamped total, the function copies effect bytes in list order
from the near DS output into byte zero of three-byte-stride entries
at DS:9B0A. This local copy assigns only that byte, not the other two
bytes of each entry. It does not perform the status selector's
priority sort. The effect loop then uses a word index beginning at
zero, bounded by both 35 and the current DS:9B75.

### Zero icons and null request pointers take different continuations

Each cached effect byte is sign-extended and multiplied by 31 to
index the effect-name table at 4C87:0000. The word at record +1D
supplies the ICON resource number. FND-COMBAT-029 identifies that field;
admitted effect IDs still belong to Q-COMBAT-009.

A zero ICON number shifts subsequent cached effect bytes down by one,
decrements the loop index and total DS:9B75, then rejoins the common
index increment. This rechecks the same logical slot on the next
iteration. The shift copies only effect bytes and includes the byte
at the old total as its final source. Under a prior total of 35 that
source is DS:9B73, the first-count field, rather than another active
effect. It becomes inactive after decrementing the total; no special
boundary byte is established. DS:9B73 is not decremented by this local
zero-icon branch.

For a nonzero ICON number, the function first writes a null far pointer
to its local BP-16, passes its address to 38FF:04AB and requests that
number zero-extended to 32 bits. It does not test returned AX. Its
next branch tests only whether the local far pointer is nonzero.

If the pointer remains zero, it skips the coordinate, draw and release
calls and reaches the common index increment. It does not compact the
cache or reduce DS:9B75 in this branch. Thus a zero-icon entry is
removed, whereas a nonzero icon request leaving a null pointer retains
its list slot and advances without these draw requests.

If the pointer is nonzero, it proceeds regardless of the reader's
returned status. FND-CONFIG-151 records why those tests are distinct:
the shared reader can assign an allocation before a later transfer
error. This finding does not assume every such pointer contains a
complete valid image.

### Coordinates and the two draw requests

The nonzero-pointer continuation requests an eight-byte local record
from 3F96:01A6, using the window pointer at DS:1431 and identifier
2BCD plus the current list index. Its returned AX is ignored. The
helper first clears all eight output bytes, calls 39D1:049C for APFM
and the low identifier word, then either returns FFFF on a null record
or copies eight bytes from record +0x10 through 1000:0452 and returns zero. Its guard,
lookup and copy effects remain dependencies; the caller supplies no
separate success test before reading the first two output words.

The caller adds those two words to window fields +0x96/+0x98. It makes
two calls to overlay 190 +0000, trampoline 571F:00DE, with these
word/far-pointer arguments in callee order:

| Argument | First request | Second request |
|---|---|---|
| First word | 1 | 1 |
| X word | First local coordinate + window +0x96. | First-request X + 1. |
| Y word | Second local coordinate + window +0x98. | First-request Y + 1. |
| Fourth word | 0 | 0 |
| Image far pointer | Pointer stored at 4E71:002F. | Requested ICON pointer. |
| Final word | 2 | 0 |

These are caller-cleaned fourteen-byte argument lists. The final word,
not the fourth word, is the frame index used by the reached image
wrapper. The wrapper forwards all arguments to resident 2D40:3BEC.
Both bodies index the resource's offset table at +6 + 4*frame_index.
The resident body's initial 3B67 precheck saves/restores SI, where it
holds that argument; its later backend calls push SI as their frame
argument. Pointer-helper and backend effects remain dependencies.
This agrees with FMT-IMAGE-001's frame-offset layout. The effect ICON
request therefore supplies frame zero; the other image request supplies
frame two. It is not a request to advance ICON frames.

FND-COMBAT-030 found at least one frame for every shipped referenced
ICON and four for ICON/16102. Frame zero is present in that installed
corpus, including the multi-frame resource. The local caller's fixed
index does not prove pixels, animation elsewhere, selected archive
identity or validity of a buffer left nonzero after a failed read.
The downstream backends, target surfaces, coordinate validity and
palette remain separate presentation dependencies.

The overlay-190 wrapper tests its supplied image pointer, but after
calling 2D40:3BEC it does not branch on that call's outcome before
reading frame geometry, requesting a rectangle operation and clearing
its temporary DS:33BC flag. Those reads therefore still require valid
image/frame storage; this wrapper is not an error-to-no-draw adapter.

### Release is requested after the draw calls

After both image calls return, the icon consumer passes the ICON
pointer to 444C:0092 and writes returned DX:AX back to its local slot.
Neither draw result gates this call. FND-CONFIG-165 records the
wrapper's metadata/marker accesses, unconditional runtime request and
zero pointer on normal return; it does not establish successful release
for an arbitrary or partially initialized pointer. The local loop then
increments its index. No reader error rollback or alternate retry is
present in this effect-loop continuation.

## Interpretation

This answers the local failure continuation and requested frame for
Q-COMBAT-010. It also supplies the second direct collector caller's
actual frame/output path for Q-COMBAT-012/013. The result is a
conditional request/lifetime contract, not an observed native panel.

Q-COMBAT-010 retains admitted archive/resources, valid pointer outcomes,
backend/frame processing, release effects and later interaction with
retained cache slots. Q-COMBAT-009 retains the active effect-byte domain;
Q-COMBAT-012/013 retain aggregate storage and membership-content
admission. FMT-COMBAT-003 remains supported rather than established.

## Alternatives

- Treating a null resource pointer as the zero-icon branch incorrectly
  removes a list entry that this continuation retains.
- Treating a nonzero pointer as successful I/O ignores the reader's
  assigned-before-error path and the caller's discarded AX result.
- Interpreting the fourth argument as frame selects the wrong parameter:
  the wrapper indexes frames with the final word at BP+0x12.
- Replacing the two draw requests with a rendered-icon claim skips
  buffer validity, resource selection and backend/palette dependencies.
- Normal return of the release wrapper supplies a zero local pointer,
  not proof of a successful native deallocation or safe failed-read path.

## How to reproduce

Use the locked evidence Python and fingerprinted installed DSUN.EXE.
Run direct_callers.py for 193+1D00 and 190+0000. Read overlay_listing.py
descriptor 202, 8AC6F..8AE72, and descriptor 190, 78340..783D2.
Read resident_listing.py for 2D40:3BEC..2D40:3D4C and
3F96:01A6..3F96:0211, 2D40:3B67..2D40:3BD1 and
1000:0452..1000:046E. Bounds are hexadecimal and half-open.

Follow the two collection destinations and post-call clamps, byte-only
cache copy/shift, distinct zero-field/null-pointer branches, discarded
AX results, coordinate outputs, seven-word-equivalent draw arguments,
actual BP+0x12 frame producer and release ordering. Compare reader and
release dependencies with FND-CONFIG-151/165 without changing their
claims. Original-derived listings remain only in
GAME_DIR/analysis/combat-effects/. No original function was executed.
