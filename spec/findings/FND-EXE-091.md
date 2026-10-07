---
id: FND-EXE-091
title: Handler scan retains a pre-reader modifier and a physically selected zero-return fallback method
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F55E0..0x005F575F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4FF0..0x005F503F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5900..0x005F5906
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00353018..0x0035301B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00356AE0..0x00356AE3
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading and hash-guarded physical PE data reads
environment: null
---

## Observation

FND-EXE-090 records a handler helper that saves four words from its freshly
reloaded first input minus 48 before the stored-target call. Prefix offset
32 becomes local -128, offset 24 becomes -132, offset 12 becomes -136 and
offset 36 becomes -64. The subsequent handler reading uses local -128 as
the metadata reader's input cursor, zero as its context and address of
local -72 as its output. These correspondences are conditional on the
handler's adjusted frame being that earlier frame and on intervening writes;
the stored handler alone does not establish either invariant.

The pre-reader local -64 is exactly output offset eight from local -72.
FND-EXE-060's reader has no direct store at that offset. Its direct output
stores therefore retain this caller's previously saved prefix-offset-36
word as the modifier consumed by FND-EXE-064's indexed typed reader,
subject to unread callee writes and aliases. It is not a proved zero field
or a newly supplied metadata output. Normal reader completion stores its
relative target at output offset twelve, local -60, and independently
returns its continued cursor. The handler does not save that cursor for
either later scan call. The scan in FND-EXE-064 starts at output offset
twelve minus the explicit displacement minus one, with both subtractions
wrapping at 32-bit width. Here each call's displacement is saved local
-132, not the metadata reader's returned cursor or its byte count.

For the first scan the candidate input is saved context head plus 80 and
the second object is the word freshly read through that head. For the
fallback scan the candidate is zero and the second object is fixed
`0x00755E18`. The scan copies its candidate input into a local and passes
that local's address to comparator `0x005F4FF0` for each nonzero decoded
index. Indirect writes or aliases can change the scan local despite an unsuccessful
comparison, which has no direct copy-back. The fallback zero is the initial
candidate, not proof of its value on every iteration.
Decoded zero ends the scan without calling the comparator, as separately
recorded in FND-EXE-064. The first object's identity still comes from the
indexed typed read and is not established by the fixed second object.

The fixed second object's shipped first word at virtual `0x00755E18`,
file offset `0x00353018`, is full pointer `0x007598D8`. The dispatch word
at its offset eight, virtual `0x007598E0`, file offset `0x00356AE0`, is
full target `0x005F5900`. All four bytes of each word map contiguously to
physical PE section data. This is a shipped target, not proof that every
native caller sees an unchanged object and table.

The selected target's seven-byte body clears full EAX and returns without
reading an input argument, dereferencing the supplied object, writing object or shared memory,
making another call or removing argument bytes. Comparator `0x005F4FF0`
passes the second object in one outgoing word and tests AL on return.
With the physically identified slot unchanged and a normal call to this
body, AL is zero and the comparator bypasses its conditional read through
the current candidate. In particular, on the first fallback comparison this
gate does not dereference the initial zero candidate. It does not make all
later accesses safe or prove successful matching.

The comparator then freshly reads the first object's dispatch word at
offset sixteen and calls it with first object, saved second object, address
of candidate local and full word one in four outgoing slots. That target
and its effects remain conditional. The same candidate-local address is
used after the bypass, so zero is not silently replaced by the second
object. A nonzero returned AL copies the current candidate back to the
scan's candidate local and gives full-word comparator return one; zero
does not directly perform that copy and returns zero. Indirect writes or
aliases can still affect the local or its caller. FND-EXE-064 records this
per-call behavior; the concrete zero-return target narrows only its first
dispatch gate for the fixed fallback object.

## Interpretation

This caller supplies a preexisting modifier field and an explicit scan
displacement, while the fallback object's shipped dispatch skips the
conditional candidate dereference. Q-EXE-009 retains frame identity,
prefix-field producers, stream extent, all object/table writers, concrete
first-object targets and indirect effects. Neither a zero initial candidate
nor a zero-return method proves that the entire fallback scan is empty,
bounded, safe or successful. No original execution or completed handler
admission is claimed.

## Alternatives

Using the metadata reader's returned cursor as the scan start, assuming
output offset eight was initialized by that reader, unconditionally
dereferencing the initial fallback candidate, or replacing it with the
second object's address on a zero method result is ruled out on these
conditional direct paths. Treating the first scan's object as the same
fixed fallback object is unsupported. A direct zero-return target does
not settle the comparator's separate first-object virtual target.

## How to reproduce

Verify FND-EXE-011's length 3802624 and XXH3-128
`09861838aa3018346f9f15c9a4f5925c`. Use the PE image base and section raw
extents to map virtual `00755E18` and the first-word table's offset eight;
require all four bytes of each read to map to contiguous file bytes. Read
only that object's first word and table offsets zero, four, eight and twelve
as a bounded data query; only offset eight is used for the target claim.
Other stored targets are not admitted or characterized here. Read fifteen
instructions at `005F5900`, restricting claims through `005F5906` and
excluding the next method. Read forty-eight at `005F4FF0`, restricting to
its cited body. Read forty at `005F5040` as the scan-prefix control, using
FND-EXE-064 for the complete direct return branches. Reuse FND-EXE-090's
fifty-instruction `005F55E0` and sixty-five-instruction `005F5695` queries,
and FND-EXE-060 for output stores. Track relative field locations, full
versus low-byte results, fresh pointer loads and every remaining call/alias
assumption. Keep physical-read results and reports in local original-content
storage; do not execute the interpreter or a virtual target.
