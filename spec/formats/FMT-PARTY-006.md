---
id: FMT-PARTY-006
title: Character record chunk
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF", "CD:CHARSAVE.GFF"]
byte_order: little
size: null
text: false
definition: fmt_party_006.ksy
evidence: [FND-PARTY-051, FND-PARTY-052, FND-PARTY-054, FND-PARTY-181, FND-PARTY-182, FND-PARTY-196, FND-PARTY-216, FND-PARTY-217, FND-PARTY-220, FND-PARTY-221, FND-PARTY-222, FND-PARTY-223, FND-PARTY-224, FND-PARTY-225, FND-PARTY-227, FND-PARTY-228, FND-PARTY-229, FND-PARTY-230, FND-PARTY-231, FND-PARTY-234, FND-PARTY-235, FND-PARTY-236, FND-PARTY-238, FND-PARTY-239, FND-PARTY-245, FND-PARTY-248, FND-PARTY-247, FND-PARTY-249]
conflicting: []
split_with: []
related: []
---

## Layout

One link of the chain an FMT-PARTY-001 character record is made of: a 10-byte header and the
data its `len_data` gives. The load reads chunks one after the other until one whose `chunk_type`
is 0xFF, numbering them from 0 in that order, and handles each by its `chunk_type`
[FND-PARTY-051]. The game writes the chunks depth first from the character, giving each the
index of its parent chunk, its record number and the field it was reached through [FND-PARTY-054].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `chunk_type` | How the load handles the chunk: 1 to 4 (see below); 0xFF ends the record; any other value makes the load fail with -2. | supported | FND-PARTY-051 |
| `0x01` | 1 | `UINT8` | `target_chunk` | For types 2, 3 and 4, the number of the earlier chunk whose object the chunk attaches to. The load does not read it for type 1; in the shipped records the first chunk and the end header hold the number of chunks there. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x02` | 1 | `UINT8` | `kind` | The kind of record the data is: 2 for type 1, 3 for type 3, 1 for types 2 and 4 in every shipped record. Overlay 187 copies the data into the table of that kind. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x03` | 1 | `UINT8` | `unk_03` | Purpose unknown. 0 in every shipped record; a type-3 chunk is copied only when the word at `0x02` is 3. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x04` | 2 | `UINT16LE` | `record_ref` | Types 2, 3 and 4: replaced, in the loaded copy, by the number of the record the data was copied into, which a type-3 chunk then stores in its target's field and a type-4 chunk links in. The load does not read it for type 1. | supported | FND-PARTY-051 |
| `0x06` | 2 | `UINT16LE` | `field` | Types 2 and 3: the number of the field of the target chunk's object that receives the new record, through the `FNFO` field tables. 15, 16, 17 or 4 in the shipped records' chunks of these types, and 2 in their type-4 chunks, for which the load does not read it. In a chunk the game writes, the field the walk reached the record through: 4, 16 or 17 for type 2, 15 (5 only under a kind-5 first chunk) for type 3, 2 for type 4 and the slot for type 1, never 6. | supported | FND-PARTY-051, FND-PARTY-052, FND-PARTY-235 |
| `0x08` | 2 | `UINT16LE` | `len_data` | Number of data bytes after the header: 49 for type 1, 66 for type 3, 23 for types 2 and 4, and 0 in the end header, in every shipped record. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x0A` | `len_data` | `BYTE[len_data]` | `data` | The record the chunk holds (see below). For types 2 and 4, a 23-byte record of the table at `57E0:19C1`: its signed word at `0x00` is an object number, negated, which placing the record's object passes on as the number of the `OJFF` and `RDFF` resources to load, and which is negative and the negation of an `OBJEX.GFF` `RDFF` number in every shipped record; its byte at `0x12` gives bits 0 to 2 of the placed object's entry. In the shipped records its word at `0x04` is 9999 in each record that ends its chain and another value in each record a type-4 chunk follows, its word at `0x08` is 9999 in all but three, and its byte at `0x15` is 0. | supported | FND-PARTY-051, FND-PARTY-181, FND-PARTY-182, FND-PARTY-196 |
| | | | | Total size `10 + len_data` | | |

For a 23-byte kind-1 attached record, the signed little-endian word at
`0x02` is `quantity` under the shipped field descriptors. The placement
reader treats a nonpositive value as one; a partial split writes the moved
amount on the new object and the remainder on the source object. This
does not restrict the stored word to positive values or specify other
quantity consumers [FND-PARTY-216].

Word `0x06`, kind-1 field 3, is `value`, read unsigned by every consumer that widens it, so
the shipped -30536 is 35000. For the money objects 900 to 910 a pickup adds it, times the
quantity when the item's 15-byte entry at the far pointer `DS:19CD` has bit 1 of its byte `0x0B`
set, to the party's money and prints the amount. A store bases what it charges and what it
offers on it, and a value of 9999 is an item the shopkeeper will not buy. The item details
window prints it unless it is 0 or 9999. Script functions 48 and 49 of opcode `0x22` read and
write it as field 3 of a kind-1 object [FND-PARTY-245].

Under the shipped kind-1 field descriptors, word `0x0A` (field 5) is `item_type`, the number of
the item's 15-byte entry in the table at the far pointer `DS:19CD`, with 9999 for none; word
`0x14` (field 11) is `name_number`, a line of `TEXT/1000`, whose high byte `0x15` is 0 in every
shipped record; and byte `0x16` (field 12) is `bonus`, a signed number the item's name shows as
`+n` or `-n` and that combat code adds or tests for a negative value.
In the reviewed action-completion path, a nonzero low-byte action argument
and item kind 3 first restore a word from `0x10`: to `0x0E` when the item
type is below 9999 and bit 1 of its type record's byte `0x0B` is set,
otherwise to quantity at `0x02`. After returning, the type-flag branch
with action argument 2 skips the subsequent decrement; other paths
decrement byte `0x0E` and pass the item onward when the whole word is zero.
This copy-before-decrement order does not establish a persistent uses-left
counter or the meaning of every action argument [FND-PARTY-248].

Runtime word `0x0C` is `cached_bitmap_number` in the reviewed inventory
drawing path. A nonzero word is reused as an unsigned BMP resource number.
Zero invokes a resolver, after which the selected record is reconstructed
and the returned word is stored before the BMP request. A zero resolver
result is still requested; it is not locally replaced with a placeholder.
Cache identity across the child, all producers and invalidation, and actual
image acquisition remain open [FND-PARTY-224].

The resolver requests OJFF using the 16-bit negation of record word
`0x00`, sign-extended to a doubleword. It offers a sixteen-byte local
buffer and returns zero for any nonzero returned status word; for zero
status it returns that original local buffer's image word at `0x0C`,
as laid out by FMT-ACTOR-001. Actual transfer into that buffer and
selection preservation remain prerequisites [FND-PARTY-225].

The resolver's nonnull offered data pointer selects existing-buffer reuse
in the full-resource reader. The selected resource length supplies the
transfer count; the reader receives no independent sixteen-byte capacity.
A selected unchanged shipped OJFF has that sixteen-byte extent, while
actual archive selection, aliasing and OS transfer remain open
[FND-PARTY-231].

The current-item image-number helper also reuses a nonzero cache and
resolves/stores a zero cache, including a zero result. It returns 19103
without a record read when its separate selection word is FFFF;
that fallback does not replace a zero resolver result. Actual selection
and record identity remain prerequisites [FND-PARTY-227].

The inventory opener separately resolves/stores a cache equal to exactly
9999 before calling the current-item helper. A zero result may consequently
trigger a second resolver request there under stable selection state.
Complete sentinel producers and native branch admission remain open
[FND-PARTY-228].

One item-selection path instead captures an object-slot image word before
a placed-entry removal request, rebuilds a graph and copies that saved
word into its first kind-1 record's cache. Its local store rejects neither
zero nor 9999. Complete transfer/graph/slot admission, selection preservation
and later effects remain open [FND-PARTY-229].

Shipped kind-1 field 6 selects this cache word at offset `0x0C`,
with type 2 and initial copy width 2. The generic setter uses the current
width table; runtime bindings and copy outcomes remain open [FND-PARTY-230].
No generic field-accessor request the game makes selects field six: no
direct call, wrapper, script operand or chunk header names it, so the cache
word is read and written only through direct accesses [FND-PARTY-234,
FND-PARTY-235, FND-PARTY-238, FND-PARTY-239].

A numbered save writes the whole kind-1 table, 400 records of 23 bytes with
each record's `0x0C`, as `SAVE` resource `selector * 61 + 7`, and the numbered
restore reads that resource back over the table and then sets `0x0C` to zero
in all 400 records, on its success and failure paths alike [FND-PARTY-236].

Byte `0x13` is `inventory_position`, a position code interpreted as a
signed byte and increased by four to select a logical inventory-map word.
The main inventory draw loop consumes positions 4 through 29; other
branches use positions 42 through 47. A byte of `0xFF` maps to position 3,
outside that main draw loop [FND-PARTY-217]. The inventory recipient-space
check marks occupied codes 14 through 25, selects the first unused code
and reports no space when all twelve are occupied. Its caller associates
that failure with backpack capacity. These codes project to logical draw
indices 18 through 29 [FND-PARTY-220]. Their rectangle requests select
`APFM/11230` through `APFM/11241` in `WIND/13500`. The shipped origins are
column-first: six at x=186 and six at x=204, each column's y values 18,
36, 54, 72, 90 and 108. These are source window-relative coordinates;
loaded control state, full image placement and other position labels remain
open [FND-PARTY-222].

Codes zero through thirteen select logical indices four through seventeen
and `APFM/11216` through `APFM/11229`. Their source origins are listed in
SCR-UI-008; this maps their positions without assigning individual equipment
names [FND-PARTY-223].

The reviewed recipient-transfer route supplies target graph entry zero and
field four, which the shipped kind-2 descriptors bind to the combatant's
head word at `0x0C`. It later requests field ten with the selected position.
For kind 1, shipped field ten addresses `0x13` with type zero. The setter
copies the current runtime width for that type: setup assigns width one,
whereas the executable's initial width is two. The low-byte position write
therefore depends on reaching and retaining that setup assignment, admitted
descriptor/record storage and the called helpers' effects. Neither this
route nor its success byte establishes a complete native list transfer
[FND-PARTY-221].

## Enumerations and flags

### `chunk_type`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 1 | `CHUNK_COMBATANT` | Copies 49 bytes into the slot's FMT-COMBAT-001 record. | supported | FND-PARTY-051 |
| 2 | `CHUNK_ATTACHED_RECORD` | Copies 23 bytes into a new record of the table at the far pointer `57E0:19C1` and stores its handle in the field `field` names of the object of `target_chunk`. | supported | FND-PARTY-051 |
| 3 | `CHUNK_DETAILS` | Copies 66 bytes into an FMT-COMBAT-002 record, for a slot of 4 or less the slot's own, and stores that record's number in the field `field` names of the object of `target_chunk`. | supported | FND-PARTY-051 |
| 4 | `CHUNK_CHAINED_RECORD` | Copies 23 bytes into a new record of the table at `57E0:19C1` and links it after the record of `target_chunk` through the records' words at `+0x04`. | supported | FND-PARTY-051 |
| 0xFF | `CHUNK_END` | Ends the record. | supported | FND-PARTY-051 |

The fields the shipped records name are, by the `FNFO` 1 resource of `OBJEX.GFF`: field 15 of a
kind-2 object is FMT-COMBAT-001's `details_index` at `0x04`; fields 16, 17 and 4 of a kind-2 object
are its words at `0x08`, `0x0A` and `0x0C`; field 4 of a kind-1 record is its word at `0x08`
[FND-PARTY-052]. That word heads the chain of items stored inside the item, 9999 when it holds
none; in the shipped records only the items named `Chest` (item type 19, name 102) hold any. The
game reaches the contents only through its object-graph walk, and the inventory redraw keeps them
out of the character's map positions 1 to 29 by setting their position byte to -1 [FND-PARTY-247].

## Differences between builds

None known.

## Coverage

The chunks of all 19 `CHAR` resources of the installed `CHARSAVE.GFF`: each walk from offset 0
ends on a `chunk_type` of 0xFF whose header is the last 10 bytes of the resource [FND-PARTY-052].

Two other local graph walkers select kind-1 entries whose ancestor query
returns 16 and whose word `0x10` is exactly 313, and pack word `0x0E` from
separate bytes. One keeps its low byte and writes a bounded helper result
in the high byte. The other clears the high byte and, on a zero helper
result, conditionally decrements the low byte after `roll_sum(1, 100)`.
Neither preserves the incoming high byte. These are qualified local
writer effects, with action/consumer meaning unresolved. For descendants
the query returns the selector immediately under a kind-2 ancestor;
for root index zero it instead returns the root object number
[FND-PARTY-249].

## Open questions

- What the word at `0x10` (field 8) of a 23-byte record holds;
  FND-PARTY-196 gives the shipped values and FND-PARTY-248 the accesses
  (Q-PARTY-020).
- Gameplay meaning, admitted inputs and later consumption of the packed
  bytes written by overlay 197 (FND-PARTY-249, Q-PARTY-106).
- Remaining direct readers and writers of the bitmap-number cache
  (FND-PARTY-224, Q-PARTY-087).
- Which record creations and whole-record copies set the cache word, and to
  what, beyond the CHAR load and the numbered restore, which both leave zero
  (FND-PARTY-051, FND-PARTY-236, Q-PARTY-091).
- Resource/buffer admission for the cache resolver and image acquisition
  (FND-PARTY-225, Q-PARTY-088).
- Cache-specific runtime table/field/width bindings and record-selection
  preservation across child calls (FND-PARTY-227, FND-PARTY-229,
  FND-PARTY-230, Q-PARTY-089).
- Which lists the character's words at `0x08` and `0x0A`, fields 16 and 17, head
  (Q-PARTY-082). Generic graph order is recorded in FND-PARTY-054; it does not assign those
  labels.
- Which screens and actions other than the recipient transfer read or select the field-4 head
  at `0x0C`; FND-PARTY-221 identifies field four as that transfer's target (Q-PARTY-097).
- Which character-screen actions change the kind-2 field-16, field-17 and
  field-4 heads, including chain preservation, failures and later consumers
  (Q-PARTY-086). Graph/root/storage admission is Q-PARTY-099.
- Which action opens a container item, shows its contents or moves items into or out of it
  (FND-PARTY-247, Q-PARTY-083).
- Complete quantity producer/consumer coverage and runtime field/table binding
  stability, beyond the placement use FND-PARTY-216 reads (Q-PARTY-084).
- Where an item with a position outside the main draw loop's 4 to 29 appears, and whether
  anything moves the loaded rectangles of codes 0 to 25, whose source origins FND-PARTY-222 and
  FND-PARTY-223 give (FND-PARTY-217, Q-PARTY-085).
- Which code writes the position byte, beyond the recipient-space scan (FND-PARTY-220) and the
  recipient transfer's conditional field-ten request (FND-PARTY-221) (Q-PARTY-098).
- Which graph and current object the projection reads, and its map capacity, direction flag,
  parent traversal, mode bytes and recursive `+146B` calls (FND-PARTY-217, Q-PARTY-099).
- What the inventory's item draw requests put on the screen, beyond the local placement
  branches FND-PARTY-226 reads (Q-PARTY-100).
