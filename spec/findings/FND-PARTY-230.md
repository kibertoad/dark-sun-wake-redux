---
id: FND-PARTY-230
title: Shipped kind-1 field six selects the attached-record image cache with type two and initial width two
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF54..0x0001FF56
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00020322..0x00020323
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D40E..0x0004D40F
tool: tools/evidence/report.mjs table; hash-checked numeric source queries and the accessor readings in FND-PARTY-051, FND-PARTY-214 and FND-PARTY-216
environment: null
---

## Observation

FND-PARTY-216 binds the shipped FNFO field-offset descriptors,
field-type bytes and executable width table to the generic accessor.
FND-PARTY-224 and FND-PARTY-227 identify the kind-1 record word
at +0C as the reviewed image-number cache. The same bounded numeric
query applied to field six gives this binding:

| Source | Selected value |
|---|---|
| Kind-1 FNFO 1 descriptor, base 1FF48 plus twice field 6 | Record offset 12 decimal, 0C hexadecimal |
| FNFO 2 type byte, base 2031C plus field 6 | Type 2 |
| Executable initial width table, DS:040C plus type 2 | Width 2 |

The installed OBJEX.GFF query verifies size 6816516 and XXH3-128
e08d3cac153c547f6804ca27b155915d. The executable query verifies
size 634416 and XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
These are source descriptor and initial-data values, not a native
capture of the current tables.

Under those descriptor bindings, an admitted kind-1 field-six
request to 1AA0:012A selects record +0C. FND-PARTY-051 describes
its record/field lookup, invalid-object and missing-field statuses,
and copy request. Type two takes the ordinary value-copy path,
whose byte count comes from the current width table. With width
two, it requests copying two bytes of the supplied value into the
cache word. This field request is a concrete indirect route to
consider when tracing producers; it need not contain a literal +0C
record-store instruction in the caller.

This does not establish a call that actually selects kind one and
field six, successful record allocation, current descriptor/table
identity, safe source/destination aliasing or the copy child's full
effects. FND-PARTY-221 distinguishes initial widths from runtime
width mutations for another type. No all-writers stability claim
for type two is made here. Other kind/field combinations can select
unrelated words; a field-six immediate alone is not cache evidence.

## Interpretation

The shipped field-six offset/type/initial-width binding is mapped.
Q-PARTY-087's indirect-producer research can now test that named
field route while retaining actual caller admission and all other
producers. This does not close the cache lifecycle or turn a literal
offset scan into a complete writer census.

## Alternatives

- Treating kind-1 field six as an unassigned field omits its shipped
  descriptor's exact match to the reviewed cache word.
- Treating field six of every kind as the cache ignores the
  kind-indexed offset row and record table.
- Treating initial width two as immutable runtime state omits the
  accessor's dependence on the current width table.
- Treating the descriptor as evidence of an actual writer call
  conflates an available field binding with a reached request.

## How to reproduce

Use tools/evidence/report.mjs table with the locked source identities
above. Read the kind-one offset row at file 1FF48, 98 little-endian
words of stride two, the 196-byte per-kind row described in
FND-PARTY-051 and FND-PARTY-216; inspect row index six. The positive
controls at fields one, four and ten match the prior findings.
Read one type byte at 20322 and one executable width byte at
4D40E. Retained query definitions are under
GAME_DIR/analysis/party-record-heads/drawing-20261018/:
cache-field-offsets-query.json, cache-field-type-query.json and
cache-field-width-query.json. They contain numeric query parameters,
not a new format reader.

The result is offset 12, type 2, initial width 2. Compare the field
and kind indexing with the cited accessor findings. Preserve current
binding, width, alias, caller and child-result conditions when using
this result; no original function or full game was executed here.
