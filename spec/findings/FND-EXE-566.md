---
id: FND-EXE-566
title: The overlay manager keeps loaded overlays in a buffer as a queue linked through header word 0x1C, and never uses word 0x1E
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:00F6..4AE5:0126
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:031B..4AE5:03DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:055A..4AE5:05A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:05E9..4AE5:061A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0637..4AE5:0672
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:06E4..4AE5:0753
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0785..4AE5:07AD
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations applied for a load image at segment 0x1000 (tools/research/exec-census/overlay_queue.py)
environment: null
---

## Observation

In the installed `DSUN.EXE`, with DS `55CE` (FND-EXE-560) unless a step sets it. Paragraph
counts are in 16-byte units.

- Buffer setup, in the startup routine at `4AE5:00F6..4AE5:0126`. The far routine's word
  argument at `BP+0x0C` plus 1 goes to `[0x124]` and `[0x120]`, and the word at `BP+0x0A` to
  `[0x126]`. After the descriptor walk (FND-EXE-561), when `[0x126]` minus `[0x124]` is below
  `[0x11A]`, the routine returns `0xFFFB`. Otherwise it stores that difference shifted right by
  2 in `[0x118]` and calls `4AE5:031B`.
- Preload, `4AE5:031B`. It takes the first kept header from `[0x122]` (`55DF:0012`, the start of
  the `+0x12` list) and stores it in `[0x12C]`. With DS set to each kept header in turn, it
  forms the paragraphs from this header's `payload_offset` to the next kept header's. While the
  running total from `[0x124]` stays at or below `[0x126]`, it stores the header's position in
  the buffer in `+0x10` and the next kept header in `+0x1C`. It stops at the first header that
  does not fit, or at the last kept header, whose size it cannot form, and stores 0 in `+0x1C`
  of the last header it placed (of the first header when none fits). The total goes to
  `[0x120]`. When it is above `[0x124]`, one read (`4AE5:03E8`, DOS `42h` then
  `3Fh` in chunks of up to `0xFFF0` bytes) loads that many bytes from the first header's
  `payload_offset` to segment `[0x124]`. Then, for each header from `[0x12C]` through `+0x1C`,
  it applies the fixups when `fixup_size` is not 0 (`4AE5:0421`), writes the jump form of every
  trampoline when there are any (`4AE5:0693`), stores the header segment at word `0x0E` of the
  paragraph before the code and calls the far pointer at `[0x86]` with AX `0xFFFF`.
- Helpers. `4AE5:07A1` returns `(code_size + 0x11) >> 4`. `4AE5:0785` returns, with no carry,
  the head's `+0x10` minus `[0x120]`; when `[0x12C]` is 0 or that subtraction borrows, it
  returns `[0x126]` minus `[0x120]` with carry set.
- Append, `4AE5:0735`. It adds the overlay's paragraphs to `[0x120]`, then, from DS `55DF`
  (whose word `0x1C` is `55CE:012C`), follows word `0x1C` to the header whose word is 0, stores
  ES there and stores 0 in ES's `+0x1C`.
- Move, `4AE5:06E4`. It sets `+0x10` to `[0x120]`, copies `(code_size + 1) / 2` words from the
  old segment, backwards when the new one is higher, stores the header segment before the code,
  and when the first trampoline is not `CD` rewrites every trampoline's segment word
  (`+0x23` onward, step 5) after `4AE5:075F` (FND-EXE-562).
- Allocation, `4AE5:055A`, from the entry at `4AE5:05A4` (FND-EXE-562), with the paragraphs
  needed in DX. It adds 1 to `[0x12A]`. While the space from `4AE5:0785` is smaller than DX, it
  first calls `4AE5:0637` when that space came with carry, then takes the head out of the queue
  (`[0x12C]` gets its `+0x1C`). When the head's byte `+0x1B` is 0, it unloads it (`4AE5:061F`:
  trap form, the cache hook, `+0x10` set to 0). Otherwise it subtracts 1 from `+0x1B`, moves the
  overlay to `[0x120]` and appends it. When the space is enough, it stores `[0x120]` in the new
  overlay's `+0x10`.
- Wrap-around, `4AE5:0637`. It counts the queue, sets `[0x12C]` to 0 and `[0x120]` to `[0x126]`,
  then takes the headers last first. Each goes back to the head of the queue, `[0x120]` drops by
  its paragraphs and `4AE5:06E4` moves it there. At the end `[0x120]` is `[0x124]`.
- Probation walk, `4AE5:05E9`, after an overlay has been loaded or found loaded. Starting with
  the space from `4AE5:0785`, it walks the queue from `[0x12C]` while a header has a next one and
  the space is below `[0x118]`. Each header whose `+0x1B` is 0 gets the trap form of its
  trampolines (`4AE5:06B1`), and its paragraphs are added to the space. A header with a nonzero
  `+0x1B` adds nothing.
- Header word `0x1E`. A linear disassembly of `4AE5:0010..4AE5:1258` ends exactly at `0x1258`.
  Its only operands `0x1C` are the ten listed above (`035C`, `0366`, `03D0`, `056F`, `05F1`,
  `0640`, `0656`, `0742`, `0749`, `074D`); no operand is `0x1E`. A byte search of the same range
  for every encoding that can hold either value as a displacement or direct offset finds those
  ten, two short jumps for `0x1C`, and for `0x1E` only bytes that are a `push ds` opcode, a jump
  displacement or the modrm byte of `mov bx, [0]`.

## Interpretation

The manager holds loaded overlays in one buffer, from paragraph `[0x124]` to `[0x126]`. The
loaded overlays form a queue in buffer order, oldest first. Its head is `[0x12C]` and header
word `+0x1C` links each overlay to the next. `[0x120]` is where the next overlay goes. When the
end leaves no room, every loaded overlay is slid to the top of the buffer and placement starts
again from the bottom. To make room, the manager evicts from the head. An overlay whose count at
`+0x1B` is not 0 gets another pass: it is moved to the free end and requeued with the count one
lower. At startup the first kept overlays that fit, in file order, are loaded with one read.

Overlays near the head, within a quarter of the buffer (`[0x118]`) of being evicted, are put
back in the trap form. A call to one of them goes through the `INT 3Fh` handler, which finds it
loaded and sets `+0x1B` to 1 (FND-EXE-562), so an overlay in use survives its next turn at the
head. The manager does not use header word `0x1E`.

## Alternatives

The callers of the startup routine, and so the buffer bounds the game passes, were not read.
Neither were what `[0x11C]` and `[0x12A]` count, nor the routine at `[0x86]`. The search for
word `0x1E` covers the manager's code. Code elsewhere that sets a segment register to an overlay
header and uses offset `0x1E` would not be found.

## How to reproduce

Run `python -I tools/research/exec-census/overlay_queue.py <install dir>/DSUN.EXE` from the commit
that adds this finding, with the locked evidence Python. It checks the file's size and XXH3-128,
applies the MZ relocations, disassembles the seven ranges above, lists the linear sweep's
operands `0x1C` and `0x1E` with where the sweep ends, and prints the instruction holding each
byte-search hit. The ten `0x1C` operands are the search's positive control.
