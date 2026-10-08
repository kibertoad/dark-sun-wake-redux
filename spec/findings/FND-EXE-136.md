---
id: FND-EXE-136
title: Three count-driven prefix selectors distinguish complementary counts and fresh word versus retained full-width comparisons
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A13C0..0x004A1437
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1D0F..0x004A1D22
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1E38..0x004A1E54
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A0F68..0x004A0FB3
tool: Ghidra 12.1.3 PUBLIC bounded count-driven prefix branch reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 58 at `0x004A1427`, selector 59
at `0x004A1D0F` and selector 60 at `0x004A13C0`.
Each zero-extends byte count C at `0x01BA29E4`. Selector 60
subtracts one from C; selectors 58 and 59 subtract C from 32.
Their thirty-two-bit logical shifts use the low five bits of the resulting
count: k is (C minus one) modulo 32 for 60, and (32 minus C) modulo 32
for 58/59. No local count-zero or upper-count rejection precedes these shifts.

Selectors 58 and 60 join at `0x004A13C8`. They read full N at
`0x01BA29E0`, shift right k and test its low bit. They then read
current full mask M at `0x0075B204`, setting mask one for extracted
bit one or clearing it for bit zero. The zero-bit alternate at
`0x004A1E47` rejoins at `0x004A13DF` after that clear.
They zero-extend word V at `0x01BA29E8`, setting mask `0x40`
when that word is zero or clearing it otherwise; the nonzero alternate
at `0x004A1E3F` rejoins at `0x004A13F2`.

At that join they freshly read and zero-extend word W from N's storage,
rather than reuse the earlier full N. They replace mask `0x80` with
V's bit fifteen shifted right eight. The arithmetic shift operates on
zero-extended masked V, so the contribution is zero or `0x80`.
They XOR zero-extended V and W and logically shift right fifteen. The
result is zero or one; testing its low byte therefore replaces mask
`0x800` according to whether V and W differ at bit fifteen.
A zero result takes `0x004A1E38`, transfers the current mask to the
clear gate at `0x004A0F51`; a nonzero result transfers the current
mask to the set gate at `0x004A0FAE`. FND-EXE-134 grounds these
shared clear/set gates. These branches do not independently replace
mask `0x10`; other M bits are retained locally before the lookup.

Selector 59 instead transfers its complementary count directly to
`0x004A0F68`. FND-EXE-134 records that continuation for selector 61:
read full N, extract bit k for mask one, read full V for mask `0x40`
(V zero), replace mask `0x80` from V's bit thirty-one, and replace
mask `0x800` from the sign bit of full N XOR full V. Here the
retained full N participates in the XOR, with no fresh word read.
The count setup differs from selector 61's decremented count; the
continuation itself is shared, including its alternate clears and joins.

All three paths clear mask four and freshly read a byte at V's storage
for the common lookup described in FND-EXE-132. FND-EXE-133 records
its physical shipped contributions. They publish the resulting full mask,
clear the full stored selector and return the full published mask through
that shared return. No calls occur in these bounded paths. The separate
full-N, V, fresh-W and lookup-byte reads do not establish atomicity,
producer ranges, aliases or runtime table immutability.

## Interpretation

The three physical selectors have explicit count arithmetic and input/read
contracts. Selectors 58/60 mix full-width bit extraction with a later word
comparison; selector 59 retains full-width comparison inputs. Q-EXE-009
in FMT-EXE-006 remains open for count/field/mask/selector producers, lifetime,
other prefix branches and remaining callee contracts. No named originating
instruction, complete helper reading or actual PATH behavior is established.

## Alternatives

- C zero selects bit 31 for selector 60 but bit zero for 58/59. C one
  selects bit zero for 60 but bit 31 for 58/59. C 32 selects bit 31 for
  60 and bit zero for 58/59; C 33 repeats C one's effective counts.
- Upper bits of N can affect the initial bit extraction in 58/60. They do
  not participate in the later word XOR, which uses a fresh read W.
- Word V `0x8000` contributes mask `0x80` in 58/60; full V
  `0x00008000` does not contribute that bit in selector 59.
- Mask `0x800` comes from differing high bits at the stated widths,
  not an exact-input equality test or an established arithmetic overflow rule.
- Sharing a lookup and return does not make the differing count setups or
  retained/fresh reads interchangeable under unresolved runtime lifetime.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls,
then FND-EXE-131's selector slots. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A1427` limit
eight, `0x004A1D0F` limit eight, `0x004A13C0` limit 28,
`0x004A1E38` limit nine and `0x004A0F68` limit 22. Exclude
instructions after the declared branches. Use FND-EXE-134 for the shared
clear/set gates and remaining full-width continuation, and FND-EXE-132
for the fresh-byte lookup and return. Track full subtraction before CL's
masked shift count, full N versus the fresh zero-extended W, retained V,
independent bit clears/sets and direct joins. Check the count controls and
width distinctions in Alternatives as local arithmetic cases; they are not
native input observations. No native or emulated execution is part of this finding.

The location ranges use exclusive ends. Verify the final transfers
already described above with additional windows at `0x004A1435`,
`0x004A1D1D` and `0x004A1E4F`, each with limit two; the
following instruction starts are their exclusive ends. The separately
cited continuation ending at `0x004A0FB3` delegates its remaining
shared suffix to FND-EXE-134 as described above.
