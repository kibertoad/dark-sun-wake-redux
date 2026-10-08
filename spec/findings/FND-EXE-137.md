---
id: FND-EXE-137
title: Signed word and full-width prefix shifts share a fresh-count mask gate and unconditional high-mask clear
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A0FBD..0x004A1064
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1F03..0x004A1F19
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1E14..0x004A1E1E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A20EF..0x004A20F7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A217B..0x004A2189
tool: Ghidra 12.1.3 PUBLIC bounded signed prefix shift branch reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 41 at `0x004A1025` and selector
42 at `0x004A0FBD`. Both zero-extend byte C from `0x01BA29E4`,
decrement it and arithmetically shift a thirty-two-bit value right by CL.
The effective count k is (C minus one) modulo 32. Selector 41 first
sign-extends word N at `0x01BA29E0`; selector 42 reads full N there.
Each tests the shifted result's low bit, then reads current full mask M
at `0x0075B204`, setting mask one for bit one or clearing it otherwise.
The alternate clear paths rejoin before V is read: selector 41 through
`0x004A1F0B` to `0x004A1047`, selector 42 through
`0x004A217B` to `0x004A0FDD`.

Selector 41 zero-extends word V at `0x01BA29E8` and tests AX;
selector 42 reads full V and tests the full value. Each sets mask `0x40`
for V zero or clears it for V nonzero. Selector 41's alternate at
`0x004A1F03` rejoins at `0x004A105A`; selector 42's alternate
at `0x004A20EF` rejoins at `0x004A0FED`. Selector 41 masks
zero-extended V with `0x8000` then arithmetically shifts right eight,
yielding zero or `0x80`. Selector 42 masks full V with `0x80000000`
then logically shifts right 24, also yielding zero or `0x80`.

Both join at `0x004A0FF5`. They AND the retained mask with
`0xFFFFF77B`, clearing masks `0x800`, `0x80` and four, then
OR in the stated V high-bit contribution. Thus mask `0x800` is
unconditionally cleared here; there is no N/V XOR or exact-N gate.
They freshly zero-extend a byte from V's storage and read the word at
`0x006F2B70` plus twice that byte, ORing its zero-extended value
into the mask. FND-EXE-133 grounds the physical shipped contributions;
runtime writers remain unresolved.

After the lookup they freshly test byte `0x01BA29E4` with
`0x1F`. Nonzero low five bits set mask `0x10`; zero low five
bits clear it through `0x004A1E14`. Both transfer to the publication
at `0x004A0E6E`. FND-EXE-132 grounds that full mask store,
subsequent full selector clear, saved-register restoration and full mask
return. The final count test uses a fresh original count byte, not the
retained decremented shift count. No calls occur in these bounded paths.
Other M bits remain retained locally apart from the stated replacements.
No atomicity, producer bounds or runtime lookup immutability follows.

## Interpretation

The signed word and full-width branches have distinct N/V widths, a shared
unconditional mask clear and a separate fresh-count gate. Q-EXE-009 in
FMT-EXE-006 remains open for count/field/mask/selector producers, runtime
writers and other branch/callee effects. These observations do not establish
a named originating operation, complete helper reading or actual PATH behavior.

## Alternatives

- Word N `0x8000` is sign-extended before the shift in selector 41.
  At k 16 through 31 its extracted bit is one; zero-extension would give zero.
- C zero gives k 31, C one gives k zero, C 32 gives k 31 and C 33
  gives k zero. No local count rejection precedes that extraction.
- If the fresh count equals the original C, C one and C 33 set mask
  `0x10`, whereas C zero and C 32 clear it. Testing decremented
  count instead would invert these example groups; native lifetime is unresolved.
- Word V `0x8000` contributes mask `0x80` in selector 41, while
  full V `0x00008000` does not in selector 42. Upper-only V can
  also differ at the zero gate. Neither branch derives mask `0x800`
  from sign disagreement or an exact N value.
- The final byte lookup and count gate are fresh reads, so retained V/C
  cannot silently replace them under an unproved lifetime contract.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-131's selector slots. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A1025` limit
24, `0x004A0FBD` limit 24, `0x004A1F03` limit eight,
`0x004A1E14` limit three, `0x004A20EF` limit two and
`0x004A217B` limit three. Exclude following unrelated instructions.
Use FND-EXE-132 for the shared publication and return and FND-EXE-133
for physical lookup values. Track signed word extension versus full reads,
masked decrement counts, arithmetic shift sign filling, independent zero
clears, the shared AND mask, high-bit contributions, fresh lookup/count
reads and publication order. Check Alternatives' count and width examples
as local arithmetic controls, not native observations. No native or emulated
execution is part of this finding.

The location ranges use exclusive ends, including the final transfers
already described above. Verify them with additional windows at
`0x004A1062`, `0x004A1F14`, `0x004A1E19`, `0x004A20F2`
and `0x004A2184`, each with limit two; the following instruction
starts are their declared exclusive ends.
