---
id: FND-COMBAT-111
title: The complete numbered reader has no own effect-row validation after its transfer loop
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x00070B4A
tool: tools/research/exec-census/overlay_listing.py at d4aa17fe; locked evidence Python and Capstone
environment: null
---

## Observation

Q-COMBAT-018 retains checks and postprocessing of restored effect rows and
count. FND-COMBAT-035 reads the transfer loop; FND-SAVE-022 describes the
continuation. This reading accounts for the complete enclosing body,
overlay 187 +0A6C..+0D1A, specifically for that remaining local validation
question. The exclusive file end 70B4A follows the far return at 70B49.
Offsets below are hexadecimal.

### The transfer flag gates reconstruction, not an effect validator

After the numbered loop, +0B34 tests its saved success byte BP-1. Nonzero
calls +1CEE, testing its returned AL and calling the diagnostic on zero.
This does not assign that result to BP-1. The following shared path
conditionally releases DS:55B8 and replaces that pointer with returned
DX:AX, then copies 4C13:035B to DS:19B8. Diagnostic termination and
external release effects remain prerequisites to reaching later code.

At +0B78 the body tests BP-1 again. Zero skips the placed-entry recount
and object reconstruction, reaching +0CB7. Nonzero clears DS:264E and
counts eight-byte placed entries while their byte-five bit three remains
set. It advances the local far pointer's offset by eight at word width;
the loop supplies no independent extent bound. DS:264E is the placed
entry count, not the effect count DS:2100.

The following loop visits 320 object slots. For slots below four it
calls 25AF:071F. A slot whose word at record offset one equals FFFF
skips reconstruction. Other slots save seven presentation fields and
call 31E0:0EFF with flags zero; an FFFF result skips restoration of
those fields and the coordinate call. Otherwise the body restores the
fields and calls 31E0:444A. FND-COMBAT-077 already reads these arguments,
the skipped RDFF branch and the coordinate helper's two own writes;
reuse that finding rather than treating this loop as effect processing.

### Shared ending after either transfer result

At +0CB7 both paths pass the low byte BP-2 as part of an outgoing word
to +1829. The earlier selector-zero branch sets that byte to one;
otherwise it remains zero. The AL load alone does not normalize AH.
FND-SAVE-023 describes the script-entry conversion helper; it is not
an effect-row validator inferred from its position after restoration.

The body next clears word +0C of each of 400 twenty-three-byte records
through the current far pointer DS:19C1, reloading that pointer each
iteration. It then calls 2D40:2196 with current DS:1A32, word 40 and
segment 5072. With selector zero it assigns DS:1A34 to zero or FFFF
from byte 4F49:0C33 plus three times current DS:1A32. Both paths end
by loading AL from BP-1 and restoring the saved registers and frame.

Across the complete body, there is no own read of DS:2100, traversal of
the ten-byte effect table, comparison of effect IDs, owner-order check
or count clamp. The resource request loop's comparisons concern pointer
identity, null destinations, reader results, index and loop bound.
The subsequent loops concern placed entries, object slots and item
records. These statements classify this body's decoded operands and
branches; they do not establish nonaliasing or absence of child effects.

## Interpretation

Q-COMBAT-018's own-body postprocessing assessment is complete for this
reader: the continuation does not supply the missing explicit effect
validator. This removes rereading the remainder of +0A6C as an unread
local obligation. It does not close the item or establish restored state.
The already-read caller continuations are FND-COMBAT-043/044.

Remaining admission includes the selected archive and fallback resource,
destination extents and aliases, earlier transfers that can change state,
and effects of the actual child routes: allocation and diagnostics before
the loop, map reconstruction, release, object initialization, script-entry
conversion and item enumeration. In particular, valid disjoint storage
and stable saved locals cannot be inferred from the lack of an explicit
effect operand. A returning diagnostic does not establish an error-free
load, and BP-1 alone does not combine every later child's result.
No native save was loaded and no evidence status or parity row changes.

## Alternatives

- Counting placed entries does not validate the effect count.
- A bounded object-slot loop does not bound the earlier placed-entry walk.
- Postprocessing still runs in part after a failed transfer loop.
- Returning the transfer flag does not prove every reconstruction succeeded.
- The absence of an own named-field check is not a global writer census.

## How to reproduce

Resolve the installed DSUN.EXE through tools/game-dir.mjs. With the locked
evidence Python run overlay_listing.py with descriptor 187 and half-open
file bounds 7089C 70B4A. The reader verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Start decoding at the inventoried entry,
follow BP-1 separately from BP-2, and account for both success tests and
the shared +0CB7 ending. Compare FND-COMBAT-035/077, FND-SAVE-022/023
and FND-PARTY-236 for existing transfer, reconstruction and item-write
contracts. Keep the listing only in the approved local store:
GAME_DIR/analysis/hit-incoming-20261104/restore-full-reader.txt.
No original-derived listing or resource content is committed.
