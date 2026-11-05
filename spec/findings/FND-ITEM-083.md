---
id: FND-ITEM-083
title: Eviction cleanup gates descriptor decrement on a saved handle and clears the object after kind-two child requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0B4A..31E0:0D09
tool: tools/research/exec-census/resident_listing.py at 6201ee9b; Python 3.12.10, Capstone 5.0.7; retained window_operands.py census; composition with FND-COMBAT-149, FND-PARTY-130/131 and FND-ITEM-077/080/082
environment: null
---

## Observation

Q-PARTY-116 retains descriptor count writers and intervening object lifetime.
The retained descriptor operand report identifies 31E0:0C0B in the eviction
cleanup as another count store. FND-COMBAT-149 binds the eviction caller's
two zero words and return handling; FND-PARTY-130/131 identify its kind-two
children. This reads the full local body, without admitting native inputs
or treating the operand search as an exhaustive writer census. Addresses,
offsets and sentinel words are hexadecimal; other counts are decimal.

### Entry, links and saved rectangle

31E0:0B4A returns AL zero if DS:265B is zero. Otherwise it selects
DS:67BB+37*slot with word arithmetic, without its own slot bound. It uses
object word +1 times eight at word width to read placed-entry byte +5
through DS:67B7, and saves object word +10 before later work. It returns
AL zero when object byte-zero mask two is set or the placed byte's low
three bits are six or seven. The placed read precedes these rejection gates.

For object word +21 other than FFFF it writes object word +23 into that
neighbor's word +23. For object word +23 other than FFFF it writes object
word +21 into that neighbor's word +21. Neither index has its own range
or reciprocal-link check. Native link/slot identity and disjoint storage
are needed before these writes can be called a consistent unlink.

If the second argument's low byte is nonzero, it saves a rectangle:
object words +3/+5 minus DS:1408/140A, then those coordinates plus unsigned
bytes +13/+14. All additions/subtractions keep word width. It captures
these values before any descriptor or kind-two child request.

### Descriptor and handle branch

Saved object word +10 equal to FFFF skips descriptor work and the later
store to +10. Otherwise signed object word +16 greater than zero selects
DS:6577+9*index, without its own less-than-64 or 00FF exclusion. It subtracts
one from the count byte at byte width. Zero becomes 255; one becomes zero.
The zero-result path requests release of a nonnull descriptor far pointer
at +3 through 444C:0092, stores its returned DX:AX, then fills all nine
descriptor bytes with zero, including when the pointer was already null.
FND-ITEM-080 supplies the fill and conditional release-wrapper contracts;
a zero pointer result does not prove underlying release success.

A nonzero resulting count skips release and descriptor fill. Nonpositive
object word +16 also skips them. Every admitted saved-handle branch then
writes FFFF to object word +10. It does not clear object word +16 here.
Descriptor zero is excluded by the positive-word test, unlike acquisition's
successful index-zero result. This branch differs from FND-ITEM-082's
decrement helper, which first excludes zero counts and never frees buffers.

### Later children, object fill and return

Next it reads the slot kind byte at 4F49:0C33+3*slot. Kind two requests
overlay 193 +1DB0 through 573B:002F(slot), then 371D:0002(slot, FFFF,
zero dword, one). These gates do not depend on the second argument's low
byte or saved handle. All admitted kinds then request 2D40:2410(slot)
and 2D40:03C1(slot), followed by a fill of the selected 37-byte object
with FF. A returned child result does not gate this fill or the next step.
The children still need their own effects and preservation admission.

With second-argument low byte zero it returns AL one immediately after
the fill. Otherwise, nonzero DS:143F first requests 3D72:0B84,
2707:00B4(zero dword, one word), and 3D72:0942, then clears DS:143F.
It requests 31E0:128C with the current DS:1408/140A, third argument,
the four previously captured rectangle words and one. Thus the rectangle
is captured before children while the view words are reread afterward.
The redraw result is ignored; AL is set to one. FND-ITEM-077 describes
redraw's own gates, not successful completion of all graphics effects.

SI/DI/BP are restored on return. The explicit result is AL; no full AX
result or ES/scratch preservation is inferred. Early-zero exits occur
before link stores; later child failures have no own zero-result branch.

## Interpretation

This removes eviction cleanup's local descriptor/count/handle gates,
neighbor-store ordering, object fill and child/redraw request ordering
from Q-PARTY-116. It adds a distinct lifetime writer to acquisition and
common cleanup, without asserting a globally balanced count. Native
placed/object/descriptor/neighbor/header spans, counts, aliases, actual
eviction inputs and the named children's effects remain open.

## Alternatives

Assuming this decrement always follows eviction admission ignores the
saved-handle gate. Assuming kind-two requests require redraw mode ignores
their independent kind test. AL one does not prove resource release,
effect removal or redraw success. The existing eviction caller and
kind-two findings remain valid bounded readings; no native defect,
exhaustive writer coverage, complete reading or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run the approved resident_listing.py
with project-local evidence Python -I on the half-open location range.
GAME_DIR/analysis/item-removal-20261104/ holds
effect-eviction-cleanup-contract.txt and its -repeat pair, both SHA-256
8c94411beb874b9b5710c51d57c1d30f12d70aa75f9edbd99541842926ec8a6c.
Reuse effect-descriptor-operands.txt as a candidate index with its retained
controls and exclusions. Keep original-derived reports local. No original
game or isolated function was executed.
