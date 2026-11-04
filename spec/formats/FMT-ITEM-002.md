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
evidence: [FND-ITEM-011, FND-ITEM-012, FND-ACTOR-017, FND-ACTOR-018, FND-COMBAT-065, FND-COMBAT-067, FND-PARTY-100, FND-RNG-043, FND-EXE-616, FND-PARTY-085, FND-PARTY-248, FND-PARTY-252]
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
| `0x00` | 1 | `UINT8` | `byte_00` | Raw byte at generic selector 87; local display and property-mask uses below. | supported | FND-ITEM-011, FND-ITEM-012, FND-PARTY-252 |
| `0x01` | 1 | `UINT8` | `byte_01` | Generic selector 88. Initial hit-word mask in the armed attack path, before magic-level bits are added. | supported | FND-ITEM-011, FND-COMBAT-065 |
| `0x02` | 1 | `UINT8` | `byte_02` | Raw byte at generic selector 89; contributes to the guarded heavy classification below. | supported | FND-ITEM-011, FND-ITEM-012 |
| `0x03` | 1 | `UINT8` | `byte_03` | Generic selector 90. State-one object's removal threshold: the recorded damage routine queues removal when positive incoming damage is greater. | supported | FND-ITEM-011, FND-RNG-043 |
| `0x04` | 1 | `UINT8` | `byte_04` | Raw byte at generic selector 91; material nibble, local label and image-cache gates below. | supported | FND-ITEM-011, FND-ITEM-012, FND-PARTY-252 |
| `0x05` | 1 | `UINT8` | `byte_05` | Generic selector 92. Category compared with the per-position codes of the automatic equipment helper. | supported | FND-ITEM-011, FND-EXE-616 |
| `0x06` | 1 | `UINT8` | `byte_06` | Generic selector 93. Weapon selection distinguishes one from values above one; a ranged attack requires this value at least its distance argument. | supported | FND-ITEM-011, FND-COMBAT-065 |
| `0x07` | 1 | `UINT8` | `byte_07` | Generic selector 94. Item rate source when the swing's distance argument is above one; otherwise the armed path uses the attacker's details rate. | supported | FND-ITEM-011, FND-PARTY-100, FND-COMBAT-067 |
| `0x08` | 1 | `UINT8` | `byte_08` | Generic selector 95. Damage die sides supplied by the item path of the recorded swing. | supported | FND-ITEM-011, FND-PARTY-100, FND-COMBAT-067 |
| `0x09` | 1 | `UINT8` | `byte_09` | Generic selector 96. Damage die count supplied by the same path. | supported | FND-ITEM-011, FND-PARTY-100, FND-COMBAT-067 |
| `0x0A` | 1 | `UINT8` | `byte_0a` | Generic selector 97. Read as a signed damage addend in the item swing before other terms; raw storage remains unsigned here. | supported | FND-ITEM-011, FND-PARTY-100, FND-COMBAT-067 |
| `0x0B` | 1 | `UINT8` | `byte_0b` | Raw byte at generic selector 25; guarded display, completion and classification uses below. | supported | FND-ITEM-011, FND-ITEM-012, FND-PARTY-248, FND-PARTY-252 |
| `0x0C` | 2 | `UINT16LE` | `word_0c` | Generic selector 40. Automatic equipment requires a shared bit with the combatant details class-flags word. This is one eligibility predicate, not the complete equipment rule. | supported | FND-ITEM-011, FND-EXE-616, FND-PARTY-085 |
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

The purposes above reconcile existing native findings. FND-COMBAT-067
records later rate scaling, ammunition limits, damage terms and effect gates;
the source bytes alone are not complete combat formulas. FND-EXE-616 records
the automatic equipment helper's additional position, hand and sum checks.
Its class-flags overlap does not settle every manual class restriction or
every other placement route. FND-PARTY-100 is the current successor of
FND-PARTY-092; its bounded negative search is not a universal absence claim.

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
| 0x20 | `LOCAL_IMAGE_CACHE_GATE` | When clear, the qualified property caller adds result bit 0x01 and may fill a zero item bitmap cache from its placed slot; when set it skips that branch. | supported | FND-PARTY-252 |
| 0x40 | `LOCAL_MATERIAL_GATE` | Independent raw-byte mask suppressing material text; the hand description still requires its other predicates. | supported | FND-ITEM-012 |

Values zero through five apply to the low nibble; 0x20 and 0x40 are separate masks
of the raw byte. The codes are the installed source indices, not a bound enforced by the native
nibble lookup; indices six through fifteen remain unexamined. The definition
derives the material value without adding a validity restriction.

### byte_00

These names describe the recorded local predicates, not universal meanings
in every consumer.

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0x01 | `LOCAL_HEAVY_GATE` | Required by the heavy-label predicate, with its other guards below. | supported | FND-ITEM-012 |
| 0x13 | `LOCAL_DESCRIPTION_MASK` | Any set bit satisfies the first part of the hand-description predicate and separately assigns property result 0x08 before later overrides. | supported | FND-ITEM-012, FND-PARTY-252 |

### byte_0b

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0x01 | `LOCAL_DOOR_CLASSIFICATION_GATE` | The qualified caller tests the door range; a zero result ORs 0x10 into its result, while nonzero replaces it with 0x10. | supported | FND-PARTY-252 |
| 0x02 | `LOCAL_COMPLETION_VALUE_GATE` | Selects the local completion restore/decrement branch below; separately contributes property result 0x08 when quantity is nonzero. | supported | FND-PARTY-248, FND-PARTY-252 |
| 0x08 | `LOCAL_PROPERTY_40_GATE` | The qualified property caller ORs 0x40 into its local result. | supported | FND-PARTY-252 |
| 0x40 | `LOCAL_HAND_GATE` | Required together with byte zero mask 0x13 for the hand description; suppresses the heavy label. | supported | FND-ITEM-012 |
| 0x80 | `LOCAL_ARMOR_GATE` | Selects the armor-bonus display and separately assigns property result 0x08 before later overrides. | supported | FND-ITEM-012, FND-PARTY-252 |

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

The completion helper's bit-0x02 test selects a full-word copy from the attached
item's word 0x10 to its word 0x0E when the unsigned type index is below 9999
and the bit is set. Otherwise it copies to quantity word 0x02. For nonzero low-byte action arguments in the recorded
kind-three completion path, the helper runs before the conditional low-byte
decrement; argument 2 bypasses that decrement when the same type flag is set.
These local uses do not establish a universal charge or stackability flag,
all action meanings, or stable identity across other calls (FND-PARTY-248).

The classifier's byte-zero mask 0x13 and type-byte-eleven bits 0x02/0x80
can assign 0x08, but a later admitted name-number test replaces its result
with 0x80. Its caller can replace that result again in the door branch.
The caller's type-byte-four bit-0x20-clear path copies the placed slot's word
0x19 into an item bitmap cache only when that cache word equals zero; it
retains a nonzero cache. Its item/type work is guarded by the recorded kind
and unsigned-below-9999 checks. These checks are not proofs of table capacity,
resource validity or all incoming domains. Result consumers and admitted
inputs remain Q-PARTY-108; independent cache production/identity remain
Q-PARTY-087 and Q-PARTY-089 (FND-PARTY-252). No new player-visible action rule
or complete reading follows from these local flag bindings.

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

- Other flag domains, native transformations and field uses beyond the recorded
  attack, display, equipment, completion, classification and cache branches,
  independent consumers and complete reader/writer coverage (Q-ITEM-005).
  The raw layout and local bindings above do not establish those meanings
  or a complete reading of every consumer.

- FND-ITEM-048 adds guarded placement uses of byte-five categories, byte-four
  masks 10/20 hex, byte-eleven masks 08/40 hex, byte-zero masks 01/04 hex,
  byte-two unsigned threshold 30 and word +0C any-bit class overlap. These
  consumer bindings do not close independent reader/writer or admission
  coverage (Q-ITEM-005).

- FND-ITEM-050 bounds the canonical byte-two distribution and conditional
  signed-byte field-89 samples. Runtime width/table/row preservation and
  independent semantic consumers remain Q-ITEM-005; this source bound is
  not a bound on every live shared record.

- FND-ITEM-053 records all 102 source class masks and their bounded producer
  composition. Runtime retention and independent reader/writer contracts
  remain Q-ITEM-005; a source mask is not an immutable live eligibility rule.
