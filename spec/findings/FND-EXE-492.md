---
id: FND-EXE-492
title: Only the disc's Miles driver files pass the sound utility's driver test and none requests program execution
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:0A5B..1C08:0A89
  - build: BLD-GOG-EN-1.1
    file: SOUND.INI
    offset: 0x00000000..0x0000D58D
  - build: BLD-GOG-EN-1.1
    file: SOUND.BAT
    offset: 0x00000000..0x00000072
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-491's install routine 1C08:0A25 accepts an image only when
bytes 3..8 are `DIGPAK` (test at 1C08:0A5E..0A76) or bytes 2..5 are `Copy`
(test at 1C08:0A79..0A89); any other image returns FFFF without changing
a driver slot. Every file under the installation directory except
`game.gog` (10503 files, local analysis folders included) and every file
of the ISO 9660 volume on the data track of `game.gog` (264 files) was
read and tested the same way. Exactly 19 pass, all with `Copy` at byte
2 and all in the disc's root directory:

| Disc file | Size | Interrupt 21 requests |
|---|---|---|
| ADLIB.ADV | 14775 | none |
| ADLIBG.ADV | 16257 | none |
| ALGDIG.ADV | 5770 | none |
| ARIADIG.ADV | 5839 | AH=62 at 0x0FED |
| ARIATSR.ADV | 8792 | none |
| GF1DIGI.ADV | 5248 | AH=35 at 0x0FAC and 0x0FC8 |
| GF1MIDI.ADV | 10816 | AH=35 at 0x042F and 0x044B |
| MT32MPU.ADV | 10707 | none |
| PASDIG.ADV | 4361 | none |
| PASFM.ADV | 15242 | none |
| PASOPL.ADV | 16333 | none |
| PCSPKR.ADV | 8434 | none |
| SBAWE32.ADV | 39859 | none |
| SBDIG.ADV | 4667 | none |
| SBFM.ADV | 14825 | none |
| SBP1FM.ADV | 15235 | none |
| SBP2FM.ADV | 16357 | none |
| SBPDIG.ADV | 4946 | none |
| TANDY.ADV | 8746 | none |

Each file's size and XXH3-128 match its `CD:` entry in the build manifest.
In each, every `CD 21` pair that decodes as an aligned `int 21` loads AH
from an immediate earlier in the same aligned decode. No file contains
`int 2E` or a `B4 4B` pair.

SOUND.INI, 54669 bytes in the installation, says in its header that
driver names include an extension and gives each card a real-mode MIDI
name, a real-mode digital name, protected-mode names and chunk numbers.
Its real-mode names are 14 of the `.adv` files above and `ibmsnd.com`;
its protected-mode names are `a32*.dll` files. Neither `ibmsnd.com` nor
any `.dll` exists in the installation or on the disc. SOUND.BAT copies
`D:\*.ADV` to the current directory, runs SOUND_DS and deletes `*.ADV`.

## Interpretation

Whatever name FND-EXE-491's loader receives, the only files of this build
it can install as driver code are these 19 Miles drivers, and none of them
issues a DOS program-execution request through an interrupt 21 instruction
or interrupt 2E. A name that fails to open, or a file that fails the
signature test, leaves no driver code installed. SOUND.BAT's copy step
agrees with the utility reading its real-mode `.adv` names from the
current directory, but the parse that fills the device record from
SOUND.INI is not read here, and nothing above depends on it.

This census covers the drivers' direct interrupt 21 instructions only.
Runtime-built requests and indirect transfers inside the drivers, and
vectors they chain after AH=35 reads, are not read.

## Alternatives

Reading the DIGPAK branch as live for this build would need a file that
begins with `DIGPAK` at byte 3; none exists, and RESOURCE.GFF's embedded
DIGPAK text sits inside an archive the utility does not name. Reading the
protected-mode names as loadable ignores that no such file exists in the
build. Relying on SOUND.INI alone to identify the loaded drivers would
leave the device-record parse as an unread link; the signature scan does
not need it.

## How to reproduce

Run `python -I tools/research/exec-census/driver_signature_scan.py
<install dir>` and `python -I tools/research/exec-census/disc_adv_exec_census.py
<install dir>/game.gog spec/builds/BLD-GOG-EN-1.1.files.yaml` from the
commit that adds this finding, with the locked evidence Python (Capstone
5.0.7, xxhash 4.0.1). The first reads 2352-byte MODE2 sectors with user
data at byte 24, walks the volume from the primary descriptor at sector
16 and tests bytes 2..5 and 3..8 of every file; the second checks each
root `.ADV` file against the manifest and lists interrupt instructions
whose aligned decodes agree from at least 20 of the 48 preceding start
offsets. Decode SOUND_DS.EXE load offsets 0xCADB..0xCB09 for the
signature tests. Read SOUND.INI and SOUND.BAT as text. Nothing is
extracted to disk and nothing is executed.
