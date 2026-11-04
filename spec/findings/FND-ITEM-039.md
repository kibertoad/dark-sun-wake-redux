---
id: FND-ITEM-039
title: Known item initializer and scalar setter exclude a name invalidation while the record sentinel returns success without a store
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E78B..0x0005E83A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E35D..0x0008E49C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E49F..0x0008E4FD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E500..0x0008E5A1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E5A1..0x0008E5C5
    kind: file-data
tool: tools/research/exec-census/overlay_listing.py and tools/evidence/report.mjs table at d1e39206; Python 3.12.10, Capstone module 5.0.7; composition with FND-ITEM-013 and FND-PARTY-156/245/248
environment: null
---

## Observation

Q-PARTY-115 needs an admitted type-50 item with name word +14 hex at
least 9999, among other Look conditions. This reading removes three known
producer families from the remaining name/type uncertainty. It assumes
valid nonwrapping record spans, stable table pointers and disjoint record,
global and stack storage. It does not establish those conditions for every
caller or exclude stores through aliases.

### Direct initializer and free-list leaves

The complete overlay-173 body +3E6B..+3F1A has no child calls or loops.
It uses the low word of 23 times the supplied index. Its first table load
sets ES; later stores reload the table offset while retaining ES. Under
the stated conditions it writes type 21 and name 65, the nominal values
already recorded in FND-PARTY-245. It also writes identifier -13999,
zero words +02,+06,+0C,+0E,+10, byte +12 equal to one and byte +16 zero.
Offsets here are hexadecimal. It leaves link +04, contents head +08 and
position byte +13 untouched: this is not a complete record clear.
There is no index-capacity check in this body.

This initializer therefore cannot itself produce type 50 with an invalid
name. A later type change preserving its name still leaves name 65.
FND-ITEM-013's complete allocator/release leaves write only the free-list
head, record link +04 and, on release, identifier +00. They preserve name
and type under the same separation conditions. Allocation may retain an
earlier invalid name; preservation is not a guarantee of valid input.

### Independent scalar script setter

FND-PARTY-156 identifies script opcode 22 function 49 with overlay 204
+259D. This is a separate API from generic setter 1AA0:012A. The reading
follows its kind-one paths, shared identifier-store tail and common return.
The inventoried body has the three code ranges above; the gaps between
them are not treated as decoded instructions.

The function initializes its result byte to one. Before reading kind or
the field selector it reads the target metadata record word. Exact 9999
jumps directly to the common return: AL is one and there is no record
store. Consequently a success byte alone does not prove a mutation.
Other record numbers are not admitted by a capacity check here.

For kind one, the full 32-bit selector is compared against six dword keys.
The loop starts with CX six, advances the key pointer four bytes on each
miss and uses the remaining count to index the following six target words.
The fingerprint-checked tables give:

| Selector (decimal) | Target in overlay 204 | Store in the 23-byte item |
|---|---|---|
| 0 | +2767, then shared +261D | word +00 |
| 1 | +2773 | word +02 |
| 3 | +2787 | word +06 |
| 7 | +279B | word +0E |
| 8 | +27AF | word +10 |
| 10 | +27C3 | byte +13 |

Each uses the low value word or byte from the value argument. These
kind-one paths have no child call. A match retains result one. A miss
sets result zero; only AL is promised by the return sequence. Thus field
eleven (name) and field five (type) both return zero without a record
store for a non-sentinel kind-one input. Kind two has a child-call path;
this finding does not extend the kind-one no-call claim to that branch.

## Interpretation

Together with FND-ITEM-037/038, this excludes the initializer's own writes,
the admitted allocator/release leaves and this scalar setter's kind-one
stores as direct name-invalidating or type-changing mechanisms. It also
separates an unsupported-field result from the sentinel success-without-write.
The scalar setter can change word +10 while preserving name, but that alone
does not admit the invalid-name prerequisite or a visible Look change.

Remaining mechanisms include other direct/computed stores, whole-record
copies/restores and nonfirst source records, changed metadata/streams,
the decoded script gap, aliases and actual use-time admission. This is a
bounded producer exclusion, not a global writer census or a complete
reading promotion. No original function was executed.

## Alternatives

A general reset might have invalidated the name; the initializer instead
writes 65, and the free-list leaves preserve it. A generic field-number
interpretation might permit name eleven in the scalar API; its own complete
six-key table excludes it. A success result might imply a write; the early
record-9999 branch refutes that implication. None excludes an unrelated
writer or an aliased destination.

## How to reproduce

Use installed DSUN.EXE length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py PATH 173
0x5E78B 0x5E83A, then descriptor 204 separately over 0x8E35D..0x8E49C,
0x8E49F..0x8E4FD and 0x8E500..0x8E5A1. The retained combined listing is
GAME_DIR/analysis/item-removal-20261104/name-direct-producers.txt.
Reuse FND-ITEM-013 for the resident free-list leaves.

Run node tools/evidence/report.mjs table CONFIG with the same source hash,
six rows, limit six, unsigned field value at offset zero: keys start
0x8E5A1, stride/width four; targets start 0x8E5B9, stride/width two.
Count evidence is overlay +2748's six-entry loop and its matched-key
target calculation. Retained configurations/results in that directory are
name-scalar-keys.json and name-scalar-targets.json and their .report.json
outputs. The keys are 0,1,3,7,8,10 and target words are
10087,10099,10119,10139,10159,10179. Source identity checks pass.
No original code or data tables are committed.
