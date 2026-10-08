---
id: FND-EXE-191
title: Adopted-pointer recovery decodes a fixed buffer prefix before checking its record header
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600530..0x006005B0
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-190's recovery helper at `0x00600530` masks incoming EAX to
its low word, saves EBP and EBX, and reserves `0x58` stack bytes. It
forms a buffer address at `EBP - 0x58`, pushes capacity `0x42`, that
address and the masked input, then calls `0x00602530`. It tests returned
EAX at full dword width. Zero branches to a failure call; every nonzero
value reaches the same fixed-length decoder. There is no local comparison
of the return with the thirty-two bytes that decoder reads.

The decoder starts with EBX zero, EDX 31 and ECX one. It reads the buffer
at offsets 31 down through zero. When a byte equals `0x41`, it ORs the
current ECX mask into EBX. Every other byte takes the no-OR path. Both
paths double ECX at dword width and decrement EDX before testing whether
EDX remains nonnegative. Thus byte position j, for zero through 31,
contributes bit `31 - j` exactly when that byte is `0x41`. It performs
thirty-two byte reads regardless of the nonzero retrieval count and does
not validate an alternate-byte alphabet or stop at a terminator.

After those reads, the helper compares the dword at the recovered EBX
address with `0x3C`. It performs that dereference without a local nonzero
or allocation-extent check. Equality returns EBX in EAX, restores the saved
EBX and EBP through LEAVE, and returns near at `0x0060057E`, with no
extra argument cleanup. This is a header predicate, not storage admission.
FND-EXE-190's callers use the returned dword as an adopted pointer or
compare it with their candidate before publication; its validity and
preservation through those callers remain obligations.

A mismatched header calls `0x00601F80` at `0x0060058F`, with outgoing
values including `0xEA`, `0x00754E74` and `0x00754EB0`. A zero retrieval
result calls that same target at `0x006005A4`, with outgoing values
including `0xE4`, `0x00754E74` and `0x00754EE0`. Under an assumption
that the first failure call returns, its continuation reaches the second
call. If the second also returns, local traversal reaches the exclusive
region end at the neighboring initializer's entry. No local error return
or cleanup is inferred for either failure path.

The controlled source import table identifies `0x00602530` through slot
`0x02431820` as KERNEL32.dll `GetAtomNameA`, and `0x00601F80` through
`0x02431918` as msvcrt.dll `_assert`. Their loaded argument, cleanup,
initialization and failure effects remain unread. SRC-WIN32-ATOMS supplies
the external retrieval-count contract only; it does not prove the native
buffer's initialized extent or the admitted atom's identity.

The original-source traversal covers 123 bytes at
`0x00600530..0x0060055B` and `0x00600560..0x006005B0`, including its
one local near return. It remains incomplete because the assumed failure-
call continuation reaches the exclusive region end. Its five-byte hole
and all call-return assumptions remain separate from native evidence.

## Interpretation

This resolves FND-EXE-190's local low-word input, bit decoding, header
predicate and normal return. A nonzero retrieval result alone does not
admit the fixed prefix. The recovered header also cannot prove what may
be read after it or how long the record exists. Q-EXE-001 and Q-EXE-010
retain initialized-prefix producers and admitted identifiers, unchanged query
names, external effects, stored-record extent/lifetime and failure completion.
No formal complete reading follows.

## Alternatives

Treating the retrieval count as the decoder's bound is contradicted by the
fixed thirty-two iterations. Treating every non-`0x41` byte as rejected is
contradicted by the no-OR path. Treating a matching size header as proof of
valid allocation conflates a dereferenced value with storage admission.
Assuming `_assert` does not return would suppress conditional continuations
that still need the actual loaded failure contract.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
Run the committed wrapper's `x86-bounds` with sourceKind `pe32`, entry
2095408, one named region `[2095408,2095536)` entries `[2095408]`,
and evidence naming bounded recovery with initialized input extent, imported
effects and failure continuation unresolved. Omit segment/ip, seeds and
callee summaries. Preserve both reached intervals, the five-byte hole,
local return and incomplete exclusive-end edge.

In the saved shipped-PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x00600530`, count 24; at `0x00600560`,
count 28; and at the two import trampolines, count one. Restrict the
reading to the declared location and exclude the neighboring initializer
beginning at `0x006005B0`. Run `x86-imports` with FND-EXE-187's
independent controls, slot `0x024319D0`/MSVCRT.DLL/`malloc` and
`0x024319E4`/MSVCRT.DLL/`memset`; select the two named slots by address.
Keep rich reports in the local licensed-source store, outside Git;
no original-game execution is involved.
