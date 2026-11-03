---
id: FMT-SCRIPT-004
title: Script trigger record
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: []
byte_order: little
size: 13
text: false
definition: fmt_script_004.ksy
evidence: [FND-SCRIPT-013, FND-SCRIPT-014, FND-SCRIPT-015, FND-SCRIPT-017, FND-SCRIPT-027, FND-SCRIPT-028, FND-CONFIG-156, FND-SAVE-015, FND-SAVE-021, FND-SAVE-023, FND-SCRIPT-031]
conflicting: []
split_with: []
related: [RULE-SCRIPT-008]
---

## Layout

One of the 200 records of the table at the far pointer `57E0:40C0` in BLD-GOG-EN-1.1, kept only
in memory [FND-SCRIPT-014]. Each is on the free list or on one of the trigger lists, and names a
script entry point that the game runs when the record's test passes [FND-SCRIPT-013,
FND-SCRIPT-015]. The tests read the key fields in different ways for different lists
[FND-SCRIPT-014].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `entry_offset` | Offset of the entry point in the script, or its `GPLI` entry number while overlay 187 has converted it. | supported | FND-SCRIPT-013, FND-SCRIPT-015, FND-SCRIPT-017 |
| `0x02` | 2 | `UINT16LE` | `entry_script` | Number of the `GPL ` resource that holds the entry point. | supported | FND-SCRIPT-013, FND-SCRIPT-015, FND-SCRIPT-017 |
| `0x04` | 2 | `INT16LE` | `key_1` | The object number of a one-key trigger, the first number of a key-pair trigger, or the cell column of a tile trigger or of a box trigger's first column. | supported | FND-SCRIPT-014, FND-SCRIPT-015, FND-SCRIPT-027, FND-SCRIPT-028 |
| `0x06` | 2 | `INT16LE` | `key_2` | The cell row of a tile trigger or of a box trigger's first row, or the second number of a key-pair trigger. In a one-key trigger only the low byte is written: 1 when `MAS ` resource 99 registered it, which keeps it at a region change. | supported | FND-SCRIPT-014, FND-SCRIPT-015, FND-SCRIPT-027, FND-SCRIPT-028 |
| `0x08` | 1 | `UINT8` | `unk_08` | Who may set off a tile trigger (0 the leader, 1 the party, 2 anyone), the width of a box trigger's box, or, in a key-pair trigger, 1 when `MAS ` resource 99 registered it. | supported | FND-SCRIPT-014, FND-SCRIPT-015, FND-SCRIPT-027, FND-SCRIPT-028 |
| `0x09` | 1 | `UINT8` | `unk_09` | The height of a box trigger's box. | supported | FND-SCRIPT-014, FND-SCRIPT-027, FND-SCRIPT-028 |
| `0x0A` | 1 | `UINT8` | `unk_0A` | Who may set off a box trigger (0 the leader, 1 the party, 2 anyone). | supported | FND-SCRIPT-014, FND-SCRIPT-027, FND-SCRIPT-028 |
| `0x0B` | 2 | `INT16LE` | `next` | Index of the next record on the same list, or -1 at its end. | supported | FND-SCRIPT-014, FND-SCRIPT-015 |
| `0x0D` | | | | Total size 13 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read from the code that sets up, walks, fills and converts the records [FND-SCRIPT-014,
FND-SCRIPT-015, FND-SCRIPT-017]; no record was observed in memory.

FND-CONFIG-156 locates allocation in the initializer reached through 565C:0020.
On its nonpositive-status initialization path, the known caller supplies 200;
the initializer requests 200 times thirteen, or 2600 bytes, through its allocation
helper and stores the returned far pointer at 57E0:40C0. The allocation can be
suppressed or fail, and the outer caller's low-byte check does not establish
success. This identifies the allocation route without asserting valid storage
at every later use.

FND-SCRIPT-031 distinguishes the allocator's final FFFF link from the
separate resident reset gated by DS:13FB. Under its buffer/pointer preservation
conditions, that reset fills the current table with FF and writes links i plus
one for records zero through 199, leaving record 199 linked to 200. Its own
following head assignments do not replace that link with FFFF. Which phase
reaches a particular use remains Q-SCRIPT-019; this does not establish a live
access beyond the 200-record pool.

FND-SAVE-021 binds that pointer to save-table entry 38. FND-SAVE-015 gives its
length as 0x0A28, also 2600 bytes, and includes it in the selector-zero resource
loop as SAVE/38. Nonzero selectors cover only entries 1 through 17 and omit
this table. FND-SAVE-023 connects selector-zero conversion of its entry points
to GPLI numbers before the writer's resource phase, reverse conversion afterward,
and reverse conversion after the reader's resource phase, including failed-read
continuations. These call and transfer contracts do not prove archive success,
current capacity or pointer lifetime.

## Open questions

- Which admitted allocation, failure, replacement and restore states leave valid
  storage for all 200 records when they are used (Q-SCRIPT-019). The concrete
  allocation and save bindings above do not establish live capacity or lifetime.
- Whether additional incoming routes reach the entry-point conversions outside
  the known save writer and reader calls (Q-SCRIPT-020). FND-SCRIPT-030's
  adjacent relocated-pair result does not exclude computed, unrelocated,
  split or runtime-constructed targets.
