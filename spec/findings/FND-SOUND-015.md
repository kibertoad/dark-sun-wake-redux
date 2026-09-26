---
id: FND-SOUND-015
title: CSEQ resource 1000 is an Extended MIDI file with one sequence that sets a tempo, loops and plays no note
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x378C6C..0x378CBA
tool: hex inspection with Python 3.14.7, reading the GFF directory as FMT-GFF-001 describes; a metadata-only record profile and a comparison with the GPL resource numbers of GPLDATA.GFF made with the repository's inspector
environment: null
---

## Observation

`RESOURCE.GFF` holds one `CSEQ` resource, number 1000, 78 bytes, at file offset `0x378C6C`. Read as
big-endian chunks of a four-letter name and a 32-bit length, it is:

| Offset | Chunk | Length | Content |
|---|---|---|---|
| `0x00` | `FORM` | 14 | `XDIR`, then `INFO` of length 2 holding the little-endian word 1 |
| `0x16` | `CAT ` | 48 | `XMID`, then `FORM` of length 36 holding `XMID` and `EVNT` of length 24 |

The 24 event bytes are `FF 20 01 09`, `FF 51 03 07 0A E2`, `B9 74 00`, `B0 77 7F`, a delay byte
`1B`, `B9 75 7F`, `FF 2F 00`, and one byte 0.

An earlier inspection divided the 78 bytes into six units of 13 and compared each aligned word
with the 330 `GPL ` resource numbers of `GPLDATA.GFF`: at the leading-word position one of the
six values is a GPL number, and at the other word positions 1, 1, 0, 1, 0 and 2 are. Offset 11 of
the units holds five distinct values and offset 12 four.

## Interpretation

The resource is an Extended MIDI (XMIDI) file, the format of the Miles sound drivers: a
catalogue of one sequence. The sequence sets a tempo of 461,538 microseconds a beat (`FF 51`),
starts a loop with controller 116 on channel 10, sends controller 119 on channel 1, waits 27
ticks and ends the loop with controller 117; it plays no note. `CSEQ` reads as a music sequence
for the FM or MIDI driver. The division into 13-byte units matches no layout: the chunk
boundaries fall elsewhere, and the GPL-number matches are chance.

## Alternatives

No code that reads a `CSEQ` resource was located, so what the game does with this one is not
known; a sequence of silence could be a placeholder. The meanings of the controllers follow the
published XMIDI conventions and were not checked against the game's driver.

## How to reproduce

Read resource `CSEQ/1000` of `RESOURCE.GFF` through its directory and split it into
IFF chunks.
