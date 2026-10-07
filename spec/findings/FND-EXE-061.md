---
id: FND-EXE-061
title: Marker modifier selects zero-return branches and a full-byte bypass before its mask-class abort boundary
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4CC0..0x005F4D28
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600AB0..0x00600AB6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600AC0..0x00600AC6
tool: Ghidra 12.1.3 PUBLIC, bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-060 prepares a zero-extended marker and original context in registers
before calling this helper. FND-EXE-058 also prepares a zero-extended metadata
byte and the callback's sixth argument. This body reads the marker from the
incoming return register, not a stack-argument slot. It initializes its
local result register to zero and first compares the incoming low byte with
255. Equality returns zero immediately through ordinary frame restoration,
before masking the marker.

For any other low byte it masks the full incoming register with 112,
clearing all bits except the low-byte bits selected by that mask. The resulting
classes and direct destinations are:

| Masked class | Direct local behavior |
|---|---|
| 0 | Return zero |
| 16 | Return zero |
| 32 | Pass prepared context to `0x00600AC0`, then return its result |
| 48 | Pass prepared context to `0x00600AB0`, then return its result |
| 64 | Pass prepared context to `0x00600A90`, then return its result |
| 80 | Return zero |
| 96 | Reach `0x006020C0`, the abort import |
| 112 | Reach the same abort import |

The branches use full-word comparisons after the mask, with only these
possible masked values. Bits outside the mask do not change this selection,
except that the initial full-byte-255 test takes precedence. Thus marker
255 returns zero despite having masked class 112. Input register bits above
the low byte cannot change either the byte comparison or the masked class.

Both newly read local callees clear their return register, restore their
frame and return. They read no original argument, make no additional call
and perform no memory access beyond ordinary frame handling. FND-EXE-060
establishes the same direct constant-zero contract for `0x00600A90`.
Consequently all six non-abort classes return zero on normal direct completion;
the outgoing context is not consumed by any of those three local bodies.
The modifier itself does not dereference context or compute a nonzero base.

Each local-callee branch reserves twelve outgoing stack bytes and pushes
the context register. Its return path copies the callee result and restores
the frame directly rather than depending on a guessed consumed parameter
count. The rejection boundary is the exact abort thunk established by
FND-EXE-041; no behavior after that imported abort call is inferred here.
The decompiler's inferred extra abort arguments do not establish an input
contract for the import.

For FND-EXE-060's first non-255 marker, the prepared modifier result supplied
to the subsequent typed reader is therefore zero for these admitted classes.
This does not establish what that typed reader does with zero, how many bytes
it consumes, or whether the overall metadata read succeeds. For
FND-EXE-058's metadata-byte call, the saved result is likewise zero on these
local ordinary branches. Its later consumers still require their own reading.

## Interpretation

The modifier's complete direct mask-class selection and constant local
callees are now bounded. It is not an unknown contextual base producer on
these branches. Q-EXE-009 retains marker provenance and admission, typed-reader
consumption, saved-result consumers, stream bounds, exceptional import effects
and the remaining matching/dispatcher contracts. No safe schema or replacement
reader is implemented from this finding.

## Alternatives

Always applying the mask before testing 255, rejecting the special marker
because its mask class is 112, treating bit 128 as a distinct ordinary class,
or inferring a contextual pointer lookup inside the three local callees are
ruled out by the direct bodies. Giving all marker classes a normal zero return
would erase the two abort boundaries. Prepared context arguments alone are
not evidence that the callee reads them.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005F4CC0`,
read thirty instructions there, eighteen from `0x005F4D00`, and twelve
from `0x00600AB0`. Restrict claims to the cited bodies, excluding the later
functions printed. Use FND-EXE-060 for the previously read zero helper and
prepared metadata inputs, FND-EXE-058 for the other callback caller, and
FND-EXE-041 for the exact abort import. Track low-byte bypass priority,
full-word mask, all eight classes, context preparation versus consumption,
constant returns and frame restoration. Keep marker admission, import and
later reader effects conditional. Keep rich reports local and execute no
interpreter or game.
