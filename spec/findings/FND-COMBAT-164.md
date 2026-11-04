---
id: FND-COMBAT-164
title: Admitted root cleanup clears temporary kind 319 before child deletion but leaves its presentation index for the parent loader to clear
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2410..2D40:27AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:27B7..2D40:286C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0C92..2D40:0CC2
tool: tools/research/exec-census/resident_listing.py at e01e0d89; Python 3.12.10; Capstone 5.0.7
environment: null
---

## Observation

FND-COMBAT-163 leaves the kind/state after an RDFF error's 2D40:2410 call
unadmitted. Existing FND-CONFIG-143/145, FND-PARTY-214/218 and
FND-ITEM-014/015 describe the traversal and deletion bodies. This reading
instantiates their root-release ordering for temporary slot 319, without
inferring valid graph state from a returning wrapper alone.

### Root binding and release prefix

2410 accepts slot 319 when its kind byte is nonzero, asks 2196 to enumerate
it into an 800-byte SS-local graph, then passes that graph and index zero
to 2452. The wrapper does not test the enumeration's returned count.
2452 constructs a 160-byte SS-local map through 27B7 and invokes 248D
with root index zero. Both map construction and deletion access the
supplied near map through DS, so DS/SS storage identity is required.

27B7's final store overwrites map word zero with its first argument,
supplied from graph entry zero's word +6. An intact root graph produced
for slot 319 has that word equal to 319. This final store occurs even
when its signed enumeration-count loop does no work; it does not by
itself validate the graph's other fields.

For root kind two, 248D first clears word +6 of the 49-byte record
selected by graph word +4 under DS:19C9. It then reads map word zero.
Admitted value 319 passes its signed comparison with 524, so it calls
0C92 with 319 before parent-field or recursive-child work.

The complete 0C92 body puts signed indices 320 through 519 on a pool
route. Index 319 instead reaches its no-child-call clear branch, which
sets byte 4F49:0FF0 to zero. That byte is the temporary object's kind.
This branch does not call 0CC2 and does not write the presentation
entry-index word at resident DS:95D7. Under disjoint, nonwrapping graph,
map, stack and record storage, that prefix therefore clears the kind
while the presentation index can remain populated.

The prefix precedes both child passes. A returning 2410 alone does not
establish that no later child or aliased write changed that kind again.
It also does not establish an empty presentation record.

### A sufficient childless return case

Consider an admitted enumeration result with exactly one graph entry:
step byte one, kind word two, valid record number, and root word +6 equal
to 319. With DS equal to SS, valid separate local spans and intact fields,
27B7's sole entry takes its non-step-two branch, then forces map[0] to
319. It makes no getter call on that entry.

The deletion prefix above clears kind 319. Root index zero selects parent
-1 and skips the parent-field setter. It sets map[0] to 9999. Both child
passes begin at index one and compare with signed count one, so neither
executes. No later child or setter can rewrite the kind in this case;
the deletion wrappers return with kind zero and the presentation index
unchanged by their admitted writes.

For an RDFF parse-error route carrying status FFFE into this cleanup,
if the subsequent buffer release preserves that cleared kind and status,
the later common kind-two metadata check is inapplicable because kind is zero.
The final result remains FFFE, so the presentation loader takes its
release/fill failure route described by FND-COMBAT-163. The presentation
fill belongs to that later parent route, not to 2410's kind clear.

## Interpretation

This removes root kind-release ordering and the sufficient childless
cleanup outcome from Q-COMBAT-045's unread error-state effects. It
separates object metadata emptiness from presentation-record emptiness,
which the numbered restore tests independently.

It does not establish that an actual RDFF error produces the named
childless graph or that every buffer release preserves the resulting state.
Other enumeration results, invalid/reset counts, near
storage mismatches, record/descriptor aliases, later child effects and
save-call timing remain open. A zero kind must not be substituted for
a FFFF presentation index when assessing what a save contains.
No native run, universal cleanup guarantee or parity promotion follows.

## Alternatives

0CC2's special rejection of slot 319 does not prevent this kind clear:
0C92's 319 branch bypasses that pool helper. Conversely, the root clear
does not prove the presentation record is already FF. A broader returned
cleanup needs its own descendant and storage admission; the childless
case does not supply it.

## How to reproduce

Resolve licensed DSUN.EXE through tools/game-dir.mjs. With the project-local
evidence Python run resident_listing.py on the complete half-open ranges
in Locations. The approved reader verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and declared mappings.
The combined listing is retained at
GAME_DIR/analysis/hit-incoming-20261104/temporary-root-cleanup.txt.
Follow the wrapper's root arguments, 27B7's final map[0] store, kind-two
branch, +257D signed comparison and 0C92's 319 branch. Then use count
one and root index zero in the complete deletion continuation. Reuse
FND-COMBAT-163's status and parent reading; keep original-derived output
only in GAME_DIR.
