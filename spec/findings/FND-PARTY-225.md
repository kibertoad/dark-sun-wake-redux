---
id: FND-PARTY-225
title: The inventory bitmap resolver requests the negated object's OJFF image and returns zero on a failed request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007B26F..0x0007B2B9
tool: tools/research/exec-census/overlay_listing.py and trampoline_target.py; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

The hash-checked trampoline resolver maps 571F:00C5 to descriptor
190 +2F2F, file 7B26F. FND-PARTY-224 qualifies the inventory callers
that pass a selected 23-byte attached-record number to this entry.
FND-ACTOR-006 independently lists the OJFF immediate at file 7B2A0;
this is a reached push in the current entry, not merely a tag-byte hit.

The complete local body has one word argument and one child call.
Its sequence is:

1. Reserve 22 stack bytes. Put the SS far address of the first
   sixteen-byte local region into a separate local far-pointer word
   pair. The local data region itself is not cleared by this body.
2. Multiply the supplied record index by 23, retaining the low word,
   and add that word to the offset of the far record table at DS:19C1.
   Read the selected record's first word.
3. Negate that word at 16-bit width, retain the low word, then
   sign-extend it to a doubleword. Use it as the OJFF request number.
4. Pass tag OJFF, that doubleword number and the SS far address of
   the local far-pointer variable to 38FF:04AB. Remove twelve
   argument bytes after the child returns.
5. Test the whole returned AX word. For any nonzero AX, return zero.
   For zero AX, return the local data word at offset 0C hexadecimal
   within the offered sixteen-byte region. Return far.

The success continuation reads its original stack region directly.
It does not reload the possibly changed local far-pointer variable
and dereference it. A zero child status alone therefore does not
prove the image word was transferred into that original buffer;
the generic reader's supplied-buffer contract remains a dependency.

### Arithmetic and source-field correspondence

The record-index multiplication, near-offset addition and negation
are word operations, with no local index, object-number or overflow
guard. For example, a stored word representing -603 produces request
603 after negation and signed extension. Word 8000 hexadecimal
negates to 8000 and extends to FFFF8000; it is not converted into
positive 32768. A positive stored word one instead produces request
FFFFFFFF. These are instruction-width readings, not admitted native
input cases or executed experiments.

The offered buffer's returned word is at local BP-0A; the offered
buffer starts at BP-16 hexadecimal, so the difference is 0C. This
matches FMT-ACTOR-001's image word in a sixteen-byte OJFF definition.
FND-ACTOR-001 records every supported OJFF definition at that size
and associates its image word with a BMP resource. As an independent
data-layout control, its existing object-603 row gives image 2082.
Thus a successful transfer of that supported definition into the
offered region would make this body return 2082. No inventory call
using that control was executed.

The failure return zero is indistinguishable from a zero word read
on the success continuation. A child AX of 0100 hexadecimal takes
the failure branch despite AL being zero. The body does not pass
the child's nonzero status value through to its caller.

## Interpretation

This answers the local index/object-number arithmetic, OJFF selector,
source-field correspondence and zero/nonzero status branches of
Q-PARTY-087. It also qualifies the overlay-190 tag occurrence left
open by FND-ACTOR-006 and that part of Q-ACTOR-002.

FND-PARTY-224's caller can now be described conditionally as storing
the object-definition image returned by this resolver before its BMP
request. The cached word may also be nonzero before this path runs;
FND-PARTY-196 records such shipped values. This is not a claim that
every cached number was produced by the resolver.

Admitted record indices and identifiers, actual resource selection,
successful transfer into the original local buffer, register/global
preservation and all cache producers/invalidation remain open. No
complete reading or native pixel result is claimed.

## Alternatives

- Returning a current actor-slot image is not this body's local
  source: it requests OJFF by the negated first attached-record word
  and reads the offered definition buffer's image word.
- Treating the request number as unsigned after negation omits the
  explicit signed extension and changes the 8000 case.
- Treating failure as the resource service's exact error code omits
  the local replacement with zero.
- Following a rewritten output pointer instead of the offered local
  region would describe a different success read.
- The other OJFF tag occurrences in FND-ACTOR-006 and other cache
  writers are not covered by this local body.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python.
Resolve 571F:00C5 with trampoline_target.py, then read
overlay_listing.py descriptor 190, 7B26F..7B2B9, hexadecimal and
half-open, matching the committed function inventory's complete
local range. Inspect the reached OJFF push at 7B29E, whose tag
operand begins at FND-ACTOR-006's 7B2A0.

Track the local buffer, separate pointer variable, signed object
number and word result independently. Compare the returned local
offset with FMT-ACTOR-001's image field and FND-ACTOR-001's existing
object-603 data control; do not treat that correspondence as an
executed inventory case. Preserve the nonzero-word failure and
zero-status/original-buffer conditions. The report remains under
GAME_DIR/analysis/party-record-heads/drawing-20261018/bitmap-number-resolver.txt.
