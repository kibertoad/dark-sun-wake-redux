---
id: FND-SCRIPT-033
title: The object-context setter repeats a negative selector until its cursor reaches 320
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:06E6..2D40:0747
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0D94..2D40:0E1D
tool: resident_listing.py at 700bce0a with MZ relocations; locked evidence Python; FND-CONFIG-141 for the selector contract
environment: null
---

## Observation

Q-SCRIPT-002 retains the behavior of 2D40:06E6 when an input identifies
several slots. FND-SCRIPT-028 and FND-PARTY-241 record its coordinate
stores; FND-CONFIG-141 records its selector callee. This reading includes
the setter's full 97-byte inventory body, especially the loop after those
stores, rather than ending at its branch target 073E.

The setter initializes its SS-local cursor word to zero at 06EC. At
06F1..06FE it calls 2D40:0D94 with the original input word and a far
pointer to that cursor. Returned AX equal to 9999 skips all three context
stores and goes to 073E. Every other result is copied to CX and used
to select a 37-byte placed-object record. It reads the words at DS:67C5
and DS:67C7 plus 37 times that result, logically shifts each right four
at word width and writes them to 4C10:001B and 4C10:001D. It then writes
zero to 4C10:001F. The multiplication and addressing use word arithmetic;
there is no separate returned-slot bound or kind check in this setter.

At 073E it compares the updated cursor with 320 as signed words. A cursor
less than 320 branches back to 06F1. Otherwise it leaves its frame and
far-returns at 0746. The selector is its only call site, and there is no
local early return between the context stores and this cursor test.

FND-CONFIG-141's selector has these relevant branches, confirmed through
its inventory-defined return at 0E1C:

- A signed-nonnegative input is returned directly and assigns cursor 320.
  This includes 9999, whose result the setter skips. It does not validate
  another nonnegative input as an actual allocated slot.
- A negative input scans from the supplied cursor while the index is signed
  less than 320. It skips zero kind bytes in 4F49:0C33 plus three times the
  index; otherwise it calls 1AA0:0009(index, 0) and compares returned AX
  with the original input. A match returns the index and writes index plus
  one to the cursor. No match returns 9999 and writes cursor 320.

Thus, under the selector's returning and preservation conditions, the setter
repeats for negative-input matches in increasing slot order. A match at 319
sets cursor 320 and exits after its stores. A last match below 319 is followed
by a terminating unsuccessful scan, whose local setter branch performs no
context store. No match performs no direct store to these three fields.

## Interpretation

For stable valid records and selector inputs, a negative value matching
several active slots produces one local coordinate assignment per match.
The highest matching slot supplies the last such assignment. The setter
does not stop at the first match or return a list of contexts. A direct
nonnegative slot input takes one selector invocation. Input 9999 leaves
the three fields unchanged by the setter's own instructions.

These are local assignment and loop results. Actual field preservation
through getters, metadata loading, aliasing, slot lifetime and every caller
remain separate; this does not claim the final live globals are immutable
after the last local assignment or establish a player's observed trigger.

The field-table producer is also already known rather than an unanswered
writer search: FND-PARTY-051 and FND-CONFIG-150 identify the initializer's
FNFO/1 load into DS:5AF7, the kind-1 base of the 196-byte rows selected
relative to DS:5A33. FND-CONFIG-153 supplies the source spans and sizes;
FND-PARTY-216 gives kind-1 field zero's offset zero and metadata nine.
Those bindings do not name field zero's meaning for every kind or prove
that the table has been successfully loaded and preserved at every use.

## Alternatives

- Treating 073E as the end of the body misses its cursor test and backward
  branch. The inventory ends at 0747, after the far return.
- Treating a negative selector as a single object omits the repeated calls
  with the updated cursor. Calling its result an unconditional live last-match
  value would omit the child and shared-state preservation conditions.
- Treating the unchanged three fields on the no-match local path as a zero
  result ignores that the setter has no default coordinate-clear block.
- The known FNFO load removes the table-producer identity gap. It does not
  establish the semantic identity of field zero (Q-SCRIPT-002) or the use
  of 4C10:001F (Q-SCRIPT-021).

No status promotion, complete-reading declaration, live allocation result or
original-game run is claimed.

## How to reproduce

Use installed DSUN.EXE, size 634416 and XXH3
e296af55ba2ecde7e77f555c90f33d0b. With the locked evidence Python run
resident_listing.py on 2D40:06E6..2D40:0747 and
2D40:0D94..2D40:0E1D. These are the committed inventory's complete
97-byte and 137-byte bodies respectively. Use actual MZ relocation operands
for the loaded 4C10, 4F49 and 1AA0 segments.

Trace the setter's single cursor initialization, both outcomes of its 9999
test, all three stores and the signed cursor branch at 0743. In the selector
keep the initial signed input gate, zero-kind skip, field-zero getter, AX
comparison, match cursor increment and no-match cursor 320 distinct.
Check no match, one match before 319, multiple matches, match at 319,
nonnegative direct input and 9999 paths against those local branches.
Do not substitute synthetic return values for admitted native getter state.

Reuse FND-CONFIG-140/141/150/153, FND-PARTY-051/216/241 and
FND-SCRIPT-028 for their separate accessor, metadata, table and caller
claims. Retained bounded listings are context-setter.txt and selector.txt
under GAME_DIR/analysis/trigger-object-context-20261103/. The preliminary
setter range ended at 073E and was replaced with the full inventory range.
No original code or listing is committed.
