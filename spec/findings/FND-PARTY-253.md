---
id: FND-PARTY-253
title: Resident door consumers reapply closed-door GMAP strips and suppress the mode-two interaction pointer for an undiscovered secret door
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:023B..25AF:02FA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2608..28C9:2702
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2833..28C9:2839
tool: tools/research/exec-census/resident_listing.py and trampoline_target.py at commit 7ccbb6ac; Python 3.14.7, Capstone 5.0.7
environment: null
---

## Observation

This reads the two resident item-word +0x10 consumers named by
Q-PARTY-020. FND-PARTY-250 supplies the door-range predicate;
FND-PARTY-251 supplies the GMAP strip helper; FND-PARTY-252 supplies
the local classification byte. FND-INPUT-013 already locates the
pointer-image function and its mode selection. No original code ran.

### Closed-door strip reapplication

`25AF:023B` walks placed-slot indices zero through 319 in order. For
metadata kind one it calls door predicate `182 +0C00` with the slot's
object-number word +0x1B. A zero AL advances to the next slot. With
nonzero AL it rereads the metadata record index, addresses its
23-byte record and loads the full word +0x10. Bit 0 clear advances
without the door helper. Bit 0 set calls trampoline `56BD:0093` with
the slot and the complete flag word. That trampoline enters `182 +0FC5`.

Under admitted stable bindings, this invokes FND-PARTY-251's strip
helper with bit 0 set and therefore requests setting GMAP mask 0xC0.
It does not call that helper to clear the strip for bit-0-clear doors
on this path. Neither bit 1 nor bit 3 is independently tested by this
resident door branch, and there is no local partner search or item
flag store. The helper's own kind check and grid-coordinate guards
still apply.

Kind-two slots take a separate branch. It skips a slot when bit 7
or bit 5 of placed-slot byte +0 is set; otherwise it requests
`2D40:32C5` with the slot, the unsigned coordinate words shifted right
four, the far pointer to DS:053C and word 249. Other kinds advance.
That branch's child effect is outside the door-word question, and
no complete occupancy-rebuild claim is made for this function.

### Mode-two map pointer

`28C9:2608` accepts two word arguments and starts with image 19101.
With DS:1440 exactly two it calls `31E0:2DBD` with DS:1408, DS:140A
and those two arguments. A result FFFF selects image 19108.
Otherwise it keeps the returned slot and calls `1B0B:00A4` with two
coordinate pairs from the selected slot and the slot named by
`4C13:0369`, each coordinate shifted right four, and zero for the
two additional words. A nonzero AX selects image 19108.

If that result is zero, it calls the placed-slot classifier
`28C9:28E3`. Nonzero AL passes the classification gate. Zero AL
falls back to `1695:0834(slot)`; this passes only when its returned
word has at least one bit of mask 0x1D. A failed gate selects 19108.
This establishes one actual consumer of the classification's byte:
this path tests any nonzero byte, not specifically its 0x08 or 0x80 bit.

After the gate, metadata other than kind one selects image 19107,
as does a kind-one record index unsigned at least 9999. A kind-one
record below that threshold calls the door predicate with the placed
object-number word. A non-door selects 19107. A door instead rereads
the record index and tests item word +0x10 with mask eight: bit 3
set selects 19108; clear selects 19107. Both branches jump to the
common return at `28C9:2833`, which returns the image word in AX.

Thus a qualifying otherwise-interactive door with the discovery bit
still set uses the same 19108 result as the earlier failed gates.
This is pointer-image selection, not proof that the object cannot
be clicked or otherwise interacted with. Other DS:1440 modes are
outside this bounded branch reading.

## Interpretation

The two resident consumers of word +0x10 now have concrete local
roles: bit 0 drives reapplication of closed-door grid masks, and
bit 3 suppresses the mode-two interaction pointer image on the
qualified door branch. This connects the context-specific bits from
FND-PARTY-250 to map effects and pointer selection without naming the
word globally or implementing unmeasured gameplay.

The mode-two classifier gate answers part of Q-PARTY-108's consumer
question; it does not settle other returned-byte consumers or input
admission. Q-PARTY-107 retains door storage/admission and resource
dependencies. Q-PARTY-020 retains the nonresident consumers.

## Alternatives

- This resident door-grid branch does not clear open-door masks or
  normalize a locked flag into a closed flag: it tests bit 0 alone
  and calls only when that bit is set.
- Image 19108 is not unique evidence of a secret door. Missing target,
  nonzero preliminary helper result and failed classification all
  produce the same image earlier in the branch.
- The pointer gate does not distinguish classifier 0x08 from 0x80.
  A valid-name replacement can change which bit is returned while
  leaving this nonzero gate's outcome unchanged.
- These bodies do not establish the target finder, preliminary helper
  or script fallback's complete semantics; resource/slot/table identity,
  actual pointer rendering and admitted state remain qualified. No
  absence claim covers other direct, computed or aliased consumers.

## How to reproduce

With the locked evidence Python and installed `<dsun>`, use scripts
under `tools/research/exec-census/`:
`resident_listing.py <dsun> 25AF:023B..25AF:02FA`,
`resident_listing.py <dsun> 28C9:2608..28C9:2702 28C9:2833..28C9:2839`,
and `trampoline_target.py <dsun> 56BD:0093`.
The scripts verify the source length and XXH3. Start decoding at
the declared function entries, follow each mode-two conditional
branch to its return and preserve the distinction between an
image-number result and actual image rendering. The whole 25AF
function is read; only the declared mode-two path of 28C9 is covered.
