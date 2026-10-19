---
id: FND-PARTY-254
title: Overlay 189 dispatches a one-based item effect selector and its second helper cannot reach the call after an unsigned FFFF bound
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077C1A..0x00077D26
tool: tools/research/exec-census/overlay_listing.py at commit c8246483; Python 3.14.7, Capstone 5.0.7
environment: null
---

## Observation

This reads the complete inventoried bodies `189 +326A` and `+332B`
in installed DSUN.EXE. FND-PARTY-121 already identifies the former's
item-effect argument; this extends that reading to its complete local
dispatch gates and the separate second helper. FND-PARTY-096 identifies
`2C5F:0003` as the DATA request, FND-PARTY-142 describes the ability
rebuild `197 +1E7B`, and FND-PARTY-248 describes reset `190 +365A`.
No original code was executed.

### First helper

`189 +326A` takes an owner word and an item-record index word. It
initializes a local byte to zero and reads word +0x10 from the indexed
23-byte record. Zero returns. Otherwise it subtracts one at 16-bit
width, then requests `193 +1C95(owner, 4)`. Any nonzero AX returns.

A zero result continues to a signed comparison of the decremented
word with 319. At least 319 requests `197 +1E7B(owner)` and then
`190 +365A(item index)`, in that order, before returning. Under the
local arithmetic this branch corresponds to source words 320 through
32768. Larger source words produce a negative signed decremented
value and instead enter the lower branch; there is no local unsigned
upper bound rejecting them before DATA access.

The lower branch asks `2C5F:0003` for the decremented selector and
reads the low two bits of byte zero through its returned far pointer.
A value one enters the effect request path. Otherwise it returns
unless the item's signed bonus byte +0x16 is negative; for a negative
bonus it requests the same DATA pointer again, and only low-two-bit
value zero enters the effect request path. Other values return.
The second DATA request and repeated read remain separate operations.

The effect path first calls reset `190 +365A` with the item index.
After return it sets DS:6174 to one and requests `193 +2290` with
owner as both first object arguments, the decremented selector,
word six, two zero doublewords, the SS-based far pointer to the local
byte, word one and word zero. It then clears DS:6174 and returns.
The flag is assigned one and then zero; it does not save and restore
the incoming flag value. Reset and flag publication precede the
dispatch, and dispatch return precedes clearing. No successful effect
or storage preservation is inferred from making those calls.

### Second helper and unsigned return

`189 +332B` also takes an owner and item index, and reads word +0x10.
Zero returns. It decrements a nonzero word and then reconstructs
its original 16-bit value for two unsigned comparisons. An original
word from 320 through 327 inclusive calls `197 +1E7B(owner)` and
returns. Other values reach `+335F`.

At `+335F` it compares the decremented word with FFFF and branches
unsigned-below-or-equal to its return. Every 16-bit value satisfies
that comparison. No call or flag-changing instruction lies between
the comparison and branch. Thus ordinary local execution never reaches
the subsequent unsigned bound of 328 or the encoded call to
`197 +38C7`. This is a complete local branch result, including values
whose signed interpretation is negative. Treating the condition as
a signed comparison with -1 would give a different, incorrect result.

Neither helper writes word +0x10 itself. There is no independent item
index-capacity check in these bodies.

## Interpretation

These two consumers use item word +0x10 as a one-based effect selector,
not as a universal flags word. The first's DATA and dispatch behavior
depends on its status gate, signed threshold, DATA properties and bonus.
The second's local reachable behavior is restricted to its zero return
and the 320..327 ability-rebuild case. The code after the FFFF comparison
does not prove another reachable item effect.

This answers the two overlay-189 consumers in Q-PARTY-020. Admitted
caller inputs, DATA-pointer validity, effects of status/reset/dispatch
children and DS:6174 consumers remain Q-PARTY-109. The statically
unreachable call is not by itself a documented gameplay defect or
evidence that another entry cannot reach its target.

## Alternatives

- The first helper's 319 bound is signed, while the second helper's
  320..327 bounds and FFFF comparison are unsigned. They cannot be
  replaced by a shared signed range rule.
- The second helper's later call is not reachable merely because
  its bytes decode. All ordinary paths from the established entry
  return before it; independently entering those bytes is not a path
  established by these bodies.
- No claim covers complete caller census, native item/index domains,
  far-pointer validity or state preservation across the child calls.
  In particular the high source-word branch of the first helper
  supplies no admitted negative DATA selector by itself.

## How to reproduce

With locked evidence Python and installed `<dsun>`, run
`tools/research/exec-census/overlay_listing.py <dsun> 189 77C1A 77D26`.
The tool checks source length and XXH3. Compare the two complete
inventory ranges beginning at 77C1A and 77CDB. Follow zero/nonzero,
the status result, signed threshold, both DATA-property paths and
the negative-bonus gate. For the second body, check the unsigned
branch at `+3362` against 16-bit minimum and maximum values and
retain the distinction between encoded and reached calls.
