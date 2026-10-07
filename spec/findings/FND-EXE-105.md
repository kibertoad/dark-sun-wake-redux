---
id: FND-EXE-105
title: Callback insertion consumes a free node while removers unlink every matching target or target-argument pair
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F2490..0x004F25F8
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F2600..0x004F2670
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F2680..0x004F26DA
tool: Ghidra 12.1.3 PUBLIC bounded callback-record producer and removal reading
environment: null
---

## Observation

FND-EXE-104 records active head `0x01D291E4`, free head `0x01D291E0`
and a callback record's single-precision field zero, full argument at four,
full target at eight and full link at twelve. A producer at `0x004F2490`
pushes two saved registers and reserves eight stack bytes. It retains free
head N, loads its second argument as a single-precision value, then tests N.
With no free node it pops that floating value and returns without inserting,
allocating, publishing a failure word or normalizing EAX. Its first and third
full arguments are at current ESP plus twenty and twenty-eight; the float
argument is at plus twenty-four.

With N nonnull it checks byte `0x01D292D0`. Nonzero adds the current
single-precision value at `0x01D292C0` to the incoming floating value.
Zero instead uses current full `0x006F00A0`, `0x006F00A4` and
`0x0075B0E8` in floating division/addition after computing their wrapped
integer difference. Numeric/exceptional contracts remain unresolved. The
result is stored as single precision at N. The producer writes its first full
argument to N plus eight, reads N's previous link, freshly reads active head,
writes its third full argument to N plus four, then publishes the old free
link as free head. These stores occur before the insertion search.

If active head is null it writes zero to N plus twelve and publishes N as
active head. Otherwise it compares the current head's float with the retained
new floating value, transferring x87 status to integer flags. Below-or-equal
flags select traversal; other flags insert N before the head by publishing N
as active head before writing the old head to N plus twelve. In traversal,
it keeps a predecessor and reads its link. A null link appends N by writing
N's link zero, then predecessor's link N. A nonnull next node's comparison
selects insertion before that node only on above flags; otherwise traversal
continues. Middle insertion writes N's link to that next node before changing
the predecessor's link. Equality therefore continues through existing equal
values under normal finite comparisons. Unordered status also follows the
below-or-equal/non-above routes; no finite-input or x87 environment gate is
proved. No local cycle/iteration bound admits the search as always terminating.

After insertion it uses the selected or freshly reloaded active head and fresh
full budget globals in another floating calculation. It saves the current x87
control word, ORs a local copy with mask `0x0C00`, uses that copy for a
signed full integer conversion, then restores the saved word. The converted
integer is compared signed with retained current `0x0075B0E8`. An integer
at least that retained value leaves the budget words untouched. A smaller
integer adds the retained `0x006F00A4` and `0x0075B0E8` modulo
thirty-two bits, publishes that sum to `0x006F00A4`, then publishes full
zero to `0x0075B0E8`. The converted value itself is not published there.
No success/count convention is assigned to EAX before return. The function
makes no direct calls; original float precision, exceptional paths, aliases
and valid storage remain open.

The remover `0x004F2600` retains its first full argument as target T and
second as argument V after two register pushes. It walks active nodes with
a null initial predecessor. A node matches only when its full offset-eight
word equals T and its full offset-four word equals V. `0x004F2680`
instead retains one full target input after one register push and matches
only offset eight. Neither calls a matching callback or tests its return.

Both removers continue after a match. For an interior node they read its
next link and publish it through the predecessor at offset twelve, freshly
read free head, write that free head to the removed node's link, and publish
the removed node as free head. They then freshly read the predecessor's
new link as the next active node, retaining that same predecessor. For a
head match they publish the matched node's next link as active head before
free insertion, then freshly reload active head with predecessor still null.
A nonmatch advances predecessor and reads the node's link. Exhaustion
returns without a normalized removed-count or success value. Thus adjacent
matches and repeated head matches remain candidates for removal; this is not
first-match-only behavior. No list validity, cycle check or local iteration
bound proves termination, and fields zero, four and eight are not directly
cleared when a node enters the free list.

## Interpretation

The admission callback records have a concrete reuse/insertion producer and
two distinct removal predicates. Target and argument fields precede active
publication, while free-list detachment precedes the insertion search. Removal
unlinks active storage before free insertion and continues through subsequent
matches. Caller-supplied targets, pool initialization, list lifetime, aliases,
floating numeric contracts and native scheduling remain Q-EXE-009 in
FMT-EXE-006; this finding does not establish actual PATH behavior.

## Alternatives

- No free node does not trigger a local allocation or establish an error
  return. Loading and popping the float precedes ordinary return.
- The two removers do not have the same predicate, and neither stops merely
  because it removed one match. A matched head keeps the predecessor null.
- Free insertion does not erase the callback and argument fields. Reuse
  writes them explicitly before active insertion.
- Budget invalidation publishes the retained sum and zero, not the converted
  integer. Its comparison is signed and effects follow active insertion.
- Floating equality, unordered status and integer wrap cannot be replaced by
  an ideal ascending real-number list without further contracts.

## How to reproduce

Recheck FND-EXE-011's identity and FND-EXE-099's physical target controls.
Use the saved Ghidra program with -noanalysis. ReportReferences.java queries
`0x01D291E4` and `0x01D291E0`, cap 200 references per target, supply
leads only. FND-EXE-104's head read at `0x004F2701`, removal publication
at `0x004F275C` and free publication at `0x004F2776` are independent
controls; no negative search or exhaustive writer claim is made.

Read ReportInstructionWindow.java at `0x004F2490` limit 115,
`0x004F2600` limit 75 and `0x004F2680` limit 35, restricting observations
to the ranges above. Track saved registers/local reservation for argument
positions, free-head consumption before insertion, floating status branches,
head versus interior write order and the post-match loop targets. Preserve
numeric/environment and alias uncertainties. Keep all reports and identity
controls in GAME_DIR/analysis/exe-batches. No original listings or bytes in
Git; run neither the interpreter nor the game.