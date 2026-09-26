---
id: FMT-CONFIG-004
title: game.ins disc-track mapping file
status: unknown
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["game.ins"]
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

The build manifest lists `game.ins`, and the build record says it maps the Ogg files to disc
tracks. Its layout, byte order and the consumer of each field have not been established.

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The installed `game.ins` file listed by BLD-GOG-EN-1.1.

## Open questions

- What is the bounded layout of this file, and which part of GOG's DOSBox setup reads it (Q-CONFIG-003)?
