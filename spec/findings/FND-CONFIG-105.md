---
id: FND-CONFIG-105
title: Overlay 174 supplies three zero-byte-gate routes to the feedback selector
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5682:0020
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5682:003E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5682:0048
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 573B:0089
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV trampoline mapping
environment: null
---

## Observation

Three declared calls from overlay 174 enter selector 573B:0089
(FND-CONFIG-101). Each passes zero for its byte gate at BP+1A and
one for the following byte argument. Their containing exported entries
and selector code inputs are:

| Entry | Entry file offset | Selector call file offset | Third word argument, the selector code |
|---|---|---|---|
| 5682:0020 | `0x0005EE00` | `0x0005EE99` | Result of the local helper beginning at `0x0005EEB6` |
| 5682:003E | `0x0005F404` | `0x0005F5A8` | Sign-extended table byte plus 235, with word arithmetic |
| 5682:0048 | `0x0005F64D` | `0x0005F700` | Result of the local helper beginning at `0x0005F5ED` |

The entry at 0020 passes its first three word arguments to the first
helper and retains its returned word. A negative signed result exits
before the selector. A second local helper at `0x0005EF81` receives
the retained result, first argument and two local output-word pointers;
its zero byte result also skips the selector. The selector receives
the first argument unchanged, minus one as its second word, and the
retained result as its third word. A local byte initialized to zero is
passed by far pointer.

The entry at 0048 passes its first word argument to the second code
helper. A result of minus one exits. The same helper at
`0x0005EF81` checks the retained result and fills two local words;
a zero byte result skips the selector. The first three selector words
again are the first argument, minus one and retained result. Its local
byte passed by far pointer also starts at zero. Both entries derive
the selector's four coordinate words from those two local outputs,
scaling each by 16 and adding eight.

The entry at 003E rejects a signed first argument below four. It follows
that argument's record reference through a 49-byte record, then a
66-byte record, reading the byte at the latter's offset 64. Only values
one through eight continue; subtracting one selects a far pointer in
the table at `0410:0003`, with four-byte stride. The caller counts
bytes to the first `0xFF` terminator. A resident helper result at
`0x0005F484`, multiplied by the signed word count and divided by
32768 with signed arithmetic, supplies the offset of the selected byte.
The byte is sign-extended to a word and retained at local BP-2.

Before the selector, this entry requires the coordinate helper at
`0x0005F4F3` to return zero. The signed result of another coordinate
helper must be no greater than the signed result of the helper called
at `0x0005F549`, supplied with the first argument and selected byte
plus 235. The helper called at `0x0005F55C`, supplied with the
selected byte, has its byte result sign-extended and compared unsigned
against word offset two of the 49-byte record; a greater result skips
the selector. The selector then receives the caller's first and second
arguments unchanged, and selected byte plus 235 as its third word.
A local word reset to zero is passed by far pointer. Earlier helper
side effects remain part of each route.

## Interpretation

All three calls satisfy the selector's zero-byte requirement. Its
separate signed code interval 235 through 268 still decides entry into
overlay 176 (FND-CONFIG-100). For 0020 and 0048, this requires their
retained helper result to lie in that interval. For 003E, the addition
maps the sign-extended byte to that interval exactly when the byte is
zero through 33. The caller does not itself establish which values
are present in the selected table or which helper guards pass.

## Alternatives

The two code-producing helpers, table contents and producers, caller
inputs, and guard-helper effects remain unread (Q-CONFIG-008).
The resident selection helper's range is not established here; the
multiplication and division alone do not prove a valid table index.
These local routes do not establish an original-game action, visible
feedback, or successful message-window acquisition. The other six
selector call sites in FND-CONFIG-101 remain unread.

## How to reproduce

Resolve overlay 174's three exported trampolines with FMT-EXE-002
through FMT-EXE-004. Read bounded instruction blocks from each entry
through its selector call and return: `0x0005EE00..0x0005EEB6`,
`0x0005F404..0x0005F5ED`, and `0x0005F64D..0x0005F724`.
Check that no preceding return assigns a call to a neighbouring entry.
Track the code-producing call results at `0x0005EE1C` and
`0x0005F662`, and the selected-byte read and sign extension at
`0x0005F4B2..0x0005F4B9`. Compare each argument construction with
the selector's parameter gate in FND-CONFIG-100. Distinguish immediate
byte-gate values from computed code values and from the later pointed
local state.
