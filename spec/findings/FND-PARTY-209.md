---
id: FND-PARTY-209
title: Only character generation and one DUAL branch clear a slot's SPST entry at 4D62:039C, each over bits 0 to 114, so a 9-byte SPST loaded into a box keeps bits 72 to 119 from whatever the box held before
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/immediate_search.py, overlay_listing.py, resident_listing.py)
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006EB01..0x0006EB3B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00094A7C..0x00094AC3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00042BBC..0x00042BF8
    kind: file-data
---

## Observation

Overlay offsets are written `descriptor +offset`. The table of `SPST` entries is at `4D62:039C`,
15 bytes per slot (FND-PARTY-012); code reaches segment `4D62` through the segment word `0x0320`
(descriptor 100).

**Every instruction with the word `0x039C`.** `immediate_search.py <dsun> 039C` decodes at every
byte of the load image and of every overlay. Leaving out the hits that start inside another
instruction (one byte after an `ES:` prefix) and `2CC4:0007`, a near `call 0x39C`, the hits are:

| Site | Instruction | What it does with the entry |
|---|---|---|
| overlay 177 `+053D` | `mov al, es:[bx + 0x39C]` | reads a byte |
| overlay 184 `+1A9D` | `and es:[bx + 0x39C], dl` | clears bit `n` for `n` from 0 to 114 (`+1A7A..+1AA9`), when `[bp - 5]` is not 0 (`+1A71`) |
| overlay 184 `+1BFD`, `+1C21`, `+1C33`, `+1C69`, `+1C7B`, `+1C8D` | `or es:[bx + 0x39C], 1`, `0x80`, `0x10`, `2`, `4`, `8` | sets one bit of byte 0 |
| overlay 186 `+01EB`, `+0295` | `add ax, 0x39C` | the write and the read of the `SPST` resource (FND-PARTY-012, FND-PARTY-203) |
| overlay 209 `+0556` | `or es:[bx + 0x39C], dl` | sets one bit, before the text `IS LEARNED!` |
| overlay 209 `+1945` | `and es:[bx + 0x39C], dl` | clears bit `n` for `n` from 0 to 114 (`+1922..+1951`), when its argument `[bp + 8]` is 11 (`+191C`) |
| overlay 209 `+195D` | `or es:[bx + 0x39C], 0x80` | sets bit 7 of byte 0 right after that loop |
| overlay 209 `+1B4D` | `mov al, es:[bx + 0x39C]` | reads a byte |
| overlay 213 `+07B2` | `mov al, es:[bx + 0x39C]` | reads a byte |
| overlay 213 `+07F7` | `or es:[bx + 0x39C], dl` | sets one bit |
| `29CF:000A` | `or es:[bx + 0x39C], dl` | sets one bit, inside `28C9:102B`, which FND-PARTY-116 reads as setting bits 0 to 114 of each slot |

Each overlay site loads `ES` with the segment word `0x0320` within the 25 bytes before it, and indexes the byte as `15 * slot + (n >> 3)`.

The 60 bytes of the table in the file (`0x42BBC..0x42BF8`) are all 0.

## Interpretation

No instruction that names the table clears a whole entry, and the two loops that clear bits stop
at bit 114: one runs in character generation, the other in a DUAL branch for class code 11. When
DROP empties a box, nothing among these sites clears its entry. A 9-byte `SPST` loaded into a
slot (FND-PARTY-203) therefore leaves bytes 9 to 14 as they were: 0 when nothing has used the slot
since the program started, and otherwise the bits the slot's last character had, from its own
load, from generation or from the sites that set bits. Bits 115 to 119 are never cleared once set.

## Alternatives

- The census covers instructions that carry `0x039C` as an immediate or displacement. A transfer
  that reaches the table through a pointer formed without it, such as a block copy of a whole
  segment or a read into `4D62` at a computed offset, is not covered; none was searched for.
- Whether any of the bit setters runs for a slot that a later ADD reuses was not followed; the
  reading gives what the bytes can hold, not which bits a given sequence leaves.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 039C`;
`overlay_listing.py <dsun> 184 6EAF0 6EB50`, `184 6EC70 6ED30`, `209 936A0 936C0`, `209 94A70 94AD0`
and `213 99A20 99A60`; `resident_listing.py <dsun> 29CF:0000..29CF:0012`. Read the 60 bytes at file
offset `0x42BBC`.
