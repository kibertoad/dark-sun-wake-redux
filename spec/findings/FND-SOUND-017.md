---
id: FND-SOUND-017
title: All forty GOG audio tracks have complete checksum-valid Ogg page framing and stereo 44100 Hz Vorbis identification
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: MUSIC/Track02.ogg
    offset: 0x00000000..0x00376612
  - build: BLD-GOG-EN-1.1
    file: MUSIC/Track41.ogg
    offset: 0x00000000..0x003C5A9F
  - build: BLD-GOG-EN-1.1
    file: game.ins
    offset: 0x00000000..0x00000BAC
tool: tools/research/ogg-tracks.mjs at 8cd7457e; committed manifest XXH3-128 identities; Node.js 24.15.0; published framing contracts SRC-XIPH-OGG-VORBIS
environment: null
---

## Observation

FND-SOUND-002 identifies forty prefix signatures and the cue mapping;
FND-CONFIG-008 identifies the configured host consumer. This validates
the container framing throughout every Track02..41 file, independently
of the extension and cue file-kind token. Every input's size and XXH3-128
matched BLD-GOG-EN-1.1.files.yaml before inspection. No sample is played,
decoded or copied into a committed fixture.

### Complete page-envelope census

The forty inputs total 99,131,938 bytes, 22,644 pages and 263,860 packets.
The bounded inspector consumed every byte without resynchronization,
trailing data or an undecided page. Every page has capture OggS, version
zero, only declared flag bits, a complete lacing table/body and the correct
32-bit direct CRC with its checksum field zeroed during calculation.
Each file contains one serial stream with consecutive page sequence
numbers from zero, BOS only on its first page and EOS on its final page.
Continued packets agree with the preceding lacing state; no final packet
is unfinished. Each final granule is specified rather than all-one bits.

Every file's first packet is the 30-byte Vorbis identification header,
alone on the first page. Its fields are version zero, two channels,
sample rate 44100, block sizes 256 and 2048 and a set framing bit.
The next two packets have the ordered type-three comment and type-five
setup signatures. All subsequent packets have the audio type bit clear.
These checks establish container framing and identification, not complete
comment/setup semantics or audio-packet decodability: those bodies remain
opaque, and no PCM or audible result was produced.

### Cue mapping and configured consumer

Fresh size/hash checks also matched game.ins and
dosbox_darksun2_single.conf to the same build manifest. The complete
2988-byte cue has exactly 41 FILE/TRACK/INDEX triples. Track one names
game.gog as BINARY/MODE2/2352. Each track n from two through 41 names
music\\TrackNN.ogg with the same decimal n, file-kind MP3, track-kind
AUDIO and index 01 at 00:00:00. Tracks use two indentation spaces;
the first index uses four and the forty audio indexes use six. Lines
are CRLF, with no final terminator. Exact reconstruction matched all bytes.

The shipped single-game configuration contains the imgmount command
selecting D: from ../game.ins with type iso, as FND-CONFIG-008 records.
Thus the configured consumer is the host DOSBox disc-image/audio path;
the cue's MP3 token does not make the payload MPEG audio. No GOG DOSBox
process was launched, attached to or controlled.

## Interpretation

Q-SOUND-008's container, file-to-track mapping and configured-consumer
questions are answered. FMT-SOUND-005 can describe a supported Ogg
envelope with identified Vorbis I packets and opaque codec bodies.
This does not establish native playback, audio fidelity, actual host
decoder behavior, complete codec conformance or the original retail CD's
track order. Gameplay music selection remains the separate existing
sound rules and questions. No parity validation or complete-reading claim
follows from format validation.

## Alternatives

Calling these MPEG files from their cue token contradicts their packet
identification. Prefix signatures alone would not establish the 22,644
page checksums, complete framing or absence of trailing bytes. Treating
an opaque setup signature as a fully validated Vorbis setup or decoded
audio would exceed the inspector. Treating configured consumption as an
observed playback result would confuse static configuration with execution.

## How to reproduce

With the licensed installation resolved through tools/game-dir.mjs, run
node tools/research/ogg-tracks.mjs twice. The tool fingerprints the forty
inputs against the committed build manifest, bounds each file to 16 MiB
and each packet to 1 MiB, rejects unsupported chaining/multiplexing and
emits only metadata. Its independent bitwise CRC and malformed-envelope
controls are specified in DISC-AUDIO-SURVEY.

The accepted reports are ogg-tracks.json and ogg-tracks-repeat.json under
GAME_DIR/analysis/disc-audio-survey-20261104/. They repeat byte-for-byte,
SHA-256 f7c6e858081d1b24130b34b36c5fa85a3042546ecb53aefeaaae8ed7ad56c6a6.
Reconcile every path/track with the complete cue and verify the configured
imgmount consumer without executing it. Original media and payloads remain
in their licensed local stores.
