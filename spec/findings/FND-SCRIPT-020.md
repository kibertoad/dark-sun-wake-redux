---
id: FND-SCRIPT-020
title: A guarded interpreter helper clears four supplied working buffers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:31ED
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

The complete near helper 172C:31ED occupies
`0x0000F6AD..0x0000F71F`, ending with near return at
`0x0000F71E`. It tests byte 4C0E:0000 against one.
Any other value returns without a fill call. Equal one
calls resident 1000:3FA2 four times with fill byte zero:

| Far-pointer field in 4C13 | Requested byte count |
|---|---:|
| 033B | 9 |
| 0337 | 64 |
| 0333 | 40 |
| 032F | 420 |

Declared MZ relocations map the state segments and fill
callee before those fields are assigned. FND-CONFIG-144
reads the complete fill body: it clears direction and
writes the stated count from the supplied far pointer,
with no further callee. This helper has no own pointer
validation, allocation or interrupt request.

FND-CONFIG-156 reads an initializer that assigns these
four pointer fields from allocations of those same sizes,
then writes one to 4C0E:0000 before returning status two.
Under successful, valid and unchanged allocations, these
fills stay in those working buffers. That conditional
allocation provenance is not a universal non-aliasing or
capacity invariant for every invocation.

FND-SCRIPT-007's script entry calls this helper after its
status-two check and before its direct frame/status resets.
FND-SCRIPT-019's loader calls it only after the stop and
current-pair early returns are bypassed. Neither caller
passes a numeric argument to this near helper.

The helper makes no direct store to the cached current
number, selector, cache arrays or archive-state fields.
Its indirect fills can affect other state when their
pointers alias it; no valid-pointer assumption is silently
substituted for that possibility.

## Interpretation

The previously unread helper has a bounded buffer-reset
contract and one byte gate. It does not itself select an
archive or allocate script-buffer space. This narrows both
script-entry and loader effects, while preserving pointer
validity, aliasing and later state as dependencies. The
function keeps its neutral name; no broader variable or
region-reset rule is established here.

## Alternatives

Q-SCRIPT-003 and Q-SCRIPT-005 retain pointer producers,
other reset paths, semantic buffer identities and timing.
One reading uses the initializer's distinct allocated buffers;
another reaches changed or aliased pointers. Their field
provenance and intervening writes distinguish them. The
local gate alone does not prove a safe or effective reset.
Q-CONFIG-008 retains the same dependencies at the named
script-99 call in FND-CONFIG-160.

## How to reproduce

Read 31ED from its entry through the near return. Verify
the byte comparison, each declared segment relocation,
far-pointer field and packed count/fill argument. Resolve
1000:3FA2 using FND-CONFIG-144. Compare the field assignments
and gate producer in FND-CONFIG-156, then the call order
in FND-SCRIPT-007 and FND-SCRIPT-019. State the allocation,
capacity and non-aliasing conditions separately from the
helper's direct effects.
