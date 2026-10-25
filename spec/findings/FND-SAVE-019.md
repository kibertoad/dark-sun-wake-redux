---
id: FND-SAVE-019
title: The exit after the main loop runs two atexit routines, one exit procedure, the runtime's vector restore and DOS function 4Ch; none of them writes a saved game, DARKRUN.GFF or CHARSAVE.GFF
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388..1000:03EE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0163..1000:01F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0140..4AE5:01F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067300..0x00067350
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067942..0x00067A40
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, direct_callers.py and immediate_search.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000; overlay 180 offsets `+x` are at file
`0x671E0 + x`. Startup calls `1000:03DF` when the main loop returns (FND-SAVE-012), which calls
`1000:0388(status, 0, 0)`. With both flags 0 that routine:

1. while the word `DS:3572` is not 0, decrements it and calls the far pointer at
   `DS:A446 + 4 × DS:3572`, the table `1000:030B` fills;
2. calls `1000:0163`, which runs the exit-procedure records from `DS:39BE` to `DS:39C4`: one far
   record, `4AE5:0193`, which restores an interrupt vector its library saved, closes a DOS handle
   it holds and calls two near pointers of its own;
3. calls the far pointer `DS:3676`;
4. calls `1000:01F3`, which restores the vectors of interrupts 0, 4, 5 and 6 saved at startup;
5. calls `1000:0176`, which sums the first 47 bytes at `DS:0000` and writes the runtime's
   null-pointer message to handle 2 when the sum is not the stored one;
6. calls the far pointers `DS:367A` and `DS:367E`;
7. calls `1000:019E`, which issues `INT 21h` with `AH` = `0x4C` and `AL` the status.

In the file `DS:3676`, `DS:367A` and `DS:367E` point at `1000:0387`, a return; resident
`136E:0004` sets `DS:3676` when streams are used. `direct_callers.py` finds two calls of
`1000:030B`: resident file `0xBAD0`, registering `167B:0029`, and overlay 180 `+014F`, registering
`180 +0762`. `180 +015D` passes `180 +0848`, which shows a Ctrl-Break message, to `1000:0361`.

`180 +0762` calls `4A6E:0187` when `DS:13F7` is not 0, `200 +05C5`, `25AF:0036`, `188 +03BA`,
`1000:0481`, `1000:0873`, `1BF3:4FEB(0)`, `188 +0491` and `167B:0029`; closes the handles at
`DS:13FE` and `DS:13FC` through `2D40:3BD1` and stores -1 in the first; calls `39D1:0326` when
`4DE7:0001` is not 0 and `1BF3:2973(3)` when `4E71:0001` is not -1; calls `180 +0722` or
`4842:071A(0)` by `DS:13F7`; calls `38FF:02B5(-1)` and `44DE:0623(2, 4DE7:0338)`; and stores 0 in
`DS:1462`.

## Interpretation

Leaving the game shuts down sound, video, the timer and the archives and returns to DOS. None of
the steps writes a resource or copies a file, so play since the last save is lost on exit.

## Alternatives

The callees of `180 +0762` and `4AE5:0193` were read at their heads; that they shut subsystems
down rests on their arguments and first instructions. A write reached only through those callees'
own computed calls was not excluded.

## How to reproduce

`resident_listing.py <dsun> 1000:0100..1000:0400`, `... 167B:0029..167B:0070
136E:0000..136E:0010 4AE5:0140..4AE5:01F0`; `overlay_listing.py <dsun> 180 67300 67350` and `...
180 67942 67A40`; `direct_callers.py <dsun> 1000:030B 1000:0361`; `immediate_search.py <dsun> 3676
367A 367E 3572`.
