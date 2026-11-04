---
id: FND-ITEM-052
title: The placement removal gate checks existing target items against seven initialized effect bits
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077D26..0x00077F3A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FE93..0x0007FF07
tool: tools/research/exec-census/overlay_listing.py; retained placement-removal-gate.txt of FND-ITEM-048; composition with FND-COMBAT-029/036 and FND-PARTY-217
environment: null
---

## Observation

FND-ITEM-048 identifies overlay 189 +3376 as the local gate called before
placement category checks. This reads its complete local predicate and
composes the bitmap offsets with the existing preparation contract. It
removes local snapshot interpretation from Q-ITEM-003, not native admission.

### Target and early-return ordering

The helper takes a logical position L and message-enable byte. It first
reads map word L through DS:617D. A FFFF word returns AL one without
calling bitmap preparation. There is no independent local position or map
capacity check; valid nonwrapping map storage is a prerequisite.

For a non-FFFF map entry, it calls overlay 193 +1BA3 with the current
character at 4E71:0B44 and a near pointer to its BP-10 hex local bitmap.
Only after preparation returns does it check L. Positions other than
6, 7 and 14 return AL one without inspecting snapshot bits. Thus an
occupied ordinary position still invokes preparation; this early success
is not a no-callee or side-effect-free contract.

For those three positions it first tests snapshot byte 13 mask 04 hex,
corresponding to effect ID 106. A set bit immediately takes rejection,
without needing the target item's object-number word.

### Six effect and existing-item pairs

If that bit is clear, the remaining tests run in the order below. Bitmap
byte offsets and effect IDs are decimal; masks and object-number magnitudes
are hexadecimal.

| Order | Bitmap byte | Mask | Effect ID | Required existing object-number magnitude |
|---|---:|---|---:|---|
| 1 | 4 | 80 | 39 | 1B40 |
| 2 | 2 | 02 | 17 | 1B42 |
| 3 | 7 | 80 | 63 | 1B43 |
| 4 | 11 | 04 | 90 | 1B3F |
| 5 | 11 | 80 | 95 | 1B41 |
| 6 | 10 | 02 | 81 | 1B45 |

A clear effect bit skips its item read. For each set bit, the helper rereads
current character, selects its graph through DS:6189 plus four times that
character, rereads map word L, and takes that graph entry's word +4 as a
23-byte attached-record index through DS:19C1. It reads word zero of that
existing record and forms its signed-word absolute magnitude with sixteen-bit
arithmetic. A matching magnitude rejects immediately; otherwise it advances
to the next bit/pair. FMT-PARTY-006 binds that signed word to the object
resource number. This is not a comparison with incoming type or name fields.

Both signs of each listed magnitude match. The signed-word minimum -32768
retains low word 8000 under this absolute operation and matches none of the
six listed constants. With stable map/graph/record state the six reads name
the same target item; that stability is a condition, not an inferred cache.
If no pair matches, the helper returns AL one.

Every rejection joins one tail. A nonzero message byte calls local +1D78
with the grafted-item message, then returns AL zero. A zero message byte
returns zero without that message call. Other effects of preparation or the
message helper are not suppressed by describing this as a predicate.

### Bitmap storage and meaning

FND-COMBAT-029's +1BA3 clears exactly 15 bytes through DS:bitmap and ORs
membership bits for matching current-character rows. With admitted IDs
0..112, returning helpers and its table/count/boundary prerequisites, its
writes stay in those fifteen bytes. It does not clear the caller's full
16-byte reservation. The tested byte offsets are 2,4,7,10,11,13; all are
inside the initialized span, and none reads the uncleared byte 15.

With DS near storage bound to the caller's SS-local bitmap, ID 8*b+k is
therefore the membership represented by byte b/mask 1<<k. This gives the
seven IDs above, independently of status-panel category filtering or effect
labels. Preparation does not use the incoming item or the supplied position
as its owner: its argument is the current character. Effect 106 alone blocks
all three occupied target positions in this local contract, rather than
requiring the existing item to have supplied that effect.

These conclusions require admitted sorted row/count and boundary storage,
nonwrapping frames, DS/SS near identity, valid map/graph/record selections,
returning callees and retained bitmap/current-character state. The preparation
owner read before its count check remains FND-COMBAT-029's separate admission
condition. A near offset or initialized byte count alone does not prove the
snapshot was populated at this native call.

## Interpretation

This settles which side of placement the gate reads, all local rejection
pairs, and whether its tests depend on the unused sixteenth bitmap byte.
FND-ITEM-051's repeated queries can invoke this target-side gate even when
the proposed item is unchanged; query message mode does not select a different
bitmap owner or bypass these rejection tests.

Q-ITEM-003 retains actual bitmap table/owner/boundary and near-storage
admission, target map/graph/record lifetime, message and preparation effects,
incoming event/identifier state and complete transfer/action contracts.
No universal removal prohibition, observed grafting action, confidence,
complete-reading or parity promotion follows from this local composition.

## Alternatives

Using the incoming item's object number would read an argument this helper
does not receive. Checking all occupied positions would omit the explicit
6/7/14 filter. Requiring an object-number match for effect 106 would add a
test absent from its branch. Treating the bitmap's uncleared sixteenth byte
as a dependency of these seven tests would include a byte none reads.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Reuse the retained controlled
GAME_DIR/analysis/item-removal-20261104/placement-removal-gate.txt of
FND-ITEM-048, regenerated with overlay_listing.py descriptor 189 over
77D26..77F3A hex. Read all early returns, bitmap call arguments, bit tests,
map/graph/record reloads, absolute-word operations and common message tail.

Compose +1BA3 with FND-COMBAT-029/036 rather than interpreting an unused
reserved byte as initialized. For each tested BP displacement subtract the
bitmap base BP-10 hex to obtain its byte offset, then compute 8*offset plus
the set mask's bit ordinal. Verify the six object comparisons and the
independent unconditional effect-106 branch. No original code, bitmap fixture,
resource payload or image is committed or executed.
