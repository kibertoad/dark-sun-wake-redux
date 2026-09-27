---
id: FND-CONFIG-014
title: The launcher, Preferences button and save path use two separate speech gates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024..277B:023F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0AFA..2C5F:0C8D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident and FBOV ranges; ReportPhysicalBytePattern.ps1
environment: null
---

## Observation

`DS:14E4` is zero in the executable's loaded image. The `-L` option that the
GOG `RAVAGER.BAT` supplies sets it to one, while `-P` clears it
(FND-SOUND-010). The Preferences dispatcher changes that byte when the
player clicks voice button `16310` and redraws that button from the same
byte (`DSUN.EXE+0x0008B3F9` and `0x0008B852`, FND-CONFIG-010). Neither
branch reads or writes `DS:1439`.

`DS:1439` is one in the loaded image. The save routine copies it to
`PREF/100` offset `0x08`; the load routine copies that offset back. Neither
routine includes `DS:14E4` in the nine-byte resource (FND-SAVE-004,
FND-SAVE-005). The speech player at `2C5F:0AFA` requires both bytes to be
nonzero, as well as the master and digital-sound gates (FND-SOUND-008).

An overlay 187 path at `DSUN.EXE+0x000720F3..0x0007211F` also tests the
master and digital gates followed by `DS:14E4` and `DS:1439` in that order;
it skips a sound-library call when any test is zero. This second consumer
again reads the two bytes separately.

A physical search of the installed `DSUN.EXE` for the two-byte
little-endian displacements `E4 14` and `39 14` returned 13 and 8 matches,
respectively. These are candidate literal references, including data and
instruction-interior matches; they do not rule out indirect access.

## Interpretation

The ordinary Preferences voice click changes a runtime gate, not the byte
that this save format persists. The GOG launch command enables that runtime
gate independently of `PREF/100`. In the named playback paths, either gate
at zero prevents the associated speech or sound-library call. A save and
load cycle can restore `DS:1439` without restoring a Preferences click's
change to `DS:14E4`.

## Alternatives

This reading does not prove that no other routine synchronizes the bytes,
copies a larger memory block over either one, or changes either value during
new-game initialization. The overlay 187 call's audible effect and the
reason for keeping two gates were not established. A saved value of zero at
offset `0x08` would block the identified paths, but how an ordinary player
could produce that saved value remains open.

## How to reproduce

Read the command-line switch branches and loaded-image bytes listed in
FND-SOUND-010. Compare the nine-byte copy lists in FND-SAVE-004 and
FND-SAVE-005 with the Preferences voice branch at file offset
`0x0008B3F9`. Disassemble the speech-player gate at `2C5F:0AFA` and the
bounded overlay 187 path at `0x000720F3..0x0007211F`. Search the shipped
file for `E4 14` and `39 14`, inspecting instruction alignment before
treating a raw match as a reference.
