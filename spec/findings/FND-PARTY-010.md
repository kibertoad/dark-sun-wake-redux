---
id: FND-PARTY-010
title: Unpacked, CHARTRAN.EXE names charsave.gff and OBJEX.GFF and moves Dark Sun 1 characters into the party list
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 1896:6C4B..1896:6CE3
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 1896:6DC9..1896:6F48
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 1896:70D1..1896:71C1
tool: DarkSunWakeRedux.Inspect unlzexe, commit e455d51, then hex inspection with Python 3.14.7
environment: null
---

## Observation

`CHARTRAN.EXE` (24,761 bytes, XXH3-128 `f7466f2ac604dc7c353358bc80bd0131`) is packed with LZEXE
0.91. Unpacked, it is 70,288 bytes with XXH3-128 `a2804715759141397dca547934213843`, and its data
segment is `1896`. It holds, among other NUL-terminated strings:

| Address | String |
|---|---|
| `1896:6C6F` | `%s\OBJEX.GFF` |
| `1896:6C7C` | a message that `OBJEX.GFF` could not be opened in the Dark Sun 2 directory |
| `1896:6DC9` | a message that characters are being saved to the Wake of the Ravager party list |
| `1896:6EA5` | `%s\charsave.gff` |
| `1896:6EB5` | a message that `CHARSAVE.GFF` was not found |
| `1896:6ED6` | a message that `charsave.gff` could not be closed |
| `1896:6F24` | a message that there is no space to transfer a character, with `%Fs` for its name |
| `1896:70D1`, `1896:70E2` | prompts for the Dark Sun 1 path and the Dark Sun 2 path |
| `1896:71B4` | `%s\SAVE*.SAV` |

Its code pushes the tags `CHAR`, `CACT`, `PSIN`, `PSST` and `SPST` as immediate operands
(FND-PARTY-011).

The packed file holds none of these strings. A search of the packed file in Ghidra found `CHAR`
once, at `1000:58AD` in the packed image, with no reference, no `PSIN`, and no `CHARSAVE.GFF`.

## Interpretation

`CHARTRAN.EXE` is the character transfer utility: it reads a Dark Sun 1 saved game
(`SAVE*.SAV`) and writes its characters into the `CHARSAVE.GFF` of the Wake of the Ravager
directory, which the game calls its party list, using `OBJEX.GFF` for items. The packed file's
single `CHAR` is a coincidence of the compressed bytes; searches of the packed file say nothing
about what the program does.

## Alternatives

None known. The earlier conclusion that the utility has no character-archive path came from the
packed bytes and is wrong.

## How to reproduce

Unpack the file with `dotnet run --project tools/DarkSunWakeRedux.Inspect -- unlzexe
CHARTRAN.EXE <out>`, check the output's size and hash, and list the NUL-terminated strings of
its data segment, which starts at file offset `0x92F0` (load image at file offset `0x990`,
segment `1896` at linear `0x8960`).
