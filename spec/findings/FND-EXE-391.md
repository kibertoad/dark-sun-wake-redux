---
id: FND-EXE-391
title: Game type-one slot producer publishes relocated targets but returns zero even without publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0107..1425:01C3
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1425:0107..1425:01C3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:04FF..1425:0589
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1425:04FF..1425:0589
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0360..15F3:036F
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:0360..15F3:036F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000099D9..0x000099E1
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x000099D9..0x000099E1
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The far dispatcher 1425:04FF starts DX at two and reads a word
through the incoming far pointer at SS:BP+6. It subtracts one at
word width, rejects unsigned results above three and otherwise doubles the
result and dispatches through CS words at 0589. The four shipped words
are 0518, 0547, 0532 and 0563: incoming types one,
two, three and four select those branches respectively. Type zero wraps
to FFFF and rejects; other types outside one through four reject too.
The rejected path returns AX two without those branches' calls.

Type one pushes the incoming far pair at SS:BP+0E, the word
at offset two in the pointed-to record, and the incoming far pair at
SS:BP+0A, in that order. It manufactures a far return and calls
local 0107, removes ten outgoing bytes and copies returned AX into DX.
The shared exit copies DX back into AX, restores BP and returns far
without incoming cleanup. The other types call 0329, 0462 and
01C3; their effects remain unread here. Their own dispatch indices and
bounds are not inferred from the type-one branch.

0107 saves BP and allocates four local bytes. Its incoming far pointer
at SS:BP+0C supplies a quantity word; the request DX is the
larger unsigned of that word and four. It pushes SS and the offset
BP-4, SS and BP-2, that DX word, and the incoming
word at SS:BP+0A, then calls far 15F3:02C3 and removes
twelve bytes. The returned AX is not tested. Current local word BP-4
below four goes directly to the zero-AX exit. Otherwise it calls far
15F3:02A9 and stores returned AX into current DS word 3F40
installed or 3EB4 on disc. That word zero also goes to the
zero-AX exit without the later quantity, slot or index writes.

The continuing path reloads the quantity pointer from SS:BP+0C. Its
current word below local BP-4 is set to zero; otherwise local BP-4
is subtracted from it. It next loads ES:BX from the incoming far
index pointer at SS:BP+6, reads the index through ES and multiplies
it by fourteen at word width. It writes current local BP-2 into
current DS word indexed from 3F4E installed or 3EC2 on disc,
which is FND-EXE-559's cleanup argument field.

Each following pair publication reloads the index word through that same ES
and incoming offset before multiplying by fourteen. The writes are:

| Order | Installed indexed field | Disc indexed field | Modeled value |
| --- | --- | --- | --- |
| 1 | 3F44 | 3EB8 | Segment 1425 |
| 2 | 3F42 | 3EB6 | Offset 00DB |
| 3 | 3F48 | 3EBC | Segment 1425 |
| 4 | 3F46 | 3EBA | Offset 00AF |
| 5 | 3F4C | 3EC0 | Segment 15F3 |
| 6 | 3F4A | 3EBE | Offset 0360 |

The segment immediates are MZ relocated: index 19 at 0425:0187
holds shipped 0425; index 18 at 0425:019C also holds 0425;
index 17 at 0425:01B1 holds 05F3. With modeled load segment
1000 they give the values above in both editions. There is no
local upper-bound check on the supplied index. Keeping every publication in
one slot requires the pointed-to index to remain stable; aliases with earlier
quantity or slot stores are not excluded by this helper.

After publication it reloads the incoming index offset and increments the
word through the current ES. All local exits clear AX, use LEAVE
to discard the four local bytes, restore BP and return far without incoming
cleanup. Thus AX zero does not distinguish publication from either earlier
bypass. Earlier request/helper effects are not rolled back locally. The input
pointers, local output writers, native results and preservation, actual DS/ES
and storage bounds remain unadmitted.

The two preliminary call segments also have MZ relocations: index 21 at
0425:0132 and index 20 at 0425:0143 hold shipped 05F3,
giving 15F3 with modeled load segment 1000. Their bodies remain
unread here; the four local bytes have no initializer in 0107 before
the first call. No output contents are assumed for a failed or skipped
native operation.

The published cleanup target 15F3:0360 saves BP and DX, loads
DX from its incoming word at SS:BP+6, sets AX 4500 and
requests interrupt 67. It restores incoming DX/BP and returns far without
incoming cleanup, without testing or mapping the native result. FND-EXE-559's
slot dispatcher supplies its stored argument word and ignores the result. Native
request effects and preservation remain open; this reading does not prove a
successful release or admission of this slot at runtime.

## Interpretation

This supplies one concrete producer of the indirect cleanup targets, its
type-selection bound, paired publication order and conditional index update.
It resolves the modeled cleanup target for type one without treating the
producer's zero AX as success. Q-EXE-007 retains preliminary helper/output
contracts, the other type producers, complete writer/caller coverage, aliases and
state admission, the two other published targets and native cleanup effects.
No complete slot contract or game launch exclusion is claimed.

## Alternatives

Admitting type zero contradicts the decrement and unsigned rejection. Selecting
branches by neighbouring table order contradicts the actual indexing and four
shipped words. Treating AX zero as successful publication contradicts both bypass
paths. Treating the incoming index as locally bounded to sixteen invents a guard.
Treating every pair as necessarily belonging to the initially selected slot
ignores repeated index reads and unresolved aliases. Treating segment immediates
as their shipped values ignores relocation. Treating the cleanup request as
checked success ignores its absent native-result test.

## How to reproduce

At revision a3764c5f require both identities from FND-EXE-350. Use MZ
header size 5200, modeled load segment 1000 and relative code segments
0425 and 05F3. Decode the address ranges in Locations in sixteen-bit
mode; read the four dispatch words at the file-data locations as little-endian
words, not instructions. Track types zero through five and FFFF through
the decrement/bound/index steps, the type-one outgoing widths and cleanup,
quantity comparisons, local-word reads without an assumed initializer, each index
reload and ordered slot store, and all zero-AX exits. Read header relocation
count at 0006 and table offset at 0018 and check indices 17
through 21 and their operands above. Use FND-EXE-559 for the consuming
slot dispatch. No negative caller/writer census is claimed. Licensed bytes remain
outside Git; no game process, DOSBox or emulated call runs.
