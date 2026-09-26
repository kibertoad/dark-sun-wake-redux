---
id: FND-PARTY-005
title: The disc's CHARSAVE.GFF holds characters 40 to 43 and 50 to 53; the installed copy adds 29 to 39
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CD:CHARSAVE.GFF
    offset: 0x00..0xF18
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x00..0x2DD7
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The disc's `CD:CHARSAVE.GFF` (3,864 bytes, XXH3-128 `9060735df0d4ab81c844888196e75446`) holds
eight resources under each of the tags `CHAR`, `PSIN`, `PSST` and `SPST`, numbered 40 to 43 and
50 to 53, and nothing else.

The installed `CHARSAVE.GFF` (11,735 bytes, XXH3-128 `ff66cc83e6c938ca9d32ee883db0f573`) holds 19
resources under each of those four tags, numbered 29 to 43 and 50 to 53, together with ten `GREQ`
resources (1 to 10), one `PREF` (100) and eleven `CACT` (29 to 39). Each of the 32 resources the
disc copy holds is byte for byte the same as the installed resource with the same tag and number,
at the same file offset. The files differ in their headers and directories and in the resources
only the installed copy has.

## Interpretation

The disc's archive is the one the game shipped with: two sets of four characters, 40 to 43 and 50
to 53. Characters 29 to 39, with their `CACT` resources, were added after installation, by
creating or transferring characters (FND-PARTY-011, FND-PARTY-012), so the installed copy is one
player's state and not a second shipped set.

## Alternatives

That 29 to 39 were added after installation is inferred from their absence on the disc and from
the code that writes `CHAR` and `CACT` resources together; how the installed copy came to hold
them was not observed. Which four characters START GAME puts in the party is not shown by either
file; a routine of the game loads characters 40 to 43 into the four party slots
(FND-PARTY-013), and the four characters the owner's captures show match records 40, 41 or 53,
42, and 33 or 43 (FND-PARTY-020). A public player review
(<https://steamcommunity.com/profiles/76561198045525236/recommended/1904580>) names a member of
the party START GAME supplies; the name matches records 43 and 33, and the review gives that
character the origin and class the fourth capture shows. Neither file holds a list of party
members (FND-PARTY-007, FND-PARTY-019).

## How to reproduce

Read `CHARSAVE.GFF` from the disc image `game.gog` (a Mode 2/2352 ISO 9660 image) and from the
installation directory, list both directories (FMT-GFF-001), and compare each resource the disc
copy holds with the installed resource of the same tag and number.
