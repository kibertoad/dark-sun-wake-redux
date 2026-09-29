---
id: FND-CONFIG-149
title: A declared direct setup caller joins two preceding paths and checks two following returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0020
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00C0
tool: Python 3.14.7 and Capstone 5.0.7 declared relocation/fixup call inventory and entry-based bounded instruction checks
environment: null
---

## Observation

A declared far-call inventory for setup 5702:00D4
(FND-CONFIG-144 and FND-CONFIG-147) covers the resident
MZ relocation words and the validated 49 overlay fixup lists,
8262 fixups. It selects far-call opcode 9A immediately before
an offset 00D4 and a declared segment operand mapping to
5702. The only selected call is overlay 180 at file offset
`0x00067693`; its operand at `0x00067696` names
descriptor 188. There is no selected resident call. This is
an inventory of that declared direct encoding, not all possible
incoming routes or address-taking uses.

The containing exported entry is 56B2:0020, code target
0141, file offset `0x00067321`. Entry-based decoding
confirms the call boundary. At `0x00067681` it tests
byte DS:13F7. Nonzero calls local entry 56B2:0057
(code target 0026, file offset `0x00067206`), then jumps
to the setup call. Zero calls 4842:06CF at
`0x0006768E` and falls through to the same setup call.
Neither helper's return is tested at this local join. Their
transitive effects and earlier paths are not established here;
the join assumes the selected helper returns.

Immediately after setup, `0x00067698` calls 5702:00C0.
It zero-extends returned AL and, when zero, calls 1000:03DF
with word one. Then `0x000676AB` calls 5702:00B6.
A zero returned AL again calls 1000:03DF with word one.
This records branch order, not whether 03DF returns or exits.
FND-CONFIG-150 reads the post-setup 00B6 initializer.

Entry 5702:00C0 resolves to code target 046C, file offset
`0x0007330C`, ending with far return at
`0x00073330`. It reads DS:193D. If zero, it calls
565C:0020 with word 200 and a double word 10000, stores
returned AL in DS:193D, then returns that byte. A nonzero
stored byte bypasses the call and returns the stored byte.
The callee's effects remain unread. DS:193D is not the
setup gate DS:193E.

A separate selected literal-write query for displacement
193E, over the resident image and the same 49 overlay ranges,
found no first-memory-operand write candidate using default DS
or explicit DS with the write-like mnemonics below. The initial
byte DS:193E is zero at file offset `0x0004E93E`.
This does not cover adjacent wider writes, pointer/block writes,
other segments, aliases or every instruction encoding, and does
not establish the gate value at this call.

The call operands at `0x00067691`,
`0x00067696`, `0x0006769B` and `0x000676AE`
are declared FBOV fixups. Descriptor 75 maps to 4842;
descriptor 188 maps to 5702. Entry 00C0's call operand
at `0x00073324` names descriptor 169, mapped 565C.
DS fields use the mapped 57E0 data segment in FND-SCRIPT-005.

## Interpretation

Setup has a concrete direct caller whose two immediately
preceding branches converge before it. The caller then checks
two different initialization results. This does not make setup's
zero gate an invariant: earlier helpers, stored state and
indirect writes can still affect DS:193E. The three adjacent
bytes 193C, 193D and 193E must not be treated as one flag.
No gameplay identity or unconditional startup reachability is
assigned to the caller.

## Alternatives

FND-CONFIG-155 subsequently reads the zero-mode helper's
local code-segment resets and flag return. It assigns neither
the setup gate nor an archive registration. The other mode's
helper and earlier state still remain open.

Q-CONFIG-008 retains earlier incoming paths, the two pre-setup
helpers, the 00C0 callee, indirect/address-taking setup routes,
other DS:193E producer forms, bypass state and later iterator
inputs. One reading keeps the gate at its initial zero; another
reaches a nonzero gate through unread state changes. The literal
query does not distinguish them. Do not repeat that query without
new write-form coverage, a new tool or a new reading.
FND-CONFIG-147's zero-gate result remains conditional on
which branch the setup actually takes.

## How to reproduce

Use the validated range selection in FND-CONFIG-148. Match
resident declared MZ words mapping to 5702 and overlay fixups
naming descriptor 188; require preceding far-call opcode and
offset 00D4. Confirm the sole selected site by decoding from
56B2:0020, then read the bounded local join and following
return checks at `0x00067681..0x000676BC`.
Resolve and read the complete 00C0 body separately from its
following routine. Verify the named fixup operands. For the
gate query, search raw little-endian 193E, decode starts one
through five preceding bytes, and select first memory operands
at that displacement with default/explicit DS for mov, inc,
dec, add, sub, and, or, xor, xchg or pop. Keep the initial
byte separate from runtime state and unselected write forms.
