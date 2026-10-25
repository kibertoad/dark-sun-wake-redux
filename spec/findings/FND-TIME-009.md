---
id: FND-TIME-009
title: DSUN.EXE writes the timer control port only with 0x36, channel 0 in mode 3, and with the channel 0 latch command; of the ADV drivers only PCSPKR and the DIGPAK image write that port, and the installed SOUND.CFG selects neither
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:12C3..1000:12C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:05A6..4842:05A8
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x75754..0x77846
tool: tools/research/exec-census/port_out_census.py and gff_byte_search.py at the commit that adds this finding; a byte search of DSUN.EXE with Python 3.14.7
environment: null
---

## Observation

`port_out_census.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` lists two `OUT` instructions to
port `0x43` in the inventoried code: `1000:12C3` with `AL` 0, in the counter read of FND-TIME-004,
and `4842:05A6` with `AL` `0x36`, in the reload routine of FND-TIME-007. A search of the whole file
for the bytes `E6 43` (`out 0x43, al`) finds only those two, at file offsets `0x64C3` and
`0x3DBC6`, and finds no `BA 43 00` (`mov dx, 0x43`).

`gff_byte_search.py RESOURCE.GFF ADV e643 ba4300` finds `E6 43` in two of the 17 `ADV` resources:
resource 5 at `+0x57E`, after `mov al, 0xB6`, and resource 10 at `+0x4EC`, after `mov al, 0x90`,
and at `+0x560`, after `mov al, 0x36`. It finds no `BA 43 00`. FND-EXE-590 gives resource 5 as
`PCSPKR.ADV` and the installed `SOUND.CFG` as selecting resources 11 and 8.

## Interpretation

`0x36` sets channel 0 to mode 3 with a 16-bit reload, and 0 latches channel 0's count without
changing its mode. `0xB6` sets channel 2 to mode 3, `0x90` channel 2 to mode 0. So channel 0 runs in
mode 3 whenever the game or a driver sets it, and with the installed drivers no driver writes the
port at all. `calibrate_wait` (RULE-TIME-001) can rely on the counter running in mode 3.

## Alternatives

An `OUT DX` whose `DX` is computed, or code outside the inventory and the `ADV` images, is not
covered by these searches; `port_out_census.py` reports OUT opcode bytes in overlay code outside the
inventory, which were not examined.

## How to reproduce

`port_out_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`;
`gff_byte_search.py <RESOURCE.GFF> ADV e643 ba4300`; a byte search of `DSUN.EXE` for `E6 43` and
`BA 43 00`.
