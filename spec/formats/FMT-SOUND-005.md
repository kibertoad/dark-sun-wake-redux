---
id: FMT-SOUND-005
title: GOG disc-audio Ogg tracks
status: unknown
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["MUSIC/Track*.ogg"]
byte_order: little
size: null
text: false
definition: null
evidence: []
conflicting: []
split_with: []
related: []
---

## Layout

The build manifest lists forty `.ogg` files, numbered Track02 through Track41,
under `MUSIC/`. Their encoded layout, relationship to disc tracks, and playback
path have not been verified. The `byte_order` metadata above is required by the
format-entry schema; it is not a finding about these files.

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The forty `MUSIC/Track*.ogg` paths in BLD-GOG-EN-1.1.

## Open questions

- What is the validated audio container and track mapping, and which part of
  the GOG runtime consumes these files (Q-SOUND-008)?
