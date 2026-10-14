---
id: FND-PARTY-218
title: Object-graph word 6 carries the root object or a descendant field selector, and deletion dispatch uses the entry kind
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
tool: Python 3.12 with Capstone 5.0.9, tools/research/exec-census/resident_listing.py
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:044C..31E0:0461
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2196..2D40:2410
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2410..2D40:256E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:051C..1AA0:063F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00024DAD..0x00024DB7
    kind: file-data
---

## Observation

This replaces FND-PARTY-172's graph reading while retaining its directly
read deletion-switch facts. All addresses refer to installed DSUN.EXE
relocated to load segment 1000.

### Entry construction and field-selector provenance

2D40:2196 starts CX and SI from the object argument, clears its count,
parent-index local and state-stack depth, then walks the object graph.
For the first entry SI is still that root object. It writes SI to entry
word +6 at 2295; this root value is not an incoming child field number.

For later entries, 23B7..23C5 loads SI from the local state stack's
word array at BP-E6 plus twice the state depth. The stack base passed
to 1AA0:0566 is BP-128, so this is that stack's offset +42. The producer
chain establishes what that word holds:

1. 1AA0:0566 selects a word DI from DS:5ECB plus 34 times the object's
   kind plus twice a descending local selector. When DI is nonzero,
   it passes DI as the field argument to 1AA0:0009 at 05B5.
2. When the accessor's low word is not 9999, it passes that returned
   object, the state-stack near pointer, DI and the caller's parent index
   to 1AA0:05DC at 05CD.
3. 05DC stores the object at state +2 + 2 * depth, the field argument
   DI at state +42 + 2 * depth, and the parent index at state +82
   + 2 * depth. It also keeps three-byte object metadata at state +C2
   when the object is DS:60EB, and bounds the depth increment at 31.
4. 1AA0:051C decrements a nonzero depth, returns the object from the
   +2 array, and restores kept metadata for DS:60EB. Back in 2196,
   23A6..23B7 loads the selected parent index from the +82 array and
   23B7..23C5 loads SI from the +42 array. Entry word +6 therefore
   receives the selected field number for a descendant entry.

These paths access the near state pointer through DS while the caller
constructs it from a BP-relative local allocation. Their storage identity
remains a prerequisite, not established merely by those matching offsets.

The entry layout directly written by 2196 is:

| Offset | Own write |
|---|---|
| +0 byte | Step code 1 for the first entry. Later, 4 when DS:60ED + SI holds type 8; otherwise 3 when the object equals DS:60EB, or 2 when it differs. |
| +1 byte | Low byte of the selected parent-entry index; at exit the first entry's byte is overwritten by the total count. |
| +2 word | Zero-extended object kind from 4F49:(0C33 + 3 * object). |
| +4 word | Record number from 4F49:(0C34 + 3 * object). |
| +6 word | SI: the root object initially; selected field number for later entries. |
| +8 word | Kind-indexed stride word from DS:6167. |

Each ordinary entry is ten bytes. The loop checks count below 79 at
23CB before another pass. On exit it writes FF at the following entry's
byte +0 and count into the first entry's byte +1, returning count in AX.
Invalid object-kind paths can return zero and leave other output bytes
untouched. No fully initialized output contract is claimed for those paths.

The walk uses 1AA0:0566 and 051C for its descendants. Type-8 traversal can
rewrite the metadata for DS:60EB before another entry is made. Their
callers, input tables, accessor status, aliases and total traversal progress
are not a complete reading here.

### Deletion entry and switch

31E0:044C skips its body when DS:265B is zero; past that guard its later
call reaches 2D40:2410 (FND-PARTY-213). FND-PARTY-047 locates startup's
writer of that guard. 2410 returns without graph construction for an
unsigned object at least 9999 or with kind zero. Otherwise it lists the
object through 2196 into an 800-byte SS-local buffer and passes that
far buffer and root index zero to 2452. The latter calls 27B7 and 248D;
FND-PARTY-214 supplies the separate map-construction reading.

248D dispatches on entry word +2 minus one, accepting unsigned indices
0 through 4 in the five-word CS table at 27AD. In terms of entry kind:

| Kind | Branch | Effect in the portion read |
|---|---|---|
| 1 | 253B | Calls 2D40:0EB4 with entry word +4. |
| 2 | 251B | Clears word +6 in the indexed 49-byte combatant record through DS:19C9. |
| 3 | 24B6 | Scans 42 combatant records for a nonzero identifier and a details index equal to entry word +4. Only when none matches, clears word +0E of the indexed 66-byte details record through DS:19C5. |
| 4 | 27A9 | Returns without that field-clear operation. |
| 5 | 2551 | Clears the first word of the indexed 23-byte record through DS:19D1. |

For kinds 1, 2, 3 and 5, execution continues at 256E after this portion.
This finding does not establish the continuation's complete effects.

## Interpretation

The graph records provenance of each descendant reference as a field
selector. Calling its word +6 a previous object number loses the link to
the field descriptor and misreads the parent helper's DS:60ED lookup.
The root entry remains a special case: its word +6 is the original object.

In the deletion switch's read portion, only kind 3 clears the details
record's occupied word, and it does so only after the combatant-reference
scan finds no live reference. Kind 2 clears the combatant identifier instead.
The field-selector correction does not change those branch observations,
or prove that the whole cleanup clears every party box.

## Alternatives

- FND-PARTY-172 incorrectly named graph word +6 as the previous object.
  The producer path above corrects that fact; the old finding's text is
  retained as superseded. Its valid entry widths, kind-switch cases and
  limited character-box interpretation are preserved here.
- Historical citations in EXP-PARTY-001, FND-PARTY-173 and FND-PARTY-214
  concern the deletion uncertainty, kind comparisons or entry width,
  not the wrong word-6 label. No complete_reading or active format/rule
  evidence list depended on that label in the citation audit. Those
  historical records keep their earlier attribution.
- DS/SS aliases, the source field tables, the transient object's other
  writers and allocation/call effects remain unproved. The recorded
  +42/+82 producer path is not proof of admitted native stack storage.
- Remaining graph and position contracts are Q-PARTY-082 and Q-PARTY-085;
  cleanup and map identity remain Q-PARTY-078 and Q-PARTY-081. No native
  experiment or complete-reading declaration is claimed.

## How to reproduce

With the locked evidence Python and installed DSUN.EXE, run
resident_listing.py ranges 31E0:044C..31E0:0461,
2D40:2196..2D40:2410, 2D40:2410..2D40:256E and
1AA0:051C..1AA0:063F. Decode 27AD..27B7 as five little-endian
target words, not instructions. Trace the +42 field and +82 parent
arrays from 05DC through 23B7 to the word-6 store, retaining each
access's actual segment rather than assuming a near pointer names SS.
