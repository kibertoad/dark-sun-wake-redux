---
id: FMT-SCRIPT-002
title: String in script code
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["GPLDATA.GFF"]
byte_order: big
size: null
text: false
definition: fmt_script_002.ksy
evidence: [FND-SCRIPT-010]
conflicting: []
split_with: []
related: [RULE-SCRIPT-004]
---

## Layout

A string inside the code of a script resource (FMT-SCRIPT-001), after the expression byte `0x92`
[FND-SCRIPT-010]. Its first byte says how the rest is stored. `read_string` (RULE-SCRIPT-004)
decodes it into a NUL-terminated string of at most 299 characters.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `kind` | How the string is stored; see the table below. | supported | FND-SCRIPT-010 |
| `0x01` | `packed_size` if `kind == STRING_PACKED` | `BYTE[packed_size]` | `packed` | Characters of 7 bits each, packed high bit first with no gaps: the first character is the top 7 bits of the first byte, the second the last bit of that byte and the top 6 of the next, and so on. The string ends at the first character whose value is 3, and the rest of the byte that holds it is unused. A character below `0x20` or above `0x7E` is read as a space. `packed_size` is the number of bytes up to and including the one that holds the value 3, at most the 262 bytes that hold 299 characters. | supported | FND-SCRIPT-010 |
| | | | | Total size `1 + packed_size` for `STRING_PACKED`, 1 for `STRING_CHARACTER_NAME` | | |

## Enumerations and flags

### kind

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 1 | `STRING_CHARACTER_NAME` | No more bytes follow; the string is copied from one the game supplies. | supported | FND-SCRIPT-010 |
| 5 | `STRING_PACKED` | 7-bit packed characters follow. | supported | FND-SCRIPT-010 |

## Differences between builds

None known.

## Coverage

Read from the executable's decoder [FND-SCRIPT-010]. No sweep over the shipped scripts was made.

## Open questions

- Kind 2 stores bytes up to one whose translated value is 3, each passed through an unread
  routine; its layout is open (FND-SCRIPT-010, Q-SCRIPT-004).
- Which string the game supplies for kind 1; SRC-OPENDS-5C6CBD7 and the project's earlier notes
  take it to be the active character's name (Q-SCRIPT-004).
- What the game does with any other kind byte: the reader reads nothing more and leaves the
  previous string (FND-SCRIPT-010), so the bytes after it would be read as code. (Q-SCRIPT-004)
