---
id: FND-EXE-121
title: Shared state helpers bound input to sixteen slots and publish byte and mask state in different orders
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F2060..0x004F20D0
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F20E0..0x004F2105
tool: Ghidra 12.1.3 PUBLIC bounded shared state-helper reading
environment: null
---

## Observation

FND-EXE-110, FND-EXE-114, FND-EXE-119 and FND-EXE-120 record
calls or tail transfers to `0x004F2060` and `0x004F20E0` with a
full object field as first input. Both helpers read the full input N at
entry ESP plus four and use unsigned comparisons. Values above fifteen,
including full values with the high sign bit set, return without local
stores or EAX normalization. Neither helper calls another function.

The first helper divides admitted inputs into zero through seven and eight
through fifteen. For zero through seven it compares byte
`0x01D291F0` plus eight times N against zero, then sets AL to one and
publishes byte one at `0x01D291F1` plus eight times N. The earlier
compare flags survive those moves. A nonzero tested byte returns without
changing either full mask word. Zero instead forms full one shifted left
N and ORs that mask into full `0x01D271C8`, then returns. EAX on these
paths has only AL explicitly set to one; its upper twenty-four bits are
not normalized.

For eight through fifteen it publishes the same per-input byte one, forms
full mask one shifted left N and ORs it into full `0x01D271C4`.
Only after those stores it tests byte `0x01D291F0` plus eight times N.
Nonzero returns. Zero then tests current byte `0x01D29200`; nonzero
returns, while zero additionally ORs the retained mask into full
`0x01D271C8`. All admitted high-input ordinary exits retain that full
one-bit mask in EAX. The publication to `0x01D271C4` is not cancelled
by either later byte gate, and the per-input byte is set even on gates that
suppress `0x01D271C8` publication. No byte-zero gate is tested before
those two high-input stores.

The second helper admits zero through fifteen as one range. It forms full
`0xFFFFFFFE` rotated left N, a mask clearing bit N and retaining the
other bits. It ANDs full `0x01D271C8` with that mask, then ANDs full
`0x01D271C4`, then publishes byte zero at `0x01D291F1` plus eight
times N. It does not test the other slot byte or `0x01D29200` before
clearing. On ordinary admitted return EAX retains the full clearing mask,
not a normalized success/count value. Storage lifetime, concurrent effects
and caller interpretation remain unresolved.

## Interpretation

These are concrete bounded publishers for the full word consumed by the
optional admission path in FND-EXE-104. Setting and clearing are not symmetric
publication sequences: low-input setting has a pre-byte-store gate, high-input
setting has post-store gates, while clearing writes both masks before the
slot byte. Q-EXE-009 in FMT-EXE-006 still requires slot/field producers,
caller input admission, optional mask dispatch and lifetime/native conditions.
The local helper bodies do not establish those complete contracts or actual
PATH behavior.

## Alternatives

- Unsigned bounds reject negative full inputs rather than accepting them as
  indices below fifteen.
- A gated set still publishes its per-input byte; high-input setting also
  publishes `0x01D271C4` before either later gate.
- Clearing does not consult the set gates and writes the byte last.
- Low-input set leaves EAX's upper bytes unchanged; admitted high-input set
  and clear return masks rather than one common success value.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
The state-call findings above independently supply both direct targets. Use
the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004F2060` limit 65, restricting
observations to the two declared ranges and excluding following admission
code. Track unsigned comparisons, AL-only versus full EAX writes, gate flags
across byte stores, per-input stride, full shifts/rotation and publication
order. FND-EXE-104 independently records consumption of `0x01D271C8`.
Keep reports in GAME_DIR/analysis/exe-batches; commit no original listings
or bytes and execute neither interpreter nor game.