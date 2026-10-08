---
id: FND-EXE-155
title: Guarded byte prefix branch admits equality conditionally and preserves original bytes for later masks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1D22..0x004A1DE1
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1ECE..0x004A1EDB
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1F5B..0x004A1F71
tool: Ghidra 12.1.3 PUBLIC bounded guarded-byte equality prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 7 at `0x004A1D22`. It
zero-extends byte V from `0x01BA29E8`, then byte N from
`0x01BA29E0`, and saves V at current ESP plus `0x9F`.
It compares V with N unsigned at byte width. V below N takes
`0x004A1ECE`, reads full mask M from `0x0075B204` and
sets mask one without reading the optional guard.

For V at or above N it reads full G from `0x01BA29F4`.
G zero reaches the local mask-one clear. G nonzero compares the
same retained V and N again, setting mask one through the same
alternate path on equality and clearing it otherwise. All paths
read M on their respective set/clear routes and join at
`0x004A1D59`. Thus mask one is set exactly when V is below N,
or V equals N with full G nonzero. V greater than N clears it
even with G nonzero.

The join reads and zero-extends byte D from `0x01BA29E4`,
clears mask `0x10` and saves original D at ESP plus `0x9E`
before changing BL. It XORs that working byte with retained N
in CL and saved V, contributing (D XOR N XOR V) AND `0x10`.
Saved V zero sets mask `0x40`; nonzero clears it through
`0x004A1F67`. Both paths join at `0x004A1D8D`.

The next portion zero-extends saved V, retained CL as N and saved
D. It clears mask `0x80` and contributes V AND `0x80`.
It forms V XOR N and D XOR N, then XORs the latter full
zero-extended value with `0x80`. It ANDs the two expressions,
logically shifts right seven and tests AL. All operands remain
bounded to a byte, so the shifted result is zero or one. Nonzero
sets mask `0x800`; zero clears it through `0x004A1F5B`.
Equivalently that mask is set exactly when:

((V XOR N) AND (D XOR N XOR `0x80`) AND `0x80`) is nonzero.

Both paths join at `0x004A1DCF`, clear mask four, zero-extend
saved V as the lookup index and directly transfer to `0x004A0E64`.
They skip the fresh byte load at `0x004A0E5D`.
FND-EXE-132 grounds the common zero-extended word contribution
at `0x006F2B70` plus twice that index, full mask publication,
full selector clear, frame/register restoration and full published-mask
return. FND-EXE-133 records the shipped lookup values, with runtime
writers unresolved. Other M bits remain locally retained apart from
the stated replacements.

FND-EXE-131 grounds the enclosing 220-byte reservation. Saved V
has one writer before comparison, saved D has one writer after the
selected mask-one route, and neither slot is overwritten before its
later consumers. There are no calls or local ESP changes between
those stores and reads. The adjacent slots are consumed separately
as bytes, not as one word. Retained CL holding N is unchanged
until its last consumer; ECX is reused only afterward for the
outgoing mask. D is read on every path, and no N/D/V global
reload occurs after their initial byte reads. Separate read order
does not establish atomicity, native admission or storage lifetime.

## Interpretation

Selector 7 uses optional full-guard admission for byte equality,
while strictly below admission does not read the guard. Its later
high-mask expression flips bit seven of the D/N XOR, unlike the
unflipped expression in FND-EXE-151 and FND-EXE-152. Original
saved D survives modification of its working BL. This is a local
branch contract, not a complete helper reading, originating operation
or PATH outcome. Q-EXE-009 in FMT-EXE-006 remains open for
producers, native admission, lifetime, runtime lookup writers and
remaining branch/callee effects.

## Alternatives

- V zero and N 128 set mask one without G; V 128 and N zero
  clear it even with G nonzero. A signed comparison reverses admission.
- Equal V/N with G zero clear mask one; equal bytes with full
  G `0x00010000` set it. Narrowing G to a word changes that case.
- D zero, N zero, V 128 set mask `0x800`; changing only D
  to 128 clears it. D 128, N 128, V zero also set it;
  changing only D to zero clears it. Omitting the `0x80` XOR
  reverses these high-mask outcomes.
- D zero, N 16 and V zero contribute mask `0x10`, but saved
  D remains zero. Treating modified BL as original D would change
  the later high-mask inputs.
- Upper storage bytes do not participate in the byte comparisons,
  masks or lookup. The lookup consumes saved V, not a fresh global byte.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Recheck selector
7's four-byte slot at shipped offset `0x0032219C`; it selects
`0x004A1D22`. Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004A1D22` limit 50,
`0x004A1ECE` limit four and `0x004A1F5B` limit seven.
Exclude instructions at or beyond the declared exclusive ends.
Use FND-EXE-132 for the shared lookup/publication return. Track
V-before-N order, saved V before comparison, unsigned comparison
direction, optional full guard and retained equality operands, each
mask-one route, saved D before BL modification, unchanged CL and
ESP, independent byte-slot consumers, bounded shifted AND with its
extra bit-seven XOR and direct lookup transfer. Check the Alternatives
as local arithmetic controls, not admitted native inputs. No native or
emulated execution is part of this finding.
