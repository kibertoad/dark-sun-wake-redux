---
id: FND-ACTOR-011
title: The RDFF request routines use the 37-byte slot records and not the 13-byte records of segment 1695
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:010F..1695:0170
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:07A8..28C9:0840
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2839..28C9:28E3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0E1B..31E0:128C
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The routine at `1695:010F` takes a far pointer to a word, and while the word is not -1 it
multiplies it by 13 and adds it to the far pointer stored at `57E0:40C0`. When the byte at `6` of
the 13-byte record found this way is 0 it calls `1695:07DD`; otherwise it calls itself with a
pointer to the record's offset `0xB`. The resident image holds six near calls to it, all within
segment `1695`.

The code that requests `RDFF`, at `28C9:07A8..28C9:0840`, `28C9:2839..28C9:28E3` and
`31E0:0E1B..31E0:128C` (FND-ACTOR-003, FND-ACTOR-005), multiplies slot numbers by `0x25` and
reads the records at `DS:67BB` and the table at `DS:67B7`. It contains no multiplication by 13,
no reference to `57E0:40C0`, and no call into segment `1695`.

## Interpretation

The `RDFF` requests belong to the table of 37-byte object slots, and not to the chain of 13-byte
records that the routines of segment `1695` walk.

## Alternatives

The two tables could still be linked through code outside these ranges, through a routine
reached by a pointer, or through a value stored in one and read from the other. The earlier
Ghidra analysis compared the same paths through their decompilations and reached the same
result; it also recorded that the `SCMD` loader's direct callers `31E0:1808` and `31E0:2670` use
the 37-byte records, which was not checked here beyond the call to `31E0:2670` at `31E0:115B`
(FND-ACTOR-003).

## How to reproduce

Disassemble the listed ranges with the relocations applied, and search the resident image for
near and far calls to `1695:010F`.
