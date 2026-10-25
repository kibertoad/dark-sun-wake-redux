---
id: FND-EXE-622
title: The 6 routines of overlay 191 that no entry placed drop the first item of the object slot in DS:1A32 as a new object, move a lying object's item into another slot's held objects, test for a worn item of a given number, remove a held item, hand a slot to the drop or the party paths by its state, and step a point off the placed entries already on it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007C7CB..0x0007C8A3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007CBB7..0x0007CCFA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007CD7C..0x0007CEC0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D0F5..0x0007D212
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 6 rows of overlay 191 (header segment `572F`, 17
trampolines, code from file offset `0x7C1F0`, 4,130 bytes) that no spec location covered, 1,148
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x7C1F0` plus it, and `DS` is `57E0`. Every routine has a trampoline in `572F`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. A held-object list is what `2D40:2196` writes for an object slot:
10-byte entries with the kind in word `+2` and the item in word `+4`, and their count as its
result (`fn_2D40_2196`). An item is a 23-byte record at the far pointer `DS:19C1`
(`item_records`). A slot's state byte is at `4F49:0C33 + 3 * slot` (FND-PARTY-131), its 37-byte
record at `DS:67BB + 37 * slot`, and its placed entry is the 8-byte entry under the far pointer
`DS:67B7` that the record's word `+1` numbers, with its position in words `+0` and `+2`;
`DS:264E` of them are in use (FND-COMBAT-054). The event state is the word `28C9:2522` returns
(FND-CONFIG-117).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+05DB` | `0x7C7CB..0x7C8A3` | `572F:002F` | Takes `x`, `y` and a flag byte. Lists the held objects of the slot in `DS:1A32` into `5072:0040` and takes the first entry of kind 1 (or the index past the last when there is none). Places an object through `31E0:0121` (`fn_31E0_0121`) with the negated word `+0` of the entry's item, -1, `x`, `y`, 0, the item's byte `+0x12` and the flags `0x08`, or `0x0B` when the flag byte is set, and returns when that gives -1. Otherwise it lists the new slot's held objects into a stack buffer and calls `2D40:288C` (`fn_2D40_288C`) with `5072:0040`, the entry, -1, the buffer, 0 and -1; when that returns nonzero in `AL` it stores -1 in `DS:1A34`. Called from `+0BE6` at `+0C76` and `+0CC6`, from `+01DE`, `+083A` and `+0926`, and from overlay 184 `+22AD` and overlay 189 `+0A6C`. |
| `+09C7` | `0x7CBB7..0x7CC84` | `572F:0034` | Takes a slot `t` and a slot `s`. When `s`'s state byte is 0 it calls `2D40:000A` with `s`, its record word `+0x1B` as a double word, `RDFF` and the low three bits of byte `+5` of its placed entry. Lists the held objects of `t` and of `s` into two stack buffers, takes the first kind-1 entry of `s`'s list and calls `2D40:288C` with `s`'s list, that entry, -1, `t`'s list, 0 and 4, as `drop_held_objects` does (RULE-COMBAT-016). When that returns nonzero in `AL` it releases `s` through `31E0:05BB` with its record word `+1`, 1, 1 and 1. Called from one resident far call at `2D40:1C9D`, which passes the two slots after `2D40:35EF` of them returns 2 or less. |
| `+0A94` | `0x7CC84..0x7CCFA` | `572F:005C` | Takes a number `a` and a far held-object list. Asks overlay 173 `+2E1B` (FND-EXE-600) with -2, the list, 1 and a stack count for an entry, and returns 0 when it gives -1, when the entry's kind is not 1, or when the item's signed byte `+0x13`, its position (FND-PARTY-217), is above 13. Returns 1 when the item's word `+0` equals `a`, and 0 otherwise. No direct call was found. |
| `+0B8C` | `0x7CD7C..0x7CDD6` | `572F:0043` | Takes a slot and an item number. Lists the slot's held objects into a stack buffer and removes the first kind-1 entry whose item is that number through `2D40:2452` with the buffer and the entry's index. Called from overlay 190 `+2622` (FND-PARTY-243) and overlay 213 `+0807` and `+08EF`. |
| `+0BE6` | `0x7CDD6..0x7CEC0` | `572F:006B` | Takes `x`, `y` and a slot `s`. When `s`'s state byte is 0 it calls `2D40:000A` as `+09C7` does and returns when that is nonzero. Then by the state byte: for 1 it calls `+05DB` with `x`, `y` and 1 when the event state is 1 (0 otherwise), and when overlay 191 `+0000` then returns 0 in `AL` calls `+01F7` with `s` and `1695:0290` with `DS:426D` and `DS:1A32`; for 2 it calls `+074B` with `s` when `s` is 0 to 3; for any other value it calls `+05DB` as for 1 and returns. Called from `+019D` and from overlay 178 `+02EA` and `+0B37` (FND-EXE-604). |
| `+0F05` | `0x7D0F5..0x7D212` | `572F:0039` | Takes far pointers to `x` and `y`. Searches the placed entries by bisection, taking them as sorted by word `+2` and then word `+0`, for one at (`x`, `y`). While one is found it moves the point: of two step words `a` and `b` that start at 0 it subtracts 4 from `a` when they are equal and from `b` otherwise, adds `a` to `x` and `b` to `y` through the pointers, and searches again. Called from one resident far call at `31E0:01C9`, inside the placement routine `31E0:0121`. |

## Interpretation

`+05DB` drops the item the slot in `DS:1A32` holds onto the ground at a point as a new placed
object and moves the held entry into it, then stores -1 in `DS:1A34`. `+09C7` is the reverse for an object lying within reach of a slot: the slot takes
the object's item and the object is released. `+0BE6` sends slot `s` to the drop for every state
other than 2, and to `+074B` for a party member in slots 0 to 3 with state 2 (FND-PARTY-131). `+0A94` asks whether a worn or held item (position 0 to 13) has a given number,
and `+0B8C` removes a used-up item from a slot (FND-PARTY-243). `+0F05` keeps two placed objects
from sharing one point, stepping a new object 4 pixels at a time up and to the left until the
point is free. The names stay `fn_191_<offset>` until a reading shows what the fields and tables
are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+0A94` in particular may be called only through its trampoline
by a computed far call. `2D40:000A`, `2D40:288C`, `2D40:2452`, `2D40:35EF`, `31E0:05BB`,
`1695:0290`, and overlay 191 `+0000`, `+01F7` and `+074B` are not read here; the reading of them as
loading an object's `RDFF`, moving and removing held entries, measuring distance, releasing a
slot and handing an item to a party member rests on their arguments and on the entries cited.
Which of `x` and `y` is the column is taken from FND-COMBAT-054's order of the placed entry's
words. The bisection in `+0F05` assumes the placed entries are kept in that order; this finding
does not show where they are sorted.

## How to reproduce

Run `overlay_listing.py` for overlay 191 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`191+<offset>`), and compare the ranges with the
rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. Run `resident_listing.py` over
`2D40:1C88..2D40:1CA8` for the caller of `+09C7`.
