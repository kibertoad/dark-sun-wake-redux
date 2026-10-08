---
id: FND-EXE-134
title: Two prefix branches replace mask bits from distinct word and shifted full-width input contracts
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A0F01..0x004A0FBD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A20F7..0x004A2115
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1E54..0x004A1E69
tool: Ghidra 12.1.3 PUBLIC bounded prefix word and byte-count/full-width branch reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 56's physical target `0x004A0F01`
and selector 61's target `0x004A0F60`. Both finish at the shared
low-byte lookup/publication return described in FND-EXE-132. FND-EXE-133
records that lookup's complete shipped contribution values. These two
branches have different consumed input widths and do not share one
unstated arithmetic-instruction contract.

Selector 56 zero-extends the word at `0x01BA29E0` as N and tests
its sixteen-bit width for zero. It reads current full `0x0075B204`
as M, setting mask one for N nonzero or clearing it for N zero. It then
zero-extends the word at `0x01BA29E8` as V. The branch explicitly
replaces the following mask bits, retaining other M bits locally:

| Mask | Set when | Cleared when |
| --- | --- | --- |
| `0x00000001` | N nonzero | N zero |
| `0x00000010` | V's low four bits nonzero | V's low four bits zero |
| `0x00000040` | V zero | V nonzero |
| `0x00000080` | V's bit fifteen set | V's bit fifteen clear |
| `0x00000800` | N exactly `0x8000` | N not `0x8000` |

The bit-fifteen contribution masks zero-extended full V with
`0x8000` then arithmetically shifts right eight. Its result is zero
or `0x80`; it is not an upper-byte sign extension. The exact-N test
uses CX, so upper bytes in the original full storage are ignored. Its
alternate zero/nonzero paths rejoin before the next independent test:
a zero low nibble does not bypass the V-zero test, and a nonzero V does
not bypass the high-bit and exact-N tests.

Selector 61 first zero-extends byte `0x01BA29E4`, decrements that
full value and uses CL for a thirty-two-bit logical shift. Thus its effective
count k is (byte minus one) modulo 32. It reads full N from
`0x01BA29E0`, computes N shifted right k, and tests its low bit.
It then reads current full M, setting mask one when that extracted bit is
one or clearing it otherwise. It reads full V at `0x01BA29E8`,
not only the word or byte. Its locally replaced bits are:

| Mask | Set when | Cleared when |
| --- | --- | --- |
| `0x00000001` | bit k of full N set | bit k of full N clear |
| `0x00000040` | full V zero | full V nonzero |
| `0x00000080` | V's bit thirty-one set | V's bit thirty-one clear |
| `0x00000800` | full N XOR full V has bit thirty-one set | that bit clear |

The last gate is the sign-bit result of the full XOR, not equality of N
and V, arithmetic overflow of a proved operation, or a word-sized compare.
This branch does not independently replace mask `0x10`; other M
bits remain retained until the common lookup gate. Byte count zero becomes
k 31, count one becomes k zero, and count 33 also becomes k zero.
There is no local special early return for count zero or a count above 32.
Input/count producers and whether those controls occur remain unresolved.

Both branches then explicitly clear mask four from their computed mask
and freshly read byte `0x01BA29E8` for the common zero-extended
word lookup at `0x006F2B70` plus twice that byte. They OR the
lookup contribution into the mask, publish the resulting full word to
`0x0075B204`, clear full stored selector `0x01BA29EC`, restore
saved registers and return that full published mask. FND-EXE-132 grounds
that shared sequence; these branches do not apply its width-branch blanket
mask `0xFFFFF76A` on the way to the lookup. Their individual bit
replacements above must therefore be kept distinct from those earlier
branches' arithmetic.

No calls occur in these bounded paths. Their input fields are read at
separate points, and the final low-byte lookup is fresh rather than assumed
to equal retained V's low byte under native concurrency. Full mask publication
precedes selector clearing. A zero returned mask is not a distinct failure
result and does not preserve the stored selector on these paths.

## Interpretation

The physical dispatch branches now have explicit consumed widths, masked
shift-count behavior and independent per-bit replacement conditions. Their
shared mask return does not identify which original operation or producer
made the fields. Q-EXE-009 in FMT-EXE-006 remains open for field/mask/count
and selector producers, runtime table writers, other prefix branch effects,
publisher indirect targets and remaining selected-callee paths. This is
not a complete helper reading, named instruction model or actual PATH
behavior.

## Alternatives

- Selector 56 uses low words for N and V; selector 61 uses full N/V and
  a separately read byte count. Upper bytes matter only in the latter.
- The word branch's exact `0x8000` condition and the full branch's
  XOR sign-bit condition are different sources for mask `0x800`.
- Count zero is not rejected; the decremented shift count is masked by
  the thirty-two-bit instruction width. Counts one and 33 select the same bit.
- These paths clear only their stated bits before the lookup; importing
  the broader mask from FND-EXE-132 would drop retained bits without evidence.
- The final fresh byte lookup and selector clear do not turn the returned
  full mask into a success value or make the separate input reads atomic.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-131's exact selector slots. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A0F01`
limit 45, `0x004A20F7` limit twelve, `0x004A1E54` limit
seven and `0x004A0FAC` limit five. Restrict claims to the declared
branches and their alternate joins, excluding following unrelated paths.
Use FND-EXE-132 for the direct shared join at `0x004A0E5D` and
FND-EXE-133 for physical lookup contributions. Track zero-extension,
CX/AX/AL versus full-width tests, decremented CL's low-five-bit count,
full XOR sign flags, individual AND/OR mask writers, direct transfers,
fresh byte lookup and full publication before selector clear. Check word
zero/nonzero, low-nibble zero/nonzero, N `0x8000` versus upper-byte-only
changes, each high-bit combination for full N/V, and byte counts zero,
one, 32 and 33 as local arithmetic controls. These are not native inputs
or original execution. No native or emulated execution is part of this
observation.

The location ranges use exclusive ends, including the final transfers
already described above. To verify their boundaries, additionally read
`0x004A0FB8`, `0x004A2110` and `0x004A1E64`, each with limit
two; their following instruction starts are the declared exclusive ends.
