---
id: FND-ITEM-082
title: Animation acquisition callers select automatically and publish failures while descriptor decrement skips zero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1808..31E0:1893
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2670..31E0:271E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1B38..31E0:1B60
tool: tools/research/exec-census/resident_listing.py at 6666bc00; Python 3.12.10, Capstone 5.0.7; composition with FND-SCRIPT-018, FND-COMBAT-149/162 and FND-ITEM-081
environment: null
---

## Observation

Q-PARTY-116 retains acquisition caller and descriptor-count admission.
FND-SCRIPT-018 identifies these two direct callers. This reads their
complete local bodies and the decrement child, extending the existing
flags-zero reading in FND-COMBAT-149/162. Addresses, field offsets and
sentinel words below are hexadecimal; other counts are decimal.

### Lazy resolution

31E0:1808 returns without touching a record when DS:265C is zero. Otherwise
it forms DS:67BB+37*slot with word arithmetic, without its own slot bound.
Object word +16 equal to 8000 or signed nonnegative also returns unchanged.
For another negative word it saves its word negation as the resource number.
The admitted number is consequently 1 through 32767 decimal on this path.

It converts frame byte +18 from FF to zero, retaining other values, before
calling 31E0:1893 with that number and selector 8000. It writes returned AX
to object word +16 without a success gate. If that word is negative but not
8000, it changes it to 8000 only when object word +1B is signed 1 through
600 decimal. A nonnegative result, including index zero, remains intact.
FND-ITEM-081's common FFFF failure and query-failure negated number are
therefore not universally retained or universally converted to 8000.
There is no rollback of the preceding frame normalization.

### Explicit image change

31E0:2670 saves its image argument in DI, requires nonzero DS:265B and
unsigned slot below 320, and selects that slot's 37-byte record in SI.
Flags byte mask one first requests 31E0:3388(slot, image-is-zero,
object-byte-zero masked by four). That last push defines its low byte;
the upper byte is not cleared by the local mask. The child effect and
preservation of saved state remain requirements, not assumed success.

With flags mask two and nonzero saved image it calls 31E0:1B38 with the
old object word +16, then calls acquisition with the image and selector
8000. This path has no own signed-positive image restriction. It saves
returned AX locally and to object word +16 before testing its sign.
A negative result exits, without restoring the old word or descriptor count.

A nonnegative result stores the supplied frame byte at +18 and sets
object byte-zero mask one. For slots below four it also copies unsigned
byte +13 divided by two to byte +7 and unsigned byte +14 to word +8.
No own descriptor-upper-bound or frame-count test precedes these stores.

When mask two is clear or the saved image is zero, it instead writes
8000 for zero or the word negation of the image for nonzero. It does not
call the decrement or acquisition on this route. Thus the flags-zero
negative-word behavior in FND-COMBAT-162 remains its bounded case, not
the behavior of every flag/image combination.

Both acquisition sites pass 8000. Neither supplies an explicit descriptor
index to FND-ITEM-081's alternate selector path. This is a statement about
these two known direct sites, not exclusion of unresolved indirect callers.

### Paired decrement

31E0:1B38 requires DS:265B nonzero and its argument signed greater than
zero. It forms DS:6577+9*argument with word arithmetic, without its own
less-than-64 check, and decrements the count byte only when it is nonzero.
Descriptor zero is skipped even when its count is positive. Count one
becomes zero, but this helper neither clears descriptor fields nor releases
the cached buffer. This differs from FND-ITEM-080's cleanup decrement and
zero-result release/fill path. No global balanced-count invariant follows.

## Interpretation

This removes the known acquisition sites' local selector binding, returned
word publication, frame/failure handling and paired decrement gates from
Q-PARTY-116. Automatic selection can return descriptor zero, while this
paired decrement excludes it. That conditional asymmetry is not a native
leak or defect diagnosis. Actual object/resource/count inputs, other writers,
record/table/stack nonaliasing and use-time preservation still need evidence.
The flag-one child and resource children remain separate obligations.

## Alternatives

Assuming every change releases the old descriptor ignores the flags/image
gates. Treating zero as failure ignores both callers' signed result checks.
Assuming decrement frees a buffer or admits only indices 1 through 63 adds
checks and effects absent from its body. Treating failure as transactional
ignores the caller's already-published word and prior decrement. Existing
flags-zero findings are retained; no confidence or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and the approved resident_listing.py
with project-local evidence Python -I on the three half-open location
ranges above in order. Under GAME_DIR/analysis/item-removal-20261104/,
effect-descriptor-callers-contract.txt and its -repeat pair both have SHA-256
d34040734dfba6bc3c39b08abc54a4ac9f092d4faf80096e9ec83707c121133c.
The tool checks the source fingerprint and applies its MZ relocations.
Keep listings and original-derived contents local. No original was run,
complete reading declared or exhaustive caller/writer census asserted.
