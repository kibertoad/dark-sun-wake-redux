---
id: FND-PARTY-221
title: The recipient-transfer route targets field four and conditionally writes position through field ten
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007C93B..0x0007CBB7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:288C..2D40:2B6A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2B6A..2D40:2C87
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:286C..2D40:288C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:012A..1AA0:0209
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073B7C..0x00073B9E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D40C..0x0004D40D
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF5C..0x0001FF5E
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00020326..0x00020327
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py and trampoline_target.py; tools/evidence/report.mjs table; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-220 identifies the inventory character-recipient route to
572F:0048, descriptor 191 +074B. This entry accepts a signed recipient
word only from zero through three. It lists the object at DS:1A32 into
the graph at 5072:0040, keeps its count, and scans for the first kind-one
entry. It stores the resulting index at DS:1A34. When the scan finds
none it still proceeds with the index equal to the count; there is no
local missing-kind-one rejection before its following indexed reads.

Those reads use the selected 23-byte record's word +0A to index a
15-byte record through DS:19CD. Bits 10 or 20 hexadecimal of that
record's byte four select a separate call/message route. Neither the
meaning of those item bits nor the complete effects of that child are
established here. The space/transfer continuation below is the route
where both bit tests pass to +085B.

### Recipient and slot selection before transfer

At +085B the entry asks local +06B3 for recipient space and retains AX.
It also constructs the recipient graph in an 800-byte SS-local buffer.
It adds the source and recipient graph counts as words and rejects a
signed sum at least eighty by replacing the retained space with FFFF.
This is a wrapped word addition followed by a signed comparison, not
a general independently checked bound on both source graphs.

If the retained space is FFFF, it scans party indices zero through
three. Only candidates with a nonzero details-record word +0E receive
the space query and graph construction. It applies the same combined
count comparison; the first candidate with a space other than FFFF
becomes the recipient. If no candidate succeeds, it takes a separate
fallback call/message path. Thus the chosen recipient need not be the
original argument. All graph, helper and callback outcomes remain
conditions on this local route.

With a non-FFFF space, it rebuilds the chosen recipient graph and calls
2D40:288C with these argument words, in callee order:

| Argument at the far-return frame | Supplied value |
|---|---|
| BP+6, far pointer | Source graph 5072:0040 |
| BP+0A, word | Current DS:1A34 source entry index |
| BP+0C, word | The retained space value |
| BP+0E, far pointer | The SS-local recipient graph |
| BP+12, word | Zero, the target graph entry index |
| BP+14, word | Four, the target field selector |

These are sixteen argument bytes. The last two words come from one
doubleword push of 00040000 hexadecimal; they are not one field value.
After return the caller tests AL only. Zero returns without its later
success actions. Nonzero calls another helper and writes FFFF to
DS:1A34; a changed recipient also takes a message-formatting path.
No rollback of an earlier callee store is performed in this caller.

### The target-head request and final position request

2D40:288C first sign-extends the target graph's count byte and rejects
a signed value at least 79. It calls 2D40:2B6A with the target graph,
requested position and a near output word. Zero AL rejects; nonzero
continues through map construction. Count, near storage and output
identity must be admitted separately.

For the nonnegative target field supplied by this caller, it restores
the target entry's kind byte and record-index word into the current
object's three-byte metadata using 2D40:286C. That helper copies the
entry byte +2 and word +4, in order, into the supplied far destination.
The transfer then asks 1AA0:0009 for field four of current DS:60EB.

An unsigned returned AX at least 9999 invokes local 2D40:0C31. Only
AX exactly 9999 from that child takes this local
failure exit. Otherwise it passes the returned object, sign-extended
to a doubleword, to 1AA0:012A for field four of a freshly reread
DS:60EB. Setter status is not tested. Current-object preservation
through the getter and 0C31 remains a prerequisite for calling
this a write to the intended recipient's head.

When the getter instead returns unsigned AX below 9999, the transfer
requires current DS:40C4 equal to zero, the returned object's kind
equal to one, and the source entry's kind equal to one. It keeps the
target record number. A matching source record number can skip the
intervening splice work and join the final field-ten setter. Other
paths perform field-two and source-graph/map operations; their complete
children, ownership and failure effects remain open.

At the common +2B4E continuation it sign-extends the output position
word from 2B6A to a doubleword and requests field ten on the target
object through 1AA0:012A. It then sets AL to one without checking
setter status. Its local failure continuation sets AL to zero.
Neither return proves that every requested write succeeded.

Under FND-PARTY-052's shipped kind-two field descriptors, target field
four is the combatant word at +0C. This makes that head the target of
the reviewed recipient-transfer request; it does not yet distinguish
all uses of the other two heads or prove actual native head identity.

### Field ten and runtime copy width

The installed OBJEX.GFF FNFO/1 kind-one field-ten offset word is 19
decimal, at file 1FF5C. Its FNFO/2 type byte is zero at 20326.
Thus it addresses record offset 13 hexadecimal, the byte position
reader of FND-PARTY-217 and FND-PARTY-220.

The executable's initial DS:040C width byte for type zero is two.
The setup prefix in descriptor 188 at 73B8A directly stores one at
DS:040C after its string fill (FND-CONFIG-154). That particular byte
assignment does not depend on the fill's direction, although reaching
it, storage identity and retention through later calls remain required.

1AA0:012A has no separate type-zero byte-store shortcut. It resolves
the record and field, reads the current type-indexed width, then asks
1000:3F5A to copy that many bytes from its doubleword argument. With
the shipped descriptors and retained setup width one, field ten copies
the low byte to +13. With the initial width two retained instead, its
request spans +13 and +14. A static initial width or resource type byte
alone therefore cannot establish the active write width. Rejected
objects and missing descriptors have their own status exits.

For a kind-two target, 2D40:2B6A checks kind-one descendants against the
requested signed position and returns that position through its output
word when it finds no collision. Its special requested value -2 instead
searches 14..25. This caller supplies an already selected positive
space, not -2. Other helper inputs and its complete storage/child
contracts remain outside this transfer admission claim.

## Interpretation

The reviewed inventory route connects recipient selection, a backpack
space query, target graph entry zero and field four, and the later
field-ten position request. These local bindings answer part of
Q-PARTY-082, Q-PARTY-085 and Q-PARTY-086. They do not establish all
screen actions, complete list transfer or an unconditional byte write.

Actual current-object identity, descriptor/width stability, graph/map
storage, source selection and the full effects of 0C31 and graph helpers,
all callees and native event/presentation admission remain open.
No original function was executed and no complete reading is claimed.

## Alternatives

- Treating the final packed argument as a single mode loses the target
  entry zero and field-four words consumed separately by the callee.
- Naming every use of +0C as backpack membership goes beyond this
  request path and its unresolved current-object preservation.
- Treating AL one as successful mutation omits both ignored setter
  statuses and their rejected-object/missing-field paths.
- Inferring a two-byte native position write solely from the executable
  image omits setup's direct type-zero width assignment; inferring an
  unconditional byte write omits the width table and its lifetime.
- This caller's positive space is not 2B6A's independent -2 request.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and installed OBJEX.GFF, size
6816516, XXH3-128 e08d3cac153c547f6804ca27b155915d.
With the locked evidence Python read overlay_listing.py descriptor
191, 7C93B..7CBB7, and descriptor 188, 73B7C..73B9E. Follow
the latter prefix from FND-CONFIG-144's setup entry; the short range
is its width assignment, not a separately established entry.

Read resident_listing.py 2D40:288C..2D40:2B6A,
2D40:2B6A..2D40:2C87, 2D40:286C..2D40:288C and
1AA0:012A..1AA0:0209. The position helper's inventory has fragments;
follow its reached branches and shared continuation rather than
assuming the first body range is the whole function. Preserve each
argument width, fresh current-object read, setter-status omission,
source-kind rejection and position-collision exit.

Use report.mjs table with explicit source/hash and count/limit one:
OBJEX.GFF start 1FF5C, stride/width two for the offset; start 20326,
stride/width one for the type; DSUN.EXE start 4D40C, stride/width one
for the initial width. Fields have offset zero. Existing kind-two
field-four binding in FND-PARTY-052 and the directly read +13
position consumer are independent controls. All ranges/offsets above
are hexadecimal and half-open. Reports/configurations remain under
GAME_DIR/analysis/party-record-heads/transfer-20261018/.
