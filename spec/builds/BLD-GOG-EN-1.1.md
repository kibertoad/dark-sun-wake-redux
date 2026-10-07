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
writes land there, mounts `game.ins` as `D:`, and offers game, sound setup and exit branches. The
game branch names bare `ravager`, the setup branch bare `sound` (FND-EXE-010).
These match the installed batch-file stems, but interpreter search and batch
continuation remain Q-EXE-009. The installed launcher names `DSUN -W0 -L`
(FND-EXE-008).

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

### Shipped interpreter

`DOSBOX/DOSBox.exe` is an i386 PE32 executable addressed by full virtual
addresses at its header's preferred image base `0x00400000` (FND-EXE-011).
It is included in the manifest because its compiled label-handling paths
are studied in FND-EXE-011, FND-EXE-012, FND-EXE-013, FND-EXE-014,
FND-EXE-015, FND-EXE-016, FND-EXE-017, FND-EXE-018, FND-EXE-019 and
FND-EXE-020, FND-EXE-021, FND-EXE-022, FND-EXE-023, FND-EXE-024, FND-EXE-025, FND-EXE-026, FND-EXE-027, FND-EXE-028, FND-EXE-029, FND-EXE-030, FND-EXE-031, FND-EXE-032, FND-EXE-033, FND-EXE-034, FND-EXE-035, FND-EXE-036, FND-EXE-037, FND-EXE-038, FND-EXE-039, FND-EXE-040, FND-EXE-041, FND-EXE-042, FND-EXE-043, FND-EXE-044, FND-EXE-045, FND-EXE-046, FND-EXE-047, FND-EXE-048, FND-EXE-049, FND-EXE-050, FND-EXE-051, FND-EXE-052, FND-EXE-053, FND-EXE-054, FND-EXE-055, FND-EXE-056, FND-EXE-057, FND-EXE-058, FND-EXE-059, FND-EXE-060, FND-EXE-061, FND-EXE-062, FND-EXE-063, FND-EXE-064, FND-EXE-065, FND-EXE-066, FND-EXE-067, FND-EXE-068, FND-EXE-069, FND-EXE-070, FND-EXE-071, FND-EXE-072, FND-EXE-073, FND-EXE-074, FND-EXE-075, FND-EXE-076, FND-EXE-077, FND-EXE-078, FND-EXE-079, FND-EXE-080, FND-EXE-081, FND-EXE-082, FND-EXE-083, FND-EXE-084, FND-EXE-085, FND-EXE-086, FND-EXE-087, FND-EXE-088, FND-EXE-089, FND-EXE-090, FND-EXE-091, FND-EXE-092, FND-EXE-093, FND-EXE-094, FND-EXE-095, FND-EXE-096, FND-EXE-097, FND-EXE-098, FND-EXE-099, FND-EXE-100, FND-EXE-101, FND-EXE-102, FND-EXE-103, FND-EXE-104, FND-EXE-105, FND-EXE-106, FND-EXE-107, FND-EXE-108, FND-EXE-109, FND-EXE-110, FND-EXE-111, FND-EXE-112, FND-EXE-113, FND-EXE-114, FND-EXE-115, FND-EXE-116, FND-EXE-117, FND-EXE-118, FND-EXE-119, FND-EXE-120, FND-EXE-121, FND-EXE-122, FND-EXE-123, FND-EXE-124, FND-EXE-125, FND-EXE-126, FND-EXE-127 and FND-EXE-128. Executable data locations use
shipped-file offsets, separately from code addresses. This does not change
the original game's 16-bit integer width or treat the interpreter as a second
game edition.

The interpreter function inventory is
`coverage/BLD-GOG-EN-1.1/DOSBOX/DOSBox.exe.tsv`: Ghidra 12.1.3 function starts
at that preferred image base and body byte counts only, with no inferred
names. It comes from a fresh PE import whose automatic analysis stopped at
a 180-second timeout, followed by recovery of cited entries during these
findings and a final start/size export. It is an analyzer-discovered inventory,
not a census; body size is not a contiguous function span. Existing MZ
file-offset inventory tools do not validate these PE virtual addresses.

## Compared with other builds

No other build has been studied. Whether the disc image is byte-identical to a retail CD of
version 1.0 or 1.1 is not known, and neither is the history of the installed files that differ
from the disc's.

## Other files

The listing covers two things: every file under the install directory, recursively, and every
file of the ISO 9660 volume on the data track of `game.gog`, read as 2,352-byte `MODE2/2352`
sectors with the 2,048 bytes of user data at offset 24 and walked from the root directory record
of the primary volume descriptor at sector 16. An installed file is listed by its path relative
to the install directory, and a disc file as `CD:` and its path on the disc without the `;1`
version suffix. The listing did not go inside any archive: `webcache.zip`,
`DOSBOX/dosbox-0.74-2.1.tar.gz` and the disc's `MENZO/DATA.A00` and `PANZER/DATA.A00` are listed
as single files, and so are the GFF files, whose members are the subject of the GFF format
entries.

`node tools/evidence/build-listing.mjs <install dir> <install dir>/game.gog
spec/builds/BLD-GOG-EN-1.1.files.yaml spec/builds/BLD-GOG-EN-1.1.other-files.yaml` in the
rebuild's repository makes the listing, hashes each file with xxh3 and compares it with the
manifest and the list of other files. It exits with code 0 only when every path in the listing is
in the manifest with the manifest's size and xxh3, or in the list of other files, and every path
of both files is in the listing. A path in the list of other files that ends in `/` stands for
every file under that directory, and the tool counts those files without hashing them.

The Survey listing command reconciles the complete installation and disc with
the manifest and Other files list. Its path and hash results remain in local
validation output, rather than copied totals here. An independent 7-Zip 22.01
listing of the track stripped to 2,048-byte sectors agreed on disc paths and
sizes.

The paths the manifest leaves out, each with its reason, are in
`BLD-GOG-EN-1.1.other-files.yaml`. The now-studied launch metadata `goggame-1432903719.info` and both
`dosbox_darksun2` configuration files are hashed in the manifest for the direct
wrapper finding FND-EXE-010; this does not make them game-read resources.
The remaining excluded paths include GOG's DOSBox in `DOSBOX/`, the disc image `game.gog` itself,
whose files are listed under `CD:`, `Manual.pdf` and a byte-identical copy named
`ds_wakerave_manual_pdf.pdf` (SRC-MANUAL-1994), `Cluebook.pdf`, `README.TXT` (SRC-README-1.1),
the shortcut and icons, GOG's metadata, licence, web cache and install log, and the uninstaller
`unins000.*`. Three directories hold nothing the build ships: DOSBox writes screenshots to
`capture/`, the game's writes land in `cloud_saves/`, and `analysis/` holds this repository's
local reports. On the disc they are its installer (`INSTALL.EXE`, `INSTALL.NFO`, `BOOT.NFO`) and
the directories `MENZO/` and `PANZER/`, each with an installer, an ARJ extractor and an ARJ
archive for other products.

## Code ranges

The overlay-header bounds recorded in FND-EXE-003 give these half-open code
ranges. Fixups and padding are excluded; resident code is located by address.

| File | Range | Overlay | Finding |
|---|---|---|---|
| `DSUN.EXE` | `0x00057580..0x00057B94` | 169 | FND-EXE-003 |
| `DSUN.EXE` | `0x00057C70..0x00058205` | 170 | FND-EXE-003 |
| `DSUN.EXE` | `0x00058210..0x0005925A` | 171 | FND-EXE-003 |
| `DSUN.EXE` | `0x00059430..0x0005A6FE` | 172 | FND-EXE-003 |
| `DSUN.EXE` | `0x0005A920..0x0005EA7C` | 173 | FND-EXE-003 |
| `DSUN.EXE` | `0x0005EE00..0x0005FB41` | 174 | FND-EXE-003 |
| `DSUN.EXE` | `0x0005FBE0..0x00061347` | 175 | FND-EXE-003 |
| `DSUN.EXE` | `0x00061540..0x00062067` | 176 | FND-EXE-003 |
| `DSUN.EXE` | `0x00062100..0x0006300E` | 177 | FND-EXE-003 |
| `DSUN.EXE` | `0x000630B0..0x00064195` | 178 | FND-EXE-003 |
| `DSUN.EXE` | `0x000642E0..0x00066F95` | 179 | FND-EXE-003 |
| `DSUN.EXE` | `0x000671E0..0x00067D64` | 180 | FND-EXE-003 |
| `DSUN.EXE` | `0x00067EB0..0x0006873C` | 181 | FND-EXE-003 |
| `DSUN.EXE` | `0x00068850..0x0006AD08` | 182 | FND-EXE-003 |
| `DSUN.EXE` | `0x0006AFE0..0x0006CE93` | 183 | FND-EXE-003 |
| `DSUN.EXE` | `0x0006D090..0x0006F3B0` | 184 | FND-EXE-003 |
| `DSUN.EXE` | `0x0006F640..0x0006F6F9` | 185 | FND-EXE-003 |
| `DSUN.EXE` | `0x0006F700..0x0006FDAD` | 186 | FND-EXE-003 |
| `DSUN.EXE` | `0x0006FE30..0x00072B8B` | 187 | FND-EXE-003 |
| `DSUN.EXE` | `0x00072EA0..0x000747E6` | 188 | FND-EXE-003 |
| `DSUN.EXE` | `0x000749B0..0x00077F3A` | 189 | FND-EXE-003 |
| `DSUN.EXE` | `0x00078340..0x0007BE16` | 190 | FND-EXE-003 |
| `DSUN.EXE` | `0x0007C1F0..0x0007D212` | 191 | FND-EXE-003 |
| `DSUN.EXE` | `0x0007D300..0x0007E160` | 192 | FND-EXE-003 |
| `DSUN.EXE` | `0x0007E2F0..0x00080ECE` | 193 | FND-EXE-003 |
| `DSUN.EXE` | `0x00081130..0x000816C9` | 194 | FND-EXE-003 |
| `DSUN.EXE` | `0x00081770..0x00082DAA` | 195 | FND-EXE-003 |
| `DSUN.EXE` | `0x00082F70..0x000833C2` | 196 | FND-EXE-003 |
| `DSUN.EXE` | `0x00083400..0x00086F67` | 197 | FND-EXE-003 |
| `DSUN.EXE` | `0x00087310..0x000878CC` | 198 | FND-EXE-003 |
| `DSUN.EXE` | `0x00087900..0x00088F11` | 199 | FND-EXE-003 |
| `DSUN.EXE` | `0x00089160..0x0008976E` | 200 | FND-EXE-003 |
| `DSUN.EXE` | `0x000897C0..0x0008A368` | 201 | FND-EXE-003 |
| `DSUN.EXE` | `0x0008A4A0..0x0008B12B` | 202 | FND-EXE-003 |
| `DSUN.EXE` | `0x0008B240..0x0008BCC8` | 203 | FND-EXE-003 |
| `DSUN.EXE` | `0x0008BDC0..0x0008E660` | 204 | FND-EXE-003 |
| `DSUN.EXE` | `0x0008E8B0..0x0008E948` | 205 | FND-EXE-003 |
| `DSUN.EXE` | `0x0008E950..0x0009004C` | 206 | FND-EXE-003 |
| `DSUN.EXE` | `0x00090180..0x000916CB` | 207 | FND-EXE-003 |
| `DSUN.EXE` | `0x00091890..0x00093088` | 208 | FND-EXE-003 |
| `DSUN.EXE` | `0x00093160..0x00094D09` | 209 | FND-EXE-003 |
| `DSUN.EXE` | `0x00094F60..0x00095D9A` | 210 | FND-EXE-003 |
| `DSUN.EXE` | `0x00095E10..0x00097F00` | 211 | FND-EXE-003 |
| `DSUN.EXE` | `0x00098200..0x000990EF` | 212 | FND-EXE-003 |
| `DSUN.EXE` | `0x00099260..0x0009AB3A` | 213 | FND-EXE-003 |
| `DSUN.EXE` | `0x0009ACD0..0x0009AD07` | 214 | FND-EXE-003 |
| `DSUN.EXE` | `0x0009AD10..0x0009AD4D` | 215 | FND-EXE-003 |
| `DSUN.EXE` | `0x0009AD60..0x0009ADAD` | 216 | FND-EXE-003 |
| `DSUN.EXE` | `0x0009ADC0..0x0009AE1C` | 217 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00057440..0x00057A54` | 169 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00057B30..0x000580C5` | 170 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000580D0..0x0005911A` | 171 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000592F0..0x0005A61C` | 172 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0005A840..0x0005E9AA` | 173 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0005ED30..0x0005FA71` | 174 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0005FB10..0x0006128B` | 175 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00061480..0x00062017` | 176 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000620C0..0x00062FCE` | 177 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00063070..0x00064155` | 178 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000642A0..0x00066EF5` | 179 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00067140..0x00067CA6` | 180 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00067DF0..0x0006867C` | 181 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00068790..0x0006ACAD` | 182 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0006AF90..0x0006CE88` | 183 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0006D080..0x0006F3A0` | 184 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0006F630..0x0006F6E9` | 185 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0006F6F0..0x0006FD9D` | 186 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0006FE20..0x00072B6E` | 187 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00072E80..0x000747E0` | 188 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000749B0..0x00077F0F` | 189 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00078310..0x0007BDE1` | 190 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0007C1B0..0x0007D1D2` | 191 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0007D2C0..0x0007E0C5` | 192 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0007E260..0x00080E38` | 193 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000810A0..0x00081639` | 194 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000816E0..0x00082DCB` | 195 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00082F90..0x0008354B` | 196 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000835A0..0x00087034` | 197 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000873E0..0x0008799C` | 198 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000879D0..0x00088FEB` | 199 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00089240..0x0008984E` | 200 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000898A0..0x0008A446` | 201 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0008A580..0x0008B20B` | 202 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0008B320..0x0008BE5F` | 203 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0008BF60..0x0008E7CF` | 204 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0008EA20..0x0008EAB8` | 205 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0008EAC0..0x000901B3` | 206 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000902E0..0x0009182B` | 207 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000919F0..0x000931D5` | 208 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000932B0..0x00094E59` | 209 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x000950B0..0x00095EB6` | 210 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00095F30..0x00098020` | 211 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00098320..0x0009920F` | 212 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x00099380..0x0009AC5E` | 213 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0009ADF0..0x0009AE27` | 214 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0009AE30..0x0009AE6D` | 215 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0009AE80..0x0009AECD` | 216 | FND-EXE-003 |
| `CD:DSUN.EXE` | `0x0009AEE0..0x0009AF3C` | 217 | FND-EXE-003 |
