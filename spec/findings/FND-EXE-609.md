---
id: FND-EXE-609
title: The 7 routines of overlay 171 that no entry placed pass the ADD window's position to overlay 190 +3456, select the first stored character not in the party, draw and mark one list line, test a line against the party, show a line's name, close the window and reopen the member's last window, and delete a stored character's CACT entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058210..0x00058336
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058B48..0x00058CB5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059014..0x0005925A
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 7 rows of overlay 171 (header segment `5664`, 12
trampolines, code from file offset `0x58210`, 4,170 bytes) that no spec location covered, 1,241
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x58210` plus it, and `DS` is `57E0`. Every routine has a trampoline in `5664`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. Overlay 171 is the ADD window of the Create Characters screen
(FND-PARTY-046): `+0126` opens window `0x4845`, kept at `4C4C:0004`, and `+03C5` is its handler.
Its list of stored characters is the 51-byte entries under the far pointer `DS:40C8`, `DS:40C6` of
them, each with an identifier in word `+0`, a number in word `+2` and a name from `+4`
(FND-PARTY-012). `4C4C:0000` is the selected line and `4C4C:0002` the entry shown on line 0, so
line `i` shows entry `4C4C:0002 + i` in button `0x4780 + i`. A party member `m` from 0 to 3 has a
49-byte record at `DS:19C9 + 0x31 * m` whose word `+6` is its character identifier
(FMT-COMBAT-001).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0000` | `0x58210..0x58230` | `5664:0052` | Takes a far window pointer and calls overlay 190 `+3456` with `0x11`, the window's words `+0x96` and `+0x98`, and 0. Returns 0. No direct call was found; `+0126` passes `5664:0052` to `3A8E:0D3C` for the window at `4C4C:0004` (`+0203..+0214`). |
| `+0020` | `0x58230..0x58336` | `5664:0057` | Takes no argument. Calls `28C9:2ABD`. While `4C4C:0000` is below 40 and `4C4C:0000 + 4C4C:0002` is below `DS:40C6`, it tests that entry's identifier against word `+6` of the four members' records; at the first entry that matches none it calls `+0A46` with `4C4C:0000`, sets button `0x4780 + [4C4C:0000]` to state 4 through `3EBE:071F` and stops, and otherwise it adds 1 to `4C4C:0000`. When the walk ends without a find, it sets both words to 0 and walks once more; after a second walk without a find it stores `0xFFFF` in `4C4C:0000`. Ends with `28C9:2A81` and 0. Called from `+0126` at `+0200` and from `+03C5` at `+0559`, `+0671`, `+06C7` and `+083B`. |
| `+0938` | `0x58B48..0x58C17` | `5664:003E` | Takes a line `i`. Writes the name of the line's entry into button `0x4780 + i` through `3EBE:07FF`, sets the button to state 2, then 0, then 5 through `3EBE:071F`, and to state 1 when `+0A07` with `i` returns nonzero. Called from `+0126` at `+01F5` and from `+03C5` at `+0526`, `+054E`, `+0666`, `+06BC`, `+0707`, `+0830` and `+0B6D`. |
| `+0A07` | `0x58C17..0x58C56` | `5664:002A` | Takes a line `i` and returns 1 when the identifier of the line's entry equals word `+6` of any of the four members' records, and 0 otherwise. Called from `+03C5` at `+0430`, `+047E`, `+0588` and `+05D6`, and from `+0938` at `+09DC`. |
| `+0A46` | `0x58C56..0x58CB5` | `5664:0043` | Takes a line `i`. Sets button `0x4780 + i` to state 0 and writes the name of the line's entry into control `0x47E1` through `4228:002B`. Called from `+0020` at `+0080` and from `+03C5` at `+04BC`, `+0611` and `+0771`. |
| `+0E04` | `0x59014..0x5915D` | `5664:0039` | Takes no argument. Passes a null far pointer to `39D1:0430`, closes the window at `4C4C:0004` through overlay 182 `+0128` with 0 and clears that pointer, frees `DS:40C8` through `444C:0092` when it is set, keeping the result there, and calls overlay 190 `+319E` and `2C5F:0182`. It then reads the word `DS:4265 + 2 * [4E71:0B44]` and, passing the current member's 66-byte and 49-byte records and the member, calls overlay 211 `+02B5` for 3, overlay 202 `+0287` for 4 and overlay 212 `+061C` for any other value; for 2 it calls overlay 190 `+36D2` with the member and `0x270F` (FND-INPUT-012). Called from `+0126` at `+025F` and from `+03C5` at `+08D4`. |
| `+0F4D` | `0x5915D..0x5925A` | `5664:004D` | Takes no argument. Calls overlay 186 `+0572`. When the byte `DS:044B` is set it takes the identifier of entry `4C4C:0000`; otherwise it takes word `+6` of the 49-byte record that word `+2` of entry `4C4C:0000` numbers. Neither adds `4C4C:0002`. It reads `CACT` 1 to 39 through `38FF:04AB` and keeps the first whose word is nonzero and equal to the identifier; with none it returns. When `37ED:0064` with `DS:144A` returns 0 it removes that `CACT` through `37FC:05C5` and writes it again holding a 0 word through `37FC:00E8` (FND-PARTY-012). It ends with overlay 186 `+05AD`. Called from `+03C5` at `+0821`, after choice 1 of `DELETE THIS PERSON?` (`DS:0473`) for button `0x477F`. |

## Interpretation

`+0020` moves the selection to the first stored character who is not already in the party,
wrapping once to the top, and marks it; `+0938` draws one line and gives a character already
in the party state 1; `+0A46` shows the selected name in the name box. `+0E04` closes the ADD window and
reopens the window the member last had, by the word that overlay 211 `+073D` sets to 3 for the
spell window (FND-EXE-608). `+0F4D` deletes the selected stored character by clearing its `CACT`
entry; it reads the entry by `4C4C:0000` alone, so a deletion made with the list scrolled reads
the entry of the line's position counted from the top of the list. `+0000` is the window's
`3A8E:0D3C` callback in the form of overlay 175 `+0000` (FND-EXE-598). The names stay
`fn_171_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+0000` may be reached only through the callback pointer `+0126`
sets. Overlay 186 `+0572` and `+05AD`, overlay 190 `+319E` and `+3456`, `2C5F:0182` and
`37ED:0064` are not read here; the reading of `37ED:0064` as a check that comes before the write
rests on the same pattern in the save routines. What `DS:044B` selects, and whether word `+2` of
an entry is a slot in that mode, is not settled; the scroll position may be 0 whenever `+0F4D`
runs, which would make the missing `4C4C:0002` harmless.

## How to reproduce

Run `overlay_listing.py` for overlay 171 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`171+<offset>`), read `+07D7..+0840` for the
delete branch, and compare the ranges with the rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
