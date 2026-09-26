---
id: FND-ITEM-009
title: DSUN.EXE holds the item placement and pick-up messages, pushed by overlays 179, 189 and 191
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:877D..5000:87A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:998D..5000:9A96
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:9E18..5000:9E83
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56A7:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5713:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 572F:0000
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The data segment of `DSUN.EXE` is `57E0`; its offset `x` is at `5000:x+0x7E00`. It holds these
NUL-terminated messages. For each, a search of the whole file for `68` followed by the message's
offset as a little-endian word, the encoding of `push` of a 16-bit constant, finds one match in
the `FBOV` pack (FMT-EXE-001) and none in the resident image, except where the table says none:

| Offset | Message | Pushed at |
|---|---|---|
| `097D` | a comma, then that the backpack is full | none |
| `098D` | that a weapon in hand made an action fail | `DSUN.EXE+0x000667A7` (overlay 179) |
| `1B8D` | that the item cannot be readied | `DSUN.EXE+0x0007780E` (overlay 189) |
| `1BA9` | that the placement is illegal | `DSUN.EXE+0x00077828` (overlay 189) |
| `1BBB` | that a container cannot be put in a bag | `DSUN.EXE+0x0007785F` (overlay 189) |
| `1BD7` | that a container cannot be put in a box | `DSUN.EXE+0x00077883` (overlay 189) |
| `1BF3` | that the load is too heavy to carry | `DSUN.EXE+0x000778DB` (overlay 189) |
| `1C0D` | that there are too many items to carry | `DSUN.EXE+0x00077931` (overlay 189) |
| `1C25` | that a two-handed weapon is in use | `DSUN.EXE+0x00077987` (overlay 189) |
| `1C3E` | that two heavy weapons cannot be used | `DSUN.EXE+0x000779F1` (overlay 189) |
| `1C5B` | that two free hands are needed | `DSUN.EXE+0x00077A1E` (overlay 189) |
| `1C6F` | that two shields cannot be used | `DSUN.EXE+0x00077A56` (overlay 189) |
| `1C86` | that the item is grafted on | `DSUN.EXE+0x00077F26` (overlay 189) |
| `2018` | in capitals, that money is found, with `%u` | `DSUN.EXE+0x0007C79B` (overlay 191) |
| `2025` | in capitals, that something is too big to carry | `DSUN.EXE+0x0007CA45` (overlay 191) |
| `2036` | in capitals, that there are too many items to carry | `DSUN.EXE+0x0007CB31` (overlay 191) |
| `204E` | in capitals, that a character, `%Fs`, gets an item | `DSUN.EXE+0x0007CB96` (overlay 191) |
| `205C` | that a weapon is corroded, with a line break | `DSUN.EXE+0x0007CF2D` (overlay 191) |
| `2070` | that armour is corroded, with a line break | `DSUN.EXE+0x0007D0C9` (overlay 191) |

The overlays' resident headers are at `56A7:0000` (179), `5713:0000` (189) and `572F:0000`
(191). The code of overlay 189 is 13,706 bytes from file offset `0x749B0`, and that of overlay
191 is 4,130 bytes from `0x7C1F0`.

## Interpretation

Overlay 189 holds the code that checks where an item may be placed on the inventory screen:
readying, containers in containers, weight, the number of items, hands and shields, and items that
cannot be removed. Overlay 191 holds the code that gives found items and money to a character, and
the code that corrodes weapons and armour. These are the limits the manual describes for the
inventory (RULE-ITEM-001), and more.

## Alternatives

The pushes were not read in context, so which checks run in which order, and on what values, is
not known. A message may also be reached through a table of offsets, which this search does not
find; the backpack message at `097D` may be reached that way or be unused.

## How to reproduce

List the NUL-terminated strings from file offset `0x4D97D`, `0x4EB8D` and `0x4F018`, search the
file for `68` and each offset, and place each match with `tools/ghidra/ReportFbovOverlayMap.ps1`.
