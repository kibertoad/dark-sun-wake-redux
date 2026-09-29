---
id: FND-CONFIG-097
title: The selection-change helper skips APFM records and calls only button or menu paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:059E..3D72:0629
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit resident disassembly; MZ segment mapping
environment: null
---

## Observation

The pointer routine calls 3D72:059E with argument zero when the prior
selected control or child index differs from its hit-search result
(FND-CONFIG-095). The helper starts at file offset `0x00032EBE`.
It reads the prior selected pointer at DS:A125 and the tag at the
record it points to. Its explicit two-entry dispatch table contains:

| Tag | Branch file offset |
|---|---|
| BUTN | `0x00032F36` |
| MENU | `0x00032F05` |

Any other tag goes directly to the zero-result exit at `0x00032F44`.
The BUTN branch passes the selected pointer and argument word to
resident 3EBE:042B. The MENU branch first requires DS:A12D not to
be `0xFFFF`, derives a zero-or-one word from whether the argument is
one, and calls three resident menu routines. Those tag-specific
callees' effects are outside this reading.

An APFM record selects neither branch, so its ordinary dispatch path
returns zero without either tag-specific call. The helper does not
have a local null-pointer check before reading the selected record's
tag. The caller replaces DS:A125 with the new hit pointer after the
helper returns (FND-CONFIG-095).

## Interpretation

For an unchanged, valid prior APFM selection, this selection-change
helper adds no button or menu effect before selection refresh. A
transition from a prior button or menu has different callees and is
not covered by that bounded negative result.

## Alternatives

A null, replaced or differently tagged prior pointer does not satisfy
the APFM path's premise. The effects of button/menu calls, stack-guard
failure and other pointer-routine helpers remain unread (Q-CONFIG-008).
This finding does not establish which prior control exists when a player
enters the item-feedback window or whether its message is reached.

## How to reproduce

Map resident 3D72 to file base `0x00032920`. Inspect the helper's
pointer and tag read at `0x00032EBE..0x00032EFB`, decode exactly
two tag words and two target words at `0x00032F49..0x00032F55`,
and inspect the two bounded branches through return
`0x00032F48`. Compare the caller's changed-selection test and later
pointer replacement in FND-CONFIG-095. Keep the prior selected
control distinct from the new hit-search result.
