---
id: FND-EXE-604
title: The 5 routines of overlay 178 that no entry placed sort a clicked slot into an action or open a small window at 4DA0:0000 with three action buttons, a hover label of TALK, GET, USE or OPEN and a USEABLE BY list, handle that window's events, and close it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000630D0..0x00063CF3
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

After the restage of FND-EXE-601, `npm exec -- standard-coverage` reported 5 rows of overlay 178
(header segment `56A2`, 9 trampolines, code from file offset `0x630B0`, 4,325 bytes) that no
spec location covered, 3,104 bytes in all. Each row below is one row of the inventory; "offset"
is the overlay code offset, the file offset is `0x630B0` plus it, and `DS` is `57E0`. Every
routine has a trampoline in `56A2`. Callers are the hits of `direct_callers.py`, checked against
the instructions around them, named by the row that holds them; a routine that overlay 178 hands
on as a far pointer is named with the setter it goes to (FND-EXE-587, FND-EXE-588). A slot is one
of the 320 object slots whose 3-byte entries are at `4F49:0C33` (FND-PARTY-131): a state byte and
the item word after it. Its 37-byte record is at `DS:67BB + 37 * slot`; an item's 23-byte record is
under the far pointer `DS:19C1`, and that record's word `+0x0A` indexes the 15-byte records under
`DS:19CD`. The window pointer is the double word `4DA0:0000`, also kept at `DS:1431`, and `DS:08CA`
holds its slot (`0x270F` when none). `DS:426D` is the current party member (FND-EXE-598).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0020` | `0x630D0..0x630F0` | `56A2:0043` | Calls overlay 190 `+3456` with `0x10`, the words at `+0x96` and `+0x98` of the record its far pointer argument names, and 0, and returns 0, the form of `+0000` (FND-EXE-599) with `0x10` for 10. Passed to `3A8E:0D3C` in `+0040` for a window on a state 2 slot. |
| `+0040` | `0x630F0..0x633C2`, `0x633C5..0x63743` | `56A2:0025` | Takes a slot, `x` and `y`. Sets `DS:08CC` to 1 and `DS:655C` to 0, takes `a` from `1695:0834` and `b` from `28C9:28E3` for the slot (`b` without bit `0x80`, or 8 when only that bit is set and `a` is 0; `a` masked with `0x1D`), and calls `1695:02CA` with `DS:426D` and the slot. When that changes the slot record's word `+0x1B`, or `a` without bit 8 and `b` are both 0, or the record's word `+1` is -1, or the state byte is not 1, 2 or 5, it sets `DS:0DA4` to 1 when `DS:655C` is 0 and returns. When `b` is 1, or the state is 1 with an item whose 23-byte record's word `+0` has an absolute value from 900 to 910, and `a` is 0, it hands the slot to overlay 191 (`+0BE6` with the record's word `+5` twice when `DS:1A34` is not -1, else `+01F7`) and calls `28C9:2535` with 3 when overlay 191 `+0000` returns nonzero. Otherwise `b` or `a` of 4 calls `2C5F:018C` with the slot and 1 and `1695:0218` with `DS:426D` and the slot; `b` of `0x10` with `a` 0 calls overlay 182 `+0C23` with `DS:426D`, the slot and 0; `b` or `a` of `0x40` calls overlay 175 `+002C` with the slot (FND-EXE-598); a state of 5 returns. In any other case it opens the window when it is not open, at `x`, `y` or at `0xD0`, `0x38` when `a` has bit 8: for state 1 resource 15500 with `+06EA` as handler and `+0000` as callback, and the states of controls `0x3BC7`, `0x3BC5` and `0x3BC6` 0 when bit 1, 4 or `0x10` of `b` or `a` is set and 1 when not; for state 2 resource 3020 with `+06EA` and `+0020`, and controls `0x3BCC`, `0x3BCA` and `0x3BCB` in the same way. Control `0x3BC8` gets state 1 unless the state is 1 and the item's 15-byte record has neither bit `0x10` nor bit `0x20` in byte `+4`. It registers `+06EA` for control `0x3B60` through `3F96:02AB`, calls `3A8E:060D`, then overlay 213 `+0BFB` with the item and the window for state 1 or `+0F1F` with the window and the slot for state 2, and stores the slot at `DS:08CA`. Ends with `28C9:2A81` and 1, overlay 182 `+059C` with `0x4A9D` and 1, `28C9:2A44` with 1, and `DS:9D76` set to 0. The `jmp` at `0x633C2` follows another `jmp` and nothing reaches it, so the row leaves its three bytes out. Called from `+0D40` (at `+0F15`). |
| `+0693` | `0x63743..0x63752` | `56A2:0020` | Calls `+06A2`, then overlay 182 `+19F8`. Called five times from `+06EA` and from resident `28C9:1164` (in `28C9:0CFF`) and `28C9:13E4` (in `28C9:1261`). |
| `+06A2` | `0x63752..0x6379A` | `56A2:0034` | When the window is open, closes it with overlay 182 `+0128` and 1 and clears `DS:1431` and `4DA0:0000`. Sets `DS:08CA` to `0x270F` and calls `2C5F:0182`. Called from `+0693`. |
| `+06EA` | `0x6379A..0x63CF3` | `56A2:0039` | The window's handler, for an event kind, a control and a value. Kind 1 calls `+0693` and returns -1. Kind 5 for control `0x3B60` goes through the 6-value table at `+0C53`: `0x40` and `0x100` set `DS:9D76` to 1 and, like `0x20` and `0x80` when `DS:9D76` is set, call `+0693` and `3D72:0D83`; then these and value 2 write an empty label to control `0x3BCD` (state 2 slot) or `INFO` to `0x3BC8` with `3EBE:07FF`, through the window pointer that `+06A2` has cleared when it ran. Value 4 takes the rectangle of control `0x3B60` (`3F96:01A6`) and, when overlay 190 `+077A` puts the point inside it and the point lies in the row from `0x3B` (state 2) or `0x3E` down by 15, finds which of three 15-wide cells 20 apart from `x` 3 (state 2) or `0x0D` holds it and writes `TALK`, `GET` or `USE` (from the far pointers at `DS:08CD`) to `0x3BCD` or `0x3BC8`, with `OPEN` for the third, on a slot not in state 2, when the slot's item has bit 8 in byte `+0x0B` of its 15-byte record; it calls overlay 182 `+1FDB` with 4 or 6 and `3EBE:042B` for the `BUTN` control (`39D1:049C`). Kind 2 goes through the 8-word table at `+0C43` for controls `0x3BC5` to `0x3BCC`: `0x3BC7` and `0x3BCC` close the window and hand the slot to overlay 191 (`+0BE6` with the record's words `+0x0A` and `+0x0C` when `+0000` returns nonzero, else `+01F7`), then when `+0000` is nonzero call `28C9:2535` with 3, overlay 182 `+059C` with the result of overlay 191 `+0013` and 0, and `1695:0290` with `DS:426D` and `DS:1A32`; `0x3BC5` and `0x3BCA` close it and call `2C5F:018C` and `1695:0218` as in `+0040`; `0x3BC6` and `0x3BCB` close it, call `3D72:0D83` and overlay 182 `+0C23`, and for a state 1 slot whose item has bit 8 in byte `+0x0B` of its 15-byte record overlay 175 `+002C`; `0x3BC8` toggles `DS:08CC`, calls `3A8E:060D`, and when `DS:08CC` is set calls overlay 213 `+0BFB` with the item, and when it is clear writes `USEABLE BY:` and below it, 8 apart through `2C5F:09F3`, the name at `+0x21` of the 49-byte record `j` under `DS:19C9` for each `j` from 0 to 3 whose 66-byte record under `DS:19C5` has a nonzero word `+0x0E` and a word `+0x10` that shares a bit with the item's 15-byte record's word `+0x0C`, or `NO ONE`; then `28C9:2A81` with 1. `0x3BC9` does nothing. Ends with `2C5F:0182` and returns 0. Passed to `182+016B` and `3F96:02AB` in `+0040`. |

## Interpretation

`+0040` is what clicking a slot does when overlay 178 `+0D40` receives it: some slots act at
once, through overlay 191, the talk routines `2C5F:018C` and `1695:0218`, overlay 182 `+0C23` or
overlay 175 `+002C`, and the rest open a small window with three buttons whose labels show on
hover, which fits talk, get and use. Its info control switches between the item details of
overlay 213 and the list of party members who can use the item. `+0693` and `+06A2` close the
window. The names stay `fn_178_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. Which of the three buttons is talk, get and use rests on the
routines each calls, not on the resource; `1695:0834`, `28C9:28E3`, `1695:02CA`, `1695:0218`,
`2C5F:018C` and overlay 191 `+0BE6` and `+01F7` are not read here. Passing the record's word `+5`
twice to overlay 191 `+0BE6` in `+0040`, where `+06EA` passes the words `+0x0A` and `+0x0C`, may be
a slip in the original or an argument that `+0BE6` ignores. `3EBE:07FF` may do nothing for a
zero window pointer.

## How to reproduce

Run `overlay_listing.py` for overlay 178 against the installed `DSUN.EXE` from `0x630B0` to
`0x63D1B` and `direct_callers.py` for each offset in the table (`178+<offset>`), read the tables
at `+0C43` and `+0C53` and the far pointers at `DS:08CD`, and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
