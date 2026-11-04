---
id: FND-ITEM-036
title: Only canonical type 50 lacks the other type predicates that force a nonzero Look classification
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2608..28C9:2839
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:28E3..28C9:2A25
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00063D1B..0x00063DF0
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001F291..0x0001F88B
tool: resident_listing.py and look_property_domain.py at c5a396a9; Python 3.12.10; Capstone 5.0.7; xxhash 4.0.1; composition with FND-PARTY-252/253 and FND-INPUT-013
environment: null
---

## Observation

Q-ITEM-005 retains semantic consumer coverage and Q-PARTY-108 retains
the downstream effects of the property classifier. FND-PARTY-252 gives
the helper/caller predicates; FND-PARTY-253 reads Look's nonzero gate.
This reading covers the remaining pointer-selector body and derives a
complete canonical type-domain restriction on the item word +10 hex test.

### Scope and conditions

The input is a placed item whose metadata remains kind one and whose
attached-record index is valid. Its type index is in 0..101 and selects
the unchanged canonical ALL/4 table. Record/table/stack spans are valid,
nonwrapping and physically separate; the cache store and intervening
returning callees preserve the inputs and local results used below.
Type, item and slot identity remain stable through each reread. These
are admission conditions, not observations of a native Look action.

### The pointer body consumes the classifier only as nonzero

The established 561-byte body 28C9:2608..2839 has one own call to
28C9:28E3, at +2687 on the mode-two path. Its +268D/+268F continuation
tests AL as any nonzero byte. A zero result calls 1695:0834 and tests
the returned WORD with mask 1D hex. FND-PARTY-253's subsequent kind/door
and secret-door tests remain distinct from that classification gate.

The complete remaining mode-four and mode-one branches make no own
call to that classifier. They select their image words through target,
preliminary-result, range/equipment or cell predicates. Other modes
retain default image 19101. This is an own-body statement, not a global
consumer census or exclusion of transitive calls inside those children.
The positive control is the actual reached own call at +2687, decoded
from the established entry, rather than an unverified operand candidate.

### Sufficient type predicates

Let B0, B4 and B11 be shared type bytes zero, four and eleven. Under
the conditions above, any of the following makes the caller's returned
classification nonzero regardless of item word +10:

| Predicate | Why it suffices |
|---|---|
| B0 has any bit of 13 hex | The helper assigns 08; its later name replacement is 80, also nonzero. |
| B11 has bit 80 hex | The helper likewise assigns 08 before the nonzero name replacement. |
| B11 has bit 08 hex | The caller ORs 40 into its result. |
| B4 has bit 20 hex clear | The caller ORs 01 before its cache branch. |
| B11 has bit 01 hex | The door-result branches either OR 10 or replace the result with 10; both are nonzero. |

No later stable kind-one branch clears that result. In particular, a
replacement can change individual bits while preserving nonzeroness.
Thus only rows satisfying all three conditions can expose an independent
zero/nonzero contribution from item word +10:

- B4 bit 20 hex is set.
- B0 mask 13 hex is zero.
- B11 mask 89 hex is zero.

The complete fingerprint-checked 102-row census returns exactly index
50. The other 101 rows have at least one sufficient type predicate.
Independent synthetic controls admit a candidate row, eliminate it with
each sufficient bit and retain it with unrelated bits; joint-mask and
wrong-extent controls also pass. This is not an assumption that those
unrelated bits have no other consumer meaning.

### What row 50 still requires

For row 50, a zero classifier result also requires item value word +06
at most one, name-number word +14 unsigned at least 9999, word +10
zero, and either zero quantity word +02 or type-byte-eleven bit 02 clear.
Otherwise another helper predicate already supplies a nonzero result.

With those other predicates false and intact storage, changing word +10
from zero to a nonzero word changes the classifier from zero to 08.
It changes the Look gate only when the earlier target/preliminary tests
pass and the zero-result fallback's word mask 1D is zero. It changes
the final selected pointer from 19108 to 19107 only when the later door
checks do not independently suppress it: the target is not a qualifying
secret door with word +10 bit 08 set. A nonzero classifier alone does
not prove a different final image or an allowed interaction.

Actual item producers, row-50 use, field values, fallback results and
later door state are not supplied by this static census.

## Interpretation

This removes the pointer body's remaining own classifier-width question
and narrows the independent word-+10 Look contribution from 102 canonical
types to one candidate. It explains why changing a classifier bit need
not change this consumer's result. The surviving item/use-time admission
is Q-PARTY-115; other property consumers remain Q-PARTY-108 and the
broader semantic type contract remains Q-ITEM-005.

FND-COMBAT-171 observes a source-equal native table at one paused boundary;
it does not establish its preservation at a particular Look call. No
original ran in this batch. No new gameplay label, confidence, parity or
complete-reading promotion follows.

## Alternatives

Treating 08 and 80 as different Look-gate outcomes is ruled out by the
actual AL nonzero test. Treating all nonzero word-+10 changes as visible
is ruled out by the other type/item predicates, fallback and later door
checks. Row 50 is a candidate, not a native reachability result: an
admitted item/call satisfying the stated conditions would establish its
use, while a complete producer/domain exclusion would rule it out.

## How to reproduce

Resolve licensed DSUN.EXE and OBJEX.GFF through tools/game-dir.mjs. The
existing readers verify their recorded fingerprints. Read the complete
pointer body with resident_listing.py 28C9:2608..28C9:2839 and compose
FND-PARTY-252/253's full classifier/caller and Look branches. The retained
listing is GAME_DIR/analysis/item-removal-20261104/property-pointer-consumers.txt.

Run look_property_domain.py at c5a396a9 with the approved OBJEX.GFF path.
It verifies length 6816516 and XXH3-128
e08d3cac153c547f6804ca27b155915d before selecting 1F291..1F88B, then
enumerates exactly 102 rows with the three stated predicates. The result
is retained beside the listing as look-property-domain.json. It reports
candidate_indices [50] and type_forced_nonzero_rows 101. Original-derived
reports and bytes remain only in GAME_DIR.
