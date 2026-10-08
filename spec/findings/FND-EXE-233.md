---
id: FND-EXE-233
title: Shared loader helper publishes before copying and conditionally rewrites trampoline segments
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:06E4..4AE5:0735
tool: Ghidra 12.1.3 PUBLIC, p-code segment-output reporter and executable-reader 2.5.0
environment: null
---

## Observation

The shared callee in FND-EXE-228 and FND-EXE-232 has 37 instructions and
81 bytes, ending exclusively at `4AE5:0735`. It returns near at
`4AE5:0734` without extra argument cleanup. Its sole call, reached on one
tail path, is to `4AE5:075F`; the bounded CFG assumes it returns.

It loads AX from current DS-relative word `0x0120`, reads the old segment
word from current ES-relative word `0x0010` into DX, and immediately stores
AX into that ES-relative word. Thus publication precedes the copy below.
It reads current ES-relative word `0x0008` into CX, increments at sixteen-bit
width and logically shifts right by one. For an unsigned input w, the copy
count is `((w + 1) modulo 65536) >> 1`, ranging from zero to 32767 words.
Input 65535 therefore gives zero, rather than 32768 words.

The unsigned comparison of new AX against old DX selects direction. A new
word below the old word uses zero SI and clears direction; otherwise SI
becomes twice the decremented count at sixteen-bit width and direction is
set. DI receives SI. The helper saves DS and ES, loads DS from old DX and
ES from new AX, then repeats word copies from DS:SI to ES:DI with that count.
The copy uses the selected direction and clears direction afterwards.
With count zero, REP performs no word copies even though the backward-path
starting offset can wrap to `0xFFFE`. Ordering by segment words alone does
not establish non-overlapping physical storage or bounds of the buffers.

Next AX is decremented at sixteen-bit width and loaded into DS. Saved ES
is restored and written to this new DS-relative word `0x000E`. AX is then
incremented, and saved DS is restored. These stores and restores are ordered
after the copy; equality of source, destination, header and stack storage
has not been excluded.

The tail tests current ES-relative byte `0x0020` against the interrupt
instruction opcode `0xCD`. Equality skips to the return. Otherwise it calls
`4AE5:075F`, reloads CX from current ES-relative word `0x000C`, sets DI to
`0x0023` and clears direction. Each iteration stores AX as a word through
ES:DI, then advances DI by three more bytes and uses sixteen-bit LOOP.
The net offset advance is five modulo 65536. There is no count-zero guard
before the first store: a zero reload gives 65536 iterations if accesses
complete. The post-call AX, ES and count-source effects require the callee
reading; the pre-call values cannot simply be assumed to survive.

Decoded p-code positively identifies DS outputs at `4AE5:0709`,
`4AE5:0711`, `4AE5:0719` and ES outputs at `4AE5:070B`, `4AE5:0713`.
These establish segment-load directions where displayed MOV operands are
reversed. They do not establish live segment contents or storage identity.

## Interpretation

This reads the shared helper's explicit publication, copy, header store and
conditional rewrite order, including independent copy and output counts.
It does not prove successful copying, alias-free buffers, safe output extent,
preservation of caller stack words or admissible trampoline count. In
particular, bounded instruction traversal is not a bound on the stores.

Q-EXE-001 and Q-EXE-010 retain native caller/state/header admission, field
writers, physical aliases, source/destination and stack bounds, the tail
callee's effects, interrupts and remaining publisher callees. No allocation
contract, complete_reading or replacement inventory is established.

## Alternatives

An unbounded rounded-up copy count is contradicted by the increment's
sixteen-bit wrap. Publication only after successful copying is contradicted
by the earlier segment-field store. A zero trampoline count producing zero
stores is contradicted by the store-before-LOOP order. Rendered segment-MOV
operands do not override positive register-output semantics.

## How to reproduce

At revision `1135d25`, use FND-EXE-226's original-source region, hash and
default x86-bounds limits. Set entry and sole entries value to `0x00040734`
(`4AE5:06E4`) with no seeds or summaries. Check all 81 covered bytes, the
37 instructions and the sole returning-call assumption.

In the resident Ghidra snapshot, read-only with analysis disabled, run
ReportInstructionWindow at `4AE5:06E4`, count 28, then at `4AE5:071A`, count
nine, and inspect the return at `4AE5:0734`. Restrict claims to the interval
above. Run tools/ghidra/ReportSegmentWrites.java at revision `1135d25` with
names DS and ES separately and limit 10000; check the positive outputs
above. FND-EXE-231 and FND-EXE-229 record the corresponding scan totals and
opaque-output limitations. No negative or preservation claim follows.
Sources, listings and reports remain in GAME_DIR.
