---
id: FND-CONFIG-136
title: Script opcode dispatch invokes the registered iterator-flag writer before its instruction handler
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:018F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0F84
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:37F3
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; MZ relocation mapping and verified instruction boundaries
environment: null
---

## Observation

FND-CONFIG-135 identifies a zero-gated setup assignment of
2D40:37F3 to DS:02F6/02F8. FND-SCRIPT-005 identifies the
script dispatcher as a consumer of that far pointer. A bounded
reading confirms the following sequence in resident 172C:018F,
file offsets `0x0000C64F..0x0000C681`.

The dispatcher compares its byte argument with 128 unsigned.
Larger bytes skip both the callback and table dispatch and call
the stop routine. For bytes zero through 128, it compares the
entire double word at DS:02F6 with zero. A nonzero pointer
causes the argument byte to be zero-extended to a word and
passed to the indirect far call at `0x0000C667`. A zero pointer
skips that call. Both returning paths then reload the argument
byte from the stack, zero-extend it, and call the local handler
selected by the word table at DS:030A plus twice that byte.
The callback's return value is not used as a replacement opcode.

When this pointer still names 2D40:37F3, the callback therefore
receives the opcode byte as its word argument before the handler.
Its decimal 51 comparison in FND-CONFIG-135 corresponds to
opcode 0x33, and decimal 49 corresponds to opcode 0x31.
Opcode 0x33 reaches the flag-one assignment without the
callback's earlier call branch. Opcode 0x22 passes decimal 34
and reaches the flag-zero assignment without that branch.
These statements concern this registered route; other invocations
of the writer are not classified here.

The shipped table word for opcode 0x22 is 0F84, checked at
file offset `0x0004D34E`. Its handler begins at `0x0000D444`.
It first calls local parameter reader 172C:367B with four,
then forwards two words and two double words from 4C13:00B4,
00B8, 00BC and 00C0 to overlay entry 5787:0066 at
`0x0000D46B`. It widens the returned word into the accumulator
and returns at `0x0000D481`. The far call's segment operand
at `0x0000D46E` is a declared MZ relocation holding raw 4787,
mapped to 5787; the parameter and accumulator segment operands
map raw 3C13 to 4C13. FND-CONFIG-077 identifies request one's
route from that dispatcher to the rest entry.

FND-SCRIPT-010 records that parameter decoding can dispatch
nested instructions. This reading does not inspect its complete
call tree or establish that the iterator flag is preserved while
parameters and later helpers are evaluated. Reloading the original
opcode after the callback only preserves dispatch selection,
not the state written by nested callbacks or handlers.

## Interpretation

The registered flag writer has a concrete script-opcode producer.
The callback runs before the handler, including the opcode 0x22
handler whose request-one branch reaches the rest messages.
On that registered route, the immediate pre-handler assignment
clears the flag. That does not prove a zero flag at the later
iterator call: parameter evaluation and intervening helpers must
also be read. The byte is neither proved permanently zero nor
proved to represent a physical key or pointer event.

## Alternatives

Q-CONFIG-008 retains pointer replacement and indirect or block
writers, setup invocation and timing, parameter evaluation effects,
other callback callers and the writer's input-49 callee effects.
A reading that treats 51 on this route as a keyboard code is ruled
out by the opcode argument producer. A reading that treats the
opcode 0x22 clear as a permanent rest-call invariant remains
unproven because nested or intervening code can change the flag.
Reachable shipped script requests and other rest-entry routes
remain open under FND-CONFIG-077.

## How to reproduce

Read the complete bounded dispatch block at
`0x0000C64F..0x0000C681`, tracking the full-width pointer
comparison, byte argument, indirect call, stack reload and table
selection. Check the exported setup stores and resident writer
in FND-CONFIG-135. Read the stated table word and complete
handler `0x0000D444..0x0000D482`; verify its three named
segment operands against the MZ relocation list before mapping
addresses. Compare FND-SCRIPT-005, FND-SCRIPT-010 and
FND-CONFIG-077 without importing unread dependency effects.

A raw direct-operand search for DS:02F6/02F8 found the known
comparison, indirect call and two setup stores. Local decoding
also produced an overlapping word-width comparison starting one
byte into the real prefixed double-word instruction. Reject that
candidate using the established boundary; it is not another use.
The candidate search does not exclude indirect, block or aliased
pointer changes and is not an exhaustive incoming-call proof.
