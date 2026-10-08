---
id: FND-EXE-063
title: Nibble-nine byte reader sign-fills only after termination under a full-word shift guard
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4D70..0x005F4DC3
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-062's nibble-nine branch prepares an input cursor and local output
address in registers before calling this reader. The reader saves both
incoming register values in separate local words, initializes its full-word
accumulator and shift-position word to zero, and reads no original
stack-argument slot for those inputs.

Each iteration reads one byte at the saved cursor and increments that cursor
at 32-bit width. It zero-extends the byte, masks its low seven bits, shifts
that payload left at 32-bit width using the current shift position, and ORs
it into the accumulator. The shift word increases by seven each iteration
at 32-bit width; the x86 shift instruction masks its count to five bits.
As in FND-EXE-059, group k contributes its low-seven-bit payload shifted
by `(7 * k) modulo 32`, with overflowing word bits discarded.

Bit seven of the original byte controls continuation. Set repeats; clear
terminates after including that byte's payload. The reader performs at least
one byte read and has no local source-length, byte-count, canonical-encoding,
maximum-shift or cursor-wrap guard.

Only after termination does it compare the updated full shift word with
thirty-one at unsigned width. A larger value skips sign fill. A value at
most thirty-one tests bit six of the terminating byte. If that bit is set,
it forms an all-ones word shifted left by the updated shift position and
ORs that word into the accumulator. Otherwise it leaves the accumulated
payload unchanged. This is a final-byte test, not sign extension of every
payload byte. For ordinary streams of one through four bytes, the shift
word is seven through twenty-eight; five bytes yield thirty-five and skip
the fill. No finite byte limit follows: extraordinarily long streams can
wrap the full shift word, so the actual unsigned guard is the contract,
not a universal four-byte rule.

It then writes exactly one full accumulator word through the saved output
address, loads the advanced input cursor into the return register, restores
its saved registers and frame and returns. There is no intermediate output
store, additional call or separate validity status. The output write follows
all input reads for this invocation, while alias effects on subsequent
caller reads remain conditional.

Arithmetic controls distinguish the sign boundary: a single terminating
payload of 64 produces the full-word representation of negative 64; a
single terminating payload of 127 produces all ones. A continuing payload
of 64 followed by a terminating zero produces positive 64, because only
the last byte's bit six selects fill. Four continuation bytes with zero
payload followed by a terminating payload of 64 produce zero: the shifted
payload overflows the word and the updated shift value thirty-five skips
fill. These are instruction-arithmetic controls, not evidence that those
encodings occur in shipped metadata.

For FND-EXE-062's normally terminating nibble-nine call, this establishes
the prepared local word's full-width last writer and the returned cursor.
That caller then uses its shared decoded-zero, base-adjustment and optional
indirect-read stages. The reader itself supplies no target validation and
does not interpret the final accumulator as an address.

## Interpretation

The remaining typed-reader callee now has a direct byte-consumption,
post-termination sign-fill, output and cursor-return contract. It does not
establish stream provenance, finite bounds or admissible marker/value
combinations. Q-EXE-009 retains those inputs and bounds, other matching
helper consumers, remaining classification, record identity and dispatcher
provenance. No validated schema or replacement parser is implemented.

## Alternatives

Sign-extending every byte, using bit seven for sign fill, always filling
a fifth-byte value, rejecting overlong input, or returning the decoded word
rather than the cursor are ruled out by the instructions. A caller's signed
test of the decoded word does not prove a finite source length. Treating
this body as an ordinary bounded signed decoder would erase masked shifts,
full-word counter wrap and the exact unsigned fill guard.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read forty instructions from
`0x005F4D70` and three from `0x005F4DC0`, restricting claims to the cited
body. Use FND-EXE-062 for register writers, the local output and its return
consumer, and FND-EXE-059 for the contrasting no-fill reader. Track saved
addresses, byte zero-extension, payload mask, full accumulator and shift
words, masked shift count, final-byte continuation and bit-six tests,
unsigned post-loop guard, sole output store and cursor return. Check sign,
continued-sign, fifth-byte and overlong arithmetic boundaries without
asserting shipped-input admission. Keep source bounds, alias and exceptional
effects conditional. Keep rich reports local and execute no interpreter or game.
