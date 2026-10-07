---
id: FND-EXE-113
title: First callback callee writes or replaces a byte before pair removal and conditional scheduling
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A41F0..0x005A42AF
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4467..0x005A446A
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4470..0x005A4482
tool: Ghidra 12.1.3 PUBLIC bounded first-callee byte-write and scheduling prefix
environment: null
---

## Observation

FND-EXE-112 calls `0x005A41F0` with object O, a zero-extended byte and
zero. This callee pushes three registers and reserves sixteen stack bytes.
Its first full input O is current ESP plus thirty-two, its second full input
V plus thirty-six, and the third input's low byte plus forty. It saves that
third byte as local F and V's low byte as local B. It reads record R from
O plus `0x014C`, full index I from R plus twelve, full count C from
R plus sixteen and full size L from R plus eight. It computes P = I + C
modulo thirty-two bits and subtracts L once when P is at least L unsigned.
No local nonzero-size or resulting-position bound is proved.

If C is below L unsigned, it reads R's full base and writes B at base plus
P, sets a local result to full one, and increments the current full count
at R plus sixteen modulo thirty-two bits. If C is at least L it instead
chooses P minus one when P is nonzero, or L minus one modulo thirty-two
bits when P is zero, reads R's full base and writes B at that position.
That path sets the local result to zero and does not directly increment
count. Both converge on its low-byte test. Zero ORs mask two into local F;
one leaves F unchanged. This result is an internal branch value, not a
proved function return. With L zero, the zero-position replacement can
form all-ones offset; the prefix does not reject that condition.

After the byte write and any count/F change, it freshly reads O plus
`0x010C`, ORs full mask `0x1C`, and calls `0x004F2600` with fixed
target `0x005A4BB0` and that combined full argument. FND-EXE-105
records the pair remover's repeated-match behavior. Its return is not tested.
On normal return this caller freshly reads R from O plus `0x014C`,
full O plus `0x015C`, and the new R's full count at sixteen. Equality
branches to `0x005A4470`; inequality instead loads a single-precision
value at `0x0073BF00`, multiplies by O's current single-precision
`0x0108`, freshly reads full `0x010C`, ORs `0x1C`, stores the
single-precision product as outgoing second argument, and calls
`0x004F2490` with fixed target `0x005A4BB0` and that combined argument.
It continues at `0x005A42AF` without testing insertion success.

The equality branch does not perform that immediate insertion. Its observed
prefix zero-extends bytes O plus `0x0118` and `0x011C`, ORs mask one
into the former, computes their full intersection, and publishes the modified
`0x0118` byte. Its later priority/state path is outside this bounded reading.
The insertion path and that branch's later joins can continue into further
local-F and record behavior; no complete function exit or global failure
convention is assigned here. Removal occurs after the initial byte write,
and fresh reads follow it, so neither state nor callback arguments can be
replaced by the entry snapshot without further contracts.

## Interpretation

The first zero-branch callee has a concrete byte-write/replacement prefix
followed by pair removal and a count-sensitive scheduling branch. A count
at least size selects replacement, not a locally rejected input. Floating
product meaning/environment, buffer and object validity, aliases, record
producers, later equality/flag paths and callee effects remain Q-EXE-009 in
FMT-EXE-006. This is not a complete callee reading or an actual PATH outcome.

## Alternatives

- P uses wrapped addition and at most one subtraction, not general modulo.
- Replacement does not directly increment count; zero P selects L minus one.
- Pair removal follows the byte write and does not prove rollback of it.
- Post-removal count and object fields are fresh reads, not retained inputs.
- Immediate insertion is only the inequality path, and its result is ignored.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
FND-EXE-112 independently supplies this direct call target. Use the saved
Ghidra program with -noanalysis and ReportInstructionWindow.java at
`0x005A41F0` limit 140 and `0x005A4467` limit 25. Restrict observations
to the declared ranges, following the replacement zero-position branch at
`0x005A4467` back to `0x005A4231` and the equality destination at
`0x005A4470`. Track three saved registers, local reservation, byte/full
inputs, arithmetic widths, write/count order, low-result F update, fresh
post-removal reads and outgoing floating-product arguments. Keep reports in
GAME_DIR/analysis/exe-batches; commit no original listings or bytes and execute
neither interpreter nor game.