---
id: FND-CONFIG-132
title: Overlay 204 computes grouped-call thresholds in a first pass and consumes them in a second pass
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0020
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5691:0061
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0776
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV trampoline and fixup mapping
environment: null
---

## Observation

Overlay 204 entry 5787:0020 starts at file offset
`0x0008CA7A`. Its refusal-message branch exits when the stored
combat-state word is nonzero; the separate state-one announcement
branch continues into processing (FND-CONFIG-049). The state-one
check calls the getter in FND-CONFIG-117.

Locals BP-06, BP-08 and BP-0A begin at zero. A first pass uses
resident 2D40:0776 with sentinel 9999 to obtain an index in SI,
repeating until it returns that sentinel. The caller's third word
must be 32766 or equal to the current index to process it. For that
selected 49-byte record, first argument one or record byte offset
20 equal to two or three assigns that byte one. Only byte one
continues to the value queries.

The entry queries helper 5691:0061 with the current index and
codes one through four. It compares returned low bytes signed,
choosing between pairs and then between their selected results,
and re-queries selected codes on the chosen branches. The final
byte is sign-extended; when strictly greater than BP-06, it replaces
that local and captures SI in BP-0C. Codes five through eight use
the same comparison shape to update BP-08 and capture SI in DI.
Code eleven's sign-extended byte separately updates BP-0A and
captures SI in BP-0E when strictly greater. Equal values do not
replace the captured index.

Helper 5691:0061 begins at `0x00061CAC` and returns at
`0x00061D2D`. It follows the first argument through a three-byte
reference, a 49-byte record and a 66-byte record. It scans exactly
three bytes starting at offset 27 for the supplied code byte. A
match returns the byte at the same ordinal starting at offset 30.
No match returns one for code twelve and zero for other codes.
This helper calls no other routine and has no stored-data write;
its internal index and loop control use registers only.

After the first pass returns sentinel 9999, the caller starts a
second pass through the same resident helper. Each returned index
reaches `0x0008CD25`, calls another helper with a local output
pointer, and conditionally calls 2D40:06E1 when BP-0A is at least
ten. It then uses the first-pass BP-06 and BP-08 values and captured
indices for the seven grouped calls in FND-CONFIG-119. The
first-pass third-word filter is not repeated in this second-pass
block. Later record updates and helper calls precede iteration's
next call at `0x0008D040`.

## Interpretation

The grouped-call thresholds are accumulated before the call groups
execute. With the directly read value helper and unchanged source
records between its repeated reads, the signed-byte comparisons
select the greatest value in each four-code group; strict local
updates retain the first index reaching an equal greatest value.
The threshold comparison still starts from zero, so nonpositive
returned bytes do not establish a captured index. Positive thresholds
used by the groups therefore have a preceding index assignment.

The second pass consumes those accumulated values for each index
it returns, rather than computing them anew for that index. This
establishes data provenance and ordering, not a completed rest rule,
record meaning or live feedback outcome.

## Alternatives

The resident iterator's contract and producers, earlier setup helper
effects, allowable input records, second-pass helper effects and
termination remain unread (Q-CONFIG-008). The helper's absence of
writes does not prove concurrent or intervening source state is
unchanged. Record fields and code bytes keep their neutral meanings;
this finding does not infer a class, power or gameplay statistic.

## How to reproduce

Read entry setup and first-pass filter at
`0x0008CA7A..0x0008CB29`. Track the signed-byte comparison and
re-query blocks through `0x0008CD25`, then second-pass entry and
threshold consumption through `0x0008CDD0`. Check its iterator
continuation at `0x0008D03F..0x0008D051`. Read the complete
bounded value helper `0x00061CAC..0x00061D2E`; verify its three
scan positions, first-match return, code-twelve default and lack of
stored-data writes. Resolve segment operands through their declared
FBOV fixups rather than labelling shifted descriptor indices as
mapped segment addresses. Compare FND-CONFIG-119's call groups
and FND-CONFIG-077's already-recorded incoming routes.
