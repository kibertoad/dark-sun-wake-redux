---
id: FND-EXE-217
title: Decoded host segment-output search leaves initial bases and external preservation unresolved
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00418A82..0x00418A83
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004278DE..0x004278E1
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00427C2E..0x00427C31
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00427CB7..0x00427CB9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00427D00..0x00427D02
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00427D77..0x00427D79
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00427E13..0x00427E15
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00427E80..0x00427E82
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00428647..0x00428649
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00428693..0x00428695
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00428703..0x00428705
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00428773..0x00428775
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00428B40..0x00428B42
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004FBA6F..0x004FBA71
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004FBA8B..0x004FBA8D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004FEEAF..0x004FEEB1
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004FEECB..0x004FEECD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0050CB22..0x0050CB24
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0050CB26..0x0050CB28
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00514E0F..0x00514E11
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5769..0x005F576D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5779..0x005F577C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00696FD9..0x00696FDB
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00696FE8..0x00696FEA
tool: Ghidra 12.1.3 PUBLIC, local p-code segment-output audit and engine 13.6.0 instruction windows
environment: null
---

## Observation

The saved host-processor listing for FND-EXE-011's shipped interpreter
contains 369,192 decoded instructions in the audit domain. A p-code output
search for DS and SS completes with no matching instruction, no output
truncation, 24 instructions containing opaque user operations and 472
instructions with no p-code. The opaque-site list also completes without
truncation. These are properties of the saved decoded listing, not all
possible instruction streams in the shipped bytes or loaded process.

The individually checked opaque sites have these decoded classes:

| Sites | Decoded class |
|---|---|
| `0x00418A82` | One port-input instruction |
| `0x004278DE`, `0x00427C2E` | x87 packed-decimal load and store respectively |
| `0x00427CB7`, `0x00427D00`, `0x00427D77`, `0x00427E13`, `0x00427E80`, `0x00428647`, `0x00428693`, `0x00428703`, `0x00428773`, `0x00428B40`, `0x004FBA6F`, `0x004FBA8B`, `0x004FEEAF`, `0x004FEECB`, `0x0050CB22`, `0x0050CB26`, `0x00514E0F`, `0x00696FD9`, `0x00696FE8` | x87 mathematical operations |
| `0x005F5769`, `0x005F5779` | LOCK-prefixed memory arithmetic, bounded separately in FND-EXE-033 |

None of these decoded instructions is an explicit DS/SS assignment.
This classification does not model their opaque effects, faults, interrupt
handling or external continuations. The empty-p-code instructions are not
classified individually in this finding. No imported library code or
initial descriptor table is included in the scan.

The independently constructed reporter controls distinguish a segment read
from a write. A synthetic DS read does not match; direct DS and SS writes,
a DS stack pop and combined far-pointer loads into DS and SS do match.
An interrupt remains opaque and a NOP is reported as having no p-code.
The output-cap control retains the total match count and reports truncation.
These validate the reporter's modeled effect query; they are not native
controls establishing the shipped process's segment bases.

## Interpretation

Q-EXE-011 now has a controlled decoded segment-output search and concrete
classes for all its opaque sites. It still lacks admission of initial DS/SS
descriptor bases, classification of empty effects and unsearched instruction
streams, and preservation through external code and exceptional paths.
SRC-WIN32-X86-ABI describes an external flat-mode expectation only.
FND-EXE-198's numeric local-frame separation therefore remains conditional
on actual storage identity. No complete_reading declaration follows.

## Alternatives

Treating every opaque operation as a possible explicit segment assignment
ignores the individually checked instruction classes. Conversely, treating
zero modeled outputs as proof that loaded segment state is equal or never
changes ignores initial state, external code, empty effects, undecoded or
overlapping streams and exceptional execution. Synthetic register-write
controls do not supply original descriptor-state evidence.

## How to reproduce

Hash-check the shipped source against XXH3-128
`09861838aa3018346f9f15c9a4f5925c`, size 3,802,624. Use the saved PE
project at FND-EXE-011's preferred base, read-only with analysis disabled.
Run tools/ghidra/ReportSegmentWrites.java from tooling commit d8fa9f2 with
arguments DS+SS and 256. Its domain is every decoded instruction in that
listing, independently of analyzer function ownership. It compares output
register-space byte intervals with the named register intervals and retains
opaque and missing effects separately. Check its completion and both
truncation fields; do not rely solely on the headless exit code.

At each of the 24 location starts above, run ReportInstructionWindow with
instruction count one. Check the complete exclusive span and decoded class,
excluding the following instruction. Verify the reporter controls with
tools/ghidra/Test-SegmentWrites.ps1 in a separate synthetic project. That
test admits only its named fixture and checks the expected bytes before
decoding; it is never applied to a licensed source. Keep rich reports local.
Execute no interpreter or game and infer no native segment state from the
saved analyzer model.
