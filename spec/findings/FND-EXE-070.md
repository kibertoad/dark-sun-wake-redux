---
id: FND-EXE-070
title: Stored callback handler separates exact state-one finalization from signed counter cleanup and forwarding
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5562..0x005F5595
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F55BF..0x005F55DC
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FACB0..0x005FAD2C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD220..0x005FD2B5
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-055 directly stores `0x005F5562` as its nested record's handler.
At that entry the code adds 24 to the incoming frame-pointer register,
reads the full word at adjusted-frame offset minus 100, and saves it at
adjusted-frame offset minus 176. It then compares the word at adjusted-frame
offset minus 104 with exactly one. These are adjusted-frame accesses;
without the dispatcher's frame producer they are not identified as the
ordinary callback's locals merely because the offsets match.

A value other than one writes zero to the state word and calls
`0x005FACB0` without a newly prepared explicit argument. After normal return
it reserves twelve outgoing bytes, pushes the saved word, writes all ones
to the state word and calls FND-EXE-052's forwarding helper. Cleanup's return
is not tested or retained as a status. The stored saved word and subsequent
callee effects remain conditional on valid frame storage and preservation.

A value exactly one instead reserves twelve outgoing bytes and pushes the
same saved word to FND-EXE-067's classification-one helper. After normal
return it writes one to the state word, removes sixteen outgoing bytes and
calls FND-EXE-041's finalizer. It does not test that helper's return either.
No ordinary handler return or complete dispatcher admission is established
at either terminal boundary. The separate classification-one route between
these ranges is not the handler's local fall-through contract.

The cleanup helper creates its own conventional frame and calls
`0x005FD220`, saving its returned context address before reading that
address's first word as a head. There is no local null-context guard before
this read. A zero head returns through frame restoration without a direct
context or head-field store.

For a nonzero head it reads full words at head offsets 48 and 52 and checks
FND-EXE-055's signature pair. Mismatch first stores zero to the context's
first word, then prepares head plus 48 in one outgoing stack slot for
`0x00601110`. That callee remains unread. On normal return it restores its
frame and returns without testing the callee's result or restoring the head.
The clear precedes the call; it is not evidence of rollback or resource release.

Signature equality reads the full counter at head offset twenty and tests
it at signed width. The complete local branches are:

| Original counter | Ordered direct effects |
|---|---|
| Negative other than all ones | Increment at 32-bit width; store the result at head offset twenty; return |
| All ones | Read context offset four and head offset sixteen; store head-offset-sixteen value to context head, then incremented context counter to context offset four, then zero to head offset twenty; return |
| Positive greater than one | Decrement at 32-bit width; store the result at head offset twenty; return |
| One | Read head offset sixteen and store it to context head; call the unread callee with head plus 48; return after normal completion |
| Zero | Decrement produces all ones, whose signed-negative test reaches the finalizer; no direct counter store precedes it |

For the all-ones arm, both source words are loaded before its first context
store. Its context-counter increment wraps at 32-bit width. The subsequent
head-counter store occurs after both context stores, so aliases can affect
final storage. Other negative values increase toward zero but do not select
the all-ones arm until the incremented word is zero. The one arm does not
store zero to the head counter before calling the unread callee. A generic
decrement-and-store procedure would erase these distinctions.

The helper supplies no common Boolean or separately saved status return.
Its ordinary return register retains path-specific prior reads or callee
results. The studied handler overwrites or ignores it before forwarding or
finalization. Null head and ordinary counter arms do not call the unread
callee, while zero counter reaches FND-EXE-041's finalizer without an ordinary
return established. No direct local rollback reverses earlier writes.

The context getter creates a nested record through FND-EXE-045's setup,
with FND-EXE-055's callback entry, unread metadata `0x006EF294`, adjusted
frame local and stored handler `0x005FD2B6`. It reads the full guard at
`0x0071B180` and initializes a saved result to address `0x0242BE70`. Guard
zero calls FND-EXE-049's cleanup, reloads the saved address and returns.
A nonzero guard, including a negative word, reads the index at `0x0242BE60`,
sets its nested state to one and calls FND-EXE-068's preserved-error lookup
wrapper. It saves that full return without a zero test, calls cleanup,
reloads the saved value and returns through full frame restoration.

Unlike FND-EXE-067/068's provider, this getter does not initialize a negative
guard or allocate after a null lookup. It returns the lookup's null unchanged;
the cleanup helper then dereferences it without a local guard. These are
bounded local control-flow contracts, not a claim that a null lookup is
admitted by all callers or occurs in a run. Static-context initializers,
index validity, imported effects, aliases and stored-handler effects remain
conditional. No original program or API was executed.

## Interpretation

The stored handler now has exact state admission, discarded-helper-return
and forwarding/finalizer contracts, together with the cleanup helper's full
signed counter branch table and its distinct context getter. Q-EXE-009 retains
dispatcher frame/segment provenance, the unread head-associated callee,
static-context producers, lifetime, aliases and remaining stored-handler
contracts. Matching offsets alone do not establish that this dispatch uses
the callback's ordinary frame. No complete exception lifecycle or shell outcome
is established or implemented.

## Alternatives

Treating state one as any nonzero state, testing cleanup's return for success,
always storing a decremented counter, clearing the head counter before the
one arm's call, initializing a negative guard in this getter, or replacing
its null lookup with a newly allocated context is ruled out. A head clear
before an unread call is not proof of release; the handler's adjusted frame
requires its own producer before storage identity is claimed.

## How to reproduce

Use FND-EXE-011's length and XXH3-verified PE and image base. Read forty
instructions from `0x005F5562`, seventy from `0x005FACB0`, and fifty-two from
`0x005FD220`; restrict claims to the cited ranges and exclude ordinary
classification-one, later entries and stored getter handlers. Use
FND-EXE-055 for the handler writer and signature pair, FND-EXE-045/049 for
setup/cleanup, FND-EXE-052 for forwarding, FND-EXE-067 for the helper result,
FND-EXE-068 for lookup, and FND-EXE-041 for finalization. Track adjusted frame
provenance, exact state comparison, saved input across cleanup, signed counter
edges zero/one/all ones/minimum, every ordered store, call admission and the
getter's saved address versus lookup value. Keep dispatcher, callee, initializer,
alias and exceptional effects conditional. Keep rich reports local and execute
no interpreter or game.
