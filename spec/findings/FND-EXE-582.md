---
id: FND-EXE-582
title: The overlay manager's hook calls in DSUN.EXE go to manager routines or to an empty far routine, and declaring them reaches two more XMS calls and no interrupt site
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:01A8..4AE5:01B2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:03C6..4AE5:03CF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0547..4AE5:0550
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:05BF..4AE5:05D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:061F..4AE5:062F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:09B9..4AE5:09C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0BBA..4AE5:0BC6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0CBE..4AE5:0CC2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:1155..4AE5:1180
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:02FC..1000:02FD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004AF60..0x0004AF6A
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_manager_census.json, dsun_interrupt_census.py, store_values.py)
environment: null
---

## Observation

Addresses, regions and the walk are those of FND-EXE-580, whose walk leaves 69 computed transfers
unresolved. Six of them are calls in the overlay manager's segment `4AE5` through words of its data
segment `55CE` (DS loaded from `CS:0005`) or of an overlay's stub header (ES):

| Call | Pointer | Value at load | Writers | Targets |
|---|---|---|---|---|
| `4AE5:01A9`, after `push cs` | `[0084]` | `1257` | `4AE5:09B9`, `0EA2` | `4AE5:1257`, `4AE5:0EA2` |
| `4AE5:01AE`, after `push cs` | `[0082]` | `1257` | `4AE5:0BBA`, `1155` | `4AE5:1257`, `4AE5:1155` |
| `4AE5:062B`, when header `+0x18` is `04C6` | `[0080]` | `1256` | `4AE5:09BF` and `4AE5:0BC0`, both `0D11` | `4AE5:1256`, `4AE5:0D11` |
| `4AE5:03CB`, `4AE5:054C` | `[0086..008A]` | `1000:02FC` (the segment word carries an MZ relocation) | none | `1000:02FC` |
| `4AE5:05D1` | header `+0x18` | 0 in each of the 49 overlay headers | `4AE5:02E5`, `04C6`; `4AE5:0CBE`, the loader word of a cache record (`0A4B` or `0BFE`, FND-EXE-563) | `4AE5:04C6`, `4AE5:0A4B`, `4AE5:0BFE` |

The writers are every hit of a search of every byte offset of the regions for a direct store to
`[0080]` to `[008A]`, and, for header `+0x18`, of every byte offset of segment `4AE5` for a store
to displacement `0x18`. The other hits of the second search start inside those two stores (one
byte after their segment prefix), or are `mov [0018], bp` at `4AE5:0CF1`, a store to the data
segment. `4AE5:1256` is a `ret`, `4AE5:1257` and `1000:02FC` are a `retf`, and the stores of
`0EA2`, `1155` and `0D11` are in the EMS and extended-memory cache setups FND-EXE-563 reads.

**The walk with these targets.** The query `q007_dsun_manager_census.json` is FND-EXE-580's query
with exhaustive `indirectCalls` declarations for the six calls above and for two more calls through
`[0086]` that the walk then reaches, `4AE5:09DB` and `4AE5:0BDF`, also to `1000:02FC`. The walk
reaches 1,417 routines and decodes 107,162 instructions in 1,533 ranges. It leaves 65 transfers
unresolved: the 69 less the six declared, plus two calls through `[0043]` it now reaches in
`4AE5:1155`, `4AE5:1172` with AH 0D and `4AE5:117C` with AH 0A, after `cmp byte [0042], 0` and
`cmp word [0047], 0`. Every interrupt site keeps the reading of FND-EXE-580: 82 of the 100
interrupt 21 sites reached, 18 candidates, and all 4 interrupt 2F sites reached.

## Interpretation

The overlay manager's hooks go to manager routines in `4AE5` (the file loader, the two cache
loaders, the cache copy and setup routines) or to empty returns. None of them leaves the file. The
two calls they newly reach go to the XMS entry, as the other calls through `[0043]` do
(FND-EXE-581), with functions in the set that finding lists.

## Alternatives

- The store searches find direct stores only, so an indexed, string or block store into the data
  words or the headers is not excluded, and `[0086]` could be set that way.
- The direct-store search also finds stores to these offsets outside `4AE5`: `1000:01DD`,
  `1000:01E1`, `1000:2981`, `1000:2987` and `1000:2A03`, and four in `15F3` from `15F3:073F` to
  `15F3:0821`. The segment each of them goes through was not traced, so a write to `55CE:0080` from
  one of them is not excluded.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_manager_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
`store_values.py <dsun> 80 82 84 86 88` lists the direct stores. The header search decodes an
instruction at every byte offset of file `0x40050..0x41350` (segment `4AE5`) and keeps a store to
a memory operand with displacement `0x18`. The overlay headers' `+0x18` words are read from each
flags 3 descriptor's header (FMT-EXE-002).
