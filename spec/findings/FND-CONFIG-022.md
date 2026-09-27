---
id: FND-CONFIG-022
title: Sound setup populates the nine-byte SOUND.CFG tail from a record and a constant
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:4897..1000:48C9
tool: raw displacement search and Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded MZ ranges
environment: null
---

## Observation

The installed `SOUND_DS.EXE` at `C:\GOG Games\Dark Sun 2\SOUND_DS.EXE`
has the 204,593-byte length recorded in FND-CONFIG-004. Its data segment
is `1E36`. The `SOUND.CFG` write in FND-CONFIG-004 takes 59 bytes from
`1E36:E01B`; offsets `0x32..0x3A` of that buffer are `E04D..E055`.

The routine at file offsets `0x00005C97..0x00005CC9` assigns those bytes
from a far record pointer passed as an argument:

| Output offset | Writer | Value supplied |
|---|---|---|
| `0x32` | `0x00005C9A..0x00005C9E` | word at input record `+0x2A` |
| `0x34` | `0x00005CA1` | literal word 4 |
| `0x36` | `0x00005CAA..0x00005CAF` | word at input record `+0xD6` |
| `0x38` | `0x00005CB5..0x00005CBA` | word at input record `+0xD8` |
| `0x3A` | `0x00005CC0..0x00005CC5` | byte at input record `+0xDA` |

Two other bounded paths write the same output word at `0x36` from input
record `+0xD6` and the byte at `0x3A` from `+0xDA`: file offsets
`0x00005D36..0x00005D49` and `0x00005DCE..0x00005DE1`. A raw search for
the five output displacements found these writes and a comparison of
`E04D` with 4 at `0x0000572A`.

## Interpretation

The nine-byte tail is constructed by sound setup from four input-record
fields and one fixed word, rather than copied verbatim from a sound-card
record. The installed file's word at `0x34` is 4, agreeing with the
literal write (FND-CONFIG-003).

## Alternatives

This reading does not establish what the input fields at `+0x2A`, `+0xD6`,
`+0xD8` and `+0xDA` mean or which setup selection reaches each writer.
The raw displacement search bounds literal references to these output
addresses; computed writes or whole-buffer copies remain possible. The
game's consumers of tail offsets `0x34..0x3A` also remain unread.

## How to reproduce

Use the installed `SOUND_DS.EXE` whose size and XXH3-128 are recorded in
FND-CONFIG-004. Search for the little-endian displacements `4D E0`,
`4F E0`, `51 E0`, `53 E0` and `55 E0`, then disassemble only
`0x00005C97..0x00005CC9`, `0x00005D36..0x00005D4C`,
`0x00005DCE..0x00005DE4` and the comparison near `0x0000572A` in
16-bit mode. Correlate the displacements with the 59-byte write buffer at
`1E36:E01B` (FND-CONFIG-004).
