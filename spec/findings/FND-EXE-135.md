---
id: FND-EXE-135
title: Byte and full-width prefix branches use distinct exact-input gates and lookup read ordering
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1064..0x004A1114
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2189..0x004A21A6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2151..0x004A216F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1E69..0x004A1E75
tool: Ghidra 12.1.3 PUBLIC bounded byte and full-width prefix branch reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 55's physical target `0x004A1064`
and selector 57's target `0x004A10C7`. Both consume storage at
`0x01BA29E0` as N and `0x01BA29E8` as V, but at different
widths. Selector 55 zero-extends only each byte; selector 57 reads each
full thirty-two-bit value. Each reads current full mask M at `0x0075B204`
after testing N, and replaces the following bits while retaining the other
M bits locally until the lookup:

| Mask | Selector 55 sets when | Selector 57 sets when |
| --- | --- | --- |
| `0x00000001` | byte N nonzero | full N nonzero |
| `0x00000010` | byte V's low four bits nonzero | full V's low four bits nonzero |
| `0x00000040` | byte V zero | full V zero |
| `0x00000080` | byte V's bit seven set | full V's bit thirty-one set |
| `0x00000800` | byte N exactly `0x80` | full N exactly `0x80000000` |

Each listed bit is cleared when its condition is false. A zero low nibble
rejoins before the independent V-zero test; a nonzero V rejoins before the
high-bit and exact-N gates. N zero does not skip the later V tests.
The byte branch tests CL and BL, so upper storage bytes do not affect those
conditions. The full branch tests full ECX for N and full EAX for V zero,
although its low-nibble test consumes AL. Its high-bit contribution masks
V with `0x80000000` and logically shifts right 24, giving zero or
`0x80`. This does not sign-extend V or test its low byte's bit seven.

For selector 55, the N-zero alternate at `0x004A2199` reads M,
clears mask one and rejoins at `0x004A107B`. The zero-nibble alternate
at `0x004A2191` clears mask `0x10` and rejoins at `0x004A108E`.
The V-nonzero alternate at `0x004A2189` clears mask `0x40` and
rejoins at `0x004A1099`. The exact-N alternate at `0x004A1E69`
sets mask `0x800` and rejoins at `0x004A10BA`; the unequal path
clears that bit. Both clear mask four, zero-extend retained BL, and transfer
directly to the common lookup at `0x004A0E64`. They skip the preceding
fresh byte read at `0x004A0E5D` recorded in FND-EXE-132.

For selector 57, the N-zero alternate at `0x004A2159` reads M,
clears mask one and rejoins at `0x004A10DE`. The zero-nibble alternate
at `0x004A2151` clears mask `0x10` and rejoins at `0x004A10EE`.
The V-nonzero alternate at `0x004A2167` clears mask `0x40` and
rejoins at `0x004A10F9`. The exact full-N comparison transfers directly
at `0x004A110F` to `0x004A0F4D`, preserving its equality flags.
FND-EXE-134 records that shared gate: equality sets mask `0x800`,
inequality clears it, both clear mask four and transfer to the fresh byte
read at `0x004A0E5D`. Thus selector 57's lookup input is a later byte
read from V's storage, not necessarily retained full V's low byte.

FND-EXE-132 grounds the common lookup and return: zero-extend the word
at `0x006F2B70` plus twice the chosen byte, OR it into the computed
mask, publish full `0x0075B204`, clear full selector `0x01BA29EC`,
restore saved registers and return the full published mask. FND-EXE-133
records all physical shipped lookup contributions. No calls occur in these
bounded branch paths. The distinct input reads, mask publication and selector
clear do not establish atomicity or runtime table immutability.

## Interpretation

The byte and full-width branches share local mask positions but differ in
consumed width, exact-N value and retained versus fresh lookup input. Their
conditions do not identify the originating arithmetic instruction or prove
producer ranges. Q-EXE-009 in FMT-EXE-006 remains open for field/mask/selector
producers, runtime table writers, other prefix branches and selected-callee
contracts. This is a bounded branch reading, not a complete helper reading,
a named operation model or established PATH behavior.

## Alternatives

- Full N `0x00000080` satisfies the byte branch's exact-N gate but not the
  full branch's; full N `0x80000000` has the reverse exact-N result.
- V `0x00000100` is zero at byte width but nonzero at full width. Its
  low nibble is zero in both branches, which does not merge their zero tests.
- V `0x00000080` contributes mask `0x80` only in the byte branch;
  V `0x80000000` contributes it only in the full branch.
- The byte lookup retains BL; the full branch reloads a byte. No local call
  intervenes, but that alone proves neither global lifetime nor concurrency.
- Exact-N equality is not generic arithmetic overflow; the upstream operation
  and field producers must be read before giving those bits semantic names.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls,
then FND-EXE-131's exact selector slots. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A1064` limit
44, `0x004A2189` limit twelve, `0x004A2151` limit twelve and
`0x004A1E69` limit five. Restrict claims to the declared ranges and
exclude subsequent unrelated instructions. Use FND-EXE-134's shared gate
at `0x004A0F4D` and FND-EXE-132's separate lookup entries and return.
Track byte zero-extension versus full reads, each alternate clear and join,
full comparison flags across the direct transfer, high-bit extraction,
retained BL versus the fresh lookup byte, publication and selector clearing.
Compare zero/nonzero and low-nibble cases independently; use the exact-N,
upper-byte-only V and distinct high-bit examples in Alternatives as local
arithmetic controls. No native or emulated execution is part of this observation.

The location ranges use exclusive ends, including the final transfers
already described above. To verify their boundaries, additionally read
`0x004A110F`, `0x004A21A1`, `0x004A216A` and `0x004A1E70`,
each with limit two; their following instruction starts are the declared
exclusive ends.
