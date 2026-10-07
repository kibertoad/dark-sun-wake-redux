---
id: FND-EXE-126
title: Physical full-width fallback slots select a four-call method that retains upper return bits
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    offset: 0x00355830..0x00355833
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    offset: 0x00355EE8..0x00355EEB
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    offset: 0x00354F80..0x00354F83
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00417970..0x004179E2
tool: Verified physical PE word queries and Ghidra 12.1.3 PUBLIC bounded full-width target reading
environment: null
---

## Observation

FND-EXE-098 records first-word publication of three table addresses, with
separate owning objects and admission conditions. FND-EXE-099 grounds six
physical byte/word slots. FND-EXE-125 records the full-width mapped reader's
virtual offset sixteen. Reading exactly those full-width slots from the
same identity-verified shipped PE gives:

| Selected table | Full-width slot | Shipped offset | Target |
| --- | --- | --- | --- |
| `0x00758620` | `0x00758630` | `0x00355830` | `0x00689860` |
| `0x00758CD8` | `0x00758CE8` | `0x00355EE8` | `0x00417970` |
| `0x00757D70` | `0x00757D80` | `0x00354F80` | `0x00417970` |

These are file-backed little-endian words, not analyzer names. The shared
target in the last two rows does not equate their owners or remaining
slots. FND-EXE-098's `0x00758CD8` publication is to the separate
object at `0x01B7BB18`; its other two publications concern the fixed
fallback object on their respective routes. The physical target
`0x00689860` requires its own larger branch/callee reading; only its
identity is established here.

The shared target `0x00417970` reserves twenty-eight stack bytes and
saves EBX, ESI and EDI in that reservation. It reads full object input O
at current ESP plus thirty-two and full address input A at plus thirty-six.
It reads O's current first-word table, then calls its offset-eight target
with O and A in two outgoing slots. After an ordinary return it retains
the full EAX result R0, freshly reads O's first-word table, and calls
that table's offset-eight target with O and wrapped A plus one. It freshly
reads the table again before calling its offset-eight target with O and
wrapped A plus two, then again before calling with O and wrapped A plus
three. All four calls use retained O; there is no local object/target-null
guard or failure test on their return values.

The method combines full returns as R0 OR (R1 shifted eight) OR
(R2 shifted sixteen) OR (R3 shifted twenty-four), each shift and OR at
thirty-two-bit width. It performs no AL mask or zero-extension between
calls. Thus upper bits of the first three returned values can contribute
beyond the corresponding byte position. For example, conditional returns
R0 equal to `0x00000100` and the remaining three zero produce
`0x00000100`, not zero. This is a local arithmetic distinction, not a
claim that those values are returned by the actual methods. It restores
its saved registers and reservation and returns the combined full EAX.
There is no normalized success value or early zero-byte termination.

By contrast FND-EXE-125's boundary assembly retains only AL for each
indirect byte read before combining. FND-EXE-099 already records that the
shared table's byte target `0x00417900` reaches a shared transfer call
without a proved normally returning source-byte continuation. The four-call
method's ordinary-return arithmetic cannot establish that any of those
calls actually return, leave O unchanged, or preserve its backing storage.
Its fresh table loads allow a returning byte method to affect subsequent
target selection; they are not four calls to one retained target.

## Interpretation

The concrete full-width table slots are now identified. The shared smaller
method and the boundary helper do not have interchangeable return-width
contracts: full values are combined in one, truncated bytes in the other.
Q-EXE-009 in FMT-EXE-006 remains open for the larger full-width target,
byte-method/shared-transfer effects, table and object lifetime, indirect
writers, caller admission and remaining selected-callee branches. These
facts do not establish complete mapped-memory or actual PATH behavior.

## Alternatives

- The two shared full-width slots do not imply that their entire tables
  or owning objects are identical.
- The method does not truncate each byte-method return locally; treating
  it as four byte values needs a separately proved callee contract.
- The same retained object is used, but its method table is reloaded
  before every call. Initial target selection does not fix later targets.
- A complete local combination on ordinary return is not evidence that
  the shared byte transfer returns normally or that all inputs are valid.

## How to reproduce

Verify FND-EXE-011's length and XXH3-128 identity. Use the PE image base
and file-backed section mapping to read four contiguous bytes at each
of the three stated full-width slots, decoding little-endian. Recheck all
six independently recorded byte/word slots in FND-EXE-099 as physical
mapping controls, including both shared-target tables. Reject an identity
change, absent raw mapping or a noncontiguous four-byte span. Keep raw
queries local; the three result-driving slots and offsets are listed above.

Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x00417970` limit 75, restricting
claims through `0x004179E2` and excluding following write methods.
Track the twenty-eight-byte frame, each outgoing slot's last writer,
wrapped address increments, each fresh first-word table, full result shifts
and register restoration. Check the stated upper-bit arithmetic control;
it is not an original run. A window at the other exact physical target
`0x00689860` limit 65 supplies further branch leads, not a complete
contract. No native or emulated execution is part of this observation.
