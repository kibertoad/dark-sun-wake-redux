---
id: FND-CONFIG-106
title: Overlay 189 and 213 selector calls have literal nonzero feedback gates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5713:004D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5713:0039
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57CE:0048
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 573B:0089
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV trampoline mapping
environment: null
---

## Observation

Three declared selector calls in FND-CONFIG-101 supply a literal one
for byte argument BP+1A and zero for the following byte argument:

| Overlay | Exported entry | Entry file offset | Selector call file offset | Third word argument, the code |
|---:|---|---|---|---|
| 189 | 5713:004D | `0x00074F09` | `0x0007511A` | Word at DS:43F5 |
| 189 | 5713:0039 | `0x00077C1A` | `0x00077CCA` | Word offset 16 in a 23-byte selected record, decremented by one |
| 213 | 57CE:0048 | `0x000995AF` | `0x00099CAF` | Local word BP-4 |

Each entry has its own prologue, with no preceding return between the
entry and call. Each call constructs the gate explicitly in its argument
block: `0x000750F4..0x0007511A`,
`0x00077CB6..0x00077CCA`, and `0x00099C8A..0x00099CAF`.
Each passes a far pointer to a local byte initialized to zero earlier
in its containing entry.

At the first overlay 189 branch, DS:43F7 must be nonzero and the
unsigned word argument at BP+18 below eight. The branch clears that
byte before its helper and selector calls. At the second, the record
word must be nonzero before decrement; a nonzero helper result and a
signed decremented code of 319 or greater skip this selector call.
The remaining branch tests a returned record's low two bits and, in
one branch, the selected record's signed byte at offset 22.

The overlay 213 branch distinguishes local code plus one equal to
326: that path writes a record word and bypasses the selector.
The alternative uses the literal nonzero gate above. Its earlier
selection and event guards remain outside this reading.

## Interpretation

The selector's direct overlay 176 branch requires a zero byte gate
(FND-CONFIG-100). These three invocations therefore cannot take it,
regardless of whether their code lies in 235..268. A zero pointed
local byte does not cancel the separate nonzero gate argument.

## Alternatives

Alternative selector callees can have other feedback effects; this
reading does not exclude them. Code producers, upstream registration,
events and live state remain open (Q-CONFIG-008). Excluding these
invocations does not exclude computed or unrelocated calls.

## How to reproduce

Resolve the three exported trampolines using FMT-EXE-002 through
FMT-EXE-004. Check function ownership from each entry through its
listed call, then inspect the three bounded argument blocks above.
Read the overlay 189 local guards at `0x000750D5..0x000750F4`
and `0x00077C1A..0x00077CB6`, and overlay 213's bypass at
`0x00099C4D..0x00099C8A`. Compare the argument slots with the
selector's byte gate in FND-CONFIG-100.
