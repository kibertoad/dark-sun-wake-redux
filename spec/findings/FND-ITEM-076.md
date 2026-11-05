---
id: FND-ITEM-076
title: Effect removal clears conditional map masks while the middle cell helper reads before its child coordinate gate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:0088..25AF:00BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:01DD..25AF:023B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:04D0..25AF:054A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:060F..371D:069E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001B23A..0x0001B266
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table at c7aac608; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0; composition with FND-COMBAT-147 and FND-ITEM-073
environment: null
---

## Observation

Q-PARTY-116 retains three removal-side cell children. FND-COMBAT-147
already reads the two leaf mask helpers; this composes their actual
caller gates and reads the remaining 04D0/0088 path. Addresses, field
offsets and masks are hexadecimal. Selector values are decimal.

### Removal caller and existing leaves

For each matching placed row, 371D:050F selects an eighteen-byte effect
record using its saved effect index. Effect-record byte +2 bit 10
requests 25AF:020C. It always requests 25AF:04D0 with effect-record word
zero. Byte +2 bit 20 requests 25AF:01DD afterward. Each call independently
obtains its coordinates from the current row's two unsigned words shifted
right four. Child/frame/table preservation remains a prerequisite for
these successive row rereads.

FND-COMBAT-147 establishes that 020C clears cell mask 80 and 01DD clears
mask 40, with null-map and unsigned x<128/y<98 gates. Each updates only
one byte at the map segment and low-word offset base+128*y+x. Neither
has a child. Those own gates validate neither map capacity nor offset
wrap or aliasing. The removal caller does not test a result from either
leaf or from 04D0 before continuing toward row compaction.

### Middle helper's selected mask and preliminary read

04D0 first tests the full far map pointer DS:0538 and returns zero when
it is null. Otherwise it searches eleven selector words at CS:054A,
with paired target words twenty-two bytes later. The entire table gives:

| Selector word | Computed mask m |
| --- | --- |
| 100, 109, 114 | 3 |
| 208, 221 | 2 |
| 161, 180, 206, 207, 222, 226 | 1 |
| Any other word | 0 |

The branch targets enter a three-increment fallthrough sequence to
produce these values. No additional selected table arm calls a child.
The helper then reads a map byte at low16(base+128*y+x), using word
arithmetic and the supplied coordinates. This read has no preceding
coordinate-range test. Let v be that byte's low two bits.

If m<v, it returns v without a child or own map write. Otherwise it calls
0088(x,y,m) and returns m. The return value is therefore max(m,v), not
the resulting cell bits and not a success flag.

### Conditional clear in 0088

0088 rereads the full map pointer, rejects null or unsigned x>=128 or
y>=98, and otherwise computes the same word-offset address. It clears
the bits selected by the low mask byte: new_byte=old_byte AND NOT m.
Other bits are retained. Under stable pointer/storage, m=3 clears both
low bits, m=2 clears only bit one, m=1 clears only bit zero and m=0 leaves
the byte unchanged. The parent reaches this child only when m>=v.
For example m=2/v=1 returns two while retaining low bits one; m=3/v=2
returns three while clearing the low bits to zero. These are derived
local cases, not native gameplay observations.

An out-of-range coordinate can therefore permit the parent's preliminary
read, then have the child reject the write. A changed/null pointer at
the child also prevents its write without changing the parent's already
computed return m. The parent restores SI/BP; the leaf restores BP.
ES and scratch registers are not preserved claims. With valid disjoint
map, stack, globals and effect/row tables, the only persistent stores of
these children are the described cell-byte updates. No DOS, timer, port
or indirect external call is reached by their local paths.

## Interpretation

This removes the three local cell-child bodies and their caller ordering
from Q-PARTY-116. Native coordinate/read-span admission, map capacity,
aliasing and pointer/row/slot preservation remain. A matching row can
still reach compaction when a leaf skips its map write; the caller adds
no success gate. This neither establishes that a constructed row survives
until removal nor proves redraw/unload effects.

## Alternatives

Calling 04D0 a direct category assignment contradicts its conditional
bit clear and maximum return. Applying the child's coordinate gate to
the parent's earlier read would omit an actual access. A helper return
does not prove cell mutation, and a bounded coordinate does not prove
backing storage. No original was run, complete reading declared or parity
status promoted.

## How to reproduce

Use approved DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py with -I and
the four code ranges above in order. Under
GAME_DIR/analysis/item-removal-20261104/, effect-removal-cell-contract.txt
and its -repeat pair have SHA-256
1f6d9aaab03d6ef918fb9c3fae902c7e8d89ec796256c34d2bc13fa528e92d33.

Use report.mjs table at file 1B23A with 22 stride-two width-two rows:
eleven keys then eleven paired destinations, bounded by 04F4..0509.
Local effect-cell-mask-table-controls.json records the query;
effect-cell-mask-table.json and its -repeat pair have SHA-256
2467b24dbeaaa88aa7e1753d906d5a4f0161e2fd5d25b0fb9cd52cfaf35ae5d7.
Keep original-derived reports, tables and listings in that local store.
