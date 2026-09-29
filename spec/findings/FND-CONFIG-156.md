---
id: FND-CONFIG-156
title: The post-setup initializer's failure word survives its caller's low-byte zero check
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 565C:0020
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00C0
tool: Python 3.14.7 and Capstone 5.0.7 entry-based 16-bit instruction checks and declared FBOV fixup mapping
environment: null
---

## Observation

FND-CONFIG-149's post-setup call to 565C:0020 resolves through
descriptor 169 to code target zero, file offset `0x00057580`.
Its local body ends with far return at `0x000576F3`.
The caller supplies word 200 and double word 10000. The
callee reads the first word at BP+6 and only the next word at
BP+8; it does not read the supplied double word's high half.
The second value is zero-extended from that word when stored
at 4C13:0313.

At entry, a signed-positive word at 4C0E:000B bypasses the
initialization and is returned in AX. Otherwise the body sets
words 4C0E:0009 and 000B to minus one and requests a buffer
whose length is formed by multiplying the first argument
by 13 in word arithmetic, then zero-extending the product. For the supplied 200, this is 2600 bytes. The
returned far pointer is stored at 57E0:40C0.

The near allocation helper, code target 0552,
`0x00057AD2..0x00057B1B`, initializes its local result to
zero. It calls 444C:00FA only when byte 4C0C:0000 is zero,
passing the requested double-word length. A zero returned
pointer sets that byte to one. Any nonzero incoming byte
suppresses the allocation and leaves the local result zero.
The helper returns the local far pointer. The byte's
loaded-image value is zero at `0x000412C0`; that is not an
invariant about its value at this invocation.

The near check at code target 0600,
`0x00057B80..0x00057B94`, returns AL zero exactly when the
same byte equals one, and AL one otherwise. Immediately
after the first allocation, a zero check result takes
`0x000575D5`: AX is set to FFFF and the routine returns
without its later calls. On this nonpositive-status entry
path, an incoming failure byte of one, or a zero allocation
result that changes it to one,
reaches a full-word minus-one return.

When the first check permits continuation, the local link
loop writes word i+1 at offset 11 of each 13-byte record,
for i below the unsigned first argument. It then overwrites
the last record's link with FFFF. For argument 200, record
199's link is therefore FFFF at this point. A zero first
argument would still reach the later decrement and final
write; this body has no local positive-argument guard. The
named caller supplies 200, so that edge is not its supplied
case. Buffer validity is required for these writes.

The remainder requests four buffers of lengths 101, 400,
80 and 420, checks the same failure byte, sets the status
word to one and calls local targets 059B and 023B. Target
059B uses the fill helper in FND-CONFIG-144 to clear those
four buffers. Target 023B, `0x000577BB..0x000579A2`,
requests buffers of lengths 300, 9, 64, 40, 420 and 32,
and then the stored 4C13:0313 value plus one. Its checked
continuation clears working arrays and changes the status
word to two. Its allocation-check failure returns FFFF.
These are local effects, not proof of successful allocations.

After target 023B returns, the outer body stores its AX in
4C0E:000B and calls 172C:000C with word 99 and double word
00020000 at `0x000576BA..0x000576C7`. That callee's
transitive effects are not read here. The outer body then
tests 4C0E:0009 against one. When it differs, it sets
4C0E:000B to FFFF and calls cleanup target 0174, which
sets 000B to zero and 0009 to FFFF after its local release
calls. The outer return reloads the status word; it is
not normalized to a boolean. External allocator, release
and script-call outcomes remain outside this finding.

The caller 5702:00C0, `0x0007330C..0x00073331`, stores
only returned AL in DS:193D and returns that byte. The
following check in FND-CONFIG-149 zero-extends AL and
requests termination only when it is zero. FFFF therefore
becomes stored FF and bypasses that zero branch. No signed
negative or full-word failure comparison occurs there.
A nonzero DS:193D also bypasses future calls to 565C:0020.

Declared fixups resolve the local state segments to 4C0C,
4C0E, 4C13 and 57E0, the fill callee to 1000, allocation
and release to 444C, and the later callee to 172C. The
entry has no direct call to the archive-open entry
38FF:0066 or its wrapper 56BD:002F. This does not exclude
archive effects through the unread 172C call or other
external callees.

## Interpretation

DS:193D is a retained low-byte result, not a demonstrated
success flag. A concrete local allocation-failure path
produces FF that passes the following nonzero check. The
memory setup cannot be treated as an archive registration,
and passing that check cannot be treated as proof that all
its buffers exist. This records the code's branch contract;
it does not establish a live allocation failure or a bug's
player-visible consequence.

## Alternatives

Q-CONFIG-008 retains the failure-byte producers, buffer
validity, external callee effects and later changes to
DS:193D. One reading reaches the ordinary allocation and
status-two path; another reaches the early FFFF return and
still passes the caller's zero check. Actual inputs and
allocation outcomes distinguish them. Merely calling the
stored byte a success flag would discard the second path.
The incoming signed-positive status bypass also remains
state-dependent; its full word need not equal its low byte.

FND-CONFIG-157 locates an earlier OBJEX.GFF open attempt.
It does not prove that the archive survives every intervening
callee or that the later metadata requests succeed.

## How to reproduce

Validate descriptor 169's overlay flag, CD3F header,
trampoline, code and fixup bounds. Resolve trampoline 0020
to target zero and read the entry through its far return.
Follow near helpers 0552, 0600 and 059B and the explicit
push-CS/near-call edges to far-return bodies 023B, 0174,
0422 and 0536. Resolve declared fixups before assigning
state or callee segments. Check the entry's BP-relative
operand sizes, the allocator byte comparisons, first failure
return, final-link overwrite and status reload. Compare
the caller's AL store and subsequent zero check separately
from the full AX result. Keep the unread external effects
and actual memory and operating-system outcomes conditional.
