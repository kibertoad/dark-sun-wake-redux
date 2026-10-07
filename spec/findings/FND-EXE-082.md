---
id: FND-EXE-082
title: Registered cleanup dispatch rereads a mutable cursor before publishing its next slot
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005C4540..0x005C4568
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003195D0..0x003195D3
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EB390..0x002EB3EB
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading and physical PE pointer-table reading
environment: null
---

## Observation

FND-EXE-080 supplies this entry as the outgoing callback in an atexit
registration attempt. It does not establish that registration succeeds or
that an external runtime invokes the callback. This finding reads the entry
and its shipped cursor/table, with normal returns and readable memory explicit.

The entry loads the full word at `0x0071A9D0` into its working pointer, then
compares the full word through that pointer with zero. It does not test the
pointer itself before dereferencing it. A zero pointee skips all callbacks
and cursor stores; the loaded pointer remains in the return register.

A nonzero pointee reaches an indirect call through the working pointer's
current full word. No newly populated argument slots precede this call. After
normal return, the entry rereads the global cursor into another register;
it does not keep the original cursor as the source of progression. It forms
that fresh cursor plus four with 32-bit wrap, reads the full word at that
advanced location, then publishes the advanced cursor to the global. The
read precedes publication. Only then does it test the saved next word.

A zero saved next word reaches frame restoration and return. The advanced
cursor remains in the return register, not a Boolean or a callback result.
A nonzero saved next word repeats the indirect call through the advanced
working pointer, reloading that location's target at the call itself. The
saved tested word is not the target passed to the call. Mutation between the
test and call may therefore change the target; there is no second local
nonzero check immediately before calling it.

An exceptional callback transfer does not reach this entry's post-call
cursor update; the callback's own effects remain possible. Likewise, a
failure of the advanced-word read precedes its local publication. No local
bounds, retry, error query, completion marker or rollback is shown. These
are instruction-order observations, not proof of a particular exception or
concurrent actor occurring in the original.

### Shipped pointer admission

The physical full word at `0x003195D0` maps to cursor `0x0071A9D0` and contains
`0x006EBF90`. Consecutive table words begin at physical `0x002EB390`. There
are 22 nonzero words before the first zero, at virtual `0x006EBFE8`, physical
`0x002EB3E8`. The four first nonzero targets are:

| Zero-based slot | Virtual slot | Shipped offset | Stored target |
|---|---|---|---|
| 0 | `0x006EBF90` | `0x002EB390` | `0x00418870` |
| 1 | `0x006EBF94` | `0x002EB394` | `0x004A7F10` |
| 2 | `0x006EBF98` | `0x002EB398` | `0x004A8640` |
| 3 | `0x006EBF9C` | `0x002EB39C` | `0x004ADA40` |

The initial cursor points directly at a target word, not a count/header to
skip. A sixteen-word query stops without the terminator and is rejected as
partial; a 128-word query reaches the first zero. The query cap is not a
runtime bound. FND-EXE-083 reads the first target. The other targets remain
unread here, and no inverse relationship with FND-EXE-080's 29 constructor
slots is established. Under an unchanged cursor/table and normal returns,
this entry visits the nonzero words in ascending slot order. That condition
is not proof that every callback preserves the cursor or table.

The indexed reference report returns the initial read, fresh read and store
at `0x005C4545`, `0x005C4552` and `0x005C455E`. Its indexed domain is not an
all-writer search: indirect aliases, undecoded operands, external runtime
writes and copied pointers remain outside a completeness claim.

## Interpretation

The registered callback has a precise mutable-cursor and publication-order
contract. Q-EXE-009 retains actual registration/invocation, cursor initial
state and writers, table lifetime, remaining targets and callback effects.
No complete shell, cleanup lifecycle or native execution is established.

## Alternatives

A null-cursor guard, a skipped leading header, reverse dispatch, progression
from a saved pre-call cursor, publication before the next read, calling the
saved tested target, callback-return admission and a Boolean success return
are ruled out by the local entry. Twenty-two shipped nonzero words do not
bound mutated inputs or prove successful cleanup.

## How to reproduce

Verify FND-EXE-011's exact executable length and XXH3 identity. Read 22
instructions from `005C4540`, restricting claims to the cited body and
excluding the later entry after the gap. Map cursor `0x0071A9D0` and every
four-byte table word through PE section raw extents; require all four bytes
to map contiguously within the file. Query with caps 16 and 128, stop only at
the first zero and reject a reached cap without zero. Record the cursor,
terminator and exactly the first four target words as concrete reading inputs.
Run ReportReferences.java for `0071A9D0` with its 200-reference cap, preserving
its indexed-domain limits. Trace fresh pointer reads, widths, wrapping
arithmetic, the pre-publication next read and the later target reload. Keep
source reports local; do not invoke a callback, the runtime or the original.
