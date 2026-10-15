---
id: FND-COMBAT-047
title: Guard and wait make distinct local stores before a resolver-gated zero-or-mask update
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D909..0x0005D98B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D98B..0x0005D9E7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3E64..2D40:3EBF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A54F..0x0006A58B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079A07..0x00079A58
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079AA3..0x00079AFD
tool: tools/research/exec-census/trampoline_target.py, overlay_listing.py, resident_listing.py and direct_callers.py; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

The hash-checked trampoline resolver identifies 5671:0093 as descriptor
173 +2FE9, file 5D909 hexadecimal, and 5671:0098 as +306B, file 5D98B.
The committed inventory independently ends their contiguous bodies at
5D98B and 5D9E7 respectively. Both entries return far.

### Action one and action two

The first entry receives a slot word at SS:BP+6 and an action word at
SS:BP+8. Before testing the action, it reads the word at
4F49:0C34 plus three times the slot into SI. This is the mapping called
object_slot_combatants in FND-EXPLORE-002. Index arithmetic is 16-bit;
this entry has no local slot, type or mapping-bound check.

| Action word | Local work before the shared helper | Shared-helper mask |
|---|---|---|
| 1 | Store byte one at 4E71:0B1A plus SI. | Zero. |
| 2 | Read the word at 4C4F:00DC plus four times SI using signed comparisons. If greater than ten, store ten. Otherwise, if greater than one, decrement it. Otherwise leave it unchanged. | 10 hexadecimal, decimal 16. |
| Anything else | No state store or child call after the initial mapping read. | No call. |

Thus action two preserves negative words, zero and one locally. Two
becomes one, ten becomes nine, and eleven becomes ten. The byte store
on action one and the conditional word store on action two precede the
shared helper; there is no local rollback after it.

The helper call uses a pushed CS and a near call to make the far-return
frame its callee expects. The caller pushes mask then slot, and removes
four argument bytes after return. No preceding child call can replace
these two argument words within this entry. The original slot remains
in DX until it is pushed; the mapping-derived SI is not passed as slot.

### The shared update is conditional on both resolver indexes

At +306B the second argument is saved in SI as a word mask. The entry
passes the original slot and two explicit SS-relative far pointers to
2D40:3E64. Its first index output is the local at BP-2; its second is
at BP-4. The ten bytes of resolver arguments are removed after return.
Only AL is tested, and zero returns without this helper's state stores.

The resolver reading agrees with FND-EXPLORE-002: it rejects unsigned
slot values at least 9999 and mapping-type bytes other than two; it
writes the first output before rejecting first indexes at least 42,
and writes the second before rejecting second indexes at least 17.
Consequently a nonzero AL here requires both index tests, even though
the shared update uses only the first index. Failure does not imply
that neither output local was written. Its only stores are through
these two supplied far pointers, as FND-PARTY-130 also records.

On nonzero AL, let index be the word in BP-2:

- With zero mask, first store FFFF at 4C4F:00DC plus four times index,
  then store zero at 4C4F:01D8 plus twice index.
- With nonzero mask, OR the full mask word into 4C4F:01D8 plus twice
  index. That branch does not store at the 00DC-based location.

There are no other child calls in either complete local body. The
action entry does not interpret the shared helper's return registers.
For action one, resolver failure leaves the earlier byte store in place;
for action two it leaves any earlier adjustment in place. This is a
local sequencing statement, conditional on admitted segment and storage
identity, not a native failure experiment.

### Known command callers distinguish the three requests

The menu continuation already reached in FND-COMBAT-026 compares its
returned choice with one, two and three. Choices one and two pass
DS:426D and action one or two to 5671:0093. Choice three passes DS:426D
and zero directly to 5671:0098, bypassing the action entry's initial
mapping read and byte store. The caller removes four argument bytes on
each route, then executes its common postprocessing.

The G handler of FND-COMBAT-025 pushes action one and jumps to the
common slot-push/call sequence. Its W handler pushes action two before
that same sequence. Both load the slot after their message calls;
admission and preservation across those earlier calls remain separate.

The direct_callers query returns other byte-pattern candidates,
including an in-overlay call to +2FE9 and many calls to +306B. This
finding qualifies only the local helper-to-helper call and known menu
and keyboard continuations above. It makes no exhaustive caller claim;
computed calls, jumps, other raw candidates and upstream paths remain.

## Interpretation

This answers the local action-one/action-two/shared-update branches of
Q-COMBAT-006. Guard and END TURN both request a zero-mask update, but
only Guard first writes the mapping-indexed byte. Wait adjusts a signed
word and requests a mask OR instead of the zero-mask stores.

The meanings and lifetimes of these three state locations require their
producers and later consumers. In particular this finding does not call
the adjusted word a time, initiative, action-point or remaining-move
quantity, or infer a turn scheduler from its constants. Native command
effects and complete admitted caller/storage contracts remain open.

## Alternatives

- A single identical action for Guard and END TURN omits Guard's earlier
  byte store and unchecked mapping read.
- Treating Wait as always subtracting one contradicts its signed clamp
  and its unchanged values at or below one.
- Resolver rejection is not rollback of the action entry, and it can
  still write one or both output locals before returning zero.
- The first mapping read alone does not admit the later resolver: its
  type byte and second-index test impose additional conditions.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, after Bootstrap-Project.ps1
-ValidateFactsOnly. The readers verify this identity themselves.
Resolve 5671:0093 and 5671:0098 with trampoline_target.py.
Use overlay_listing.py descriptor 173, 5D909..5D9E7; descriptor 182,
6A54F..6A58B; descriptor 190, 79A07..79A58 and 79AA3..79AFD.
All file bounds are hexadecimal and half-open. Read resident_listing.py
2D40:3E64..2D40:3EBF with its declared MZ relocations.

Run direct_callers.py targets 173+2FE9 and 173+306B as a separate raw
candidate cross-check. The helper's call at file 5D982 and menu calls
at 6A566 and 6A57A are independent positive controls. No absence or
exhaustive-caller conclusion depends on that query. Inspect the signed
JLE branches, ES selections, SS output pointers, AL-only result test,
four- and ten-byte cleanups and store order. Reports stay locally under
GAME_DIR/analysis/combat-actions/. No original function was executed.
