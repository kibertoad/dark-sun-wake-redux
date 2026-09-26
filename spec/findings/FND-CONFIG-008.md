---
id: FND-CONFIG-008
title: The installed cue sheet has one data track and forty file-backed audio tracks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: game.ins
    offset: 0x000..0xBAB
tool: Windows PowerShell 5.1 text and byte inspection
environment: null
---

## Observation

The installed `game.ins` is 2,988 ASCII bytes. It consists of 41 consecutive
three-line records. Lines end with CR LF except the final line, which has no
terminator. Every record has a `FILE` line, a `TRACK` line and an `INDEX` line.
The first names `game.gog` as `BINARY`, track `01` as `MODE2/2352`, and index
`01` at `00:00:00`. Records 2 through 41 name `music\TrackNN.ogg` as `MP3`,
track `NN` as `AUDIO`, and index `01` at `00:00:00`. The `NN` values are
consecutive decimal numbers from 02 through 41. A bounded full-file pattern
check matched all 2,988 bytes and found no other records.

The installed `dosbox_darksun2_single.conf` issues `imgmount d
"..\game.ins" -t iso`. The installed DOSBox manual describes `IMGMOUNT` as
the command to mount a CD image through a cue sheet.

## Interpretation

The installed DOSBox configuration is the consumer of this cue sheet. The
`FILE` lines select the disc image and audio files; the `TRACK` and `INDEX`
lines give DOSBox their track numbers, kinds and starting positions. The `MP3`
token is the cue sheet's file-kind token, even though the named files have
Ogg Vorbis payloads (FND-SOUND-002).

## Alternatives

This inspection establishes the shipped file's layout and its configured
consumer. It does not establish how DOSBox handles malformed cue sheets or
whether the original retail CD had the same audio track order.

## How to reproduce

Read the installed `game.ins` as ASCII and verify the 41 `FILE`/`TRACK`/`INDEX`
triples cover every byte, including line endings. Compare the `imgmount` line
in `dosbox_darksun2_single.conf` with the installed DOSBox manual's `IMGMOUNT`
description. Do not start DOSBox.
