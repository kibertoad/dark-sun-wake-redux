---
id: FND-EXE-131
title: Selected prefix helper admits sixty-four stored selectors and shares a zero-return default without clearing the selector
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A0DE0..0x004A0E2D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    offset: 0x00322180..0x0032227F
    kind: file-data
tool: Verified physical PE dispatch words and Ghidra 12.1.3 PUBLIC bounded prefix dispatch reading
environment: null
---

## Observation

FND-EXE-124 records a direct call to `0x004A0DE0` after the selected
callee publishes its first input's low byte and before its fresh mode gates.
The helper reserves 220 bytes, reads full selector S from
`0x01BA29EC`, saves EBX, ESI and EDI in local slots at current ESP
plus `0xD0`, `0xD4` and `0xD8`, and compares S unsigned with
63. These local saves preserve the comparison flags. S greater than 63
branches directly to `0x004A0E10`; values zero through 63 jump
through a full word at `0x00724F80` plus four times S.

The default at `0x004A0E10` restores those three registers, sets full
EAX zero, releases the reservation and returns. Its local path has no
calls or global stores, and does not clear the stored selector. Unsigned
values with their sign bit set also take it. Repeated admission of the
same out-of-range stored selector therefore remains possible unless some
other writer changes it; initialization, lifetime and other writers remain
unresolved. FND-EXE-124's caller does not test this helper's EAX before
its own fresh mode gate.

Identity-verified physical little-endian reads of every admitted table slot
resolve the following complete mapping. Selector groups share only the
stated target, not a proved common semantic meaning:

| Stored selectors | Target |
| --- | --- |
| 0, 62, 63 | `0x004A1803` |
| 1 | `0x004A15D4` |
| 2 | `0x004A152C` |
| 3 | `0x004A1BA6` |
| 4, 13, 19, 31 | `0x004A0EC4` |
| 5, 14, 20, 32 | `0x004A0E9A` |
| 6, 15, 21, 33 | `0x004A0E2E` |
| 7 | `0x004A1D22` |
| 8 | `0x004A1ABA` |
| 9 | `0x004A1A14` |
| 10 | `0x004A1437` |
| 11 | `0x004A1114` |
| 12 | `0x004A175F` |
| 16, 22 | `0x004A11CB` |
| 17, 23 | `0x004A16C0` |
| 18, 24 | `0x004A1C40` |
| 25 | `0x004A1683` |
| 26 | `0x004A1DE1` |
| 27 | `0x004A1B75` |
| 28 | `0x004A1CCD` |
| 29 | `0x004A14E5` |
| 30 | `0x004A1964` |
| 34 | `0x004A18E1` |
| 35 | `0x004A1875` |
| 36 | `0x004A180E` |
| 37 | `0x004A1344` |
| 38 | `0x004A12C4` |
| 39 | `0x004A19AB` |
| 40 | `0x004A125E` |
| 41 | `0x004A1025` |
| 42 | `0x004A0FBD` |
| 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54 | `0x004A0E10` |
| 55 | `0x004A1064` |
| 56 | `0x004A0F01` |
| 57 | `0x004A10C7` |
| 58 | `0x004A1427` |
| 59 | `0x004A1D0F` |
| 60 | `0x004A13C0` |
| 61 | `0x004A0F60` |

The table spans loaded addresses `0x00724F80` through
`0x0072507F`, physically shipped offsets `0x00322180` through
`0x0032227F`. Each slot is four contiguous file-backed bytes;
selector 63 starts at `0x0072507C` / `0x0032227C`.
Selectors 43 through 54 therefore share the locally read zero-return
path with rejected values above 63. Selector zero does not select that
path: its physical slot selects `0x004A1803`. No branch semantics
or return values beyond the shared default are established here. The
bounded gate justifies these table queries, not a scan of nearby words.

## Interpretation

This resolves the prefix helper's local index admission and concrete
branch-target mapping before FND-EXE-124's subsequent mode gates. Its
zero-return default does not consume the stored selector locally, and
admitted selector zero has a distinct branch. Q-EXE-009 in FMT-EXE-006
remains open for selector/state producers, branch effects and shared
joins, remaining selected-callee paths, publisher indirect targets and
mapping lifetime. This is not a complete helper reading or actual PATH
behavior; the existing helper and installed callback in FND-EXE-102
remain the evidence for that separate already-read contract.

## Alternatives

- Full unsigned admission rejects sign-bit-set inputs rather than treating
  them as negative indices below 63.
- Admitted selector zero is not the local default, while 43 through 54
  explicitly share the same default target as out-of-range inputs.
- Returning zero does not clear the stored selector or establish that the
  caller checks failure; its next gate reads other state independently.
- Equal physical targets do not prove equal input admission, semantic
  field meanings or complete effects of the unread branches.

## How to reproduce

Verify FND-EXE-011's executable length and XXH3-128 identity. FND-EXE-124
independently supplies the direct target. Use the saved Ghidra program
with -noanalysis and ReportInstructionWindow.java at `0x004A0DE0`
limit 55, restricting claims to the declared prefix/default range and
excluding following branch bodies. Track full selector width, unsigned
63 comparison, flags across saves, four-byte indexing, default local
stores, register restoration and full zero return.

Using the PE image base and file-backed section mapping, read each of the
sixty-four full slots at `0x00724F80` plus four times selector zero
through 63, decoding little-endian. Require each four-byte span to map
contiguously inside raw file data; reject identity or mapping changes.
Recheck all six physical byte/word controls in FND-EXE-099 before relying
on the mapping. The exact slots, span and all result-driving targets are
recorded above; keep raw queries local. Check selector zero, 43, 54, 63,
64 and a sign-bit-set value as local admission controls, not native inputs.
No native or emulated execution is part of this observation.
