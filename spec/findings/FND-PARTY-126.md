---
id: FND-PARTY-126
title: Overlay 182 +1167 changes DS:0D9C and DS:0DAB only while DS:0DAB is 0, and the message window of overlay 172 +05DB leaves display mode 2 again before it returns when it entered it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000699B7..0x00069A18
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069A83..0x00069A89
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069C6E..0x00069C74
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059A65..0x00059A84
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059B69..0x00059B76
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059BDA..0x00059C08
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059C39..0x00059C46
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. FND-PARTY-124 lists the stores to `DS:0DAB` and `DS:0D9C`.

**Overlay 182 `+1167`** jumps to its end at `+1522` when the word at `DS:0DAB` is not 0, at
`+11BE..+11C5`, before any store to either word (`+1167..+11C8`). Its store of 1 to `DS:0D9C` at
`+1233` and its store of 1 to `DS:0DAB` at `+141E` both come after that test.

**Overlay 172 `+05DB`**, the message window, calls the getter of `DS:0DAB` (`28C9:2522`) and,
unless it returns 2, stores 1 in the byte at `4C4D:0006` and calls overlay 182 `+1856`
(`+0635..+0654`). It ends by calling its own `+0746` and returning (`+0739..+0746`). **`+0746`**
tests that byte: when it is 1 it calls overlay 190 by trampoline `571F:007A` if the getter returns
2, then `3D72:0D83`, overlay 182 `+19F8` and `28C9:2A81` with 1 (`+07AA..+07D8`), and before it
returns it stores 0 in the byte (`+0809..+0815`).

## Interpretation

Overlay 182 `+1167` acts only from mode 0, so it cannot leave `DS:0D9C` at 1 while a window that
`+1856` opened is up. A message shown through overlay 172 `+05DB` from mode 1 enters mode 2 and
returns to mode 1 through overlay 182 `+19F8` before its caller goes on, so a script instruction
that follows a message runs in the mode the message found.

## Alternatives

- Overlay 172 `+05DB` may be reached through paths that leave it early, before `+0746`; its
  branches before `+0739` were not each read.
- Whether a script runs while a window other than the message window keeps mode 2, for example a
  script an item use starts from the inventory, was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 182 699B7 69C80` and
`overlay_listing.py <dsun> 172 59A0B 59C46`.
