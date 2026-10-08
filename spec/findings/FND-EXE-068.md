---
id: FND-EXE-068
title: Guarded context acquisition separates initialization, preserved-error lookup and zero-return publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD3A0..0x005FD453
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601840..0x006018AF
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601910..0x00601939
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601940..0x0060196A
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-067's negative context guard enters `0x005FD3A0`. It reads the full
word at `0x0242C910`, sets its accumulator to all ones, and tests the read
word. Zero skips the initialization call and tests the still-all-ones
accumulator, so it necessarily clears the guard at `0x0071B180` to zero
before rereading it. This route does not retain a concurrently changed
nonnegative guard through an earlier reread.

A nonzero read instead writes the nested state to one and calls
`0x00601840`, preparing four full stack slots in callee-facing order:
`0x0071B184`, `0x005FD1F0`, all ones, all ones. It removes sixteen bytes,
then tests the full returned word. Nonzero clears the guard. Zero rereads
the guard and clears it only if that read is negative. It then freshly
rereads the guard again, resets the saved context result to `0x0242BE70`,
and selects common cleanup on zero or FND-EXE-067's lookup path on nonzero.
These are separate reads, not a snapshot or an interleaving guarantee.

The initialization wrapper reads only its first two original stack slots,
as a flag address and callback target. Either null returns full-word 22.
Otherwise a nonzero flag word returns zero. For a zero flag it calls
FND-EXE-048's InterlockedIncrement import with flag-address plus four and
tests the full return for zero. Zero calls the saved callback indirectly,
with no newly populated argument, then directly stores one to the flag word
and returns zero after normal callback completion. Its callback return is
not tested. A nonzero increment result rereads the flag; nonzero returns
zero, while zero repeatedly calls FND-EXE-048's Sleep import with zero and
rereads the flag until nonzero. There is no local wait bound or error-query
branch. The wrapper's low-byte null-test construction supplies the bit later
tested, without relying on uninitialized upper register bytes.

The studied caller supplies nonnull address words for the first two slots,
so the wrapper's 22 return is not its direct admitted null-argument branch.
Valid mapped storage, ordinary import completion and preservation of saved
locals remain conditional. Callback `0x005FD1F0`, the flag/count initializers
and concurrent writers remain unread; the increment's zero branch alone does
not establish a once-only initialization lifecycle or that the callback sets
a positive context guard. The extra two prepared slots are not consumed by
this wrapper's local body.

The positive/nonzero guard lookup uses `0x00601910`. This local wrapper calls
FND-EXE-047's GetLastError import, saves its full return, passes its first
original full-word argument to FND-EXE-048's TlsGetValue import, saves that
full return, and passes the earlier error word to SetLastError. It restores
its frame and returns the saved lookup value, discarding the error-restorer's
return. There is no local lookup-result classification or later error query.
The provider tests that full lookup return: nonzero joins FND-EXE-067's saved
context return; zero enters allocation.

The allocation path requests eight bytes through FND-EXE-024's malloc thunk.
It saves the full result before testing it. Null sets the nested state to one
and calls FND-EXE-041's finalizer. Nonnull freshly reads the index word at
`0x0242BE60` and prepares it with the saved allocation for `0x00601940`.
Two other register-derived words occupy additional outgoing stack slots;
this local callee reads only the first two original slots. The earlier
lookup index and this fresh index need not agree without an interleaving
or lifetime argument.

The setter wrapper prepares its first original argument as the index and
second as the value for FND-EXE-048's TlsSetValue import. A nonzero full import
return produces full-word zero directly. A zero import return calls
FND-EXE-047's GetLastError and returns that full result. This is a converted
return convention, not the raw TLS result. In particular, local control flow
does not distinguish a nonzero TLS result from a zero TLS result followed
by a zero error word; both yield wrapper return zero. No rollback, release
or error preservation pair is present in this setter body.

The provider accepts the setter's full zero return. It then reloads the
saved allocation, writes zero to its first four bytes and next four bytes
in that order, and joins the saved-context cleanup/return. Thus initialization
follows the setter call rather than preceding it. A nonzero wrapper return
instead sets the nested state to one and calls the finalizer. There is no
local allocation release before that call, and no ordinary continuation or
return from finalization is established. The zero return does not by itself
prove that the TLS publication succeeded, nor that another observer could
not see the allocation before these two stores.

Under normal zero-return publication and valid distinct storage, these two
stores provide direct initializers for the context head and counter consumed
by FND-EXE-067. That is a branch-specific writer contract, not proof that its
static address context, previously returned TLS contexts or exceptional paths
have those initial values. Alias, external and handler effects remain
conditional. No original program or import was executed.

## Interpretation

The context provider's guarded routes now have local initialization admission,
lookup return, allocation and publication-result contracts, including the
precise ordering of its two zero stores. Q-EXE-009 retains the initialization
callback, flag/index/static-context producers, import effects, interleaving,
record lifetime and stored-handler admission. The zero-return convention
cannot support an unconditional successful-publication claim or a complete
context lifecycle. No replacement runtime or shell outcome is implemented.

## Alternatives

Always retaining a reread guard on the skipped-call branch, waiting only once,
testing the callback's return, returning the error-restorer's value, testing
only a lookup byte, treating the setter as a raw Boolean result, initializing
the allocation before publication, or inferring a zero context from every
returned address is ruled out. The allocation's saved pointer and a zero
setter return establish local accesses, not successful external publication
or identity with the static context.

## How to reproduce

Use FND-EXE-011's length and XXH3-verified PE and image base. Read sixty-five
instructions from `0x005FD3A0`, thirty from `0x00601840` and one hundred
from `0x00601880`, restricting claims to the four cited bodies and excluding
other wrappers. Use FND-EXE-067 for entry/common cleanup and return,
FND-EXE-024 for malloc, FND-EXE-047/048 for exact PE import-slot bindings,
and FND-EXE-041 for finalization. Track full guards and separate rereads,
initialized low-byte predicate, consumed versus prepared slots, callback
completion before flag publication, unbounded polling, saved error/lookup
returns, fresh setter index, converted zero result, and zero stores after
publication. Keep callback, initializer, library, alias, interleaving and
exceptional effects conditional. Keep rich reports local and execute no
interpreter or game.
