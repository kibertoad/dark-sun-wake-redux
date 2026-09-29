---
id: FND-CONFIG-137
title: Nested parameter instructions restore parameter blocks rather than the iterator flag
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3278
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3351
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:36CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:370D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F5A
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; declared MZ relocation mapping and expression-table inspection
environment: null
---

## Observation

The opcode 0x22 handler in FND-CONFIG-136 calls parameter
reader 172C:367B with four. Its complete local body at
`0x0000FB3B..0x0000FB8E` calls expression reader 172C:3278
once per parameter and stores the returned double word and
address in the parameter block at 4C13:00B4 and 4C13:00D4.
This wrapper does not restore the iterator flag after evaluation;
its expression reader and dependency effects must still be considered.

FND-SCRIPT-010 identifies an expression form that dispatches an
instruction. The bounded expression selection block at
`0x0000F784..0x0000F7EC` confirms its index normalization:
bytes from 0x80 through 0xCF with bit 0x40 set have 0x40
subtracted before the word-table lookup. The original extended
bit is stored separately. Thus byte 0xCC selects the same table
entry as 0x8C, not the table's unnormalized 0xCC position.
The word for normalized 0x8C at `0x0000FA7B` is 3351.

The nested-instruction branch at
`0x0000F811..0x0000F82C` calls local save helper 172C:36CE,
then opcode reader 172C:20F5, then passes its returned word to
172C:018F. That is the pre-handler callback dispatch in
FND-CONFIG-136. After dispatch returns, it calls restore helper
172C:370D and reads the accumulator at 4C13:031B as the
expression value. Opcode reader 20F5 calls the byte reader,
stores the returned byte at 4C13:032A and reloads that byte;
this establishes argument forwarding, not the byte reader's full
state effects.

Save helper 36CE, file `0x0000FB8E..0x0000FBCD`, compares
signed nesting word 4C0E:0010 with two. Below two, it copies
64 bytes from 4C13:00B4 to 4C13:0034 plus 64 times that
word, then increments it. At two or greater it calls the clear
entry 5702:00B1 from FND-CONFIG-135 instead. No lower-bound
check is present. Restore helper 370D, file
`0x0000FBCD..0x0000FC09`, decrements a positive nesting word
and copies 64 bytes from that indexed slot back to 4C13:00B4.
At zero or negative it calls the same clear entry instead.
It has no upper-bound check.

Both copy calls resolve to resident 1000:3F5A. Its complete
body at `0x0000915A..0x0000917E` copies the supplied byte
count from source far pointer to destination far pointer using
words and an optional trailing byte, preserving DS on return.
It calls no further routine. For ordinary save depths zero and
one, the two slots occupy 4C13:0034..00B3. For corresponding
restore depths one and two, the only copied destination is
4C13:00B4..00F3. Those spans contain no 4F49:000A flag.
Neither normal helper stores a saved flag or invokes a callback
to restore it. Their error paths instead use the entry whose
common returning path clears the flag (FND-CONFIG-135).

The segment operands for the nesting word, parameter block,
copy helper and clear entry are declared MZ relocations. Raw
3C0E, 3C13, zero and 4702 map to 4C0E, 4C13, 1000
and 5702 respectively. No raw operand is treated as a loaded
segment without that relocation.

## Interpretation

On the ordinary two-slot save/restore paths, a nested instruction's
callback is not undone by parameter restoration. The restored
bytes are parameters and their addresses, not the iterator flag.
This removes parameter-block restoration as a proposed reason
that the opcode 0x22 pre-handler clear must still hold at the
later rest iterator. It does not prove a particular later flag:
nested handlers, expression continuation, byte readers and other
intervening helpers can still change state.

## Alternatives

Q-CONFIG-008 retains reachable shipped nested instructions,
byte-reader and remaining expression effects, other flag writers,
pointer replacement and timing relative to the rest entry.
A reading that treats ordinary parameter restoration as flag
rollback is ruled out by its copy bounds and destinations.
Malformed nesting words and the provenance of their ordinary
range remain unread; the save helper's signed upper check alone
is not a lower-bound invariant. The copy-span conclusion above
is scoped to the named ordinary depths, not arbitrary corrupted
state. No complete flag invariant or rest-loop termination claim
is made.

## How to reproduce

Read the parameter wrapper at its stated bounds. Follow expression
index preprocessing before looking up normalized 0x8C in the
99-word table at 172C:35A3. Read the nested branch, opcode
reader `0x0000E5B5..0x0000E5CB`, complete save/restore
helpers and complete copy helper at the stated bounds.
Verify MZ operand membership at `0x0000FB8F`,
`0x0000FB9E`, `0x0000FBB0`, `0x0000FBB6`,
`0x0000FBBC`, `0x0000FBCA`, `0x0000FBCE`,
`0x0000FBF0`, `0x0000FBF4`, `0x0000FBFC` and
`0x0000FC06` before mapping segments. Track the ordinary
slot spans and distinguish error calls from restoration.
Compare FND-CONFIG-135, FND-CONFIG-136 and FND-SCRIPT-010;
do not infer preserved state from the word-table target alone.
