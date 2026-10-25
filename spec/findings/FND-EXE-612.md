---
id: FND-EXE-612
title: The 2 routines of overlay 210 that no entry placed multiply a slot's greatest hit points less its constitution bonus by its class count, and sum a thief value from dexterity, origin and thief level while reading the details record through DS
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000954A5..0x000954D2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009562D..0x000956A0
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 2 rows of overlay 210 (header segment `57B9`, 17
trampolines, code from file offset `0x94F60`, 3,642 bytes) that no spec location covered, 160
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x94F60` plus it, and `DS` is `57E0`. Every routine has a trampoline in `57B9`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. A slot `s` has a 66-byte details record at `DS:19C5 + 0x42 * s`, with
its greatest hit points in word `+0x08` (FMT-COMBAT-002), its origin in byte `+0x12`, 1 for human,
its three classes in bytes `+0x1B` to `+0x1D` in the 17-code numbering, where 17 is Thief
(FND-PARTY-057), and their levels in bytes `+0x1E` to `+0x20` (FMT-PARTY-001), and a 49-byte
record at `DS:19C9 + 0x31 * s` whose byte `+0x1A` is the current dexterity (FND-PARTY-161).
Overlay 210 `+0301` returns the constitution bonus and `+02BA` the number of classes, 1 for a
human (FND-PARTY-074, FND-PARTY-081).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0545` | `0x954A5..0x954D2` | `57B9:0048` | Takes `s`. Returns the low word of `(word +0x08 - [+0301 of s]) * [+02BA of s]`, signed. Called from overlay 184 `+183E`, which stores the result in the word `+0x0A` of `s`'s details record. |
| `+06CD` | `0x9562D..0x956A0` | `57B9:0052` | Takes `s`. Loads only the offset word of the far pointer `DS:19C5` and reads the details bytes at `DS` plus that offset plus `0x42 * s`. For each class position `i` from 0 to 2 whose class byte is 17, skipping a position above 0 of a human whose level there is not above the level at position 0, it sets the total to `5 * (dexterity - 16)`, or 0 when that is negative, adds 5 when the origin is not 1, and adds `8 * level + 10`. Returns the total, 0 when no position counts. No direct call was found. |

## Interpretation

`+0545` gives the word `+0x0A` that overlay 210 `+0233` multiplies by the level sum, divides by
the sum of the bytes `+0x39` and by the class count, and adds the constitution bonus to again
(FND-PARTY-081), so the stored word reads as the rolled hit points scaled back up by the class
count. `+06CD` sums a thief value from
dexterity above 16, a bonus for any origin but human and 8 per thief level, in the shape of the
thief skill of overlay 204 `+0887` (FND-PARTY-169) with other constants; for a human it counts a
thief class at a later position only when its level is above the first class's level, a test of
the kind FND-PARTY-165 describes for a held-back former class. Each counted position sets the total afresh, so a later position replaces an
earlier one. The details table is far: `DS:19C5` held `761B:0004` in the runs of
FND-PARTY-186, so `+06CD` reads data-segment bytes in place of the record whenever it runs.
The names stay `fn_210_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached; `+06CD` may be reached
only through its trampoline by a computed call, or be unused, in which case its reads through
`DS` have no effect in play. What value `+06CD` stands for is not settled; the thief reading rests
on class 17 and the level term.

## How to reproduce

Run `overlay_listing.py` for overlay 210 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`210+<offset>`), read overlay 184
`+183D..+1853` for the store of `+0545`'s result, and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
