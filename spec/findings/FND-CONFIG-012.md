---
id: FND-CONFIG-012
title: The saved PREF byte at offset 2 feeds the sound library's music-level setting
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A6E:0064..4A6E:00D7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A6E:011F..4A6E:0187
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident and FBOV ranges; ReportPhysicalBytePattern.ps1 and ReportFbovOverlayMap.ps1
environment: null
---

## Observation

The load routine in overlay 192 passes the byte at `DS:26B4`, copied from
`PREF/100` offset `0x02`, to `4A6E:0064` at
`DSUN.EXE+0x0007D9FB` (FND-SAVE-005). The far-call fixup names descriptor
81, resident segment `4A6E` (FMT-EXE-002, FMT-EXE-005).

The routine at `4A6E:0064` checks the card ID at offset `0x08` of the
`SOUND.CFG` record (FMT-CONFIG-001). When it is 113, it returns the byte at
`4E71:0C14` without applying the argument. Otherwise it caps its word
argument at 100, then at 90 when the record's word at `0x32` is 3. It stores
the capped byte at `4E71:0C14`, calls `3842:0D18` with that byte and driver
handles, and returns the stored byte. The installed `SOUND.CFG` has card ID
122 and word `0x32` equal to 1.

On entry to the Preferences routine at `DSUN.EXE+0x0008B240`, overlay 203
calls `4A6E:011F` and writes its returned byte to `DS:26B4` when the master
sound byte `DS:13F7` and music-enable byte `DS:1436` are nonzero. The getter
returns 255 for card ID 113 and several unavailable-driver cases; otherwise
it asks the driver through `3842:0D0C` and returns its byte result.

Overlay 203 passes `DS:26B4` when music is enabled, or zero when it is not,
alongside `DS:26B6` to a drawing path at `DSUN.EXE+0x0008B4AB`. On a
music-enable change, its state routine passes `DS:26B4` to `4A6E:0064`;
when the returned value is smaller, it copies the unmodified `DS:26B4` to
`DS:26B6` (`DSUN.EXE+0x0008B81B`). The save-load path uses the same
comparison and copy (FND-SAVE-005). The music player also passes
`4E71:0C14` back to `4A6E:0064` when it starts a song (FND-SOUND-013).

## Interpretation

`PREF/100` offset `0x02` is the requested music level that the game submits
to its sound library. The library uses a 0-to-100 cap, or a 0-to-90 cap for
one configured driver type. `PREF/100` offset `0x04` participates in music
display and receives the request when the library returns a smaller level;
its full role remains unidentified. Neither byte is changed by the
Preferences arrow branches described in FND-CONFIG-010.

## Alternatives

The calls at `3842:0D18` and `3842:0D0C` were not read, so their physical
effect on loudness and the getter's returned range are not established here.
The drawing path's interpretation of its two levels was not read. A music
volume key or another control outside the ordinary Preferences arrows has
not been ruled out. This reading does not settle the new-game initialization
of either byte or whether indirect writes exist.

## How to reproduce

Resolve the far-call fixup at `DSUN.EXE+0x0007D9FB` through descriptor 81 of
the segment table at file offset `0x4B080`, then disassemble
`4A6E:0064..00D7` and `4A6E:011F..0187`. In overlay 203 inspect bounded
windows at file offsets `0x0008B240..0x0008B281`,
`0x0008B490..0x0008B4C0` and `0x0008B7F0..0x0008B845`.
Search the shipped file for the little-endian displacements `B4 26` and
`B6 26`, and check instruction alignment at the reported code locations.
