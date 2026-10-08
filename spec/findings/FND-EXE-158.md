---
id: FND-EXE-158
title: Strict byte prefix comparison preserves saved inputs and bounds partial-register mask work
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A15D4..0x004A1683
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1F71..0x004A1F88
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A21A6..0x004A21B2
tool: Ghidra 12.1.3 PUBLIC bounded strict-byte prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 1 at `0x004A15D4`. It
zero-extends byte V from `0x01BA29E8`, then byte N from
`0x01BA29E0`, and saves V at current ESP plus `0xCE`.
It compares V with N unsigned at byte width. V below N reads
full mask M from `0x0075B204` and sets mask one. V at or
above N takes `0x004A1F7B`, reads M and clears mask one.
Both paths join at `0x004A15F9`. Neither reads the optional guard.

The join zero-extends byte D from `0x01BA29E4`, copies the
selected mask into EDX and clears mask `0x10` there. It saves
D at ESP plus `0xCF`, then replaces AL in EAX with original
D and XORs AL with retained N in CL and saved V. Upper EAX
bits still come from the selected mask, but the following full
AND with `0x10` removes all except bit four. The contribution
ORed into EDX is therefore (D XOR N XOR V) AND `0x10`.
Saved V zero sets mask `0x40`; nonzero clears it through
`0x004A1F71`. Both paths join at `0x004A162F`.

The next portion zero-extends saved V, retained CL as N and saved
D. It clears mask `0x80` and contributes V AND `0x80`.
It computes V XOR N and D XOR N, XORs the latter with
`0x80`, ANDs both expressions and logically shifts right seven.
All operands are bounded to a byte, so the shifted result is zero
or one and its AL test is sufficient. Nonzero sets mask `0x800`;
zero clears it through `0x004A21A6`. Equivalently the condition is:

((V XOR N) AND (D XOR N XOR `0x80`) AND `0x80`) is nonzero.

Both paths join at `0x004A1671`, clear mask four, zero-extend
saved V as the lookup index and transfer directly to `0x004A0E64`,
bypassing the fresh byte load at `0x004A0E5D`.
FND-EXE-132 grounds the common zero-extended word contribution
at `0x006F2B70` plus twice that index, full mask publication,
full selector clear, frame/register restoration and full published-mask
return. FND-EXE-133 records shipped lookup values but leaves runtime
writers unresolved. Other M bits remain locally retained apart from
the stated replacements.

FND-EXE-131 grounds the enclosing 220-byte reservation and saved
register slots starting at ESP plus `0xD0`. This path's V/D slots
at `0xCE` and `0xCF` lie before those saves and are independently
consumed at byte width, never combined into a word. Each has one
original-byte writer and no later overwrite before its consumers.
No local call or ESP change intervenes. CL remains N until its
zero-extension for the high-mask expression; ECX is reused for the
outgoing mask only after its last N consumer. D is read on every
path, and no global N/D/V reload follows these initial byte reads.
Read order does not establish atomicity, native admission or lifetime.

## Interpretation

Selector 1 uses a strict byte comparison without optional equality
admission. It shares FND-EXE-155's saved-input, flipped high-mask
shape but does not import selector 7's guard. Its partial AL writes
must be distinguished from both the preceding full mask and the
later zero-extended byte operands. This is a local branch contract,
not a complete helper reading, originating operation or PATH outcome.
Q-EXE-009 in FMT-EXE-006 remains open for producers, native admission,
lifetime, runtime lookup writers and remaining branch/callee effects.

## Alternatives

- V zero and N 128 set mask one; reversing them clears it.
  Signed comparison reverses these cases. Equal bytes clear it
  regardless of any optional guard value, which this path never reads.
- D 16, N zero and V zero contribute mask `0x10` even when
  M's bit four was clear. All three zero clear that bit even when
  M's bit four was set. Upper M bits cannot leak through the AND.
- D zero, N zero, V 128 set mask `0x800`; changing only D
  to 128 clears it. D 128, N 128, V zero also set it;
  changing only D to zero clears it. Omitting the `0x80` XOR
  reverses these high-mask outcomes.
- Upper storage bytes do not participate in comparisons, masks or
  lookup. Adjacent saved D does not become the upper byte of the
  lookup index, which is a zero-extended saved V byte.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Recheck selector
1's four-byte slot at shipped offset `0x00322184`; it selects
`0x004A15D4`. Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004A15D4` limit 50,
`0x004A1F71` limit seven and `0x004A21A6` limit four.
Exclude instructions at or beyond the declared exclusive ends.
Use FND-EXE-132 for the common lookup/publication return. Track
V-before-N order, saved V before unsigned comparison, both mask-one
paths, D's saved-byte writer, AL-only replacement and the following
full AND, retained CL and later zero-extension, both byte-slot consumers,
their separation from register saves, unchanged ESP, bounded shifted
AND with its extra bit-seven XOR and direct lookup transfer.
Check the Alternatives as local arithmetic controls, not admitted
native inputs. No native or emulated execution is part of this finding.
