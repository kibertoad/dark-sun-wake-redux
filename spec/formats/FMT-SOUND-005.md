---
id: FMT-SOUND-005
title: GOG disc-audio Ogg tracks
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["MUSIC/Track*.ogg"]
byte_order: little
size: null
text: false
definition: fmt_sound_005.ksy
evidence: [FND-SOUND-002, FND-CONFIG-008, FND-SOUND-017]
conflicting: []
split_with: []
related: []
---

## Layout

The forty files are single-stream Ogg containers carrying Vorbis I packets.
All page envelopes and checksums were checked across all forty inputs;
identification fields and header ordering were checked, while comment/setup
and audio bodies remain opaque (FND-SOUND-017). The published framing and
codec contracts are SRC-XIPH-OGG-VORBIS. Multi-byte page fields use little
endian encoding.

Each page has this envelope; its complete size is 27 plus its segment count
plus the sum of its lacing values (FND-SOUND-017):

| Offset | Size | Type | Name | Meaning | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | 4 | char[4] | capture | OggS. | supported | FND-SOUND-017 |
| 4 | 1 | UINT8 | version | Zero. | supported | FND-SOUND-017 |
| 5 | 1 | UINT8 | flags | Continuation/BOS/EOS flags. | supported | FND-SOUND-017 |
| 6 | 8 | UINT64LE | granule | Position; all-one bits denotes unspecified. | supported | FND-SOUND-017 |
| 14 | 4 | UINT32LE | serial | Logical stream serial. | supported | FND-SOUND-017 |
| 18 | 4 | UINT32LE | sequence | Page number, starting at zero. | supported | FND-SOUND-017 |
| 22 | 4 | UINT32LE | checksum | Direct CRC-32, polynomial 04C11DB7; checksum bytes treated as zero. | supported | FND-SOUND-017 |
| 26 | 1 | UINT8 | segment_count | Number of lacing values. | supported | FND-SOUND-017 |
| 27 | segment_count | UINT8[] | lacing | One-byte segment lengths. | supported | FND-SOUND-017 |
| 27 + segment_count | Sum of lacing | BYTE[] | packet_segments | Opaque packet segments. | supported | FND-SOUND-017 |

A lacing value below 255 terminates a packet, including zero after an exact
multiple of 255 bytes. A packet spanning pages requires continuation on the
following page. Each observed file has BOS on its first page, EOS on its
last, one serial and consecutive sequence numbers, with no trailing bytes
(FND-SOUND-017).

The first packet is a 30-byte identification header, alone on the first
page: type one, six-byte Vorbis signature, 32-bit version zero, one-byte
channel count two, 32-bit sample rate 44100, three signed 32-bit bitrate
hints, two four-bit block-size exponents eight and eleven, and a set framing
bit. The bitrate hints are not used to infer measured bitrate. Header packet
types three and five follow, with the same signature, then audio packets
whose low type bit is zero (FND-SOUND-017; SRC-XIPH-OGG-VORBIS). Their codec
bodies are outside this bounded container description.

## Enumerations and flags

Page flag masks: one continues a packet, two marks the first page, four
marks the last. No other flag bit is set in these inputs (FND-SOUND-017).

## Differences between builds

None known.

## Coverage

The forty `MUSIC/Track*.ogg` paths in BLD-GOG-EN-1.1.

The cue maps Track02 through Track41 to audio tracks two through 41,
respectively, with index 01 at zero. Its MP3 file-kind token does not identify
the codec. GOG's shipped single-game configuration mounts game.ins through
DOSBox's host disc-image path (FND-CONFIG-008, FND-SOUND-017). This static
consumer binding is not an observed playback result.

## Open questions

None.
