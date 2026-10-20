---
id: FND-EXE-594
title: The DSUN.EXE rows 3F96:0375, 3F96:0381 and 1BF3:5334 are not routine entries, and 15 walk routines in segment 4842 are one-byte iret targets inside an or bh, 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0371..3F96:03C2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:5300..1BF3:53B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:53B4..1BF3:5814
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:050E..4842:0515
tool: Ghidra 12.1.3 with tools/ghidra/SetFbovTrampolineBodies.java and ApplyDeclaredEntries.java at the commit that adds this finding; tools/research/exec-census/inventory_boundaries.py, direct_callers.py, far_pointer_words.py and resident_listing.py; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

The inventory checked is the committed `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` that FND-EXE-593
checked (snapshots `INST-single-20261010` and `INST-overlay-owned-20261009`).

**3F96:0371.** The bytes from `3F96:0371` decode linearly as one routine that saves `BP`, sets up
an 8-byte frame (`sub sp, 8` at `3F96:0374`, three bytes), compares `SP` with the word at
`0x9C`, skips the far call to `1000:2E48` at `3F96:0381` with the `jb` at `3F96:037F` to
`3F96:0386`, far-calls `39D1:049C` at `3F96:039B` and returns with the `retf` at `3F96:03C1`
(`3F96:0371..3F96:03C2`). The inventory splits it into the rows `3F96:0371` (3 bytes),
`3F96:0375` (12), `3F96:0381` (5) and leaves `3F96:0386..3F96:03C2` to the trampoline rows
`5699:0043` and `56A2:002A` (FND-EXE-593). `3F96:0375` is the second byte of the `sub sp, 8`. No
byte of the inventory's `3F96` rows begins a near call, jump, conditional jump or loop to
`3F96:0371`, `3F96:0375`, `3F96:0381` or `3F96:03C2`; the one to `3F96:0386` is the `jb` at
`3F96:037F`. No far call in the load image or overlay code targets `3F96:0371`, `3F96:0375`,
`3F96:0381`, `3F96:0386` or `3F96:03C2`, and no relocated far pointer names `3F96:0375` or
`3F96:0381`; the control `1000:2E48` finds its far call at `3F96:0381` (file offset
`0x34EE1`) among 252. In a copy of the resident snapshot, removing the functions that run from each overlay
header's trampolines past `INT 3Fh` leaves `3F96:0375` with no flow reference; those decodes were
its only ones.

**1BF3:5334.** The row `1BF3:5334` (1247 bytes) covers `1BF3:5334..1BF3:53B4` and
`1BF3:53B5..1BF3:5814`. The bytes from `1BF3:5300` to `1BF3:53B4` are zero bytes and the words 2
to 0x10, all even, which decode only as `add` instructions with memory operands; at `1BF3:53B4`
the bytes are `push bp`, `mov bp, sp`, `push ds`, `push si`, `push di`, `push cs`, `pop ds`.
No near or far transfer and no 16-bit word of the file targets `1BF3:5334`. The near call at
`1BF3:5D5A`, after `push cs`, targets `1BF3:53B4`, and FND-RNG-058's walk decodes the routine
`1BF3:53B4` as `1BF3:53B4..1BF3:5814`.

**Segment 4842.** Fifteen of the walk's routines, `4842:050F`, `0562`, `0592`, `05BA`, `06C5`,
`0710`, `0774`, `083F`, `0888`, `08A7`, `08CE`, `0987`, `09C8`, `0ABA` and `0C20`, are one byte
each. At each such X the byte is `CF` (`iret`) and is the second byte of the three-byte
`or bh, 0` (`80 CF 00`) at X-1, which is followed by `push cs` at X+2 and a near call to X at
X+3 (`4842:050E..4842:0515` for the first). In the Ghidra view the byte at X decodes only as part
of that `or bh, 0`: creating a function at each X found no instruction start there.

## Interpretation

`3F96:0375` and `3F96:0381` are not routine entries: the first starts inside an instruction and
was reached only by decoding trampoline bytes after `INT 3Fh`, and the second is the prologue's
stack check inside `3F96:0371`, which owns `3F96:0371..3F96:03C2`. `1BF3:5334` is not a routine
entry either: nothing transfers to it, its bytes up to `1BF3:53B4` read as data, and the routine
is `1BF3:53B4`, which a call reaches. Each of the 15 one-byte routines is entered by its near
call after `push cs`, so its `iret` pops the return offset, `CS` and the flags; its byte is also
the operand of the `or bh, 0` that runs before the call, so it shares that byte with the
instruction of the row that holds it.

## Alternatives

- A computed transfer could reach `3F96:0375`, `3F96:0381` or `1BF3:5334`; none of FND-RNG-058's
  resolved transfers does, and no stored word names them.
- `1BF3:5300..1BF3:53B4` may be a table some code reads; no reader was looked for.
- An inventory that lets two rows share a byte could give each `iret` a row of its own; the
  join of the views rejects overlapping bodies, and Ghidra's listing holds one decode per byte.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE` and `<inv>` the inventory above: `inventory_boundaries.py <dsun> <inv> 3F96:0371
3F96:0375 3F96:0381 3F96:0386 3F96:03C2` and `... 1BF3:5334 1BF3:53B4`; `direct_callers.py
<dsun> 3F96:0371 3F96:0375 3F96:0381 3F96:0386 3F96:03C2`, `... 1BF3:5334 1BF3:53B4` and the
control `... 1000:2E48`;
`far_pointer_words.py <dsun> 3F96:0375 3F96:0381 3F96:0371` and `... 1BF3:5334 1BF3:53B4`;
`resident_listing.py <dsun> 3F96:0360..3F96:03D0 1BF3:5300..1BF3:53B6 4842:0500..4842:0522`. The
flow references come from `SetFbovTrampolineBodies.java` (its `TARGET` lines) on a copy of the
`INST-resident` snapshot, and the missing instruction starts from `ApplyDeclaredEntries.java`
(`NOINSTRUCTION`) run on the same copy with the 15 starts declared. Outputs are kept under
`GAME_DIR/analysis/exe-inventory-20261021/restage/`. Nothing was executed.
