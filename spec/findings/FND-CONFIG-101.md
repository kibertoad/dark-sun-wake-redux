---
id: FND-CONFIG-101
title: Eleven declared overlay calls enter the selector upstream of overlay 176 feedback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 573B:0089
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5691:0043
tool: Python 3.14.7 bounded FBOV fixup-table, MZ relocation and relative-call inspection
environment: null
---

## Observation

The selector at overlay 193's 573B:0089 has a conditional call to
5691:0043, containing overlay 176's two shared-message sites
(FND-CONFIG-100). Eleven declared FBOV direct far-call fixups target
that selector across eight overlays:

| Caller overlay | Call file offsets |
|---:|---|
| 172 | `0x0005A217` |
| 173 | `0x0005D2CC` |
| 174 | `0x0005EE99`, `0x0005F5A8`, `0x0005F700` |
| 189 | `0x0007511A`, `0x00077CCA` |
| 208 | `0x0009302F` |
| 211 | `0x00096711`, `0x0009747E` |
| 213 | `0x00099CAF` |

Selecting MZ relocation words preceded by a direct far-call opcode and
the exact offset/segment operands for 573B:0089 finds no resident call.
A bounded byte search within overlay 193's declared code range finds
no candidate 16-bit relative near call to its code entry at
`0x00080580`. Applying the same searches to 5691:0043 finds no
resident call or local 16-bit relative near-call candidate in overlay
176. Its declared incoming overlay call is the single selector site
in FND-CONFIG-100.

## Interpretation

The eleven sites give concrete upstream locations for reading the
selector's inputs. They are syntactic direct calls, not eleven proven
entries into the psionic feedback branch: the selector's code range
and byte gate must be checked at each caller.

## Alternatives

FND-CONFIG-102 traces overlay 172's zero-gate frame-code route, and
FND-CONFIG-104 traces its resident frame-event producer.
FND-CONFIG-103 excludes overlay 173's invocation from the selector's
direct overlay 176 branch by its literal nonzero byte. FND-CONFIG-125
reads overlay 174's three zero-gate routes and their local code sources.
FND-CONFIG-106 excludes overlay 189 and 213's three literal-nonzero
invocations. FND-CONFIG-126 reads overlay 211's two zero-gate routes;
FND-CONFIG-108 reads overlay 208's stored gate and code. All eleven
sites now have local gate readings, while producing state, remaining
guards and upstream reachability remain open (Q-CONFIG-008).
A site can call this selector with codes outside 235..268 or a nonzero
byte gate and therefore bypass overlay 176. Computed pointers, far
jumps, address aliases, unrelocated operands or other relative-call
encodings are outside this inventory. The negative searches do not
prove the absence of every indirect or local incoming route.

## How to reproduce

Locate the approved DSUN.EXE segment table and declared overlay code
and fixup ranges using FMT-EXE-002 through FMT-EXE-004. For each
overlay, select fixup words naming descriptor 193 whose preceding
direct far-call offset is 0089. Count and group only the selected call
locations. For MZ calls, select relocation words carrying resident raw
segment 473B and direct call offset 0089. Search only overlay 193's
code range for the near-call opcode with a 16-bit relative displacement
landing at `0x00080580`; do not classify absent candidates as an
indirect-call result. Repeat the target-specific selection for descriptor
176, offset 0043, resident raw segment 4691 and file entry
`0x000616D6` to compare FND-CONFIG-100.
