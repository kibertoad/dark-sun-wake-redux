---
id: FND-ITEM-094
title: ETAB loading clears storage before read success and recount propagates flags by normalized slot identity
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089242..0x00089339
tool: tools/research/exec-census/overlay_listing.py and window_operands.py at 223536a7; Python 3.12.10, Capstone 5.0.7; composition with FND-MAGIC-011, FND-PARTY-022 and FND-ITEM-066/080/093
environment: null
---

## Observation

Q-PARTY-116 retains count/row producers before effect construction and
removal. FND-MAGIC-011 and FND-PARTY-022 identify overlay 200 +00E2
as the ETAB count producer. This reads its full local body, including
post-count flags, failure ordering and boundary reads. Overlay offsets and
field offsets below are hexadecimal; counts are decimal.

### Clear, transfer and count publication

The routine returns without these operations when DS:265B is zero or
its region word is outside unsigned 1 through 99. Otherwise it requests
1000:3FA2 to fill 12000 bytes through current far DS:67B7 with zero.
FND-ITEM-080 records that fill helper's contract; valid spans remain required.

It then requests 38FF:04AB for ETAB and the zero-extended region number,
passing the address of DS:67B7. Nonzero returned AX exits before changing
DS:264E. The clear and any child writes are not rolled back. Consequently
an error can leave old count metadata alongside cleared or changed table
storage; no actual failed native transfer is inferred from this local route.
Pointer identity and transferred extent/content remain child obligations.

A zero result resets DS:264E to zero and captures the then-current table
pointer in a local far cursor. The routine reads each cursor row's signed
slot word +6. Zero terminates. Positive words are negated at word width;
negative words are retained. Every visited nonzero row receives flags
byte +5 mask 08, then the cursor offset advances by eight and count increments.
Coordinates and key byte +4 have no own stores on this recount path.

At the next condition it reads the new row's slot word before testing
count below 1500. Reaching count 1500 thus still reads row index 1500
word +6; it does not modify or count that boundary row. FND-ITEM-066's
modeled ordinary allocation supplies capacity for that read, but its initial
zeroing does not guarantee those boundary bytes are zero. A table pointer
from another allocation or restore needs its own span admission.

Under retained disjoint storage and valid child outputs, count zero after
this successful-read route means the first row's slot word is zero. It
does not imply that the first row's coordinate/key bytes are zero: ETAB
can supply them independently. FND-ITEM-093's zero-key case therefore
needs more than this producer's zero-count result.

### Flag propagation after normalization

The second phase scans row indices below current unsigned DS:264E.
For each row whose flags byte has mask 20, it saves that row's normalized
slot word and scans the same counted prefix, setting mask 20 in every
row with an equal slot word. It rereads the count at loop conditions
and current DS:67B7 when selecting rows. There is no own slot-range,
positive-slot or effect-constructor test on these flag writes.

For intact finite storage without external mutation, one initially flagged
row therefore flags its whole equal-slot group after normalization.
Positive a and negative a words normalize to the same negative word;
the signed-minimum word remains negative and is retained. Unrelated slot
groups acquire no mask 20 from this loop. The loop neither creates rows
nor changes count, coordinates, byte +4 or slot words in this phase.

This supplies another conditional source of counted flagged rows besides
the constructor. It does not establish that shipped ETAB resources contain
such seeds, that the normalized slot later names an admitted object, or
that those flags survive later creation/restoration/removal. SI/BP are
restored; no overall result normalization or broader preservation is asserted.

## Interpretation

This removes ETAB recount's local clear/read/count boundary, slot
normalization and grouped flag propagation from Q-PARTY-116. Actual
region/caller admission, archive/resource identity and content, successful
full transfer, count/pointer/key writers, aliases and use-time lifetime
remain. The inventory-backed count-operand report retains direct writes
and pointer candidates, not an exhaustive producer census; computed,
implicit and other-segment writes remain explicit exclusions.

## Alternatives

Treating read failure as no mutation ignores the earlier fill. Treating the
row cap as protecting every read reverses the boundary order. Treating
count zero as a zero key equates different fields. Treating mask 20 as
exclusive to constructor-created rows ignores equal-slot propagation.
No native defect, complete reading or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. With project-local evidence Python -I,
run overlay_listing.py on descriptor 200, file range 89242 89339.
GAME_DIR/analysis/item-removal-20261104/ holds
effect-etab-recount-contract.txt and its -repeat pair, both SHA-256
92d413a9364a5c8c5a7d759a4ffcc8d34aad7d26ef54783850ca6114c492a40e.
effect-placement-count-operands.txt uses window_operands.py with the
committed DSUN.EXE inventory and window 264E..2650; constructor increment
and ETAB reset/increment are positive controls. It preserves raw interior
candidates and the reader's exclusions. Keep reports local. No original
game or isolated function was executed.
