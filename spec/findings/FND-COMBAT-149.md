---
id: FND-COMBAT-149
title: Flags-zero object re-placement still has conditional eviction and script or animation child paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0D09..31E0:128C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2670..31E0:27A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:071F..25AF:0742
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0B54..2D40:0BA4
tool: tools/research/exec-census/resident_listing.py at 14b24617; project-local Python 3.12.10 and Capstone 5.0.7; inventoried entry boundaries and MZ relocations
environment: null
---

## Observation

Q-COMBAT-066 retains reconstruction children of the numbered reader.
FND-COMBAT-077 establishes that its call to 31E0:0EFF supplies flags zero
and skips the RDFF branch. FND-ACTOR-003 describes the OJFF request and
record initializer but leaves the continuation after 1040 read mainly for
calls. This accounts for the complete loader's own paths and the direct
children specified below. Offsets and masks are hexadecimal unless a
quantity is explicitly decimal.

### Flags zero skips RDFF, not the remaining loader

The loader reads its requested slot into SI, initializes a local byte to
zero, and returns FFFF when DS:265B is zero. Requested slot FFFF becomes
319 decimal. It supplies the address of a local destination-pointer slot
to the complete OJFF reader, with sign-extended object number. A nonzero
resource result requests 2D40:03C1 for that slot, then fills its indexed
37-byte presentation record with FF and returns FFFF. Actual selected
resource length, stack backing, aliases and external effects remain
prerequisites; the request has no own destination-capacity argument.

The successful OJFF path makes the image overrides and record initialization
described by FND-ACTOR-003. The complete 0E1B initializer has no child call;
its own stores are to the supplied presentation record, using the selected
placed entry and OJFF fields. Under unchanged flags zero, the low-byte
mask-four test skips RDFF to 1040. That destination does not return early:
it continues through allocation, linking and presentation work below.

At 1040 an entry-index word FFFF in the indexed presentation record takes
the fill/FFFF path. Other cases set the local category byte to one when
the current slot's three-byte-table kind is two, then distinguish slot 319
from other slots. A passing caller slot range is not a general parent
bound: subsequent arithmetic uses word-width multiplied indices, and a
replacement allocator result is rejected only for sentinel 9999 decimal.

### Temporary-slot allocation can reach owner-effect removal

For slot 319 the loader asks 2D40:0461 for a replacement using that local
category byte. On sentinel 9999 it calls 31E0:0D09 with its supplied
category byte and two zero words, then retries allocation. A second failure
calls 0D09 with the category, one and zero, then retries a third time.
Final failure clears the temporary kind byte, fills the temporary 37-byte
record with FF and returns FFFF. Success copies the temporary three-byte
kind/index entry and 37-byte record to the replacement slot through
1000:0452. FND-PARTY-119 and FND-COMBAT-078 supply the free-list and
copy/fill contracts and their bounds.

The complete 0D09 eviction helper returns without work when DS:265B is
zero and caps its first argument's low byte at six. It walks placed
entries against the current unsigned DS:264E count, using word-offset
increments. It skips signed slot words below four. Inside all view/window
coordinate bounds, it also skips when its second argument's low byte is
zero; otherwise it reaches the candidate checks. Candidates need a signed
slot word at least 48 decimal and the placed-entry category low three bits
no greater than the capped first argument. No own upper slot bound or
independent extent bound is added here.

Both loader calls supply third argument zero, so the helper's 31E0:05BB
branch is skipped. It instead calls 31E0:0B4A with the candidate slot and
two zero words. A nonzero returned AL replaces the placed entry's slot
word with the negation of the previously saved presentation object number;
zero leaves that word unchanged. The helper advances to the next entry.

FND-PARTY-130/131 already identify 0B4A's kind-two branch: it requests
overlay 193 +1DB0 with the victim slot, then 371D:0002. FND-COMBAT-053/054
describe that owner-effect removal and the additional child routes. Thus
flags zero at the original re-placement call does not by itself exclude
a later owner-removal request through temporary-slot eviction. Reaching
that request still requires a populated saved temporary slot, allocation
failures, valid candidate state and every intervening gate. This reading
does not show a native save or observed load meeting those conditions.
Q-COMBAT-045 retains that admission and remaining transitive effects.

### Ordinary continuation writes and script-chain insertion

For an admitted resulting slot, the parent writes it into the selected
placed entry's word +6. A signed slot below four requests 2D40:0B54.
That complete leaf has no child call: it checks byte +11 of the slot's
19-byte script record for FF and that the slot is not already DS:5AF5.
A passing case makes the slot the chain head, writes its previous/next
link bytes and, for an old head other than FFFF, writes that record's
back-link byte. It has no own slot/head range or cycle check. Its direct
footprint is not an effect-row writer under admitted disjoint storage,
but it can change the chain later visited by script conversion.

The parent writes its object-number argument to presentation word +1B,
converts frame byte FF to zero for an image-selector request, calls
31E0:2670 with the slot, selected local image word, frame and supplied
flags byte, then clears presentation byte-zero mask 20.

The complete 2670 helper checks DS:265B and unsigned slot below 320.
For unchanged flags zero, both flag-one and flag-two child-call branches
are skipped. A zero image argument writes 8000 to presentation word +16;
a nonzero argument writes its word negation. This admitted branch has no
child call. Other flag values have child routes that are not imported into
the flags-zero statement.

For a current kind-two slot with signed presentation word +16 greater
than FFFF, the parent selects a direction from byte +1F of the corresponding
49-byte record, mapping values above seven to zero. It indexes the table
at DS:141C, selects a word in the 72-byte table under DS:6573 and passes
its low byte to 31E0:271E. These table accesses do not independently admit
the supplied record/table extents or aliases.

The complete 271E body has its own DS:265B and unsigned slot bounds and
skips a designated image value. Otherwise it calls 1808 before testing
whether the presentation image word is negative. A nonnegative result
stores frame byte +18, sets byte-zero mask one and copies DS:145E to
dword +1D. Its frame-count comparison has no conditional branch before
the following flag update overwrites flags; it is not a frame-index guard.
An image-specific +21 chain can loop to another slot without repeating
the initial slot bound. Another branch calls 27A8. Existing
FND-EXPLORE-019 covers the lazy-animation context; these reached children,
chain validity and termination still need admission for restored state.

### Placed-entry relinking and return

For signed resulting slot greater than zero and a selected placed entry
with mask 20 set in byte five, the loader sets presentation mask two.
It then walks current DS:264E placed entries: a negative slot word whose
word negation equals the object-number argument and whose byte-five mask
20 is set is replaced with the resulting slot. The cursor advances its
offset by eight at word width, and the global count is reread for loop
tests; this does not validate the table extent or exclude aliases.

Normal completion fills temporary slot 319's presentation record with FF,
clears its kind byte and returns the resulting slot. The invalid-entry
failure path instead fills the indexed current record and returns FFFF.
These endpoint writes and returned slot do not prove rollback or successful
reconstruction of every child.

The small pre-call 25AF:071F helper used for the reader's first four slots
has no child call: unsigned slot below four writes 129 and 99 decimal
to the two words at DS:0528 plus four times slot; other values do nothing.
FND-COMBAT-077 separately bounds the caller's final coordinate helper.

## Interpretation

This removes the flags-zero loader's own remaining branches/stores,
the zero-flags image-selector path, script-chain insertion leaf,
party-coordinate reset leaf and temporary eviction helper's own gates
from unread work under Q-COMBAT-066. It composes the temporary-slot
consequence with existing owner-removal evidence instead of treating
the skipped RDFF branch as a general preservation guarantee.

Valid OJFF source/length and local storage, preserved flags and locals,
actual slot/placed/script/image table backing, allocation/free-list state,
lazy-animation and unload children, callbacks and indirect effects remain.
In particular, a nominal incoming slot range does not validate replacement
allocator values, and a literal absence of an effect operand does not exclude
aliases or transitive mutations. No native eviction, lost effect, visible
defect, complete reading or parity promotion is claimed. Q-COMBAT-045/065/066
retain those independent admission, storage and child-effect obligations.

## Alternatives

- Zero load flags make object reconstruction a call-free presentation copy:
  RDFF is skipped, but allocation/eviction, resource, chain and animation
  paths remain.
- Zero flags in 2670 still load an animation resource there: both of its
  flag-controlled call branches are skipped on that admitted branch.
- Skipping 05BB in the eviction helper excludes all effect removal:
  its other unload call can request the already documented owner removal.
- A populated temporary slot is known reachable in an original-made save:
  no such save or caller-state admission was established by this reading.

## How to reproduce

Resolve the fingerprinted installed DSUN.EXE through tools/game-dir.mjs.
Using the project-local evidence Python with `-I`, run resident_listing.py
with 31E0:0D09..31E0:128C, 31E0:2670..31E0:27A8,
25AF:071F..25AF:0742 and 2D40:0B54..2D40:0BA4. Each span begins at an
inventoried entry; follow the known internal branch targets and all local
returns. The retained original-derived listing is only at
GAME_DIR/analysis/hit-incoming-20261104/restore-object-children.txt.
Reuse FND-COMBAT-077/078 and FND-PARTY-119/130/131 for existing caller,
copy/allocation and unload facts, and FND-COMBAT-053/054 for effect-removal
limits. No original-derived listing or payload is committed or executed.
