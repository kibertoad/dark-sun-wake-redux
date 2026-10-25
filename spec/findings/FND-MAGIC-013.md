---
id: FND-MAGIC-013
title: A CHAR load copies the character's Strength to Charisma at 0x23..0x28 into combatant bytes +0x19..+0x1E, facing and unk_2a into +0x1F and +0x20; overlay 197 +1E7B rebuilds +0x19..+0x1E from the base scores in the same order, and 27E5:04A0(slot, k) returns byte +0x19 + k unbounded, so k = 4 reads Wisdom
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:04A0..27E5:04F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:0236..27E5:0283
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FE30..0x0006FE70
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007028E..0x00070328
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008527B..0x00085776
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D1D7..0x0006D1E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D541..0x0006D5D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080278..0x0008030F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00085776..0x000857AA
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, direct_callers.py, record_base_accesses.py, field_stores.py and block_copies.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7; a short read of the 26 words at file 0x85776
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x6FE30 + x` for overlay 187, `0x6D090 + x` for 184,
`0x7E2F0 + x` for 193 and `0x83400 + x` for 197. The combatant table is the 49-byte records under
the far pointer `DS:19C9` (FMT-COMBAT-001), the details table the 66-byte records under `DS:19C5`
(FMT-COMBAT-002), and a party slot's combatant number is the word at `4F49:0C34 + 3 * slot`.

**`27E5:04A0(slot, k)`** (`27E5:04A0..27E5:04F1`) returns 9 when the slot is 9999 or more, when the
state byte at `4F49:0C33 + 3 * slot` is not 2, or when the combatant number is 9999 or more
(`27E5:04A9..+04D6`, `27E5:04EC`). Otherwise it adds `k` to the address of the slot's combatant
record and returns the byte at `+0x19` of the sum, zero-extended (`27E5:04D8..+04E8`). Nothing
tests `k`: `k` 0 to 5 reach `+0x19..+0x1E`, 6 and 7 reach `+0x1F` and `+0x20`, and larger values
read into `name` at `+0x21`. `direct_callers.py <dsun> 27E5:04A0` finds far calls at overlay 176
`+06AF`, `+06BD`, `+06CB`, `+0830`, `+087B`, `+08EB`, overlay 177 `+0753`, overlay 197 `+139A`,
`+13D6`, overlay 204 `+0895`, overlay 209 `+0FBD` and overlay 211 `+1352`, and six near calls in
`27E5` at `+02B1`, `+02CA`, `+02E3`, `+02FC`, `+0315` and `+032E`, in short wrappers that push `k`
0, 0, 1, 1, 2 and 4.

**The writers.** `record_base_accesses.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --source
19C9 19 1A 1B 1C 1D 1E 1F 20` lists every inventoried access at those displacements whose base
comes from `DS:19C9`. The writes are:

| Site | Byte | In routine |
| --- | --- | --- |
| `28C9:0FBC`, `28C9:0FD3` | `+0x1A`, `+0x19` (25 each) | `28C9:0CFF` |
| `28C9:18AC`, `28C9:1D73` | `+0x1F` | `28C9:1584`, `28C9:19A1` |
| overlay 173 `+1FF4` | `+0x1F` | file `0x5C5E1` |
| overlay 177 `+04D4` | `+0x1F` | file `0x62592` |
| overlay 195 `+026E` | `+0x1B` (decrement) | file `0x81841` |
| overlay 197 `+22CE`, `+231D`, `+233D`, `+2364` | `+0x19` plus an index | `+1E7B` |
| overlay 204 `+26A2`, `+26D5` | `+0x1F`, `+0x20` | file `0x8E35D` |

Run without `--source`, the same query adds stores whose base is another pointer (`DS:1429`,
`DS:142D`, `DS:19C5`, a local or a stack copy). Of those at `+0x1D`, overlay 183's go through
`DS:1429`, overlay 184 `+0845` through `DS:1429`, and overlay 193 `+2015` and `+201A` through a
local far pointer that `+1F88` sets to a 0x49-byte copy on its own stack (`+1FAB..+1FC2`), so
none of them is a combatant record. `field_stores.py <dsun> <inventory> --es 19 1D` adds nothing
at `+0x1D` with the `DS:19C9` base.

`block_copies.py <dsun> <inventory> 24` marks 14 copy sites in routines that read `DS:19C9`. Only
overlay 187 `+0478` has a combatant record as its destination; the others copy into the details
table, stack locals or other tables.

**The load copy.** Overlay 187 `+0000` stores the `CHAR` resource's byte 2 as the slot's state
byte and passes its first far pointer argument on to `+039A` (`+0000..+0040`). FND-PARTY-049 reads
that pointer as the resource's address plus `0x0A`. For state 2, `+039A` copies `0x31` bytes from
it into combatant record `si` through `1000:3F5A` (`+045E..+047D`), then stores 9,999 in the
record's words `+0x04`, `+0x0A`, `+0x08` and `+0x0C`, makes `+0x14` 1 when it is 0, and stores
`si` as the slot's combatant number (`+0480..+04F5`). So combatant byte `+n` takes `CHAR` byte
`0x0A + n`: `+0x19` to `+0x1E` take `0x23` to `0x28`, `+0x1F` takes `0x29` and `+0x20` takes
`0x2A`.

**The rebuild.** Overlay 197 `+1E7B(slot)` (`+1E7B..+2376`) finds the slot's combatant and details
numbers through `2D40:3E64` and copies details bytes `+0x15 + i` for `i` 0 to 5 into two local
arrays of six words, a current and a raised array (`+1EC2..+1EF4`). It then:

- walks the effect entries at `4E71:024D` whose affected slot is the slot, and for codes 2, 0x0F,
  0x3C, 0x62 and 0x69 adds the entry's signed byte `+9` to current index 0 and holds it at 3 to 24;
  for 0x1F raises raised index 0 to at least 24 and for 0x63 to the greatest details byte `+0x15`
  of party slots 0 to 3, both setting a flag `R`; for 0x33 adds 4 to current index 1; and for 0x25 sets a flag `F`
  (`+1F22..+203B`, code words at `cs:2386`, handlers at `cs:2398`). When the routine behind trampoline
  `573B:0020`, called with the slot and 4, returns nonzero, a flag `P` is cleared and only entries
  whose source word `+4` is 0xEB or more count (`+1EAF..+1EBE`, `+1F22..+1F3E`).
- while `P` is set, walks the slot's item list from `2D40:2196`, and for an entry whose word `+2` is
  1 and for which the routine behind trampoline `5671:007F` returns 0x10, takes the item record's word `+0x10` less 0x140 as an index `c`. For `c`
  from 0 to 5 the value `v` at word `+0x0E` changes index `c`: below 0x10 it is added to current
  `c`, from 0x10 to 0x1F it raises raised `c` to `v` and sets `R`, and from 0x20 it
  takes `v - 0x1F` from current `c`. For `c` 7 it changes combatant byte `+0x13` (`+2056..+22AB`,
  handlers at `cs:2376`).
- for `i` from 0 to 5 stores into combatant byte `+0x19 + i`: 3 when `i` is 3 or 4 and `F` is set,
  else the larger of current and raised `i` when `R` is set, else current `i`; and then 25 when the
  stored byte is above 25 (`+22AE..+236F`).

`direct_callers.py <dsun> 197+1E7B` finds the trampoline `575A:0039` and far calls at overlay 179
`+1646`, `+27EE`, `+2804`, overlay 189 `+32AA`, `+3357`, overlay 193 `+18D6`, `+1A23` and overlay 195
`+0800`.

**Generation.** Overlay 184 stores six bytes at `+0x19 + si` of the record the far pointer
`DS:142D` names, for `si` from 0 to 5, from three score routines (`+0147..+0150`,
`+04B1..+0544`).

## Interpretation

Byte `+0x1D` of the combatant record is the character's Wisdom. The load fills `+0x19` to `+0x1E`
from the `CHAR` record's Strength, Dexterity, Constitution, Intelligence, Wisdom and Charisma at
`0x23` to `0x28` (FMT-PARTY-001), and every later rewrite of those bytes, overlay 197 `+1E7B`,
takes them from `base_scores` (details bytes `+0x15..+0x1A`, FND-PARTY-051) in the same order, so
index 4 stays Wisdom. `+0x1F` is the facing and `+0x20` the `CHAR` byte `unk_2a`.

`27E5:04A0(slot, k)` returns ability score `k` in the order Strength, Dexterity, Constitution,
Intelligence, Wisdom, Charisma, and 9 for a slot that holds no loaded character. The priest spell
count's second term (FND-MAGIC-002) is Wisdom, and overlay 176 `+06A2`'s arguments 2, 3 and 4
(FND-PARTY-165) are Constitution, Intelligence and Wisdom. A `k` of 6 or more reads the facing,
`unk_2a` or the name, so a psionic power whose score code is 6 or more would test one of those.

In play Wisdom is the base score changed by items of kind 0x144 and set to 3, with Intelligence,
by an effect of code 0x25; it is held at 25 at most. Strength is the only score effects change in
other ways, apart from code 0x33's 4 points of Dexterity.

## Alternatives

- That `CHAR` bytes `0x23..0x28` are the six scores in that order rests on FMT-PARTY-001, whose
  rows for them are `established`.
- That the copy's source is the resource address plus `0x0A` rests on FND-PARTY-049's reading of
  `2D40:000A`; the call at `2D40:00FB` passes the far pointer `[bp-4]`, whose store was not listed
  here.
- A store to `+0x1D` through a base the walk of `record_base_accesses.py` cannot follow (after a
  call or jump) or outside the inventory would be missed. The ones it printed as `undecided` were
  none at `+0x1D`; `field_stores.py` covers every byte of the image and overlays and found no other
  store at that displacement through `ES` that the source walk tied to `DS:19C9`.
- The `SAVE` resource 5 holds the whole combatant table (FND-SAVE-021) and puts back what was
  saved, so it does not change which field `+0x1D` is.
- What the three overlay 184 score routines compute and which record `DS:142D` names at
  generation were not read.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`, `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: `resident_listing.py <dsun>
27E5:0236..27E5:0350 27E5:0490..27E5:0500`; `overlay_listing.py <dsun> 187 6FE30 70440`, `197 8527B
85790`, `184 6D1C0 6D1E8`, `184 6D540 6D5D4` and `193 80278 80330`; `direct_callers.py <dsun>
197+1E7B 27E5:04A0`; `record_base_accesses.py <dsun> <inventory> --source 19C9 19 1A 1B 1C 1D 1E 1F
20` and the same without `--source`; `field_stores.py <dsun> <inventory> --es 19 1D`;
`block_copies.py <dsun> <inventory> 24`, keeping the sites marked `19C9`. Read the nine words at
file `0x85786`, the nine at `0x85798` and the eight at `0x85776`.
