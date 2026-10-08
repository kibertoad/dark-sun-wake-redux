---
id: FND-EXE-077
title: Controlled recovery classifies the three additional guard literals as full-word reads with conditional frame admission
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FC4F0..0x005FC56A
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FC5F5..0x005FC617
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FC650..0x005FC6AD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FC702..0x005FC723
tool: Ghidra 12.1.3 PUBLIC controlled recovery and bounded context reports, with Capstone 5.0.7 prefix cross-check
environment: null
---

## Observation

FND-EXE-076 records three physically mapped guard-address literals with no
containing decoded instruction in the then-current analyzer. This finding
adds a controlled reading of those regions; it does not correct or replace
the earlier observation about that analysis state.

### Entry and stored-target provenance

A bounded physical prefix inspection places a conventional frame prologue
at `0x005FC4F0` in the PE's executable section. An independent 32-bit decode
of 96 physically backed bytes from that boundary agrees with the subsequent
Ghidra instructions. The prefix constructs a nested record using the same
already-read setup and callback entries as FND-EXE-167/055. It also directly
calls the independently located guard-reading helper `0x005FB8B0` before
the first literal. These connected prefix instructions and known call
boundaries ground the local decoding; the literal positions themselves
were not chosen as instruction entries.

The entry establishes its frame and saves registers. It reads its second
original full-word stack argument into a saved local and stores the stack
pointer before outgoing setup space. Its record begins at frame minus 140:

| Record field offset | Prefix writer |
|---|---|
| 24 | FND-EXE-165's callback entry `0x005F50A0` |
| 28 | unread metadata `0x006EF0F4` |
| 32 | frame-minus-24 address |
| 36 | stored target `0x005FC650` |
| 40 | stack pointer before outgoing setup space |

It passes the record address to FND-EXE-167's setup, removes sixteen
outgoing bytes after normal return and writes all ones to record offset
four. It then calls `0x005FB8B0`; the following full-word guard load replaces
that callee's return in the return register before the guard test. The
helper's other effects are not read here. The record's stored target, rather
than proximity alone, supplies the second recovery entry `0x005FC650`.

The entry's caller admission remains unresolved. A scan of the current
analyzer's resolved call/jump targets over `0x005FC400..0x005FC740` supplies
only internal jump leads. Independent physical searches find no exact
four-byte `0x005FC4F0` value and no physically mapped five-byte relative
call/jump candidate to that boundary. These are restricted navigation
models, not proof that the entry is unused: computed, encoded, external,
indirect and undecoded incoming paths remain open. Known direct call
`0x005FCD5B` to FND-EXE-075's initializer supplies the relative-flow control,
and FND-EXE-025's stored callback supplies the exact-address control.
No absence-of-all-callers claim is made.

### The three reads

After recovering only the grounded prefix entry and its explicit stored
target, each literal lies inside a full-word absolute load:

| Prior literal position | Actual instruction start | Local destination |
|---|---|---|
| `0x005FC545` | `0x005FC544` | return register |
| `0x005FC5F7` | `0x005FC5F5` | second working register |
| `0x005FC704` | `0x005FC702` | saved working register |

All three load the word at `0x0242C910`; none directly stores to that word
or materializes its address as a pointer. The distinction follows the actual
instruction widths and operand directions, not only their new READ labels.

The first read is followed by a direct full-word store of `0x02427E30`
to frame minus 40. The entry then tests the complete loaded guard. Nonzero
jumps to `0x005FC6B3`; zero continues at `0x005FC558`. These continuations
are outside this prefix reading. The intervening local store does not
replace the guard value or narrow its test to one byte.

The word at frame minus 40 is loaded before the second guard test, including
its zero path. The second read tests its complete value. Zero jumps to `0x005FC618` and
skips the following call. Nonzero reserves twelve outgoing bytes, writes
record state one, pushes the word read from frame minus 40 and calls
FND-EXE-074's `0x006019F0`, then removes sixteen outgoing bytes after normal
return. The prefix's earlier frame-minus-40 store is a producer candidate
for this argument; the intervening paths, possible writes/aliases and
helper effects must be read before claiming it is invariably the same
value. The later continuation and result consumption are not covered here.

After the third guard load, this path saves another local word, reads frame minus 40 into
the return register, and tests the complete guard. Nonzero jumps to
`0x005FC724`. Zero reserves twelve outgoing bytes, reloads and pushes the
saved local word and jumps to `0x005FC6A4`, where the shared prefix writes
all ones to record state. Neither branch of this local test directly writes
the shared guard. Effects after these targets remain conditional.

### Stored-entry state prefix and frame limits

The explicitly stored target begins by adding 24 to its incoming frame
register. It reads adjusted-frame offsets minus 136, minus 132 and minus
128. The first is compared at full-word width against these exact states:

| First saved word | Selected target |
|---|---|
| 1 | `0x005FC6C8` |
| 2, 3, 4 or 7 | third guard read at `0x005FC702` |
| 5 | `0x005FC6DE` |
| 6 | `0x005FC74F` |
| Any other word | increment the third saved word, then branch as below |

For the final row it increments the third saved word at 32-bit width and
tests the wrapped result for exactly zero. Zero selects `0x005FC73C`;
nonzero falls through to reserve twelve outgoing bytes and push the second
saved word, then reaches the all-ones state write at `0x005FC6A4`.
This is a local state-prefix table; the targets' complete effects, forward
calls and final outcomes are not established by it.

The frame-plus-24 adjustment fits the caller prefix's stored frame-minus-24
address, but actual dispatcher admission and incoming frame identity still
need their own evidence. Analyzer function ownership is not that evidence.
An ordinary prefix branch can enter code physically beyond the stored entry,
so neither analyzer body nor stored-handler label proves how every incoming
path formed the frame. No offset equality alone equates caller locals with
handler locals on an untraced incoming path.

A refreshed exact-address reference query now lists seventeen guard READ
rows, including the three independently decoded loads. The bitmap control
still lists its known reads and two writes. This reconciles the earlier
physical candidates with current reference metadata, without proving an
all-writers search. Computed/aliased, partial/relative, generated, loader and
external writes remain outside the literal/reference models.
The permitted function inventory gains only the recovered entry starts
and their analyzer body sizes; sizes are not contiguous spans or a complete
reading of these functions. No original program or API was executed.

## Interpretation

The three missing-ownership candidates are now locally classified as reads
with explicit full-width guard branches. The nested record provides a stored
entry and state-prefix route to the third read, while broader caller and
frame admission remain conditional. Q-EXE-009 retains shared-guard producers,
startup/alias/lifetime effects, the unreviewed continuations and concrete
callbacks. This evidence does not establish that the guard has no writer,
that any path is inactive, or that the associated storage is synchronized.

## Alternatives

Starting instruction decoding at the literal bytes, classifying a READ
from the analyzer label alone, testing only the guard's low byte, always
calling the second wrapper, or inferring runtime frame identity from an
analyzer function boundary is ruled out or unsupported. The recovered
loads settle the three literal access kinds; they do not exclude other
access models or settle all incoming paths.

## How to reproduce

Verify FND-EXE-011's source identity and preferred PE base. Inspect exactly
80 bytes at `0x005FC500` and 48 at `0x005FC4E0` as navigation data. Independently
decode 96 physically backed bytes from `0x005FC4F0` in x86 32-bit mode,
checking the prologue and calls to the known setup/guard helper boundaries;
do not treat a decoded candidate as actual caller admission. Recover only
`005FC4F0`, then its prefix's explicit stored target `005FC650`, in the local
Ghidra program. Request instruction context at the three literal positions
`005FC545`, `005FC5F7`, `005FC704`; each reporter context is at most eight
instructions on each side and the header identifies the containing start.
Read 24 instructions from `005FC650`, restricting the state-table claim to
the cited prefix and excluding subsequent targets' effects.

For incoming navigation use ReportCallsToRange with inclusive endpoints
`005FC400`, `005FC740` and mode `all` (50-site cap). Repeat whole-file exact
four-byte scans for `0x005FC4F0`, `0x005FC650` and control `0x005FCD50`, and
physically mapped five-byte relative-call/jump candidate scans for
`0x005FC4F0` and control `0x00601970`, with a hard 128-match cap per target.
Relative candidates need decoded boundary and reachability validation before
being called actual flows. These navigation scans finish below their caps.
Refresh ReportReferences for `0242C910` and control `02427E40` (200-reference
cap), and export the allowed start/size inventory only, retaining its address
format and reviewing every changed row. Track record field writers, full
widths, flag use and the frame adjustment independently of analyzer ownership.
Keep rich reports local and execute no original interpreter or game.
