---
id: FND-RNG-087
title: Of the 104 calls of the copy routines 1000:0452 and 1000:3F5A, 61 have a destination that reaches neither DS:426D nor 4C13:0369, three copy into records that cover DS:426D only at indices their callers do not pass as 9999 or their loop does not reach, two have an index or base not read, and 38 take the destination from a far pointer in memory or an argument
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:36CE..172C:3740
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0927..2D40:0944
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:05D5..277B:06AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:0462..2834:047F
tool: Capstone 5.0.9 over the load image and overlay code; tools/research/exec-census/block_copies.py (context 24), operand_range.py and resident_listing.py at the commit that adds this finding, with coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv; Python 3.14.7
environment: null
---

## Observation

Addresses are static; segment immediates in load-image code are relative to 0, so `0x1000` is
added, and an overlay's segment word names the resident segment of its descriptor (FMT-EXE-003).
`block_copies.py` lists 55 calls of `1000:0452`, which copies `CX` bytes to the far pointer pushed
first from the one pushed last, and 49 of `1000:3F5A`, which copies the count pushed first to the
far pointer pushed last from the one pushed before it. Each call's destination, read from the
pushes before it:

| Destination | Calls |
|---|---|
| a frame local `SS:[bp - n]`, from `lea ax, [bp - n]` and `push ss` | 40: `1AA0:0338`, `1B2E:02F0`, `27E5:002D`, `27E5:0248`, `3150:007B`, `31E0:1DFC`, `31E0:1E76`, `31E0:1FD6`, `31E0:2468`, `31E0:24F4`, `31E0:41E3`, `31E0:43B1`, `362C:0AD2`, `362C:0AE5`, `362C:0AF8`, `371D:0079`, `371D:03A8`, `37FC:07DC`, `3D72:056E`, `4734:014A`; overlay 173 `+2F35`, `+3ABD`; overlay 187 `+06AF`; overlay 188 `+1302`, `+1339`, `+137D`; overlay 193 `+016E`, `+063B`, `+1418`, `+1EB8`, `+1FB7`, `+2349`; overlay 203 `+08A7`; overlay 204 `+133A`; overlay 208 `+005F`, `+05DE`, `+081D`, `+08B1`, `+097E`, `+0CDB` |
| `DS:[bp - 0x22] + 8 × SI`, a frame local through `DS` | overlay 182 `+0447` |
| `DS` at a constant: `9C02`, `9C06`, `9BFE`, `A424`, `9B86` | `362C:0B12`, `362C:0B25`, `362C:0B38`, `4611:0256`, overlay 208 `+1634` |
| `4C13:0034 + 0x40 × n` and `4C13:00B4`, 0x40 bytes, with `n` the word `4C0E:0010`, which the first call's routine raises only from below 2 and the second's lowers only from above 0 | `172C:36F3`, `172C:3739` |
| `4E71`, `4E4F` or `4F49` at a constant or a constant plus an index times 3, 10 or 19 | 13: `1AA0:0558`, `2D40:2FD2`, `31E0:10F7`; overlay 183 `+1E43`; overlay 184 `+1DE3`, `+1DF7`; overlay 187 `+1E59`; overlay 188 `+184D`; overlay 193 `+23DD`; overlay 204 `+1F5B`, `+1F76`, `+1F91`, `+1FAD` |
| `DS:67BB + 37 × s`, a slot record, 37 bytes | `2D40:2FAE` (`[bp - 2]`), `31E0:110D` (`SI`) |
| `DS:55CB + 6 × SI`, 6 bytes | overlay 188 `+1803` (`SI` from 5 below `0x30`), `2D40:093F` (`SI` the routine's record number) |
| `DS:SI + 3 × [SI] + 0xC2` | `1AA0:0625` |
| a far pointer held in `DS` at `40C0`, `6573`, `19C1`, `19C5`, `19C9`, `19D1`, or in the table at `2A63` | 13: `172C:2E99`, `172C:2F3E`, `172C:301E`, `172C:30FE`, `31E0:1AF0`, `371D:0320`; overlay 184 `+1694`, `+16E8`, `+1708`; overlay 187 `+0478`, `+050E`, `+0562`, `+05C6` |
| a far pointer from an argument, a frame local or a record | 25: `172C:2AC7`, `172C:2B1B`, `1AA0:01E9`, `2D40:3E08`, `31E0:1B07`, `37FC:081E`, `38FF:019D`, `39D1:09D9`, `39D1:09FC`, `3A8E:1184`, `3CFA:015E`, `3F96:0208`, `4072:0091`, `4237:009D`, `4420:0194`; overlay 187 `+0321`, `+033A`, `+1E27`; overlay 188 `+13C4`; overlay 196 `+00B8`; overlay 204 `+1FCD`, `+1FE8`, `+2003`, `+201F`; overlay 211 `+1FA8` |

The segment table's descriptor 93 gives segment `4C13` a size of `0x36B` bytes, so `4C13:0369` is
its last word; descriptor 218 gives `DS` (`57E0`) `0xA4EC` bytes. `4E4F`, `4E71` and `4F49` lie
between them.

`operand_range.py` over the offsets `0x3800` to `0x426E` finds five pushes or moves of an immediate
between `0x4200` and `0x426D` other than the two pushes of `DS:426D` itself (`28C9:2146`,
`31E0:2D50`): `0x4200` is the DOS seek request at three `int 0x21` (`15F3:0166`, `4AE5:009A`,
`4AE5:03ED`); overlay 175 `+090F` fills the six words from `DS:4218` with `0xFFFF`; overlay 173
`+124D` passes `DS:420A` to `2D40:3E64`; `277B:05D5` passes `DS:4226` to `1000:374E` with `DS:076C`
and three times to `2D40:3DC2` with the count `0x28`; and `2834:0466` passes `DS:4252` with the
count 3 to `44DE:01AF`. The stores through `[bx + 0x4265]` (overlays 189, 202, 211 and 212) store
1 to 4.

## Interpretation

A copy through the first four rows of the table cannot reach `DS:426D` or `4C13:0369`: frame
locals lie above the static data, the constants lie above `0x426D`, and the copies into `4C13` end
at `0xF4` while `4C0E:0010` stays between 0 and 2, which holds unless another write to it was
missed. The 13 copies into `4E4F`, `4E71` and `4F49` lie above `4C13`, and would reach
`DS:426D` only at an offset above `0xCBDD`. A 37-byte slot record covers `DS:426D` only for the
slots 1513, 3284, 5055, 6826, 8598, -259 and others further from 0, so not for 9999 or any slot
below 320; a 6-byte record at `DS:55CB` covers it only for index -827 or 10096, which overlay 188's
loop does not reach. What is left are the index of `2D40:093F`, the pointer `SI` of `1AA0:0625`,
and the 38 calls whose destination is a far pointer in memory or an argument.

## Alternatives

The census does not cover copies through the inline string moves that `block_copies.py` also
lists, through other copy and string routines (`1000:374E`, `2D40:3DC2` among them), or file
reads (`44DE:01AF` among them). Where the far pointers of the last two rows point was not read.
What `2D40:3E64`, `2D40:3DC2`, `1000:374E` and `44DE:01AF` write through the pointers at
`DS:420A`, `DS:4226` and `DS:4252`, and how far, was not read. The bounds of `[bx + 0x4265]`'s
index were not read; its values are below 9999 either way.

## How to reproduce

`block_copies.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv 24` and read the pushes before each
call; `operand_range.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv 3800 426E` and
`... 4100 4290`; `resident_listing.py <dsun> 172C:36CE..172C:3745 277B:05D5..277B:06B0
2834:0440..2834:0480 2D40:08E0..2D40:0960`; `overlay_listing.py <dsun> 188 74660 746B0`,
`... 173 5BB58 5BB80` and `... 175 604E4 60520`; the segment table rows 93 and
218 (FMT-EXE-003).
