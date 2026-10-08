---
id: FND-EXE-098
title: PATH mapped-table reset producers distinguish consecutive ranges from a retained index list
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00417E00..0x00417E3E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00417F30..0x00417F87
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0041A403..0x0041A445
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0041A4D1..0x0041A4F7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0041A621..0x0041A647
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00418800..0x00418860
tool: Ghidra 12.1.3 PUBLIC bounded instruction and reference reports
environment: null
---

## Observation

FND-EXE-094 records the PATH readers' direct mapping array at `0x0075B6D0`
and fallback-object array at `0x00F5B6D0`. Indexed-reference leads identify
actual stores in two helpers; their instruction bodies establish the access
direction independently of the analyzer's DATA reference label.

The helper at `0x00417E00` reads a full starting index from entry ESP plus
four and a full count from entry ESP plus eight. It tests the count before
entering its body. Zero count performs no table writes and returns the
starting index in EAX. For each admitted iteration, in order, it writes
full zero to direct array `0x0075B6D0` at index times four, full zero to
separate array `0x00B5B6D0` at the same index, fixed address `0x01B7BB28`
to fallback array `0x00F5B6D0`, then that address to separate array
`0x0135B6D0`. It increments the index and decrements the retained count,
using the decrement's zero flag for the next iteration. There are no calls
or local index-range checks. On normal completion EAX is the starting
index plus count modulo thirty-two bits. Effective indexed addresses also
use thirty-two-bit arithmetic; this is not a proof that every write is
inside an allocated table or cannot alias another destination.

The helper at `0x00417F30` instead initially reads a full count from
`0x01B5B6D0` and sets a cursor to `0x01B5B6D4`. With a nonzero retained
count it reads one full index through the cursor, advances the cursor by
four, and performs the same four ordered table writes. It then copies
the retained count to EAX and decrements that copy. A nonzero remainder
is written back to `0x01B5B6D0`, copied into the retained count, and the
next cursor word is consumed. The last remainder is zero; after leaving
the loop the helper writes full zero to `0x01B5B6D0` in all normal cases.
The consumed list words are not directly cleared. A zero initial count
skips list and table accesses but still writes the zero count. EAX is zero
on normal completion of a nonempty list; on the initially empty route
this body does not assign EAX. No local list extent, index bound or object
validity check is present. The retained count, not a fresh count-word
reload, controls each loop transition; aliases and validity of indexed
writes remain unproved.

One observed caller at `0x0041A43A` supplies the full saved word at its
current ESP plus twenty-four as the starting index and full one as the
count. The one is written immediately before the outgoing first slot
is filled and the call is made. A preceding call to `0x004F6FC0` has
returned before these argument stores. The reset return is not tested;
the continuation reloads a pointer at current ESP plus thirty-two,
stores current ESI through it and branches onward. This window proves
one reset iteration on this normal route, not the saved index's upstream
range or the preceding callee's effects.

Two caller continuations, at `0x0041A4F2` and `0x0041A642`, invoke the
list helper after normal return from `0x004F6FC0`. Their preceding calls
supply a selected object's field at offset 5160 first, full one second,
and its field at offset 4116 third. The subsequent reset helper takes
no stack arguments in its directly read body. Its EAX return is not
tested before the caller reads another object field. The reset therefore
uses whatever count/list state exists after the preceding callee returns;
these windows do not identify that callee as the list's producer.

The fixed fallback address does not establish an immutable first word.
A separate instruction body at `0x00418800` compares incoming full EDX
with 65535 and retains incoming full EAX for a later test. When EDX
matches and incoming EAX equals one, it writes `0x00758620` to
`0x01B7BB28`, then full 48 to `0x01B7BB2C`, then `0x00758CD8` to
`0x01B7BB18` and full 48 to `0x01B7BB1C`. The intervening second zero
branch sees the same nonzero AND-result flags: the MOV instructions do
not change them. When EDX matches and incoming EAX equals zero, a
later full-zero test and byte conjunction admit ordered writes of
`0x00757D70` to `0x01B7BB18` and `0x01B7BB28`. Its second zero branch
likewise retains the admitted conjunction's flags. Other input pairs
make none of these stores. The direct body returns without calls.
This grounds first-word mutation, not complete writer coverage or the
physical contents and selected methods of those three table addresses.

## Interpretation

A reset can remove a direct mapping while retaining a fallback-object
address for the PATH readers. Consecutive-range resetting and list-driven
resetting have different input and return contracts. The fixed address
can later expose a different first word; concrete virtual target admission
needs its own producer and physical-table reading. These bounded producers
do not establish PATH list construction, complete mapping lifetime or the
shell's eventual launch outcome (Q-EXE-009 in FMT-EXE-006).

## Alternatives

- A DATA-labeled reference is only an immediate-address lead. The actual
  indexed instructions here write the arrays; FND-EXE-094 independently
  supplies read controls at `0x004F6592` and `0x004F734E`.
- The range helper does not necessarily reset the whole mapping. Its
  observed caller supplies one iteration, and the index source is still open.
- Clearing direct entries does not make the reader reject an address locally:
  the reset also installs the fallback address used by the reader's null arm.
- The list helper does not directly erase the stored indices, and its empty
  route does not normalize EAX to zero. Neither caller window tests EAX.
- A fixed object address is not a fixed virtual target. The separately
  observed first-word stores exclude that inference, without proving which
  producer state reaches a particular PATH call.

## How to reproduce

Use the shipped interpreter identity and PE mapping in FND-EXE-011: length
3802624 and XXH3-128 `09861838aa3018346f9f15c9a4f5925c`. Read the saved
Ghidra program statically with -noanalysis; do not start the interpreter.

ReportReferences.java queries `0x0075B6D0`, `0x00F5B6D0` and
`0x0058CD50`, followed by `0x00417E00`, `0x00417F30`, `0x01B5B6D0`
and `0x01B7BB28`, use a cap of 200 analyzer references per target.
They select leads, not exhaustive writer/caller coverage. The known reader
instructions in FND-EXE-094 and decoded reset stores are read/write controls;
no negative search or all-reference-kind claim is made.

ReportInstructionWindow.java queries are entry `0x00417E00`, limit 75;
`0x00417F30`, limit 35; `0x0041A4A0`, limit 35; `0x0041A3D3`, limit 45;
`0x0041A612`, limit 20; and `0x00418800`, limit 32. Restrict each claim to
its stated location above rather than following a window into another body.
Initial requests `0x0041A3D0`, limit 55, and `0x0041A610`, limit 22,
reported starts inside instructions at `0x0041A3CD` and `0x0041A60D`.
The reported next starts `0x0041A3D3` and `0x0041A612` supplied the
corrected continuations; the rejected windows are not absence evidence.

Keep the reference, body, caller and continuation reports under
GAME_DIR/analysis/exe-batches. No listing, executable bytes or database belongs
in Git. This finding records bounded direct paths only; other table/list
writers, predecessor admission, aliases and concrete fallback methods remain
Q-EXE-009.