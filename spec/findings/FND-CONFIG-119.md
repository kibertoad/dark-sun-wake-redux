---
id: FND-CONFIG-119
title: Overlay 204 has seven local calls to the temporary-state wrapper in two guarded groups
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0020
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0025
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV trampoline and fixup mapping
environment: null
---

## Observation

Seven 16-bit relative near-call candidates within overlay 204's
complete declared code-and-data range target the temporary-state
wrapper 5787:0025 at file offset `0x0008D074`
(FND-CONFIG-117). All seven verify as instructions in exported
entry 5787:0020, beginning at `0x0008CA7A`, with no preceding
return between that entry and each call.

| Call file offset | First word argument | Second word | Third word | Fourth argument byte |
|---|---:|---|---|---|
| `0x0008CD54` | 148 | SI | Local BP-0C | Low byte of local BP-06 |
| `0x0008CD67` | 157 | SI | Local BP-0C | Low byte of local BP-06 |
| `0x0008CD7A` | 159 | SI | Local BP-0C | Low byte of local BP-06 |
| `0x0008CD91` | 148 | SI | DI | Low byte of local BP-08 |
| `0x0008CDA2` | 157 | SI | DI | Low byte of local BP-08 |
| `0x0008CDB3` | 159 | SI | DI | Low byte of local BP-08 |
| `0x0008CDCA` | 178 | SI | DI | Low byte of local BP-08 |

The first three calls form a sequence guarded by signed local
BP-06 at least five. After that sequence or its bypass, the next
three form another sequence guarded independently by signed local
BP-08 at least five. The seventh follows when BP-08 compares
signed at least seven. Each call cleans up eight argument bytes.
A helper can have effects between calls; the groups are sequential
paths, not seven alternative dispatch targets.

The declared FBOV direct far-call inventory finds no incoming call
to descriptor 204 trampoline 0025. Exact relocated MZ direct calls
to raw 4787:0025 also find none. These encoding-specific negatives
do not exclude computed or aliased routes. The verified local calls
supply concrete incoming sites despite those negative sections.

## Interpretation

The temporary state-five wrapper has concrete local callers. On
normal return, each invocation restores its own captured state
before the next invocation begins (FND-CONFIG-117). The literal
first-word values and local thresholds establish these argument
routes, without assigning their gameplay meanings or the producing
locals' values in a live state.

## Alternatives

FND-CONFIG-132 reads the first-pass threshold and index producers
and the second-pass consumption. Upstream inputs, iterator producers,
earlier setup effects and effects of intervening calls remain unread
(Q-CONFIG-008). Other incoming encodings remain possible. Passing a
local threshold does not establish that the helper terminates,
changes a particular record or reaches shared feedback.

## How to reproduce

Resolve overlay 204 entries 0020 and 0025 with FMT-EXE-002 through
FMT-EXE-004. Search relative-call candidates across
`0x0008BDC0..0x0008E660`, then verify their instruction boundaries
from entry `0x0008CA7A`. Read the complete grouped call block
`0x0008CD41..0x0008CDD0`, distinguishing signed word guards
from the low-byte argument values and immediate first words.
Compare the wrapper's four argument slots in FND-CONFIG-117.
Select declared FBOV direct-call fixups and relocated MZ direct
calls separately, without extending their negatives to local calls.
