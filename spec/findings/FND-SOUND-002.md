---
id: FND-SOUND-002
title: The installation's music is 40 Ogg Vorbis files that game.ins mounts as audio tracks 2 to 41
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: game.ins
    offset: 0x00..0x40
  - build: BLD-GOG-EN-1.1
    file: MUSIC/Track02.ogg
    offset: 0x00..0x23
  - build: BLD-GOG-EN-1.1
    file: MUSIC/Track41.ogg
    offset: 0x00..0x23
tool: hex inspection and file listing with Python 3.14.7
environment: null
---

## Observation

`MUSIC/` holds 40 files, `Track02.ogg` to `Track41.ogg`, 99,131,938 bytes in all. Each starts with
the page signature `OggS`, and each first packet, at offset 28, starts with the byte 1 and
`vorbis`.

`game.ins` is a cue sheet. Its first entry gives `game.gog` as track 1, `MODE2/2352`; each further
entry gives one of the Ogg files as the audio track of the same number, from `music\Track02.ogg`
as track 2 to `music\Track41.ogg` as track 41, each with one index at `00:00:00`.

## Interpretation

The game's music is compact-disc audio. GOG's DOSBox mounts `game.ins` as the disc and plays the
Ogg Vorbis files in place of the original audio tracks, so track `n` of the original disc is
`MUSIC/Track{n:02}.ogg` here. The Ogg files are lossy copies; their lengths and levels are not
those of the original tracks.

## Alternatives

Whether the original disc had exactly these 40 audio tracks in this order is not shown by this
build; the cue sheet is GOG's. Which track the game asks for, and when, is FND-SOUND-012 and
FND-SOUND-013.

## How to reproduce

List `MUSIC/`, read the first 35 bytes of each file, and read `game.ins` as text.
