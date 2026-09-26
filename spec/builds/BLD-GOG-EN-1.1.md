---
id: BLD-GOG-EN-1.1
title: "Dark Sun: Wake of the Ravager 1.1, English, GOG release"
superseded_by: []
developer: Strategic Simulations, Inc.
publisher: Strategic Simulations, Inc.
publisher_version: "1.1"
distribution: GOG
languages: [en]
int_width: 16
manifest: BLD-GOG-EN-1.1.files.yaml
---

## Obtaining

Buy *Dungeons & Dragons: Dark Sun Series* from GOG and install *Dark Sun 2: Wake of the Ravager*
(GOG game ID `1432903719`, build ID `52095422060333615`, language English, all three from
`goggame-1432903719.info`). The installation's `README.TXT` (SRC-README-1.1) heads the game data as
Version 1.1, dated 12/14/94, and warns against saves made with versions 1.0 and 1.01.

The installation holds the disc as a raw image, `game.gog` (109,132,800 bytes, XXH3-128
`ece85c3c6ef6be3b304027d31b7fb489`), described by the cue sheet `game.ins`. Track 1 is the data
track, in 2,352-byte `MODE2/2352` sectors whose ISO 9660 volume is labelled `WAKE1_0`. The manifest
lists every file of that volume the game could read, under `CD:` and its path on the disc, hashed
over the file's own bytes.

GOG starts the game with its DOSBox 0.74-2 build and the configuration files
`dosbox_darksun2.conf` and `dosbox_darksun2_single.conf`. They emulate an S3 SVGA card
(`machine=svga_s3`) with 16 MB of memory, a Sound Blaster 16, and a fixed 15,000 cycles. The second
file mounts the install directory as `C:`, overlays the `cloud_saves` directory on it so that
writes land there, mounts `game.ins` as `D:`, and runs `RAVAGER.BAT` from `C:`, which runs
`DSUN -W0 -L`. The executable the game runs is therefore the installed `DSUN.EXE`.

Eight installed files differ from the files of the same name on the disc: `DSUN.EXE`,
`RESOURCE.GFF`, `GPLDATA.GFF`, `OBJEX.GFF`, `CHARSAVE.GFF`, `SOUND.INI`, `SOUND.BAT` and
`STDPATCH.AD`. `GPLDATA.GFF` and `OBJEX.GFF` keep the disc's sizes; the others change size. The
installation also ships `PATCH.EXE` and `PATCH.RTP`, and whether they turn the disc's files into the
installed ones is not known. Both copies of each file are in the manifest, and the spec cites the
copy it studied. `CHARSAVE.GFF` is 3,864 bytes on the disc and 11,735 bytes installed; which copy
`DSUN.EXE` reads when it runs from `C:` with the disc on `D:` is not known yet. All the other
files the disc and the installation share are byte-identical, whatever their directory: the
disc's `CINE/`, `SPEECH/` and `SOUND/` files match the installed `*.FLI`, `SPCH*.VOC` and
`SOUND*.VOC` files, and the region files match. The disc's `INTR/` voice files have no installed
copy.

The disc has no raw audio tracks in this build. `game.ins` gives tracks 2 to 41 as the files
`MUSIC/Track02.ogg` to `MUSIC/Track41.ogg`, lossy Ogg Vorbis re-encodings that DOSBox plays as
those tracks. The manifest lists them as `data` files by their install paths, hashed as shipped,
and a rule about music cites the Ogg file for the track it plays.

### DSUN.EXE

`DSUN.EXE` is a 16-bit real-mode MZ executable, 634,416 bytes long. It carries no packer
signature. Its header is 20,992 bytes (`0x5200`), and its MZ image ends at file offset
`0x57570`, so the resident load image is the file range `0x5200..0x57570` (FND-EXE-001). An
address in the resident image is written `segment:offset` with the load image at segment
`0x1000`, and sits at file offset `0x5200 + (segment - 0x1000) * 16 + offset`.

The rest of the file, `0x57570..0x9AE30`, is an `FBOV` overlay pack (FND-EXE-001, FMT-EXE-001)
whose segment table lies in the resident image. It holds the code of 49 overlays, each with a
resident header of trampolines, 854 trampolines in all (FND-EXE-003, FND-EXE-004, FMT-EXE-003).
The spec numbers an overlay by the index of its segment-table descriptor, 169 to 217. Overlay
code has no fixed load address, so the spec locates it by its file offset, padded to eight digits:
`DSUN.EXE+0x0006C01D`. `tools/ghidra/ReportFbovOverlayMap.ps1` in the rebuild's repository prints
each overlay's resident header segment and the file range of its code, which is enough to check
any overlay location.

### Other executables

`SOUND_DS.EXE`, `SVIEW.EXE`, `PATCH.EXE` and the disc's `DSUN.EXE` are MZ executables with no
packer signature, addressed the same way as the resident image of `DSUN.EXE`. `CHARTRAN.EXE` is
packed by LZEXE 0.91 (the signature `LZ91` at offset `0x1C`), so its manifest item gives the
unpacked file, and addresses in it are addresses in the unpacked file with its load image at
segment `0x1000`.

## Compared with other builds

No other build has been studied. Whether the disc image is byte-identical to a retail CD of
version 1.0 or 1.1 is not known, and neither is the history of the installed files that differ
from the disc's.

## Other files

The installation adds GOG's DOSBox in `DOSBOX/` with its configuration files
`dosbox_darksun2.conf` and `dosbox_darksun2_single.conf`, `Manual.pdf` and a byte-identical copy
named `ds_wakerave_manual_pdf.pdf` (SRC-MANUAL-1994), `Cluebook.pdf`, `README.TXT`
(SRC-README-1.1), icons, the GOG metadata files `goggame-1432903719.*`, `EULA.txt`,
`webcache.zip`, `goglog.ini` and the uninstaller `unins000.*`. DOSBox writes screenshots to
`capture/` and the game's writes land in `cloud_saves/`.

The disc also holds its installer (`INSTALL.EXE`, `INSTALL.NFO`, `BOOT.NFO`), which the manifest
leaves out, and the directories `MENZO/` and `PANZER/`, each with an installer and an ARJ
archive for other products, which it leaves out as well.
