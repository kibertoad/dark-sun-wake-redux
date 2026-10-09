---
id: FND-EXE-351
title: Fill-loop pointer helper normalizes a stored pair and preserves the chunk register
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0583..1000:05C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:05E5..1000:060B
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-299 calls 1000:0583 with DX=SS, AX the address of its
local pointer, BX the saved chunk and CX=0000. The helper pops
the near-return offset into ES, pushes CS and then that offset, and
loads ES from incoming DX. It exchanges BX and AX, reads the stored
pointer's segment word at ES:BX+02 into DX, pushes the pointer's
storage offset BX, then reads its offset word at ES:BX into BX.
AX now holds the incoming low delta and CX its high word.

It tests CX as signed. Nonnegative CX selects addition: add AX to BX;
an unsigned carry adds 1000 to DX. It takes the low four bits of the
incoming high delta word into a segment contribution shifted left twelve,
adds that contribution to DX, then adds the wrapped BX shifted right
four. It keeps only the wrapped offset's low nibble in AX. All segment
additions are word-width, with no overflow rejection.

Negative CX first forms the two-word negation of CX:AX using complement,
low-word plus one and carry into the high word. It reaches the shared
subtraction tail at 05E5: subtract the negated low word from BX, subtract
1000 from DX on borrow, subtract the negated high word's low-nibble
segment contribution, then add wrapped BX shifted right four to DX.
It again retains only the offset's low nibble in AX. Segment operations
remain word-width and no native address-range test occurs.

Both paths pop the saved storage offset into BX, write AX to ES:BX
and DX to ES:BX+02 in that order, and far-return through the constructed
CS/offset pair. The extra CS word is consumed by that far return, giving
the caller the ordinary near-call stack depth. ES remains the incoming
storage segment, BX the storage offset, AX/DX the normalized pointer
and CX a modified scratch value. Neither path writes SI or BP, calls
another procedure or contains an interrupt instruction.

## Interpretation

For FND-EXE-299's caller, the storage accesses are SS-relative accesses
to its local pointer, rather than accesses through the destination segment
stored in that pointer. A positive chunk at most FA00 selects addition.
Under unchanged local storage, the result has an offset from zero through
fifteen and a word-wrapped segment; the helper normalizes the encoded pair,
not an unrestricted linear address with guaranteed writable extent.

FND-EXE-299's byte-fill helper also does not write SI. Therefore the
ordinary local call sequence retains chunk SI for the remaining-count
subtraction. This closes that register-preservation obligation without
establishing asynchronous preservation, valid destination storage or forward
fill direction. The pointer stores occur low word before high word and
can overlap other state if storage admission or aliases permit it.

Q-EXE-010 retains the outer incoming path and argument ranges, original
direction-flag provenance, native segment/extent admission, aliases and
lifetime. Other callers of this helper, including its negative-delta path,
are not inventoried here. No complete-reading promotion or initialized
storage claim follows.

## Alternatives

Treating ES as the destination buffer segment ignores its incoming DX
storage binding. Treating normalization as checked address arithmetic ignores
word-width segment wrap. Treating the far return as consuming an incoming
far-call frame ignores the locally constructed CS word. Treating scratch
CX changes as loss of the saved chunk ignores unchanged SI and the
caller's explicit use of SI for the subtraction.

## How to reproduce

At revision 18f03bf require FND-EXE-176's installed DSUN.EXE identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode shipped
half-open 0x00005783..0x000057C7 at IP 0583 and
0x000057E5..0x0000580B at IP 05E5, modeled CS 1000, with
locked Capstone 5.0.7 in sixteen-bit mode. Follow the negative branch
into the shared tail without attributing the intervening sibling entry's
prologue to this call. Track the constructed return frame, the initial
BX/AX exchange, ES storage binding, carry/borrow branches, both ordered
stores and unchanged SI. Compare FND-EXE-299's local caller and fill
callee. Source bytes and reports remain outside Git. No original execution
or complete caller search is claimed.
