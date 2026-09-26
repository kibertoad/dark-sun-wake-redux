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
evidence: [FND-TEXT-003]
conflicting: []
split_with: []
related: []
---

## Layout

The layout of every resource under the tag `TEXT` [FND-TEXT-003]. The encoding is ASCII, with
only the printable characters `0x20` to `0x7E`. Each line ends with CR LF (`0x0D 0x0A`),
including the last. No shipped line is empty. There are no keys, sections or comments, and the
resource has no header or terminator: its size is the one the GFF directory gives. What the
original does with a malformed resource is not known.

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

- How the game loads a `TEXT` resource and splits it, and which screen uses which resource and
  line. The resident code names the tag only as unreferenced data; three occurrences lie in the
  code of overlays 186 and 188 (FND-TEXT-004, Q-TEXT-002).
