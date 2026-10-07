---
id: FND-EXE-018
title: Compiled selector writers differ in guard order and success meaning
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B4F70..0x004B4FB4
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B88E0..0x004B8921
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F67A0..0x004F67E9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00593E64..0x00593ECA
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction and reference reporters
environment: null
---

## Observation

The analyzer finds two direct writes to the initial selector byte identified
in FND-EXE-017. This is a reading of those two routines, not a complete writer
census: indirect aliases, unrecognized instructions and initialization remain
outside its scope.

The routine at `0x004B4F70` reads a four-byte incoming argument and uses its
low byte. It rejects byte values greater than 26 with an unsigned comparison
before any pointer-table load in this routine. Values 0 and 1 bypass the
pointer check. Values 2 through 26 load the four-byte pointer at that byte's
index in the array at `0x01BE6D60`; null skips the update. An admitted path
stores the byte at `0x01BE6D16`, then calls `0x004F67A0` with first argument
`0xB36` and second argument the zero-extended selector byte. The skipped
paths simply return. This routine has no explicit success-value assignment.

The routine at `0x004B88E0` also consumes only the low argument byte. It
zero-extends that byte and checks the indexed pointer in the same array
before testing the byte's range. A null pointer returns 32-bit zero. With a
nonnull pointer, a byte at most 26 stores the selector and makes the same
subordinate call, then returns 32-bit one on normal completion. A byte greater
than 26 skips both the selector store and that call but still returns one.
There is no local pre-load guard limiting this routine's byte index to 26.
No array allocation or reachable-input claim follows from this observation.

The subordinate routine receives a 32-bit first argument and the low byte
of its second argument. It indexes a pointer array at `0x00B5B6D0` by the
first argument shifted right by 12. With a nonnull pointer it writes the
second argument's byte at pointer plus the full first argument, and returns.
Otherwise it loads an object pointer from the array at `0x0135B6D0` with the
same index and calls the near target at vtable byte offset 20, passing the
object, full first argument and zero-extended second byte. For the selector
writers' fixed first argument, the array index is zero. The arrays' producers,
object types, virtual targets and alias relationships remain unread; this
finding does not identify the byte destination as a particular memory model
or prove that it preserves the selector global after the initial store.

One caller of the second routine lies in the external-command path identified
in FND-EXE-014. The bounded interval reads the first command byte as signed,
uses an imported classification-table word and mask 259 as a preceding
condition, then passes that byte to the call at `0x00601D20`. It subtracts
65 from that return's low byte, zero-extends the byte and passes it to
`0x004B88E0` at `0x00593EB3`. The caller tests only the returned low byte:
nonzero branches to `0x00593E00`; zero enters its diagnostic continuation.
The import semantics, earlier command-selection gates and the full diagnostic
path have not been read here. Consequently, this interval does not yet prove
the actual admitted selector range or the meaning of the continuation.

## Interpretation

A truthy return from the second writer does not by itself prove that it made
its selector store. Its table predicate and update predicate differ. Both
writers admit 26 to their local store, whereas FND-EXE-017's filename helper
requires a selector at most 25 before its initial table load. The distinction
must remain explicit when tracing wrapper resolution. It does not establish
that the declared wrapper reaches the differing branch, that the table has
only 26 entries, or that an invalid access occurs.

## Alternatives

A shared pre-load range guard in both writers, unconditional table checks for
values 0 and 1 in the first writer, and success exactly equivalent to a store
in the second writer are ruled out by their complete local branch sequences.
A caller-specific alphabetic input bound might prevent the differing branch,
but the unread import and command gates do not yet settle that reading.
The subsequent virtual call could affect shared state; a final-state guarantee
cannot be inferred from the preceding direct selector store alone.

## How to reproduce

Use the verified PE and image base from FND-EXE-011. Query references to
`0x01BE6D16`, then summaries for `0x004B4F70` and `0x004B88E0`. Check their
instructions using windows of 22 and 20 instructions respectively; stop at
each stated body's end and disregard the following function. Read 30
instructions from `0x004F67A0`, likewise excluding the next function, to
verify argument widths, both pointer arrays and the indirect target's offset.
Read 45 instructions from the exact boundary `0x00593E64` for the caller's
byte conversion and low-byte return test, interpreting only the cited interval.
Do not start at `0x00593E60`, which is inside an instruction. Enumerate the
first writer's greater-than-26, 0/1, null-pointer and update branches, and the
second writer's null, nonnull-through-26 and nonnull-greater-than-26 branches.
Keep reports local. These checks execute neither the interpreter nor the game.
