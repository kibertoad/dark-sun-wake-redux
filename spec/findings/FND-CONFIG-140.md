---
id: FND-CONFIG-140
title: The selector lookup reads typed values and conditionally writes a three-byte table slot
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:0009
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:06C4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:039A
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; declared MZ relocation and effective operand-size checks
environment: null
---

## Observation

FND-CONFIG-139's chained wrapper calls 1AA0:0009. Its
complete body is `0x0000FC09..0x0000FD2A`, ending before
the following local routine. It rejects input index 9999 or a
signed-negative index, or a zero byte at 4F49:0C33 plus three
times the index, by assigning minus two to DS:40C4 and returning
zero. It does not reset DS:40BC on that branch.

A nonzero table byte selects a stride word at DS:6167 plus
twice the byte and a far base pointer at DS:614F plus four
times the byte. Multiplying the stride by the paired word at
4F49:0C34 and adding its low word to the base offset forms
a local far pointer with 16-bit offset arithmetic.

The routine calls 2D40:06C4 with the table byte and the low
byte of its selector word. That complete helper at
`0x00022CC4..0x00022CE1` returns the word at DS:5A33
plus 196 times its first argument byte plus twice its second
argument byte. It has no call or stored-data write.

After that call, the lookup reads byte DS:60ED plus the full
selector word and stores it at 4C0E:0005. Returned offset
minus one assigns minus one to DS:40C4 and returns zero,
without assigning DS:40BC. Otherwise the offset is added to
the local far pointer. Metadata byte six or at least 128 returns
the pointer value initially; other metadata reads through that
pointer. Width byte DS:040C plus the metadata selects signed
word for width two, double word for width four, and signed
byte otherwise. The byte conversion uses default 16-bit
operand size before the explicit word-to-double-word extension.
The pointer is then assigned to DS:40BC.

When the metadata byte is at least ten, it passes the value as
a double word and metadata minus nine as a word to local
1AA0:039A, replacing the result with that helper's returned
DX:AX pair. Other metadata skips this call. The successful
continuation assigns zero to DS:40C4 and returns the result.
The only two callees in this body are 2D40:06C4 and
1AA0:039A.

Helper 039A, `0x0000FF9A..0x0000FFD2`, selects the
three-byte slot at 4F49:0C33 plus three times DS:60EB.
It stores the low byte of its metadata-minus-nine argument
there, and the low word of its value argument at the paired
word 4F49:0C34. It returns DS:60EB sign-extended as a
double word. It has no callee and no direct store to the
iterator flag at 4F49:000A. With slot indices zero through
523, these table writes remain in 4F49:0C33..1256,
separate from that flag. No slot-index bound is checked here.

Declared MZ operands at `0x0000FC23`,
`0x0000FC42`, `0x0000FC51`, `0x0000FC77`,
`0x0000FC86`, `0x0000FFA5` and `0x0000FFB9`
map raw 3F49, 47E0, 1D40 and 3C0E to 4F49,
57E0, 2D40 and 4C0E as applicable. DS is the data
segment identified by FND-SCRIPT-005.

## Interpretation

The expression lookup is not wholly read-only: metadata at
least ten causes the temporary slot write. The complete local
callee graph contains no opcode dispatch or known flag-clear
call. For the stated ordinary slot range, its explicit stores
are the error word, result pointer, metadata byte and separate
table slot, rather than the iterator flag.

The table slot is itself relevant to the iterator's stored-index
branch (FND-CONFIG-133). If DS:60EB selects the same index
as DS:1A32, this helper writes that branch's table byte and
paired word; no such equality or reachable timing is established
here. A flag-preserving local path therefore need not preserve
all the iterator's inputs.

## Alternatives

FND-CONFIG-153 and FND-CONFIG-154 subsequently identify the
installed metadata source and bounded traversal selector cases.
They do not establish an actual load, valid paired indices or
later field values; those conditions still bound this getter reading.

FND-CONFIG-142 records the initial slot word, guarded setup
assignment and qualified literal-writer inventory. It does not
establish a complete slot range or immutable metadata.
Q-CONFIG-008 retains slot-index and metadata producers,
stride/base/offset tables, selector and record validity, table
aliasing outside the named ordinary range, reachable expressions,
and changes before the rest iterator. The local bodies rule out
a claim that every lookup merely reads a value. They do not
establish that the shipped path reaches a particular write,
that arbitrary indices preserve the flag, or that DS:40BC
is cleared by the failed-index and missing-offset branches.
No gameplay role or complete reader contract is assigned.

## How to reproduce

Read the three complete bodies at the stated bounds. Track
the input guards, pointer arithmetic, low-byte offset request,
full-word metadata index, missing-offset exit, width selection,
DS:40BC assignment, conditional slot write and DS:40C4
continuations. Verify the named MZ operands before applying
load segment 1000. Check the unprefixed conversion at
`0x0000FCDB` rather than interpreting its printed mnemonic
as a wider instruction. Keep 1AA0:0009's return at
`0x0000FD29` distinct from later local functions. Compare
FND-CONFIG-133, FND-CONFIG-138 and FND-CONFIG-139;
retain the slot-range condition when describing flag preservation.
