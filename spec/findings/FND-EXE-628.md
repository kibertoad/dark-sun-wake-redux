---
id: FND-EXE-628
title: The 7 routines of overlay 196 that no entry placed are the FLI player's chunk dispatcher, file open, frame record reader and its wrapper, play entry, frame loop and timer slot release
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082F70..0x00083086
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00083092..0x0008318E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00083190..0x0008337B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000833B3..0x000833C2
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 7 rows of overlay 196 (header segment `5755`, 9
trampolines, code from file offset `0x82F70`, 1,106 bytes) that no spec location covered, 1,036
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x82F70` plus it, and `DS` is `57E0`. Every routine has a trampoline in `5755`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. FND-VIDEO-008 reads the routines as the game's FLI player and gives
their behaviour in full; its locations name only the trampolines, so this entry places the code.
The screen descriptor is the 20 bytes at `DS:24E4` (FND-VIDEO-008), the header buffer the 128
bytes at `DS:6299` and the file handle the word `DS:6319`.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0000` | `0x82F70..0x83086` | `5755:0020` | Takes a far screen descriptor, a far 16-byte frame record head, a far data pointer and a flag word. For each of the head's word `+6` chunks, switches on the chunk's word `+4` less `0x0B` through the 6-word table at `+0116` and passes the data after the chunk's 6-byte head on as FND-VIDEO-008 gives (palette with `57D7:0020` when the flag is set and `57D4:0020`; `57DD:0020`; a clear of 64,000 bytes through `1000:3FA2`; nothing; `57DA:0020` with the descriptor's word `+6`; a copy of 64,000 bytes through `1000:3F5A`), then steps the far pointer by the chunk's double word size and normalises it. Called from `+0231` (in `+017B`). |
| `+0122` | `0x83092..0x830EB` | `5755:0025` | Takes a far file name and a far 128-byte buffer. Opens the file through `44DE:0086` with mode 1 (-3 on failure), reads 128 bytes through `44DE:01AF`, and returns the handle when all arrived and the buffer's word `+4` is `0xAF11`; otherwise closes the file through `44DE:003A` and returns -5. Called from `+0288` (in `+0264`). |
| `+017B` | `0x830EB..0x8318E`, `0x83190..0x831BF` | `5755:002A` | Takes a handle, a far screen descriptor and a flag. Reads a 16-byte record head (-8 when short, -6 when its word `+4` is not `0xF1FA`); returns 0 when its double word size is 16. Otherwise allocates the size less 16 through `444C:00FA`, passing the low word only, zero-extended; when that fails it formats `Can't find %ld bytes` (`DS:24FE`) into a stack buffer through `1000:374E` and returns -2. Reads the size less 16 bytes into the allocation, freeing it through `444C:0092` and returning -8 when fewer arrive; then calls `+0000` with the descriptor, the head, the data and the flag, frees the data and returns 0. Called from `+025C` (in `+024F`). The `jmp` at `+021E` follows a `jmp` and nothing reaches it, so the row leaves its two bytes out. |
| `+024F` | `0x831BF..0x831D4` | `5755:002F` | Takes a handle and returns `+017B` of it with `DS:24E4` and 1. Called from `+0354` and `+03C3` (in `+02F9`). |
| `+0264` | `0x831D4..0x83269` | `5755:0043` | The play entry of FND-VIDEO-008: takes a far file name, a byte `song`, a byte `ticks` and a dword `delay`; calls `+0414`, `1BF3:2973` with `0x13` and `+0122` with the name and `DS:6299`, keeping the handle in `DS:6319` and returning 2 when it is negative; starts the song through `4A32:0011`; when `DS:1436` is set calls `2660:04F3` until the byte `4E71:0C4A` is above `song` or 20,000 calls have been made; calls `+02F9` with `ticks` and `delay`, closes the file through `44DE:003A`, calls `+0443` and overlay 187 `+2B6C` (`56EF:00F7`, which stops the music, FND-EXE-596), and returns `+02F9`'s result. Called from overlay 187 `+278D`, in the cinematic routine `+2546` (FND-VIDEO-004). |
| `+02F9` | `0x83269..0x8337B` | `5755:0048` | The frame loop of FND-VIDEO-008: takes `ticks` (1 when 0) and `delay`; waits for `delay` ticks of `DS:21E0` (giving up after 50,000 passes while the word stays 0), reads the first record through `+024F` (calling `1000:1C32` and returning 3 when that fails), then for each later record waits `ticks` less the time the previous read took, reads it (3 on failure), returns 4 when the count reaches the header's word `DS:629F`, and returns 1 when `44B6:0011` gives bit 0 or 1 and `DS:631B` is set or more than 240 records have been read. Called from `+02D7` (in `+0264`). |
| `+0443` | `0x833B3..0x833C2` | `5755:0039` | Passes the timer slot `DS:21E2` that `+0414` took to `4842:084A`, the release routine of the timer slots (FND-EXE-585). Called from `+02EC` (in `+0264`). |

## Interpretation

These rows are the body of the FLI player FND-VIDEO-008 describes: `+0264` sets up the screen,
the file, the music and a 1 ms timer slot, `+02F9` shows the records on time, `+024F` and
`+017B` read one record and `+0000` hands its chunks to the decoders, and `+0443` gives the timer
slot back. `+017B` allocates the record's size modulo 65,536 but reads the full size, so a frame
record of 65,552 bytes or more would overrun the allocation. The names stay `fn_196_<offset>`
until a reading shows what the decoders do.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. `4842:084A` is named a release routine by FND-EXE-585 without a
reading of its body here, and the decoders `57D4`, `57D7`, `57DA` and `57DD`, `444C:00FA`,
`1000:374E`, `2660:04F3` and `1000:1C32` are not read; the reading of them rests on FND-VIDEO-008
and on how their arguments are used. Whether any shipped FLI record reaches 65,552 bytes was not
checked.

## How to reproduce

Run `overlay_listing.py` for overlay 196 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`196+<offset>`), read the table at `+0116`, and
compare the ranges with the rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
