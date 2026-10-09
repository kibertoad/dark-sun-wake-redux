---
id: FND-EXE-561
title: The overlay manager keeps a descriptor when bit 1 of its flags is set and its second word is not 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:029B..4AE5:031B
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations applied for a load image at segment 0x1000 (tools/research/exec-census/overlay_manager_fields.py)
environment: null
---

## Observation

In the installed `DSUN.EXE`, the routine at `4AE5:029B` runs with DS `55CE` (FND-EXE-560). Its
near callers in `4AE5:0000..4AE5:1293` are `4AE5:0107`, in the startup routine, and `4AE5:0D2F`.

It visits the 8-byte records from `55CE:01A0` while SI is below `0x08C8`: 229 records, the
segment table at `55E8:0000` (FND-EXE-002). It tests the record's word `+4` with `test word
ptr [si+4], 2` and its word `+2` against 0, and skips the record unless bit 1 is set and the
word is not 0. The shipped table has 79 records with flags 0, 87 with 1, 49 with 3 and 14 with 4,
so it keeps the 49 overlay descriptors. Each record's word `+0` is in the MZ relocation table, so
at run time it is the header's absolute segment.

For a kept record it stores that segment in word `+0x12` of the previously kept header (`55DF`
for the first), then reads byte `+0x1A` of the record's header. When that byte is `0xFF`, it
stores 0 in the previous header's `+0x12` instead and goes on. Otherwise it stores `0x04C6` in
header word `+0x18`, adds the payload start from `[0x114]` and `[0x116]` to the doubleword at
`+4` (FND-EXE-520), and keeps in BX the largest result of `4AE5:07AD`, which returns
`(code_size + 0x11) >> 4` plus `(fixup_size + 0x0F) >> 4`. It stores BX plus 2 in `[0x11A]`.

Byte `+0x1A` is 0 in every shipped header. The record's words `+4` (beyond bit 1) and `+6` are
not read by this routine.

## Interpretation

The walk treats bit 1 of `flags` as the overlay mark, together with a nonzero `unk_02`, which for
an overlay descriptor is its header size. Value 3 is the only shipped value with bit 1 set. The
kept headers form a list through word `+0x12`, from `55DF:0012`, and word `+0x18` names the near
routine that loads the overlay. A header whose byte `+0x1A` is `0xFF` would be left out of the
list.

## Alternatives

The walk does not test the whole value 3, so it does not tell bit 0 from the other bits. What
flags 0, 1 and 4 and the words `+2` and `+6` of the other descriptors mean is not decided here:
the walk ignores those records. The fixup pass reads each fixup's descriptor at `55E8` plus the
index times 8 (FND-EXE-520), and may read other records; that was not checked here.

## How to reproduce

Run `python -I tools/research/exec-census/overlay_manager_fields.py <install dir>/DSUN.EXE`
from the commit that adds this finding. It counts the records at `55CE:01A0` by flags, by bit 1
and by a nonzero `+2`, checks that no kept header has `0xFF` at `+0x1A`, disassembles
`4AE5:029B..4AE5:031B`, and lists the near calls to `4AE5:029B`.
