---
id: FND-EXE-182
title: Shipped interpreter two-service candidate has distinct argument-consumer leads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00568540..0x00568593
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

A bounded shipped-interpreter candidate reads the word at `0x0075B1E0`
with zero extension and compares it with `0x4300` and `0x4310`. Other
values return zero in EAX. The `0x4300` arm writes byte `0x80` at
`0x0075B1E0` and returns one. The `0x4310` arm reads the dword at
`0x0240D640`, stores its upper word at `0x0075B100`, stores that upper
word shifted left four as a dword at `0x0075B110`, stores its lower word
at `0x0075B1EC`, and returns one.

The original-source bounded traversal covers 77 instruction bytes at
`0x00568540..0x0056855A` and `0x00568560..0x00568593`. Its three near
returns are `0x00568559`, `0x00568585` and `0x00568592`, without extra
argument cleanup. It lists no calls, interrupts or decoding gaps.

The saved listing gives two positive references to the candidate address:
stores of that address as a stack argument at `0x00568E96` and
`0x0069A0E3`. The first is followed by a call at `0x00568E9D` to
`0x004BDB70`; the second by a call at `0x0069A0EA` to `0x004BDB20`.
They are distinct consumer leads, not established callers or exhaustive
reference results.
The input word, output fields, stored dword producer, registration consumer
and other references are not yet read completely.

## Interpretation

The service gates, admission byte and split-pointer output pattern match
SRC-DOSBOX-GOG-0742's external XMS multiplex source contract. This is
bounded binary evidence for FND-EXE-180's provider-correspondence question.
It does not prove that the input and output globals are the guest registers
named in the source, that the callback is registered or enabled, or that the
entire source compiled to the shipped binary. Q-EXE-001 and Q-EXE-010 retain
field producers, registration and dispatch admission, live target identity
and preservation. No complete-reading declaration follows.

## Alternatives

This replaces FND-EXE-181, which incorrectly assigned both argument consumers
to `0x004BDB20`. Bounded instruction context shows the first instead calls
`0x004BDB70`. The two consumers cannot be treated as one registration path.
The service-body observation is retained; no complete reading existed to preserve.

A match of two constants alone would be circumstantial. The additional
byte result and two-word split supply a narrower behavioral correspondence,
but cannot replace producer and consumer reading. The input is a whole
word while the admission arm writes only its low byte; the other byte's
writers and every subsequent whole-word consumer remain obligations.

## How to reproduce

Use the shipped interpreter with XXH3-128
`09861838aa3018346f9f15c9a4f5925c`. Run the committed wrapper's
`x86-bounds` command with sourceKind `pe32`, entry file offset `0x00167940`
(1472832), and one named region start `0x00167940`, exclusive end
`0x00167993` (1472915), entries `[1472832]`. Record the region as a bounded
service-gate candidate with unresolved input/registration producers. Omit
segment/ip and let the reader derive the mapping: the executable section
starts at virtual `0x00401000`, raw offset `0x400`. Supply no seeds or
callee summaries. The two reached intervals remain distinct from their hole.

In the saved shipped-PE project, with analysis disabled and read-only mode,
run ReportInstructionText with tokens `0x4300` and `0x4310`, preserving the
256-match cap; run ReportInstructionWindow at `0x00568540`, count 26.
Restrict the observation to the candidate's reached ranges, excluding the
neighboring routine beginning at `0x005685A0`. Run ReportReferences at
`0x00568540` and ReportInstructionContext at `0x00568E96` and `0x0069A0E3`.
No empty result is used for absence or caller completeness. Compare only
the fingerprinted source contract in SRC-DOSBOX-GOG-0742; preserve its
source-to-binary limits. Rich reports remain outside Git in the local store.
