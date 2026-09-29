---
id: FND-CONFIG-172
title: Child cleanup distinguishes recursive error propagation from local error origins
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3BA6:0EC2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3BA6:049F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3CFA:0544
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:0AFB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:12A4
tool: Python 3.14.7 and Capstone 5.0.7 complete local 16-bit entry readings and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-168's MENU branch calls 3BA6:0EC2 and
returns FFFF if that call returns nonzero AX. The complete
local 0EC2 span is `0x00031B22..0x00031B8D`. It saves
SI and has a stack-limit guard through 1000:2E48, then
starts a word index at zero. It re-reads the unsigned
word at supplied object+20 before each iteration and
continues only while that count exceeds the index.

Each iteration passes the object, index and word one
to local far 049F. It dereferences the returned pointer
without an own null check. If that record's double word
at +12 is zero, it skips the recursive call and increments
the index. Otherwise it recursively calls 0EC2 with the
record's far pointer at +16. Returned AX FFFF immediately
returns FFFF. Any other result passes the current record's
pointer at +16 to 444C:0092, then increments the index.
The common finished-scan path returns zero. There is no
other local FFFF source or nonzero normal return value.

Thus the encoded failure return only propagates FFFF
from the same routine. On a finite valid traversal with
ordinary balanced returns, leaves return zero and the
recursive continuations do likewise. The existence of
the propagation branch alone does not prove an originating
MENU error, including FND-CONFIG-168's conditional exit.
Cycles, changing counts, invalid pointers, stack-guard
outcomes and native graph reachability remain separate;
no termination or successful-release claim is inferred.

The complete 3BA6:049F span is
`0x000310FF..0x000311AA`. After its guard, it compares
the object's unsigned word at +20 with the supplied word
index. An index greater than that word returns a null
pointer; equality is accepted. It starts at the object's
segment with wrapped offset object+25, then follows a
record's word length by adding it to that offset. This
walk uses signed index comparisons. Indices with the
high bit set differ from the unsigned initial check.
Word one as its third argument returns the selected
record pointer without entering its text-copy branch.
Both named callers here supply one. The other branch
copies a byte-counted field to a local buffer and invokes
1000:37B6; its callers and bounds are not established
by this selected-path finding.

The APFM and BUTN paths and the final target cleanup in
FND-CONFIG-168 call 3CFA:0544 on addresses inside the
object. The complete body at `0x000326E4..0x00032726`
has the same stack-limit guard, then separately tests
far fields at supplied address+24 and +2C. Each nonzero
field is passed to 444C:0092 in that order. It does not
locally clear either field or test the wrapper's returned
result, and returns zero. There is no own initial null
test before dereferencing the supplied address.

The EBOX path calls 409B:0AFB, complete local span
`0x000366AB..0x000367D3`. After its guard, a null object
returns FFFF. A nonnull object equal to current DS:A17F
first calls local far 12A4 with that pointer. Nonzero AX
returns FFFF before the following field operations.
Other cases process object far fields +6A, +98, +9C,
+A0 and +A4 in order. Each nonzero field is passed to
444C:0092 and then locally cleared when that call returns.
The wrapper's result is not checked. A zero field skips
both call and clear.

Next it calls 3CFA:0544 with object+1A, preserving the
supplied segment and wrapping the offset addition. Its
nonzero-result branch returns FFFF, but the normal local
0544 body returns zero as read above. The continuing
path clears current DS:A17F and A183 if A17F still equals
the object, then returns zero. FND-CONFIG-168's caller
does not inspect EBOX's AX; after its return, the common
continuation still passes the child object to 444C:0092.
A returning EBOX result is therefore not a local guard
on that subsequent request.

The complete 409B:12A4 span is
`0x00036E54..0x00036EB0`. It reads object word +96.
Bit 8000 clear returns zero without the later clears.
Bit set reads word current DS:A179. Value one calls
local far 1675 with the object; nonzero AX returns FFFF
without the following local clears. Other cases clear
word DS:A179, far fields DS:A17F/A183 and bit 8000
in object+96, then return zero. FND-CONFIG-174
subsequently bounds 1675's local
effects and common zero return; its complete external
effects and producer conditions remain open. All external
segment operands above were verified through declared
MZ relocations; DS-relative fields mean current DS.

## Interpretation

The cleanup callees have different local contracts.
MENU propagates an error value without a local leaf
origin; 0544 requests two pointer operations without
own field clears; EBOX has an early null return and
encoded result exits, but its caller
still requests the child pointer operation afterward.
FND-CONFIG-165 and FND-CONFIG-167 bound that wrapper's
returned zero and discarded runtime result. None of
these normal-return contracts proves successful release
or absence of aliasing, stale fields or prior changes.

## Alternatives

FND-CONFIG-174 subsequently reads 1675's common zero return, with all
nested call results discarded. Under valid ordinary returns this supplies
no local nonzero origin to 12A4 or nonnull 0AFB; the latter's null-input
FFFF and transitive state/return dependencies remain separate.

Q-CONFIG-008 and Q-SCRIPT-003 retain valid graph/count
and record-length producers, actual index ranges, aliases,
all callers, 1675's transitive effects, runtime guards, DS/index state
preservation and native outcomes. One reading supplies
finite valid MENU traversal and zero leaf results;
another has invalid, changing or cyclic graph state.
The latter is not established as a native input, and
its existence would not by itself supply a FFFF origin.
The local propagated-error branch cannot distinguish it.

One EBOX reading bypasses 12A4 or returns zero; another
changes state or fails to return inside a transitive callee.
FND-CONFIG-174 rules out a locally originating nonzero
1675 result on ordinary balanced returns. Complete
callee and producer evidence is still needed for state
and outcomes. The
conditional encoded FFFF paths are not observations of
failed cleanup or visible dismissal. Resident cases
remain in Q-SCRIPT-007 after supported layouts and the
harness exist; none were run here.

## How to reproduce

Read 3BA6:0EC2 through 0F2C, following every iteration,
recursive return and final zero. Trace FFFF to its only
recursive producer and distinguish propagation from origin.
Read 049F through 0549, keeping the named third-argument
one bypass separate from its other callers. Check the
unsigned range gate, equality, signed walk and word-length
advancement. Read 3CFA:0544 through 0585, 409B:0AFB
through 0C22 and 12A4 through 12FF. Verify declared MZ
operands and ordered pointer operations/clears. Compare
which results FND-CONFIG-168 consumes or ignores, retaining
1675's transitive effects and actual input/return conditions.
