---
id: FND-EXE-011
title: Shipped DOSBox target-token routine retains a trailing colon
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00597BA0..0x00597D27
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003158D4..0x003158E8
tool: Ghidra 12.1.3 PUBLIC, bundled bounded reporters, executable-reader 2.2.0 XXH3
environment: null
---

## Observation

The shipped interpreter is a 3802624-byte i386 PE32 executable, XXH3
`09861838aa3018346f9f15c9a4f5925c`, with preferred image base `0x00400000`.
The data record at shipped offset `0x003158D4`, mapped at `0x00716CD4`,
contains five consecutive 32-bit values: a pointer to the NUL-terminated
ASCII command name GOTO at `0x0073B0A1`, value 1, target `0x00597BA0`,
value 0, and pointer `0x0073B0A6`. These are observed record contents;
the dispatcher consuming them has not been read. In particular the fourth
word is retained rather than silently discarding a possible object adjustment.

The target routine has a non-help path that advances over leading bytes using
an external character-classification facility, then checks an object field at
offset `0x28`. A zero field returns without searching. With a nonzero field,
the routine removes exactly one leading colon by advancing its target pointer
once at `0x00597D1D`. It scans the remaining token to NUL, replacing the first
space or tab with NUL. That scan does not remove a trailing colon.

For a nonempty resulting token, the call at `0x00597C77` passes the object
field at `0x28` and the target pointer to `0x0059E6C0`. The caller tests the
low byte of the result at `0x00597C7C`; nonzero returns, while zero enters
its diagnostic route. Empty-token handling is a separate route. No text
rendering, command dispatch or game execution was observed.

## Interpretation

This is direct compiled evidence for one part of the source prediction in
SRC-DOSBOX-GOG-0742: on the inspected non-help path, a target token such as
`INS_GM1:` reaches the search call with its trailing colon intact. FND-EXE-012
records part of that callee and its cleanup. This does not establish that every
GOTO command reaches this target, the complete helper outcome, or global
source-to-binary correspondence.

## Alternatives

Removing a trailing colon in this target-token scan is ruled out by its
space/tab/NUL conditions and the single leading-colon increment. Normalization
elsewhere, a dispatcher selecting another target, and environment-dependent
file lookup remain outside this bounded reading. A label-name search alone
would not prove command dispatch; the pointer record is not used as such proof.

## How to reproduce

Verify the manifest identity of `DOSBOX/DOSBox.exe`, then import that file into
Ghidra 12.1.3 as its PE32 image at `0x00400000`. The recorded import used a
180-second analysis timeout and left some code undisassembled. Use the pinned
`ReportBytePattern.java` to locate the five-byte ASCII GOTO/NUL token
(`474F544F00`), with 100 hits and 20 references per hit as reporter limits.
Follow the little-endian pointer to the record at `0x00716CD4`; read 20 bytes
with `ReportDataBytes.java`. Independently locate that exact five-word record
in the shipped file with `ReportPhysicalBytePattern.ps1`, maximum 16 matches;
the recorded match was `0x003158D4`. Keep these reports local.

Recover only entry `00597BA0` with `RecoverCitedFunctions.java` if no function
exists there. Use `ReportFunctionSummary.java` and bounded instruction windows
starting at `0x00597BA0` (120 instructions), `0x00597CE6` (30 instructions)
and `0x00597D1D` (4 instructions). Inspect only the function's reported bodies
`0x00597BA0..0x00597C3E` and `0x00597C40..0x00597D27`, with exclusive ends;
do not interpret an adjacent function printed after a gap. Follow the pointer
update, token terminators, call arguments and low-byte result test directly.
Do not substitute the nearby diagnostic-registration routine for this target,
and do not run DOSBox, the shell or a command harness.
