---
id: FMT-ITEM-002
title: Shared item-type record in ALL/4
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["OBJEX.GFF"]
byte_order: little
size: 15
text: false
definition: fmt_item_002.ksy
evidence: [FND-ITEM-011, FND-ITEM-012, FND-ACTOR-017, FND-ACTOR-018]
conflicting: []
split_with: []
related: []
---

## Layout

The initializer offers ALL/4 as the shared table at DS:19CD and publishes
its pointer as the kind-four generic-accessor base. Native item-type consumers
use a fifteen-byte stride. The installed resource contains 1530 bytes, or
102 such logical records (FND-ACTOR-017).

The raw structural partition is supported. Neutral field names preserve
storage independently of the interpretations of particular consumers.
No runtime decoder or complete semantic contract is claimed.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `byte_00` | Raw byte at generic selector 87; local mask uses below. | supported | FND-ITEM-011, FND-ITEM-012 |
| `0x01` | 1 | `UINT8` | `byte_01` | Raw byte at generic selector 88; remaining interpretations stay open. | supported | FND-ITEM-011 |
| `0x02` | 1 | `UINT8` | `byte_02` | Raw byte at generic selector 89; contributes to the guarded heavy classification below. | supported | FND-ITEM-011, FND-ITEM-012 |
| `0x03` | 1 | `UINT8` | `byte_03` | Raw byte at generic selector 90; remaining interpretations stay open. | supported | FND-ITEM-011 |
| `0x04` | 1 | `UINT8` | `byte_04` | Raw byte at generic selector 91; material nibble and local label flag uses below. | supported | FND-ITEM-011, FND-ITEM-012 |
| `0x05` | 1 | `UINT8` | `byte_05` | Raw byte at generic selector 92; remaining interpretations stay open. | supported | FND-ITEM-011 |
| `0x06` | 1 | `UINT8` | `byte_06` | Raw byte at generic selector 93; remaining interpretations stay open. | supported | FND-ITEM-011 |
| `0x07` | 1 | `UINT8` | `byte_07` | Raw byte at generic selector 94; remaining interpretations stay open. | supported | FND-ITEM-011 |
| `0x08` | 1 | `UINT8` | `byte_08` | Raw byte at generic selector 95; remaining interpretations stay open. | supported | FND-ITEM-011 |
| `0x09` | 1 | `UINT8` | `byte_09` | Raw byte at generic selector 96; remaining interpretations stay open. | supported | FND-ITEM-011 |
| `0x0A` | 1 | `UINT8` | `byte_0a` | Raw byte at generic selector 97; signed uses require explicit consumer interpretation. | supported | FND-ITEM-011 |
| `0x0B` | 1 | `UINT8` | `byte_0b` | Raw byte at generic selector 25; guarded label flags below. | supported | FND-ITEM-011, FND-ITEM-012 |
| `0x0C` | 2 | `UINT16LE` | `word_0c` | Raw word at generic selector 40; remaining interpretations stay open. | supported | FND-ITEM-011 |
| `0x0E` | 1 | `UINT8` | `byte_0e` | Raw byte at generic selector 56; signed type contribution in the armor-bonus display below. | supported | FND-ITEM-011, FND-ITEM-012 |
| | | | | Total size 15 | | |

The installed generic descriptor mapping is known (FND-ITEM-011). Selectors
87 through 97 address offsets zero through ten respectively. Selector 25
addresses byte eleven, selector 40 the little-endian word at twelve, and
selector 56 byte fourteen. Metadata is one for these byte fields except
offsets ten and fourteen, whose metadata is zero; the word's metadata is
three. With the documented width setup these fields cover all fifteen bytes.
The definition retains byte patterns as unsigned storage. The native getter
and direct consumers can interpret them differently; a signed-byte use converts
values at least 128 by subtracting 256. The layout's raw types do not replace
those consumer transformations. Direct consumers can address interior bytes.

## Enumerations and flags

### byte_04

The low nibble selects material text in the known detail renderer:

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0 | `MATERIAL_WOOD` | Wood. | supported | FND-ITEM-012 |
| 1 | `MATERIAL_BONE` | Bone. | supported | FND-ITEM-012 |
| 2 | `MATERIAL_STONE` | Stone. | supported | FND-ITEM-012 |
| 3 | `MATERIAL_OBSIDIAN` | Obsidian. | supported | FND-ITEM-012 |
| 4 | `MATERIAL_METAL` | Metal. | supported | FND-ITEM-012 |
| 5 | `MATERIAL_LEATHER` | Leather. | supported | FND-ITEM-012 |
| 0x40 | `LOCAL_MATERIAL_GATE` | Independent raw-byte mask suppressing material text; the hand description still requires its other predicates. | supported | FND-ITEM-012 |

Values zero through five apply to the low nibble; 0x40 is a separate mask
of the raw byte. The codes are the installed source indices, not a bound enforced by the native
nibble lookup; indices six through fifteen remain unexamined. The definition
derives the material value without adding a validity restriction.

### byte_00

These names describe the known detail renderer's predicates, not universal
meanings in every consumer.

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0x01 | `LOCAL_HEAVY_GATE` | Required by the heavy-label predicate, with its other guards below. | supported | FND-ITEM-012 |
| 0x13 | `LOCAL_DESCRIPTION_MASK` | Any set bit satisfies the first part of the hand-description predicate. | supported | FND-ITEM-012 |

### byte_0b

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0x40 | `LOCAL_HAND_GATE` | Required together with byte zero mask 0x13 for the hand description; suppresses the heavy label. | supported | FND-ITEM-012 |
| 0x80 | `LOCAL_ARMOR_GATE` | Selects the armor-bonus calculation below. | supported | FND-ITEM-012 |

### Combined local uses

The same renderer uses byte four bit 0x40, byte zero mask 0x13 and byte
eleven bit 0x40 together for its material and two-handed description. With
byte four bit 0x40 clear it shows material, prefixed by the hand description
only when both other tests are true. With that bit set it shows only the hand
description when both other tests are true, otherwise neither description.

Its heavy label requires byte zero mask 0x01, byte eleven bit 0x40 clear and
unsigned byte two greater than thirty.
This binds a displayed classification, not a physical unit or carrying rule.

With byte eleven bit 0x80 set, it formats an armor-class bonus from signed
byte fourteen plus the attached item's signed byte 0x16 (FND-ITEM-012).
Combat's effect-dependent calculation is separately recorded in
FND-COMBAT-068. Other flag domains and the complete field contract remain open.

## Differences between builds

None known.

## Coverage

The installed OBJEX.GFF ALL/4 directory span and conditional native base/stride
binding (FND-ACTOR-017). Its structural partition is described above;
complete semantic reader/writer coverage remains open.

The 1216 known shipped inline kind-four payloads duplicate their indexed
shared rows, with duplicate-row ambiguity retained (FND-ACTOR-018). This
source relationship does not supply unexamined field meanings.

FND-ITEM-011 reads the complete 98-selector descriptor row and all 102
shared records under that structural partition. Semantic reader/writer
coverage remains incomplete.

FND-ITEM-012 binds the six source material labels through declared MZ
relocations and records the guarded detail-renderer meanings above. It does
not establish all other consumers or the live table's admission/lifetime.

## Open questions

- The unexamined field purposes, flag domains and native transformations,
  independent consumers and complete reader/writer coverage (Q-ITEM-005).
  The raw layout and local bindings above do not establish those meanings
  or a complete reading of every consumer.
