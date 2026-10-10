---
id: FND-PARTY-073
title: Character generation keeps class codes 1 to 8 of the DS:1164 numbering in its working details record at 4E4F:0029, and overlay 184 +1648 turns them into the 17 codes through the table at 4E4F:00C0 when it stores the character
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B163..0x0006B31D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006BA1E..0x0006BBBE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D868..0x0006D8E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006DCFB..0x0006DD15
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006DEC4..0x0006DEF7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E696..0x0006E7E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006EE5E..0x0006EEF5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004378C..0x000437D4
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, field_stores.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Segment `4E4F` (segment word `0x0360`, descriptor 108) starts at file `0x436F0`.

**The working records.** Overlay 184 `+07D8` (trampoline `56DD:003E`; called from overlay 184
`+1641` and overlay 190 `+0EF8`) stores its argument in the word at `4E71:0B44`, sets the far
pointer at `DS:1429` to `4E4F:0029` and the one at `DS:142D` to `4E4F:006B` (`+07E4..+07FB`).
When the word at `DS:112E` is -1 it then sets, among other fields, through `DS:1429` origin 1, gender 1, alignment
5, the dword at `+0x00` to 0 and the class bytes at `+0x1B`, `+0x1C` and `+0x1D` to 0, and the
word at `DS:42C2` to 0 (`+0810..+0850`); otherwise it leaves the record as it is.

**Choosing a class.** Overlay 183 `+0A3E` (trampoline `56CC:0061`) takes a code, a second word
and a flag. When the flag is nonzero it puts the code in the first of the bytes at `+0x1B`,
`+0x1C` and `+0x1D` of the record at `DS:1429` that is 0, unless an earlier one holds the code,
which it clears instead (`+0A44..+0AB5`). It then passes the three bytes to `+08DB` and branches
on the result through the table at `+0C91`; the branches move the class bytes down to fill a
cleared one, and call overlay 185 `+0020` with `DATA` 1001 (`push 0x3E9`), the origin byte less 1
and the first class byte less 1 (`+0B0B..+0B24`, `+0B7B..+0B99`). Its callers pass these codes
(each as `push flag`, `push word`, `push code`):

| Caller | Code | Flag |
| --- | --- | --- |
| overlay 183 `+01AE`, overlay 184 `+0C80` (when `DS:112E` is -1) and `+1067` | 3 | 1 |
| overlay 183 `+0278` | 1 | 1 |
| overlay 183 `+02F6` | 2 | 1 |
| overlay 183 `+0337` | 6 | 1 |
| overlay 183 `+02B7` | 7 | 1 |
| overlay 184 `+0E62`, branches at `+0E34`, `+0E3C`, `+0E44` and `+0E5C` | 3, 4, 5, 8 | 1 |
| overlay 184 `+0C80` (when `DS:112E` is not -1) | 0 | 0 |

The overlay 183 callers at `+0278`, `+02B7` and `+02F6` then test button `0x7D1` plus their
code (`0x7D2`, `0x7D8`, `0x7D3`) through `+0E5E` and store 0x80 or 0 in the word at `DS:42C2`;
the one at `+0337` tests button `0x7D7` and stores in the word at `DS:42C0` instead.

**Storing the character.** Overlay 184 `+1648` (trampoline `56DD:0020`; called from overlay 184
`+0184`, `+0642` and `+10CF` and overlay 183 `+05E3` and `+0646`) takes a slot. It sets a
variant from 0 to 3 to the place of the first of the bits 0x80, 0x40, 0x20 and 0x10 that is set
in `DS:42C2`, or to 0 when none is (`+1653..+1672`). It copies the 66 bytes at `4E4F:0029` to the
details record of the slot in the table at `DS:19C5` and the 49 bytes at `4E4F:006B` to its
combatant record in the table at `DS:19C9`, the slot being the word at `DS:112E` when that is
not -1 and its argument otherwise (`+1674..+170D`). Then for each of the three class bytes of
the record at `DS:1429` it stores the byte at `4E4F:00C0` plus 4 times that byte plus the
variant into the class byte of the details record numbered by `4E71:0B44` (`+1710..+1750`), and
calls overlay 210 `+0365` through trampoline `57B9:0039` with its argument (`+1753`). The table at `4E4F:00C0` (file `0x437B0`),
four bytes per code from 0:

| Code | Variants 0 to 3 |
| --- | --- |
| 0 | 0, 0, 0, 0 |
| 1 | 1, 2, 3, 4 |
| 2 | 5, 6, 7, 8 |
| 3 | 9, 9, 9, 9 |
| 4 | 10, 10, 10, 10 |
| 5 | 11, 11, 11, 11 |
| 6 | 12, 12, 12, 12 |
| 7 | 13, 14, 15, 16 |
| 8 | 17, 17, 17, 17 |

**Editing a stored character.** Overlay 184 `+1606` (trampoline `56DD:0048`, called from overlay
190 `+1004`) stores its slot argument in `DS:112E`, calls `+1DCE` with the slot's details and
combatant records, and then `+07D8` with the slot (`+160D..+1641`). `+1DCE` copies the two records
to `4E4F:0029` and `4E4F:006B`, sets `DS:42C2` to 0, and for each class byte of the details record
numbered by `4E71:0B44`, reads the two bytes at `4E4F:009C` plus 2 times the code: the first goes
to `DS:42C2` while `DS:42C2` is still 0, and the second to the class byte at `4E4F:0029` plus
`0x1B` plus the position (`+1DD7..+1E63`). The pairs from `4E4F:009C` are those of FND-PARTY-083.

`field_stores.py` with `--es` and the displacements `1B`, `1C` and `1D` finds no other byte store
to a class byte in overlays 183 and 184 than those above and the clearing at overlay 183
`+010D..+0117`.

## Interpretation

While a character is made, its class bytes hold the classes' positions in the 8-name table at
`DS:1164`: the buttons store codes 1 to 8, the default is 3 (Fighter), and overlay 183 indexes
`DATA` 1001 by the code less 1. Storing the character turns each into a code of the 17-code
numbering of `DS:1487` (FND-PARTY-057), with one variant from `DS:42C2` for all three classes, so
a finished character keeps the same numbering as the shipped records, and editing one turns the
codes back first. The variant chooses among the four codes of Cleric, Druid and Ranger; the bits
set for each code by the pairs at `4E4F:009C` are the inverse of the table at `4E4F:00C0`.

## Alternatives

- `+1DCE` reads the class bytes of the record numbered by `4E71:0B44`, which `+07D8` sets only
  after it in `+1606`, so it reads the slot of the last call of `+07D8` unless another writer has
  set the word to the slot. The other writers of the word (file `0x75CBC`, `0x78934` and
  `0x93170`) were not read, and whether the two can differ is open.
- Whether the variant bits stand for clerical spheres or anything else is not shown here.
- FND-PARTY-063 recorded the same readings but named the overlay 210 routine `+1648` calls as
  `+0039`, which is the offset of its trampoline in segment `57B9`; the routine is `+0365`.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 183 6B160 6B340`,
`183 6BA1E 6BC40`, `184 6D860 6D8E0`, `184 6DE37 6DF00`, `184 6DCF0 6DD18`, `184 6E696 6E7E2` and
`184 6EE5E 6EF40`; `trampoline_target.py <dsun> 57B9:0039`; `direct_callers.py <dsun> 184+1648 184+1606 184+07D8 184+1DCE 183+0A3E`; and
`field_stores.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es 1B 1C 1D`. Read the 72
bytes at file `0x4378C`.
