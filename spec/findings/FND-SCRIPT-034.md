---
id: FND-SCRIPT-034
title: Trigger keys use kind-specific field-zero words and kind four has no shipped mapping
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF48..0x0002031C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0AE5..2D40:0B00
tool: tools/evidence/report.mjs table and resident_listing.py at c79b5a31; executable-reader 4.3.0; locked evidence Python
environment: null
---

## Observation

Q-SCRIPT-002 asks whether the field-zero value used by trigger walkers is
the object's number. The complete 27-byte wrapper at 2D40:0AE5 reads its
word argument. A signed-negative value is copied directly to AX and returned.
Otherwise it calls 1AA0:0009(handle, 0) and returns the callee's AX without
checking its error word. It has no local table write or further conversion.

FND-CONFIG-153 locates FNFO/1 at OBJEX.GFF file 0x1FF48, length 980.
Its 196-byte rows select the per-kind field offsets after loading into
DS:5AF7 (FND-PARTY-051/216). Reading only the first word of each row gives:

| Kind | Field-zero descriptor | Previously documented field |
|---|---|---|
| 1 | 0 | Attached-record signed word +00, the negated object resource number (FMT-PARTY-006) |
| 2 | 6 | Combatant word +06, character archive identifier (FMT-COMBAT-001) |
| 3 | 14 | Details-record signed word +0E, the shared details key (FMT-COMBAT-002) |
| 4 | FFFF, interpreted as -1 | Missing field |
| 5 | 0 | First word of the 23-byte kind-five record; meaning remains Q-ACTOR-003 |

The first three values match FND-PARTY-216 and FND-COMBAT-074, providing
independent controls. FND-ACTOR-015 records the kind-five payload copy
without naming its first word. Repeated table reports are byte-identical;
source OBJEX.GFF is size 6816516, XXH3 e08d3cac153c547f6804ca27b155915d.

FND-CONFIG-154 records field-zero metadata nine in FNFO/2, distinct from
initial executable metadata. Under successfully loaded/preserved metadata
and its separately recorded width-two setup conditions, the valid present
field returns a signed word as a dword. Metadata nine skips the getter's
metadata-at-least-ten handle-conversion child. The trigger wrapper uses AX.
For offset -1 the getter instead assigns -1 to DS:40C4 and returns zero
(FND-CONFIG-140); the wrapper does not distinguish that from a valid zero.

## Interpretation

The mechanism is a trigger comparison key: negative arguments pass through;
nonnegative arguments request a kind-specific field. It is not uniformly a
placed-object slot or record offset. Kind four's missing field also prevents
assigning a valid object-number schema to every kind from this table.

The known kind-one, kind-two and kind-three source identities are answered.
FND-ACTOR-016 subsequently answers kind-five source-key meaning. Its other
fields remain Q-ACTOR-003. Actual metadata, width/record preservation and
which kinds reach a walker remain separate admission obligations in
Q-CONFIG-008 and Q-SCRIPT-022. No native
missing-field case, complete reading or status promotion is established.

## Alternatives

- Returning the input handle would ignore the getter's result on the
  nonnegative branch. Negating negative literals would contradict its direct
  return. Field zero is not record offset zero for every kind.
- The attached-record key, character identifier and details key have different
  documented producers. Their shared field number does not make them one
  universally identified resource number.
- Shipped FNFO bytes do not prove successful loading, direction/width setup,
  immutability, valid record pointers or live reachability. A missing-field
  zero is a conditional local result, not an observed gameplay consequence.

## How to reproduce

Use `node tools/evidence/report.mjs table CONFIG` with fingerprint-checked
installed OBJEX.GFF and the xxh3 above. Table config: start 0x1FF48, count
5, stride 196, one field named fieldZeroOffset at offset zero and width two;
countEvidence names FND-CONFIG-153's 980-byte resource and
FND-PARTY-216's kind stride. Require rows 0, 6, 14, FFFF, 0 and compare
the first three against prior bindings. Interpret FFFF through the getter,
not the unsigned table reporter. Repeat and compare bytes. Report SHA-256:
7df2334781d04fd534ca1ae7b27e5b08c85f588f3371beb69c913238d585f122.

With locked evidence Python and DSUN.EXE size 634416, XXH3
e296af55ba2ecde7e77f555c90f33d0b, run resident_listing.py on the complete
inventory body 2D40:0AE5..2D40:0B00. Keep its signed gate, negative
return, field-zero pushes and actual relocated getter call. Reuse
FND-CONFIG-140/154 for the accessor and metadata/width conditions.

Reports remain under GAME_DIR/analysis/trigger-object-context-20261103/:
field-zero-offsets.json, its report and repeat, and trigger-key-wrapper.txt.
No original game was run; no resource bytes or instruction listing is committed.
