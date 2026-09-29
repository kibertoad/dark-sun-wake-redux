---
id: FND-CONFIG-003
title: SOUND.CFG is 59 bytes: two equal 10-byte blocks, a word, two 14-byte driver names and nine more bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND.CFG
    offset: 0x00..0x3A
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`SOUND.CFG` is 59 bytes (XXH3-128 `64d376205212cb8a6b8a782b88576327`). The disc has no file of that
name. Read as little-endian:

| Offset | Bytes | Reading |
|---|---|---|
| `0x00` | 10 | the words `0x0220`, 5, 1, 1 and 122 |
| `0x0A` | 10 | the same ten bytes again |
| `0x14` | 2 | the word 11 |
| `0x16` | 14 | `sbp2fm.adv`, then four NULs |
| `0x24` | 14 | `sbpdig.adv`, then four NULs |
| `0x32` | 9 | the words 1, 4, 8 and 11, then the byte 0 |

In `SOUND.INI` (FND-CONFIG-007), the two records with card ID 122 have `sbp2fm.adv` as their
real-mode music driver and `sbpdig.adv` as their real-mode digital driver, 11 as their music
driver chunk number and 8 as their digital driver chunk number, `0x220` as their first music and
digital address, 5 as their first IRQ, 1 as their first DMA channel, and 1 as their flag count.

## Interpretation

`SOUND.CFG` records the sound card chosen from `SOUND.INI` and its settings: one 10-byte block of
port, IRQ, DMA channel, flag count and card ID for music and one for digital sound, the music
driver's chunk number, the names of the music and digital drivers, and a tail that holds the
digital driver's chunk number among other values. The installed file was made for a Sound Blaster
16.

## Alternatives

The field boundaries and meanings were first read from the values and their
match with one `SOUND.INI` record. FND-CONFIG-022 now identifies direct
writers for the nine-byte tail but does not establish what the input-record
fields mean; the two ten-byte blocks have not been traced field by field.
Which block is music and which is digital cannot be told from equal installed
blocks. An earlier reading gave the tail as 13 bytes; it is 9.

## How to reproduce

Read `SOUND.CFG` as bytes and compare the values with the records of `SOUND.INI` whose card ID is
122.
