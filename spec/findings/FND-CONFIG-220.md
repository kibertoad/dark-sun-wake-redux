---
id: FND-CONFIG-220
title: No uncompressed shipped program or batch file names PATCH.RTP, and PATCH.EXE keeps its program compressed after a 4,474-byte loader
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: PATCH.EXE
    offset: 0x00000000..0x0000137A
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: PATCH.EXE
    offset: 0x0000137A..0x0000B18A
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00000000..0x0009AE30
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    offset: 0x00000000..0x00031F31
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: SVIEW.EXE
    offset: 0x00000000..0x00015BE5
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: RAVAGER.BAT
    offset: 0x00000000..0x00000014
  - build: BLD-GOG-EN-1.1
    file: SOUND.BAT
    offset: 0x00000000..0x00000072
tool: tools/research/exec-census/name_census.py at the commit that adds this finding; Python 3.14.7, xxhash 4.0.1; the repository's Inspect unlzexe command for CHARTRAN.EXE
environment: null
---

## Observation

A case-blind search of the bytes of each file for `PATCH`, `.RTP` and `RTPATCH`, with a control
text known to occur in each:

| File | XXH3-128 | `PATCH` | `.RTP`, `RTPATCH` | Control found |
|---|---|---|---|---|
| `DSUN.EXE` | `e296af55ba2ecde7e77f555c90f33d0b` | once, in `stdpatch` at `0x503F5` | none | `DARKSAVE.GFF` and `RESOURCE.GFF` |
| `SOUND_DS.EXE` | `236c2dc23c071eca421eb5b427caee57` | once, in `stdpatch` at `0x1D1BC` | none | `SOUND.INI` |
| `SVIEW.EXE` | `2b5581f61c843f6e8f27c8487dd852a9` | none | none | `CONFIG.SYS` |
| `CHARTRAN.EXE`, unpacked (70,288 bytes, `a2804715759141397dca547934213843`, FND-PARTY-010) | | none | none | `CHARSAVE.GFF`, `DSUN` |
| `PATCH.EXE` | `16f9279ce9fe0ee1e3e5f7cc8b012d48` | once, in `PATCH.EXE` at `0x20D` | none | `.EXE` |
| `RAVAGER.BAT` | `f300902d1d5cd90db675a2454a10f9b3` | none | none | `DSUN` |
| `SOUND.BAT` | `7f4161a0e9226e5fb6228ee55b0b0c0f` | none | none | `SOUND_DS` |

`RAVAGER.BAT` runs `DSUN -W0 -L` and `exit`; `SOUND.BAT` copies `D:\*.ADV`, runs `SOUND_DS` and
deletes the copies. Neither names `PATCH`.

`PATCH.EXE` is 45,450 bytes. Its MZ header gives 10 pages with 378 bytes in the last, so its
load image ends at file `0x137A`, and a header of 32 paragraphs, so the load image is the 4,474
bytes from `0x200`, the size of the one region of the committed `PATCH.EXE` inventory. The
40,464 bytes after it, to `0xB18A`, are outside the load image. The load image holds the text
`PATCH.EXE` and the messages `Fatal Error`, `DOS function xxxx returned an error code of xxxx`,
`Internal error number xxxx in .EXE decompression code` and `Cannot find compressed file in the
current directory or along the PATH environment variable`, with advice to run the program from
its own directory. The bytes after the load image hold nine runs of 8 to 11 printable
characters, none of them a word or a file name.

## Interpretation

No program the installed game runs, and neither batch file, names `PATCH.RTP`; `stdpatch` is
the sound driver file `STDPATCH.AD`. `PATCH.EXE` is a loader that finds its own file and
decompresses the program kept after its load image; the code that reads `PATCH.RTP`, if any,
is in that compressed part, which no committed tool unpacks. FND-EXE-570 gives the only reading
of `PATCH.RTP` so far, from where the patched files' sizes fall.

## Alternatives

- A program could build the name `PATCH.RTP` at run time, or take it from its command line;
  a byte search does not show that. Only the compressed part of `PATCH.EXE` is not covered by
  the search at all.
- That the part after the load image is compressed code rests on the loader's messages and the
  absence of words in it; the loader's decompression routine was not read.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`:
`name_census.py --text PATCH --text .RTP --text RTPATCH --text DARKSAVE.GFF --text SOUND.INI
--text RESOURCE.GFF` and a second run with `--text .GFF --text DSUN --text SOUND_DS --text .EXE
--text CONFIG.SYS`, each over the installed `DSUN.EXE`, `SOUND_DS.EXE`, `SVIEW.EXE`, `PATCH.EXE`,
`RAVAGER.BAT`, `SOUND.BAT` and a copy of `CHARTRAN.EXE` unpacked with the Inspect project's
`unlzexe` command into a folder outside the repository. Read the MZ header words at `PATCH.EXE`
offsets 2, 4 and 8. The outputs are retained under `GAME_DIR/analysis/config-patch-rtp-20261018/`.
No original program was run.
