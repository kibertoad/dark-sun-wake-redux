---
id: FND-SAVE-006
title: DSUN.EXE copies DARKSAVE.GFF to DARKRUN.GFF and counts SAVE01.SAV to SAVE10.SAV against the free disk space
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:04B0..277B:0531
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:8520..5000:856C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:8920..5000:8B86
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:9E85..5000:9EAA
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

`DS` is the data segment `57E0`; `DS:x` is at `5000:x+0x7E00`. Overlay 180's resident header is
at `56B2:0000` and its 2,948 bytes of code start at file offset `0x671E0`; overlay 192 is as
FND-SAVE-004 gives it. The bytes at `DS:44F2` hold a string, empty in the file.

These NUL-terminated strings are in the data segment:

| Offset | String |
|---|---|
| `DS:0720` | `darksave.GFF` |
| `DS:072D` | `darkrun.GFF` |
| `DS:0739` | a message that copying `DARKSAVE.GFF` to `DARKRUN.GFF` failed, with `%Fs` for a path |
| `DS:0B20` | `DARKSAVE.GFF` |
| `DS:0B2D` | `DARKRUN.GFF` |
| `DS:0B39` | a message that copying one file to another failed, with three `%Fs` |
| `DS:0D5E` | `SAVE%.2d.SAV` |
| `DS:0D6B` | a message giving a maximum number of saved games, with `%ld` |
| `DS:2085` | `SAVE??.SAV` |
| `DS:2090` | `%Fs:%d` and a line break |
| `DS:2098` | `..\src\loadsave.c` |

The resident routine at `277B:04B0` copies the string at `DS:44F2` into two local buffers,
appends `darksave.GFF` to the first and `darkrun.GFF` to the second, at most 80 characters each,
and calls the far routine at `4544:0000` with the two paths and two zero double words. When that
returns 0 in `AL`, it prints the message at `DS:0739` with `DS:44F2`.

Overlay 180, from `DSUN.EXE+0x000674DA`, does the same with `DS:0B20` and `DS:0B2D` and, on
failure, formats `DS:0B39` with the two names and `DS:44F2`.

Overlay 180, from `DSUN.EXE+0x00067BDA`, loops a counter from 0 to 9. For each it formats
`DS:0D5E` with the counter plus 1, appends it to a copy of `DS:44F2`, opens the file, and when
the open succeeds adds its length to a total and closes it. It then divides the total by a word
of the drive information it read earlier through a routine whose failure shows a message
about reading the drive, adds another value from that information, and divides
by the size a saved game needs, computed from 1,330,000 (`0x144B50`) and the same information.
When the result is below 10, it shows `DS:0D6B` with the result.

Overlay 192 pushes `DS:2085` at `DSUN.EXE+0x0007D412`, and `DS:2098` and `DS:2090` together at
`DSUN.EXE+0x0007D581` and `DSUN.EXE+0x0007D585`.

## Interpretation

`DS:44F2` holds the game's directory. The game copies the shipped `DARKSAVE.GFF` to
`DARKRUN.GFF`, and plays in the copy. A saved game is a file `SAVE01.SAV` to `SAVE10.SAV` in the
game's directory: ten saved games at most, each assumed to need up to 1,330,000 bytes, and the
game warns when the disk cannot hold all ten. Overlay 192 lists the saved games by searching for
`SAVE??.SAV`, and the source file of the save and load code was `loadsave.c`, which its assertion
message names.

## Alternatives

FND-SAVE-008 reads the resident `4544:0000` open, transfer and close loop, confirming its
file-copy behavior. When each `DARKSAVE.GFF` to `DARKRUN.GFF` copy runs is not known. The drive
information's fields were not identified.

## How to reproduce

List the strings at file offsets `0x4D720`, `0x4DB20`, `0x4DD5E` and `0x4F085`, and disassemble
`277B:04B0` to `277B:0531` (file offset `0x1CE60`), overlay 180 from file offset `0x674DA` to
`0x67575` and from `0x67BDA` to `0x67C9E`, and the pushes in overlay 192.
