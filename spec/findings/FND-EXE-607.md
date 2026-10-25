---
id: FND-EXE-607
title: The 8 routines of overlay 207 that no entry placed sort a store's items into a page of six slots, draw the store panel and its slots, move a bought item onto the pointer, draw the character panel and the party's money in the inventory window, and adjust a buying and a selling price by the words at 4E71:0036 and 4E71:0034
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000906D5..0x000909F5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00090C1D..0x00090DD4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000910F9..0x000915B6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000915B8..0x000916C9
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 8 rows of overlay 207 (header segment `57A0`, 12
trampolines, code from file offset `0x90180`, 5,451 bytes) that no spec location covered, 2,725
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x90180` plus it, and `DS` is `57E0`. Every routine has a trampoline in `57A0`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. Overlay 207 is the store of FND-PARTY-245 and RULE-ITEM-004. The
store's list is the 10-byte list that `2D40:2196` fills for the store object `DS:1A30` into the
far pointer `DS:619D`, with an entry's kind in word `+2` and its item in word `+4`
(fn_2D40_2196); overlay 190 `+3900..+395E` allocates it and sets the far pointer `DS:6199` to
the 49-byte record (FMT-COMBAT-001) of the slot that word `4F49:0C34 + 3 * store` names. An
item's 23-byte record under the far pointer `DS:19C1` has its object number in the signed word
`+0` (FMT-PARTY-006), its quantity in word `+2` and its value in word `+6` (FND-PARTY-245), and
its position byte at `+0x13` (FND-PARTY-217). The six words at `DS:9B7A` hold, for store slots 0
to 5, the list index shown there or `0xFFFF`; the slots are identifiers `0x1E` to `0x23` of
overlay 189's routines (FND-EXE-598). The word `DS:2932` is the page, and `4C13:0357` the party's
money (FND-PARTY-245). `DS:1431` is the window the routines draw into.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0555` | `0x906D5..0x908B9` | `57A0:002A` | Takes a far pointer to a 49-byte record and the store object. Sets the six words at `DS:9B7A` to `0xFFFF` and returns when the record's word `+6` is 0. Fills the list through `2D40:2196`. For each entry `i` of kind 1 whose item is below `0x270F`, it looks for a later entry of kind 1 and item below `0x270F` whose record has the same object number; when there is one it writes `0xFF` to the position byte of `i`'s record and skips `i`. Otherwise it adds 1 to a count `c`, writes `c` to the position byte, and when `c - 1` lies from `6 * DS:2932` to `6 * DS:2932 + 5` (unsigned) stores `i` in word `c - 1 - 6 * DS:2932` of `DS:9B7A`, noting that the page is not empty; otherwise it notes an item off the page. After the list, when nothing fell on the page and `c` is not 0, it sets `DS:2932` to 0 and starts again from the clearing of `DS:9B7A`. Ends by setting the state of control `0x33F7` of the window `5072:003C` through `3EBE:071F` to 0 when `c` is above 6 and to 1 otherwise. Called from `+0875` at `+0A00`, from `+0A9D` at `+0AB3`, `+0BA4` and `+0C4A`, from overlay 189 `+0053` and `+0A9E` with `DS:6199` and `DS:1A30`, and from overlay 190 `+395E`. |
| `+0739` | `0x908B9..0x909AD` | `57A0:0025` | Takes no argument. Calls `28C9:2ABD` and overlay 190 `+3456` with 2, `0xE9`, 4 and 0. For the controls `0x33F7` and `0x33F8` it looks up the `BUTN` record of the window `5072:003C` through `39D1:049C` and passes it to `3EBE:042B`. Loads `BMP ` 12000 through `38FF:04AB` into `5072:0038` and, when that is set, draws its frame 0 at (`0xEE`, 6) with `2D40:3BEC` and frees it through `444C:0092`, keeping the result there. Calls overlay 199 `+1567` with `DS:25BD` (FND-EXE-603), `+082D` with each slot 0 to 5, and `28C9:2A81` with 0. Called from `+0875` at `+08B0` and `+0A07`, and from overlay 189 `+005B` and `+0AA6`. |
| `+082D` | `0x909AD..0x909F5` | `57A0:004D` | Takes a slot `s`. Calls overlay 189 `+1893` and `+1946` with `s + 0x1E`, and when word `s` of `DS:9B7A` is not `0xFFFF` also overlay 189 `+1AD7` and `+0C54` (the price line of FND-PARTY-245) with `s + 0x1E`. A `cmp` of `s` with 6 at `+0834` has no branch after it. Called from `+0000` at `+04FB`, `+0739` at `+0818` and `+0875` at `+08F5`. |
| `+0A9D` | `0x90C1D..0x90DD4` | `57A0:003E` | Takes the slot `s` of a sale, as `+0875` passes it after `SOLD!`. Calls `+0555` with `DS:6199` and `DS:1A30` and fills the list again. Notes whether any other entry's record has the same object number as the record of the entry in word `s` of `DS:9B7A`. Fills the list at `5072:0040` for `DS:1A32` through `2D40:2196` and calls `2D40:288C` with the store list, that entry, -1, the list at `5072:0040`, 0 and -1, going to the end when it returns 0. Fills `5072:0040` again and stores in `DS:1A34` the index of its first entry of kind 1, or the count when there is none. Calls `+0555` again. When another entry had the same object number, it calls overlay 188 `+1184` with `DS:1A30`, the absolute value of the object number and the quantity of the record that word `5072:0044` names, the store list, a local buffer and -2; a `cmp` of its result with that quantity has no branch after it. Ends with a third call of `+0555`. Called from `+0875` at `+09E4`. |
| `+0F79` | `0x910F9..0x9128C` | `57A0:0057` | Takes no argument. Sets controls `0x33F7` and `0x33F8` of `5072:003C` to state 1 through `3EBE:071F` and calls overlay 190 `+3456` with 2, `0xE9`, 4 and 0. For the party member `m` in `4E71:0B44`, returns there when word `+0x0E` of its 66-byte record (`DS:19C5`) is 0, an empty box (FND-PARTY-046). Otherwise it sets `DS:2D14` to `0x6C`, calls overlay 183 `+1D52` with `DS:1431`, `0xEC` and `0x35`, and prints the six current scores, bytes `+0x19` to `+0x1E` of `m`'s 49-byte record (FND-PARTY-161), with `%C%C%C%d` (`DS:29A0`) at (`0x104`, `0x35 + 7 * i`) through `401B:0172`. It prints `%C%C%CPSI:` (`DS:29A9`) at (`0xEC`, `0x63`) and calls overlay 183 `+191D` with `DS:1431`, the two records, `0x119` and `0x63` (FND-PARTY-059); calls overlay 183 `+19A0` with `DS:1431`, the result of overlay 173 `+2C00` with `m`, `m` and 0 (FND-PARTY-145), `0xEC` and `0x71`; and calls overlay 190 `+2A97` with `DS:1431`, `m`, `0xEC`, `0x78`, `DS:2D14` and `DS:2D16`. Called from overlay 189 `+0043`, when `DS:1A30` is `0x270F`, and `+105A`, `+288F` and `+29F5`. |
| `+110C` | `0x9128C..0x914A5` | `57A0:002F` | Takes no argument. Loads `BMP ` 13000 into `5072:0038`, draws its frame 0 at (`0x39`, `0xB7`) and frees it as `+0739` does. Formats the signed double word `4C13:0357` = `M` into an 80-byte local buffer through `3150:000E`: above 999,999,999 with `%d,%03d,%03d,%03d$` and the groups `M / 10^9`, `M / 10^6`, `M / 1000` and `M`, each modulo 1000; above 999,999 with `%5d,%03d,%03d$`; above 999 with `%9d,%03d$`; and otherwise with `%12d$` and `M` modulo 1000, which is `M` from -999 to 999. Prints the buffer with `%C%C%C%s` (`DS:295E`) at (`0x3C`, `0xB9`) through `401B:0172`. Called from `+0875` at `+09D2` and `+0D74` at `+0ED5`, and from overlay 189 `+0104` and `+2720`. |
| `+1325` | `0x914A5..0x915B6` | `57A0:0034` | Takes a price `p`, a double word, and adjusts it by the signed word `w` at `4E71:0036`. With `w` 0 it returns `p` unchanged. With `w` from 1 to 100 it adds `w`; above 100 it takes `p * w / 100`; from -100 to -1 it subtracts `w`, which adds its magnitude, and takes 1 when `p - w` is negative; below -100 it takes `p * (w + 200) / 100`, and 1 when that is negative. All products and quotients are signed 32-bit. On these paths it then takes 1 when the price is 0 or less. Returns the price in `DX:AX`. Called from `+0875` at `+0935` and from `+0C54` at `+0D14`. |
| `+1438` | `0x915B8..0x916C9` | `57A0:0039` | The same as `+1325` with the word at `4E71:0034`. Called from `+0D74` at `+0DDB`. |

The rows `+1325` and `+1438` end two bytes before their routines' `pop bp` and `retf`.

## Interpretation

`+0555` lays the store's stock out as pages of six: copies of one object show as one slot, the
last copy in the list, and the rest get position `0xFF`; a page left empty sends the store back
to page 0; and control `0x33F7` is enabled only when there are more than six, which fits the MORE
button of RULE-ITEM-004. `+0739` draws the store side of the inventory window and `+082D` one of
its slots with its price. `+0A9D` completes a purchase: after the price comes off the money it
moves the store's entry into the list at `5072:0040` and puts the item on the pointer, and when
the store held another copy of the object it calls overlay 188 `+1184` with the object number and
quantity, which may restock or merge the copy. `+0F79` draws the character side of the same
window when no store is open (`DS:1A30` is `0x270F`): the six scores, psionic points, armor class
and the line of overlay 190 `+2A97`, with both store buttons disabled. `+110C` prints the party's
money with thousands separators. `+1325` and `+1438` are the store's buying and selling price
adjustments: a small positive word adds that many units and a word above 100 is a percentage,
while a word below -100 is a percentage of 200 less its magnitude. For a word from -100 to -1 the
code adds the magnitude, so a small discount raises the price; a sign slip is the plainest reading.
The names stay `fn_207_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. Overlay 188 `+1184`, overlay 183 `+1D52` and `+19A0`, overlay
190 `+3456` and `+2A97`, overlay 189 `+1893`, `+1946` and `+1AD7`, `3EBE:042B`, `39D1:049C`,
`2D40:288C` and `28C9:2ABD` are not read here; the reading of them as restocking, drawing panel
parts, drawing slots and moving an entry rests on their arguments. Who writes `DS:2932`,
`4E71:0034` and `4E71:0036` was not searched, so the page and the two price words are named from
how these routines use them. The scores and the record whose word `+6` `+0555` tests are taken
from the cited formats; that `DS:6199` names the store keeper's record rests on overlay 190
`+3922..+3946`. The store's handling of a negative word from -100 to -1 may be intended.

## How to reproduce

Run `overlay_listing.py` for overlay 207 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`207+<offset>`), and compare the ranges with the
rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. Read overlay 190 `+3900..+3963` for the list and
`DS:6199`, and overlay 189 `+0028..+0063` for the calls of `+0F79`, `+0555` and `+0739`.
