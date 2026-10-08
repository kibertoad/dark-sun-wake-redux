---
id: FND-EXE-081
title: First selected startup callbacks preserve two empty bodies, increment paired words and initialize a distinct context
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FF590..0x005FF595
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FDB40..0x005FDBED
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FDAB0..0x005FDAB5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FC860..0x005FC875
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading with physical callback-table provenance
environment: null
---

## Observation

FND-EXE-080 physically resolves the first four callbacks selected by its
reverse table dispatch. This finding reads those concrete entries only;
normal return, preserved saved counter and an unchanged table remain the
conditions for the parent reaching each later callback.

The first target `0x005FF590` and third `0x005FDAB0` each only save the frame
register, establish a frame, restore it and return. Their complete local
bodies read no original argument, call no helper, directly publish no non-stack
state and leave the return register unchanged. Neither returns an explicitly
constructed success predicate. In particular, the nearby `0x005FF5A0`
initializer is not the first table target; the instruction reporter prints
it only after the first body's return and a gap. Its behavior is excluded.

### Second selected callback

Entry `0x005FDB40` directly updates twelve adjacent low/high word pairs.
Each pair's low word gets a 32-bit add of one, followed immediately by a
32-bit add-with-carry of zero to its high word at plus four. No call or
flag-writing instruction intervenes within a pair. The pair bases, in
actual execution order, are:

| Order | Low-word address | High-word address |
|---|---|---|
| 1 | `0x0071B210` | `0x0071B214` |
| 2 | `0x0071B218` | `0x0071B21C` |
| 3 | `0x0071B260` | `0x0071B264` |
| 4 | `0x0071B268` | `0x0071B26C` |
| 5 | `0x0071B248` | `0x0071B24C` |
| 6 | `0x0071B230` | `0x0071B234` |
| 7 | `0x0071B238` | `0x0071B23C` |
| 8 | `0x0071B220` | `0x0071B224` |
| 9 | `0x0071B258` | `0x0071B25C` |
| 10 | `0x0071B250` | `0x0071B254` |
| 11 | `0x0071B240` | `0x0071B244` |
| 12 | `0x0071B228` | `0x0071B22C` |

For ordinary sequential execution without interfering writers, each pair
therefore receives an add of one modulo 64 bits, with its low word first.
A low word of all ones becomes zero and carries one into the high word;
a pair of all ones becomes two zero words. Other low inputs increment
without changing the high word. These arithmetic controls describe the
instructions, not observed initial fields or a semantic name for them.
The paired stores are separate and have no lock prefix: this is not an
indivisible publication or a measured concurrency contract.

The first pair is written before frame setup, the second between saving
and establishing the frame, and the last five after restoring the frame
register. Those ordinary paths still reach the remaining direct updates
and the final return. There is no helper call, original-argument read,
result construction, local rollback or conditional early exit in this body.
The field meanings, producers and other users remain open. These concrete
stores are separate from shared guard `0x0242C910`.

### Fourth selected callback

Entry `0x005FC860` reserves twenty bytes and pushes `0x02427E30` as one
full-word input to FND-EXE-075's initializer `0x00601970`. After normal return
it removes sixteen outgoing bytes, restores its frame and returns with
that initializer's full result unchanged. There is no local result test,
conversion, release, retry or guard predicate. Remaining frame reserve
is discarded by frame restoration; it is not another consumed argument.

On that initializer's normal path, the supplied address receives all ones
and its plus-four word `0x02427E34` receives the unchecked CreateSemaphoreA
return, in that order (FND-EXE-075). This is distinct from the failure-pool
input `0x02427E50` and its adjacent word. It supplies a concrete initialization
path for the address written into a frame local by FND-EXE-077, without
proving that every later read of that local retains the same value or that
creation succeeded. No import or original callback was executed.

FND-EXE-080's parent does not test any of these callback results before its
next decrement/dispatch. A zero creation return from this fourth callback
still permits the next selected callback on normal return. Its prepublished
guard remains one; these bodies do not clear it or certify all initialization.
The first four local contracts do not determine the other selected targets,
import effects, shared-guard writers, table mutation or complete lifetime.
The function inventory gains only these four recovered starts and their
analyzer body sizes; no names, code or inferred contiguous spans are added.

## Interpretation

The selected startup prefix now has bounded callback effects and a concrete
initializer for the distinct context address used in FND-EXE-077. No direct
store in these four callback bodies assigns the shared guard; the fourth's
external effects still prevent a complete no-writers claim. Q-EXE-009 retains
remaining startup callbacks, paired-field provenance/meaning, aliases,
external creation and dispatcher/frame admission. No complete launch outcome
or replacement initialization behavior is implemented.

## Alternatives

Assigning the first callback the adjacent initializer's behavior, interpreting
an unchanged return register as an explicit zero result, treating each pair
as two unrelated increments, discarding carry, initializing the failure-pool
address instead of the supplied context, testing the creation return here,
or assuming the table stops on that return is ruled out. Paired arithmetic
alone does not name the fields or prove atomicity or successful creation.

## How to reproduce

Verify FND-EXE-011's source identity and FND-EXE-080's physical target slots.
Read 32 instructions from `005FF590`, twelve from `005FDAB0`, 42 from
`005FDB40` and fifteen from `005FC860`; restrict every claim to the cited
bodies and exclude instructions printed after their returns/gaps. Follow
each low add's carry immediately into its own high add, retaining pair
execution order and frame stores apart from non-stack publications. Use
FND-EXE-075 for initializer input consumption, API binding, publication and
raw return; FND-EXE-077 for the distinct context-address producer candidate;
and FND-EXE-080 for actual selection and dropped callback returns.
Recover only the four physically selected entries with RecoverCitedFunctions,
then export the allowed start/size inventory, preserving its address format
and reviewing every changed row. Keep field/alias/runtime and later callbacks
conditional. Keep rich reports local and execute no original interpreter.
