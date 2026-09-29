---
id: FND-CONFIG-138
title: Parameter byte advancement preserves the iterator flag on its ordinary path and clears it after an end-check failure
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2805
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:281B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:20B3
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; declared MZ relocation and operand-size checks
environment: null
---

## Observation

FND-CONFIG-137 leaves the expression byte-reader dependencies
open. Resident 172C:2805, file `0x0000ECC5..0x0000ECDB`,
calls local reader 172C:281B, retains its byte result in a stack
local, calls advancement helper 172C:20B3 and returns the retained
byte. It does not substitute an error byte after advancement.

Reader 281B, file `0x0000ECDB..0x0000ED0D`, reads byte
4C13:0192 and sign-extends it to a word. That index selects a
word at 4C13:0295 plus twice the index. It adds word
4C13:0193 with 16-bit arithmetic, then adds the resulting
word to the offset of the far pointer at 4C13:0255. It reads
one byte through that resulting pointer and returns. Its complete
body has no call and no stored-data write. It contains no bound
check before that read; this finding does not establish the
pointer, index or buffer range as valid.

Advancement helper 20B3, file
`0x0000E573..0x0000E5B5`, sign-extends the same selected
byte and increments the selected word at 4C13:0295 plus
twice the index. It reloads that word, zero-extends it to a double
word, and compares it unsigned with the double word at
4C13:0313. A smaller value returns directly. An equal or
greater value calls overlay entry 5702:00B1 before returning.
FND-CONFIG-135 reads that entry's common returning flag-clear
path. The advancement's word increment wraps before its
comparison; no wider carry is retained. The byte wrapper's read
therefore precedes this end check.

Because the selector is a signed byte, the increment target starts
within 4C13:0195..0393. It cannot be the separate flag at
4F49:000A. Neither reader nor ordinary advancement contains
an opcode callback, indirect call or direct flag write. The only
advancement call is the failed-check route just described.
On that route, the known entry clears the flag if it returns
through its common returning path; its preceding helper effects
and successful return are not established here.

The byte-to-word conversions at `0x0000ECE4`,
`0x0000E57C` and `0x0000E594` use the default 16-bit
operand size, without an operand-size override. Their actual
operation extends AL into AX; the disassembler's printed
mnemonic alone is not used to infer a wider conversion.
All eight segment-load operands in these reader and advancement
bodies are declared MZ relocations holding raw 3C13, mapped
to 4C13. The failed-check call operand at `0x0000E5B2`
is a declared relocation holding raw 4702, mapped to 5702.

## Interpretation

These helpers do not dispatch another script instruction or restore
an earlier opcode's flag. With ordinary returning calls and no
failed end check, reading and advancing a byte preserves the
iterator flag in their local dataflow. A failed end check instead
has a concrete flag-clear route; it does not establish a flag-one
route. This narrows the remaining effects in FND-CONFIG-137
without proving which flag reaches the later rest iterator.

The post-read comparison is not a pre-read memory-safety check.
No complete script-reader contract, valid-input range, buffer
provenance or end-of-script behavior is inferred from these
local bodies alone.

## Alternatives

FND-CONFIG-139 reads the B1 expression root, selector parser
and chained wrapper, retaining its seed and lookup callee effects.
Q-CONFIG-008 retains the selection-byte and cursor producers,
far-pointer provenance, reachable expressions and nested handlers,
remaining expression helper effects, pointer replacement and later
rest-entry state changes. A reading that every byte read invokes
an opcode callback is ruled out by the complete helper bodies.
A reading that ordinary byte advancement itself sets the iterator
flag is also ruled out; its exceptional call is the known clear
path. Asynchronous effects and the exceptional entry's transitive
calls are not covered by the local preservation statement.

## How to reproduce

Read the complete wrapper, reader and advancement bodies at
the stated file bounds. Track the signed-byte selector, 16-bit
cursor and offset additions, retained return byte, increment,
zero-extension and unsigned comparison. Verify default operand
size at the three conversion locations before assigning meaning
from the mnemonic. Check MZ relocation membership at
`0x0000ECDC`, `0x0000ECE8`, `0x0000ECF4`,
`0x0000ECFE`, `0x0000E574`, `0x0000E580`,
`0x0000E58C`, `0x0000E5A3` and `0x0000E5B2`.
Apply resident load segment 1000 to map their addresses.
Compare FND-CONFIG-135 and FND-CONFIG-137; keep the
ordinary path distinct from the failed-check callee's effects.
