---
id: FND-CONFIG-112
title: Resident mode five dispatch forwards two input words to overlay 208 entry 006B
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:05CF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0C85
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:006B
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; MZ relocation and FBOV fixup mapping
environment: null
---

## Observation

The exact MZ relocated direct-call inventory has one resident call
to overlay 208 entry 57A6:006B, at file offset `0x0001EB15`.
Its containing resident routine 28C9:05CF starts at
`0x0001E45F` and returns at `0x0001EB7A`, with no intervening
return before this call. The segment's file base is `0x0001DE90`.

An early gate continues directly when word `3C10:0019` is zero
or one. Otherwise it exits when signed DS:44E6 is four or greater,
or when bit `0x20` is set at byte offset 24 of the selected 49-byte
record reached through DS:19C9. That reading does not establish
validity or provenance of the selected record index.

The routine then reads DS:1440, subtracts one and admits only
unsigned results zero through four. Its five-target table at
`0x0001EB85..0x0001EB8F` selects the block at
`0x0001EB11` for stored mode five. That block pushes the four
bytes at BP+12 as two words and calls 57A6:006B. The callee
therefore receives the resident caller's words at BP+12 and BP+14
as its first and second word arguments. The branch has no further
comparison before the call. After return, it dispatches on signed
byte DS:43FA; that later dispatch does not gate the earlier call.

Overlay 208 setup explicitly stores five in DS:1440 before its
later helpers and local call (FND-CONFIG-109). Entry 006B adds
DS:1408 and DS:140A to its received words, then applies the
stored-record and helper guards before the selector caller
(FND-CONFIG-110). These are separate gates after resident dispatch.

The declared FBOV direct far-call inventory finds no overlay call
to 57A6:006B. Searching overlay 208's declared code-and-data range
finds no 16-bit relative near-call candidate to its entry at
`0x000929DA`. The fixup decoding was checked against the known
eleven declared selector calls in FND-CONFIG-101.

## Interpretation

The guarded local route in FND-CONFIG-110 has a concrete resident
producer selected by stored mode five. Setup supplies an explicit
mode-five assignment, while the resident caller forwards its own
input words rather than constructing them from the setup arguments.
A qualifying mode does not itself establish the resident routine's
invocation, its early gates or 006B's later helper results.

## Alternatives

The resident routine's incoming calls and input-word producers,
DS:1440 changes between setup and dispatch, record state and the
later helpers remain unread (Q-CONFIG-008). No physical input or
visible message is assigned to this route. Computed, aliased,
unrelocated or differently encoded calls remain outside the incoming
inventory; its negative sections do not exclude those routes.

## How to reproduce

Select MZ relocation words with raw segment 47A6, preceding direct
far-call offset 006B and opcode, yielding `0x0001EB15`.
Read the prologue and early gate from `0x0001E45F` through
`0x0001E49F`, the mode range and dispatch at
`0x0001E49F..0x0001E4B3`, the call block at
`0x0001EB11..0x0001EB1D`, and return at
`0x0001EB78..0x0001EB7B`. Decode exactly five target words at
the table above using resident file base `0x0001DE90`; verify that
the fifth points to the call block. Track the double-word push as
two argument words and compare FND-CONFIG-110. For overlay calls,
use FMT-EXE-005's decoded descriptor 208 and trampoline 006B,
checking the FND-CONFIG-101 positive control separately. Search
local near-call candidates only within
`0x00091890..0x00093088`.
