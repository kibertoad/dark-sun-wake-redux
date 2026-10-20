---
id: FND-EXE-600
title: The 15 routines of overlay 173 that no entry placed score and count items, use up a counted item, reset combatant slot tables, and gather the slots in a cone, a strip, a rectangle or a circle into a list sorted by distance
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AC5D..0x0005AEBA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B17D..0x0005B278
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CE64..0x0005CE80
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D3BA..0x0005D520
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D73B..0x0005D83B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005DE0C..0x0005E3BF
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-597
environment: null
---

## Observation

After the restage of FND-EXE-597, `npm exec -- standard-coverage` reported 15 rows of overlay 173
(header segment `5671`, 45 trampolines, code from file offset `0x5A920`, 16,732 bytes) that no
spec location covered, 2,955 bytes in all. Each row below is one row of the inventory; "offset"
is the overlay code offset, the file offset is `0x5A920` plus it, and `DS` is `57E0`. Every
routine has a trampoline in `5671`. Callers are the hits of `direct_callers.py`, checked against
the instructions around them, named by the row that holds them. An item list is the 10-byte
entry list that `2D40:2196` fills: an entry's word `+2` is its kind, its word `+4` an index into
the 23-byte records under the far pointer `DS:19C1`, and entry 0's byte `+1` the count. A
23-byte record's word `+0x0A` indexes the 15-byte records under `DS:19CD`. A slot is one of the
320 object slots whose 3-byte entries are at `4F49:0C33` (FND-PARTY-131); its position is the
words `+0x0A` and `+0x0C` of its 37-byte record at `DS:67BB`, and with `m` the word after its
state byte, its 49-byte record is entry `m` under the far pointer `DS:19C9`.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+033D` | `0x5AC5D..0x5AD64` | `5671:002F` | Takes a combatant, an item list, its count, a current index and a flag. Scores the current entry and then each kind-1 entry with a 15-byte record whose byte `+0` has bit 0 or 1 set (with the flag set, only entries whose 23-byte record has `2`, `3` or `10` in byte `+0x13`) through `+0444`, and returns the index of the entry with the highest positive score that is below the current entry's score, or equal to it and after the current index; `0xFFFF` when there is none. Called twice from `+1238`. |
| `+0444` | `0x5AD64..0x5AEBA` | `5671:0034` | Takes a combatant, an item list and an index. Returns 0 when `2D40:3E64` returns 0 for the combatant or when `+2A9A` returns less than 1 for the entry. Otherwise, with `r` the entry's 15-byte record and `q` its 23-byte record, it sums `r[9] * r[8]`, twice the signed `r[0x0A]` and four times the signed `q[0x16]`, adds 10 when `q[0x16]` is above 0, `q`'s word `+0x10` is not 0 or bit 2 of `r[4]` is set, and returns the sum times `r[7]` when `r[6]` is above 1. Otherwise it returns the sum plus `27E5:02A7` of the combatant plus three times `27E5:042B` of the entry, times byte `+0x24` of the 66-byte record under `DS:19C5` that the second word `2D40:3E64` wrote names. Called twice from `+033D`. |
| `+085D` | `0x5B17D..0x5B20E` | `5671:003E` | Fills the 42 double words at `4C4F:00DC` and the 42 words at `4E71:0AC6` with `0xFF` bytes through `1000:3FA2`, and zeroes the 42 bytes at `4E71:0B1A`, the 42 words at `4C4F:01D8` and the 42 bytes at `4C4F:0184` and `4C4F:01AE`. Sets `DS:054E` to 1 and returns what overlay 188 `+18BD` returns for its argument. Called from `+059A` and `+1CC1`. |
| `+08EE` | `0x5B20E..0x5B278` | `5671:00F2` | Takes `n`. When the word at `4C10:0019` is not 0 (a fight is going on, FND-PARTY-089), sets entry `n` of `4C4F:00DC`, `4C4F:00DE` (4 bytes apart) and `4E71:0AC6` to `0xFFFF` and entry `n` of `4E71:0B1A`, `4C4F:0184` and `4C4F:01AE` to 0. Called from overlay 179 `+1373`, overlay 195 `+1000` and resident `31E0:0121`. |
| `+2544` | `0x5CE64..0x5CE80` | `5671:0061` | Returns the sum of its two arguments brought into 0 to 7 by adding or subtracting 8 once. Called from `+1CC1`. |
| `+2A9A` | `0x5D3BA..0x5D4A8` | `5671:0070` | Takes an item list, an item, its 15-byte record index and a flag. Sets `4C4F:0009` to 0 and returns 999 when the item or the record index is `0x270F` or more, or when bit 3 of the record's byte `+0` is clear. Otherwise it walks the list's kind-1 entries whose 23-byte record has 1 in byte `+0x13` until one has a nonzero word `+2`; for each it stores the record's byte `+0x16` at `4C4F:0009` and word `+0x10` at `4C4F:0007`, and with the flag set and the word above 0 takes the result of `+2B88` with 1 instead. Returns that word, or 0. Called from `+0444`, `+1CC1`, twice from `+2560` and from resident `28C9:254C`. |
| `+2B88` | `0x5D4A8..0x5D50D`, `0x5D50F..0x5D520` | `5671:0075` | Takes an item list, an item, its index and `n`. When bit 1 of byte `+0x0B` of the item's 15-byte record is set and the 23-byte record's word `+2` is not 0, subtracts `n` from that word and returns it while it stays above 0. When the bit is clear or the word reaches 0, removes the entry through `2D40:2452` and returns 0. The `jmp` at `0x5D50D` follows another `jmp` and nothing reaches it, so the row leaves its two bytes out. Called from `+2A9A` and overlay 191 `+0D60`. |
| `+2E1B` | `0x5D73B..0x5D7E7` | `5671:0084` | Takes a current index, an item list, a number and a near pointer to a count. Sets the count to 0 and returns `0xFFFF` for current index `0xFFFF`. Otherwise counts the kind-1 entries for which `+2DD0` returns `0x11` and whose 15-byte record has byte `+6` above 1 when the number is above 1, or equal to 1 when it is not, and returns the first such index after the current one, or `0xFFFF`. Called from `+1CC1` twice, overlay 183 `+19E9`, overlay 190 `+2A97`, overlay 191 `+0A94`, `+0CD0` and the routine at `+0B0A`, which has no row in this inventory, overlay 197 `+259E`, overlay 212 `+0B50` and resident `28C9:254C`. |
| `+2EC7` | `0x5D7E7..0x5D83B` | `5671:0089` | Takes a slot. When its state byte at `4F49:0C33 + 3 * slot` is 2 and, with `m` the word after that byte, entry `m` of `4C4F:00DC` is not `0xFFFF`, returns word `m` of `4C4F:01D8`; otherwise 0. Called from `+1098` twice, `+1238` and resident `28C9:1584`, `28C9:1EDB` (four calls), `28C9:305F`, `2D40:1397` (two calls) and `362C:06C4`. |
| `+34EC` | `0x5DE0C..0x5DE34` | `5671:00C0` | Takes a combatant. Returns `(L - 1) / 4 + 2`, at most 5, with `L` what `27E5:00C1` returns for the combatant and class code `0x11`. Called from `+2560`. |
| `+3514` | `0x5DE34..0x5DE8A` | `5671:00BB` | Takes `n` and a slot. When bit `0x200` of word `+0x10` of the 66-byte record that the slot's 49-byte record names by its word `+4` is clear and `n` is above 1, returns the negated result of `27E5:02D9` for the slot, at least 0; otherwise 0. Called from `+1CC1`. |
| `+356A` | `0x5DE8A..0x5DFA8` | `5671:00C5` | Takes a position, a range, two directions, a mask, a flag, a near list and a slot to skip. For each other slot in state 2 whose 49-byte record has in byte `+0x15` the number of a bit set in the mask, with distance (`2D40:382F`) at most the range and direction from the position (overlay 179 `+09CE`) less the first direction, modulo 256, at most the second less the first: with the flag set, it requires `1B0B:00A4` of the two positions divided by 16 to return 0, and then adds the slot and its distance to the list through `+02AA`, which keeps the 4-byte entries sorted by distance. Returns the count. Called from overlay 208 `+0000`. |
| `+3688` | `0x5DFA8..0x5E1BF` | `5671:00CA` | Takes two ends, a width, a flag for the ends, a flag for `1B0B:00A4`, a mask and a near list. Turns both ends and each candidate slot about the midpoint by minus the direction between the ends (overlay 179 `+09CE` and `+08B5`). A slot whose turned position lies between the ends and within half the width across is taken with the larger of its two offsets from the midpoint as distance; with the first flag set, a slot within half the width of either end is taken with that distance. The rest is as in `+356A`, tested and measured from the midpoint. Called from overlay 208 `+0000`. |
| `+389F` | `0x5E1BF..0x5E2EC` | `5671:00CF` | Takes two corners, a flag, a mask and a near list. Takes the slots inside the rectangle, with their distance from its centre, the `1B0B:00A4` test from the centre when the flag is set, and `+02AA` as above. Called from overlay 193 `+0AAD`. |
| `+39CC` | `0x5E2EC..0x5E3BF` | `5671:00D4` | Takes a position, a range, a mask, a flag and a near list. Takes the slots within the range, as above. Called from overlay 195 `+0789`, overlay 197 `+31AA` and overlay 208 `+0000` twice. |

## Interpretation

`+033D`, `+0444`, `+2A9A`, `+2B88` and `+2E1B` rank, count and use up items in a combatant's
list: `+2A9A` returns 999 for an item whose record lacks bit 3, and otherwise the count of the
first item at position 1 with one, which fits a weapon that needs ammunition and the ammunition
it finds. `+085D` and `+08EE` reset tables of 42 entries indexed by combatant at the start of a
fight and for one combatant during one. `+356A` to `+39CC` collect the slots in an area into a
list sorted by distance, which fits the target sets of area effects in overlay 208. The names stay
`fn_173_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine with no
recorded caller may still run. What `1B0B:00A4`, overlay 179 `+09CE` and `+08B5`, `27E5:02A7`,
`27E5:042B` and `27E5:02D9` compute is not read here; the shapes rest on how their results are
compared. The fields are named by offset only.

## How to reproduce

Run `overlay_listing.py` for overlay 173 against the installed `DSUN.EXE` and `direct_callers.py`
for each offset in the table (`173+<offset>`), and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
