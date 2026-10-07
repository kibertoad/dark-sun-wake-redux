---
id: FND-EXE-106
title: Callback pool links are initialized before head publication and the counter helper retains callback successors
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F2C36..0x004F2C9E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F28B0..0x004F2905
tool: Ghidra 12.1.3 PUBLIC bounded callback pool and counter reading
environment: null
---

## Observation

FND-EXE-105 describes reuse of the free head at `0x01D291E0` and
active head at `0x01D291E4`. A reached initialization segment starts at
`0x004F2C36` by setting a full index to zero. Each iteration retains the
old index, multiplies it by sixteen, increments the index, forms the address
`0x01D271F0` plus sixteen times the old index, and stores that address at
`0x01D271EC` plus sixteen times the old index. The unsigned repeat test
is incremented index at most `0x01FE`. Thus old indices zero through 510
write successor links for records starting at `0x01D271E0`, with stride
sixteen, ending in the successor address `0x01D291D0`. This segment then
stores full zero at `0x01D291DC`, that last record's offset-twelve link,
before publishing `0x01D271E0` as free head. The resulting local link
construction covers 512 records. It does not clear their fields zero, four
or eight.

The segment explicitly clears EAX and publishes full zero as active head
at `0x004F2C99`. No intervening call separates the index initialization,
link loop, terminal-link write and these head publications. Earlier calls,
entry admission, repeated initialization, other writers and aliases remain
unread contracts; this is not proof that either list is always valid. A
later call can change the published state, and no durable lifetime follows
from this reached segment alone.

The complete local body `0x004F28B0..0x004F2905` saves EBX and
reserves eight stack bytes. It first publishes full zero to `0x0075B0E8`,
reads active head and full `0x006F00A0`, increments full
`0x01D271CC` modulo thirty-two bits, then publishes the retained
`0x006F00A0` value to `0x006F00A4`. With a nonnull active head it
loads floating one, visits each node, subtracts that retained one from the
node's single-precision field zero and stores the single-precision result
back before reading offset twelve as its next node. At exhaustion it pops
the retained floating one. A null initial head skips this floating loop.
No node links or callback/argument fields are directly changed in that loop.
Floating environment, exceptional behavior and list validity are not gated.

Only after that loop it reads a separate head at `0x01D292B0`. Each
nonnull record contributes its offset-four successor, retained in EBX
before calling the full target at offset zero with no locally written
argument. After an ordinary return it follows that saved successor rather
than rereading the record's link or the global head. A callee mutation of
those locations therefore does not by itself replace the retained successor.
This statement assumes normal register preservation and valid storage;
callee targets and exceptional effects remain unknown. Exhaustion restores
the local stack and saved EBX and returns full EAX zero from the final null
pointer test. Neither loop contains a cycle check or iteration bound.

## Interpretation

The pool construction supplies a concrete initial free chain for the reuse
operations in FND-EXE-105. The helper reached from the wait path in
FND-EXE-103 modifies active-node floats before traversing a different
callback list. Its second traversal preserves the next pointer across the
call. Q-EXE-009 in FMT-EXE-006 still requires initialization admission,
callback producers/targets, storage lifetime, floating contracts and native
conditions; these observations do not establish actual PATH behavior.

## Alternatives

- The terminal free link is cleared explicitly; it is not inferred from
  neighboring zeroed storage or an assumed allocator.
- Pool link construction does not initialize each callback or float field.
- The counter helper does not remove active records or dispatch their
  offset-eight targets in its floating loop.
- Its separate callback traversal uses a pre-call successor, not a fresh
  post-call link. Unknown callee effects cannot be assumed absent.

## How to reproduce

Verify the executable length and hash in FND-EXE-011 and the physical target
controls in FND-EXE-099. Use the saved Ghidra program with -noanalysis.
ReportReferences.java targets `0x01D291E4` and `0x01D291E0`, cap
200 each, identify the initializer leads `0x004F2C99` and
`0x004F2C74`; FND-EXE-105's read at `0x004F24F0` and write at
`0x004F24F9` supply independently read controls. These are leads only,
not an exhaustive writer search. ReportInstructionContext.java at
`0x004F2C74` and `0x004F2C99` gives eight instructions on each side.
ReportInstructionWindow.java at `0x004F2BD0` limit 80 covers the pool
segment; restrict the observation to its declared range. The window at
`0x004F28B0` limit 65 covers the helper through its return; exclude the
following procedures. Check old versus incremented loop index, final link
address, publication order and successor retention across the indirect call.
Keep reports in GAME_DIR/analysis/exe-batches; commit no original listings
or bytes and execute neither the interpreter nor the game.