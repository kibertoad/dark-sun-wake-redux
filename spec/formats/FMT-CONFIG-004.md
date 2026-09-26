---
id: FMT-CONFIG-004
title: game.ins disc-track mapping file
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["game.ins"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-CONFIG-008, FND-SOUND-002]
conflicting: []
split_with: []
related: []
---

## Layout

The installed file is an ASCII cue sheet of 2,988 bytes, with 41 consecutive
three-line records and no header or trailer. Lines end in CR LF except the final
line, which has no terminator. Each record has this layout [FND-CONFIG-008]:

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| `FILE` | quoted relative path, file-kind token | file | Selects the file backing one track. | supported | FND-CONFIG-008 |
| `TRACK` | two-digit decimal number, track-kind token | track | Assigns the next track number and kind. | supported | FND-CONFIG-008 |
| `INDEX` | index number and minute:second:frame position | index | Starts the track at the beginning of that file. | supported | FND-CONFIG-008 |

Record 1 names `game.gog` with file kind `BINARY`, track `01` with type
`MODE2/2352`. Records 2 through 41 name `music\TrackNN.ogg` with file kind
`MP3` and track type `AUDIO`, with `NN` equal to the track number. `MP3` is the
cue-sheet token; the named files contain Ogg Vorbis audio [FND-SOUND-002].
The installed DOSBox startup configuration passes `game.ins` to `imgmount` as
drive D [FND-CONFIG-008].

## Enumerations and flags

The file-kind tokens are `BINARY` for record 1 and `MP3` for records 2 through
41. The track-kind tokens are `MODE2/2352` and `AUDIO`, respectively
[FND-CONFIG-008].

## Differences between builds

None known.

## Coverage

The installed `game.ins` file listed by BLD-GOG-EN-1.1.

## Open questions

None known.
