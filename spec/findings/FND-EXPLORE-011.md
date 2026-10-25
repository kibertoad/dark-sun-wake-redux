---
id: FND-EXPLORE-011
title: 3D72:0D83, which the edge-scroll pass calls first in pointer mode 5, sets DS:2FB8 and runs the page flip 4328:0B4C, and with its hook at DS:A119 never set it writes neither view word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0D83..3D72:0DE5
tool: Capstone 5.0.9; tools/research/exec-census/resident_listing.py and immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

`3D72:0D83..0DE5` calls far through `DS:A119` with 0, bracketed by `39D1:0609` and `39D1:05E1`, when
that double word is not 0, and returns `0xFFFF` when the call returns nonzero. It then stores 1 in
`DS:2FB8`, calls `4328:0B4C`, makes the same bracketed call with 1 when `DS:A119` is not 0, and
returns `0xFFFF` or 0 as before (FND-CONFIG-171). It stores nothing else.

FND-EXE-588 finds `DS:A119` 0 in the load image and written only at `39D1:0272` (0) and in
`3D72:08E1`, which has no caller. FND-PARTY-195 reads `4328:0B4C`: past its gates it writes
`DS:2FB8`, `CS:118E`, `CS:0004`, `CS:0006` and video memory, and calls `409B:1675` and the pointer
services `3D72:0DE5`, `3D72:0E10`, `3D72:0B84` and `3D72:0942`. `immediate_search.py` with `1408`
and `140A` finds direct stores to `view_left` and `view_top` only in segment `2C5F` (at
`2C5F:01FF..0244`), none of them in those routines.

## Interpretation

In the edge-scroll pass, the call of `3D72:0D83` while `pointer_mode` is 5 shows the drawn page
and changes neither `view_left` nor `view_top`; the step that follows takes the view words as they
were.

## Alternatives

The pointer services are cited from FND-CONFIG-189, FND-CONFIG-197 and FND-PARTY-195 rather than
read again. Stores to the view words through a pointer, the way the step at `362C:0ED3` writes
`view_left` (FND-EXPLORE-007), are not covered by the immediate search; none of the routines named
here is given the address of a view word.

## How to reproduce

`resident_listing.py <dsun> 3D72:0D83..3D72:0DE5`; `immediate_search.py <dsun> 1408 140A`.
