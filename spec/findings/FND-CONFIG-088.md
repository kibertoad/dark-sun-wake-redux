---
id: FND-CONFIG-088
title: An overlay 213 window callback reaches the shared message entry through one guarded control event
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57CE:0048
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57CE:0061..57CE:0917
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57CE:1700..57CE:18AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Python 3.14.7 FBOV trampoline and fixup inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Overlay 213's `57CE:0048` trampoline targets the far callback at file offset
`0x000995AF`. Four address-taking sites pass that same far pointer: the
window-creation calls at `0x000992C1` and `0x0009A980`, and later control
setup calls at `0x00099331` and `0x0009AAF5`. The callback's event dispatch
separates event first words 1, 5 and 2. Event 1 calls its cleanup routine at
`0x00099535`. Event 5 has a branch for control word `0x3B60`; no direct
call to the shared `566A:002A` entry was found in that branch. Event 2
accepts control words `0x3BC8` and `0x3BC9` in the bounded dispatch at
`0x000996BF..0x000996DB`.

The `0x3BC9` path begins at `0x0009986F`. It reads the selected index from
`DS:2E62` and a value from the indexed 23-byte record. A stack word at
`[BP+0x18]` of 8 or more branches to local cleanup and a separate call,
past the direct message site. For a smaller value, the path enters the
record checks at `0x000998AF`. If its value is outside
`0x7530..0x7917`, it reaches the local branch at `0x000999A9` directly.
For a value within that range, it performs other state-changing calls;
return bit `0x10` from the call at `0x00099903` determines whether it
continues toward the same branch. The branch at `0x00099971..0x0009997A`
can exit when `DS:655C` is nonzero. The `0x3BC8` event-two branch does not
directly enter this message-selection path.

At `0x000999A9`, the record's word at `+0x10` and later range and helper
tests select the text arguments already identified in FND-CONFIG-050.
The converged call at `0x00099B72` passes that pointer to overlay 172's
`566A:002A` entry. The local branches at `0x000999A2`, `0x00099A7F`,
`0x00099A86`, `0x00099B67` and `0x00099B6E` all jump to that call after
their own guards. A separate branch at `0x000999DC..0x000999EB` skips to
`0x00099B7D`, past the message call, for values outside its listed ranges.

## Interpretation

The shared message call in overlay 213 is downstream of an event-two
callback path for control `0x3BC9`, with a stack-word limit, record tests
and several conditional exits. FND-CONFIG-050 identifies the local text
choices. These code paths do not prove that the event or a particular
message occurs in a live state, or that the later `WIND/10501` acquisition
gate succeeds.

## Alternatives

The producers of the callback's event record, the selected index and the
record fields have not been read completely. Other paths into overlay 213
may call the same local message-selection code indirectly. A direct-call
inventory does not exclude computed calls to overlay 172's entry.

## How to reproduce

Resolve overlay 213 stub `57CE:0048` to code offset `0x034F`, file offset
`0x000995AF`. Disassemble bounded windows around `0x000992C1`,
`0x00099331`, `0x0009A980` and `0x0009AAF5` for the address-taking sites;
then read `0x000995AF..0x000996DB`, `0x0009986F..0x000999A9` and
`0x000999A9..0x00099B7D` in windows no larger than 512 bytes. Resolve the
far call at `0x00099B72` through its FBOV fixup to `566A:002A`.
