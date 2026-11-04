---
id: FND-CONFIG-222
title: Window count has three literal writers and wrapped bookkeeping differs from graph validity and signed traversal
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0009..39D1:0316
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:02B3..3A8E:05EF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0A26..3A8E:0C13
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0003..3A8E:02B3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0EB8..3D72:1157
tool: tools/research/exec-census/window_operands.py and resident_listing.py at af3b3388; existing producer/removal contracts FND-COMBAT-093 and FND-CONFIG-168/173
environment: null
---

## Observation

Q-COMBAT-046 retains the actual DS:A103 state that selects the
dispatch branches of FND-CONFIG-221. This inventories its literal
operands and composes the known count mutations with those branches,
without admitting a native graph, guard or callback. Offsets and count
bit patterns below are hexadecimal.

### Three literal writers with independent controls

window_operands.py uses coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv at
af3b3388 and half-open window A103..A105. It reports fourteen operands
at listed instruction starts and one separate overlapping candidate.
The three listed writes are independently checked from their entries:

| Instruction | Own count effect | Ordering |
| --- | --- | --- |
| 39D1:0200 in 0009 | Store word zero. | Reached after prior initialization/asset/service calls; the next instruction clears head DS:A105. Entering 0009 alone does not prove reaching this store. |
| 3A8E:0333 in 02B3 | Increment word with wrap. | New head publication at 032F precedes this; region and child setup follow it. |
| 3A8E:0ABA in 0A26 | Decrement word with wrap. | Head/link handling precedes this; 0003, child disposal and later pointer clears follow it. |

These known write sites are the positive controls. The other eleven
listed operands read the count in collectors, region recalculation,
registration/removal, dispatch and pointer hit traversal. The separate
candidate is marked inside a listed instruction; it is not another
established reader or writer. No hit-bearing routine has a decoded
string store, but that is not a whole-program bulk-write exclusion.

The query covers encoded displacements/immediates in the declared
resident and FBOV code and retains the every-byte candidate pass.
Computed aliases, overlapping writes from a different displacement,
changed DS, runtime code and unresolved instruction boundaries remain
outside its conclusion. Three literal writers are not an all-writer proof.

### Mutations do not count only completed operations

FND-COMBAT-093 reads registration's publication before later failure
branches. Its region-helper nonzero result, missing child or rejected
constructor can return FFFF without an own reversal of head/count.
An increment therefore cannot be counted as a completed usable window
merely because it occurred. Its own body has no duplicate-membership
test or independent count-overflow gate.

FND-CONFIG-168 reads removal's corresponding order. Null input rejects
before the own decrement. A target equal to the current head replaces
that head from target+EE and reaches decrement. A different target
with count one skips the membership search and still reaches decrement;
other counts use the unbounded link search. Later child failure does
not restore the earlier count. Neither operation supplies graph validity
or count conservation on its own.

After a reached zero store, if those increments/decrements are the
only changes and storage/DS are preserved, the word is
`(P - U) mod 65536`, where P and U count executions of the two mutation
instructions. P does not mean successful opens, and U does not mean
successful final cleanup. Actual mutations, resets and preservation
are required to use that conditional relation.

### Count and graph need not agree under the body's admitted local tests

The following are own transition cases, not observed native states:

| Inputs at the relevant own gate | Result of the named transition |
| --- | --- |
| Registration reaches 0333 with count FFFF. | Count becomes zero after its new head was already stored. |
| Removal has nonnull target equal to head and reaches 0ABA with count zero. | Count becomes FFFF; an admitted null target+EE can already have made the head null. |
| Removal has count one and a nonnull target different from head. | The own count-one branch skips membership search, leaves the own head unchanged and decrements to zero. |

The last branch does not itself prove target membership. Each case
still requires valid accessible records, permitted earlier callees
and preserved frame/global state. Later children may change state or
prevent return. No such input was seeded or run. These cases show why
a count test alone does not prove the head is null or a window is usable.

### Signed walks and dispatch classify the same word differently

FND-CONFIG-173's two window passes start their indices at zero and
compare them signed to the current count. FND-UI-068's pointer hit
walk likewise uses signed comparisons. For a stable word 8000..FFFF,
zero is not signed-less than the count, so those initial window-body
iterations are skipped. The helpers can still perform other operations;
this is not a no-effects contract for their entire bodies.

FND-CONFIG-221's dispatcher instead asks only whether the word equals
zero. Those same nonzero bit patterns take its nonzero-count route,
without an own head-validity or signed-positive gate. Count zero takes
the kind-three rewrite before a global call, irrespective of a retained
nonnull head. Actual preservation to dispatch and actual targets remain
open. A nonzero count is not an admission of a pointer hit or live
Save/Load window, and an unsigned reading cannot replace the signed walks.

## Interpretation

The local count producer and arithmetic contracts are now explicit for
Q-COMBAT-046. They remove an assumed equation between successful window
operations, the count and a coherent live graph. Remaining work must
admit the actual word, graph membership, reached mutation/reset history,
guard/callee effects, target fields and event delivery; it cannot use
the literal writer census or a nonzero count as substitutes.
Q-CONFIG-008 retains the shared message-callback dependencies. No queue
item closes or evidence status is promoted.

## Alternatives

- An initializer entry is not proof that its later count clear ran.
- A failed registration can retain an earlier own increment.
- A removal decrement can precede a later failure or pointer clear.
- Zero count does not independently prove a null head.
- Nonzero and signed-positive are different conditions on this word.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. The readers verify length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run window_operands.py with the named
inventory and window A103..A105; require the clear/increment/decrement
controls and retain the overlapping candidate separately. Read all
listed resident ranges from their inventoried starts. Follow the
mutation-before-failure order and compare signed loop gates with the
zero-only dispatcher gate in FND-CONFIG-221. Reuse FND-COMBAT-093,
FND-CONFIG-168/173 and FND-UI-068 rather than promoting their caller,
guard, allocation or geometry assumptions.

Retain window-count-operands.txt and window-count-transitions.txt only
under GAME_DIR/analysis/hit-incoming-20261104/. No original code, graph,
count fixture or memory image is committed, and no original is executed.
