---
id: FND-EXE-278
title: Fallback state updater publishes a new bound on its zero-result path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:177C..1000:17F2
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The near updater establishes BP, saves SI and reads the high argument word
at SS-relative BP plus six. At word width it increments that value,
subtracts DS-relative word 0x90, adds 0x3F and shifts right six. It compares
the result to DS-relative word 0x3908. Equality publishes the high and low
argument words to DS-relative words 0xA4 and 0xA2 respectively and returns
AX one through the common suffix.

Inequality shifts SI left six, adds DS-relative word 0x90 to a copy in AX,
and compares that sum unsigned to DS-relative word 0xA8. If the sum is
above that bound it replaces SI with the bound minus word 0x90, at word
width. It pushes SI and word 0x90, pushes CS and makes a near call to
1000:298E. The pushed CS forms part of the outgoing return frame; that
callee's return and preservation contract remain unread. Two subsequent
pops into CX remove the outgoing argument words on ordinary continuation.

Returned AX is copied to DX and tested against 0xFFFF. Equality shifts
the then-current SI right six into AX, publishes it to DS-relative word
0x3908, and enters the same argument-pair publication and AX-one suffix.
No local reload of SI or DS occurs after the call before these accesses.

Any other returned AX adds that value, held in DX, to then-current
DS-relative word 0x90 and writes the word-width result to DS-relative
word 0xA8. It writes zero to DS-relative word 0xA6 and returns AX zero.
This path does not locally publish the argument pair to words 0xA4/0xA2.
Both suffixes restore saved SI and BP and near-return while removing four
incoming argument bytes. No local rollback call occurs.

## Interpretation

FND-EXE-276's final call is callee-cleaned on ordinary return, resolving
that stack-cleanup obligation. Its zero-result branch can follow two
explicit bound stores in this updater before the outer callee publishes
its all-ones failure pair. Thus failure does not imply unchanged shared
bounds. The requested state pair is published only on the updater's
AX-one paths, apart from effects the unread callee might perform.

The arithmetic is word-width and has no independent carry/borrow
admission checks. The post-call cache publication uses the actual SI and
DS left by the far callee, not proved preserved incoming values.
Q-EXE-001 and Q-EXE-010 retain that callee's identity and contract,
state-word writers, input ranges, aliases and lifetime. No initialized
allocation extent or complete reading follows.

## Alternatives

Treating a zero updater result as leaving every state word unchanged
contradicts its bound publications. Treating 0xFFFF as this updater's
failure result confuses the inner callee's sentinel with the updater's
AX-one continuation. Treating the count rounding as checked arithmetic
ignores the word-width operations. Assuming post-call SI or DS equals
its pre-call value requires the unread preservation contract.

## How to reproduce

At revision 46431db, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. With the
locked Python interpreter, xxhash 4.0.1 and Capstone 5.0.7 in sixteen-bit
x86 mode, decode shipped half-open range 0x0000697C..0x000069F2
at initial IP 0x177C. The MZ header is 0x5200 and model load segment
0x1000. Follow the cache equality, unsigned bound clamp and both inner
result paths; qualify BP-relative accesses by SS and absolute accesses
by the DS then in force. Track the extra CS push, two argument pops and
the final return's four-byte cleanup. Cross-check FND-EXE-276's final
call and zero-result rejection. No original execution, whole-callee
contract or caller-completeness claim is made. Source bytes stay outside Git.
