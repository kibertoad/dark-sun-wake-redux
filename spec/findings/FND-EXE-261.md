---
id: FND-EXE-261
title: Direct transfer wrapper derives a physical destination and follows carry without a zero-request bypass
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0BFE..4AE5:0C22
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00040C6F..0x00040C71
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

The candidate at `4AE5:0BFE`, whose offset FND-EXE-256 publishes to
state word `0x013C`, contains twelve instructions and thirty-six bytes.
It first calls FND-EXE-240's size helper at `4AE5:0C22`, then loads AX
from current ES-relative word `0x0010`, sets DX to sixteen and multiplies
unsigned at full DX:AX width. It copies that result to BX/CX. Thus this
request's destination pair is the header segment multiplied by sixteen,
not merely the original segment word with zero as its high component.
Actual header binding and destination admission remain unproved.

It loads the request source AX/DX from ES-relative words `0x0014`/
`0x0016` and calls FND-EXE-259's buffer helper at `4AE5:0C17`.
The SI/DI count comes from the initial size-helper call: for header word
w at offset eight, SI is `(w + 1 modulo 65536) & 65534`, DI zero.
The buffer helper adds one and clears the low bit again. Because this
SI is even and at most 65534, that second rounding leaves it unchanged,
assuming the intervening instructions and native bindings preserve it.
The direct interrupt route then requests SI divided by two as its
sixteen-bit word count, from zero through 32767. The other route uses
the even byte count in its stored request. These are request arithmetic,
not independently established output extents.

Header size 65535 gives zero at the first rounding step and therefore
zero at the second; header size zero likewise gives zero. The wrapper
contains no size-zero test before calling the buffer helper, which itself
has no separate zero-length entry return. No particular native external
zero-count behavior follows from this absence of a bypass.

Immediately after the buffer call, carry clear selects the near return
at `4AE5:0C21` without argument cleanup. Carry set selects a far tail
transfer at `4AE5:0C1C`. Its segment operand at shipped offset
`0x00040C6F` is raw zero with an MZ relocation; at load segment
`0x1000` the target is `1000:02DF`, corresponding to shipped offset
`0x000054DF`. The wrapper has no explicit cleanup or rollback on that
tail path, and does not independently check returned AX. FND-EXE-259's
explicit result/flag paths provide that carry, while external operations
and saved-storage integrity remain obligations.

The stored offset writer in FND-EXE-256 is a positive producer lead,
not proof that a native caller dispatches here. This wrapper differs
from FND-EXE-238's preceding candidate: it does not call the shared
state gate first or make the unresolved preliminary callback. Its actual
incoming frames, current CS/ES and all header writers must be admitted
separately rather than inherited from the neighboring body.

## Interpretation

This connects another concrete count/source/destination request to the
buffer helper's result consumer and verifies its far continuation from
the source relocation. It supplies request bounds under the stated
register/header assumptions, without proving initialized destination
bytes or admitting zero-count behavior. Adjacent wrappers have distinct
callee and input contracts despite sharing the buffer helper.

Q-EXE-001 and Q-EXE-010 retain live dispatch and header producers,
call-frame and effective storage aliases, source/destination bounds,
external output contracts and the far continuation. No complete_reading
or replacement inventory is established.

## Alternatives

A raw segment word as the destination pair is contradicted by the
full-width multiplication and copies. Saturating a maximal word size to
65536 ignores the size helper's sixteen-bit wrap. Treating zero as a
no-op return ignores the unconditional call. Applying the preceding
wrapper's shared gate to this body adds a call absent from its entry.

## How to reproduce

At revision `967deaf`, use FND-EXE-236's original-source identity, region
and default x86-bounds limits. Set entry and sole entries to `0x00040C4E`,
with no seeds or summaries. Check interval `0x00040C4E..0x00040C72`,
twelve instructions, calls at `4AE5:0BFE` and `4AE5:0C17`, and near/far
exits. Both calls are assumed to return; complete CFG is not a Standard
complete reading or output proof.

Decode the interval directly from the shipped source in sixteen-bit
mode with Capstone. Query operand at `0x00040C6F`, sourceKind mz and
targetOffset `0x02DF`, under the same source hash. Compare the two count
rounding steps with FND-EXE-240 and FND-EXE-259, the stored offset with
FND-EXE-256 and the preceding wrapper with FND-EXE-238 independently.
Keep source, configurations and reports in GAME_DIR.
