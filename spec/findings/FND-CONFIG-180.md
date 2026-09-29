---
id: FND-CONFIG-180
title: The bitmap and registration helpers have different result gates and exact callee argument widths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0840
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:01EF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:02AB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:02F8
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete local 16-bit readings and declared FBOV/MZ operand mapping
environment: null
---

## Observation

FND-CONFIG-179's mode helper calls local far 0840 in
its initial word-two path and local far 01EF after its
later two/three gate. Descriptor 182 maps their complete
local file spans to `0x00069090..0x000690DB` and
`0x00068A3F..0x00068A7D` respectively.

0840 first tests current far field DS:0F3B. Nonzero
bypasses its first resource request. Zero passes that
field's far output address, type BMP followed by a space
and double-word number 19000 (4A38 hexadecimal) to
38FF:04AB. Nonzero returned AX exits 0840 before the
second field test. Returned zero, or the first field's
nonnull bypass, tests current far field DS:0F3F. Zero
requests BMP/19003 (4A3B hexadecimal) through the same
wrapper and output-address contract; nonzero skips it.
There is no conditional branch using the second returned
AX. The body does not normalize an overall result; its
caller ignores AX and continues to the following calls.

FND-CONFIG-151 bounds resource allocation before possible
I/O failure. A pointer assigned during a failed first
request can therefore be nonzero at a later invocation,
which would bypass that request and allow the second
field test. This is a conditional state distinction,
not evidence of accepted bitmap contents or native failure.
No direct word DS:0DAB store occurs in 0840.

01EF's input layout is a far window pointer at BP+6,
word identifier at BP+0A, far callback at BP+0C and
word mask at BP+10. It saves SI and loads the identifier
there. Null window skips both setter calls; there is no
own AX normalization on that bypass. Nonnull first
passes the callback, zero-extended identifier and window
to 3F96:02AB. It ignores returned AX. It then passes
operation word one, the mask, a zero-extension of the
post-call SI and the same stacked window to 3F96:02F8.
Its normal return restores SI but does not rewrite the
second setter's AX. Actual SI preservation across the
first setter's guard and lookup remains a condition on
the second identifier being the original value.

The named mode call at code 1A8A through 1AA5 pushes
words 0166, mapped segment 28C9, 0061 and 4B01,
then far field 4E28:00D7, before the call. In the
01EF layout this supplies window, identifier 19201,
callback 28C9:0061 and mask 0166. Grouping the first
two pushes as callback 28C9:0166 is contradicted by
the callee's BP-relative widths and forwarding. Both
callback and mask positions are checked directly, not
inferred from the location of a segment fixup alone.

The complete resident 3F96:02AB span is
`0x00034E0B..0x00034E58`. After a stack-limit guard,
it requests APFM lookup through 39D1:049C, using the
window pointer and low word of its identifier argument.
A null returned pointer returns FFFF. A nonnull pointer
stores the supplied callback double word at record+62
and returns zero. The complete 02F8 span is
`0x00034E58..0x00034ED1`: its separate lookup returns
FFFF on a null pointer. A found record applies operation
one by ORing the mask into word record+58, operation
two by clearing those mask bits, or operation three by
zeroing that word; other operations leave it unchanged.
The found-record continuation returns zero. FND-CONFIG-074
and FND-UI-007 previously bounded these setters; this
reading resolves their named caller's argument grouping.

All overlay segments above were checked through declared
FBOV fixups; resident guard and lookup segments through
MZ relocations. No own mode-word store occurs in 01EF,
but complete lookup, guard, alias and state preservation
remain separate. The two setters perform independent
lookups and the first result does not guard the second.

## Interpretation

The early bitmap helper has a first-result gate absent
from the later three resource requests in FND-CONFIG-179.
The registration wrapper supplies a callback and mask
whose boundaries must be read through its callee. It
continues to its mask request after a returning first
setter regardless of that result. These contracts do not
establish accepted content, a stable frame graph, completed
registration or unchanged DS/mode state in the original.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain window/field and
identifier producers, graph lookup, callback identity and
reachable dispatch, SI/DS and guard effects, other callers,
later writers, archives, aliases and I/O outcomes. One
bitmap reading supplies accepted first bytes and a zero
result; another has a nonzero pointer from an earlier
failed request. The local request order differs without
proving which state occurs natively.

One registration reading finds the same valid APFM in
both lookups with preserved SI; another has a lookup
failure or changed state. Complete producer/callee and
native outcome evidence would distinguish them. The
reading that treats 0166 as the callback offset is ruled
out by the callee layout and preserved only in superseded
FND-CONFIG-169. No native or emulated result is claimed;
Q-SCRIPT-007 cannot execute this overlay.

## How to reproduce

Resolve descriptor 182 and read 0840 through 088A and
01EF through 022C. Follow the first resource result,
nonnull bypass and second request without an overall
result assumption. Map the named 1A8A..1AA5 pushes
into BP+6/+0A/+0C/+10, then follow both setters' exact
operand widths. Read 3F96:02AB through 02F7 and
02F8 through 0370, checking the independent lookups,
callback/mask stores and guard dependencies. Verify all
declared segments and retain post-call SI provenance and
actual acquisition/registration as conditions.
