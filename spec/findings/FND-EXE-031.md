---
id: FND-EXE-031
title: A sentinel collection producer skips creation on a match and links new storage after helper calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0058BDD6..0x0058BF12
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0058BF7A..0x0058BF91
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FAE40..0x005FAE5B
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction and function reporters
environment: null
---

## Observation

The bounded producer path in the routine at `0x0058BD90` reads the same
head and sentinel `0x0240DB50` used by FND-EXE-029. It saves the current
node, calls the comparator from FND-EXE-030 with node plus eight and its
input word, and tests the full 32-bit result. Zero transfers directly to
`0x0058BF7A`. Nonzero advances through offset zero and repeats until the
sentinel. Initial sentinel and later sentinel both enter the creation path.
There is no local null-link or independent traversal bound in this search.
Comparator and producer-state conditions from FND-EXE-030 still apply.

At the match destination it passes a local record address to `0x00600990`
and then restores its frame on normal return. The bounded matched path
contains no direct replacement of the existing offset-twelve word and
skips the creation calls below. This is not a complete exceptional or
record-helper reading; indirect effects remain conditional.

Creation calls `0x006D6BE0` twice. Relative to the unchanged local stack
pointer, the first call receives local offset 160, the input word read at
208, and local offset 144. The second receives local offset 164, the other
input word read at 212 and the same local offset 144. Local state values
14 and 13 precede those calls. Their output, resource ownership, buffer
sizes and exceptional behavior are not read here.

After those normal returns it passes 16 to the allocation wrapper in
FND-EXE-024, saves the returned pointer and adds eight at 32-bit width.
The guard tests that adjusted pointer for zero, not the original pointer.
A nonzero adjusted value calls `0x006D6C70` first with original pointer plus
eight and local offset 160, then with original pointer plus twelve and
local offset 164. State values ten and nine precede those calls. A zero
adjusted value skips both. The targets' field writes and ownership effects
remain unread; no reachable invalid-pointer case is inferred from this guard.

Both arms next call `0x005FAE40` with the saved original allocation pointer
as first argument and the sentinel address as second, after writing state
value eleven. Thus linkage follows both field-helper calls on the admitted
arm, before the subsequent cleanup paths. Those cleanup paths are outside
the bounded creation range and are not described as transactional here.

The link helper takes two 32-bit pointers, called new node and position
here solely for their use. It reads position plus four, writes position to
new node offset zero, writes that read value to new node offset four,
then rereads position plus four. It writes new node to position plus four,
then writes new node to offset zero of the reread pointer, and returns.
All writes are full 32-bit words. There is no null, allocation or alias
guard in this direct body. The reread is distinct from the first read:
aliased inputs can affect it through the intervening writes. This finding
does not assume different pointers or establish the collection's global
link invariants and concurrent visibility.

## Interpretation

This supplies one direct creation/linkage path for the collection searched
in FND-EXE-029. A full-width comparator match locally bypasses creation;
a sentinel exit locally attempts it. The 16-byte request includes two
link words and positions passed to helpers at offsets eight and twelve,
but does not prove those helpers' storage contracts or pointed-to payload
bounds. Q-EXE-009 still needs their effects, callers, collection initialization,
other writers and cleanup/lifetime behavior. The link sequence records
publication order, not a complete producer or exception reading.

## Alternatives

Always allocating on a match, directly replacing the matched value in this
bounded path, testing the original allocation pointer rather than pointer
plus eight, linking before the two field-helper calls, or using a single
cached position-plus-four read are ruled out by the bounded instructions.
A valid non-aliased sentinel list is consistent with the operations but
requires producer and caller evidence. No local rollback does not establish
what unread cleanup or exception helpers might undo.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Query references to
`0x0240DB50` with ReportReferences' fixed 200-reference cap as a lead, retaining
DATA references and making no complete writer claim. Summarize `0x0058BD90`.
Read 24 instructions from `0x0058BDD6`, 70 from `0x0058BE28`, eight from
`0x0058BF7A`, and 15 from `0x005FAE40`. Restrict claims to the cited ranges;
exclude later resource cleanup and following helpers. Check both sentinel
exits, comparator width, matched-path transfer, call arguments and state
writes, request width, adjusted-pointer guard, linkage ordering and both
position-plus-four reads. Treat helper results, aliases and exceptional
returns as conditional. Keep rich reports local and execute no interpreter
or game.
