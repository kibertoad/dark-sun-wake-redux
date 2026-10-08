---
id: FND-EXE-205
title: Neighboring scalar initializer publishes allocation returns before unchecked indirect writes and reuses outgoing argument slots
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005B8B10..0x005B8BB4
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601CE0..0x00601CE6
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-202's neighbor search exposes the full-dword store to
`0x0242C970` at `0x005B8B70`. The containing initializer at
`0x005B8B10` has two consecutive construction sequences. The first
publishes its initial allocation result to `0x0242CB70`; the second
publishes its initial result to `0x0242C970`. These fixed four-byte
cells are disjoint from the gate at `0x0242C910..0x0242C914`.
The second cell is the base of FND-EXE-202's indexed table, and the
first is immediately after that finding's bounded table interval.
This is address identity, not ownership of either table or a proof of
its complete schema.

Each sequence calls FND-EXE-203's direct allocation wrapper first
with full request 516 and later with 3584. Immediately after the
first normal return it publishes the returned EAX word without a
null check. It prepares three outgoing words, in callee order: that
returned pointer, zero and 516, and calls `0x00601CE0`. The thunk
jumps through `0x024319E4`, physically identified by FND-EXE-025 as
msvcrt.dll memset. Imported effects and the allocator's storage
contract are not established by these identities.

After that call it reloads the published pointer into EBX. Without
first removing the three outgoing words, it overwrites the current
slot at ESP with 3584 and calls the allocation wrapper again. On
normal return it removes twelve bytes and stores the full returned
EAX word through EBX. It then rereads the published cell and its
first word, prepares that freshly fetched pointer, zero and 3584,
and calls the same memset thunk. It does not test either allocation
return or zero-fill return locally. The first pointer publication
therefore precedes the indirect first-word store and second zero-fill
request; a later failure does not locally undo the published cell.

After the first sequence's final memset, the code again overwrites
the still-present outgoing slot at ESP, this time with 516, and uses
it for the second sequence's first allocation. It then repeats the
publication and construction sequence for `0x0242C970`. After the
last memset returns normally, it clears EAX to zero, reloads its
saved EBX from the conventional frame and uses LEAVE and a near
return. It does not first remove that final call's outgoing words
with an ADD. No branch, loop, original argument read, local rollback,
release of an earlier cell value or local allocation-failure handler
appears in the cited initializer.

Let F be the initializer's saved-frame address. Its saved EBX lies
at F -4 and the initial sixteen-byte reservation leaves ESP F -20.
The first allocation argument push leaves F -24. Under ordinary
caller-cleanup returns, the first ADD of twelve puts ESP at F -12.
Every three-word memset preparation then puts it at F -24. Each
subsequent allocation reuses the top word there rather than adding
another request slot, and its following ADD of twelve restores
F -12. The final memset leaves those words at F -24 until LEAVE
resets ESP to F and restores the saved frame. This trace identifies
the immediate request stores as the last writers of the reused
allocation slots; they are not the earlier memset pointer arguments.

Pointer reads and indirect first-word stores use DS; ordinary frame
slots use SS. The fixed cells are separate from the gate, but the
indirect stores and imported zero-fill destinations require allocator
origin, valid extent, EBX preservation and alias admission. Normal
returns, stable frame storage and the external calling convention
are conditions on the stack trace. No native gate write, allocator
unit or actual failure outcome is inferred here.

## Interpretation

This classifies the remaining nearby direct scalar publication and
separates it from its indirect writes and external destinations.
Q-EXE-009 retains imported effects, allocation storage/lifetime,
callee preservation, aliases, alternate entries and the gate's actual
initialization. FND-EXE-203 supplies the local allocation forwarding
only; SRC-WIN32-X86-ABI remains an external contract. No complete
reading or complete writer set follows.

## Alternatives

Treating the global store as the only destination misses the later
indirect first-word stores and prepared zero-fill pointers. Removing
memset arguments before following the overwritten ESP slot would
invent a different allocation argument. Treating the final zero
return as proof of allocation success ignores the absent local tests.
The numeric requests alone do not supply allocation units or extents.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved project, read-only with analysis disabled, run
ReportInstructionWindow at `0x005B8B10`, count 55, and at
`0x00601CE0`, count one, restricting claims to the cited spans.
Check ReportCitationBoundaries for `005B8B10..005B8BB4:return`
and `00601CE0..00601CE6`. Use FND-EXE-025's independent physical
import identification and FND-EXE-203's direct allocator reading.

Trace each publication, fresh cell and pointer reread, indirect store,
outgoing push, slot overwrite and cleanup in execution order. Track
ESP from F before naming reused slots or the final restoration.
Separate fixed store widths from indirectly addressed storage and
imported effects. Endpoints do not establish caller completeness or
storage admission. Keep rich reports local and execute no original
program.
