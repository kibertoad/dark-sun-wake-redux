---
id: FND-EXE-614
title: The 32 routines of overlay 188 that no entry placed are script-instruction helpers that move an object to a region and cell, find a free cell near a point, move items between holders, read a number or text typed into the dialog, ask a yes or no question, add dialog lines, roll a thief skill with a modifier, save and restore the move records of slots 5 to 0x2F, and stop a script
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073331..0x00073340
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073378..0x00073398
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073670..0x000736A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073704..0x00073890
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073948..0x00073A8D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073D00..0x00074024
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007418A..0x000742E5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00074412..0x000744A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00074591..0x0007464C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00074667..0x000747E6
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 32 rows of overlay 188 (header segment `5702`, 49
trampolines, code from file offset `0x72EA0`, 6,470 bytes) that no spec location covered, 2,692
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x72EA0` plus it, and `DS` is `57E0`. Every routine has a trampoline in `5702`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them; a caller in segment `172C` is named by the script instruction whose
handler it is in the word table at `DS:030A` (FND-SCRIPT-005). A slot's 37-byte record is at
`DS:67BB + 37 * slot`, its state byte at `4F49:0C33 + 3 * slot`, and `2D40:3E64(slot, &a, &b)`
gives its 49-byte record `a` under the far pointer `DS:19C9` and its 66-byte record `b` under the
far pointer `DS:19C5` (FND-EXPLORE-002). An item list is the 10-byte entry list that `2D40:2196`
fills for a holder: an entry's word `+2` is its kind, its word `+4` an index into the 23-byte
records under the far pointer `DS:19C1` (FND-EXE-600), whose word `+2` is the quantity
(FND-PARTY-216). The dialog is overlay 199's (FND-EXE-603): `+02A0` takes an operation, a far
string and a word, `+0F68` waits for a choice and returns its number from 1, `+1452` waits until
the text is dismissed, and `+0C21` closes it. `2D40:06E1` is a far routine that returns at once.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0491` | `0x73331..0x73340` | `5702:00C5` | Calls overlay 169 `+0174` (trampoline `565C:0025`) and clears `DS:193D`, the byte that `+046C` fills from overlay 169 `565C:0020` (FND-CONFIG-149). Called from overlay 180 `+07AA`. |
| `+04D8` | `0x73378..0x73388` | `5702:002F` | Passes its word argument to `2D40:06E1` and returns 0. Called from the handler of instruction `0x45` (`172C:1677`). |
| `+04E8` | `0x73388..0x73398` | `5702:0034` | The same as `+04D8`. Called from the handler of instruction `0x46` (`172C:1692`). |
| `+07D0` | `0x73670..0x736A9` | `5702:008E` | Takes a slot, an index `k` and a skill. Calls overlay 204 `+0A0B` with the slot, the skill as a double word and the signed byte `DS:193F + k` as a double word, and returns 1 when that returns nonzero and 0 otherwise. The bytes from `DS:193F` are 20, 0, -10, -20, 5, 0, -5 and -10. Called from the handler of instruction `0x58` (`172C:1763`) twice. |
| `+0864` | `0x73704..0x73890` | `5702:00A7` | Takes a slot, a region, a cell `(cx, cy)`, a byte passed to `2D40:06E1` and a flag. Returns 0 when the slot's state byte is 0. Closes the dialog (`+0C21`). When the region is `current_region` (`DS:140C`): calls `+0AA8` with the cell, the flag, 0 and byte `+0x37` (`footprint`) of the slot's 66-byte record (0 when `2D40:3E64` returns 0), and returns 0 when no cell is found. For a slot below `0x30` it stores `0xFF` in the byte at `4F49:0374 + 0x1C * slot` and 0 in the word at `4F49:08A7 + 0x13 * slot`. It calls `31E0:4129` with the slot and the found point unless the slot is a party member other than the leader (`4C13:0369`) while `DS:13FA` is 0; for the leader it calls `2C5F:018C` with the slot and 1. Then `1695:01D1` with the slot, the point and 0 for the leader, 1 for another party member or 2 for any other slot. When the region is another one other than `0xFF`, it returns overlay 187 `+135B` of the slot, the region and the cell's centre (`16 * c + 8`); for `0xFF` it removes the slot through `31E0:05BB` (record word `+1`, 1, 0, 0). Ends with `2C5F:0139` and `28C9:2A81` with 1 except after the leader's placement. Returns 1 on success. Called from `+0A35` and twice from the handler of instruction `0x5E` (`172C:1A43`, FND-UI-051). |
| `+0AA8` | `0x73948..0x73A8D` | `5702:00CF` | Takes a cell `(x, y)`, near pointers to an `x` and a `y`, a flag byte, a test byte and a `footprint` byte `f`. Stores `f & 7` in `DS:19B2` and `f / 8` in `DS:19B4` (FND-EXPLORE-002). For `r` from 0 to 19 it scans the cells from `(x - r, y - r)` to `(x + r, y + r)` column by column for one with column 0 to `0x7F` and row 0 to `0x61` for which `2D40:192C` does not return -1; with the flag at 1 the cell's centre must also lie more than 64 pixels outside the view (`DS:1408` and `DS:140A` plus 320 by 200), and with the test byte set `1B0B:00A4` from `(x, y)` to the cell must return 0. Stores the last cell's centre (`16 * cell + 8`) through the pointers and returns 1 when a cell passed and 0 otherwise. Called from `+08EB` and `+0C8B` and overlay 197 `+34B0`. |
| `+0E60` | `0x73D00..0x73D51` | `5702:00D9` | For each party slot 0 to 3 for which `2D40:3E64` returns nonzero and whose 49-byte record has `control_locked` (bit 6 of byte `+0x18`, FMT-COMBAT-001) clear, clears `computer_control` (bit 5). Called from the key routine `28C9:0CFF` at `28C9:1131`, the Space key of FND-AI-004. |
| `+0EB1` | `0x73D51..0x73DA5` | `5702:00A2` | Takes a count `n`, an item number, a giver and a receiver. When the giver is below 4 it first calls `+0F27` to move up to `n` items from the holder in `DS:1A32` to the receiver, then for the rest from the giver, with -2 as the last argument each time. Returns the total moved. Called from the handler of instruction `0x5C` (`172C:1989`) twice and overlay 195 `+127B`. |
| `+0F05` | `0x73DA5..0x73DC7` | `5702:0070` | Takes `n`, an item number, a receiver, a giver and a word `e`, and returns `+0F27` of the giver, the receiver, `n`, the item and `e`. Called from the handler of instruction `0x39` (`172C:12DD`) and overlay 197 `+3B20`. |
| `+0F27` | `0x73DC7..0x74024` | `5702:00DE` | Takes a giver, a receiver, a count `n`, an item number (`0x7FFF` for any) and a word `e`, and maps giver and receiver through `2D40:0D94` with a cursor of 0. When the giver maps to `0x270F` and the receiver is a slot whose state byte is 1 or 2, it adds `+1184` of the receiver, the item's absolute value, `n`, both stack lists and `e` to the count. Otherwise it repeats: lists the giver's items and takes the first kind-1 entry whose 23-byte record's word `+0` is the item. With none it stops. When the receiver maps to `0x270F` it removes the entry through `2D40:2452` when the quantity is 1 or less and otherwise decrements it, counting 1. For a receiver of state 1 or 2: when the quantity is above `n` and above 1 it adds `+1184` (receiver, item, `n`, lists, -2) to the count and subtracts `n` from the quantity; otherwise it moves the whole entry into the receiver's list through `2D40:288C` (giver list, entry, -2, receiver list, 0, 4) and counts the quantity (at least 1), stopping when that fails. It repeats while the count is below `n`. When the giver was the holder in `DS:1A32` and something moved, it sets `DS:1A34` to `0xFFFF` if that slot's state byte is 0 and refills the list at `5072:0040` for `DS:1A32`. Returns the count. Called from `+0ED3`, `+0EF3` and `+0F1C`. |
| `+12EA` | `0x7418A..0x741C1` | `5702:0075` | Takes a far pointer. Calls `+13CF`, copies the 80-byte buffer `4E71:0A71` to the stack, converts it with `1000:2E86` and stores the double word result through the pointer. Called from the handlers of instructions `0x3B` (`172C:136B`) and `0x0D` (`172C:1F71`). |
| `+1321` | `0x741C1..0x74205` | `5702:007A` | Takes a far pointer and, as `+12EA` does, stores the converted word through it, asking again while the value is above the double word at `4C13:0357`. Called from the handler of instruction `0x44` (`172C:165D`). |
| `+1365` | `0x74205..0x74237` | `5702:007F` | As `+12EA`, storing a word. Called from the handlers of instructions `0x43` (`172C:1643`) and `0x0D` (`172C:1F71`, four times). |
| `+1397` | `0x74237..0x7426F` | `5702:0084` | Takes a far buffer and a length. Calls `+13CF` and `1000:40F6` with `4E71:0A71`, and copies `4E71:0A71` to the buffer with `1000:3F5A`, the length cut to 80. Called from the handlers of instructions `0x42` (`172C:1610`) and `0x0D` (`172C:1F71`). |
| `+13CF` | `0x7426F..0x742E5` | `5702:00E8` | Clears `DS:143E` and calls `+02A0` with operation 4, which adds an input field. While `DS:143E` is 0 it calls `2D40:3E34`, builds an event with `39D1:097D` and passes it to `39D1:071F`. Then reads control `0x3071` of the window under the far pointer `52A1:0008` into `4E71:0A71` through `4228:00A9` with `0x4F`, and closes the dialog (`+0C21`). Called from `+12F2`, `+1329`, `+136D` and `+13A0`. |
| `+1572` | `0x74412..0x7444A` | `5702:0020` | Adds the lines `Press continue` and `Continue` (operation 0), shows the choice (operation 3) and returns `+0F68`. Called from the handler of instruction `0x2B` (`172C:101C`). |
| `+15AA` | `0x7444A..0x744A4` | `5702:002A` | Adds the lines `Answer Yes or No`, `Yes` and `No`, shows the choice and returns 1 when `+0F68` returns 1 and 0 otherwise. Called from the handler of instruction `0x38` (`172C:12BC`). |
| `+16F1` | `0x74591..0x745A4` | `5702:0061` | Calls `+02A0` with operation 1 and picture 0. Called from the handler of instruction `0x2A` (`172C:1016`). |
| `+1704` | `0x745A4..0x745B7` | `5702:0089` | Passes the signed byte under its far pointer argument to `2D40:06E1`. Called from the handler of instruction `0x2C` (`172C:1022`). |
| `+1717` | `0x745B7..0x745CC` | `5702:009D` | Takes `n` and calls `+02A0` with operation 1 and picture `n`. Called from the handler of instruction `0x54` (`172C:1742`). |
| `+172C` | `0x745CC..0x745E5` | `5702:0048` | Takes a byte `n` and a far string and calls `+02A0` with operation 0, the string and `n`. Called from `172C:2CF8`, the menu of instruction `0x48` (FND-TALK-001), twice. |
| `+1745` | `0x745E5..0x745EC` | `5702:00ED` | Returns 0. No direct call was found. |
| `+174C` | `0x745EC..0x74609` | `5702:004D` | Takes a far string and a byte, calls `+02A0` with operation 2, the string and the byte, and waits with `+1452`. Called from the handler of instruction `0x4F` (`172C:16BD`). |
| `+1769` | `0x74609..0x74637` | `5702:0052` | Takes a double word and a byte, writes the double word in decimal into an 80-byte stack buffer with `1000:31FB` and radix 10, and calls `+02A0` with operation 2, the buffer and the byte. Called from the handler of instruction `0x50` (`172C:16E1`). |
| `+1797` | `0x74637..0x7464C` | `5702:0057` | Calls `+02A0` with operation 2, the one-line string at `DS:1983` (a line feed) and 0. Called from the handler of instruction `0x51` (`172C:1705`). |
| `+17C7` | `0x74667..0x74682` | `5702:00F7` | Takes a near pointer, passes `Illegal Storage` (`DS:198F`) to overlay 180 `+0867`, and passes the word under the pointer to `2D40:06E1`. No direct call was found. |
| `+17E2` | `0x74682..0x746C0` | `5702:00FC` | For each slot 5 to `0x2F` copies the first 6 bytes of its 19-byte record at `4F49:08A3 + 0x13 * slot` (FND-EXPLORE-004) to `DS:55CB + 6 * slot` with `1000:3F5A`, and stores the leader byte of `4C13:0369` in `DS:55CA`. Called from overlay 173 `+0659`. |
| `+1820` | `0x746C0..0x74716` | `5702:0101` | For each slot 5 to `0x2F` whose saved word at `DS:55CF + 6 * slot` is not negative copies the 6 saved bytes back, then calls `2D40:3D4C` with the byte `DS:55CA` and the word `4C13:0369`. Called from overlay 173 `+07B2`. |
| `+1876` | `0x74716..0x7475D` | `5702:0106` | Takes a slot `s`. For each slot 5 to `0x2F` whose 19-byte record has word `+4` other than 15 and word `+0` equal to `s`, sets word `+4` to 0. Called from the slot release `2D40:03C1` (FND-PARTY-136). |
| `+18BD` | `0x7475D..0x747A1` | `5702:010B` | Takes a slot `s`. Calls `2D40:2F34` for every slot `0x30` to `0x13F` whose state byte is 2, and returns the value it gave for `s`, or `s` when `s` was not among them. Called from `+04B0` and overlay 173 `+08E3`. |
| `+1901` | `0x747A1..0x747CE` | `5702:00B1` | Closes the dialog (`+0C21`); when `DS:143C` is set passes `BAD GPL EXIT` (`DS:19A0`) to overlay 172 `+05DB`; calls `172C:00EE`, which sets the script's stop byte, and clears the byte `4F49:000A` (FND-CONFIG-056, FND-CONFIG-135). Called by 17 far calls in segment `172C`, among them the dispatcher `172C:01C1` and the stop routine `172C:20B3`. |
| `+192E` | `0x747CE..0x747E6` | `5702:0025` | Calls `+02A0` with operation 3 and returns `+0F68`. Called from `172C:2CF8` (FND-TALK-001). |

## Interpretation

Overlay 188 holds the far routines through which the script interpreter's handlers reach the rest
of the game. `+0864` moves an object to a cell of the current region, near the given cell, or hands
it to overlay 187 for another region, or unloads it for region `0xFF`, which is what instruction
`0x5E` does with an object. `+0AA8` is the free-cell search it uses, with the object's footprint
and an option to keep the cell out of sight. `+0EB1`, `+0F05` and `+0F27` move a number of items of
one kind from one holder to another, destroy them when the receiver is `0x270F`, or create them
through `+1184` when the giver is `0x270F`. `+12EA`, `+1321`, `+1365` and `+1397` read a long, an
amount no greater than the party's money (`4C13:0357`), a word or a string typed into the dialog's
input field by `+13CF`. `+1572`, `+15AA`, `+172C` and `+192E` build and show the choice lists, and
`+1717`, `+174C`, `+1769` and `+1797` show a picture, text, a number and a line break. `+07D0` is a
thief skill check whose modifier the script picks from an 8-byte table. `+17E2` and `+1820` save
and restore the move records of slots 5 to `0x2F` and the leader around overlay 173's fight
routine. `+0E60` is Space's computer-control reset of FND-AI-004. `+04D8`, `+04E8`, `+1704` and
`+17C7` reach only the empty routine `2D40:06E1`, so the calls that instructions `0x45`, `0x46`
and `0x2C` make through them change nothing. The names stay `fn_188_<offset>` until a reading shows
what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+1745` and `+17C7` may be reached only through their trampolines
or a computed far call. Overlay 169 `+0174`, overlay 187 `+135B`, overlay 172 `+05DB`, `+1184`,
`2D40:0D94`, `2D40:192C`, `2D40:288C`, `2D40:2F34`, `2D40:3D4C`, `2D40:3E34`, `2C5F:018C`,
`1695:01D1`, `1B0B:00A4`, `31E0:4129`, `4228:00A9`, `1000:2E86` and `1000:40F6` are not read here;
the reading of them as a holder lookup, a cell test, an item move, a text-to-number conversion and
a line-of-sight test rests on their arguments and on the entries cited. In `+0F27` a found entry
with a receiver that is negative or whose state byte is neither 1 nor 2 leaves the count unchanged
and the loop goes round again, so such a call would not return unless `n` is 0 or less. The table
at `DS:193F` has no bound in `+07D0`; an index above 7 reads the bytes after it. The role of the
19-byte record's word `+0` as a slot that the record follows rests on `+1876` only.

## How to reproduce

Run `overlay_listing.py` for overlay 188 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`188+<offset>`), map the resident callers to
the routines that hold them and to the word table at `DS:030A`, read the strings and the bytes at
`DS:193F` to `DS:19AE`, and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
