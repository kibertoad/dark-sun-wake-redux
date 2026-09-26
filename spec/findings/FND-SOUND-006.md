---
id: FND-SOUND-006
title: The five .VOC texts in DSUN.EXE are file-name patterns the game formats to play or delete voice files
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0F73..57E0:0FCB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:18A3..57E0:18AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0AB4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0B54..2C5F:0C3F
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportBytePattern, ReportInstructionContext); re-read with Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000, and a search for pushed and loaded offsets
environment: null
---

## Observation

`DS` is the data segment `57E0`. A search of the whole file finds `.VOC` five times, at file
offsets `0x4DF7E`, `0x4DF92`, `0x4DF9F`, `0x4DFB5` and `0x4E8AA`; Ghidra shows them as
`5000:8D7E`, `5000:8D92`, `5000:8D9F`, `5000:8DB5` and `5000:96AA`. Each is the end of a text that
starts a few bytes earlier:

| Text | Address |
|---|---|
| `%sSOUND%03u.VOC` | `DS:0F73` |
| `%c:\INTR\INTR%u.VOC` | `DS:0F83` |
| `%sSPCH%u.VOC` | `DS:0F97` |
| `%c:\SPEECH\SPCH%u.VOC` | `DS:0FA4` |
| `%s\SPC*.VOC` | `DS:18A3` |

`%c:\RESOURCE.GFF` follows at `DS:0FBA`. Ghidra finds no reference to the five `.VOC` addresses,
because the code refers to the start of each text. A search for `PUSH` of each start offset finds:

- `DS:0F73` pushed at `2C5F:0AB4`, in the sound-effect routine (FND-SOUND-007). A
  `MOV BX, 0F73h` at `3B00:0738` indexes a table in its own code segment and is not a reference.
- `DS:0F83`, `DS:0F97`, `DS:0FA4` and `DS:0FBA` pushed at `2C5F:0B54`, `2C5F:0B7C`, `2C5F:0BAE`
  and `2C5F:0C3F`, in the speech routine (FND-SOUND-008).
- `DS:18A3` pushed at `DSUN.EXE+0x00072233`, offset `0x2403` of overlay 187. The routine there,
  from `DSUN.EXE+0x00072213` (offset `0x23E3`), fills a buffer through the far routine at
  `44DE:0569`; when bit 2 (value 4) of the byte at `DS:55BC` is set, it formats `DS:18A3` with that buffer and passes the result to
  `44DE:02A9`, which calls `INT 21h` with `AH` 41h.

Each of the first four pushes is followed by a call to the far routine at `3150:000E` with a
buffer at `DS:43FB` as its first argument.

## Interpretation

The texts are `sprintf` patterns. The game builds the name of a loose sound-effect file from the
installation directory and a three-digit number, the name of a speech file from the installation
directory or the disc's `SPEECH` directory, and the name of an introduction voice file on the
disc. `44DE:0569` reads the current directory and `44DE:02A9` is the DOS delete service, so with
bit 2 of `DS:55BC` set the game deletes the installed speech files (FND-SOUND-010).

## Alternatives

That `3150:000E` is `sprintf` is read from its arguments, and that `44DE:0569` reads the current
directory is a guess from the pattern it fills; it was not read. Whether DOS accepts the wildcard in
`SPC*.VOC` for this service, and when overlay 187 runs this routine, were not examined.

## How to reproduce

Search the file for `.VOC`, read the text around each match, and search the load image and the
overlays for `68` followed by each start offset.
