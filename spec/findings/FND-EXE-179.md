---
id: FND-EXE-179
title: Callback-writer callees restore DS locally but retain interrupt and computed-call dependencies
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0D8B..4AE5:0EA2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0ECD..4AE5:1155
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The two bounded writer paths in FND-EXE-178 have these direct calls:

| Writer entry | Call site | Callee |
|---|---|---|
| `4AE5:08EB` | `4AE5:091A` | `4AE5:0D8B` |
| `4AE5:08EB` | `4AE5:0972` | `4AE5:0E3D` |
| `4AE5:0AB5` | `4AE5:0AD5` | `4AE5:0ECD` |
| `4AE5:0AB5` | `4AE5:0B90` | `4AE5:107D` |

Independent bounded traversals of those four callees give these reached
instruction intervals and local far-return epilogues. Ends are exclusive.

| Entry | Reached offsets in segment 4AE5 | DS restoration | Return / additional cleanup |
|---|---|---|---|
| `0D8B` | `0D8B..0DAA`, `0DAB..0DE5`, `0DE6..0E3D` | `0E38` | `0E3C` / zero |
| `0E3D` | `0E3D..0E4F`, `0E50..0E5B`, `0E5C..0EA2` | `0E9D` | `0E9F` / six bytes |
| `0ECD` | `0ECD..107D` | `1078` | `107C` / zero |
| `107D` | `107D..10FB`, `10FC..1155` | `1150` | `1152` / eight bytes |

These are encoded DS-restoration operations, not proof that the saved stack
word survives every external effect. The traversals retain the following
interrupt sites, written as offset/vector in segment `4AE5`:

- `0D8B`: `0DBA/21`, `0DC4/21`, `0DD1/21`, `0DD8/21`, `0DE1/21`,
  `0DE9/21`, `0DF3/67`, `0E00/67`, `0E0F/67`, `0E1B/67`.
- `0E3D`: `0E77/67`, `0E88/67`, `0E94/67`.
- `0ECD`: `0F11/21`, `0F40/2F`, `0F49/2F`, `0FBE/15`.
- `107D`: none in the bounded body.

The first two bodies have no listed calls. The `0ECD` body has unresolved
computed far calls at `0F55`, `0F60`, `0F6D`, `0F7B`, `0FA5`, `0FAF`,
and a direct call at `1006` to `11A9`. The `107D` body has unresolved
computed far calls at `10CB` and `10D9`. All offsets in this paragraph
belong to segment `4AE5`. The bounded CFGs report no decoding gaps, while
explicitly assuming every interrupt and call returns.

## Interpretation

This makes the intervening dependencies in the callback-writer paths concrete.
Local epilogues alone cannot settle FND-EXE-178's live storage admission or
callback targets. Q-EXE-001 and Q-EXE-010 still require stack integrity,
interrupt contracts, computed target producers, the direct helper, admitted
inputs and native callers. In particular the eight computed far calls must
be followed through both pointer words and all possible producers. No
complete-reading declaration or caller-completeness claim follows from a
complete local CFG result.

## Alternatives

Unconditional whole-call DS preservation is unsupported while external effects
and saved-slot integrity are unread. Conversely, an interrupt or unresolved
callee does not prove that DS changes. Retain those competing possibilities
until their relevant contracts and state writers are established.

## How to reproduce

Use installed DSUN.EXE, sourceKind `mz`, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Run the committed wrapper's `x86-bounds`
command separately with each entry in the tables, including the two writers.
Convert an entry offset to shipped offset by adding `0x00040050`.
For every query use one resident region starting at `0x00040050` (262224),
exclusive end `0x000412B0` (266928), segment `0x4AE5` (19173), ip zero,
and entries containing only that query's entry. Supply no interrupt models,
callback summaries, seeds or indirect-target declarations. Preserve every
listed assumed continuation, hardware boundary and unresolved target.

In the saved original resident project, with analysis disabled and read-only
mode, use ReportInstructionContext at `4AE5:0E3C`, `4AE5:0E9F`,
`4AE5:107C` and `4AE5:1152` to inspect the epilogues. Restrict each observation
to its bounded callee intervals. Ghidra 12.1.3's register segment-move display
can reverse operands; do not substitute rendered operand direction for the
original-byte semantics. Reports and configurations remain outside Git.
