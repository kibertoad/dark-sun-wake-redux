---
id: FND-EXE-178
title: Cleanup callback defaults name a far-return stub but later explicit writers supply other offsets
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004AF62..0x0004AF66
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B008..0x0004B00A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:1257..4AE5:1258
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:09B9..4AE5:09BF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0BBA..4AE5:0BC0
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

At the initial state base resolved in FND-EXE-176, the two words at offsets
`0x0082` and `0x0084` both hold `0x1257`. The gate word at offset `0x0128`
initially holds zero. Under the cleanup candidate's CS binding, the two
default offsets name `4AE5:1257`. A bounded original-source decode at that
address consists of one far return, with no additional argument cleanup,
calls, hardware boundaries or other reached instructions.

The saved analyzer listing has no instruction at that stub address; its
next defined instruction is `4F49:0AA8`. That gap does not invalidate the
independent source decode or establish that the stub is reached natively.

Two decoded explicit stores give concrete competing callback values:
`4AE5:09B9` stores `0x0EA2` into the DS-relative word at offset `0x0084`;
`4AE5:0BBA` stores `0x1155` into the DS-relative word at offset `0x0082`.
Their analyzer-associated entries are `4AE5:08EB` and `4AE5:0AB5`.
Each prefix saves DS and reads DS from CS-relative offset five. The entire
paths from those segment loads to these stores, including every intervening
callee and branch, are not read here. This therefore records offset writers,
not a proof that both stores access the same live state as the cleanup calls.

## Interpretation

Shipped callback defaults cannot settle FND-EXE-177's live computed targets.
The initial stub gives a bounded default behavior only. Q-EXE-001 and
Q-EXE-010 must follow the two concrete writer paths, their segment preservation,
the bodies named by the replacement offsets, other writers and native callers.
The zero initial gate likewise says nothing about its later value. No complete
reading, caller completeness or inventory replacement follows.

## Alternatives

Treating the default words as unconditional cleanup targets is unsupported
while the explicit later writer leads remain unresolved. Equating DS-relative
offsets solely because the numbers match is also unsupported. Conversely,
an undisassembled analyzer gap is not proof that the original target is data.

## How to reproduce

Use installed DSUN.EXE with XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Run the committed wrapper's `table`
command with sourceKind `mz`, start `0x0004AEE0`, count one, stride
`0x012A`, limit one and countEvidence naming one candidate state header with
only explicitly accessed fields. Read callbackWord82 at offset `0x82`, width
two; callbackWord84 at offset `0x84`, width two; gateWord128 at offset
`0x128`, width two. Preserve initial/raw values separately from live state.

Run `x86-bounds` against that original with entry `0x000412A7` (266919),
one region start `0x000412A7`, exclusive end `0x000412AF` (266927), segment
`0x4AE5`, ip `0x1257`, entries `[266919]`. Supply no callee models or seeds.
The reached instruction ends at `0x000412A8`; the rest of the query region
is not part of the observed stub.

In the saved original resident project, with analysis disabled and read-only
mode, request ReportInstructionWindow at `4AE5:1257`, count ten, preserving
its missing-start diagnostic. Run ReportInstructionContext at `4AE5:09B9`
and `4AE5:0BBA`, and ReportInstructionWindow at `4AE5:08EB` and `4AE5:0AB5`,
count 18 each. Restrict the store claim to the two cited instructions and
the segment-load observation to the prefixes, not complete writer paths.
No empty reference result or text search is used as absence evidence.
Rich output and configurations remain outside Git in the licensed local store.
