---
id: FND-EXE-014
title: Compiled batch-selection branch gates active-batch cleanup with a CALL flag
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00593BE0..0x00593E24
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00593F30..0x00593F47
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00598110..0x005981F5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0033807E..0x00338083
tool: Ghidra 12.1.3 PUBLIC, pinned bounded reporters
environment: null
---

## Observation

The dispatcher in FND-EXE-013 calls `0x00593BE0` for a nonempty command
that did not match its internal records. This routine's stack probe
requests `0x11FC`; using the probe mapping in FND-EXE-012, its incoming
object, command and suffix are at post-probe offsets `0x1210`, `0x1214`
and `0x1218`. The object is retained at local offset `0x50`. Calls and
exception registration before the branch described here are not a complete
reading; the following observations apply to their normal continuation.

The call at `0x00593D04` supplies object and command to `0x005933F0`.
A null result goes to a false-result continuation. A nonnull result is
passed through another helper to a local buffer, and another call with
that buffer and byte value 46 supplies the pointer retained at local
`0x48`. A null pointer takes a different branch, outside this reading.
For a nonnull pointer, comparison through the FND-EXE-012 imported thunk
against the shipped ASCII `.bat`/NUL literal is tested at full 32-bit
width. Equality reaches `0x00593D7B`. This does not yet establish the
lookup helper's search order or which host file produced the result.

The equality continuation saves the object's byte at `0x2C` and reads
its 32-bit field at `0x28`. If that field is null it skips old-object
cleanup. If nonnull, `0x00593F30` tests the object's byte at `0x2E`:
nonzero skips cleanup; zero calls the second entry of the old object's
pointer table at `0x00593F3F`. For an old object initialized as in
FND-EXE-012, that entry is the already recorded cleanup target. This
finding does not silently assume every possible incoming object has that
provenance or that cleanup always returns normally.

After these branches rejoin, the code passes value `0x20` to
`0x005F7E10` and retains its result. It then calls `0x0059EFB0` with that
result, the shell object, the local selected-name buffer, the incoming
command, and the constructed suffix buffer, in that order. On normal
return, it writes the saved echo byte back to shell offset `0x2C`, then
writes the retained result into shell offset `0x28`, and assigns result 1.
The allocator's units, failures and exception paths are not established
by its request argument, and constructor failure is not described as
transactional or safe by this observation.

The compiled CALL record is in the prefix read in FND-EXE-013 and selects
`0x00598110` without adjustment. On its non-help continuation, the store
at `0x005981A8` sets the shell byte at `0x2E` to 1. It passes shell and
argument pointer to parser `0x005904B0` at `0x005981B3`, then the store at
`0x005981B8` clears the byte to zero on normal return. It does not restore
a saved prior byte. Help/output paths are outside this flag-interval claim.

## Interpretation

The source prediction in SRC-DOSBOX-GOG-0742 has compiled support for a
conditional batch-selection distinction: an active batch is passed to
cleanup on the unflagged path, while CALL brackets parser execution with
the flag that skips that cleanup. Combined with FND-EXE-012, this supports
normal-path replacement rather than unconditional preservation of the
active batch. It does not settle Q-EXE-009's actual bare-command resolution,
mutable overlay contents, whole continuation or EXIT behavior.

## Alternatives

Unconditionally keeping every active batch is ruled out by the flag-zero
cleanup call. Unconditionally cleaning it is ruled out by the flag-nonzero
skip. Automatically restoring CALL's prior flag value is ruled out by the
literal zero store on normal return. Which file the GOG wrapper resolves,
what happens after failed allocation, and whether exceptional parser paths
clear the flag remain open rather than inferred from these local branches.

## How to reproduce

Use the verified manifest file and PE base from FND-EXE-011. Summarize
`0x00593BE0` with the pinned `ReportFunctionSummary.java`; the recorded
decompiler failed with a varnode-adjustment error. Do not replace that
failure with guessed source. Read instruction windows at `0x00593BE0`
(175 instructions) and `0x00593F30` (35), interpreting only the locations
above for this claim. Follow the null guards, comparison width, saved-byte
write, cleanup call and normal constructor continuation directly.
Read 24 bytes at `0x0073AE7C`; the `.bat`/NUL literal begins at
`0x0073AE7E`. Search its five ASCII bytes with
`ReportPhysicalBytePattern.ps1`, maximum 16 matches; the recorded shipped
match is `0x0033807E`.

Recover the table-evidenced entry `00598110` if absent, summarize it and
read a 90-instruction window from `0x00598110`, interpreting only
`0x00598110..0x005981F5`, with exclusive end. Inspect both byte stores
and the parser's two outgoing arguments. Use FND-EXE-012 for the stack
probe and conditional cleanup target, and FND-EXE-013 for the command-table
selector. Keep external-helper and constructor assumptions explicit; no
original program, shell harness or DOSBox was run.
