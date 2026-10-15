---
id: FND-COMBAT-044
title: The second restore caller carries a failed numbered read into a later region-setup argument
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070F62..0x0007118B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073890..0x000738AD
tool: tools/research/exec-census/overlay_listing.py; installed-build fingerprint and overlay fixup checks
environment: null
---

## Observation

The other same-overlay caller of the numbered SAVE reader in
FND-COMBAT-035 is overlay 187 +1132, exported through 56EF:0093.
It initializes byte locals BP-2 and BP-3 to zero. After earlier
preparation, it calls local +1BE7 and saves AL at BP-1. Two subsequent
calls, local +0FB7 and +08E3, test returned AL; zero takes the shared
diagnostic path at +12E2 rather than reaching the numbered read.
Their effects remain dependencies, not inferred successful preparation.

On the continuation to +1213, it copies its incoming word BP+6 into
DS:140C, pushes that same word as selector and calls +0A6C. It
zero-extends returned AL for the test. Zero sets BP-3 to one and
skips the immediate per-entry postprocessing loop; nonzero leaves
BP-3 zero and enters that loop.

FND-COMBAT-035 reads the numbered reader's selector distinction:
zero requests 61 numbered sources, including the effect row and
count slots; nonzero requests 17. This caller does not force selector
zero. Its own word parameter and actual caller domain must therefore
be admitted before treating it as another complete effect-state load.

### Local post-read work concerns placed entries

On the nonzero-reader branch, DI starts at zero. While bit three of
byte five in the eight-byte entry at DS:67B7 + low-word 8*DI is set,
the caller reads its signed word at six. A nonnegative value s writes
FFFF into current DS:67BB + low-word 37*s + 10 hexadecimal.
Negative values skip that write. DI increments at word width; no
local count/capacity bound terminates that loop.

Both reader-result branches then copy DS:67B7 into a local far
pointer, clear DS:264E, and count entries while each entry's byte-five
bit three is set, advancing the local offset by eight at word width.
This recomputes the placed-entry count; it does not recompute or cap
effect count DS:2100. The caller's own body supplies no ten-byte
effect-row ID/owner/order validation. Earlier and later child effects
and every other state writer remain outside that local statement.

If saved BP-1 is nonzero, the caller invokes +1C26. Zero returned AL
calls the diagnostic entry +0867. This result is not stored over BP-3.
FND-COMBAT-043 describes those local archive helpers and retains
their native/error-continuation dependencies.

### The failure flag is a setup argument, not the function result

After further calls, at +12C5..+12D5 the caller pushes word one,
word zero, an AX whose low byte is loaded from BP-3, and incoming
word BP+6, then calls overlay 182 +15D7 through 56BD:00C0.
The instruction loading AL does not independently clear AH at that
point. Thus BP-3 establishes the low byte of that word argument;
this finding does not assume a zero high byte or a callee byte/word
contract not read here.

That call's returned AL is saved into BP-2. After a further local
call and several state/helper operations, the caller zero-extends
saved BP-2 to test it. Zero invokes formatting and a diagnostic
request; nonzero skips those. The far return at +135A does not
reload BP-2 after those diagnostic calls. Neither the numbered
reader byte nor BP-3 is directly returned by that epilogue.
Native failure/recovery and final register results remain conditional
on the called helpers; no visible fallback behavior is established.

The known fixup-qualified caller is overlay 188 +09F0. It takes its
first word parameter into DI, compares it with DS:140C and, only
when different, forwards that word and its next two word arguments
to 56EF:0093 at +0A05. This local branch supplies no numeric range
check on the selector. It is one qualified caller, not a complete
computed/aliased incoming-reference census.

## Interpretation

This answers Q-COMBAT-009's second local numbered-reader caller,
selector producer and immediate failure/postprocessing routes.
Unlike +0EBE in FND-COMBAT-043, it preserves a separate reader-failure
flag for later setup instead of directly substituting cleanup's result
for a saved reader byte. The effect-state admission question still
requires actual selector domains, complete callees and writers,
archive/resource identity, destination backing and valid restored state.
No original function was executed and no gameplay was implemented.

## Alternatives

- Treating both callers as unconditional complete effect loads ignores
  this caller's incoming selector and the reader's 17-source branch.
- Treating BP-3 as a guaranteed word value one ignores the AL-only
  load into the outgoing AX argument.
- Treating recomputed DS:264E as effect-count validation confuses the
  placed-entry count with DS:2100.
- Treating the final return as the numbered-reader status ignores the
  separate setup result, subsequent calls and diagnostic branches.

## How to reproduce

Use the locked evidence environment and supported DSUN.EXE, size
634416 and XXH3 e296af55ba2ecde7e77f555c90f33d0b.
Read overlay_listing.py descriptor 187, 70F62..7118B, and descriptor
188, 73890..738AD. Bounds are hexadecimal and half-open. Follow
the three byte locals separately, the incoming selector's DS:140C
store and push, both post-read loops, the AL-only outgoing argument
load and the epilogue's saved-result test. Compare the reader bounds
with FND-COMBAT-035 and archive helpers with FND-COMBAT-043.
Retain all native/callee and actual-state dependencies. No original
bytes, decoded listings or saved game content are committed.
