---
id: FND-EXE-529
title: Game first-priority startup target selects a quantity and branches to resident failure code
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0D27..4AE5:0D82
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:0D26..4AD6:0D81
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-510's first-priority far target pushes DS, SI and DI.
It loads DS from an immediate segment word and calls near 029B
installed or 029A on disc. After that call it reads DS:011A into
BX, loads DS and ES from two more segment immediates, then compares
BX unsigned with ES word 3570 installed or 34E4 on disc.
BX below that word is replaced with a fresh read of it; otherwise
BX is shifted left once at word width. Both paths increment BX at
word width without carry checks. This is not uniformly a maximum followed
by doubling: the below branch does not double the selected word.

It pushes BX, multiplies AX=0010 unsigned by BX, pushes product
high DX then low AX and calls a direct far target at offset 15A5.
It removes six argument bytes into BX with three pops, leaving the
original held BX in that register if the frame remains intact and the
callee performs no incoming cleanup. It combines returned AX and DX
with OR into CX. A zero pair branches to its failure transfer.

A nonzero pair increments returned DX at word width, adds that DX
to BX at word width, then pushes DX, BX, zero and zero. It
pushes CS and calls near 0010 installed or 000F on disc, thereby
manufacturing a far return. Nonzero returned AX takes the same failure
transfer. Zero executes pops into DI, SI and DS, then returns far.
The four pushed argument words are not locally removed before those pops;
their removal and the saved-register/return-frame contract depend on the
unread callee. No unconditional preservation contract is established here.

The failure transfer is a direct far jump to offset 02AD, rather
than a local error return. It does not locally undo quantity formation,
callee effects or the preceding saves. Its target body remains unread.
There is no interrupt instruction in the bounded target body itself.

The MZ relocation records that establish the loaded segment operands are:

| Operand | Installed index and location | Disc index and location | Shipped segment word, installed / disc |
| --- | --- | --- | --- |
| First DS immediate | 4440, 3AE5:0D2B | 4427, 3AD6:0D2A | 45CE / 45BF |
| Second DS immediate | 4441, 3AE5:0D37 | 4428, 3AD6:0D36 | 47E0 / 47D7 |
| ES immediate | 4442, 3AE5:0D3C | 4429, 3AD6:0D3B | 47E0 / 47D7 |
| Far call segment | 4443, 3AE5:0D5C | 4430, 3AD6:0D5B | 0000 / 0000 |
| Failure-jump segment | 4444, 3AE5:0D80 | 4431, 3AD6:0D7F | 0000 / 0000 |

With modeled load segment 1000, these immediates yield DS 55CE
installed or 55BF on disc first, then DS/ES 57E0 or 57D7.
The far call becomes 1000:15A5 and the failure transfer 1000:02AD.
The local manufactured far call retains this target's own CS, 4AE5
or 4AD6. These identities come from the relocation records, not from
assuming the shipped segment words are already loaded addresses.

## Interpretation

This identifies the earlier startup target's actual local branches, segment
operands and callee dependencies before the recorded priority-two initialization.
Q-EXE-007 retains 029B/029A, 15A5, 0010/000F and 02AD,
their input/state producers, return and cleanup contracts, actual segment
admission, writable aliases and broader launch coverage. No complete startup
reading or launch exclusion is claimed from the absence of a local interrupt.

## Alternatives

Uniformly doubling a maximum contradicts the below branch's direct replacement.
Treating the success pops as a complete preservation proof ignores the four
argument words and unread callee cleanup. Treating zero shipped segments as
null call targets ignores relocation. Calling the failure transfer a returned
error ignores the far jump and lack of local restoration.

## How to reproduce

At revision 7111316 require both DSUN.EXE identities from FND-EXE-350.
Use header size 5200 and modeled load segment 1000. Decode shipped
40D77..40DD2 installed and 40C86..40CE1 on disc in sixteen-bit mode;
these are relative segments 3AE5/3AD6 at the offsets above. Read the
MZ relocation table from header word 0018, with count at 0006 and
four-byte offset/segment records; check the five indices and operand words
listed above. Track the two quantity branches, MUL pair, three cleanup
pops, zero-pair test, manufactured far return, four argument pushes and
failure jump. Use FND-EXE-510 for the table consumer. Keep the unread
callees and stack contract explicit. Licensed bytes remain outside Git;
no original process, DOSBox or emulated call runs.
