---
id: FND-CONFIG-241
title: Driver reset clears table bindings while ordinary shutdown dispatches through retained slots
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:06CF..4842:077F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:084A..4842:08B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0C2B..4842:0C5E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:02BC..4734:02C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:067A..4734:068C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:082C..4734:083B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067681..0x00067693
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000679EE..0x00067A06
tool: direct_callers.py, far_pointer_words.py, resident_listing.py and overlay_listing.py at 67876b26; pinned evidence Python, Capstone 5.0.9 and xxhash 4.0.1; reused FND-CONFIG-236/239/240 and FND-EXE-646
environment: null
---

## Observation

Q-CONFIG-014 retains actual driver table lifetime after FND-CONFIG-240's
publication/provider/result reading. This connected reset/shutdown reading
identifies the ordinary whole-array clear and separates it from the
standalone removal routine and the shutdown dispatch. It does not infer
native preservation from a negative caller search. Addresses, offsets and
selectors below are hexadecimal; counts are decimal.

### Reset is a bounded whole-array producer

4842:06CF saves flags, disables maskable interrupts and clears CS:0002,
0004 and the DIGPAK registration word at 0E22. It explicitly clears the
direction flag and sets ES to CS. Its first repeated word store writes
32 zero words at 0126..0166: exactly the sixteen four-byte table pointers
used by lookup. The next writes sixteen FFFF words at 0166..0186, and
the last writes sixteen zero words at 0186..01A6. These are adjacent,
separate arrays, not three alternative decodings of the table pointers.

The reset has no software child apart from its saved-flags restoration
through the interior IRET at 0710. Under intact code and disjoint ordinary
frame/backing conditions, the three counts are fixed and the stores end
at the stated boundaries. No timer, driver image or DOS call is needed
for that local clearing. Actual backing/alias admission, other entry
mechanisms and intervening writes remain conditions.

FND-CONFIG-240's music initialization caller invokes this reset at
4734:02BC and publishes phase DS:33EE = 2 immediately after ordinary
return, before its enabled music gate and later installation. The other
controlled direct site is overlay 180 +04AE. Its preceding local branch
takes reset when DS:13F7 is zero; the nonzero branch instead calls the
overlay helper at +0026. That helper is a distinct dependency, not an
implicit invocation of this reset.

### Shutdown does not locally perform the reset

4842:071A walks all sixteen slot indices using CS:01AA as scratch. For
an occupied table pointer it reads the associated word at CS:0166 and,
unless FFFF, calls timer release 084A. It then passes its caller-supplied
four-byte argument and the slot to 0C2B. After the walk it calls 0893,
which attempts timer release for indices fifteen down to zero.

0C2B rejects an unsigned slot at least sixteen. For a valid slot it
exchanges the word at CS:0186 + twice the slot with zero. If the previous
word was zero, it returns without driver dispatch. Otherwise it may
release the associated non-FFFF timer and tail-dispatches selector 68
through the ordinary lookup/dispatch chain. The caller's argument remains
on that outgoing frame; no argument value is inferred from the selector.

Neither 071A nor 0C2B directly clears CS:0126's table pointers or calls
the standalone slot-removal body at 0B06. The known driver selector-68
targets, timer descendants and their actual frame/state/alias effects
remain obligations before extending this local statement to full
preservation. The outer loop also needs its scratch slot to survive
those children; a literal bound comparison does not prove termination
when a child can change the scratch word.

084A treats FFFF or a zero selected timer-state word as an immediate
skip. Otherwise it clears that state, decrements CS:0002 and, when the
count becomes zero, calls 059C with zero and then 056C. The inspected
body contains no direct driver-table clear. This finding does not model
those timer children, native timer state or arbitrary timer-index backing.

The ordinary game teardown invokes 071A with a zero four-byte argument
when signed phase DS:33EE is at least two (4734:067A..068C). Its other
local cleanup helper 4734:082C passes DS:343C to timer release 084A,
not to driver removal. FND-CONFIG-240 records the teardown's later game
slot-word stores separately. Overlay 180 +081E also passes zero to 071A
when DS:13F7 is not one; the equal-one arm calls overlay helper +0722.
These gates have different zero/one tests and cannot be merged into one
generic enabled condition.

### Controlled direct-entry census

The unchanged DSUN.EXE source is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Existing direct_callers.py searches
relocation/fixup-backed far-call patterns in the resident image and all
overlay code, plus near-call patterns in each target's resident segment.
It reports two reset sites, two shutdown sites and no 0B06 call site.
The positive installer control returns both sites already instruction-
checked in FND-CONFIG-240. Both reset/shutdown pairs were checked against
their local caller instructions and overlay fixups.

far_pointer_words.py independently classifies every occurrence of each
queried offset word. For 0B06 it reports seventy candidates, none a
relocated/fixed-up pointer, immediate holder or far-call/jump classification.
The reset and shutdown queries positively recover all four known far-call
words. The incidental decodings of the other candidates do not establish
instructions or callers.

These finite searches leave computed/copied pointers, indirect or return-
based transfers, near jumps, equivalent segmented representations, offset
arithmetic, alternate/interior entries and code outside this source
unclassified. They cannot establish that removal never runs. The result
removes an unsupported assumption that ordinary shutdown directly calls
the removal body; it does not supply a universal negative lifetime claim.

## Interpretation

Q-CONFIG-014 can reuse the reset's complete local array writes, both
ordinary direct reset/shutdown caller pairs and shutdown's conditional
selector-68 route. It no longer needs another reading of these local
clear/dispatch instructions. Actual reset/caller/backing admission and
intervening writer history remain, together with alternative overlay
helper effects, selector-68 bindings and reached driver/timer effects,
scratch/frame preservation and excluded removal entry mechanisms.

With an admitted completed reset and no intervening publication or alias,
lookup sees a null slot and follows FND-CONFIG-239's zero result. An actual
getter still has its earlier configuration/sequence gates from
FND-CONFIG-236. No actual post-reset getter, native shutdown, continuous
table lifetime or sound result was observed. RULE-CONFIG-003 remains
supported; no complete_reading or queue closure follows. No game process
or emulated function ran.

## Alternatives

- Only single-slot removal clears the array: the reset's first repeated
  store clears all sixteen table pointers.
- Shutdown and reset have the same local table effect: shutdown can invoke
  selector 68, whereas reset directly clears the array without that dispatch.
- The game cleanup's DS:343C argument is a driver slot being removed:
  its direct callee is timer release, a distinct indexed state array.
- No direct removal caller proves removal is unreachable: the controlled
  census excludes several explicit entry and pointer mechanisms.

## How to reproduce

Under GAME_DIR/analysis/slot-lifecycle-20261108/, retain direct-callers.txt
from direct_callers.py with targets 4842:0B06, 4842:06CF, 4842:071A
and positive 4842:0A23. Retain pointer-words.txt from far_pointer_words.py
for the first three targets. Require the known positive sites and verify
the source size/hash and both target segment mappings before reusing
either report. Do not treat nearby candidate decoding as an instruction.

Use resident_listing.py for 4842:06CF..077F, 4842:084A..08B1,
4842:0C2B..0C5E and 4734:082C..083B. The retained reset-cleanup.txt
and shutdown-children.txt windows also include following routines; the
return boundaries above delimit the claims. Reuse FND-CONFIG-240's
producer and teardown reports for the game callers.

Use overlay_listing.py for descriptor 180, file ranges 67681..67693 and
679EE..67A06; reset-gate.txt and shutdown-gate.txt retain the controlled
gates. Earlier broader diagnostic windows are not alternative entries;
one began inside a prior instruction and is not used for its prefix.
Read the actual targets and interior flags-restoration byte as in
FND-CONFIG-237/240. All original-derived outputs remain in GAME_DIR.
No new reporter was introduced.
