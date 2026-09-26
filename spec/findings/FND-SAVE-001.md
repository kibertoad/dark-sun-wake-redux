---
id: FND-SAVE-001
title: The installed CHARSAVE.GFF holds ten 9-byte GREQ and eleven 2-byte CACT resources
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xD6C..0x1B47
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Read through the directory of the installed `CHARSAVE.GFF` (FMT-GFF-001, FND-PARTY-005), the
`GREQ` resources are numbered 1 to 10 and are 9 bytes each, and the `CACT` resources are numbered
29 to 39 and are 2 bytes each. The disc's copy has neither tag.

| Resource | File offset | Bytes |
|---|---|---|
| `GREQ/1` | `0x1AF6` | `00 00 00 00 00 08 20 06 00` |
| `GREQ/2` | `0x1AFF` | `00 00 00 01 F0 03 20 06 01` |
| `GREQ/3` | `0x1B08` | `00 00 00 00 00 08 20 06 00` |
| `GREQ/4` | `0x1B11` | `60 06 00 00 00 08 10 03 01` |
| `GREQ/5` | `0x1B1A` | `00 04 00 01 00 08 20 06 02` |
| `GREQ/6` | `0x1B23` | `00 04 00 01 00 08 20 06 02` |
| `GREQ/7` | `0x1B2C` | `F0 02 00 00 00 08 20 06 02` |
| `GREQ/8` | `0x1B35` | `00 00 00 00 00 08 20 06 00` |
| `GREQ/9` | `0x1B3E` | `00 00 00 00 00 08 20 06 00` |
| `GREQ/10` | `0x12EA` | `60 05 00 00 00 08 20 04 04` |
| `CACT/29` to `/35` | `0x168F`, `0x1962`, `0x1692`, `0x1494`, `0x16A8`, `0x16A5`, `0x12F3` | `00 00` each |
| `CACT/36` | `0x1126` | `18 80` |
| `CACT/37` | `0x101F` | `17 80` |
| `CACT/38` | `0xEB5` | `16 80` |
| `CACT/39` | `0xD6C` | `15 80` |

`PREF/100` is at `0x1AED`, directly before `GREQ/1`, and `GREQ/1` to `/9`
follow one another with no gap. `GREQ/10` ends at `0x12F3`, where `CACT/35` starts.

## Interpretation

Each `GREQ` resource belongs to one of ten saved games, numbered as the save routine numbers them
(FND-SAVE-004), and holds four 16-bit words and a byte. The `CACT` resources hold the identifiers
of stored characters (FND-PARTY-012); 0 marks a free number. Resources that sit next to each other
were written one after another, so `PREF/100` and `GREQ/1` to `/9` were written together, and
`GREQ/10` just before `CACT/35`.

## Alternatives

That neighbouring resources were written together assumes the archive appends a resource it
writes; the order of writing was not observed.

## How to reproduce

Read the directory of `CHARSAVE.GFF` and of the disc's copy, and list the `GREQ` and `CACT`
resources with their offsets and bytes.
