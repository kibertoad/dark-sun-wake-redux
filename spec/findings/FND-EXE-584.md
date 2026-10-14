---
id: FND-EXE-584
title: DSUN.EXE's nine calls through the record table at DGROUP 3F42 go to routines in segments 1425 and 15F3, which reach one more interrupt 21 site (AH 3F) and nine more XMS calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0176..1425:01B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0291..1425:02D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:03CB..1425:040E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:04B1..1425:04F4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0618..15F3:083E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050F42..0x00050FCE
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_tables_census.json, dsun_interrupt_census.py); Capstone 5.0.9 for the store search
environment: null
---

## Observation

Addresses, regions and the walk are those of FND-EXE-583, whose walk leaves 59 computed transfers
unresolved. Nine of them are far calls in segment `1425` through a table of 14-byte records at
DGROUP (`57E0`) `3F42`, indexed by a record number times 14: `[bx+3F42]` (field `+0`) at
`1425:0733`, `0C4A` and `1AA1`; `[bx+3F46]` (field `+4`) at `1425:05EC`, `0BFA`, `0DC9` and
`1C58`, and `[3F46]` (record 0) at `1425:1651`; `[bx+3F4A]` (field `+8`) at `1425:16AB`.

The table is 0 at load, through the ten records from `3F42` to `3FCE` that were read. A search of
every byte offset of the regions for a store to displacement `3F42` to `3F4D`, with or without a
base register, finds 24 stores, all immediate and all in four routines of `1425`. Each routine
writes one record's three far pointers at the index it computes, and a word at `+0x0C`:

| Stores | Field `+0` | Field `+4` | Field `+8` |
|---|---|---|---|
| `1425:0183..01B9` | `1425:00DB` | `1425:00AF` | `15F3:0360` |
| `1425:029E..02D4` | `1425:0009` | `1425:0044` | `1425:007F` |
| `1425:03D8..040E` | `1425:02E1` | `1425:02FE` | `1425:031B` |
| `1425:04BE..04F4` | `1425:041A` | `1425:0437` | `1425:0454` |

Each segment word (`0425`, `05F3`) carries an MZ relocation, and each of the twelve targets starts
with `push bp`, `mov bp, sp`.

**The walk with these targets.** The query `q007_dsun_tables_census.json` is FND-EXE-583's query
with an exhaustive `indirectCalls` declaration for each of the nine calls, naming the four values
of its field. The walk reaches 1,450 routines and decodes 108,389 instructions in 1,566 ranges. It
newly reaches the interrupt 21 site `15F3:0183`, AH 3F (`mov ax, 3F00` at `15F3:0180`), a
candidate until now, so 83 of the 100 interrupt 21 sites are reached and 17 remain candidates. It
leaves 59 transfers unresolved: the 59 less the nine declared, plus nine far calls through
`[00B8]` in `15F3`, in routines that load DS from CS (`15F3:0618`, `06B6` and `076B`): `15F3:0629`
with AH 0A (through `CS:[00B8]`), and `15F3:0701`, `0727`, `0754`, `07C2`, `07D7`, `07FF`, `0817` and
`083A` with AH 0B.

## Interpretation

The nine table calls go to routines in segments `1425` and `15F3`, which stay inside the file. The
code they reach adds one more reached request, a read (AH 3F), and nine more calls to the XMS
driver entry that `15F3:00DA` stores (FND-EXE-581), with free and move functions only.

## Alternatives

- The store search finds instructions with an immediate displacement in that range. A block copy
  into the table or a store through another base address is not excluded.
- Whether each call checks that its record was registered was not read. A call through a record
  no routine wrote would go to `0000:0000`.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_tables_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
The store search decodes an instruction at every byte offset of every region and keeps a store
whose memory operand has a displacement from `0x3F42` to `0x3F4D`.
