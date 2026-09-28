---
id: FND-CONFIG-071
title: Startup calls the guarded save-capacity message helper after resource initialization
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:003E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0285
tool: Python 3.14.7 resident byte-pattern search and FBOV trampoline inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Overlay 180's `56B2:003E` trampoline targets the routine at file offset
`0x00067AC5`, which contains the save-capacity message call described by
FND-CONFIG-042. The resident main routine calls overlay 180's `56B2:0020`
initialization entry at file offset `0x0001CC2F` (FND-CONFIG-061), then calls
`56B2:003E` at `0x0001CC35` without a branch between the two calls. A raw
search of the resident image for the far-call instruction and its unrelocated
segment and offset found this one literal call to `56B2:003E`.

At the helper's entry, it reads a drive-related value, then checks byte
`0370:0003`. If that byte is zero, it jumps to its return at
`0x00067C9E`. Otherwise it proceeds through its disk-space checks and
numbered-save-file count. The computed capacity at `0x00067C77` must be
below ten for the message call at `0x00067C96` to execute (FND-CONFIG-042).

## Interpretation

The startup path enters the save-capacity helper after resource
initialization. Its message is gated first by the state byte and later by
the calculated capacity; the direct startup call alone does not imply that
the message appears or waits in a particular run.

## Alternatives

The writers and meaning of `0370:0003`, the drive API's live result, and
possible indirect callers of this helper remain unread. A raw literal-call
search does not exclude a constructed or stored far pointer.

## How to reproduce

Decode overlay 180's resident header at file offset `0x0004BD20` using
FMT-EXE-003 and FMT-EXE-004. Its `0x003E` trampoline targets code offset
`0x08E5` from the code start `0x000671E0`. Search the resident image before
the FBOV envelope for the literal far-call bytes targeting unrelocated
`46B2:003E`, and inspect the bounded call window
`0x0001CC1F..0x0001CC43`. Disassemble the helper entry
`0x00067AC5..0x00067AF5` and its message branch
`0x00067C70..0x00067CA2`.
