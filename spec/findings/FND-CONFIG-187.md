---
id: FND-CONFIG-187
title: The number replacement helper checks size only on query success and updates cache after unchecked transfer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:006B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2A25
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2A44
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete local 16-bit readings and declared FBOV/MZ operand mapping
environment: null
---

## Observation

FND-CONFIG-186's changed-number branch passes a word
number and byte choice to overlay 182 entry 56BD:006B.
Its declared descriptor/trampoline mapping resolves to code
059C, complete file span `0x00068DEC..0x00068E8D`,
ending with far return at `0x00068E8C`. It saves SI
and initially retains its first word argument there. Equality
of that word with current DS:0FCC, followed by equality
of the stacked low byte with current DS:0FCF, skips all
resource/service work. Either difference continues. There
is no own overall AX normalization on that cache bypass.
DS-relative fields mean DS at each instruction.

A nonzero choice byte stores double-word type ICON in
a local field; zero stores type BMP followed by a space.
It passes that type, zero-extended current SI and the far
address of a second local double word to 38FF:05B5.
FND-CONFIG-151 reads this query's explicit length output
and result contract. The helper itself does not initialize
the local length before the call. Returned AX zero takes
a signed double-word comparison with 2,048 (0800
hexadecimal); strictly greater skips all following work.
At most 2,048 continues. A negative double word also
passes this signed gate; native producer validity remains
open. Nonzero returned AX reaches a byte comparison whose
both routes join the same continuation, without this length
rejection. Thus query failure does not locally block transfer
or later cache writes. The query's explicit output clear
on its own ordinary path does not make failure a success.

The admitted continuation calls resident 28C9:2A25,
then passes the retained local type, zero-extended post-call
SI and far output address current DS:45A6 to resource
wrapper 38FF:04AB. Its returned AX flags are not used
by a conditional branch. It passes current far DS:45A6
and word zero to 3D72:12ED, without an own pointer-null
check or accepted-transfer gate. FND-CONFIG-151 bounds
allocation/output assignment before possible I/O failure and
reuse of an existing pointer without a capacity argument.
The 2,048 comparison does not prove a fixed buffer capacity
or unchanged selected resource on this later request.

It then calls 28C9:2A44 with word zero, stores post-call
SI at current DS:0FCC and the stacked low byte at
DS:0FCF, and restores the caller's saved SI at normal
return. Register/segment/stack preservation through all calls
is a condition on that stored number being the original
argument. These writes are not gated by transfer or service
success. FND-CONFIG-186's caller has already stored its
selected word at DS:4273 before requesting this helper.
The two cache levels are separate, and a returning failure
can therefore leave a later equal-number caller bypassing
its request without proving that output content is valid.

28C9:2A25's complete resident span is
`0x000208B5..0x000208D4`. Nonzero current byte
DS:0DA0 skips all work. Zero calls 3D72:0B84, sets
current bytes DS:332C and DS:332E to one and increments
current byte DS:0DA0. The increment occurs after that
external call, so an unchanged byte/DS is a condition on
its normal result being one. The body does not add another
increment on a nonzero entry state. It normalizes no AX.

28C9:2A44's complete resident span is
`0x000208D4..0x00020911`. Nonzero low argument byte
sets current DS:0DA0 to zero. Zero decrements that byte,
then clamps a signed-negative result to zero. It re-tests
the byte: nonzero returns; zero sets byte DS:3330 to
one, clears DS:332C and DS:332E, then calls 3D72:0942.
For stable DS and the named zero argument, initial byte
zero or one reaches that service with zero; two becomes
one and skips it; 80 hexadecimal becomes 7F and skips
it; 81 becomes 80 then is clamped to zero and reaches
it. These are word-independent byte-wrap/signed branch
derivations, not native state invariants or a nested-count
contract. External service changes remain separate.

All overlay calls above were checked through descriptor-
182 declared fixups. Both resident helper service operands
were verified through header-derived MZ relocations. Their
complete 3D72 service effects, pointer contents, field and
segment producers, accepted transfer and native outcomes
remain open. No own 0DAB or 1440 store occurs in these
three bodies; calls and aliases can still affect later state.

## Interpretation

The replacement path has two own cache gates and an
asymmetric length gate: successful queries can be rejected
for signed length above 2,048, while a returning failed
query still reaches service and transfer requests. A failed
transfer is not a local cache-write barrier. The bracketing
byte helpers also have distinct zero/nonzero and signed
byte behavior, not a proven unconditional acquire/release
pair. None establishes accepted images, actual presentation
or a valid final cache merely from populated fields.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain all callers,
number/type/length/byte/pointer and segment producers,
register/stack preservation, capacities, aliases, archives,
complete 3D72 effects and actual transfer/runtime outcomes.
One reading supplies matching cache or an admitted successful
query; another supplies a returning failed query/transfer or
changed state between calls. The local branches differ but
native reachability and accepted output require more evidence.

A reading that every failed length query blocks transfer is
ruled out by the common continuation. A reading that cache
writes require a successful transfer is ruled out by the
absence of that result gate. A reading that every 2A25
entry increments a nesting count is ruled out by its nonzero
bypass. A universal pairing or rendered-purpose claim would
need all byte and service producers, not only these calls.
No native or emulated outcome is claimed; Q-SCRIPT-007
cannot execute the overlay resource helper.

## How to reproduce

Validate descriptor 182's 006B trampoline and read code
059C through 063C from its entry. Follow both cache
fields, local type/length storage, query-result branches,
signed double-word size comparison, common nonzero-result
continuation, unchecked transfer/pointer service and final
post-call cache writes. Verify every declared fixup and
compare FND-CONFIG-151's output-pointer/length contracts.
Read resident 28C9:2A25 through 2A43 and 2A44 through
2A80, checking the after-call increment, zero-argument
decrement, signed clamp and service gates. Derive named
byte cases under stable storage/segment conditions; keep
complete services, accepted buffers, native outcomes and
number/byte/register producers open.
