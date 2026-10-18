---
id: FMT-TEXT-003
title: Text resource of CR LF lines
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF", "GPLDATA.GFF"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-TEXT-003, FND-TEXT-008]
conflicting: []
split_with: []
related: []
---

## Layout

The layout of every resource under the tag `TEXT` [FND-TEXT-003]. The encoding is ASCII, with
only the printable characters `0x20` to `0x7E`. Each line ends with CR LF (`0x0D 0x0A`),
including the last. No shipped line is empty. There are no keys, sections or comments, and the
resource has no header or terminator: its size is the one the GFF directory gives.

The game reads the resources in three ways [FND-TEXT-008]:

- `TEXT/0` to `TEXT/32`, `TEXT/100` to `TEXT/118` and `TEXT/200` to `TEXT/207`: only the first
  line, up to the first CR and at most its first 15 characters, as a new character's default
  name.
- `TEXT/1000`: split into a table of 256 name entries. Every run of LF and CR bytes at a line's
  start becomes 0 bytes, so the CR that ends a line stays as the line's last character and the LF
  after it ends the string. Entry 0 and entry 1 are both line 0, and entry `m + 1` is line `m`.
  Entries not reached keep the string `thing`, or `RESERVED` for entries 0 to 9 when the resource
  cannot be loaded. Before the split the game clears the four bytes of entry `size - 1`, counted
  from the table's start, whatever the resource's size in bytes; for the shipped 2,403-byte
  resource that lies outside the table.
- `TEXT/98` and `GPLDATA.GFF#TEXT/99`: only their first four bytes, as the version numbers in the
  version line shown with the `911` command-line argument.

What the original does with a resource that breaks this layout is not known beyond these reads.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| each line, from 0 | `char[]` | `line` | One line of text, 1 to 18 characters in the shipped resources. | supported | FND-TEXT-003 |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 62 `TEXT` resources of the installed `RESOURCE.GFF`, `GPLDATA.GFF#TEXT/99`, and the 61 of the
disc's `RESOURCE.GFF` [FND-TEXT-003]. The disc's copies lack `RESOURCE.GFF#TEXT/98` and
`GPLDATA.GFF#TEXT/99`, and its other 61 resources are identical to the installed ones.

## Open questions

None known.
