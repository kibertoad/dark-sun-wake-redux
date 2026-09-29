---
id: FND-CONFIG-150
title: The post-setup initializer assigns selector entries and starting positions after resource-call checks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00B6
tool: Python 3.14.7 and Capstone 5.0.7 complete local 16-bit entry reading; declared FBOV segment mapping
environment: null
---

## Observation

FND-CONFIG-149's post-setup call 5702:00B6 resolves to
overlay 188 code target zero, file offset `0x00072EA0`.
Its complete local body ends with far return at
`0x00073259`. If DS:193C equals one, it skips the
initialization body and reaches the success continuation. Other
values enter the resource calls and direct assignments below.

The first pair of calls to 38FF:05B5 and 38FF:04AB
passes type FNFO and resource identifier one. The first receives
a far pointer to a four-byte SS local. Nonzero returned AX or
a signed local value above 980 takes the failure continuation.
The second receives a far pointer to a local far pointer whose
value is DS:5AF7. A nonzero returned AX also takes failure.
The next pair repeats that structure with resource identifier
two, signed maximum 98 and destination pointer DS:60ED.
This identifies the caller's checked values and offered buffers;
the resource callees' size, copy and failure contracts remain
unread here. It does not establish a FNFO schema.

After both pairs return zero under their local checks, direct
stores assign stride words for slot bytes one through five:

| Slot byte | DS word | Assigned value |
|---|---|---|
| 1 | 6169 | 23 |
| 2 | 616B | 49 |
| 3 | 616D | 66 |
| 4 | 616F | 15 |
| 5 | 6171 | 23 |

The segment operand at `0x00072F4C` is a declared FBOV
fixup naming descriptor 218, mapped 57E0. These ES-relative
stores therefore name the same data segment as the DS fields.

A loop at `0x00072F7B..0x00072FB5` clears sixteen
words at each of five bases: DS:5EED, 5F0F, 5F31,
5F53 and 5F75. These are the selector positions zero through
15 for slot bytes one through five under FND-CONFIG-145's
34-byte stride. Position 16, used there as the starting-position
word, is outside this clearing loop. Direct assignments then
write these selector and starting-position fields:

| Slot byte | Selector positions assigned after the clear | Starting position assigned |
|---|---|---|
| 1 | 0 = 5, 1 = 2, 2 = 4 | 2 at DS:5F0D |
| 2 | 0 = 15, 2 = 16, 3 = 17, 4 = 4 | 4 at DS:5F2F |
| 3 | None | No direct assignment |
| 4 | None | No direct assignment |
| 5 | 1 = 2 | 1 at DS:5F95 |

These stores occupy `0x00072FB5..0x00072FF7`.
The absence of a direct assignment in the last column is not
proof that its earlier or later value is zero. The five initial
image starting positions are recorded separately in FND-CONFIG-145.

The remainder of the complete local entry allocates buffers,
assigns DS:614F-based far-pointer fields, offers additional
resource buffers and builds text pointers. Those callees and
external effects are not read here. On the success continuation,
it assigns one to DS:193C and returns AL one. A local failure
calls the following entry at `0x0007325A`, then returns AL
zero if that call returns. Earlier direct table assignments can
therefore precede a later failure; this reading does not establish
transactional rollback. The failure cleanup remains unread.

The four FNFO-call operands at `0x00072EC9`,
`0x00072EF3`, `0x00072F18` and `0x00072F3F`
are declared FBOV fixups naming descriptor 37, mapped 38FF.
DS:193C is initially zero at `0x0004E93C`. This initial
byte is separate from DS:193D and DS:193E in
FND-CONFIG-149 and from its later success assignment.

## Interpretation

This is a concrete direct producer for the selector fields that
FND-CONFIG-148's literal-base query did not find. It uses
separate per-type field bases instead of the generic 5ECB and
5EEB operands used by the traversal reader. For slot bytes
one, two and five, these assignments change the initial zero
starting position to a positive value before later calls can
use the traversal table. No immutable-zero invariant follows
from the shipped image or the negative literal query.

The caller also offers DS:60ED to a resource call, a concrete
lead for a block producer outside FND-CONFIG-142's selected
literal writes. Proving the bytes written and accepted sizes
requires reading 38FF:04AB and its callees. The local selector
stores do not themselves write the flag at 4F49:000A, and
this does not establish preservation across the unread calls.
No gameplay role is assigned to the slot bytes or selectors.

## Alternatives

FND-CONFIG-151 subsequently reads the resource wrappers' length
and full-transfer paths. FND-CONFIG-152 traces the bounded
transfer request. Record stability, operating-system results and
actual metadata bytes remain unresolved.

Q-CONFIG-008 retains the two resource callees, failure cleanup,
other buffer producers, incoming state, DS:193C transitions,
selector and metadata validity, later table changes and rest-time
inputs. The earlier unchanged-table reading is ruled out for
these fields on the direct assignment path. It may still hold
on a bypassed path, or later changes may replace the assigned
values. Direct stores before an eventual failure and a successful
initialization are distinct states; no rollback or reachability
assumption selects between them.

## How to reproduce

Resolve trampoline 00B6 to code target zero and decode the
complete local entry through `0x00073259`. Follow the
DS:193C bypass, both FNFO call pairs and local signed checks,
stride assignments, sixteen-iteration clearing loop, direct
selector and starting-position assignments, subsequent failure
edges and success continuation. Verify the named declared
fixups before labeling the segments. Compare each separate
base with FND-CONFIG-145's 34-byte reader stride and the
validated negative query in FND-CONFIG-148. Keep offered
resource buffers distinct from proven writes until their callees
are read; retain subsequent external calls as open dependencies.
