---
id: FND-EXE-163
title: First diagnostic transfer separates mutable candidate traversal from saved-state admission
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600B50..0x00600B89
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600B90..0x00600CB6
tool: Ghidra 12.1.3 PUBLIC bounded first-transfer reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-025 and FND-EXE-099 ground the diagnostic chain's call to
`0x00600B50` with its object pointer minus 32. FND-EXE-087 grounds
another caller that forwards this helper's raw return. The entry saves
EBP, EDI, ESI and EBX and reserves twelve local bytes. It retains its
full first incoming argument from EBP plus eight in EDI as P, and
reads full shared pointer `0x0242F640` as S. There is no initial
P-null guard. Zero S calls `0x006005B0`, reloads S and reads its
full offset-48 word without another null test. An initially negative
signed offset-48 word, or a negative word after initialization, calls
`0x00600860` and reloads S. FND-EXE-043 and FND-EXE-200 ground
those helpers' separate boundaries.

At `0x00600B74` it freshly reads S offset 48. Zero selects the
full word at S offset 40 as candidate C. Any nonzero value selects
S offset 44, retains it across `0x00602490`, supplies it to
`0x00602580`, retains that return across a call to `0x00602440`
with the first call's return, and selects the retained second result
as C. The initial signed test does not prove the later mode read is
nonnegative. This caller alone does not establish the three callees'
stack, register or external contracts.

Both paths write C to locals at EBP minus 16 and minus 20. EBX
holds the address of local minus 20. Each iteration sets full ECX
zero and full EDX five. Nonzero current C reads its full offset-24
word into ECX and clears DL, making full EDX zero. Zero C therefore
returns full five without reading that field. Nonzero C with zero
field takes the traversal edge at `0x00600B90`: freshly reload local
minus 20, dereference its first full word, store that as the new
local candidate and repeat. There is no cycle or iteration bound.

A nonzero offset-24 target is called indirectly through ECX. The
caller pushes eight full words, which at the call are ordered:
one, one, freshly read P offset zero, freshly read P offset four,
P, the address of local minus 20, C, C. The first two pushed C
copies are retained pre-call values; passing the local address admits
changes to the candidate slot. No local P-null test precedes its
field reads. After an ordinary return the caller removes 32 bytes
and compares full EAX with six, then eight. Six takes the selected
continuation below. Eight takes the fresh-local traversal edge.
Every other full value returns full three through local frame and
register restoration. The separate nonzero-EDX gate also returns
three, but no local path in this iteration constructs an EDX value
other than zero or five before that gate.

On six, it freshly reads local minus 20, writes full zero to P
offset twelve and that fresh candidate to P offset sixteen. It
replaces local minus 20 with local minus 16, then calls
`0x00600AD0` with P in EAX and the local-minus-20 address in
EDX. It tests full returned EAX against seven. Any other value
jumps directly to frame restoration at `0x00600BD8`, retaining
that callee's full return rather than replacing it with three.
The callee can alter the addressed local; its effects remain open.

Seven freshly reads S and local minus 20, retaining the latter in
EBX. Zero S initializes and reloads it before a signed offset-48
test. Negative mode calls `0x00600860` and reloads S. A later
fresh offset-48 read decides publication: zero writes retained EBX
to S offset 40; nonzero calls `0x00602590` after pushing ESI
twice, retained EBX, and a fresh full S-offset-44 word. After the
call it pops two full words, tests full EAX and, only for zero,
calls `0x00602490`. Both routes continue into the same transfer.
The two pops do not establish the preceding callee's cleanup width;
any remaining outgoing space is superseded by the later ESP load.

The transfer freshly reloads local minus 20 into EAX, forms that
pointer plus 32 in EDX, reads a full target from EDX plus four,
then replaces EBP from the candidate's offset 32 and ESP from
EDX plus eight. It jumps to the retained target in ECX, with no
local null guard, target-status test or ordinary return-frame
restoration. The target and both restored state fields are separate
reads. Replacing EBP does not change the retained EDX used for
the ESP load. Saved-state producers, native admission and target
continuation remain unresolved.

EBP-derived local and incoming-slot addresses remain based on the
entry's conventional frame until the explicit final EBP load.
Outgoing pushes do not change those local addresses. Normal callees
and preserved registers are conditional, not proved by the local
frame alone. Fresh-local loop reads and post-callee reloads must not
be replaced by retained C or the originally selected S. No complete
caller or runtime writer enumeration is claimed.

## Interpretation

This narrows the first transfer dependency in the composed diagnostic
route of FND-EXE-162, FND-EXE-099 and FND-EXE-025. The helper can
locally return five, three or a selected callee's non-seven full
result, or take a distinct saved-state transfer after seven. Those
paths do not establish that the diagnostic route terminates, returns
normally or reaches its later finalizer. FND-EXE-041 grounds that
finalizer's separate mutable-target and abort-import boundary.
Q-EXE-009 remains open for callback/selected-helper effects, caller
invariants, state producers and exceptional/cleanup contracts.

## Alternatives

- Callback return `0x00000106` is not full six and returns three;
  `0x00000108` is not full eight and does not repeat traversal.
- Selected helper return `0x00000107` is not full seven and is
  returned unchanged, rather than triggering the state transfer.
- A callback replacing local minus 20 and returning eight makes the
  traversal read the replacement's first word, not retained C's.
- A callback replacing that local and returning six publishes the
  replacement to P offset sixteen before resetting the local from
  the separate minus-16 slot. Those slots need not remain equal.
- A fresh nonzero mode after a negative-mode helper takes the nonzero
  publication route; the earlier signed test does not make it zero.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-025's caller linkage. In the saved
Ghidra program with -noanalysis, read ReportInstructionWindow.java
at `0x00600B50` limit 70, `0x00600BED` limit 45 and
`0x00600C74` limit 40, excluding instructions at or beyond
`0x00600CB6`. Preserve the gap `0x00600B89..0x00600B90`.
Track fresh signed/mode reads, candidate source selection, both
local copies, zero-candidate admission, indirect target and all eight
outgoing word sources, full six/eight/seven tests, loop-local reloads,
ordered P writes and local reset, fresh publication mode and saved
state loads. Keep helper cleanup and preserved-register assumptions
conditional. Check the Alternatives as local width/dataflow controls,
not admitted native states. No native or emulated execution is part
of this finding.
