---
id: FND-EXE-110
title: Callback value seven publishes a flag before priority selection and tail-transfers only on a gated state change
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4A00..0x005A4AA8
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4AF7..0x005A4AFE
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4B27..0x005A4B33
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4B8D..0x005A4B97
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4966..0x005A496B
tool: Ghidra 12.1.3 PUBLIC bounded value-seven priority and state-change reading
environment: null
---

## Observation

FND-EXE-109 records the callback consumer's dispatch and saved-register
frame. Its word-value-seven path starts at `0x005A4A00` with retained
object O. It zero-extends byte O plus `0x0118`, ORs its low byte with
mask `0x10`, reads and zero-extends byte O plus `0x011C`, publishes
the modified byte to O plus `0x0118`, then computes full intersection
M of those two zero-extended values. The flag publication precedes all
priority decisions; the original flag is not restored on an ordinary exit.

It publishes byte O plus `0x011E` by the first matching condition below.
Tests are ordered and lower-priority bits do not override an earlier match.

| First matching condition in M | Published byte |
|---|---|
| mask `0x04` present | 6 |
| otherwise mask `0x10` present | 12 |
| otherwise mask `0x01` present | 4 |
| otherwise mask `0x02` present | 2 |
| otherwise mask `0x08` absent | 1 |
| otherwise mask `0x08` present | 0 |

The final two outcomes use a byte zero-test result, not the full M as a
selector. Bits outside these tested masks still affect the later M-nonzero
decision. After the selector publication, M nonzero reads byte O plus
`0x011D`. Already nonzero returns through the shared epilogue without
changing it. If zero, the path tests byte O plus `0x0123`, publishes one
to `0x011D`, and only when the tested `0x0123` byte was nonzero reads
full O plus `0x0110`, replaces the original first argument with that
value, restores its frame and tail-transfers to `0x004F2060`. The test
of `0x0123` precedes the `0x011D` write; its flags survive that write.
A zero gating byte still leaves `0x011D` set to one on normal return.

M zero instead checks the same `0x011D` byte. Already zero returns
through the shared epilogue. Otherwise it tests `0x0123`, publishes zero
to `0x011D`, and only when the gating byte was nonzero reads full
`0x0110`, replaces the original first argument, restores the frame and
tail-transfers to `0x004F20E0`. A zero gating byte does not cancel this
local clearing. Both transfers preserve the entry return address; neither
returns locally to another instruction in this consumer. Ordinary exits
restore the local stack and saved registers without normalizing a success
value. No callee result is tested after either tail transfer.

## Interpretation

This path publishes a flag and priority byte before conditional state
changes. The state byte can change even when the gate suppresses a transfer;
its change is not contingent on callee success. Q-EXE-009 in FMT-EXE-006
still requires field producers/meaning, object lifetime, dedicated zero/one
branches and both transfer targets' effects. The shared priority path can
also be reached from other branches, but this finding describes entry through
word seven only. No complete consumer reading or native PATH outcome follows.

## Alternatives

- Selector priority is ordered, not highest-bit or lowest-bit selection.
- Selector zero need not imply M zero: mask eight alone produces zero.
- A suppressed transfer does not suppress the local state-byte publication.
- Tail transfers have no local post-call success or restoration path.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
FND-EXE-109 independently supplies the dispatch target and entry frame.
Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x005A47C0` limit 110 and
`0x005A4970` limit 150. Restrict the value-seven observation to the
ranges declared above. Follow each priority branch to its shared decision,
including `0x005A4AF7`, `0x005A4B27` and `0x005A4B8D`, and the
ordinary epilogue at `0x005A4966`. Track byte/full widths, flag preservation
across stores, gate reads before state writes and original argument slots
after frame restoration. Keep all reports in GAME_DIR/analysis/exe-batches;
commit no original listings or bytes and execute neither interpreter nor game.