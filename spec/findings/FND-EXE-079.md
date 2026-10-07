---
id: FND-EXE-079
title: Declared startup reaches an x87 initializer and a memory-update helper whose equal table bounds skip its loop
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00401210..0x00401222
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00401110..0x00401201
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005C4500..0x005C4527
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005C4530..0x005C4536
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005C4400..0x005C4437
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003593D2..0x003593E0
tool: Ghidra 12.1.3 PUBLIC bounded instruction windows and physical PE import-table reading
environment: null
---

## Observation

FND-EXE-078's header-declared entry is `0x00401210`. Its conventional frame
reserves twenty bytes and pushes one as a full word before calling through
PE slot `0x02431914`. Physical import-descriptor and lookup-thunk reading
identifies that exact slot as msvcrt.dll's `__set_app_type`; the cited name
range includes its terminating NUL. FND-EXE-024's malloc/free slots and
FND-EXE-048's increment slot are mapping controls. The caller then directly
calls `0x00401110` without testing the import's return. The analyzed entry
has no recorded ordinary continuation after that call. Missing fall-through
is not used here as proof of how the interpreter terminates.

The startup callee first calls `0x00602270` with `0x00401000`, then
calls `0x005C4530`. The earlier callee's effects are not resolved here.
The local `0x005C4530` body makes a frame, executes the x87 initialization
instruction, restores the frame and returns. It has no call or explicit
non-stack memory store in this bounded body. The instruction concerns FPU
state; it is not a write to FND-EXE-078's guard or pool words. Hardware and
exceptional effects are not measured by this reading.

Later ordinary paths in `0x00401110` call `0x00601C80`, load the word at
`0x0071A9C0` and store it through the pointer returned by that call. Only
after that indirect store do they call `0x005C4500`. The return-pointer
producer and possible aliases remain unread. Thus the empty helper below
cannot be used to conclude that the entire preceding startup wrote no
shared state.

### Equal-bound memory-update helper

The actual direct callee at `0x005C4500` establishes a frame and sets its
working cursor to `0x0075A608`. It immediately jumps to the loop condition,
which compares that cursor unsigned against the identical constant
`0x0075A608`. The condition branches into the row body only when the cursor
is below the end. Equality is false for that branch, so the admitted ordinary
entry skips the row body and restores its frame/returns. There is no import,
call or memory read between the cursor assignment and this comparison that
could supply a different bound or initial cursor.

The separately present row body reads two consecutive full words at the
cursor, advances the cursor by eight and adds the first read word to the
full word at the second read word plus `0x00400000`, using 32-bit arithmetic.
It rejoins the same unsigned condition. This describes the local dormant
body, not a nonempty shipped table or proof that arbitrary inputs are admitted:
the ordinary entry supplies no arguments as row bounds and its start equals
its end. Its first-iteration memory reads and target update do not execute
on that entry path. An externally entered interior or modified code/register
state is outside this ordinary-entry claim.

The helper's ordinary entry therefore supplies no guard or pool initializer
through that row-update mechanism. Stack frame writes are kept separate from
the dormant non-stack store. No old table contents, implicit one-row behavior,
virtual zero bytes or guessed terminator are substituted for the equal bounds.

### Remaining startup route

After this helper's normal return, the parent aligns/reserves its outgoing
stack and eventually calls `0x005C4400` with freshly read words from
`0x0075B004`, `0x0075B000` and the pointer-derived word read after
`0x00601C60`. These supplied values and order are visible; their argument,
environment and runtime meanings are not strengthened by inferred names.
The parent saves this later callee's full return before a further helper,
then writes the saved word into the first outgoing slot before `0x00602280`.
Those later effects and the final call's external contract remain unread.

A bounded prefix at `0x005C4400` establishes/aligns its own frame and directly
calls `0x006017F0`, then `0x005C45C0`, then `0x006024F0`. It saves the last
return and passes a frame-minus-88 address to `0x00602500`. After normal return
it tests the saved full word, taking the nonzero target `0x005C4439` or the
zero jump target `0x005C44C3`. The earlier helper and import effects, target
continuations, constructor/admission contracts and any indirect writes still
need evidence. A name or position in startup is not a complete initialization
reading.

## Interpretation

The declared startup supplies grounded callees for the guard's initialization
search. One local body changes FPU state and another skips its potential
memory-update loop because its actual bounds are equal. Neither is a direct
producer of the virtual guard through the studied local mechanism. Q-EXE-009
retains imported/indirect effects, the later initialization helper and runtime
storage/lifetime; the entire startup and eventual guard value remain open.
No original process, API, FPU or loader was executed.

## Alternatives

Scanning at least one row despite equal bounds, testing equality instead of
unsigned below, taking row bounds from unfilled caller slots, or assigning
all startup initialization to these two local helpers is ruled out. The
indirect store and unread later callees prevent a no-startup-writers claim.
The absent ordinary continuation after the declared entry's final call is
not itself a verified exit, throw or successful launch outcome.

## How to reproduce

Verify FND-EXE-011's source identity and FND-EXE-078's header entry. Read fourteen
instructions from `00401210`, eighty from `00401110`, thirty-five from
`005C4530`, fifteen from `005C4500` and eighteen from `005C4400`. Restrict
claims to the cited bodies/prefix, excluding later entries printed after gaps.
Resolve exact slot `0x02431914` through physically bounded import descriptors
and lookup thunks, with NUL-terminated names limited to 128 bytes, descriptor
termination inside the directory and thunk termination within 4096 entries.
Check controls `0x02431858`, `0x024319D0`, `0x024319A0` against
FND-EXE-024/048. Track declared-entry admission, the direct calls in execution
order, pointer-return indirect store, the helper's jump to its unsigned
condition, both constant bounds, inaccessible row body, later outgoing word
order and saved return. Keep external, alias and interior-entry effects
conditional; keep rich reports local and execute no original interpreter.
