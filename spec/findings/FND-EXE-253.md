---
id: FND-EXE-253
title: Replacement cleanup targets clear state on different paths without a common success result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0EA2..4AE5:0ECD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:1155..4AE5:11A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000411DB..0x000411DD
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-178 identifies explicit writers of replacement cleanup offsets
`0x0EA2` and `0x1155`. The first source candidate contains sixteen
instructions and forty-three bytes, no calls and one interrupt boundary.
The second contains thirty-one instructions and eighty-four bytes and
two unresolved computed far calls. Both save BP and DS, load DS through
CS-relative word five and return far without additional argument cleanup.
These are conditional bodies under that code-segment binding, not resolved
live targets for FND-EXE-252's callback slots.

At `4AE5:0EA2`, bit two of DS-relative byte `0x0038` must be set to
continue. Otherwise the body restores and returns without explicit state
stores. It then loads DX from word `0x0032` and skips the operation when
that word equals `0xFFFF`. On the remaining path it selects AH `0x45`
and interrupts through vector `0x67` at `4AE5:0EBD`. Its continuation
stores `0xFFFF` to word `0x0032` and zero to the entire byte `0x0038`,
without testing the interrupt's returned status. It clears all eight flag
bits rather than only the bit that admitted the operation. State identity
after the interrupt and saved-stack integrity remain conditional.

At `4AE5:1155`, zero DS-relative byte `0x0042` skips all work. Otherwise
word `0x0047` selects two different paths. Nonzero loads AH `0x0D` and
DX from that word and makes a far call through the pointer beginning at
DS-relative `0x0043`, then loads AH `0x0A`, reloads DX from word `0x0047`
and calls through that pointer again. Neither call's status is tested.
No DS reload separates them: the second word and pointer come from the
post-first-call DS and memory. The path then returns without an explicit
store to flag byte `0x0042`. The stored pointer carries both an offset
and segment; its live producers and external contract remain unread.

On the zero-word path the body sets ES to zero and reads word ES:`0x0066`
into AX. It compares that word to a relocated immediate. The operand at
shipped offset `0x000411DB` is raw `0x45E3`; its MZ relocation under load
segment `0x1000` yields `0x55E3`, whose offset-zero source correspondence
is shipped offset `0x0004B030`. Inequality returns without restoration
stores or an explicit flag clear.

Equality saves the state DS, loads DS from that compared AX and reads
word `0x002F` into AX before storing it to ES:`0x0064`. It then reads
word `0x0031` before storing it to ES:`0x0066`, restores the saved DS and
clears its flag byte `0x0042`. The two reads and writes are ordered, not
an atomic pointer replacement. Native storage identity, initialization,
aliases and interrupt-time observers are not admitted by this source
reading. On the explicit path ES remains zero; only DS and BP are restored.
The other paths do not supply a common preserved-ES or success-result
contract. Far-return frame preservation remains conditional on saved storage.

## Interpretation

The first cleanup target's state reset follows a requested interrupt,
not a checked successful result. The second's two-call path, comparison
rejection and actual restoration path retain different state publications.
A single generic cleanup-success reading would erase those distinctions.

FND-EXE-252 calls the first slot before the second without checking flags.
These bodies are replacement-offset candidates only: the slot writer paths,
actual DS and CS, and intervening effects must still connect them to that
consumer. Q-EXE-001 and Q-EXE-010 retain those obligations, flag/word and
far-pointer producers, external status/register contracts, low-memory
admission and independent alias/output bounds. No complete_reading or
replacement inventory is established.

## Alternatives

A status-gated reset in the first target is contradicted by the uninterrupted
post-interrupt stores. Clearing only the tested flag bit ignores the whole-byte
store. A common flag clear in the second target is contradicted by its
nonzero-word and comparison-rejection paths. Treating the second far call
as using a retained entry pointer ignores the fresh post-call memory reads.

## How to reproduce

At revision `f918723`, use FND-EXE-236's original-source identity, region
and default x86-bounds limits. Independently set entry and sole entries to
`0x00040EF2` and `0x000411A5`, with no seeds or summaries. Check intervals
`0x00040EF2..0x00040F1D` and `0x000411A5..0x000411F9`, sixteen and
thirty-one instructions respectively. The first interrupt is at
`4AE5:0EBD`; the second body's far calls are at `4AE5:1172` and
`4AE5:117C`. Continuations assume the interrupt and calls return; complete
CFG fields do not establish Standard complete readings or native targets.

Decode both intervals directly from the shipped source in sixteen-bit
mode with Capstone. Query operand at `0x000411DB`, sourceKind mz and
targetOffset zero, under the same source hash, to verify relocation.
Follow state stores, post-call reloads, complete far-pointer reads and
segment restoration separately per path. Compare FND-EXE-178's writer
leads and FND-EXE-252's consumer without treating them as admitted live
connections. Keep source, reports and configurations in GAME_DIR.
