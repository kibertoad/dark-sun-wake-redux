---
id: FND-COMBAT-087
title: APFM mode selection can replace the setter's looked-up record or leave the prior record pointer unchanged
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0635..3D72:06D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0755..3F96:07B3
tool: tools/research/exec-census/resident_listing.py at 69f91492; Capstone 5.0.9
environment: null
---

## Observation

FND-CONFIG-189's two pointer services can dispatch record fields +5A
and +5E through DS:A15B, under different mode gates. FND-PARTY-044
and FND-EXE-588 locate two literal pointer-store sites. This reads their
containing routines from their entries, without treating that earlier
literal search as a complete storage-identity proof.

Resident 3F96:0755 saves its first far argument locally and looks up the
APFM tag through 39D1:049C, supplying the word at BP+0A as identifier.
A null returned far pointer returns AX FFFF without its later calls.
A nonnull pointer is retained at SS:BP-8. The routine calls 3D72:0B5F,
then stores the retained pointer into DS:A15B and word BP+0E into
DS:A141. It then calls 3D72:0635 and 3D72:0942 in that order, returning
AX zero if both return normally. It does not revalidate the retained
record after 0B5F or test the later callees' results.

Resident 3D72:0635 has a stack-limit guard, then clears DS:A13F and
snapshots far DS:A125 into its frame. It tests current DS:A129 for the
APFM tag. A mismatch stores zero at DS:A13D and returns, without an own
store to DS:A15B. A matching tag permits four signed word comparisons:
record+20 must not exceed DS:A109, record+24 must not be below A109,
record+22 must not exceed DS:A10B and record+26 must not be below A10B.
Failure of any comparison takes that same mode-zero tail. There is no
own null-pointer test before these record accesses.

Passing the comparisons copies record word +66 to DS:A13F. Nonzero
DS:A141 together with equality between current DS:A15B and the locally
saved A125 selects mode three at DS:A13D. The routine then copies
current DS:A125 into DS:A15B and returns. It does not test callback fields
on that mode-three path.

Otherwise, it tests both double words +5A and +5E of the locally saved
A125 record. Both must be nonzero to store mode two, copy current A125
to A15B and return. A zero field instead stores mode zero and returns
without clearing or replacing A15B. The comparisons and copies occur
after the stack-limit guard; caller state and guard preservation remain
conditions, rather than inferred from loaded values.

Consequently, the nonnull result of 0755's APFM lookup is not necessarily
the record left in A15B after 0635. When the selector's current record
differs, passes its gates and has both callback fields nonzero, the mode-two
tail replaces the setter's saved lookup result with current A125. A
mode-zero tail instead leaves the pointer that was present on entry to
the selector. These are conditional local state transitions, not observed
native mismatches or callback executions.

## Interpretation

A callback target cannot be bound solely from the APFM identifier passed
to 0755. Its preliminary 0B5F call, the selector's independently read
record/tag/coordinates and the later service's mode and target rereads
all matter. A retained nonnull A15B also does not imply mode two or an
admitted +5A/+5E callback.

## Alternatives

FND-UI-005 records zero callback bytes in all shipped APFM resources;
FND-EXE-588 and FND-PARTY-044 give the earlier field/setter census and
its explicit block-copy, record-kind, alternative-base and split-pointer
limits. With an admitted typed resource lifecycle preserving those zeros,
the selector cannot choose mode two. Those findings do not alone prove
that lifecycle for every numbered-load state. Conversely, a nonzero target
cannot be assigned merely because an indirect instruction exists.

Q-COMBAT-049 now tracks this independently answerable record-admission
obligation: actual origins, callback-field preservation, selected modes
and target identity on the numbered-load paths. Q-COMBAT-048 retains the
other callback targets and their effects. No concrete nonzero APFM target,
whole-load health preservation or complete-reading declaration is claimed.

## How to reproduce

Use resident_listing.py on the licensed installed DSUN.EXE with
3D72:0635..3D72:06D5 and 3F96:0755..3F96:07B3. Track the selector's
saved A125 separately from current A15B and the setter's saved lookup
result. Follow the four signed bounds, A141/equality branch, both full
callback-pointer tests and the shared copy tail. Preserve the mode-zero
tail's absence of an A15B store. Compare FND-CONFIG-189's later target
reads and FND-EXE-588's existing producer census and limitations; do not
repeat that census without changed coverage or a new reference form.
