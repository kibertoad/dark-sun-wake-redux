---
id: FND-PARTY-057
title: Overlay 190 names a class code through a caller's table of far pointers counted from 1, and overlay 212 passes the table at DS:1487, which names codes 1 to 4 Cleric, 5 to 8 Druid, 9 Fighter, 10 Gladiator, 11 Preserver, 12 Psionic, 13 to 16 Ranger and 17 Thief
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007B2B9..0x0007B489
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007B489..0x0007B4A9
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00098C37..0x00098C57
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C17A..0x0006C19C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004E487..0x0004E4CB
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004E526..0x0004E564
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, trampoline_target.py, direct_callers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, whose data segment `57E0` starts at file `0x4D000`.

**Overlay 190 `+2F79`** (trampoline `571F:0089`) takes a far pointer, a far pointer to a record,
two coordinates, a word and the near address of a table of far pointers. It returns without
printing when the record's byte at `+0x1B` is 0. Otherwise it prints the string whose far pointer
is at the table plus 4 times the byte less 1, and, as `+11BC` of overlay 183 does for the levels
(FND-PARTY-055), adds `/` and the strings for the bytes at `+0x1C` and `+0x1D` while those are
nonzero (formats at `DS:1F75`, `DS:2004` and `DS:1FED`). While the far value at `55CA:0000` is
nonzero, each string's colour is what `+311A` returns for its byte.

**Overlay 190 `+311A`** (trampoline `571F:008E`) reads its argument less 1 as an index into the
sixteen words at `+3149` when it is 15 or less, and otherwise returns the word at `DS:2D14`. The
words send codes 1 to 4, 5 to 8 and 13 to 16 to returns of 0x90, 0xD9, 0xB8 and 0x78 in that order
within each group, and codes 9 to 12 to the word at `DS:2D14`.

**The callers.** `direct_callers.py` finds `+2F79` called through `571F:0089` from overlay 183
`+11B2` and overlay 212 `+0A4D` only. Overlay 212 `+0A37..+0A56` passes the table at `DS:1487`;
overlay 183 `+119A..+11BB` passes the one at `DS:1164` (FND-PARTY-055).

**The table at `DS:1487`** holds 17 far pointers into the data segment:

| Code | String |
| --- | --- |
| 1 to 4 | `Cleric` |
| 5 to 8 | `Druid` |
| 9 | `Fighter` |
| 10 | `Gladiator` |
| 11 | `Preserver` |
| 12 | `Psionic` |
| 13 to 16 | `Ranger` |
| 17 | `Thief` |

The table at `DS:1164` holds the 8 names `CLERIC` to `THIEF`, followed by the score labels
(FND-PARTY-055).

## Interpretation

A class code counts from 1 in a list of 17 in which Cleric, Druid and Ranger each take four codes
and the other classes one. The four codes of one class differ only in the colour `+311A` gives
them, so they are likely the four variants of a class with an elemental sphere; which code is
which sphere is not shown here. Through the table at `DS:1487` the codes of the four members of
the supplied party (FND-PARTY-056) name Preserver/Psionic/Thief, Cleric, Fighter/Druid and
Gladiator, the class lines FND-PARTY-020 captured, with `Psionic` spelled as the screen showed it.
Through the table at `DS:1164`, codes above 8 would name score labels and alignments, so the
overlay 183 path suits codes 1 to 8 only.

## Alternatives

- The View Character screen names classes through overlay 183's table: ruled against for the
  supplied party, whose codes go up to 17 and whose captured class lines match the table at
  `DS:1487`. When overlay 183 `+119A` is reached was not traced.
- The colours stand for something other than spheres: FND-PARTY-020 records the Druid of the third
  member in red and the Cleric of the second in a darker yellow, which fits a colour per code but
  does not name the spheres.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `trampoline_target.py <dsun> 571F:0089` and
`571F:008E`, `overlay_listing.py <dsun> 190 0x7B2B9 0x7B4A9`, `212 0x98C37 0x98C57` and `183 0x6C17A
0x6C19C`, and `direct_callers.py <dsun> 190+2F79`. Read the 17 far pointers at file `0x4E487`, the
strings they name, and the 16 words at file `0x7B489`.
