---
id: FND-EXE-001
title: DSUN.EXE ends in an FBOV overlay pack that follows its MZ load image
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 55E8:0000..55E8:0728
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 55D9:0000..55D9:0728
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The MZ header of the installed `DSUN.EXE` (634,416 bytes) gives a header of 1,312 paragraphs,
20,992 bytes (`0x5200`), and a file of 699 pages with 368 bytes in the last, so the MZ image ends
at file offset `0x57570` (357,744). The resident load image is the file range
`0x5200..0x57570`, 336,752 bytes.

At `0x57570` the file holds the four ASCII bytes `FBOV`, then three little-endian 32-bit values:
276,656, 307,328 (`0x4B080`) and 229. `0x57570 + 16 + 276656` is `0x9AE30`, the end of the file,
so the 16 bytes at `0x57570` and the 276,656 bytes after them fill the file range
`0x57570..0x9AE30` exactly. `0x4B080` lies inside the load image, at `55E8:0000`, and 229 records
of 8 bytes from there end at `55E8:0728` (FND-EXE-002).

The 16-byte header at `0x57570` lies outside the load image and has no `segment:offset` address,
so this finding is located at the table the header points to.

The disc's `DSUN.EXE` (634,704 bytes) has the same shape: a 20,992-byte header, an MZ image ending
at `0x57430`, `FBOV` there with the values 277,264, 307,088 (`0x4AF90`, which is `55D9:0000`) and
229, and a pack that ends at the end of the file, `0x9AF50`.

## Interpretation

`DSUN.EXE` is a real-mode MZ program with a pack of overlay code appended after its load image.
The pack header gives the pack's length, where the segment table sits in the resident image, and
how many descriptors it has. The format is FMT-EXE-001.

## Alternatives

The marker and the layout match Borland's overlay format for real-mode programs, known from
public descriptions of its linker's output. That is a lead to the names of the fields, and nothing
here shows how the game's own loader reads them (FND-EXE-007). The 229 is read as an unsigned
count; a value this small cannot show whether the loader treats it as signed.

## How to reproduce

Read the 16-bit words at offsets `0x02` (bytes in the last page), `0x04` (pages) and `0x08`
(header paragraphs) of the file, and compute the image end as `(pages - 1) * 512 + last`. Read 16
bytes at the image end. `tools/ghidra/ReportFbovOverlayMap.ps1 -SourcePath <DSUN.EXE>` prints
`HeaderBytes`, `FbovFileOffset` and `FbovEndFileOffset` from the same fields. For the disc's copy,
read `DSUN.EXE` from the ISO 9660 volume in track 1 of `game.gog`.
