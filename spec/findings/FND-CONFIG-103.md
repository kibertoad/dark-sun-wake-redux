---
id: FND-CONFIG-103
title: Overlay 173 passes a nonzero byte gate and bypasses overlay 176 feedback at its selector call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5671:006B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 573B:0089
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV trampoline and MZ mapping
environment: null
---

## Observation

Overlay 173's selector call at file offset `0x0005D2CC` belongs
to exported entry 5671:006B, starting at `0x0005CE80`
(FND-CONFIG-101). Its nearby branch indexes a 23-byte record with
SI through DS:19C1 and requires the signed byte at `+0x16` to be
negative before entering the argument sequence.

The caller pushes literal one at `0x0005D28D` and again at
`0x0005D28F` for the selector's two final byte arguments. Its third
word argument comes from local word `[BP-0x18]`; first and second
word arguments come from DI and DS:4206. Other arguments include
literal six, record words, and a stack-local far pointer. Regardless
of those values, the byte at selector 573B:0089's `[BP+0x1A]` is one.

That selector tests the byte before its 235..268 branch. A nonzero
value selects the alternative local call at file offset
`0x000805BF`, and its following jump rejoins after the call to
5691:0043 (FND-CONFIG-100). Thus this particular selector invocation
cannot select the direct overlay 176 call at `0x00080607`.

## Interpretation

One of the eleven inventoried incoming calls is excluded from the
selector's direct route to overlay 176's two message sites by its
literal byte argument. It is not a candidate for that range-gated
route merely because it calls the shared selector.

## Alternatives

The alternative local callee's effects remain unread (Q-CONFIG-008);
it might have its own further calls. Other calls from overlay 173 or
computed routes are not excluded. This finding is limited to the
selector's direct 5691:0043 branch for this invocation, not the
absence of every psionic or message path from the containing routine.

## How to reproduce

Resolve overlay 173 stub 006B to `0x0005CE80` and confirm instruction
alignment from its prologue to the selected call. Inspect only the
nearby record guard and argument sequence at
`0x0005D26F..0x0005D2DC`. Check the two literal pushes against the
selector's byte read at `0x00080599`; follow its nonzero-byte branch,
alternative call and rejoin at `0x000805A5..0x000805C7` using
FND-CONFIG-100. Keep exclusion of this direct branch separate from
unread effects of the alternative callee.
