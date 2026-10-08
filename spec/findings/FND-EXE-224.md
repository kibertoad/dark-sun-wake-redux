---
id: FND-EXE-224
title: Four independently bounded descriptor-198 tables close a conditional outer procedure traversal
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087459..0x000874F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000874F5..0x0008753C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008753C..0x000875C2
tool: Ghidra 12.1.3 PUBLIC, executable-reader 2.5.0 and engine 13.6.0
environment: null
---

## Observation

The additional consumers in FND-EXE-223 have independently read bounds.
The selector-zero arm copies its input word to the index, decrements at
16-bit width, rejects an unsigned result above nineteen, doubles it and
jumps through the CS-relative word table at displacement `0x028A`.
The selector-one arm performs the same transformation with unsigned bound
seventeen and table displacement `0x0266`. FND-EXE-221 independently
bounds the selector-two arm at displacement `0x0242` and the masked arm
at displacement `0x022C`; no table borrows a neighboring table's bound.

Under the descriptor-relative mapping, source-derived declarations give:

| Consumer shipped offset | Table shipped interval | Slots | Distinct targets |
|---|---|---:|---:|
| `0x00087473` | `0x0008759A..0x000875C2` | 20 | 13 |
| `0x000874C1` | `0x00087576..0x0008759A` | 18 | 4 |
| `0x000874E2` | `0x00087552..0x00087576` | 18 | 4 |
| `0x00087515` | `0x0008753C..0x00087552` | 11 | 4 |

The source reader accepts all table words within the declared overlay
mapping. Starting at `0x00087459` with these four declarations reaches
89 instructions and 225 bytes in the two code intervals above. Every
discovered path closes at the far return at `0x0008753B`, exclusive end
`0x0008753C`, with zero additional argument cleanup. No calls, hardware
boundaries or CFG gaps are listed. Four explicit table-consumption
assumptions remain, each exhaustive under the supplied mapping. The
conditional CFG-complete field is true.

## Interpretation

This supplies a concrete conditional outer-body candidate for inventory
comparison, rather than only FND-EXE-222's internal tails. It resolves
FND-EXE-223's query omissions under the descriptor-base assumption, not
native CS or actual dispatch admission. No saved database or inventory has
been changed. Actual entry, incoming frame and input provenance, runtime
segment production and whole-function ownership remain Q-EXE-010.
The report's complete field is not a Standard complete_reading declaration.

## Alternatives

One shared count for adjacent tables is contradicted by the separately
read unsigned comparisons. An incomplete saved body is not the only
candidate: source traversal supplies a larger conditional body. Neither
candidate's existence alone establishes the runtime segment binding.

## How to reproduce

At revision `f27d070`, use FND-EXE-223's original-source x86-bounds config,
root, region, format controls, source hash and default limits. Retain its
eleven-slot declaration and add declarations for the other three consumer
sites and table starts/counts in the table above. Each declaration uses
exhaustive true conditional on the descriptor-base binding, stride two,
width two and fieldOffset zero. Its evidence names the independently read
selector gate, word decrement, unsigned bound and doubled index. Supply
no target-address list, register seeds or callee summaries.

In the corrected installed snapshot from FND-EXE-174, read-only with
automatic analysis disabled, use engine 13.6.0 ReportInstructionWindow at
`9211:0149` with count eight, `9211:0159` with count ten and
`9211:01A2` with count nine. Restrict the
new consumer readings to the prefixes ending at descriptor offsets
`0x0168` and `0x01B6`, respectively; later printed instructions are separate
context. The first window checks the selector-zero gate in the entry prefix.
Preserve assumedContinuations in the source report. All listings, configs
and results stay in GAME_DIR and are not committed.
