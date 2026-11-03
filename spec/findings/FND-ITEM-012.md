---
id: FND-ITEM-012
title: Item details bind six material indices and guarded hand, heavy and armor-bonus text to shared type fields
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00099E5B..0x0009A514
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00045900..0x00045918
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004EA36..0x0004EA5F
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001F291..0x0001F88B
tool: overlay_listing.py and tools/evidence/report.mjs table/operand at b7c726cc; pinned executable-reader 4.3.0 and Capstone module 5.0.7
environment: null
---

## Observation

This extends Q-ITEM-005 using FND-ITEM-011's structural mapping and retained
source-row reports. FND-PARTY-248 supplies the attached item's type index
and signed bonus byte; FND-ACTOR-017 supplies the shared table binding.
FND-PARTY-245 already records the value-line branch. FND-RNG-043 already
records type byte three's damage/removal comparison; that is not a new finding.

The complete current inventory body for overlay 213 +0BFB is
0x99E5B..0x9A514, ending in a far return at 0x9A513. The preliminary
listing through 0x9A719 also includes three adjacent helpers and is not
the renderer's boundary. The following are local source-to-text bindings,
subject to valid tables and retained item/type identity across child calls.
They do not claim every child effect or a native pixel observation.

### Material index and source domain

At +0EDE..+0EF1 and +0F27..+0F3A, the renderer zero-extends type
byte four, masks its low four bits, multiplies by four and reads a far
pointer from 5070:0000 plus that offset. The selected string supplies
the material part of the formatted label.

All 102 installed rows use low-nibble indices zero through five. The
six independently resolved pointer entries and row counts are:

| Index | Material concept | Installed rows | Loaded string pointer |
|---|---|---:|---|
| 0 | Wood | 53 | 57E0:1A36 |
| 1 | Bone | 14 | 57E0:1A3D |
| 2 | Stone | 6 | 57E0:1A42 |
| 3 | Obsidian | 8 | 57E0:1A48 |
| 4 | Metal | 14 | 57E0:1A51 |
| 5 | Leather | 7 | 57E0:1A57 |

The segment words at file offsets 0x45902, 0x45906, 0x4590A,
0x4590E, 0x45912 and 0x45916 are declared MZ relocations. Each raw
segment 47E0 resolves to loaded 57E0, and each referenced label terminates
within the separately bounded 32-byte read. The six-entry source query is
bounded by the observed index domain, not proof of the table's full capacity.
The native mask admits indices six through fifteen without a local range
test; those states and their targets are not interpreted here.

### Hand/material label selection

Under the surrounding unsigned type-index-below-9999 guard, let A mean
type byte four has bit 0x40, and H mean both that type byte zero has any
bit in mask 0x13 and type byte eleven has bit 0x40. The branches
+0E63..+0F72 select:

| A | H | Submitted label contents |
|---|---|---|
| clear | false | Material only |
| clear | true | Two-handed prefix and material |
| set | true | Two-handed description only |
| set | false | No hand/material line |

The common line submission calls 2C5F:09F3 and advances its local line
position by seven. This records the combined predicates; neither bit 0x40
is assigned a universal meaning independently of its record and guards.
FND-EXE-616's equipment helper is a separate consumer of the type-eleven
bit in its hand count.

### Heavy label

At +1178..+11F6 the renderer requires the unsigned type index below
9999, bit zero of type byte zero set, bit 0x40 of type byte eleven
clear, and type byte two greater than thirty using an unsigned byte
comparison. It then submits the heavy label through 2C5F:09F3.
Equality to thirty does not submit it. Icon-dependent local layout chooses
the line position; this predicate does not itself define an inventory
restriction, a physical unit or all uses of byte two.

### Armor-class bonus

At +1234..+129B a type below 9999 with bit 0x80 of type byte eleven
set reads type byte fourteen and attached-item byte 0x16. Each is
sign-extended from byte to word, and their word sum is formatted as the
armor-class bonus and submitted through 2C5F:09F3. No effect-state test
or clamping intervenes in this local calculation. This is distinct from
the combat calculation's effect-dependent omission of the item bonus
(FND-COMBAT-068).

The prefix-free extension instruction at +125B and +126D is interpreted
at the declared 16-bit operand size. The pinned Capstone printer calls it
CWDE even in a synthetic 16-bit control; that spelling is not evidence of
a 32-bit extension. The following pushes and addition consume AX and DX.

## Interpretation

The six shipped low-nibble material identities are direct string-table
bindings, beyond FND-COMBAT-066's earlier circumstantial material reading.
Byte two contributes to an explicitly named heavy classification under
the stated flags. Signed byte fourteen contributes the type's armor-class
bonus, with the attached item's signed bonus added for this display route.
The two bit-0x40 tests control different parts of the hand/material label.

These settle the named display bindings, not every semantic reader/writer
or all flag domains. Q-ITEM-005 retains those obligations. No complete
reading or gameplay change is introduced; no original game ran. Together
with FND-ITEM-011 this supports the raw structural format and named local
bindings, while leaving its semantic completeness open.

## Alternatives

- Treating indices zero through three as unnamed penalties is unnecessary
  for the shipped table after the direct label resolution. The display
  does not by itself prove every attack routine consumes the same live table.
- Heavy is not established as a synonym for physical weight, nor does this
  display alone prove a carrying or equipment rule. Those require consumers.
- The armor label does not make the displayed sum identical to effective
  armor class in every effect state. FND-COMBAT-068 describes a different gate.
- The complete semantic schema remains open. Higher material indices,
  mutable pointers, table aliases and unexamined consumers are not normalized
  into the observed source domain or excused by its six labels.

## How to reproduce

Use installed DSUN.EXE size 634416, XXH3 e296af55ba2ecde7e77f555c90f33d0b,
and FND-ITEM-011's installed OBJEX.GFF identity and complete row report.
Run `overlay_listing.py DSUN.EXE 213 99E5B 9A514` and check the entry,
joined branches and return. Preserve the earlier wider listing as preparation.

Use the committed table reporter with six rows at file 0x45900, stride four,
width-two offset and segment fields. For each segment-word site listed above,
use the operand reporter with loadSegment 4096 and that row's targetOffset;
require declared MZ relocation and the resolved pointer/file mapping.
Read each resolved label with count one, stride 32 and eight width-four
fields; decode ASCII only through the first NUL, requiring it within the bound.
Group FND-ITEM-011's 102 type-byte-four values by their low nibble for the
source counts. This is a source-domain measurement, not an admission census.

Configs and reports remain in GAME_DIR/analysis/item-type-fields-20261103/.
Repeated material-pointer reports have SHA-256
a79988a64d790602a638b5c7fef23154be3569460a625854d53bf4af6c4f9e30.
The exact renderer listing has SHA-256
2f25f35610c163ba616ce3106955fad454a8d1596b582948e2e1a99d0a6f41c3.
The material-operand and bounded material-label configs reproduce the
pointer/string checks. No original string arrays, bytes or listings are committed.
