---
id: FND-PARTY-246
title: Attached-record word 0x0A numbers the item's 15-byte type entry, word 0x14 its name line, byte 0x16 a signed bonus printed after the name, and an item power's use decrements byte 0x0E
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF52..0x0001FF62
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00020321..0x00020329
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A7CD..0x0007A7FF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A936..0x0007A967
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009A1EF..0x0009A265
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CF4A..0x0005CF61
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E78B..0x0005E838
tool: tools/research/exec-census/record_base_accesses.py (added with this finding), field_reads.py and overlay_listing.py; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`). A kind-1 record is the
23-byte record at the far pointer `DS:19C1` plus 23 times its number (FND-PARTY-051).

### Field descriptors

The kind-1 row of the shipped `FNFO` 1 resource (the signed words from `OBJEX.GFF` offset
`0x1FF48`, FND-PARTY-216) and the `FNFO` 2 type bytes (from `0x2031C`) give for fields 5 to 12:

| Field | Record offset | Type |
|---|---|---|
| 5 | `0x0A` | 13 |
| 6 | `0x0C` | 2 |
| 7 | `0x0E` | 3 |
| 8 | `0x10` | 3 |
| 9 | `0x12` | 1 |
| 10 | `0x13` | 0 |
| 11 | `0x14` | 3 |
| 12 | `0x16` | 0 |

Fields 13 to 15 hold -1. No field names offset `0x15` alone; it is the high byte of field 11's
word. Field 6 matches FND-PARTY-230 and field 10 FND-PARTY-221.

### Census

`record_base_accesses.py` decodes every inventory range and lists each ES access at a given
displacement with the pointer word its base register was last loaded from. For displacement 6 it
lists every inventory kind-1 hit of FND-PARTY-245 (the positive control). For displacements
`0x0A`, `0x0E`, `0x10`, `0x14`, `0x15` and `0x16` with the pointer `DS:19C1`, `DS:19C3`,
`DS:6153` or `DS:6155`, it gives 148 accesses, all through `DS:19C1` and, all but one, with the
multiplier 23 just before the load (the exception, `28C9:28D5`, multiplies outside the six
instructions before it): 62 at `0x0A`, 13 at `0x0E`, 38 at `0x10`, 11 at `0x14`, none at
`0x15` and 24 at `0x16`. The stores among them are overlay 173 `+3E6B`'s initializer (FND-PARTY-245:
`0x0A` 21, `0x0E` 0, `0x10` 0, `0x14` 65, `0x16` 0), overlay 182 `+0DA1` (clears bit 3 of
`0x10`), 182 `+0EF3` and `+0F19` (`0x10`), overlay 190 `+2602` (decrements the byte `0x0E`), 190
`+36AC` and overlay 197 `+3792` and `+38B3` (`0x0E`).

### Word 0x0A

Of the 62 reads, 29 multiply the word by 15 and add it to the far pointer `DS:19CD` within the
next four instructions, 7 compare it with 9999 first, two compare it with 21 (overlay 173
`+2100`, `+2228`), and the rest keep it in a register or pass it to a callee. Overlay 190 `+248D`
(file 7A7CD) is one such read: when the word is below 9999 it tests bit 1 of byte `0x0B` of the
15-byte record `DS:19CD + 15 * word`. FND-PARTY-092 and FND-PARTY-221 read further bytes of the
same 15-byte records.

### Byte 0x0E

Overlay 190 `+2503` ends an action that `+2448` recorded (FND-PARTY-243): with the kind byte
`DS:6275` equal to 3, `DS:6273` holds an item's record number and `DS:6276` the member. When its
argument is not 0 and the kind is 3, then unless the item's 15-byte record has bit 1 of byte
`0x0B` set and the argument is 2, it decrements the byte at `0x0E` of the record (`+2602`),
tests the whole word at `0x0E`, and when it is 0 calls overlay 191 `+0B8C` with the record number
and the member (`+25F6..+2627`).

### Word 0x14 and byte 0x16

Overlay 213 `+0F8F..+1005` (file 9A1EF) formats an item's name: when the byte `0x16` is not 0 it
pushes it sign-extended, then the far pointer `DS:56F3 + 4 * word 0x14`, and formats `%Fs%+d`
(`DS:2F1C`); otherwise it formats the pointer alone with `%Fs` (`DS:2F0F`). FND-TEXT-008 shows
`DS:56F3` is the 256-entry table of far pointers to the lines of `TEXT/1000`. The other reads of
`0x14` compare it with 9999 (overlay 178 `+0D1F`, 213 `+0DD2`) or index the same table (overlay
173 `+3B64`, 190 `+2CEF`). The byte `0x16` is read signed in combat code: overlay 173 `+2639`
adds it to a local total when overlay 193 `+1C95` returned 0 (`+262A..+2641`, file 5CF4A), and 173 `+295A` and overlay 189 `+32DC` branch
on it being negative.

FND-PARTY-196's shipped values: `0x0A` from 2 to 97, `0x0E` from 0 to 70 (0 in 123 of 151
records), `0x14` from 3 to 247 with `0x15` always 0, and `0x16` from 0 to 4.

## Interpretation

Under the shipped descriptors the attached record's fields are: `0x0A` (field 5) the number of
the item's type entry, a 15-byte record of the table at `DS:19CD`, with 9999 for none; `0x14`
(field 11, a word whose high byte `0x15` is 0 in every shipped record) the number of the item's
name, a line of `TEXT/1000`; and `0x16` (field 12) a signed bonus that the name shows as `+n`
or `-n` and that combat code adds or tests for a negative value. Byte `0x0E` (field 7) counts
down by one each time the item's power is used, and the item is passed to overlay 191 `+0B8C`
when the count reaches 0, so for such items it is the number of uses left.

## Alternatives

- The byte decrement and the word test at `0x0E` differ in width: a count whose low byte reaches
  0 while its high byte is not 0 is not passed on. The shipped values fit in a byte.
- What overlay 191 `+0B8C` does with a used-up item, what the other writers of `0x0E` (190
  `+36AC`, 197 `+3792`, `+38B3`) store, and what overlay 190 `+2448` does with byte `0x0E` (for
  items with bit 1 of their type entry's byte `0x0B`) or byte `0x02` (for others) when it fills
  `DS:445A`, were not read.
- What the word `0x10` (field 8) holds was not settled: its readers test bits 0, 1, 3 and 7 or
  compare it with 0, and overlay 182 clears bit 3 and stores whole words.
- The census lists only accesses inside the inventory and only bases loaded straight from the
  pointer word; a record reached through a far pointer kept in a local or passed as an argument
  is not attributed to the table.

## How to reproduce

With the locked evidence Python and `<dsun>` the installed `DSUN.EXE`, in
`tools/research/exec-census/` at the commit that adds this finding:
`record_base_accesses.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --source 19c1 --source
19c3 --source 6153 --source 6155 a e 10 14 15 16`, and the control `record_base_accesses.py
<dsun> <inventory> 6`; `overlay_listing.py <dsun>` for 190 `7A788 7A9A0`, 213 `9A1D5 9A270` and
173 `5CF40 5CF70`. Read the kind-1 `FNFO` words at `OBJEX.GFF` `0x1FF48 + 2 * field` and type
bytes at `0x2031C + field` for fields 0 to 15 (FND-PARTY-216's sources). Outputs are retained
under `GAME_DIR/analysis/party-record-fields-20261018/`. No original function or game process was
run.
