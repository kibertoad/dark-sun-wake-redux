---
id: FND-CONFIG-091
title: Two guarded overlay 178 calls feed overlay 175's no-effect and money-message branches
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5689:0057
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5689:0043
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5689:003E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5689:0052
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Python 3.14.7 FBOV trampoline and fixup inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Overlay 175's entry `5689:0057` targets code at file offset
`0x0005FC0C`. Its two declared direct far calls from another overlay are
both in overlay 178:

| Call file offset | Bounded caller guard |
|---|---|
| `0x00063415` | The OR of local bytes `[BP-3]` and `[BP-4]` equals `0x40`. The caller passes `SI`, then clears `DS:0DA4`. |
| `0x00063CD9` | A selected 15-byte record has bit `0x08` of byte `+0x0B` set. The caller passes `SI` and then rejoins its event handler. |

Within the `5689:0057` entry, the near call at `0x000601BE` passes a local
word to overlay 175's `5689:0043` helper, whose code starts at
`0x00060898`. The entry has earlier allocation, state and resource calls;
this bounded chain does not prove that either overlay 178 call always
reaches the helper.

The `0043` helper has two distinct paths to the shared message entry:

| Shared call file offset | Local path |
|---|---|
| `0x00060959` | With `DS:1A34` not `0xFFFF`, a selected table word not `0xFFFF`, the bit-`0x20` tests and the later `0028:0304` call returning zero, it passes the no-effect text `DS:05E3` (FND-CONFIG-052). This branch jumps to the helper's exit after the message. |
| `0x00060EF3` | With `DS:1A34` equal to `0xFFFF` and the selected table word not `0xFFFF`, the helper calls overlay 175's `5689:003E` routine at `0x0006097F`. Within that routine, a nonzero result from `00C8:288C` is required before it formats and passes a money-received line through the shared call (FND-CONFIG-052). |

The `5689:0043` and `5689:003E` trampolines have no declared direct far
call from another overlay in the inspected fixup inventory. The local
`0043` path is reached by the near call at `0x000601BE`, and the `003E`
path by the near call at `0x0006097F`. Overlay 175 also registers
`5689:0052` as a global callback (FND-CONFIG-086); its bounded event
dispatcher at `0x00060741..0x000607D8` does not directly call either
message helper.

## Interpretation

The two shared-message sites in overlay 175 have a traced incoming chain
from guarded overlay 178 callers through entry `0057`, plus separate local
conditions within the helpers. The no-effect and money-message branches
are distinct within `0043`. The registered `0052` callback alone does not
establish an input route to either message call.

## Alternatives

The overlay 178 caller's complete state, the intervening `0057` entry
gates and the results of the local helpers remain unread. Computed or
unrelocated calls to the exported entries are not excluded. A different
indirect callback or script path may reach the same helper; this finding
does not prove a visible message or a successful `WIND/10501` setup.

## How to reproduce

Resolve overlay 175's stubs `0057`, `0043`, `003E` and `0052` to file
offsets `0x0005FC0C`, `0x00060898`, `0x00060BAB` and `0x00060741`.
Select the declared FBOV fixups for direct calls to `5689:0057`, then
disassemble bounded windows around `0x00063415` and `0x00063CD9`.
Follow the near calls at `0x000601BE` and `0x0006097F`; inspect
`0x00060898..0x00060983` and `0x00060BAB..0x00060EFC` in windows no
larger than 512 bytes. Compare the two text arguments with FND-CONFIG-052.
