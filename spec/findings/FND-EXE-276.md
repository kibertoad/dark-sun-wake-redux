---
id: FND-EXE-276
title: Fallback request callee guards a candidate before returning the saved state pair
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1831..1000:18BC
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The near callee establishes BP and reserves eight local stack bytes. It
loads DS-relative word 0xA4 into AX, clears DX, puts four into CL and calls
1000:0562. It adds DS-relative word 0xA2 to AX with carry into DX, then
adds the low and high argument words at SS-relative BP plus four and six
to AX and DX with carry. It compares DX to fifteen using signed less and
greater branches. The equal case compares AX unsigned to 0xFFFF, with
the less-or-equal branch admitting every word value. Rejection sets both
DX and AX to 0xFFFF and takes the common return suffix. No final carry
test occurs before the signed high-word comparison.

The admitted path reloads DS-relative words 0xA4 and 0xA2 into DX and AX,
and the high and low argument words into CX and BX, then calls 1000:060B.
It stores returned DX and AX into SS-relative locals BP minus two and
four. It loads DS-relative words 0xA0 and 0x9E into CX and BX and calls
1000:07F0. A carry-set return rejects. Otherwise it loads DS-relative
words 0xA8 and 0xA6 into CX and BX, reloads the saved candidate into
DX and AX, and calls 1000:07F0 again. An unsigned-above return rejects.
These branches consume returned flags; comparison semantics and register
preservation are not established without the helper bodies.

The remaining path saves the then-current DS-relative word 0xA4 into
SS-relative local BP minus six, and word 0xA2 into local BP minus eight.
It pushes the candidate high and low words and calls 1000:177C. It tests
returned AX as a whole word: zero rejects, nonzero reloads DX from local
BP minus six and AX from local BP minus eight. Thus the ordinary success
result is the pair saved before this final call, not its returned pair.

The common suffix resets SP to BP, restores BP and near-returns. There
is no local argument removal after the final call; resetting SP discards
remaining local and outgoing stack storage on ordinary completion. The
callee's actual argument cleanup remains unread. No local rollback call
occurs on rejection. Every absolute state access uses the DS in force at
that access; earlier callees' preservation cannot be assumed.

## Interpretation

FND-EXE-275 supplies low and high request words in the stack positions
this body reads. The fallback's success path consumes a saved state pair
and its failure test matches the all-ones AX published here. The signed
high-word admission and unread arithmetic helper prevent treating the
initial guard as a proven unsigned byte-address bound.

Q-EXE-001 and Q-EXE-010 retain the four helper contracts, every writer of
the six state words, segment preservation, stack cleanup, physical aliases
and failure effects. The final helper could change state before returning
zero; the local rejection suffix does not show otherwise. No initialized
allocation extent, contiguous growth or complete reading is established.

## Alternatives

Treating the final helper's returned DX:AX as the success result ignores
the two stack-local reloads. Treating the high-word guard as unsigned
ignores its signed branches. Treating the two flag consumers as comparisons
of known numeric layouts assumes unread helper semantics. A zero final
helper result does not prove that the pre-call state remains unchanged.

## How to reproduce

At revision 27d62a3, statically read installed DSUN.EXE and require the
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176.
With the locked Python interpreter, xxhash 4.0.1 and Capstone 5.0.7 in
sixteen-bit x86 mode, decode shipped half-open range
0x00006A31..0x00006ABC at initial IP 0x1831. The MZ header is 0x5200
and the model load segment is 0x1000. Follow signed and unsigned guards,
each flag consumer, every local's last writer, and both final-call result
paths. Keep helper effects unresolved and qualify absolute accesses by DS
and BP-relative accesses by SS. Cross-check FND-EXE-275's argument pushes.
No original execution or caller-completeness claim is made; source bytes
and analysis output stay outside Git.
