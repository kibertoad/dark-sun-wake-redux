---
id: FND-EXE-074
title: Pool-associated wrappers gate exact wait and semaphore imports and convert their full-word returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006019A0..0x006019E5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006019F0..0x00601A28
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602310..0x00602316
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006025C0..0x006025C6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006025D0..0x006025D6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00359136..0x0035914A
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0035918E..0x0035919E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003592B6..0x003592C9
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table reading
environment: null
---

## Observation

FND-EXE-025 and FND-EXE-073 conditionally call these two entries with
`0x02427E50` in the first outgoing word. The first precedes their bitmap
read/update and the second follows bitmap publication. Each wrapper saves
that first original full-word argument as its input address. Neither has
a local null, ownership or initialized-state guard.

Three exact thunks jump through these PE import slots. Physical descriptor
and lookup-thunk reading identifies the KERNEL32.dll names, including their
terminating bytes at the cited shipped-file locations:

| Thunk | Import slot | Imported name |
|---|---|---|
| `0x00602310` | `0x024318AC` | WaitForSingleObject |
| `0x006025C0` | `0x02431854` | InterlockedDecrement |
| `0x006025D0` | `0x02431868` | ReleaseSemaphore |

FND-EXE-048 independently binds `0x00602570`, used by the first wrapper,
to InterlockedIncrement through slot `0x02431858`. Its name and
FND-EXE-024's malloc/free slots are positive physical-import controls.
No import is identified by inferred analyzer names or its position among
nearby calls. This identifies the requested external APIs, not their actual
results or a proved lifetime for the input storage.

### First wrapper

Entry `0x006019A0` reserves outgoing space and passes the input address to
InterlockedIncrement. It removes twelve additional outgoing bytes after
normal return and tests the complete returned word. Exactly zero returns
full zero immediately, without locally reading input plus four or calling
the wait import. Any nonzero word, including a negative word, takes the
wait branch; there is no signed-positive guard.

That branch pushes the increment result twice as auxiliary words, then
all ones, then the full word freshly read at input plus four. Consequently
the callee-facing outgoing words are that freshly read value, `0xFFFFFFFF`,
and two copies of the increment return. The wrapper calls the exact wait
thunk and subsequently pops two auxiliary words. Under the imported
32-bit API calling contract, the first two words are the wait arguments;
the extra two are not additional wait parameters. The outgoing timeout
word is recorded exactly; no original process or wait was executed.

It tests the complete wait return against zero. The pop between that test
and the conditional branch preserves the test flags. Zero joins the same
full-zero return as the increment-zero path. Any nonzero wait result,
including a value with a zero low byte, reserves twelve outgoing bytes,
passes the original input address to InterlockedDecrement and then returns
full one. It discards that decrement's return. There is no local retry,
error query, second wait, distinction among different nonzero wait results,
or direct store of the prior counter value.

The frame restoration drops remaining reserved space; it does not preserve
an import return as the wrapper result. Stack accounting follows the
imported API argument consumption plus the explicit additions and pops,
not an assumption that every pushed auxiliary word is an argument.

### Second wrapper

Entry `0x006019F0` passes the original input address to InterlockedDecrement,
sets a local result to zero, removes twelve additional outgoing bytes and
tests the returned full word as signed. A negative result skips both the
input-plus-four read and the semaphore call, and returns full zero.
Zero is admitted to the semaphore branch, as is any positive result.

The admitted path pushes the decrement result as an auxiliary word, zero,
one and the full word freshly read at input plus four. Thus the
callee-facing words are that value, one, zero and the auxiliary decrement
result. Under the imported 32-bit API contract the first three words are
the semaphore arguments; the last is removed by the following pop. The
wrapper tests the complete semaphore return against zero before that pop.
The pop preserves the test flags, which determine a byte zero-predicate;
that predicate is then zero-extended into the complete returned word.
A zero import return therefore produces one, and any nonzero import return
produces zero. This is a full-word zero test, not a test of only the import's
low byte, and the wrapper does not pass the raw import return through.
There is no error query, retry or direct reversal of the preceding decrement.

### Caller composition and limits

Both studied callers continue after normal return without testing either
wrapper's result (FND-EXE-025, FND-EXE-073). In particular, the first
wrapper's nonzero-wait path returns one after requesting a decrement, yet
the caller still reaches its bitmap operation. Likewise a zero semaphore
return is converted to one and ignored after bitmap publication. These
branches do not establish that a wait or signal succeeded, and neither
wrapper directly restores the bitmap.

The wrappers request increment/decrement operations on the input address
and read the adjacent word only on their respective admitted branches.
Whether that storage contains a valid initialized counter and semaphore,
its lifetime, concurrent changes, external error behavior and the producer
of the shared guard remain open. FND-EXE-073's independent before/after
guard reads can still admit only one of the pair. A decrement request after
a nonzero wait result is not proof of restoring an old counter value in
the presence of other writers or exceptional import exits.

For predicate controls, a first-import return of zero skips waiting while
all ones does not. Wait returns zero and 256 select different wrapper
results even though both have a zero low byte. Second-wrapper decrement
returns minus one and zero select different semaphore admission, and
semaphore returns zero and 256 produce one and zero respectively. These
are instruction-level arithmetic/predicate controls, not observed OS
responses, seeded executions or evidence of real caller admission.

## Interpretation

The formerly unread pool-associated calls now have local branch, import,
outgoing-slot, cleanup and return-conversion contracts. They resemble a
counter-mediated wait/signal pair, but their actual storage initialization,
external behavior and concurrent lifetime are not established. Q-EXE-009
retains those producers and effects, stored-handler admission, optional
callbacks and the wider command-search/continuation reading. No status
promotion, synchronized allocator claim or replacement behavior follows.

## Alternatives

Always waiting, waiting only for a positive increment result, signaling
only for a positive decrement result, testing only an import's low byte,
passing the wait or semaphore result through, treating the auxiliary pushes
as extra API parameters, retrying failed imports, or making the callers
reject nonzero wrapper returns is ruled out by the bounded instructions.
The counter/handle layout may support synchronization when correctly
initialized; the API identities alone do not establish that complete reading.

## How to reproduce

Use FND-EXE-011's PE identity, length and preferred image base. Read 65
instructions from `0x006019A0`, restricting claims to the two cited bodies
and excluding subsequent thunks. Read one instruction from `0x00602310`
and two from `0x006025C0`, following the analyzer gap to `0x006025D0`.
Check the actual jump slots through physical PE import descriptors and
lookup thunks: require descriptor termination inside the import-directory
size, thunk termination within 4096 entries, physical section mapping and
NUL-terminated names within 128 bytes. Select exactly slots `0x024318AC`,
`0x02431854`, `0x02431868` and controls `0x02431858`, `0x024319D0`,
`0x024319A0`. The new name ranges include their terminating NULs.
Use FND-EXE-048 for the increment thunk and FND-EXE-024 for allocation
controls; use FND-EXE-025/073 for actual outgoing input and dropped results.
Trace full-width zero versus signed-negative tests, last writers of the
outgoing slots, imported argument consumption, flag preservation across
pops, frame restoration and publication before the second call. Keep input
producers, alias/concurrency, import and exceptional effects conditional.
Keep rich reports local and execute no interpreter, API or game.
