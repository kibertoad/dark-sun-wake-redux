---
id: FND-PARTY-009
title: The resident image of DSUN.EXE holds CHAR only inside text and has no PSIN, PSST, SPST or CACT bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:825B..5000:AC2B
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

In the resident load image of `DSUN.EXE` (`1000:0000..6237:0000`), the four bytes `CHAR` occur
14 times, all between `5000:825B` and `5000:AC2B`: twice in the `CHARSAVE.GFF` strings at
`5000:9188` and `5000:9197` (FND-PARTY-008), and twelve times at the start of the upper-case
words CHARACTER, CHARACTERS and CHARS in the game's own messages and labels, such as the one at
`5000:AB5E` in the label VIEW CHARACTER. `PSIN`, `PSST`, `SPST` and `CACT`
do not occur in the resident image.

All five tags occur in the FBOV overlay pack, inside the code of overlays 171, 182, 184 and 186
(FND-EXE-006, FND-PARTY-012).

An earlier search in Ghidra, of the load image without the overlays, found the same 14 `CHAR`
matches with no direct reference to any of them, and no `PSIN`.

## Interpretation

No resident code names these tags as literals; every use of them the file holds is in overlay
code.

## Alternatives

Resident code could still pass a tag it receives from overlay code or builds at run time. A
search of the load image alone, as the Ghidra search was, cannot see the overlay code, so its
negative result for `PSIN` says nothing about the game as a whole.

## How to reproduce

Search the whole file for each tag's four bytes and split the matches at the end of the load
image, file offset `0x57570`; read the bytes around each resident match.
