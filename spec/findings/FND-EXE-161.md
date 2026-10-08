---
id: FND-EXE-161
title: Gated prefix writer restores saved words after two untested continuations
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F9770..0x004F9943
tool: Ghidra 12.1.3 PUBLIC bounded prefix writer reading under Temurin 25.0.4.1
environment: null
---

## Observation

The entry at `0x004F9770` reserves sixty bytes and saves EBX,
ESI and EDI at current ESP plus `0x30`, `0x34` and `0x38`.
It retains the full incoming word at current ESP plus `0x40` in EDI.
That slot is entry ESP plus four. Its first test examines mask `0x02`
of byte `0x0075B206`, independently of the prefix selector. A clear bit takes `0x004F978C`. A set bit writes retained
EDI and full one to the first two outgoing slots and calls
`0x00411910`. Only a nonzero returned AL takes `0x004F97F7`;
zero AL rejoins `0x004F978C`. The callee's other return bytes do
not decide this gate.

The admitted continuation reads full words from `0x01BA29E0`,
`0x01BA29E4`, `0x01BA29E8`, `0x01BA29F4`, `0x01BA29EC`
and `0x01BA29F0`, in that order. It saves them at current ESP
plus `0x10`, `0x14`, `0x18`, `0x24`, `0x1C` and `0x20`,
respectively. FND-EXE-131 through FND-EXE-160 ground local consumers
of the first four consecutive words; this does not identify their
originating operation. The saves precede the calls described below.

It retains full `0x0075B0D0` in EBX and replaces that storage
with exact target `0x004F96E0`. It reads full `0x01EDE478` as
H, forms eight times H with wrapping thirty-two-bit arithmetic,
increments H and publishes it back. At base `0x01EDE47C` plus
the wrapped displacement it writes the zero-extended word read
from `0x0075B102`, then the earlier full read from `0x0075B200`
at offset four. No local bound checks H or the computed range.
The first outgoing slot receives the zero-extended word and calls
`0x0040FF50`. After its normal return the code freshly zero-extends
word `0x0075B200`, writes that to the same outgoing slot and calls
`0x0040FF50` again. Neither return is locally tested.

After those normal returns it retains the word from `0x0075B1E8`
in SI and replaces that word with DI. It reads full `0x0075B240`
as K and forms wrapped `(K << 5) + 0x1000`, retaining only its
low sixteen bits before subtracting `0x10000000` at full width.
From that result T it writes word `0x0075B102` from T shifted
logically right sixteen, writes full `0x0075B114` from that shifted
value shifted left four, and writes full `0x0075B200` from
T AND `0xFFE0`. It writes the full reads from `0x0075B188`
and `0x0075B18C` to the first and second outgoing slots, then
calls `0x004119F0` and immediately `0x00401850`. The local
continuation tests neither returned value. Their side effects,
exceptional exits and stack/register contracts remain dependencies.

On reaching `0x004F98E7`, it restores word `0x0075B1E8` from
SI, reads byte `0x0075B1E0` zero-extended into EDX, and restores
the retained callback word at `0x0075B0D0` from EBX. It reloads
the saved full words and writes them back in the order:
`0x01BA29E4`, `0x01BA29E0`, `0x01BA29E8`, `0x01BA29EC`,
`0x01BA29F0`, `0x01BA29F4`. Between the second and third
stores it decrements the current full `0x01EDE478`, rather than
writing the old H. Before restoring the selector it copies the
zero-extended byte result from EDX to EAX. No later local instruction
replaces EAX before the shared register/frame restoration and RET
at `0x004F97D2..0x004F97E2`.

The fallback edge uses an indirect call through the full slot at
`0x01D5E400` plus four times retained EDI, then reaches that same
register/frame restoration. Unlike the admitted restoration suffix,
it does not locally replace the indirect callee's EAX with the byte
result. The indirect slot's producers and target effects remain open.
There is no local selector clear in either suffix. The writes near
`0x004F992C` restore the saved selector instead of inventing a new
selector from the callback's result.

There is no local ESP adjustment between the sixty-byte reservation
and shared release. Slot relations above describe that local frame;
ordinary callee returns that preserve it and preserved-register values
are conditional, not established here. Saved words have one local
full-width writer and no local overlapping outgoing-slot store.
Unresolved callees or aliases could modify the saved storage. The
restoration consumes what those slots then hold, not an atomic
snapshot guaranteed to survive every callee. An exceptional exit
has no local guarantee of reaching cleanup or restoration.

## Interpretation

This is a bounded save/restore writer route, not evidence that its
six restored words are newly produced operation inputs. Admission under
mask `0x02` and an AL-only helper result precede the saves. The local
restoration order, current-counter decrement and byte-derived return
are distinct from the indirect fallback's return forwarding. The
whole helper, its callers, callback, mutable table and external effects
are not established by this reading. Q-EXE-009 remains open.

## Alternatives

- A returned full value `0x00000100` has zero AL and takes the
  fallback; one has nonzero AL and admits the save sequence.
- Changing a prefix field after its save does not change the local
  saved-slot source used for restoration unless a callee or alias also
  changes that slot. Retained register values are likewise conditional
  on the callees' preserved-register contract.
- If H is `0xFFFFFFFF`, its increment wraps to zero and its indexed
  displacement is `0xFFFFFFF8`; no local rejection follows. This is
  arithmetic, not evidence of native admission or mapped storage.
- The suffix decrement uses the value present after the calls. A
  callee change to H is not overwritten by a stored original H.
- A byte result `0xFF` produces full EAX 255 in the restoration
  suffix, rather than minus one or an earlier callee's full result.

## How to reproduce

Use FND-EXE-011's executable identity and FND-EXE-099's six physical
mapping controls. In the saved Ghidra program with -noanalysis,
ReportReferences.java queries `0x01BA29EC`, `0x01BA29E0`,
`0x01BA29E4` and `0x01BA29E8` provide positive writer leads;
inspect operands rather than classifying access from reference types.
No absence or complete writer enumeration is claimed. Read
ReportInstructionWindow.java at `0x004F9770` with limit 140,
excluding instructions at or beyond `0x004F9943`. Its gap starts
at that exclusive end. Track the two admission paths, AL width,
six save sources and full slot widths, exact callback publication,
wrapped index and current-counter writes, fresh second word read,
word versus full publications, the two untested continuations,
restoration ordering and distinct EAX sources. Treat callee/frame
and alias survival as conditional. Check the Alternatives as local
arithmetic controls, not admitted native states. No native or
emulated execution is part of this finding.
