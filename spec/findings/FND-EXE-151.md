---
id: FND-EXE-151
title: Guarded byte prefix branch saves original inputs before its working-byte increment and register reuse
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1437..0x004A14E5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1EA9..0x004A1EC1
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2240..0x004A2250
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A20C2..0x004A20D8
tool: Ghidra 12.1.3 PUBLIC bounded guarded-byte prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 10 at `0x004A1437`. The branch
zero-extends byte V from `0x01BA29E8`, then byte N from
`0x01BA29E0`. It saves V at current ESP plus `0x6F` before
comparing N with V unsigned at byte width. N below V takes
`0x004A1EA9`, reads and zero-extends byte D from `0x01BA29E4`,
saves D at ESP plus `0x6E`, reads full mask M at `0x0075B204`
and sets mask one. That path does not read the optional guard.

For N at or above V it reads full G at `0x01BA29F4`. G zero
takes `0x004A2240`, reads D into ECX, saves CL into the D slot
and reaches the mask-one clear at `0x004A1472`. This overwrites
the initial V register, but not saved V. G nonzero instead reads D into
EAX at `0x004A145F`, saves AL into the D slot and only then
increments AL at byte width. The following zero test branches to the
mask-one set at `0x004A1EB4` exactly when original D was 255;
other D values reach the clear. The increment changes only a working
register byte, not saved D or its global storage.

All paths read M through their selected set/clear route and join at
`0x004A147A`. Mask one is set exactly when unsigned N is below
V, or N is at or above V with full G nonzero and original D equal
to 255. D is read and saved on every path, including those where it
does not decide that first mask. The shared join does not use the
incremented working D.

The join clears mask `0x10`, zero-extends saved D, XORs its byte
with retained DL holding N and then with saved V, and ORs only bit
four of that XOR into the mask. Thus the contribution is:

(D XOR N XOR V) AND `0x10`, using original byte inputs.

It tests saved V for zero, setting mask `0x40` for zero or clearing
it through `0x004A20C2` otherwise. Both join at `0x004A149F`,
reload saved V and D as zero-extended bytes and zero-extend retained
DL as N. It clears mask `0x80` and contributes V AND `0x80`.
It then computes D XOR N and N XOR V, ANDs them and logically
shifts right seven. All operands were zero-extended bytes, so that
shifted result is zero or one; its AL test is sufficient. Nonzero
sets mask `0x800`; zero clears it through `0x004A20CC`.
The local condition is:

((D XOR N) AND (N XOR V) AND `0x80`) is nonzero.

Both paths join at `0x004A14D6`, clear mask four, zero-extend
saved V as the lookup index and directly transfer to `0x004A0E64`.
They skip the fresh byte load at `0x004A0E5D` described in
FND-EXE-132. FND-EXE-131 grounds the enclosing 220-byte reservation.
V's slot is written once before admission, D's slot has an explicit
original-byte writer on each admission path, and no later local store
changes either slot before their reads. There are no calls or local
ESP changes between these stores and consumers. The adjacent slots
are consumed separately at byte width; no combined word read occurs.

FND-EXE-132 grounds the common zero-extended word contribution at
`0x006F2B70` plus twice saved V, full mask publication, subsequent
full selector clear, register/frame restoration and full published-mask
return. FND-EXE-133 records all shipped lookup values, with runtime
writers unresolved. Other M bits remain locally retained apart from the
stated replacements. V, N, conditional G and path-selected D are read
separately; saving them does not establish atomicity, native producer
admission or global storage lifetime.

## Interpretation

Selector 10 uses a guarded byte comparison and increment-to-zero test
while preserving the original byte inputs for its later masks and lookup.
Register reuse on the guard-zero path does not change saved V, and
the working-byte increment does not change saved D. Unlike
FND-EXE-148 and FND-EXE-150, the later mask expression does not
freshly read n/d/v from the globals. These are local branch contracts,
not an originating operation, complete helper reading or PATH outcome.
Q-EXE-009 in FMT-EXE-006 remains open for producers, native admission,
guard/field lifetime, runtime lookup writers and remaining branch/callee effects.

## Alternatives

- N zero and V 128 take the below-V set path without reading G;
  N 128 and V zero do not. A signed byte comparison would reverse it.
- With N equal to V and D 255, G zero clears mask one and any
  full nonzero G sets it. G `0x00010000` is nonzero; narrowing
  the guard read to a word would change that case.
- On the nonzero-G path, D zero becomes working byte one, D 254
  becomes 255 and D 255 becomes zero. Only the last sets mask one.
  A widened increment or nonzero-D test would not give this boundary.
- D 255 is still 255 in its saved slot after the working increment.
  For N and V zero, original D contributes mask `0x10`; using
  the incremented zero would incorrectly drop that contribution.
- Guard-zero admission overwrites ECX with D. Later V tests and lookup
  use the saved V slot, not that register's new CL value.
- D 128, N zero, V 128 set mask `0x800`; only changing D to
  zero clears it. D zero, N 128, V zero also set it; only changing
  D to 128 clears it. A single sign test or N/V XOR is insufficient.
- Upper storage bytes do not participate in N/D/V comparisons, masks
  or lookup. A later global V byte cannot replace saved V on this path.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Selector 10's
four-byte slot starts at shipped offset `0x003221A8` and selects
`0x004A1437`; recheck that exact slot. Use the saved Ghidra program
with -noanalysis and ReportInstructionWindow.java at `0x004A1437`
limit 38, `0x004A14C7` limit seven, `0x004A1EA9` limit six,
`0x004A2240` limit three, `0x004A20C2` limit three,
`0x004A14DB` limit two and `0x004A20CC` limit three.
Exclude unrelated instructions at or after the declared exclusive ends.
Use FND-EXE-132 for the shared lookup/publication return. Track V-before-N
read order, saved V before the comparison, optional full guard width,
each saved-D writer before any working increment, byte wrap and zero flags,
retained DL through every path, original-byte XOR inputs, both adjacent
slots' independent consumers and unchanged ESP, the bounded shifted AND
result and fresh-tail-read bypass. Check the Alternatives cases as local
arithmetic controls, not native inputs. No native or emulated execution
is part of this finding.
