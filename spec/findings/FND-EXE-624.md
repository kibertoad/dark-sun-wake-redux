---
id: FND-EXE-624
title: The 7 routines of overlay 177 that no entry placed find or make a slot's row of prepared spells, take a free row or evict the oldest, release one slot's row or all rows, sort a spell number into a wizard or priest range, and use up one prepared spell
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062100..0x00062159
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006237F..0x000624A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062B1E..0x00062B45
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062D36..0x00062DB3
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 7 rows of overlay 177 (header segment `5699`, 21
trampolines, code from file offset `0x62100`, 3,854 bytes) that no spec location covered, 546
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x62100` plus it, and `DS` is `57E0`. Every routine has a trampoline in `5699`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. A row is one of the 33-byte rows at `4D62:0000`, at most 28
(`0x1C`), of which the byte `DS:0654` counts those ever taken: word `+0` holds the slot the row
belongs to, or -1 for a free row; the 15 words from `+2` count the prepared spells at the 15
positions of the slot's spell list, which `+0059` fills (FND-PARTY-151); and byte `+0x20` holds
the value the byte `DS:0655` had when the row was taken. `DS:0655` is also the first byte of the
level bounds that FND-PARTY-110 reads at `DS:0655 + L`.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0000` | `0x62100..0x62159` | `5699:0066` | Takes a slot `s` and returns its row from `+0372`. When `s` has none it takes a row from `+027F`, stores `s` in its word `+0` and `DS:0655` in its byte `+0x20`, increments `DS:0655`, calls `+0059` with `s` to prepare its spells, and returns that row. Called from `+0C36` at `+0C7B` and from `+0C10`, `+0D06` and `+0E3A`. |
| `+027F` | `0x6237F..0x6240E` | `5699:0070` | Returns a row to use. It returns the first of rows 0 to `DS:0654 - 1` whose word `+0` is -1. Otherwise, when `DS:0654` is below 28, it returns `DS:0654` and increments it. Otherwise it looks over the 28 rows for the one whose byte `+0x20` plus `0xFF` less `DS:0655`, modulo 256, is below a running least that starts at 0, stores -1 in word `+0` of the row it chose and returns it. No value is below 0, so the search never chooses, and the row it frees and returns is whatever the stack byte at `[bp-2]` held. Called from `+0000` at `+001C`. |
| `+030E` | `0x6240E..0x6243B` | `5699:0075` | Takes a slot. When `+0372` finds its row it stores -1 in the row's word `+0` and returns 1; otherwise it returns 0. Called from `+033B` at `+0362`. |
| `+033B` | `0x6243B..0x62472` | `5699:0043` | Calls `+030E` with the word `+0` of each of rows 0 to `DS:0654 - 1` that is not -1, which frees every row; `DS:0654` keeps its value. Called from two resident far calls, at `28C9:21CC` and `28C9:31B2`, each followed by a call of `2C5F:097E`. |
| `+0372` | `0x62472..0x624A4` | `5699:007A` | Takes a slot and returns the first of rows 0 to `DS:0654 - 1` whose word `+0` is that slot, or `0xFF`. Called from `+0000` at `+000D`, `+0059` at `+0067` and `+030E` at `+0316`. |
| `+0A1E` | `0x62B1E..0x62B45` | `5699:0057` | Takes a spell number `n` and returns 1 for `n` from 0 to 114 (`0x72`), 2 for 115 to 234 (`0xEA`) and 0 otherwise. Called from overlay 197 `+3019`, `+303A`, `+307B`, `+30A2` and `+30C3`. |
| `+0C36` | `0x62D36..0x62DB3` | `5699:004D` | Takes a slot `s` and a spell number `n`. Takes the word at `4F49:0C34 + 3 * s` as a combatant, word `+4` of its 49-byte record at the far pointer `DS:19C9` as a details record, and byte `+0x38` of that 66-byte record at `DS:19C5` (`combatant_details`) as the spell list (FND-PARTY-151), and asks `+03A4` for the position of `n` in that list; it returns when that is -1. Otherwise it gets `s`'s row from `+0000` and decrements the row's count at that position when it is not 0. Called from overlay 174 `+0D37`, in the enemy spell choice (FND-PARTY-121). |

## Interpretation

The rows hold the prepared spells of the combatants that cast from a spell list of their own
(FND-PARTY-151): a row is made and filled the first time a slot is asked for (`+0000`), and
`+0C36` takes one spell off its count when the slot casts it. When all 28 rows are in use,
`+027F` is meant to reuse the row taken longest ago, but its search starts from 0, so it frees an
arbitrary row given by stale stack contents, which may be out of range. `+033B` clears every row,
from two resident paths. `+0A1E` sorts a spell number into the ranges of FND-PARTY-110: 1 for the
wizard spells below 115, 2 for the priest spells from 115 to 234, and 0 for the psionic powers
and other numbers. Since `+0000` increments `DS:0655`, each new row also raises the first level
bound of FND-PARTY-110, 0 in the file, by one. The names stay `fn_177_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. `+0059`, `+03A4`, `+0C10`, `+0D06`, `+0E3A`, `2C5F:097E` and the
resident routines around `28C9:21CC` and `28C9:31B2` are not read here beyond what FND-PARTY-151
gives; `+03A4` is read only as far as returning the index, 0 to 14, of `n` in the list at the far
pointer `529C:0014 + 4 * k`, or -1 for `k` 13 or more or `n` not found. Whether more than 28 slots
ever ask for a row in play, which is when the eviction fault would show, is not settled here. The
reading of `+0A1E`'s results as wizard and priest rests on FND-PARTY-110's ranges.

## How to reproduce

Run `overlay_listing.py` for overlay 177 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`177+<offset>`), and compare the ranges with the
rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. Run `resident_listing.py` over
`28C9:21C7..28C9:21D1` and `28C9:31AC..28C9:31BC` for the resident calls of `+033B`.
