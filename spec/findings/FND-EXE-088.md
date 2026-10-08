---
id: FND-EXE-088
title: Status-overlap transfer constructs before decrement admission and keeps distinct handler routes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F6F10..0x005F6FAE
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F6FB0..0x005F6FF8
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FEB60..0x005FEC1F
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading with explicit stored-handler recovery
environment: null
---

## Observation

FND-EXE-086's status overlap tail-transfers to `0x005F6F10`, having replaced
the first input slot with `0x00754560`. This target builds a local record
at frame -124, with callback `0x005F50A0`, metadata `0x006EEA84`, saved
handler `0x005F6FB0` and frame/stack words, then calls `0x006008F0`
(FND-EXE-045). It calls `0x006D6BE0` with local -40 as its first output,
incoming first input as its second and local -56 as its third, after
writing two to local record state -120. Producer behavior and the validity
of its output remain unresolved here, as in FND-EXE-036.

It next requests eight through `0x005FCD70` and saves the returned pointer
at local -128. FND-EXE-025 records the allocator's separate eighty-byte
augmentation and fallback limits. The caller has no local returned-pointer
null guard. It calls `0x005FEB60` with the allocated pointer and address
of local -40. On normal return it freshly reads local -40, subtracts twelve
with 32-bit arithmetic and saves that prefix pointer at local -132.
Equality with `0x0071B270` bypasses the decrement route.

Otherwise it supplies payload minus four and full addend all ones to
`0x005F5760`, after writing one to state -120. FND-EXE-033 establishes
that the returned word is the old value of the locked exchange-add.
Only signed old value greater than zero bypasses the following release.
Zero or negative old values supply saved prefix -132 to `0x006D4DF0`,
with address of local -72 in the next slot and two ESI-valued auxiliary
slots whose meaning is not established here. FND-EXE-040 records that
the direct release helper reads only its first argument. It does not
locally normalize a release result or clear this saved pointer.

Both bypass and normally returning release routes write all ones to
state -120 and call `0x005FAED0` with saved allocated pointer first,
`0x00755AE4` second and callback `0x005FE960` third. A fourth auxiliary
slot receives current EDX, whose value across the intervening calls is
not proved to remain the prefix. FND-EXE-025 records the publication
helper's direct consumption of its first three arguments. This local
sequence does not undo the construction or decrement before publication.
The selected callback's later effects are not covered by this finding.

The explicitly stored handler `0x005F6FB0` adds twenty-four to incoming
EBP, reads adjusted-frame word -116, writes all ones to state -120 and
supplies that word to `0x00600EB0` (FND-EXE-052). Its native frame binding
is unproved. An unexpected normal return would enter `0x005F6FC6` with
the forwarder's current EAX, not necessarily the ordinary payload value.
That physical adjacency cannot establish identical handler inputs.

Constructor `0x005FEB60` saves its incoming first pointer at local -68,
sets up a record at -64 with callback `0x005F50A0`, metadata `0x006EF70C`
and stored handler `0x005FEBD0`, and calls the record setup helper. On
normal return it reloads its saved pointer and takes its second input.
It stores full word `0x00759B28` through the reloaded pointer before
field publication, forms pointer plus four, writes one to local state -60 and calls
`0x006D6C70` with that adjusted pointer and second input. FND-EXE-032
records this field helper's separate publication branches. The constructor
then cleans its record through `0x00600990` (FND-EXE-049) and returns
that raw EAX, rather than restoring an allocated-pointer return. Its
initial pointer store has no local null guard or rollback.

The constructor's stored handler adds twelve to incoming EBP, saves
adjusted-frame word -56 at local -72, loads pointer word -68, and saves
word -52 at local -76 before calling `0x005FD000` with the loaded pointer.
After normal return it compares the saved full local -76 with all ones.
Inequality writes all ones to state -60 and forwards saved local -72 to
`0x00600EB0`. Equality, or that forwarder's unexpected normal return,
writes all ones to state -60 and supplies saved local -72 to distinct
helper `0x005F55E0`. FND-EXE-051 records the separate helper boundaries;
their effects and handler frame admission are not newly established.
The next function begins at `0x005FEC20`, outside this observation.

## Interpretation

The status-overlap path constructs and calls the field helper before its old-value decrement
gate, then ordinarily reaches the publication helper regardless of whether
a release was selected. The two saved handlers have different frame
adjustments and input-word sources. Q-EXE-009 retains producer/callback
effects, constructor aliases, allocation failure, native handler admission,
all initializers/writers and lifetime; no shell result or complete failure
contract follows from this bounded sequence.

## Alternatives

Testing the updated decrement value, releasing only for signed positive old
values, skipping construction on the fixed-prefix route, preserving the
constructor's allocated pointer as its normalized return, or treating the
two handler input words as identical is ruled out or unsupported locally.
An eight-byte request does not bound all allocation bytes; use the allocator
finding's header augmentation. A static handler pointer does not establish
native exception delivery or prove cleanup of every failing path.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-086's replaced argument
and tail target. Read fifty-five instructions at `005F6F10` and forty-five
at `005F6FC6`, keeping the cited ordinary regions distinct from handlers.
Recover only the explicitly stored targets `005F6FB0` and `005FEBD0`.
Read twelve at `005F6FB0`, forty-two at `005FEB60`, twenty at `005FEBD0`
and twelve at `005FEC07`; stop claims before the following functions.
Track local state stores, fresh output reads, full old-value comparison,
callee-consumed versus auxiliary slots and both unexpected normal-return
continuations. Use the cited allocator, copy, exchange-add, release, setup,
cleanup and forwarding findings for their separate contracts. Export only
function starts and analyzer body-byte counts. Recovery can reassign shared
tails to a new handler body without removing ordinary paths into those tails;
body-size changes are not reachability proof. Keep reports local and run no
original program.
