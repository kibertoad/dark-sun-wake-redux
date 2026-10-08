---
id: FND-EXE-132
title: Selected prefix width branches publish a full mask then clear selector while the zero-selector branch retains the mask
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A0E2E..0x004A0F01
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1803..0x004A180E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1E1E..0x004A1E38
tool: Ghidra 12.1.3 PUBLIC bounded selected-prefix width and shared return reading
environment: null
---

## Observation

FND-EXE-131 records the exact physical dispatch mapping. Selectors
6, 15, 21 and 33 select `0x004A0E2E`; 5, 14, 20 and 32 select
`0x004A0E9A`; 4, 13, 19 and 31 select `0x004A0EC4`.
These branches consume, respectively, the full word, zero-extended
sixteen-bit word and zero-extended byte at `0x01BA29E8` as V.
Each reads full mask M at `0x0075B204` and tests V for zero at
its consumed width. Upper input bytes do not participate in the narrower
zero tests or their bit contribution below.

For V zero each forms D as (M AND `0xFFFFFFEE`) OR `0x40`.
For V nonzero each forms D as M AND `0xFFFFFFAE`, using its
separate direct branch at `0x004A1E30`, `0x004A1E1E` or
`0x004A1E26`, respectively. It then masks D with
`0xFFFFF76A`. The full-width branch contributes V's bit thirty-one
shifted right twenty-four, the word branch contributes V's bit fifteen
shifted right eight, and the byte branch contributes V AND `0x80`.
Every contribution is therefore zero or `0x80`. The word's shift is
arithmetic at full width after zero-extension and mask `0x8000`;
it does not propagate a signed sixteen-bit value into the upper bytes.

The full and word branches then freshly read the byte at
`0x01BA29E8` as B and zero-extend the word at `0x006F2B70`
plus twice B as T. The byte branch instead retains its initially read
zero-extended byte as B and uses that same value for the lookup. Thus
all lookup indices are locally zero through 255, but the full/word lookup
is a separate byte read rather than reuse of their earlier wider V.
The table's backing extent, values, producers and semantic interpretation
remain open; no value is assumed from a generic bit/parity convention.

Each of the three branches ORs the bit contribution and T into masked
D and publishes the resulting full word R to `0x0075B204` at
`0x004A0E6E`. The local result is therefore:

R = (D AND `0xFFFFF76A`) OR width-specific contribution OR T.

It then restores saved EBX, forms full EAX from R, writes full zero to
stored selector `0x01BA29EC`, restores ESI and EDI, releases the
220-byte reservation and returns. Mask publication precedes selector
clearing; the full returned value is the published R, not a normalized
success result. No calls occur in these local width paths. Concurrent
state and alias/lifetime assumptions remain unresolved, and the distinct
reads are not silently treated as one atomic observation.

Selectors zero, 62 and 63 instead select `0x004A1803`, which reads
current full `0x0075B204` directly into the common return value and
jumps to `0x004A0E74`. This skips the mask store but still clears
full `0x01BA29EC` and returns the retained full mask through the
same restoration path. It reads neither V nor T locally. By contrast the
shared default in FND-EXE-131 returns full zero without clearing the
selector. Returning a zero mask on this admitted retain-mask path is
therefore not the same state transition as taking the default.

## Interpretation

These are concrete width, mask arithmetic and publication-order contracts
for three physical dispatch groups and the retain-mask group. Their shared
return consumes the stored selector locally even when the returned full
mask is zero. Q-EXE-009 in FMT-EXE-006 remains open for the lookup table,
mask/input/selector producers, other branch effects, remaining selected
callee paths, publisher indirect targets and storage lifetime. This is
not a complete helper reading or an established semantic flag model or
actual PATH behavior.

## Alternatives

- The word input is zero-extended before its arithmetic shift, so its
  bit-fifteen contribution is `0x80`, not sign-extended upper bits.
- Narrower branches ignore upper bytes in V's zero test; the wider
  branches separately reload the low byte for T rather than retain V's
  low byte as an unstated concurrency assumption.
- The retain-mask branch clears the selector but does not republish the
  mask; the default returns zero and leaves the selector stored.
- Width paths publish a full result before clearing the selector, and
  their lookup values cannot be inferred from a familiar flag convention.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-131's exact dispatch slots. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A0DE0`
limit 55 for the full/word bodies and common return, restricting those
claims to their declared branch range. Read `0x004A0EC4` limit
twelve, `0x004A0EE1` limit nine, `0x004A1E1E` limit seventeen
and `0x004A1803` limit fourteen, excluding unrelated following
branches. Track consumed widths, zero-extension, zero/nonzero gates,
masked contributions, separate low-byte lookup versus retained byte,
full store before selector clear and frame/register restoration. Check
zero, the highest bit of each consumed width and input bits above the
narrower width as local arithmetic controls; leave T symbolic until its
own physical/value evidence is recorded. No native or emulated execution
is part of this observation.

The location ranges use exclusive ends, including the final transfers
already described above. To verify their boundaries, additionally read
`0x004A0EFC`, `0x004A1809` and `0x004A1E33`, each with limit
two; their following instruction starts are the declared exclusive ends.
