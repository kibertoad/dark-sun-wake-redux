---
id: FND-EXE-513
title: Game buffer allocation wrapper reads a stack-derived offset through DS and retains its initial link sentinel
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2172..1000:21D2
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:2172..1000:21D2
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-512's far callee 2172 pushes SI and DI, copies current
SP into SI and reads word SI+8 through current DS. This is a
stack-derived numerical offset, not an SS-based read. With admitted equal
DS/SS and intact frame it denotes the caller's quantity argument; without
that admission its producer remains a DS memory word.

Zero returns zero unchanged. Nonzero adds five at word width and returns
zero if that addition carries. Otherwise it clears the low bit and raises
values below eight to eight using an unsigned comparison. Under the equal-
segment argument binding, quantity 0200 produces internal size 0204.
These transformations do not establish the allocator's units or writable extent.

It tests current DS:390C installed or DS:3880 on disc. Zero
calls near 21D2 and returns its AX. Nonzero loads BX from DS:3910
installed or DS:3884 on disc. BX zero calls near 2212 and
returns its AX. Nonzero copies initial BX into DX, then compares
word DS:BX unsigned with internal size and takes the selected-block path
when that word is at least the size. Otherwise it reloads BX from
word DS:BX+6 and compares new BX with DX. Equality calls 2212;
inequality repeats at the block-size comparison without reloading DX.

The comparison sentinel therefore retains the initial head. Returning to that
head stops an insufficient-block traversal, including a self-link there. A
cycle excluding that head does not itself stop the traversal. There is no local iteration
bound, subsequent null-pointer guard or admitted link extent. Writable aliases
and concurrent or callee changes remain unresolved.

For a selected block it copies internal size to SI, adds eight at word
width and compares current block word zero unsigned with that wrapped
threshold. At least the threshold calls near 223B and returns its AX.
Otherwise it calls near 2133, increments current word DS:BX, forms
AX as current BX plus four at word width and returns that AX.
The threshold addition has no local carry check, and the final increment
and pointer formation rely on BX surviving 2133. No result from 2133
is tested before those operations.

Every local path pops DI and SI and returns far without incoming cleanup.
The wrapper contains no interrupt and does not locally alter DS or SS.
The four near callee bodies, shared-state producers, block units and headers,
returned storage extent, segment identity and preservation are not established
by this wrapper. Both editions have matching local control flow with the
distinct globals listed above.

## Interpretation

This resolves the allocation wrapper beneath the diagnostic initializer and
exposes an additional DS/SS obligation before binding its input to the
outgoing quantity. Q-EXE-007 retains 21D2/2212/223B/2133, block
and shared-state writers, actual segments, link admission, aliases and lifetime,
the other initialization helpers and earlier startup target, and broader launch
coverage. No valid allocation or complete allocator contract is claimed.

## Alternatives

Treating SI copied from SP as an SS argument ignores the DS access.
Treating the traversal as universally bounded ignores an off-head cycle or
unadmitted link chain. Treating the split threshold as nonwrapping ignores its unchecked
addition. Treating BX+4 as admitted writable storage ignores the unread
callee, block units, header and extent producers.

## How to reproduce

At revision 16cdc4b require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
7372..73D2 in sixteen-bit mode. Track the default segment of SI+8,
word-width size transforms and carry guard, shared-state branches, DX's
initial assignment, the loop's 219E target, link reload and equality stop.
Review return to the initial head and an off-head insufficient cycle as
static paths. Track the wrapped size-
plus-eight threshold and BX-dependent stores after 2133. Keep actual
segments and storage admission separate. Licensed bytes stay outside Git;
no game process, DOSBox or emulated call runs.
