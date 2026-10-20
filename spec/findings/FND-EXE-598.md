---
id: FND-EXE-598
title: The 16 routines of overlay 175 that no entry placed open, fill, draw and close a six-slot item window and register two groups of six frame handlers for overlay 189
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005FBE0..0x00060328
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00060338..0x00060741
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000607E5..0x000609EC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00060AF0..0x00060BAB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00060EFD..0x00061347
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-597
environment: null
---

## Observation

After the restage of FND-EXE-597, `npm exec -- standard-coverage` reported 16 rows of overlay 175
(header segment `5689`, 19 trampolines, code from file offset `0x5FBE0`, 5,991 bytes) that no
spec location covered, 4,698 bytes in all. Each row below is one row of the inventory; "offset"
is the overlay code offset, the file offset is `0x5FBE0` plus it, and `DS` is `57E0`. Every
routine has a trampoline in `5689`. Callers are the hits of `direct_callers.py`, checked against
the instructions around them. FND-CONFIG-091 and FND-CONFIG-092 name `+002C` and `+0231` by
their trampolines only.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0000` | `0x5FBE0..0x5FC00` | `5689:007A` | Takes a far record pointer and calls overlay 190's `+3456` with 9, the record's words at `+0x96` and `+0x98`, and 0. Returns 0. |
| `+0020` | `0x5FC00..0x5FC0C` | `5689:002A` | Calls `+0904` and returns 0. |
| `+002C` | `0x5FC0C..0x5FE11` | `5689:0057` | The setup entry of FND-CONFIG-091 and FND-CONFIG-092, called from overlay 178 at `+0365` and `+0C29`. Takes a word into `DS:05D6` and clears `DS:05D8`. Places the window from the words at `+0x67BE` and `+0x67C0` of the 37-byte party record of `[DS:426D]` less `DS:1408` and `DS:140A`, falling back to `0x59` and `0x36` outside 0 to 200. Allocates 80 10-byte entries through `444C:0008` into `DS:4210` and passes `DS:05D9` to overlay 180 `+0867` when that fails, loads the frames of overlay 190 `+34E7` with 8 into `4E71:002F`, creates window `0x34BD` through overlay 182 `+016B` with the handler `5689:0052` (`+0B61`) and stores it at `DS:4214` and `DS:1431`. It registers `5689:005C` (`+0231`) for identifiers `0x2BCD` to `0x2BD2` through `3F96:02AB` and sets their event masks through `3F96:02F8` (FND-CONFIG-092), registers `5689:0052` for identifier `0x3391` and through overlay 190 `+11A1` (FND-CONFIG-086), calls `+0904`, shows the window through `3A8E:0D0C`, `0D3C` and `060D`, and calls overlay 182 `+059C`. |
| `+0231` | `0x5FE11..0x60207`, `0x6020A..0x60328` | `5689:005C` | The frame handler of FND-CONFIG-092. Takes a button number less `0x2BCD` and a message word, and looks the word up in the table at `+0748` (values 2, 4, `0x20`, `0x80`; targets `+028F`, `+041C`, `+0575`, `+062A`). The cases hit-test the window `55CB:0000` holds (`0x92` by `0x43` from its words `+0x96` and `+0x98`), clear `DS:0DA4`, draw with `2D40:3BEC` and the frames at `4E71:002F`, call `+0758` and `+0CB8`, and pass a slot's item through overlay 213 `+0000`. The `jmp` at `0x60207` follows another `jmp` and nothing reaches it, so the row leaves its three bytes out. |
| `+0758` | `0x60338..0x604E4` | `5689:002F` | Takes a slot 0 to 5. Reads the rectangle of button `0x2BCD + slot` through `3F96:01A6`. For a slot whose word at `DS:4218 + 2 * slot` is not `0xFFFF`, takes that entry of the list at `DS:4210` and its 23-byte record under the far pointer `DS:19C1`; when the record's word `+0x0C` is 0 it fills it from overlay 190 `+2F2F`. Loads resource `BMP ` with that number through `38FF:04AB`, reads its two sizes through `1BF3:76C2` and `1BF3:771C`, and draws it in the slot's 16 by 16 cell with `2D40:3BEC`: centred when both sizes are at most 16, otherwise scaled through `1BF3:282D` and `2D40:3BD1`. Sets `DS:2FB8` to 1 and frees the resource through `444C:0092`. Called from `+0231` twice and from `+0904`. |
| `+0904` | `0x604E4..0x60741` | `5689:004D` | Sets the six words at `DS:4218` to `0xFFFF` and walks the 10-byte list `2D40:2196` returns for `DS:05D6` into `DS:4210`. Each kind-1 entry whose word `+4` is below `0x270F` and differs from the word at `4F49:0C34 + 3 * [DS:05D6]` goes in the slot its record's position byte `+0x13` (FND-PARTY-217) names, less 1. When an entry's byte is outside 1 to 6 it gives the first free slot to the first such entry, writing slot + 1 to its byte, and starts again. Then, between `28C9:2ABD` and `28C9:2A81`, it draws frame 2 of `4E71:002F` at each slot (and frame 4 when overlay 191 `+0000` returns nonzero) and calls `+0758` for each filled slot. Called from `+0020`, `+002C`, `+0CB8` and resident `28C9:13CA`. |
| `+0C05` | `0x607E5..0x60898` | `5689:0048` | Sets `DS:05D6` to `0x270F`, calls overlay 213 `+02D5` when `55CB:0000` is set, frees `DS:4210` and `4E71:002F` through `444C:0092`, closes the window at `DS:4214` through overlay 182 `+0128` and clears `DS:4214` and `DS:1431`, then calls overlay 182 `+19F8` and `3D72:0D83`. When overlay 191 `+0000` returns nonzero it calls `28C9:2535` with 3 and `1695:0290` with `DS:426D` and `DS:1A32`. Called from `+0B61` three times, from `+0CB8` and from resident `28C9:1152` and `28C9:13D1`. |
| `+0CB8` | `0x60898..0x609EC` | `5689:0043` | Takes a slot. With `DS:1A34` equal to `0xFFFF` and the slot filled, calls `+0FCB`. With `DS:1A34` set and the slot empty, calls `+0F10`. With both set it stores 1 and the slot's item in the 3-byte entry at `4F49:0C33 + 3 * [4F49:000B]`; when `1695:0834` returns bit `0x20` for `DS:1A32` or for `4F49:000B` it calls `+0C05`, overlay 182 `+19F8`, `3D72:0D83` and `1695:0304`, which FND-CONFIG-052 follows to the `DS:05E3` message, and returns; otherwise it calls `+0E0C`. The other paths then call `+0904` and overlay 182 `+059C`. Called from `+0231`. |
| `+0F10` | `0x60AF0..0x60BAB` | `5689:0039` | Takes a slot. Finds the first kind-1 entry of the list at `DS:4210` and of the list at `5072:0040` (refilled for `DS:1A32`), and moves the second into the first through `2D40:288C` with 4. On success it writes slot + 1 to the moved item's record byte `+0x13` and sets `DS:1A34` to `0xFFFF`. Called from `+0CB8` and `+0E0C`. |
| `+131D` | `0x60EFD..0x61146` | `5689:0025` | Takes `n`. Returns 0 when word `n` of the table at the far pointer `DS:617D` is `0xFFFF`, or when the item it selects in the list at `DS:6189 + 4 * [4E71:0B44]` names, through the word `+0x0A` of its 23-byte record, no 15-byte record under `DS:19CD` (`0x270F` or more) or one without bit 3 of byte `+0x0B`. Otherwise it calls `+16DE`, picks `DS:32CB` when the low four bits of that record's byte `+4` are 5 and `DS:32CA` otherwise, calls `+162C`, `28C9:2A81` with 1, overlay 186 `+00B8` with 15 and `+157A` with that choice, and stores `n` in `DS:6185` with `DS:6188` set to 1 (`DS:32CA`) or in `DS:6183` with `DS:6187` set to 1, setting `DS:6181` from `27E5:01F3`. It calls overlay 189 `+1688` with `4E71:0B44`, then for six indices registers the handlers `5713:0052` and `5713:004D` (overlay 189) for identifiers `0x2BF0` to `0x2BF5` and `0x2BF6` to `0x2BFB` of the window at `5072:003C` through `3F96:02AB` and sets their event masks through `3F96:02F8`, calls overlay 189 `+1893` and `+1946` with the index plus `0x2A` or `0x24`, and `+1AD7` where the table word is set. Returns 1. Called from `+1566` and overlay 189 `+21EF`. |
| `+1566` | `0x61146..0x6115A` | `5689:0020` | Takes two words and calls `+131D` with the second less `0x2BCC`. Called from overlay 189 `+114E`. |
| `+157A` | `0x6115A..0x6120C` | `5689:0075` | Takes `DS:32CA` or `DS:32CB`. Reads two positions through overlay 189 `+30E2` with 6 and 5, gets a far pointer from overlay 190 `+34E7` with 4 (`DS:32CA`) or 6 into `5072:0038`, and when it is set draws it at both positions plus `0x12` with `2D40:3BEC` and frees it through `444C:0092`. Called from `+131D` and overlay 189 `+0309` and `+034E`. |
| `+162C` | `0x6120C..0x612BE` | `5689:0070` | The same with 5 or 7. Called from `+131D` and overlay 189 `+06A9`, `+086F`, `+1F87` and `+1FBA`. |
| `+16DE` | `0x612BE..0x612CD` | `5689:0061` | Calls `+16ED` and `+172A`. Called from `+131D` and overlay 189 `+0697`, `+085D` and `+1348`. |
| `+16ED` | `0x612CD..0x6130A` | `5689:0066` | Clears `DS:6187` and registers a null handler for identifiers `0x2BF0` to `0x2BF5` of the window at `5072:003C` through `3F96:02AB`. Called from `+16DE` and overlay 189 `+1F9D`. |
| `+172A` | `0x6130A..0x61347` | `5689:006B` | Clears `DS:6188` and does the same for `0x2BF6` to `0x2BFB`. Called from `+16DE` and overlay 189 `+1FD0`. |

Outside overlays 175 and 189 the byte search finds far callers only for `+002C` (overlay 178),
`+0904` and `+0C05` (resident `28C9`). It finds no caller at all for `+0000` and `+0020`;
`+0231` is entered as the handler `+002C` registers.

## Interpretation

`+002C` to `+0F10` are one window: `+002C` opens it over the list for `DS:05D6`, `+0904` sorts
the list's items into six slots by their position byte, `+0758` draws each slot's picture,
`+0CB8` acts on a click in a slot with or without the item in `DS:1A34`, `+0F10` puts that item
in an empty slot, and `+0C05` closes the window. `+131D` to `+172A` are called almost only from
overlay 189, whose callbacks FND-PARTY-217 ties to the inventory window: for an item they register
overlay 189 handlers on one of two groups of six identifiers and draw two pictures. The names stay
`fn_175_<offset>` until a reading shows what the list, the slots and the two groups are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine with no
recorded caller may still run. `DS:32CA` and `DS:32CB` are used here only as two distinct values;
what they hold is not read. Which list `DS:05D6` names is not settled.

## How to reproduce

Run `overlay_listing.py` for overlay 175 against the installed `DSUN.EXE` and `direct_callers.py`
for each offset in the table (`175+<offset>`), and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
