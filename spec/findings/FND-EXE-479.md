---
id: FND-EXE-479
title: Sound utility publishes parsed words through repeated table searches with ambiguous zero lookup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:0496..158E:06B0
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:01B0..17FB:01DE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:020C..17FB:023A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:0346..17FB:0375
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:03A4..17FB:03D3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x00000342..0x00000392
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-478's field cleanup continues at 158E:0496. The caller passes
word 0082 to 17FB:0346, removes two incoming bytes and tests full AX.
Zero skips the first write group; nonzero selects it. Each selected field
write separately calls 17FB:01B0 with the same word, removes two bytes,
multiplies returned AX signed by 00FE and uses only the low product.
It reloads the far base from current DS:188C, adds that product to its
offset at word width, then performs the field write listed below. It does
not independently test any lookup result or high product, or retain one
row selection across the group.

| Write order | Row offset | Stored word |
| --- | --- | --- |
| 1 | 00EE | SS:BP-30, FND-EXE-478's second mapped word |
| 2 | 00F0 | SS:BP-32, its second one-byte conversion |
| 3 | 00F2 | FFFF |
| 4 | 00F4 | FFFF |
| 5 | 00F6 | SS:BP-2C, FND-EXE-476's first mapped word |
| 6 | 00F8 | SS:BP-2E, its first one-byte conversion |
| 7 | 00FA | Current DI |
| 8 | 00FC | Current DI |
| 9 | 00EC | 0001 |

After that group or its skip, the caller passes word 0082 to 17FB:03A4
and makes the same zero/nonzero decision. Its selected second write group
uses 17FB:020C before every field write and the far base at current
DS:1894. It performs the same ordered fields and value sources. Zero
predicate or completed group continues at 158E:06B0, outside this reading.
The DI source requires preservation from FND-EXE-478's earlier assignment;
its two uses are current reads, not a held snapshot.

The first predicate 0346 starts CX zero and compares it signed with
current DS:EC74 before each row. Each selected row uses the low signed
CX-times-00FE product added to a freshly loaded DS:188C pair, and
compares row word 0028 with its incoming word. Equality returns AX one;
otherwise CX increments and the signed limit is rechecked. Exhaustion
returns AX zero. The second predicate 03A4 has the same local sequence
with DS:EC76 and base DS:1894. Neither makes a call or interrupt.

The lookup helpers 01B0 and 020C use those same respective limits,
bases, strides and equality tests, but return current CX on equality and
AX zero on exhaustion. Thus row-zero match and no match share the same
returned word. The initial predicate is separate from each later lookup;
it does not provide an atomic selection or freeze the limit, base or row.
All four helpers restore BP and return far without incoming cleanup.
They do not locally change SI, DI or DS, and do change BX, CX, DX
and ES. They perform no extent, high-product or offset-wrap check.

Under admitted noninterfering state with a positive signed limit, searches
inspect indices zero through limit minus one, returning the first equality.
A zero or negative signed limit selects exhaustion without a row read.
These conditional loop bounds do not establish table capacity or validate
the caller's later displacement writes. Aliases between stored fields and
selection state are not excluded by these local sequences.

All twenty far calls in this caller interval have encoded segment 07FB
and segment words targeted by MZ relocation records 212 through 193,
in descending order. Modeled load segment 1000 binds their native segment
to 17FB. The caller's low-word multiplication and field addressing wrap;
no native operation or further result test occurs locally after publication.

## Interpretation

This follows the retained configuration words into two separately selected
table write groups. The local stride is 254 bytes, while selected-row identity,
table extents and field meaning remain separate claims. A preliminary predicate
and repeated lookup are different observations of mutable state. A zero lookup
cannot by itself distinguish a row-zero match from exhaustion.

Q-EXE-007 retains base/limit/row producers and actual DS, field semantics,
table and frame extents, aliases, selection-state lifetime and re-entry,
DI preservation and later continuation from 06B0. No complete reading,
successful hardware configuration or launch exclusion is claimed.

## Alternatives

Treating the writes as one retained-row update ignores the repeated lookups.
Treating lookup zero as failure conflicts with row-zero equality. Treating
the predicate as proof of each later selection assumes stable nonaliased
state. Treating the low product as validated row addressing ignores discarded
high products and word-width base addition. Naming the final word as an
enable field assumes later consumers not read here.

## How to reproduce

At revision e8dc926 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00007176..0x00007390 at IP 0496,
modeled CS 158E; and 0x00009560..0x0000958E at IP 01B0,
0x000095BC..0x000095EA at IP 020C,
0x000096F6..0x00009725 at IP 0346 and
0x00009754..0x00009783 at IP 03A4, modeled CS 17FB,
with locked Capstone 5.0.7 in sixteen-bit mode. Bind MZ header size
1400 and load segment 1000. In the relocation table at 003E with
958 records, check records 212 down to 193 against segment words
three bytes after caller far-call starts 049A, 04AB, 04C8, 04E5,
0501, 051D, 053A, 0557, 0571, 058B, 05A7, 05B8,
05D5, 05F2, 060E, 062A, 0647, 0664, 067E and 0698.
Track signed limits, first-match returns, ambiguous zero, every fresh base
and lookup, discarded high product and ordered stores. Original bytes stay
outside Git; no original process, DOSBox or emulated call runs.
