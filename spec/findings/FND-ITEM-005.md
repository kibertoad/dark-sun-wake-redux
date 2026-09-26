---
id: FND-ITEM-005
title: Unpacked, CHARTRAN.EXE names items.bin and reports item translation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 1896:6CE3..1896:6D2A
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 1896:6E4F..1896:6EA5
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 1896:6FF2..1896:709F
tool: DarkSunWakeRedux.Inspect unlzexe, commit e455d51, then hex inspection with Python 3.14.7
environment: null
---

## Observation

Addresses are in the unpacked `CHARTRAN.EXE` (FND-PARTY-010). Its data segment holds these
NUL-terminated strings:

| Address | String |
|---|---|
| `1896:6CE3` | a note that not all Dark Sun 1 items and spells translate |
| `1896:6E4F` | `items.bin` |
| `1896:6E59` | a message that memory for the item data could not be allocated, with `%ld` for the size |
| `1896:6E88` | `rb` |
| `1896:6E8B` | a message that `items.bin` could not be opened |
| `1896:6FF2` | `OLD: %d, NEW: %d` and a line break |
| `1896:7004` | a message that no money bag was found in the Dark Sun 2 `objex.gff` |
| `1896:703E` | the total of items translated, with `%d` |
| `1896:7061` | the total of items not translated, with `%d` |
| `1896:7082` | a message that items have been translated |

An earlier search in Ghidra of the packed file, for the upper-case `ITEMS.BIN` and for `ITEMS`
with a NUL, found neither.

## Interpretation

The character transfer utility reads `ITEMS.BIN` and uses it to translate Dark Sun 1 items into
Dark Sun 2 items; the code is in FND-ITEM-006. The search of the packed file says nothing: LZEXE
compresses the strings.

## Alternatives

None known.

## How to reproduce

Unpack the file as FND-PARTY-010 says and list the NUL-terminated strings of its data segment,
which starts at file offset `0x92F0`.
