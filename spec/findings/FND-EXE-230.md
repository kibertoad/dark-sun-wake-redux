---
id: FND-EXE-230
title: Segment-state writer adds a wrapped sixteen-bit header-word increment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:07A1..4AE5:07AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0735..4AE5:073C
tool: Ghidra 12.1.3 PUBLIC and executable-reader 2.5.0
environment: null
---

## Observation

The helper at `4AE5:07A1` contains five instructions and twelve bytes,
ending exclusively at `4AE5:07AD`. It reads the current ES-relative word
at `0x0008`, adds seventeen at sixteen-bit width, sets CL to four and
logically shifts AX right by CL. It returns near at `4AE5:07AC` with no
extra argument cleanup. There are no calls or explicit memory stores in
this helper, and no explicit DS or ES register writes.

For an unsigned input word w, its returned word is
`((w + 17) modulo 65536) >> 4`. The addition wraps before the shift;
this is not an unbounded rounded-up size. Its possible results include
zero and are bounded by 4095. For example, inputs zero, 65518, 65519 and
65535 produce one, 4095, zero and one respectively. These are arithmetic
consequences of the reading, not observations from executing the original.

The procedure at `4AE5:0735`, called by FND-EXE-228's publisher, calls this
helper and immediately adds its returned AX to current DS-relative word
`0x0120`, again at sixteen-bit width. This store precedes the procedure's
later DS save and segment-load sequence. The helper's explicit instructions
leave the caller's DS and ES unchanged before that addition; this does not
prove interrupt-time preservation or native storage identity.

## Interpretation

This identifies one direct writer of the word reloaded by FND-EXE-228's
segment publisher, and the exact width and order of its increment. It also
rules out assuming that every call strictly increases the stored value:
the increment can be zero, and the accumulated word can wrap. Admission
of the actual input values could constrain those cases, but is not supplied
by this bounded reading.

Q-EXE-001 and Q-EXE-010 retain native DS/ES and header admission, all other
state writers, the remainder of the writing procedure, saved-header storage,
other callee effects and interrupt-enabled changes. No allocator unit,
allocation bound, loop termination, complete_reading or replacement inventory
is established.

## Alternatives

An unbounded addition followed by division by sixteen is contradicted by the
sixteen-bit addition before the logical shift. A strictly positive increment
for every possible word is contradicted by the wrap-to-zero inputs. Calling
the input an admitted allocation size would require the missing header and
caller evidence; the arithmetic alone does not supply that contract.

## How to reproduce

At revision `f28813e`, use FND-EXE-226's original-source x86-bounds region,
hash and default limits. Set entry and sole entries value to `0x000407F1`
(`4AE5:07A1`), supplying no seeds or summaries. The bounded report gives
twelve covered bytes, five instructions, one near return and no calls,
holes, gaps or hardware boundaries. Its CFG completion is not a Standard
complete reading.

In the resident Ghidra snapshot, read-only with analysis disabled, run
ReportInstructionWindow at `4AE5:07A1`, count fourteen, restricting this
helper to the interval above. Read `4AE5:0735`, count 38, restricting the
caller claim to its first two instructions, ending at `4AE5:073C`. The
longer window is not claimed as one function. Sources, listings, configs
and reports remain in GAME_DIR and are not committed.
