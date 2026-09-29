---
id: FND-CONFIG-111
title: Six declared setup calls supply overlay 208 gate and code inputs
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:0066
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:0043
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5713:004D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57C1:005C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57C1:0061
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57C1:0066
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57CE:0048
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV fixup and trampoline mapping
environment: null
---

## Observation

Six declared FBOV direct far calls target overlay 208 setup entry
57A6:0066, whose stores are described in FND-CONFIG-109:

| Caller entry | Call file offset | Third word, stored code | Fourth argument byte, stored gate |
|---|---|---|---|
| 566A:0043 | `0x0005A275` | SI | Zero |
| 5713:004D | `0x000754EC` | DS:43F5 | One |
| 57C1:005C | `0x00096EB9` | DS:43F5 | Zero |
| 57C1:0061 | `0x0009754C` | SI | Zero |
| 57C1:0066 | `0x000976DA` | 306 | Zero |
| 57CE:0048 | `0x00099DE2` | Local word BP-4 | One |

Each call belongs to its listed exported entry, with no preceding
return between that entry and call. The gate is a literal push distinct
from the fifth argument's literal or helper result. The overlay 172
call retains the selected code in SI used by its separate direct
selector branch (FND-CONFIG-102), but is a different branch of that
callback. The two computed-code overlay 211 calls retain the sources
read in FND-CONFIG-107: DS:43F5 and SI respectively.

The overlay 189 setup branch requires the unsigned stored code below
328. The overlay 211 005C branch also requires it unsigned below
328 before the call. The overlay 213 branch compares the local code
signed below 328. These comparisons do not force 235..268.
The remaining callers' prior event and helper guards are not exhausted
by this argument inventory.

Selecting exact direct MZ relocated calls to raw segment 47A6,
offset 0066, finds no resident call. Within overlay 208's declared
code-and-data range there is no 16-bit relative near-call candidate
to setup entry `0x00092EA2`. The declared-call query's decoding was
checked against the eleven selector calls in FND-CONFIG-101.

## Interpretation

Two setup invocations assign a nonzero gate; absent a later change,
their stored inputs cannot take the selector's direct overlay 176
branch. The code-306 invocation assigns zero gate but a code outside
235..268, likewise excluding that branch absent a later change.
The other three zero-gate invocations can assign a qualifying code,
but their computed sources and subsequent state still decide it
(FND-CONFIG-100, FND-CONFIG-108, FND-CONFIG-110).

These are input assignments for a later selector call, unlike the
literal gates pushed at direct selector call sites. A later writer
could invalidate either exclusion.

## Alternatives

The three qualifying-code candidates' data producers, full prior
guards, callback and event routes, intervening helper effects and
later writes remain open (Q-CONFIG-008). Computed or unrelocated
setup calls are outside the inventory. A declared call and eligible
stored values alone do not prove visible feedback or message-window
acquisition.

## How to reproduce

Select FBOV fixups whose stored segment-table index shifted right
three is 208 and whose preceding direct far-call offset is 0066,
using FMT-EXE-005. Verify the six selected instructions and their
argument pushes from their exported entries. Inspect bounded call
windows `0x0005A248..0x0005A27D`,
`0x000754A5..0x000754F4`, `0x00096E93..0x00096EC1`,
`0x00097528..0x00097554`, `0x000976B1..0x000976DF`, and
`0x00099DC4..0x00099DEA`. Compare setup's parameter stores in
FND-CONFIG-109. Check exact MZ relocated direct calls separately and
search only overlay 208 for 16-bit relative near-call candidates.
Use the known eleven-call selector inventory as a positive control
for shifted-index decoding, without treating it as proof about
computed or differently encoded calls.
