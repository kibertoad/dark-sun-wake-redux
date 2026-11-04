---
id: FND-ITEM-058
title: The equipment status query tests effect-four membership and locally preserves captured selector registers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FF85..0x0007FFA6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3143:000D..3143:00A8
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at 055a3620; composition with FND-COMBAT-029/031/032, FND-MAGIC-015 and FND-PARTY-254
environment: null
---

## Observation

FND-PARTY-254's after-equipment hook calls 193 +1C95(owner,4).
FND-MAGIC-015 already reads the general wrapper as an effect-row membership
query. This checks the complete local graph and composes its exact status,
register and boundary contracts with Q-PARTY-109's equipment hook.

### Query and admitted membership

The complete +1C95 body passes its owner's word and the low byte of its
second argument to 3143:0061. It returns AX zero when that result equals
the freshly read count DS:2100, otherwise AX one. The argument four is
therefore effect ID four, not bitmap mask four or item field four.
FND-COMBAT-027 labels ID four Anti Magic Shell.

0061 starts with 3143:000D's owner lower bound. Under a stable count N in
0..192, valid rows sorted by owners 0..47 and an owner in 0..47, that lower
bound is the first matching-or-greater owner index, or N. The search scans
only same-owner rows with index signed below N and compares row byte +6
with the requested low effect byte. It returns the first matching index,
or N when none matches. No duration, count byte, display category, DATA
property or item type is part of this query.

Consequently the after-hook's nonzero status is exactly existence of a
counted effect-four row for that owner in this admitted class. Duplicate
matching rows do not make a result greater than one: the outer wrapper
normalizes to zero or one. A pending expired row still counts until the
row table itself changes; the query does not inspect its duration fields.

### Boundary read and empty count

At every 0061 continuation, including its initial lower-bound result, it
reads the selected row's owner word before checking index against N.
When the index equals N, either owner-comparison branch returns N without
reading the row's effect byte. The boundary word's value therefore need
not be a particular sentinel for this no-match Boolean result, but its
address must be admitted readable storage. The code does not avoid that
read just because no counted row remains.

For N zero, 000D returns zero immediately. 0061 still reads boundary owner
word zero, then returns zero whichever owner value it holds; the wrapper
returns AX zero for stable count zero. For an unsigned owner word at least
48, 000D likewise returns N; with the admitted boundary read and stable N,
the wrapper returns zero. These cases do not establish arbitrary invalid
count or inaccessible-boundary behavior.

With N at most 192 and matching-owner rows admitted, the linear search
increments its index on every failed code comparison, so it ends after
at most the remaining counted owner rows. There is no callee in that
linear loop. The preexisting count/table writer and restoration questions
are not discharged by this local termination bound.

### Registers and own writes

+1C95 does not modify SI or DI itself. 0061 saves/restores SI and does not
write DI; its only callee 000D saves/restores both. There are no resource
loads, callbacks, effect dispatches, interrupt/port operations or persistent
data stores in this complete local call graph. Its writes are confined to
normal stack frames and saved registers. AX and other scratch state are
not asserted preserved.

Thus +326A's captured decremented selector in SI and owner in DI survive
this query under returning execution, intact nonwrapping frames and disjoint
stack/state storage. The query's own instructions do not change the selected
item's W/bonus, DATA key/buffer, effect rows/count or DS:6174. Aliased frames,
external changes and native input/lifetime admission remain separate from
this bounded local write/register result.

### Bitmap equivalence and equipment suppression

For the same owner and unchanged admitted rows, the prepared bitmap of
FND-COMBAT-029 has byte zero mask 10 hex set exactly when the status query
returns one for effect four. This equivalence additionally requires the
bitmap's valid DS/SS binding and bounded effect-ID/write domain, not merely
a near pointer with the same offset. It compares two readings of the same
row membership, not observations at different native times.

For a nonzero item W, +326A tests this status before its special ability
rebuild/reset or ordinary DATA/property/dispatch paths. A matching row
suppresses all of those later local paths and leaves its own DS:6174 writes
unreached. W zero returns before asking the query. The before-equipment
helper +332B does not make this status query: its W=320..327 ability-rebuild
request of FND-ITEM-056 remains a distinct gate/order contract.

Under retained canonical inputs and matching DATA content, FND-ITEM-057's
93 property-one payloads therefore still need absence of the owner's counted
effect-four row to enter the after-hook dispatch path. Its 20 special source
payloads need that same absence to reach their after-hook rebuild/reset.
Source frequencies do not establish whether such a row exists for an actual
selected character or how often either helper is called.

## Interpretation

Q-PARTY-109 no longer treats this status call as an opaque selector-clobbering
or item/cache-mutating child. Its remaining status obligation is actual
owner/row/count/boundary and use-time membership admission. DATA cache/read,
reset/rebuild/dispatch and DS:6174 consumer effects remain their own contracts.
Q-ITEM-003 keeps the related caller, record, frame and placement lifetime
questions. No native effect result, rule status, parity or complete-reading
promotion follows from this conditional composition.

## Alternatives

Testing mask 04 in bitmap byte zero would query a different ID. Treating a
one-past read as requiring a special sentinel value adds a value condition
that neither no-match branch needs; treating it as no read omits a required
storage condition. Counting only positive-duration effects adds fields this
search never reads. Assuming both equipment hooks share the same status gate
would add a call absent from +332B.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for descriptor 193
range 7FF85..7FFA6 hex and resident_listing.py for 3143:000D..00A8. Retained
placement-status-gate.txt includes the first body and a diagnostic adjacent
body; placement-status-search.txt contains both complete resident helpers,
under GAME_DIR/analysis/item-removal-20261104/.

Trace the code-byte argument, index/count comparison and normalized returns.
At index N follow both owner-equality outcomes before the count test. For
register/write scope include both save/restore frames and every direct call,
not only the exported wrapper. Compose bitmap membership with FND-COMBAT-029
and the two equipment-hook branches with FND-PARTY-254/FND-ITEM-056/057.
No original code, bitmap/row fixture, resource payload or image is committed
or executed.
