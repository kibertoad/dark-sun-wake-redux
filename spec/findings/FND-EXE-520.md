---
id: FND-EXE-520
title: The overlay manager loads each overlay's code to offset 0 of the segment its trampolines enter
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0010..4AE5:00D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:029B..4AE5:031B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:03E8..4AE5:0466
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:04C6..4AE5:04F4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:055A..4AE5:061F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0672..4AE5:06B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:06E4..4AE5:0735
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0753..4AE5:0785
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004C880..0x0004C8A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00057570..0x00057580
tool: Capstone 5.0.7 16-bit linear disassembly and xxhash 4.0.1
environment: null
---

## Observation

The installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`) has a 0x5200-byte MZ header and a load
image that ends at file offset `0x57570`, where the 16-byte `FBOV` header
starts. The manager code is in segment `4AE5`. Header fields below are words
of an overlay's 32-byte stub header (FND-EXE-003), addressed through ES.

Startup (`4AE5:0010..4AE5:00D4`) reads the MZ header, seeks past the load
image, checks the `FB` and `OV` signature words and stores the position after
the 16-byte FBOV header in `[0x114]` and `[0x116]`. For this file that
position is `0x57580`. The routine at `4AE5:029B` walks the stub list and, for
each stub it keeps, stores `0x04C6` in header `+0x18` and adds `[0x114]` and
`[0x116]` to the doubleword at header `+4` with `add` and `adc`
(`4AE5:02E5..4AE5:02F8`).

Descriptor 198's stub at `5768:0000` (file `0x4C880`) ships with `+4` equal
to `0x0002FD90`, a code size of 1,468 at `+8`, 34 fixup bytes at `+0x0A` and
5 trampolines at `+0x0C`. With the base added, `+4` is `0x00087310`,
FND-EXE-003's start of descriptor 198's code.

The entry path at `4AE5:05A4` tests header `+0x10`. When it is zero, it calls
`4AE5:055A`, whose last instructions load `[0x120]` and store it in header
`+0x10` (FND-EXE-228), and then calls through header `+0x18`. The routine
there, `4AE5:04C6`, sets SI:DI to the code size plus the fixup size, CX:DX to
header `+6` and `+4`, and AX to header `+0x10`, and calls `4AE5:03E8`. That
routine seeks to CX:DX from the start of the file (service `4200h`), sets DS
to AX and reads with DX zero, so the first byte at the stub's file position
lands at offset 0 of the segment in header `+0x10` (FND-EXE-248). `4AE5:04C6`
then calls the fixup pass at `4AE5:0421` when the fixup size is nonzero. That
pass sets ES to header `+0x10` and rewrites words at offsets inside the
loaded block; it does not change header `+0x10`. Between the store at
`4AE5:059F` and the read, the only other store is to word `0x0E` of the
segment one paragraph below the placed block (`4AE5:05C8..4AE5:05CF`).

After the load, the entry path calls `4AE5:0672`, the trampoline writer of
FND-EXE-227, which takes each trampoline's target segment from header `+0x10`
and keeps its offset. When the compaction routine `4AE5:06E4` moves a loaded
overlay, it stores the new segment in header `+0x10`, copies the code-size
words from offset 0 of the old segment to offset 0 of the new one, and stores
the new segment in the segment word of every trampoline (offset `0x23`,
stride 5). Before those stores it calls `4AE5:075F`, which leaves AX alone
and walks the chain of saved BP values, replacing each saved word equal to
the old segment with the new one.

## Interpretation

Whenever a trampoline sends control into an overlay, CS is header `+0x10` and
CS offset 0 is the first byte of the descriptor's code block, at the file
position its stub names. A return into an overlay that compaction moved
reaches the same offset in the new segment. For descriptor 198, `CS:0x022C`, the table its
dispatch at analysis alias `921E:0135` reads (FND-EXE-173), is file
`0x8753C`. The descriptor-base reading holds, and this settles Q-EXE-010.

## Alternatives

FND-EXE-173's analyzer-alias reading places the table 208 bytes further on,
at `0x8760C`. It would need CS offset 0 to fall 208 bytes before the code
block, and no instruction read here forms such a segment: the read, the
fixup pass, the trampoline writer and the compaction move all use header
`+0x10` with offset 0 at the block's first byte.

The writers of `[0x120]` choose where in the buffer an overlay goes, and the
earlier allocator readings (FND-EXE-226 to FND-EXE-352) concern whether that
placement is valid. Neither changes where offset 0 falls relative to the
code. This reading covers entry through the trampolines. A far transfer into
overlay code that does not go through a trampoline, a store to header `+0x10`
through an aliased segment, and code written at run time are not covered.
The disassembly is linear over the cited ranges; each range starts at an
instruction that a call, jump or stored pointer in the other ranges names.

## How to reproduce

Run `python -I tools/research/exec-census/overlay_entry_cs.py <install
dir>/DSUN.EXE` from the commit that adds this finding, with the locked
evidence Python (Capstone 5.0.7, xxhash 4.0.1). It checks the file's size and
XXH3-128, prints the FBOV header, descriptor 198's stub header as shipped and
with the payload start added, and disassembles the ten manager ranges this
finding cites. Nothing is written or executed.
