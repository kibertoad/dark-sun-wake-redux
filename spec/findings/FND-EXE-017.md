---
id: FND-EXE-017
title: Compiled filename helper writes and bounds a drive selector before its table load
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B4FC0..0x004B5082
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B512D..0x004B515C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B2340..0x004B234C
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction reporter
environment: null
---

## Observation

FND-EXE-015 identifies the candidate-availability caller and its separate
local output byte and name buffer. Its helper at `0x004B4FC0` reserves
236 bytes. Relative to that frame, the candidate pointer is at offset
240, the output name pointer at 244 and the selector-output pointer at
248. This finding reads only the initial selector phase, not the whole
filename transformation.

A null candidate, an empty candidate or a first byte equal to ASCII space
selects a failure path before this phase's selector store. That path passes
32-bit value 2 to `0x004B2340`, which reads the argument's low 16 bits and
stores that word at `0x01BE6D0A`. The helper then returns zero.

For a remaining candidate, an intervening call to `0x004F64B0` receives
`0xB36`; its return is discarded. Its external effects remain unread.
The helper next reads the byte at `0x01BE6D16` and writes it through the
selector-output pointer. The meaning and writers of this initial byte
remain unknown.

If the candidate's second byte is colon, the path at `0x004B512D` consumes
the first two bytes. It combines the first byte with 32 by bitwise OR,
subtracts 97 at byte width, and stores the result through the same output
pointer at `0x004B5139`. Thus A/a yields 0, C/c yields 2 and Z/z yields 25;
arithmetic wraps modulo 256. This is a byte calculation, not a preceding
alphabetic validation.

Both paths read the produced byte and compare it unsigned with 25 at
`0x004B505F`. A larger value fails. Otherwise the zero-extended value indexes
the four-byte pointer array at `0x01BE6D60`, with the load at `0x004B506F`.
A null selected pointer also fails. Both failures pass value 3 through the
same word setter and return zero. The selector output has already been
written on these failure paths; the failure continuation does not roll that
store back. For the availability caller's distinct stack outputs, this is
an observable output mutation even when this phase returns failure.

## Interpretation

The initial table load is protected by an unsigned byte range check and a
nonnull pointer check. This narrows the unread producer boundary in
FND-EXE-015. It does not establish the selector ultimately consumed on every
successful return: later transformation, calls, possible aliases and the
array's writers still need reading. Nor does this identify the initial
selector global's role, the selected object's type, its virtual targets or
the shared word's complete semantics. No shell resolution outcome follows
from these guards alone.

## Alternatives

A signed range check, an unchecked initial array load and failure that
restores the prior selector are ruled out for these direct paths. Naming
the initial global as a current drive, or treating later filename processing
as already validated, would require additional producer and caller evidence.
Early failure has no selector store in this phase; arbitrary output aliases
are not covered by an unchanged-output claim.

## How to reproduce

Use the verified PE and image base from FND-EXE-011. Read the function summary
and a 160-instruction window from `0x004B4FC0`, then the separate prefix and
failure window from `0x004B512D` (18 instructions). Read four instructions
from `0x004B2340` to check the low-word argument access and word store.
Trace the incoming stack arguments after frame allocation, the two selector
stores, unsigned conditional jump and table index extension. Follow both
failure routes into the shared return and distinguish their numeric values.
Interpret only the stated locations, not adjacent instructions after gaps.
Retain the rich reports locally. These are static checks; execute no game,
interpreter or synthetic native call.
