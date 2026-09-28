---
id: FND-CONFIG-077
title: Overlay 204 rest messages have handler and script-request caller routes
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
    address: 56BD:0070
tool: Python 3.14.7 FBOV fixup, MZ relocation, near-call and switch-table inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Overlay 204's `5787:0020` trampoline targets the routine at file offset
`0x0008CA7A`, which contains the two guarded calls to the shared message
entry in FND-CONFIG-049. A scan of declared FBOV fixups finds one direct
far call to `5787:0020` from another overlay, at `0x0006951C` in overlay
182. The resident MZ relocations have no direct call to that entry.
One aligned near call within overlay 204 targets the same routine, at
`0x0008C079`.

Overlay 182's `56BD:0070` handler contains the far call at
`0x0006951C`. Its earlier branch calls a local helper at `0x000694C6`
and reaches the rest call only when that helper returns zero and the
word copied from an indexed record at `+0x1B` equals `0x0525`. It passes
zero and the double word `0x7FFE0008` to the rest entry. Two direct far
calls from overlay 178 target this handler: `0x00063400`, after local
bytes `[BP-3]` and `[BP-4]` compare equal to `0x10` and zero; and
`0x00063C62`, after another local setup sequence. Both pass a word at
`DS:426D`, an index and zero. The complete upstream state for those
branches remains unread.

The near call at `0x0008C079` lies in overlay 204's 53-request
dispatcher. Its jump table maps request one to this branch. The branch
passes one and an argument from the dispatcher's stack to the rest
entry. FND-VIDEO-005 traces script opcode `0x22` to the dispatcher,
so a script request of one has this direct route to the rest entry.

## Interpretation

The combat refusal and conditional rest announcement in FND-CONFIG-049
can be entered from two statically identified routes: an overlay 182
handler's `0x0525` branch, or script opcode `0x22` request one. Both
reach the same local message guards; neither caller alone establishes a
completed rest or a later message-delay wait.

## Alternatives

Computed or unrelocated pointers could add callers. The indexed record's
field at `+0x1B`, the earlier handler helper, and the two overlay 178
branches have not been read through to their player-visible inputs.
The script request's occurrence and arguments in shipped scripts are
also not inventoried here.

## How to reproduce

Map overlay 204 from its header at `0x0004CA70`, code start
`0x0008BDC0`, and `0020` trampoline target code offset `0x0CBA`.
Search declared overlay fixups and resident MZ relocations for direct
calls to `5787:0020`; scan overlay 204's declared code for near calls
to `0x0CBA`. Disassemble `0x00069473..0x0006952B`,
`0x000633EA..0x00063420`, `0x00063C4E..0x00063C6A` and
`0x0008C071..0x0008C08C`. Decode the 53-word dispatch table at
code offset `0x06C7` to verify request one's target `0x02B1`.
FND-CONFIG-049 supplies the called routine's two message branches.
