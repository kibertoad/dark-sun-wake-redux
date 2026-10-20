---
id: FND-RNG-058
title: On a walk from every established entry, the clock's tables, label pointers and name are only read, its offset record is written only by the timezone initializer and its environment root only by the startup constructor; the classification bytes below the table proper hold the overlay pack size and the exit-routine count
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3BDC..1000:3BEE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3C53..1000:3C60
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3CC4..1000:3CCA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:030E..1000:0334
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0395..1000:03AA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:00D4..4AE5:00E7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2E98..1000:2EA1
tool: tools/research/exec-census/window_operands.py, reach_config.py, resident_listing.py and overlay_listing.py at the commit that adds this finding; scientific_method_engine 19.10.0 reach; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-RNG-016 and FND-RNG-018 read the clock
conversion `1000:386D`, its daylight helper `1000:3CD7` and the timezone initializer `1000:3B56`,
which use these data-segment windows: the classification bytes `DS:34F5..3675` the initializer
indexes with a sign-extended byte from `DS:3575`, the environment root `DS:3920`, the month tables
`DS:3950..3976`, the label pointers `DS:3976..397A`, the offset and enable record `DS:397A..3980`,
the name `DS:3980..3983` and the label buffers `DS:A4E4..A4EC` the pointers hold at load.

**The walk.** A `reach` query starts from FND-EXE-578's 21 established entries (the MZ entry point
`1000:0000`, the startup records' targets `4AE5:0D27`, `1000:0886`, `1000:11C1`, `1000:12D9`,
`1000:2877`, `1000:29AA` and `1000:3B56`, the exit record's `4AE5:0193`, and the installed
interrupt handlers `1000:01A7`, `1000:0338`, `1000:1D66`, `1000:2AF9`, `1000:2A07`, `1000:2A88`,
`4842:040C`, `4842:0519`, `1000:2B6A`, `44B6:00B5`, `4AE5:04F4` and `55E3:003F`), with FND-RNG-050's
112 computed-transfer declarations, one table jump and one call that does not return, no leaves,
the generator, the seed setter and the four reducers as targets, and `decodedRanges`. Its controls
are the first direct call of each start that has one (`1000:0025`, `4AE5:0D2F`, `1000:08C1`,
`1000:11C3`, `1000:12DC`, `1000:28EA`, `1000:29B3`, `1000:3B5E`, `4AE5:01A5`, `1000:2B3C`,
`1000:2ACB` and `1000:2BAD`), and for the other nine starts an instruction past their entry
(`1000:01AD`, `1000:0346`, `1000:1D8D`, `1000:2A24`, `4842:0423`, `4842:051A`, `44B6:00B8`,
`4AE5:04FD`), with the main routine `277B:0024`; all twelve calls reach their callees and all nine
instructions are decoded. The walk reaches 2130 routines and decodes 165,558 instructions in 2330
ranges, with no gap and no contested start. Nine computed transfers stay unresolved: the calls
through `DS:A4D0` (`1000:1D8D`) and `DS:A4DE` (`1000:2A24`), the jump through `DS:3916` at
`1000:298A`, the two table jumps of segment `2707` at `2707:0188` and `2707:058C`, the calls
through `CS:000C` at `44B6:00B8` and `CS:011A` at `4842:051A`, the timer callbacks through
`CS:[SI+6]` at `4842:0498`, whose declaration is not exhaustive, and the jump through `CS:002F` at
`55E3:003F` to the vector it replaced.

**The census.** `window_operands.py` decodes each range the walk reports and lists every memory
operand whose displacement, and every immediate other than a branch target or a relocated segment
word, lies in a window:

| Window | Writes | Reads and pointers |
|---|---|---|
| `3950..3976` month tables | none | `1000:38E5` in the conversion, `1000:3D05`, `3D13`, `3D4F` and `3D58` in the daylight helper |
| `3976..397A` label pointers | none | eight reads in the initializer, `1000:3BF2` to `1000:3CBC` |
| `397A..3980` record | the initializer only: `1000:3BDC..1000:3BEE` (default), `1000:3C53..1000:3C60` (parsed offset), `1000:3CC4..1000:3CCA` (daylight label) | `1000:387D`, `3880` and `3919` in the conversion |
| `3980..3983` name | none | the immediate `0x3980` at `1000:3B58` |
| `3920` root | `1000:29DA` in the constructor | `1000:2A00`, `1000:317A`; the immediates `0x3920` at overlay 190 `+15C0`, overlay 199 `+0D95` and overlay 201 `+0020` are compared with a key code, beside `0x011B` and `0x1C0D` |
| `A4E4..A4EC` labels | none | none |
| `3575..3675` classification table | none | `1000:09C6`, the initializer's ten reads, `455B:0042`, `0265` and `0272`, and `1000:2E98`, where the integer parser loads `0x3575` into `DI` and tests `[BX+DI]` with `BH` 0 (`1000:2E98..1000:2EA1`) |
| `34F5..3575` below the table | `1000:0330` and `1000:0395`, the count at `DS:3572` that the exit-routine registrar `1000:030B` raises (at most 32) and the exit worker `1000:0388` lowers (`1000:030E..1000:0334`, `1000:0395..1000:03AA`); `4AE5:00DC` and `4AE5:00E3`, the overlay pack's payload size at `DS:356C..3570`, with `ES` from `CS:0007` (`4AE5:00D4..4AE5:00E7`, FND-EXE-560) | the manager's reads of `356C..3572`; immediates that are the `AX` of interrupt 21 requests (`1000:01B1` to `1000:01D8`, `15F3:01CA`, `4AE5:0DE6`), a `CS` value table (overlay 197 `+3345`), and string pointers passed to the formatted print `1000:32E3` (`49DE:002F` to `49DE:0050`), the file loader `47B9:0236` (`4734:0072`) and as the source of the string append `4A68:0009` (`4734:04DA`) |

The `CS`-prefixed table jumps at `172C:3327`, `172C:3451` and overlay 197 `+337D` to `+3457` read
code-segment tables. Of the routines with a hit, only `1000:29AA`, `172C:3278`, `4AE5:08EB` and
`4AE5:0AB5` hold string stores. The second pass, decoding at every byte, finds 129 more hits in
these windows that are not instructions the walk reaches, among them the reads of the record and
the tables in the inverse conversion `1000:3986`, which has no caller (FND-RNG-057). The window
`384A..384E` in the same command is `rng_state`'s (Q-RNG-007).

## Interpretation

On every path the walk covers, the month tables, the label pointers and the name keep their
shipped values, and the offset and enable record changes only in the timezone initializer, in the
three ways FND-RNG-018 reads. The environment root has the one writer FND-RNG-054 found. The label
buffers have no direct operand at all: they are written through the pointer words, by the
initializer's copy and fill helpers and the startup clear (FND-RNG-019).

The classification bytes the initializer reads for an ASCII byte, and those the integer parser
reads, are the table proper, which nothing on the walk writes. A `TZ` byte from `0x80` to `0xFF`
makes the initializer read below the table, at `0x3475` plus the byte: bytes `0xF7` to `0xFA` give
the pack's payload size stored at startup, `0xFD` and `0xFE` the current number of registered exit
routines, and the rest strings and other words that only their readers touch on the walk.

## Alternatives

- The census covers displacements and literal pointers only. A write into a window through a
  pointer formed another way (a computed base outside the windows, an overrun of a neighbouring
  buffer, the string append's destination `DS:33F2` running past `DS:34F5`, a bulk copy with a
  register destination) is not excluded, and no string store or helper's destination was traced.
- A different segment and offset pair naming the same bytes would not show.
- The nine unresolved transfers, code reached only through them, and handlers the program reaches
  through saved vectors were not read.
- No emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`: copy FND-RNG-050's query `q-201_05D5.json` with the 21 starts, the controls and
instruction controls above, no leaves and `"decodedRanges": true`; `reach_config.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> <query>`; `python -I -m
scientific_method_engine reach <config> > <report>`; `window_operands.py <dsun> <report> 34F5..3675
3920..3922 3950..3983 A4E4..A4EC 384A..384E`; `resident_listing.py <dsun> 1000:01B0..1000:01C2
1000:030B..1000:0338 1000:0388..1000:03B0 1000:2E86..1000:2EB0 1000:3BDC..1000:3BF0
1000:3C50..1000:3C62 1000:3CC0..1000:3CCC 4AE5:00A0..4AE5:00F0 4734:04D9..4734:04F0
4734:0072..4734:0090 47B9:0236..47B9:02A0 4A68:0009..4A68:0040 1000:32E3..1000:32F0`;
`overlay_listing.py <dsun> 190 78340 7BE16`, `199 87900 88F11` and `201 897C0 8A368`. Queries,
reports and listings are kept under `GAME_DIR/analysis/rng-storage-20261020/`. Nothing was
executed.
