---
id: FND-EXE-180
title: A multiplex-result far pointer is stored as two words and consumed by the overlay-memory candidates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0F3D..4AE5:0F64
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0FD6..4AE5:0FE0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:10AF..4AE5:10B9
tool: Ghidra 12.1.3 PUBLIC and fingerprinted local interpreter source
environment: null
---

## Observation

The resident candidate requests interrupt `2Fh` with AX `0x4300`, then
compares the returned AL byte with `0x80`. The unequal arm leaves this
admission path. The equal arm requests the same interrupt with AX `0x4310`,
stores BX into the DS-relative word at offset `0x0043`, and stores ES into
the adjacent word at offset `0x0045`. It clears AX and calls through the
far pointer at offset `0x0043`. After that call it checks AH against two;
the next admitted call sets AH to eight and again uses the stored pointer.

The pointer is a two-word offset/segment pair, not just the first word.
Later tests at `4AE5:0FD6` and `4AE5:10AF` combine both words by OR before
their conditional branches. These tests distinguish a pair whose two words
are both zero, not proof that an arbitrary nonzero pair is valid.

The saved listing additionally shows consumers through offset `0x0043` at
`4AE5:0F55`, `4AE5:0F60`, `4AE5:0F6D`, `4AE5:0F7B`, `4AE5:0FA5`,
`4AE5:0FAF`, `4AE5:10CB`, `4AE5:10D9`, `4AE5:1172`, `4AE5:117C` and
`4AE5:1220`. This is a positive list of decoded consumers, not an exhaustive
use or writer inventory. DS binding and preservation between the producer
and each consumer are unresolved, as are external pointer mutation and effects.

## Interpretation

SRC-DOSBOX-GOG-0742 identifies the matching request pair and register results
as an XMS provider interface in the accompanying interpreter source. This
supports an external extended-memory-provider interpretation of the target,
not a native game function inferred from an analyzer label. Source-to-shipped-
binary correspondence, provider configuration, live target identity, interrupt
preservation, pointer writers and callee effects remain open under Q-EXE-001
and Q-EXE-010. No complete-reading declaration follows.

## Alternatives

Reading the pointer as a single offset is ruled out by the segment-word store
and the far call. Resolving the live target solely from the source archive is
unsupported: it describes an external provider, not the shipped interpreter's
verified code. The possibility of later mutation or a different admitted
provider remains until the actual target and every relevant writer are read.

## How to reproduce

Use the saved original resident project with analysis disabled and read-only
mode, whose installed source XXH3-128 is
`e296af55ba2ecde7e77f555c90f33d0b`. Run ReportInstructionWindow at
`4AE5:0F3D`, count 52; restrict the producer and initial-call observation to
`4AE5:0F3D..4AE5:0F64`. Run it at `4AE5:0FD6`, count seven, and
`4AE5:10AF`, count six, restricting each two-word test to its cited range.

Run ReportInstructionText with literal tokens `[0x43]` and `[0x45]` over
that unchanged resident listing. The positive consumers above come from this
bounded rendered-operand search; it excludes symbolic/computed address forms
and undecoded bytes and supplies no absence claim. Keep the result cap of 256
visible. Inspect the additional consumers with ReportInstructionWindow at
`4AE5:1155`, count 22, and ReportInstructionContext at `4AE5:1220`.

Read the fingerprinted xms.cpp member named by SRC-DOSBOX-GOG-0742 for the
two multiplex requests and callback dispatch. Do not substitute that source
contract for a shipped-binary correspondence check or native observation.
Reports and configurations stay in the licensed local store, outside Git.
