---
id: FND-CONFIG-035
title: Fifty-six overlay call sites target the message-delay routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Python 3.14.7 bounded FBOV fixup-table inspection; Capstone 5.0.7 16-bit disassembly of selected call windows
environment: null
---

## Observation

Overlay 172's resident entry `566A:002A` leads to the conditional
message-delay wait routine (FND-CONFIG-011, FND-CONFIG-018). The shipped
`DSUN.EXE` has 80 fixup words referring to FBOV descriptor 172. Fifty-six
of them are the segment word of a five-byte direct far call (`0x9A`) whose
offset word is `0x002A`. They occur in 21 other overlays:

| Caller overlay | Calls | Call instruction file offsets in `DSUN.EXE` |
|---:|---:|---|
| 171 | 3 | `0x058369`, `0x058465`, `0x0589A1` |
| 173 | 3 | `0x05D041`, `0x05D36E`, `0x05E4CB` |
| 175 | 2 | `0x060959`, `0x060EF3` |
| 176 | 2 | `0x0617BE`, `0x06182A` |
| 179 | 4 | `0x065A7D`, `0x065BFE`, `0x066882`, `0x0669D1` |
| 180 | 1 | `0x067C96` |
| 182 | 2 | `0x0695DD`, `0x06962A` |
| 187 | 4 | `0x070C5A`, `0x070C70`, `0x0726AC`, `0x0726D2` |
| 188 | 1 | `0x0747B4` |
| 189 | 4 | `0x0751B1`, `0x076B14`, `0x0777DB`, `0x0777E7` |
| 190 | 11 | `0x078C48`, `0x079194`, `0x079827`, `0x079864`, `0x079894`, `0x0798AE`, `0x0798D3`, `0x079A4B`, `0x079AE7`, `0x079C00`, `0x07A1DB` |
| 191 | 4 | `0x07C7C0`, `0x07CBAB`, `0x07CF30`, `0x07D0CC` |
| 192 | 1 | `0x07D8E2` |
| 195 | 4 | `0x08194A`, `0x081AC8`, `0x081C54`, `0x082509` |
| 197 | 3 | `0x08601E`, `0x086734`, `0x086F56` |
| 204 | 2 | `0x08CAA2`, `0x08CABB` |
| 207 | 1 | `0x090F28` |
| 209 | 1 | `0x0943FA` |
| 210 | 1 | `0x095A6B` |
| 211 | 1 | `0x096790` |
| 213 | 1 | `0x099B72` |

Bounded disassembly at `0x058465`, `0x079194` and `0x07D8E2` confirms a
far call with the `0x0560` encoded segment and a far-pointer argument, then
four bytes of argument cleanup. FND-CONFIG-017 separately identifies four
resident calls to the same entry.

## Interpretation

The entry is a shared message path across many overlays. The 56 locations
are syntactic direct call sites, not 56 observed messages or waits. Their
control-flow conditions, arguments and success through the later
`WIND/10501` gate have not been established by this inventory.

## Alternatives

Indirect calls or calls from code outside these overlay fixup lists may add
sites. Some listed instructions may sit on paths that the shipped game never
reaches. The fixup list alone does not decide that, nor whether the resource
reader and registration succeed at each call (FND-CONFIG-034).

## How to reproduce

For the approved `DSUN.EXE`, read the FBOV segment table as FMT-EXE-002
defines it, and locate each code block through FMT-EXE-003. For each
overlay code block, inspect only its declared fixup
word offsets. Retain words whose encoded descriptor index (`word >> 3`) is
172, then retain those preceded by far-call opcode `0x9A` and offset word
`0x002A`. Count and group the resulting physical instruction offsets by
caller descriptor. Disassemble the three sample windows above in 16-bit
mode to confirm the call and argument shape. This method does not search
indirect calls.
