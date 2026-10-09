---
id: FND-EXE-567
title: Only the overlay manager reads the segment table, using only segment, flags bit 1 and whether unk_02 is 0, and other code names an overlay header only to reach a trampoline
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0036..4AE5:0040
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0449..4AE5:044E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:023F..277B:026C
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations applied for a load image at segment 0x1000 (tools/research/exec-census/segment_table_readers.py)
environment: null
---

## Observation

In the installed `DSUN.EXE`, after MZ relocation for a load image at segment `0x1000`:

- Nine relocated words become `55CE`, the overlay manager's data segment, whose offsets `0x01A0`
  to `0x08C8` hold the segment table (FND-EXE-561), and three become `55E8`, the table's own
  segment. In segment `4AE5` they are the data word at `4AE5:0005` (FND-EXE-560); `mov ax,
  0x55CE` at `4AE5:01EA`, `4AE5:0235`, `4AE5:027E`, `4AE5:050A` and `4AE5:0D2A`; `mov ax,
  0x55E8` at `4AE5:0449`, which the fixup pass uses to read a descriptor's segment
  (FND-EXE-520); and `mov word ptr [bp+8], 0x55E8` at `4AE5:003B`, which with `[bp+6]` set to
  `0x0728` forms the far pointer `55E8:0728`, the same byte as `55CE:08C8`, just after the table.
  The other four are the segment words of descriptors 160, 161 and 162 (`55CE`) and 167 (`55E8`)
  in the table itself. No fixup word in the overlays' code names one of those four descriptors.
- 268 become the segment of an overlay header. 216 are the segment word of a far call or far
  jump, 50 are descriptors' segment words in the table (the 49 overlays and descriptor 168, whose
  `flags` is 1 and `unk_02` 0 and whose segment is overlay 169's, `565C`), and two are pushes:
  `push 0x5702; push 0xF2; push 8; call far 1000:2C77` at `277B:023F` and the same with `0xF7` and
  `0x0B` at `277B:025F`. Header `5702` holds 48 trampolines, and `0x00F2` and `0x00F7` are the
  starts of the 43rd and 44th.
- In the overlays' code, 2,082 fixup words name an overlay header's descriptor. 1,929 are the
  segment word of a far call or far jump. The other 153 are each `push` of the segment followed
  by `push` of the start of one of that header's trampolines.
- In a linear disassembly of `4AE5:0010..4AE5:1258`, the memory operands based on SI, DI or BX
  at displacement 0, 2, 4 or 6, through a segment other than ES, are string operations on the
  file name and the environment (`4AE5:01DB` to `4AE5:026F`), `[si+4]`, `[si+2]` and `[si]` in
  the descriptor walk (`4AE5:02B8`, `02BF`, `02C8`, FND-EXE-561), `lodsw` over a fixup list and
  `[di]` with DS `55E8` in the fixup pass (`4AE5:0442`, `0453`), `[di+2]` at `4AE5:04B2` with DS
  an overlay header's segment, block copies at `4AE5:070D`, `0A2D` and `0A98` (FND-EXE-563,
  FND-EXE-566), string compares at `4AE5:1018` and `1040`, stack operands through SS, and
  `4AE5:0D89`, a decode of the bytes of `EMMXXXX0` (FND-EXE-563). The walk tests `[si+4]` with
  `test ..., 2`.

## Interpretation

A constant segment in an MZ image is only right at run time when the relocation table lists it,
so outside the overlay manager nothing in the load image or the overlays forms the segment of
the segment table or of the manager's data with a constant: the manager is the only reader of
the table in memory that names it. Code outside the manager names an overlay header's segment
only as a call or jump target or as a far pointer to a trampoline, never to read a header field.
With FND-EXE-566, which finds no use of header word `0x1E` in the manager, no code found reads
or writes that word. The only fields of a descriptor that code reads are `segment`, in the walk
and the fixup pass, bit 1 of `flags` and whether `unk_02` is 0, in the walk. Nothing reads
`unk_06`, the other bits of `flags`, or the value of `unk_02`, so at run time a descriptor that is
not an overlay matters only through its `segment`, which the fixup pass gives to overlay code.

## Alternatives

A segment computed at run time, from another segment by arithmetic or from memory, is not found
by this search. The manager forms header segments that way, from a descriptor or a trampoline's
return address. The pack header's fields are read from the file, not from memory; whether
anything other than the manager's startup reads them from the file (FND-EXE-560) is not covered
here. What `1000:2C77` does with the two trampoline pointers was not read.

## How to reproduce

Run `python -I tools/research/exec-census/segment_table_readers.py <install dir>/DSUN.EXE` from
the commit that adds this finding, with the locked evidence Python. It checks the file, applies
the MZ relocations, lists each relocated word that becomes `55CE` or `55E8` with the instruction
that holds it, searches the overlays' fixups for the descriptors those words name, and sorts
the relocated words that become an overlay header's segment, decoding the two that are neither
far-transfer operands nor table words, and sorts the overlays' fixup words that name a header.
