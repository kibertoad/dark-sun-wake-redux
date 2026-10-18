---
id: FND-PARTY-245
title: Attached-record word 0x06, kind-1 field 3, is an item's value, read unsigned as the money a pickup gives and as the store's base price, with 9999 an item a store will not buy
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00060C34..0x00060EFB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00076FC6..0x000770D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007C6BD..0x0007C7C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00090343..0x0009039F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00090A89..0x00090B76
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00090E0C..0x00090E9A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00090F02..0x00090F9C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009A286..0x0009A392
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00063D1B..0x00063D51
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E78B..0x0005E838
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E252..0x0008E2EC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E2F1..0x0008E35D
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E500..0x0008E595
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E5A1..0x0008E5C5
    kind: file-data
tool: tools/research/exec-census/field_reads.py, field_stores.py, overlay_listing.py, resident_listing.py, direct_callers.py and script_decode.py at commit 6ed283c7; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`). A kind-1 record is the
23-byte record at the far pointer `DS:19C1` plus 23 times its number (FND-PARTY-051); FND-PARTY-216
binds kind-1 field 3 to its word at `0x06` with type 3. The 15-byte records at the far pointer
`DS:19CD` are numbered by the kind-1 word at `0x0A`; this finding uses only bit 1 of their byte
`0x0B`, called the stack bit below.

### Census

`field_reads.py` for displacement 6, keeping hits with `DS:19C1`, `DS:19C3`, `DS:6153` or
`DS:6155` among the six instructions before, gives 27 inventory hits and 55 outside the
inventory or inside other instructions. `field_stores.py` for displacement 6 gives every store
through a base register. Each hit was read in the listing of its routine:

- **Kind-1 reads** (all through `les bx, [19C1]` plus 23 times a record number, then `es:[bx+6]`):
  overlay 175 `+1234`, `+127F`, `+12BE`, `+12E9`; overlay 189 `+263A`, `+2685`, `+26C4`,
  `+26EF`; overlay 191 `+04F1`, `+053C`, `+057B`, `+05A6`; overlay 207 `+01EC`, `+0927`,
  `+0CAA`, `+0D9C`, `+0DCD`; overlay 213 `+1032`, `+1048`, `+1090`, `+110D`, `+1120`; overlay
  178 `+0C98`; overlay 204 `+24E4`.
- **Kind-1 stores**: overlay 173 `+3EF1` and overlay 204 `+2795`.
- **Other tables**: overlay 173 `+142C`, `+1A37` and `+2E82` read byte 6 of a 15-byte `DS:19CD`
  record, whose address the instructions just before form; the `[bp+6]` hits are stack
  arguments. The other ES stores at displacement 6 are formed from other pointers: the 37-byte
  and 8-byte tables at `DS:67BB` and `DS:67B7`, the ten-byte graph entries of `2D40:2196`, the
  49-byte combatant table `DS:19C9`, or a local or argument far pointer. Three local pointers
  were not followed to their producer: `31E0:0DF5`, overlay 182 `+0960` and overlay 200 `+0163`,
  all of whose routines test a flag byte at `+5` and a negated object number at `+6` of the same
  pointer, the layout of the 8-byte `DS:67B7` entries.

Every widening read of the word zero-extends it (`movzx`), and the one comparison with an order
(overlay 178 `+0C98`) is unsigned.

### Readers

**Money pickups.** Overlay 175 `+0FCB` (file 60BAB), overlay 189 `+23A2` (file 76D52) and
overlay 191 `+01F7` (file 7C3E7) select the kind-1 record numbered by the word at
`5072:0044 + 10 * DS:1A34` and take its object number, the word `0x00` negated when negative.
Only for an object number from 900 to 910 (the two tests `0x389..0x38D` and `0x384..0x38E`) do
they continue (175 `+10B2..+11E6`). When the stack bit of the record's 15-byte entry is set, they
multiply the word `0x06` by the quantity word `0x02` with a 16-bit `imul`, keep only the low word,
zero-extend it and add it to the double word `4C13:0357`; otherwise they add the word `0x06`
zero-extended. They then print the same 16-bit amount with `YOU GET %u` (`DS:05F3` in 175,
`DS:1B0B` in 189) or `YOU FIND %u` (`DS:2018` in 191), call `2D40:2410` with `DS:1A32`, and set
`DS:1A34` to `0xFFFF`.

**Store, overlay 207.**

- `+0875` (file 909F5): when something is on the pointer (`DS:1A34` not `0xFFFF`) it prints
  `NO DEAL!`. Otherwise it reads the selected store record's word `0x06` zero-extended, passes it
  to `+1325` and keeps the returned double word as the price; it multiplies that by the low byte
  of word `0x02` when the stack bit is set and by 1 otherwise, and compares the product, signed,
  with `4C13:0357`. A greater product prints `NOT ENOUGH MONEY`; otherwise it subtracts the
  product from `4C13:0357`, calls `+110C`, prints `SOLD!` and calls `+0A9D` with the slot.
- `+0C54` (file 90DD4), for the slots 30 to 35: the word `0x06`, times the low byte of `0x02`
  when the stack bit is set (16-bit `imul`), zero-extended through `+1325`, printed with `%u`.
- `+0D74` (file 90EF4), the sale of the item on the pointer: a word `0x06` of 9999 prints
  `I COULDN'T SELL THAT` and ends; otherwise the word zero-extended goes through `+1438`, whose
  double word result is printed with `I'll give you %lu`.
- `+0000` (file 90180), for a store slot: it compares the word `0x06` zero-extended, not
  multiplied and not passed through `+1325`, signed with `4C13:0357`, and passes pointer image 8
  when it is not greater and 7 when it is, to overlay 190 `+0000`.

`+1325` and `+1438` were not read.

**Item details, overlay 213 `+0BFB`** (file 99E5B), given a kind-1 record number: a word `0x06`
of 0 or of 9999 skips the value line. Otherwise, when the quantity word is not 0, the word in `SI` (the 15-byte
entry number it then uses) is below 9999 and that entry's stack bit is set, it prints the 32-bit product of the word
`0x06` and the quantity, both zero-extended, with `%lu`; else, unless the word is 1, it prints
the word with `%u` (`DS:2F23`, `DS:2F28`). The routine's inventory row ends at file 9A297; the
later reads lie on the fall-through of the jump at `+1037`.

**Overlay 178 `+0C6B`** (file 63D1B), given a kind-1 record number, returns 8 when the word
`0x06` is above 1 unsigned (or when other tests on `0x10`, `0x02` and the 15-byte entry pass),
and 0x80 when the word `0x14` is below 9999.

**Script field getter, overlay 204 `+2327`** (FND-PARTY-156, function 48 of opcode `0x22`). For
a slot of kind 1 (`+2492`) it matches the field number against the six double words at
`cs:2531` (file 8E2F1): fields 0, 1, 3, 7, 8 and 10 read the record's signed word `0x00`, and
the words `0x02`, `0x06`, `0x0E` and `0x10` zero-extended, and its byte `0x13` sign-extended. Any
other field leaves the result 9999. For kind 2 the nine keys at `cs:2567` are 0, 13, 14, 18, 19,
23, 25, 32 and 33; for kind 5 the three at `cs:2555` are 0, 25 and 33.

### Writers

**Script field setter, overlay 204 `+259D`** (function 49). For kind 1 (`+2740`) the six keys
at `cs:27E1` (file 8E5A1) are the same fields 0, 1, 3, 7, 8 and 10; field 3 stores the low word
of the value at `+2795`, and fields 1, 7 and 8 store words `0x02`, `0x0E` and `0x10`, field 10
the byte `0x13`.

**Overlay 173 `+3E6B`** (file 5E78B), given a record number, fills it: `0x00` -13999, `0x02` 0,
`0x06` 0, `0x0A` 21, `0x0C` 0, `0x0E` 0, `0x10` 0, `0x12` 1, `0x14` 65 and `0x16` 0.

**Scripts.** In the decode of FND-SCRIPT-024 (`script_decode.py --all`) the field-3 requests
are: function 48 with field 3 twice in `GPL/30` (`0x04AD`, `0x0539`) and function 49 with field
3 in `GPL/30` `0x05F8` and `GPL/149` `0x0310` and `0x07E2`; the path `(8028; 3)` assigned in
`GPL/149` `0x0308` and `GPL/184` `0x173C`, and the path `(802B; 3)` read in `GPL/54` `0x1E33`,
`0x1E3F`, `0x1EDF` and `0x1EEB`. `GPL/184` `0x173C` stores a draw of instruction `0x52`
(FND-SCRIPT-011) with 10, times 100, after creating object -905. `GPL/149` tests a handle's field
0 against -900 before it writes field 3. `GPL/54` adds field 3 of each object its loop visits,
when below 9999, into `var7[26]` and `var7[18]`. `GPL/30` moves fields 7 and 3 of `var9[44]` into
fields 25 and 33 across a function-9 change of the object to -81 or -82, and fields 25 and 33
into 7 and 3 across a change to -6081.

FND-PARTY-196's shipped word `0x06` values: 30000 (13), 9999 (12), 1 (9), 20 (8), from -30536 to
30000 read signed; -30536 is 35000 read unsigned. Object -900 is the most common word `0x00` (12).

## Interpretation

Word `0x06` of a kind-1 record is the item's value. For the money objects 900 to 910 it is the
amount a pickup adds to the party's money, multiplied by the quantity for an item whose 15-byte
entry has the stack bit; the party's money is the double word `4C13:0357`. In a store it is the
base of the price the party pays (through `+1325`) and of what the shopkeeper offers (through
`+1438`), and 9999 marks an item the shopkeeper will not buy. The item details window shows it,
and hides it for 0 and 9999. Every consumer that widens it treats it as unsigned, so the shipped
-30536 is a value of 35000.

The store's pointer test compares the unmultiplied, unadjusted value with the money, while the
purchase compares the adjusted price times the quantity; the two can disagree. The pickups keep
only the low 16 bits of value times quantity, both in the money added and in the message.

## Alternatives

- The generic accessor `1AA0:0009` sign-extends a type-3 word (FND-PARTY-216). The shipped script
  paths `(8028; 3)` and `(802B; 3)` reach it, so a script that reads field 3 through a path sees
  values above 32767 as negative, while function 48 sees them unsigned; whether a shipped path
  reads such a value was not checked.
- Which kind the handles `var9[44]`, `var9[40]`, `var9[43]` and the path keys `0x8028` and
  `0x802B` hold when these scripts run was not read; function 48 and 49 use field 3 as `0x06`
  only for a kind-1 slot, and return or store nothing for kinds 2 and 5.
- `+1325` and `+1438`, the price and offer adjustments, `2D40:2410` and the callers of 189
  `+23A2`, for which `direct_callers.py` finds no direct call, were not read. The money objects'
  names were not read from their resources.
- The census misses an access through a far pointer loaded more than six instructions before,
  through a pointer kept in memory, and block copies; the CHAR load copies whole records
  (FND-PARTY-051) and the numbered save and restore whole tables (FND-PARTY-236).

## How to reproduce

With the locked evidence Python and `<dsun>` the installed `DSUN.EXE`, in
`tools/research/exec-census/`: `field_reads.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
--table 19c1 --table 19c3 --table 6153 --table 6155 6` and `field_stores.py <dsun> <inventory>
6`; `overlay_listing.py <dsun>` for 175 `60BAB 60F10`, 189 `76D52 770E0`, 191 `7C3E7 7C7CB`,
207 `90180 90420`, `909F5 90C20` and `90DD4 90FA0`, 213 `9A260 9A3C0`, 178 `63D1B 63DF0`, 173
`5E78B 5E860`, 204 `8E0E7 8E2F1` and `8E35D 8E600`; `resident_listing.py <dsun>
31E0:0D09..31E0:0E00`; `direct_callers.py <dsun> 207+0000 207+0875 207+0C54 207+0D74 175+0FCB
189+23A2 191+01F7 213+0BFB 178+0C6B 173+3E6B 204+2327 204+259D`. Read the key tables as
little-endian double words followed by words: six at file 8E2F1, three at 8E315, nine at 8E327
and six at 8E5A1. The script lines are from `script_decode.py GPLDATA.GFF OBJEX.GFF --all`
(FND-SCRIPT-024). Outputs are retained under
`GAME_DIR/analysis/party-record-fields-20261018/`. No original function or game process was run.
