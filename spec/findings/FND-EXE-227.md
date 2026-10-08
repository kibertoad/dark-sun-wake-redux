---
id: FND-EXE-227
title: Resident helper rewrites trampoline offsets and segments into far jumps
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:05DB..4AE5:05DE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0672..4AE5:06B1
tool: Ghidra 12.1.3 PUBLIC, executable-reader 2.5.0 and engine 13.6.0
environment: null
---

## Observation

FND-EXE-226's shared direct callee calls `4AE5:0672` at `4AE5:05DB`.
The helper's conditional source traversal has 23 instructions and 63 bytes,
ending exclusively at `4AE5:06B1`. It has near-return exits at
`4AE5:067A` and `4AE5:06B0`, with no additional argument cleanup.
Its one call, at `4AE5:0690` to `4AE5:0753`, is assumed to return;
its effects are not supplied by this traversal.

The helper tests the ES-relative word at offset `0x000C` and returns if
zero. Otherwise it checks the first slot byte at offset `0x0020` and
returns if that is already a far-jump opcode. On the remaining path, a
nonzero ES-relative word at offset two causes the additional call above.
After that optional call, it loads the segment word from current ES offset
`0x0010`, reloads the count from offset `0x000C`, and starts at slot
offset `0x0020` with forward string stores.

For each slot it reads the old target word at slot offset two before any
store overlaps it. It then writes a one-byte far-jump opcode, that saved
target as the two-byte offset, and the loaded segment as the two-byte
segment. The destination advances five bytes. A 16-bit decrement-and-loop
uses the reloaded count. Thus both words of the far target have explicit
sources, and the target offset survives the overlapping rewrite because
it was read first.

The count guard precedes the optional call, but the loop count is loaded
after it. The guard alone therefore does not prove the loop's final count
or destination extent. A zero count at that later load would wrap through
65536 iterations of this loop if every store completes. The call's segment
and field effects have not been read.

## Interpretation

Under the current header binding, this supplies the concrete mechanism
that changes resident trap slots into far jumps and identifies header word
`0x0010` as the segment source for their targets. It advances the native CS
question beyond the stored initial trampoline in FND-EXE-225, but does not
prove which live header or segment is present when the rewrite happens.
Its producer, intervening callee effects, count/storage admission, native
entry and interrupt-enabled state changes remain Q-EXE-001 and Q-EXE-010.
No runtime transfer, complete_reading or inventory replacement is established.

## Alternatives

Reading the old target after the stores would lose part of it; the actual
read-before-write order rules out that interpretation. The initial target
word alone is not the final far pointer: the segment comes from a separate
live header word. The early count test cannot substitute for preservation
through the optional callee and the later count reload.

## How to reproduce

At revision `c068402`, use FND-EXE-226's original-source x86-bounds region,
source hash and default limits, setting entry and sole region entries value
to `0x000406C2` (`4AE5:0672`). Supply no seeds or callee summaries and
retain the call-return assumption. In the original resident Ghidra snapshot,
read-only with automatic analysis disabled, run ReportInstructionWindow at
`4AE5:05A4`, count 45, and `4AE5:0672`, count 36. Restrict this finding to
the direct call at `4AE5:05DB` and the helper interval above; later printed
instructions are separate context. Check the target read before the byte
and word stores, direction clearing and final loop count reload. Source,
configs, listings and reports remain in GAME_DIR and are not committed.
