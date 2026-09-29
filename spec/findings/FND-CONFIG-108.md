---
id: FND-CONFIG-108
title: Overlay 208 forwards stored selector gate and code without a local branch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:007F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 573B:0089
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV trampoline mapping
environment: null
---

## Observation

Overlay 208's exported entry 57A6:007F begins at file offset
`0x00092FF1` and returns at `0x0009304C`. Its declared selector
call is at `0x0009302F` (FND-CONFIG-101). No branch or earlier
return lies between entry, two helper calls and selector invocation.

It reads byte DS:9BD3 immediately before constructing byte argument
BP+1A. The following byte argument is literal one. Its first three
word arguments come respectively from DS:9BE0, DS:9BDE and
DS:9BE2; the third is the selector code. The fourth word is the
sign-extended byte at DS:9BE5. Four intervening words come from
DS:9BDC, DS:9BDA, DS:9BD8 and DS:9BD6. A far pointer to
DS:9BE6 is passed separately; it is not a local initialized byte.

After selector return, the entry stores its byte result in a local
and selects between two local helper calls on zero versus nonzero.
This later branch does not gate the earlier selector invocation.

## Interpretation

Unlike the other declared callers' literal gates, this invocation
has a stored gate byte. Its selector can enter overlay 176 only when
DS:9BD3 is zero and DS:9BE2 compares signed in 235..268
(FND-CONFIG-100). This caller supplies no local comparison that
proves either condition or the pointed state at DS:9BE6.

Together FND-CONFIG-102, FND-CONFIG-103, FND-CONFIG-105,
FND-CONFIG-106 and FND-CONFIG-107 account for the other ten sites
in FND-CONFIG-101. Across the eleven declared calls, six pass
literal zero for the gate, four pass literal nonzero, and this one
reads the stored gate. This completes that inventory's local gate
classification, not its upstream reachability or state provenance.

## Alternatives

FND-CONFIG-109 identifies the setup entry's argument stores, and
FND-CONFIG-111 inventories six declared setup invocations.
FND-CONFIG-110 traces two local incoming routes, including a
conditional repeated-call route. Remaining data producers, prior
guards, later writes and the two preceding helpers' effects remain
unread (Q-CONFIG-008). A later read can observe a helper's state changes;
loaded-image values alone would not prove the invocation's inputs.
Computed, aliased or unrelocated calls remain outside the declared
inventory. Passing the selector gates alone does not establish a
message or successful feedback-window setup.

## How to reproduce

Resolve 57A6:007F using FMT-EXE-002 through FMT-EXE-004.
Inspect the complete bounded entry `0x00092FF1..0x0009304D`,
tracking its stored byte and word reads in push order. Compare its
parameter slots with the selector in FND-CONFIG-100, keeping the
byte gate separate from the pointed state. Compare the eleven sites
in FND-CONFIG-101 with the five local caller findings cited above;
do not count a literal-nonzero invocation as an overlay 176 route.
