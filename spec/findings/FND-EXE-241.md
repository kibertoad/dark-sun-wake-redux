---
id: FND-EXE-241
title: Shared gate consumes helper results in order and returns zero AX after its final link traversal
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0C2E..4AE5:0CD5
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-239's gate first saves DS, BP and ES, then saves the low and high
words of its DS-relative pair difference: word `0x0004` minus `0x0008`,
followed by word `0x0006` minus `0x000A` with borrow. Its first size-helper
call returns SI/DI as read in FND-EXE-240. The gate subtracts that pair
from the difference at full low/high width. With no unsigned borrow, it
discards both saved difference words by adding four to SP. With borrow,
it pops the original high word into DI and low word into SI, uses DS-relative
word `0x000C` as BP and invokes the selector. The selector therefore receives
different bounds on those paths; the borrow path initially uses the retained
difference rather than the size-helper result.

On a nonzero first-selector return, AX is loaded into ES. The gate saves
DS-relative word `0x000E` in DX, replaces that word with the selected
ES-relative word `0x000E`, clears the selected word, calls the link helper,
then writes retained DX to the returned ES-relative word `0x000E`. That
helper has no explicit DX writes. These are ordered accesses through
different current segment bindings, not an admitted alias-free list layout.
Both zero and nonzero selector paths then copy DS-relative words zero and
two into words `0x0008` and `0x000A`. They restore saved ES, save it again
and recompute SI/DI with the size helper.

At the common second selector call the gate sets BP to `0x04C6`. A nonzero
returned AX is loaded into ES and its word `0x000E` replaces DS-relative
word `0x000E`; a zero return skips this store. The selector's explicit
instructions leave BX, CX, SI and DI unchanged. Both paths then restore
saved ES and BP, add SI/DI to DS-relative words `0x0008`/`0x000A` with
sixteen-bit ADD followed by ADC, store BX/CX in ES-relative words
`0x0014`/`0x0016`, and store current DS-relative word `0x000C` in
ES-relative word `0x0018`. The second selector's BP literal is therefore
not the source of this final header-word store.

Finally the gate reads ES into DX and calls the link helper at `4AE5:0CC4`.
On that helper's explicit normal return, AX is zero and DX is unchanged
(FND-EXE-240). The gate stores DX in the returned ES-relative word
`0x000E`, loads ES from DX, stores AX in that ES-relative word `0x000E`,
clears carry, restores DS and returns. No explicit instruction after the
helper changes AX. Thus the decoded normal return has AX zero and carry
clear, with ES reloaded from the value retained in DX before the final call.
Successful traversal, valid destinations and interrupt-time preservation
remain unproved.

Relative to gate-entry SP, its own saves use offsets minus two for DS,
minus four for BP and minus six for ES; the retained difference words add
minus eight and minus ten. The borrow path pops those two words; the other
path discards four bytes. Both reach the common restore at minus six,
then pop ES and BP and finally DS before the near return. The borrow path's
extra ES pop/push has net zero adjustment. These are explicit stack-depth
balances assuming calls return as read, not evidence that saved values or
the return address survive potentially aliased memory writes.

## Interpretation

This follows FND-EXE-240's helper results through the caller's branches,
stores and cleanup, distinguishing original difference, recomputed size,
selector result, saved header and final zero return. It narrows caller-result
obligations without establishing the actual state structure or its writers.

Q-EXE-001 and Q-EXE-010 retain native input/header admission, link and field
writers, effective aliases, traversal/output bounds, saved-stack integrity,
interrupt-enabled changes and outer-caller preservation. No complete_reading
or replacement inventory is established.

## Alternatives

Using the size result as both selector bounds ignores the borrow path's
restored difference. Using the second selector's BP literal as the final
header value ignores the later DS-relative reload. A nonzero final AX result
ignores the final link helper's zero return and lack of later AX writes.
Balanced stack adjustments do not establish preserved saved-slot contents.

## How to reproduce

At revision `689080e`, repeat FND-EXE-239's hash-guarded source traversal and
bounded Capstone decoding for file interval `0x00040C7E..0x00040D25`, initial
IP `0x0C2E`, with no seeds or summaries. Use FND-EXE-240's three separate
helper intervals and queries for explicit outputs and register effects.
Track both saved difference words, every SP adjustment and every segment
load in execution order; inspect both common joins and the final call.
Keep all effective-storage and successful-return assumptions conditional.
Sources, configurations and listings remain in GAME_DIR.
