---
id: FND-PARTY-315
title: Stack-limit literal writers distinguish allocation state from driver fields and object cleanup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0F46..1000:0FA5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:06B6..15F3:0851
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:0AFB..409B:0C23
tool: window_operands.py, direct_callers.py and resident_listing.py at 84d8d939; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; reuse FND-EXE-397/509/516/517/518/526 and FND-CONFIG-172
environment: null
---

## Observation

Q-PARTY-136 needs actual stack and saved-word backing, not an assumed
permanent stack floor. Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets below are hexadecimal.

The controlled operand window 0099..009E includes displacements that
could overlap either byte of word 009C for operands up to four bytes.
It reports 302 operands in inventoried instruction ranges and 1217
additional candidates outside their instruction starts. Five established
operands carry write access, counting read-and-write access as a write:

| Site | Classification from existing complete local readings |
|---|---|
| 1000:0F56 | FND-EXE-517's setter replaces current DS:009C with its argument after its unsigned margin test. |
| 1000:0F88 | FND-EXE-516's exchange publishes the computed allocation offset and returns the old value. |
| 15F3:06D1 | FND-EXE-397's driver saves DS and selects DS from CS before this word store. |
| 15F3:078A | The second driver body uses the same DS-from-CS binding. |
| 409B:0B98 | FND-CONFIG-172's EBOX cleanup clears the supplied object's far field +9C after releasing it. This is indexed ES storage, not an established game DS:009C store. |

The independently read setter and exchange are positive controls for
write and read-and-write classification. The exchange is present as
read+write; filtering only the exact access word `write` would omit it.
Under the modeled game DS 57E0 and driver CS 15F3, the two driver
stores have different backing from game DS:009C. Alternate segment
bindings and the object's actual pointer still require admission; matching
the displacement alone neither proves nor excludes a backing alias.

The setter's direct near-call search recovers the known wrapper call
0F9F and sites 20F5/22A2. FND-EXE-518/526 already classify the
latter callers: selected block/link offsets and a metadata-derived sum
reach the setter, with metadata changes preceding possible rejection.
Their input blocks and extents remain conditional. These are not new
unread setter callers, and their recorded rejection paths do not undo
earlier metadata changes. Other transfer forms remain outside this search.

FND-EXE-517's shipped A4EC seed exceeds the cursor copy's
9B86..9BCF region and number word at 9BE2. It is not a permanent
lower bound: the setter has no lower-bound or monotonicity check.
FND-EXE-516's general high-word arithmetic also cannot be replaced
with a blanket monotonic-allocation assumption. FND-EXE-509 separately
requires native preservation for startup's DS and later SS producers.
Thus the numerical seed alone cannot exclude overlap with the named
SS saved words or establish their lifetime.

## Interpretation

The established literal-window writers are classified. The remaining
stack-limit obligation is their actual setter/allocation inputs, object
and segment backing, and mechanisms excluded by this operand census,
rather than unspecified meanings for these five instructions. This
narrows Q-PARTY-136 without supplying the retained-number contract.
FMT-PARTY-007 stays supported; no complete reading is declared.

## Alternatives

- Every 009C displacement changes the game's allocation word: the driver
  selects its own segment, while cleanup uses a supplied object pointer.
- The shipped seed permanently separates local buffers and saved frames:
  an admitted setter can replace it with a smaller offset.
- No other writer exists: displacement-free, adjusted-pointer, string,
  wider, aliased, indirect and alternate-stream writes remain outside the
  classified established set. Raw candidates are not instructions proved
  reachable, and have not been dismissed as a group.

## How to reproduce

Run tools/research/exec-census/window_operands.py with installed DSUN.EXE,
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv and window 0099..009E.
Include both write access labels. Check the setter and exchange controls
against resident_listing.py range 1000:0F46..1000:0FA5. Reuse
FND-EXE-397 and FND-CONFIG-172 for segment/object interpretation.
Run direct_callers.py for 1000:0F46 and 1000:0F99, preserving its
excluded transfer forms; reuse FND-EXE-518/526 for the two other sites.

Retained reports are getter-stack-limit-writers.txt,
getter-stack-limit-runtime.txt and getter-stack-limit-setter-callers.txt
under GAME_DIR/analysis/handler-census-20261106/. A separate exploratory
getter-stack-segment-writers.txt is an inventory-range operand list, not
a validated whole-program SS census or evidence of actual segment values.
No original game, emulator session or emulated call supplied evidence here.
