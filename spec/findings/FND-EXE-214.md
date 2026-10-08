---
id: FND-EXE-214
title: Matching helpers combine marker strides, low-byte virtual results and zero-terminated index scans
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4F80..0x005F4FE9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4FF0..0x005F5040
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5040..0x005F509F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F53A7..0x005F551A
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

The first helper receives a record address in the accumulator register and
an index word in the data register. It saves the record address separately,
reads the byte at record offset 20 into the accumulator's low byte, and
chooses a stride. Marker 255 selects zero. Otherwise it masks the marker to
its low three bits and applies these branches:

| Masked marker | Stride |
|---|---|
| 0 | 4 |
| 2 | 2 |
| 3 | 4 |
| 4 | 8 |
| 1, 5, 6, 7 | Abort import from FND-EXE-041 |

The mask also discards the retained upper address bits. The comparison
that separates values above two is signed, but the preceding mask admits
only zero through seven, so it does not introduce a negative-index class.
The two-byte stride assignments follow a full zero initialization; the
four/eight assignments likewise have established upper bytes.

It multiplies the incoming index by the stride, retaining the low 32-bit
product, and subtracts that product from the full word at record offset
twelve, again at 32-bit width. It prepares this result as the typed reader's
input cursor, the original marker zero-extended as its marker, and the full
word at record offset eight as its modifier. A local word address is the
first outgoing stack argument. It calls FND-EXE-062's typed reader, then
loads that local word into the return register, restores the frame and
returns. Thus it returns the decoded output, discarding the reader's cursor
return. FND-EXE-062 and FND-EXE-063 bound that output's direct last writers
on normally returning branches.

Stride selection alone does not admit a marker. Marker 255 reaches the
typed reader's out-of-range low-nibble abort even though its stride is zero;
marker eight selects stride four but reaches that reader's abort branch.
There is no local index, multiplication, target or source-length check.
FND-EXE-060 has no direct initializer for record offset eight: this new
consumer does not establish that word's last writer or validity.

The second helper receives a first object address, second object address
and caller-cell address in the accumulator, data and count registers. It
saves all three and copies the full word at the caller cell into its own
local word before making any call. It reads the second object's first word
as a dispatch-table address and calls the full pointer at table offset eight,
preparing the second object in one outgoing stack slot. It tests only the
returned low byte. Nonzero replaces its local word with the full word read
through that local's current value; zero leaves the local unchanged. No
null guard precedes either dispatch or the conditional indirect read.

It then freshly reads the first object's first word and calls the full
pointer at that table's offset sixteen. It prepares four stack slots, in
callee-facing order: first object, saved second object, address of its own
local word, and one. The two calls' concrete targets, slot consumption and
exceptional effects remain unresolved; prepared stack values are not proof
of what those targets read. The first call's sixteen-byte cleanup includes
its outgoing reserved space, and the second call has four full-word slots
followed by sixteen-byte cleanup. Frame restoration discards the remaining
local reservation.

Again only the returned low byte is tested. Nonzero copies the current local
word to the saved caller cell and returns a full-word one; zero performs no
direct caller-cell store and returns a full-word zero. The local passed by
address can be changed by the second target. The copied word need not be the
original caller-cell value or merely its first dereference. A failed call
is not rollback: indirect callee writes and aliases remain conditional.
A return word with zero low byte is false even when its upper bytes are
nonzero; a nonzero low byte is true regardless of the other bytes.

The third helper saves its register inputs as record address, second object
and candidate word. It reads its first original stack argument as a displacement.
Its initial input cursor is record offset twelve minus that displacement
minus one, with both subtractions wrapping at 32-bit width. At least once,
it calls FND-EXE-059's byte reader into a separate local word and saves the
returned cursor for the next iteration. A decoded full-word zero ends the
scan immediately with full-word return zero, without either other helper.

For a nonzero decoded word, it calls the first helper with the saved record
and that word as index. It then calls the second helper with the first
helper's decoded return as first object, the saved second object, and the
address of its own candidate local. A true low-byte return ends the scan
with full-word one. False repeats at the byte reader's saved advanced cursor.
This is a decoded-zero terminator, not a test for one literal zero input byte,
not a fixed count, and not an unsigned range guard on nonzero indices. No
local byte limit, maximum iteration count, index admission or pointer check
is present. The success arm does not copy the candidate local back to an
incoming caller-cell address: the incoming third value was copied into this
helper's own local.

The studied callback prepares these helpers directly. After two chained
FND-EXE-063 reads, the positive signed first output is prepared as the first
helper's index with the metadata local as record. A nonzero decoded return
can become the second helper's first object; the callback's saved object
word becomes its second object, and a separate callback local address is
prepared as caller cell. The negative signed output branch instead prepares
that output in the third helper's first stack slot, with the metadata record,
saved object and current callback candidate in registers. Its low-byte return
selects different continuations. The zero first-output path, object-zero
branches and later callback classification remain separate from these local
helper contracts; this reading does not claim the whole callback algorithm.

## Interpretation

The three matching helpers now have bounded local input, arithmetic, dispatch,
output and return contracts. Their chain distinguishes a typed decoded object
from a cursor, a low-byte virtual result from a full-word status, and an
unbounded decoded-index scan from a counted loop. Q-EXE-009 retains concrete
virtual targets, record-offset-eight initialization, caller candidate/object
provenance, stream bounds, aliases, remaining classification and dispatcher
admission. No validated matching schema, record lifecycle or replacement
parser is established or implemented.

## Alternatives

This replaces FND-EXE-064. Its callback location ended inside the five-byte
jump at `0x005F5515`; the correct exclusive end is `0x005F551A`.
The observations are preserved with the complete final instruction. No
complete-reading declaration existed, and no status promotion follows.

Returning the typed reader's advanced cursor, choosing an eight-byte read
merely because the stride is eight, treating marker 255 as a successful null
result, testing whole virtual return words, publishing the caller cell on both
virtual-result arms, or treating the index scan as count-bounded are ruled out.
Calling the prepared slots consumed parameters, naming the indirect targets
from table offsets, or assuming metadata offset eight is zero would exceed
the reading. An apparently unchanged caller cell does not prove absence of
indirect callee writes or aliases.

## How to reproduce

Use FND-EXE-011's length and XXH3-verified PE and image base. Read one hundred
and ten instructions from `0x005F4F80`, twelve from `0x005F5084`, seven from
`0x005F5095`, and eighty-five from `0x005F53A7`; restrict claims to the cited
ranges and treat the gaps between bodies as gaps. Track the marker's byte
writer and masks, stride upper bytes, index-product wrap, record offsets,
typed output last writer and discarded cursor, two complete indirect pointer
loads, outgoing slots and cleanup, low-byte truth tests, conditional local
indirection/publication, decoded-zero termination and both scan exits.
Use FND-EXE-196/060 for the metadata region and preceding callback state,
FND-EXE-059/062/063 for reader contracts. Keep concrete targets, input bounds,
aliases and exceptional effects conditional. Keep rich reports local and
execute no interpreter or game.


Verify the old and corrected callback endpoints with ReportCitationBoundaries
queries `005F53A7..005F5519` and `005F53A7..005F551A`. With engine
13.6.0, run ReportInstructionWindow at `0x005F5515`, count one; its
span ends at `0x005F551A`. The other three location endpoints are aligned.
Endpoint classification does not establish interior completeness or runtime admission.
