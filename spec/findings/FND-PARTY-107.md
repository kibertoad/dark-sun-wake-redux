---
id: FND-PARTY-107
title: The hit routine's call in overlay 179 +11DC's queue branch is never taken, and overlay 193 +003C passes its third argument as the item, which overlay 179 +12B1's queue and overlay 193 +0000 fill with fixed DATA numbers other than 104 and 225 or with computed ones
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000654BC..0x00065653
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E2F0..0x0007E32C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E795..0x0007E7D3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00042C64..0x00042C70
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, segment_references.py, data_bytes.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 179 `+0D2F`, the hit routine, takes a slot `t`, a second slot and an item
number `i` (FND-PARTY-101), and is called from overlay 179 `+0D1A` (FND-PARTY-102), overlay 179
`+1299` and overlay 193 `+04DE`.

**The queue.** Overlay 179 `+12B1` (trampoline `56A7:0084`) and `+1304` (trampoline `56A7:0089`)
add a 14-byte record at `4DA1:0000 + 14 * n`, `n` being the byte at `DS:0915` after adding 1 to it,
holding their first three arguments at `+0`, `+2` and `+4`; `+1304` also fills `+6` to `+0D`. When
the byte is 5 or more they call `+12B1` with the same second argument twice, 307 and 6 instead,
and that call meets the same test (`+12B1..+1372`). `+11DC` takes the records from the last down,
and for each reads the word at `4DA1:0054 + 2 * n`: for 0 it calls overlay 193 `+003C` with the
record's words at `+2`, `+0` and `+4` first, and for 1 it calls `+0D2F` with the words at `+0`,
`+2` and `+4` first (`+11E4..+12AF`). The six words at `4DA1:0054` are 0 in the file, the segment
being descriptor 102's, at file `0x00042C10`. Of the 26 words `segment_references.py` finds naming
segment `4DA1`, none leads to a store at `+0x54` to `+0x5F`: the stores are those of `+12B1` and
`+1304` to records 0 to 5, which end at `+0x54`; the others are the segment table's entry,
reads of words at `+0x60` and `+0x62`, and overlay 179 `+09AD`, which reads a `VECT` resource to
`4DA1:0060`.

**Overlay 193 `+003C`** passes its third argument as `i` at `+04DE`, with `t` from its local word
at `bp-0x1A` and its first argument as the second slot (`+0045..+0048`, `+04A5..+04E3`). It is
called from overlay 193 `+0029`, in `+0000` (trampoline `573B:007F`), which passes its own
arguments in the same order and then calls `+11DC` (`+0006..+0037`), and from `+11DC` at `+125C`.

The third argument each caller pushes is:

| Caller | Routine called | Third argument |
| --- | --- | --- |
| overlay 179 `+1028` | `+12B1` | 305 |
| overlay 179 `+2707` | `+12B1` | 311 |
| overlay 197 `+0F7C`, `+10F1` and `+180F` | `+12B1` | 307 |
| overlay 197 `+17A3` | `+12B1` | 124 |
| overlay 197 `+1A3C` | `+12B1` | 308 |
| overlay 197 `+1A2B` | `+1304` | -1 |
| overlay 197 `+1202` | `+12B1` | its register `SI` |
| overlay 198 `+00EF` and `+013D` | `+12B1` | its register `SI` |
| overlay 193 `+22CF` | `+0000` | its register `DX` |
| overlay 176 `+03BD` and `+048A` | `+0000` | 235 plus an argument or `DI` |
| overlay 177 `+04FE` | `+0000` | its argument at `bp+0xA` |
| overlay 204 `+19AF` | `+0000` | its local word at `bp-2` |

`data_bytes.py <install>/RESOURCE.GFF 19` gives the byte at `+0x19` of `DATA` 124 as 35, of
`DATA` 305 as 69, and of `DATA` 307, 308 and 311 as 0.

## Interpretation

Nothing sets a queue record's type word, so `+11DC` always takes the branch to overlay 193 `+003C`
and its call of the hit routine at `+1299` does not run; the one record that branch was built for
holds -1 anyway. A hit with `DATA` 104 or `DATA` 225 can reach the hit routine only through
overlay 193 `+003C`, from one of the callers that compute their third argument.

## Alternatives

- A store to `4DA1:0054..005F` through a pointer formed elsewhere, or through a record index above
  5, is outside the search; the record index is kept at 5 or below by the test in `+12B1` and
  `+1304`.
- The computed arguments of overlay 197 `+1202`, overlay 198 `+00EF` and `+013D`, overlay 193
  `+22CF`, overlay 176 `+03BD` and `+048A`, overlay 177 `+04FE` and overlay 204 `+19AF` were not
  traced.
- When the byte at `DS:0915` is 5 or more, `+12B1` calls itself with the same test until the stack
  runs out; whether the queue ever holds five records was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 179 654BC 65660`,
`193 7E2F0 7E3A0` and `193 7E790 7E7E8`; `direct_callers.py <dsun> 179+12B1 179+1304 179+11DC
193+0000 193+003C`, reading the pushes before each call; `segment_references.py <dsun> 4DA1`; the
six words at file `0x00042C64`; and `data_bytes.py <install>/RESOURCE.GFF 19`.
