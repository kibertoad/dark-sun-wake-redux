---
id: FND-EXE-127
title: Larger full-width reader zero mode publishes mapping before reentry and keeps the full returned value
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689860..0x00689877
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689991..0x006899EE
tool: Ghidra 12.1.3 PUBLIC bounded larger full-width zero-mode reading
environment: null
---

## Observation

FND-EXE-126 grounds full-width target `0x00689860` in the physical
slot at `0x00758630`. Its entry saves four registers and reserves
twenty-eight bytes, making its second full input A lie at current ESP plus
`0x34`, corresponding to entry ESP plus eight. It retains A and forms
page index J by logical shift right twelve. Byte `0x01B7BB14` equal
to zero takes `0x00689991`; nonzero follows a separate branch whose
larger contract is not recorded here.

The zero-mode branch compares J unsigned with 271. J at most 271 loads
full K from `0x01B7B6D4` plus four times J; a larger J retains J
itself as K. The move retaining J preserves the comparison flags. This
bounds the local metadata index on the table-read route, not its backing
extent or the later publisher's admitted object. It calls `0x004180A0`
with J and selected K in the two outgoing slots. FND-EXE-100 records that
publisher's object selection, possible reset-list flush, indirect target
calls, ordered read/write mapping publications and fresh-count append.
The current caller does not test its returned EAX before continuing.

After an ordinary publisher return, this branch sets retained cleanup
selector EBX to full zero, writes original A in the first outgoing slot
and calls the shared full-width reader `0x004F6740` at
`0x006899B3`. FND-EXE-125 records that reader's direct, virtual and
boundary-byte alternatives. This is a fresh read through the mappings
available after publication, not a locally returned publisher value or
a direct read using the publisher's saved object.

After an ordinary reader return it tests the current selector EBX, saves
the full returned EAX in ESI without altering those test flags, and branches
to the epilogue when the selector is zero. Thus with the initialized
selector preserved, it skips the intervening cleanup path entirely. It
does not test any bit or width of the read result for success/failure.
The epilogue releases the reservation, copies saved full ESI to EAX,
restores four registers and returns. No AL/AX truncation occurs in this
local return path. Unlike an unconditional claim about external effects,
this selector observation retains the unread indirect callees' register,
stack and alias conditions.

If the post-read selector is nonzero, the shared continuation first reads
full reset-list count `0x01B5B6D0`; a nonzero count reads its indexed
last entry, compares that value with original A shifted right twelve,
and may branch to `0x00689B54`. Other routes compare the selector with
one, unsigned, and may branch to `0x00689BD0`. Those cleanup targets
and their admission from nonzero-mode paths remain open. The zero-mode
branch itself supplied no nonzero selector before reentry.

This branch has no local rollback of the publisher's stores or local
recursion-depth limit. Its read can select an object method again when a
direct entry is zero, including the physically grounded larger target.
The publisher's indirect effects and mapping producers must establish
whether that reentry terminates or selects a different target; the mere
presence of a publication call does not establish either. Exceptional
completion of either call need not reach the return epilogue.

## Interpretation

The larger full-width target has a concrete zero-mode publish-then-read
sequence, with a full result preserved through a separate selector gate.
Its relationship to the byte and word zero-mode routes in FND-EXE-100
is now grounded at its own physical target and call widths. Q-EXE-009
in FMT-EXE-006 remains open for nonzero-mode branches, concrete publisher
and reader effects, mapping/metadata storage and lifetime, cleanup targets,
input admission and remaining selected-callee branches. These observations
do not establish a complete mapping model or actual PATH behavior.

## Alternatives

- J equal to 271 reads metadata; J greater than 271 uses itself as K.
  The gate does not reject larger pages.
- The publisher's return is not the read result: an independent reader
  call follows with retained original A.
- The post-read gate concerns a selector initialized before the call,
  not a success flag obtained from EAX. Its zero case keeps full EAX.
- Publication alone does not prove reentry eliminated the fallback,
  cleanup always ran, or the read and return completed normally.

## How to reproduce

Verify FND-EXE-011's executable identity and the six physical controls in
FND-EXE-099, then FND-EXE-126's full-width target slot. Use the saved
Ghidra program with -noanalysis and ReportInstructionWindow.java at
`0x00689860` limit 65 for the entry/mode branch, restricting those
claims through `0x00689877`. Follow its zero edge with a window at
`0x00689991` limit 60, restricting observations through
`0x006899EE` and excluding the subsequent nonzero-mode error path.
Track the forty-four-byte frame displacement, unsigned 271 comparison,
flags across moves, selected K's last writer, both outgoing call slots,
zero selector publication after the publisher, flags across full result
retention and the epilogue's return width. Check J equal to 271 and 272
as local branch controls; they are not evidence that those inputs occur.
Use FND-EXE-100 and FND-EXE-125 for the already-recorded callees rather
than assuming publication makes the next read direct. No native or
emulated execution is part of this observation.
