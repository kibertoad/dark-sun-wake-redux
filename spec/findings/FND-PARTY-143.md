---
id: FND-PARTY-143
title: Byte 0x1F of a CHAR record is the combatant kind, 0 (the party's own kind) in every shipped record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x536..0x537
tool: Python 3.14.7 (a scratch script reading the CHAR resources through the GFF directory as tools/research/exec-census/details_chunks.py does)
environment: null
---

## Observation

A load copies a CHAR record's byte at `0x1F` to `+0x15` of the character's FMT-COMBAT-001 record
(FND-PARTY-049). That byte is the combatant's kind: `28C9:2E0E` reads and changes it, and treats
kind 0 as one of the four kinds of the party's side, the side whose mask `0x71` the experience
award requires of a member (FND-PARTY-095, FND-PARTY-086).

Read through the directory of the installed `CHARSAVE.GFF` as `details_chunks.py` reads it, the
byte at `0x1F` is 0 in all 19 `CHAR` resources (file `0x536` in resource 50, at `0x517`).

## Interpretation

The byte at `0x1F` is the character's combatant kind as it was saved. Every shipped character is
of kind 0, the party's own, so a loaded character is on the party's side and can be given experience; a
saved character that a provocation had moved to another kind would load with that kind.

## Alternatives

- Whether anything other than a load, such as the start of play, sets the kind of the party slots,
  was not read.
- No save written in play was read, so whether a party member's kind is ever other than 0 when
  saved is open.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python: read the directory of
the installed `CHARSAVE.GFF` as `tools/research/exec-census/details_chunks.py` does and print byte
`0x1F` of each `CHAR` resource.
