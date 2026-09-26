---
id: FND-PARTY-019
title: No PLYL resource in RESOURCE.GFF holds a character number as a byte or 16-bit word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x189B3..0x189CF
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The installed `RESOURCE.GFF` holds six `PLYL` resources, numbered 0, 10 and 50 to 53, stored
one after another at `0x189B3..0x189CF`. Resources 0, 10 and 50 are 3 bytes, 51 and 52 are 7
bytes, and 53 is 5 bytes. None of their bytes, and none of their 22 two-byte little-endian
windows at any offset, equals the number of a `CHAR` resource of the installed `CHARSAVE.GFF`
(29 to 43 and 50 to 53).

## Interpretation

The `PLYL` resources do not list characters by their resource numbers in these forms.

## Alternatives

The name suggests a player list, and the numbers 50 to 53 match the disc's second set of
characters, but that is all that links them to the party. They could list characters another
way, or hold something else. No code that reads `PLYL` has been located.

## How to reproduce

List the `PLYL` resources of `RESOURCE.GFF` through its directory (FMT-GFF-001), and compare
each byte and each two-byte little-endian window with the `CHAR` numbers of `CHARSAVE.GFF`.
