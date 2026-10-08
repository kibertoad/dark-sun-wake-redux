---
id: FND-EXE-058
title: Ordinary callback classifies a signed stored word and saves matched state on a distinct six-return path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5156..0x005F5267
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5340..0x005F53A6
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

After FND-EXE-165's offset-28 reader returns nonzero, the ordinary path
prepares the sixth original argument, that returned word and a local output
address in registers and calls `0x005F4EA0`. It saves the full return in a
separate local. It then zero-extends a byte from the prepared local region,
passes the sixth argument in an auxiliary register, calls `0x005F4CC0`,
and saves that full result elsewhere. These callees' input consumption,
output writers and semantic roles remain unread.

It calls FND-EXE-056's plus-one accessor with the sixth argument and
immediately decrements the full returned word after removing outgoing stack
space. The two 32-bit adjustments cancel modulo that width, so this local
value equals the offset-four word fetched by that accessor. It saves the
value as a loop counter, initializes separate saved words to zero, and sets
the return status to eight. A negative signed value reaches FND-EXE-165's
cleanup/status-return join. Zero selects classification one. A positive
value enters `0x005F5340`.

Each positive-counter iteration calls `0x005F4D30` twice. The first call
prepares the saved full return from the earlier helper in one register and
the address of frame offset minus 112 in another. It saves the first return
as the next chained input, then calls the same helper with that return and
the address of frame offset minus 116. It saves the second return as the
next chained input, decrements the 32-bit local counter and repeats when
the decremented value is nonzero. No helper-result truth test controls this
loop. Under normal callees that preserve the counter and local state, a
positive initial counter causes exactly that many iterations and two helper
calls per iteration. Unread alias or callee writes prevent an unconditional
termination or output-validity claim.

After the loop it reads the two addressed local words. Their last writes
and initialization are not established by this caller alone. It adds one
at 32-bit width to the minus-112 word and saves the result. If the minus-116
word is nonzero, it adds a word from the earlier local output region and
subtracts one, saving that derived result in another local; otherwise that
saved result retains the caller's earlier zero initialization, subject to
callee preservation. An incremented result of zero selects classification
zero. A nonzero incremented result with a zero derived result selects
classification two. When both are nonzero, it enters later matching work
outside the cited range; no final classification for that work is inferred.

At the shared classification join it resets saved return status to eight.
Classification zero returns through cleanup. For a nonzero classification,
bit one in the low byte of the second original argument selects the branch
studied here; a clear bit enters FND-EXE-057's shared suffix. With bit one
set, classification two also returns eight through cleanup. Other nonzero
classifications reread the third and fourth arguments and compare the exact
signature pair from FND-EXE-165.

For a matching pair, it writes five full words to the saved
fifth-argument-minus-48 base, in this order:

| Base offset | Locally supplied value |
|---|---|
| 24 | Saved classification-associated word, initially zero before matching |
| 28 | Saved derived word, initially zero before matching |
| 32 | Saved offset-28 reader result from FND-EXE-165 |
| 40 | Fifth original argument plus 32, prepared in FND-EXE-165 |
| 36 | Saved incremented word, initially zero before matching |

The loop or later matching paths may change those saved words; this table
states local sources rather than unverified field meanings. A nonmatching
signature skips these five stores. Both arms set saved return status to six,
call nested-record cleanup, reload the saved status after normal return,
and restore the frame before returning. There is no local cleanup-result
test, rollback, or six-versus-seven conflation. Stores and return claims
remain conditional on valid addresses and callee/local preservation.

## Interpretation

The ordinary signed admission, direct loop count and distinct six-return
state-save branch are now bounded. FND-EXE-053 and FND-EXE-054's second
callback consume result six as their other-result class, whereas
FND-EXE-052 permits saved-state transfer only after selector result seven.
The callback's saved-state stores therefore do not themselves prove that
this invocation transfers control. Q-EXE-009 retains the three matching
helpers, local output last writers, later matching classification, field
meanings, selected-record identity and dispatcher admission. No complete
matching algorithm or record lifecycle is established.

## Alternatives

Treating the accessor-adjusted value as an unsigned loop bound, treating
zero as one loop iteration, terminating the loop on a zero helper return,
or treating six as seven are ruled out by the instructions. The five stores
are not unconditional: signature mismatch skips them while still returning
six. Caller-prepared output addresses do not prove the callee writes valid
outputs, and unchanged-looking offsets do not establish storage identity.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read sixty-five instructions
from `0x005F5156` and one hundred from `0x005F5340`, restricting claims
to the cited ranges and excluding later matching code. Use FND-EXE-165
for entry, saved base, reader result and cleanup, FND-EXE-056 for exact
accessor arithmetic, and FND-EXE-057 for the shared suffix. Track chained
helper returns, output-address preparation, signed counter guard, decrement
width, local last-writer gaps, classification zero/one/two boundaries,
low-byte bit-one guard, signature stores in order and saved six after cleanup.
Keep callee, alias and exceptional effects conditional. Keep rich reports
local and execute no interpreter or game.
