---
id: FND-EXE-270
title: Startup first callee saves four vector pairs and restores its incoming data segment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:01B0..1000:01F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:01F3..1000:0220
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-269's declared startup prefix calls `1000:01B0`. This callee
saves incoming DS and issues four interrupt `0x21` requests with AX
`0x3500`, `0x3504`, `0x3505` and `0x3506`, in that order. After each
request it stores current BX and ES to consecutive DS-relative word pairs:
`0x0074/0x0076`, `0x0078/0x007A`, `0x007C/0x007E`, and
`0x0080/0x0082`. It does not reload DS between the requests or test carry.
Those pairs are separately stored results, not an immutable initial table.

It next sets AX `0x2500`, copies CS into DX and then DS, sets DX
`0x01A7`, and invokes interrupt `0x21`. It restores saved DS and returns
near without argument cleanup. Under the admitted CS and intact saved
stack slot, the last request's outgoing pointer is `1000:01A7`, and the
ordinary return restores the caller's incoming DS. External requests and
stack preservation are not established by that local save/restore alone.

The adjacent selected restoration candidate at `1000:01F3` has four
sequential save-DS, LDS-DX, interrupt, restore-DS groups. They request AX
`0x2500`, `0x2504`, `0x2505`, `0x2506`, loading the far pointers from
current incoming DS-relative pairs beginning at `0x0074`, `0x0078`,
`0x007C`, `0x0080`. Each LDS changes DS for its own request; the subsequent
POP restores it before reading the next pair. No result check, pair
clearing or rollback is present. Its final return is far without explicit
argument cleanup. This records the candidate body, not an admitted caller.

## Interpretation

This narrows FND-EXE-269's first-callee obligations: the local body explicitly
restores incoming DS, and its state publications depend on four external
results. These slots use the startup segment, not FND-EXE-176's separate
loader state merely because some offsets resemble its fields. Q-EXE-001
and Q-EXE-010 retain external preservation/results, other pair writers,
restoration caller admission, the installed target's effects and native
arrival at the overlay-loader root. No complete_reading or replacement
inventory follows.

## Alternatives

Assuming the initializer leaves DS equal to CS ignores its final POP.
Assuming DS is preserved throughout ignores its explicit change before the
last request. Reading all restoration pointers through the first LDS's
segment ignores the intervening POPs. The local sequence cannot establish
that the external operations succeed, preserve the stack, or install the
requested vectors; native state requires those contracts separately.

## How to reproduce

At revision `4dbb904`, verify the installed source identity in FND-EXE-236.
Decode shipped `0x000053B0..0x00005420` in sixteen-bit mode, initial IP
`0x01B0`, model segment `0x1000`, MZ header size `0x5200`. Separate the
near-return body ending exclusively at `0x01F3` from the adjacent candidate
ending exclusively at `0x0220`. Follow every store and LDS as a word pair,
and DS save/change/restore in execution order. Use FND-EXE-269's explicit
near call as initializer entry provenance; do not infer a restoration caller
from adjacency. Original source and reports stay in GAME_DIR.
