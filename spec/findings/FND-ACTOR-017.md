---
id: FND-ACTOR-017
title: Kind-four references select the shared ALL/4 type table and one inline payload matches its indexed row
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073098..0x000730F6
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001F291..0x0001F88B
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0039986A..0x003998AE
tool: Inspector gff and tools/evidence/report.mjs table at 34fbedad; retained overlay_listing.py initializer reading; FND-PARTY-259/248 for importer and field bindings
environment: null
---

## Observation

Q-ACTOR-007 separates the retained kind-four reference from the meaning of
its skipped fifteen-byte payload. FND-ACTOR-015 identifies 1216 such chunks
in the shipped 68-byte RDFF resources. This reading binds the reference and
establishes one independent payload comparison, not a census of all payloads.

### Shared table offered to the native consumers

Overlay 188 +01F8..020B passes a far pointer to its size local, resource
identifier four and tag ALL to 38FF:05B5. The zero-result continuation
supplies double words one and that local size to 444C:0008 and stores
returned DX:AX at DS:19CD. A zero pointer takes the failure continuation.
The nonzero path offers DS:19CD to 38FF:04AB with the same tag and
identifier at +0232..0244. When that returns zero, +024E..0252 copies
the resulting far pointer to DS:615F.

DS:615F is the generic accessor base at DS:614F plus four times kind four;
FND-CONFIG-150 gives its stride word as fifteen. These are conditional
local bindings, not proof of allocation success, archive selection or complete
transfer. FND-CONFIG-151/152/209 retain those service contracts and limits.

The approved OBJEX.GFF directory places ALL/4 at file offset 127633,
size 1530. At the native fifteen-byte stride this supplies 102 logical
records beginning at the resource's start. No extra header displacement is
added by the published base or the indexed consumers described below.

### The retained reference is a type-row index

FND-PARTY-248 binds kind-one field five to record word +0A with metadata
thirteen. FND-CONFIG-140's generic getter turns that metadata into a kind-four
temporary handle whose paired record number is the value read from that word.
It therefore selects the fifteen-byte base above. The direct type consumers
in FND-PARTY-248 likewise multiply the item word +0A by fifteen and add it
to DS:19CD.

FND-PARTY-259 reads the type-three chunk import: kind three copies details;
the other branch, used by kind four, retains header record_ref as the paired
record number and does not copy the payload. The subsequent setter writes
header record_ref to the earlier target object's field numbered by the
header. For these shipped chunks that field is five (FND-ACTOR-015).
Thus the retained reference supplies the shared type-row index. It is not
an address of the skipped payload or a separate RDFF resource identifier.
FND-PARTY-235 separately qualifies when the writer emits kind-four children;
no new complete serialization claim is made here.

### One source comparison control

The directory contains 1216 RDFF resources of size 68, matching
FND-ACTOR-015's known two-chunk shape. RDFF/600 is at file 3774570.
Independent bounded table queries verify its first type-one/kind-one header
and 23-byte payload, then a type-three/kind-four header at resource offset
33, field five, record_ref 61 and payload length fifteen. The end header
starts at offset 58.

The fifteen bytes at resource offset 43 equal all fifteen bytes of ALL/4
row 61, at file offset 128548. This is one exact comparison, not merely
an overlapping word or a match of record widths. Other inline payloads
have not yet been compared in this reading. No payload bytes are committed.

## Interpretation

The native reference binding is answered: a kind-four record reference
selects the shared fifteen-byte type table loaded through ALL/4, including
the item field-five relationship. The skipped payload is not authoritative
for the known import path. The control supports the reading that inline
payloads duplicate shared rows, but does not establish it for all 1216 cases.

FND-ACTOR-018 subsequently settles the full known source payload relationship;
Q-ACTOR-008 retains independent native reader discovery.
FMT-ITEM-002 and Q-ITEM-005 separately track the complete shared-row field
schema; isolated consumer bit tests are not a complete format. Native table
and getter admission remain Q-CONFIG-008 and Q-SCRIPT-022.
No evidence-status promotion or complete reading is claimed. No original
game was run.

## Alternatives

- Equal record widths alone would not identify the shared table. The
  initializer publishes its base and stride, the field metadata selects
  kind four, and direct item consumers use the same index/stride pair.
- One exact inline copy does not rule out historical or custom differences
  in other resources. The known importer can ignore such differences.
- The retained index is not itself a byte pointer. Its memory address
  requires the shared base and fifteen-byte multiplication.
- Successful local branches do not prove the offered source archive, pointer
  capacity, transfer result, later table lifetime or every admitted index.

## How to reproduce

Use DSUN.EXE size 634416, XXH3 e296af55ba2ecde7e77f555c90f33d0b,
and OBJEX.GFF size 6816516, XXH3 e08d3cac153c547f6804ca27b155915d.
Reuse FND-ACTOR-016's complete overlay-188 initializer listing and inspect
+01F8..0256, preserving both failure branches and the ALL/4 arguments.
Reuse FND-PARTY-259/248 and FND-CONFIG-140/150 for the separate importer,
type descriptor, stride and accessor claims.

Run Inspector gff on the approved object archive. Select ALL/4's directory
entry and RDFF/600, and count the known 68-byte resources. With report.mjs
table, source that archive and its explicit xxh3, query one 68-byte row at
3774570: firstType at 0 width one; firstKind at 2 width one; firstLength
at 8 width two; secondType at 33 width one; secondKind at 35 width one;
recordReference at 37 width two; field at 39 width two; secondLength at
41 width two; endType at 58 width one. Require the values above.

For the independent comparison query one fifteen-byte row with width-one
fields b0 through b14 at offsets zero through fourteen, first at 3774613,
then at 127633 plus fifteen times 61. Compare every field. The existing
table reporter checks the source identity and both physical spans.

The catalog and control-600-header/payload/type-row configs and reports
remain under GAME_DIR/analysis/kind-four-reference-20261103/.
The initializer listing remains in the FND-ACTOR-016 store. No original
code, archive member or byte array is committed.
