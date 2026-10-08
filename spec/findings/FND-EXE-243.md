---
id: FND-EXE-243
title: Adjacent stack-word consumer starts its second traversal at the first stop without repeating its upper bound
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:07FB..4AE5:0852
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0853..4AE5:0893
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

The source candidate at `4AE5:07FB` covers 151 instruction bytes in the
two intervals above, with 61 instructions, no calls and one far return
at `4AE5:0892` without extra argument cleanup. The one-byte gap at
`4AE5:0852` is not reached by the declared traversal and is not included
in the locations. No explicit interrupt instruction occurs in the candidate.

It saves BP, sets BP from SP, reserves two local bytes and initializes
SS-relative byte BP minus two to zero. It saves DS, loads DS from CS-relative
word five, then saves SI and DI. BX is initialized from this newly formed
BP; AX receives DS-relative word `0x0110` and DX receives word `0x0124`.
The candidate does not initialize CX before using it as an unsigned bound.

The first traversal tests BX against CX before reading SS-relative word
at BX. BX at least CX stops that traversal. Otherwise it reads the word
into SI and logically shifts it right by one. A zero shifted value stops;
a set original low bit skips the rest of that candidate. Continuing paths
double SI at sixteen-bit width into BX and repeat. For an even nonzero link,
it reads SS-relative word BX plus four into DI and rejects DI at least DX.
Otherwise it loads ES from DI and reads SS-relative word BX plus two into
DI. A nonzero value skips the candidate. A zero value is used as the ES
offset for a word comparison against AX. Equality sets the local byte to
one and stores DI at ES-relative DI plus two before continuing. DI is zero
on that store path; this is an explicit zero word store.

At the first stop, a local byte other than one exits. A byte of one enters
the second traversal at the current BX: there is no reset to the original
BP and no BX-versus-CX test before its reads. Thus reaching the first bound
can be followed by an SS-relative read at that stopped BX. The second
traversal again follows shifted link words until a zero shifted value and
skips candidates with the original low bit set. It again rejects the
BX-plus-four word at least DX and loads ES from an accepted segment word.

This time a zero BX-plus-two word skips the candidate. For a nonzero word
it requires ES-relative word zero equal AX and word two equal zero. It
then exchanges DI with ES-relative word two and stores resulting DI into
SS-relative word BX plus two before continuing. Given the immediately
preceding zero test, this explicitly transfers the retained nonzero word
into the ES destination and writes zero to the SS-relative destination,
subject to effective aliases and interrupt-time changes. The word link
and separate word destinations must not be conflated.

Exit pops DI, SI and DS, sets SP from BP, pops BP and returns far. These
explicit depth adjustments do not establish unchanged saved slots or return
storage across the traversal's writes. BP-based local accesses use SS even
after DS is loaded from the state-segment word. Native frame meanings,
segment identities, graph termination and the number of stores remain unread.

## Interpretation

This supplies a separate concrete state-word consumer and its two traversal
contracts. The first bound is not a bound on the second traversal or its
outputs, and the second starts from the first stop rather than restarting.
Its SS-relative writes require storage and caller admission before treating
them as safe changes to any named stack frame.

Q-EXE-001 and Q-EXE-010 retain native callers and incoming CX/frame admission,
state and link writers, effective aliases, saved-stack integrity, independent
traversal/output bounds and interrupt-enabled changes. FND-EXE-242's own
native callers remain unresolved. No complete_reading or inventory replacement
is established.

## Alternatives

Applying the first BX/CX bound to both traversals is contradicted by the
second entry's immediate read. Restarting the second traversal at original
BP adds an assignment not present. Treating BP-based local accesses as DS
state after the segment load ignores their SS default. Balanced cleanup
cannot establish preserved saved values across aliased writes.

## How to reproduce

At revision `c8acf09`, use FND-EXE-236's original-source region, source hash
and default x86-bounds settings, setting entry and sole entries value to
`0x0004084B` (`4AE5:07FB`), with no seeds or summaries. Check both intervals,
the one-byte hole, 151 covered bytes, 61 instructions and sole far return.

Independently hash-check the source against XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Decode file intervals
`0x0004084B..0x000408A2` and `0x000408A3..0x000408E3` using Capstone 5.0.7
in x86 sixteen-bit mode, initial IPs `0x07FB` and `0x0853`. Inspect both
entries, all branches, word transformations, segment loads, stores and
cleanup above. Keep native frame and effective-address interpretations
conditional. Sources, configurations and listings remain in GAME_DIR.
