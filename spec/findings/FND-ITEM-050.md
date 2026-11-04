---
id: FND-ITEM-050
title: Canonical field-eighty-nine samples cannot reject the placement load gate at Strength three or above
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF48..0x0002037E
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001F291..0x0001F88B
tool: tools/evidence/report.mjs table at 48fa94ce; composition with FND-ITEM-011/049 and FND-CONFIG-140/154
environment: null
---

## Observation

FND-ITEM-049 settles the placement caller's minimum aggregate and signed-word
comparison. This reads the remaining canonical descriptor/sample domain,
without claiming that those source tables or samples are admitted at every
native call.

### Complete source domains

OBJEX.GFF length 6816516 and XXH3-128 e08d3cac153c547f6804ca27b155915d
is the supported source of FND-CONFIG-153 and FND-ACTOR-017. Read selector
89's signed offset word in each of the five 196-byte FNFO/1 kind rows. Kinds
one, two, three and five all have FFFF (minus one); kind four has offset two.
The latter independently matches FND-ITEM-011's positive descriptor control.
FNFO/2 metadata for selector 89 is one (FND-ITEM-011).

With these descriptors retained and valid kind-one-through-five inputs,
FND-CONFIG-140's getter rejects the four missing-field cases with error -1
and zero result. FND-ITEM-049's aggregate skips those failed results. A
successful field-89 sample in this class therefore comes from kind four's
record offset two. With the documented setup width one for metadata one,
the getter sign-extends that byte to its doubleword result. This is a width
condition; the executable's initial width two is not silently treated as one.

The bounded complete ALL/4 read takes byte two from all 102 fifteen-byte
rows, ending exactly at the resource boundary. Its full frequency distribution
is:

| Byte-two value | Rows |
|---|---:|
| 0 | 71 |
| 1 | 11 |
| 4 | 1 |
| 10 | 12 |
| 20 | 2 |
| 40 | 2 |
| 50 | 1 |
| 75 | 1 |
| 100 | 1 |

Counts sum to 102. Every source byte is below 128, so signed byte extension
preserves its nonnegative value. This includes unused or unselected source
rows; their presence does not prove any particular native graph visits them.

### Conditional comparison closure

Assume the FND-ITEM-049 aggregate/near-state/copy preconditions, retained
canonical descriptors and byte width, valid kind/row selections, and that all
successful samples use unchanged canonical shared rows. Failed getter calls
are excluded from the samples, not interpreted as weight zero. Pending loss
or repeated visits need not imply a complete inventory census; the following
result applies to the successful samples actually encountered.

If at least one sample occurs, its selected signed minimum M lies in the
set 0,1,4,10,20,40,50,75,100. Its low-word signed value is unchanged. If no
successful sample occurs, FND-ITEM-049's retained initializer narrows to
-7937. For Strength S in 0..25, the exact outcomes of this load comparison
are therefore:

| Strength | Rejection condition in this source-preserving class |
|---|---|
| 0 or 1 | At least one successful sample, and every successful sample is in 40,50,75,100. |
| 2 | At least one successful sample, and every successful sample is in 75,100. |
| 3..25 | No rejection by this comparison. |

At Strength three the table value is 100, so even minimum 100 passes its
equality boundary. Higher admitted Strength rows return at least 250.
An empty successful-sample set passes for every admitted row because -7937
is less than its positive table value. None of these statements says the
complete placement gate accepts: its earlier and later predicates remain.

## Interpretation

This removes the canonical kind/width/sample-domain alternative from the
local load comparison. It does not support a claim that adding up canonical
item weights above a carrying capacity triggers this gate: the caller selects
a minimum, and the result above follows from that selection.

Q-ITEM-003 retains actual sample/traversal/getter admission and termination,
source-table and width retention, current-character/temporary-slot identity,
near storage/copy/alias conditions, runtime writes and other incoming routes.
A different width can read adjacent bytes; a changed kind descriptor or
shared row can produce samples outside this result. Q-ITEM-005 still needs
independent consumers and complete field contracts. No native bug, confidence,
complete-reading or parity promotion is claimed.

## Alternatives

Using the source distribution as a bound on all live shared records would
omit runtime mutations and aliases. Including missing-field returned zeros
would incorrectly force the minimum to zero even when those getter calls
failed. Treating a successful sample set as the entire item graph would omit
pending saturation and termination questions. Both the positive descriptor
control and the complete source row count are required for this exclusion.

## How to reproduce

Use report.mjs table with the source fingerprint above. FNFO/1 table start
1FF48 hex, count five, stride 196, one unsigned two-byte field at offset 178
reads selector 89 in each row. Interpret FFFF as the getter's signed -1
sentinel. Require the kind-four offset-two positive control. ALL/4 table
start 1F291 hex, count 102, stride 15, one unsigned byte field at offset two
reads every source value. Length/count evidence is FND-CONFIG-153 and
FND-ITEM-011; metadata one is the independently recorded descriptor case.

Private configurations placement-field89-kind-offsets-controls.json and
placement-field89-source-values-controls.json, corresponding reports and
repeat reports remain under GAME_DIR/analysis/item-removal-20261104/.
Both reports repeat byte-for-byte. Their SHA-256 values are
15bce150574ba1b2f4232ef3096d684ae2c7a8259e57742dc02affbbfe7e17ee
and 3ebb68dd73a707ea4c777e9342f896933eb28bb77a421423ea3c0d270cfd9e1b.
Verify all five offsets, frequency sum 102 and range 0..100 before composing
with FND-ITEM-049's selected result/table/width. No original code, table
fixture or payload is committed, and no original function was executed.
