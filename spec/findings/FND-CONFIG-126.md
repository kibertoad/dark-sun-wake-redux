---
id: FND-CONFIG-126
title: Overlay 211 supplies two zero-gate selector calls with stored and selection-table codes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57C1:005C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57C1:0061
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 573B:0089
tool: Python 3.14.7 and Capstone 5.0.7 bounded disassembly; verified MZ relocation and FBOV fixup segment mapping
environment: null
---

## Observation

This entry corrects the raw segment labels in FND-CONFIG-107.
Overlay 211 fixups at 0x000966EA, 0x00096708, 0x0009745A and 0x0009746E decode descriptor 110 to mapped segment 4E71.
The bounded control-flow description is retained with mapped addresses.

Overlay 211's declared calls at `0x00096711` and
`0x0009747E` enter selector 573B:0089 (FND-CONFIG-101).
The containing exported entries 57C1:005C and 57C1:0061 begin at
`0x0009662B` and `0x00096F04` respectively. No preceding return
separates either call from its entry. Both pass zero for byte gate
BP+1A and for the following byte argument.

The 005C call passes the word at `4E71:0B44` as its first word,
the caller's second word minus 11300 as its second, and DS:43F5
as its third word, the selector code. Its local branch requires
DS:43F7 nonzero and unsigned word BP+18 below eight, then clears
DS:43F7. It passes a far pointer to local byte BP-5, initialized
zero at entry. An intervening local helper result supplies the fourth
word argument; this does not replace the separately pushed code.

The 0061 call passes `4E71:0B44` as both first and second words,
and SI as the third word. Its explicit code-producing paths use
selection index `second_word_argument - 11213`, stored in DI.
The entry requires signed DS:9D79 positive and DI signed below that
count; these tests alone do not impose a lower bound on DI.

A signed byte at DS:(43E8 + selected_word), where selected_word is
the word at `4E71:0B44`, selects setup branches. Values one and two reach a table-word read at
`0x0009700B`; value three reaches another at `0x000970DF`.
Both reads select DS:9C20 plus twice DI. The value-three path adds
235 to SI with word arithmetic at `0x000971CC`; the first read's
paths join later handling without that addition. Other state-byte
values jump to that later handling without either explicit SI
initialization in these blocks.

Immediately before the second selector argument construction, a
helper receives SI and returns a record pointer whose byte offset
12 must be zero or seven. The call passes a far pointer to local
byte BP-2, initialized zero at entry. Earlier helper effects,
other event branches and possible writes through local pointers
remain outside this reading.

## Interpretation

Both invocations satisfy the selector's zero-byte gate. The code
must still compare signed in 235..268 to enter overlay 176
(FND-CONFIG-100). This requires DS:43F5 in that interval at the
first call. For the second call's value-three setup path, word
addition maps table values zero through 33 to that interval. Its
value-one and value-two paths pass the table word without that
addition, so those paths instead require 235..268 directly.

## Alternatives

Stored word and table producers, allowable state-byte values,
upstream event inputs, remaining guards and helper effects remain
open (Q-CONFIG-008). The branch that joins handling without an
explicit SI initialization does not establish a valid code there.
The local byte's initial zero does not prove its later pointed value
or successful feedback-window setup. No physical player action or
visible message is established by these calls alone.

## How to reproduce

Resolve 57C1:005C and 0061 with FMT-EXE-002 through
FMT-EXE-004. Inspect the first call's guard and argument block at
`0x000966B1..0x00096716`. For the second, read the entry's bounds
and state dispatch at `0x00096F04..0x00096F5D`, the value-one
branch through `0x00097007`, the first table-read paths at
`0x00097007..0x000970D7`, and the value-three path at
`0x000970D7..0x000971D0`. Track all explicit SI writes through
the second call, including its addition; do not infer the code from
the nearest table read alone. Inspect the returned-record guard and
argument block at `0x00097412..0x00097483`, comparing parameter
slots with FND-CONFIG-100.

For the segment-label correction, select each named operand from the
MZ relocation list or its overlay's declared FBOV fixup list. Apply the
recorded load segment to MZ operands; decode an overlay operand's shifted
descriptor index and resolve its segment-table entry before labelling an
address. Raw segment operands and mapped addresses are different forms.
