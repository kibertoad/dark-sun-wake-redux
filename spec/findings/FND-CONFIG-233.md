---
id: FND-CONFIG-233
title: Graphics cleanup releases the stored FONT pointer without an own pointer clear and reinstalls the saved graphics record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0326..39D1:03A6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:05E1..39D1:0632
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067942..0x00067A28
tool: resident_listing.py, overlay_listing.py, window_operands.py and direct_callers.py at bbecdbdd; Ghidra 12.1.3 read-only INST-single-20261103b; FND-CONFIG-165/171/175/207 and FND-SAVE-019
environment: null
---

## Observation

This identifies a concrete lifetime transition for the FONT buffer used by
FND-CONFIG-206/232's width reader. The source is installed DSUN.EXE,
634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
The distinction is between initiating release, clearing a pointer field,
and proving that no subsequent reader uses the old value.

### Pointer publications and bounded census

FND-CONFIG-207 and FND-PARTY-283 identify returned pointer words
at DS:A0E9/A0EB and its copies to A0ED and A043 in graphics
initializer39D1:0009. A043 is the font field at graphics recordA03D+6.

The current inventory-based operand report for A0E9..A0F1 lists ten
operands: three initializer stores, a pointer reload, cleanup's comparison
and argument push, three other argument pushes and one4328:0000
load. The A043..A047 window lists five operands: the initializer's
double-word store and four argument pushes. Thus the listed own stores
in these windows are the known initializer publications; cleanup contributes
reads. The independent publication sites0105,0109,0132 and0136 provide
positive controls. Ten and five raw candidates respectively are all inside
instructions of the listed decode; none supplies a new established store.

This report covers encoded displacements/immediates in inventoried ranges,
not all possible writers. Computed or based addresses, bulk copies,
physical aliases, alternate instruction streams, external effects and
changes to the graphics-record root remain separate obligations. String
store counts are zero in the routines with these window hits; that does
not exclude bulk writes in other routines.

### Cleanup and restored root

39D1:0326 has the runtime stack guard, then calls05E1,3D72:0B5F
and4464:0024 before testing the current double word at A0ED. A
nonnull value is pushed into444C:0092; the caller removes four argument
bytes but does not store the returned pair or test its release status.
There is no own clear of A0ED, A0E9 or A043 on this path.

It similarly conditionally releases9E74 and A145. A further A14D
release requires both A149 and A14D nonzero. None of these own release
sites assigns returned zero to its source field. It finishes by calling0609
and returns normally. This is not an inference that all callees leave all
these fields unchanged or that each release succeeds.

FND-CONFIG-171/175 give the connected bracket helpers. 05E1 saves
the graphics context through4328:00E0 at DS:A05B, obtains the current
record pointer through1BF3:7AAB and stores it at A057/A059. 0609
restores the context through4328:0118, then passes the pointer freshly
read from A057 to1BF3:5945, which installs it at CS:1052/1054.
The final operation therefore reinstalls that saved record pointer;
it is not a null-root assignment.

Under admitted ordinary segments, distinct valid frames/context storage,
and preservation of A057 and the named fields across reached callees,
the normal suffix can leave the old FONT pointer in A0ED and A043
and restore a root that still names A03D. If its earlier FONT release
actually relinquished or repurposed the allocation, these pointer values
alone no longer establish a live font. Successful release and a subsequent
width-reader invocation are not observed or established here.

### Connected caller and preceding helper conditions

The direct-call report finds overlay180+07D8. Decoding from the established
shutdown routine+0762 of FND-SAVE-019 qualifies the call: nonzero byte
4DE7:0001 selects0326, and its ordinary return is followed by clearing
that byte. This supplies a shutdown consumer, not proof that cleanup can
only run at shutdown; indirect and changed transfers are excluded.

3D72:0B5F conditionally calls0B84 according to3330 and33B6/33B8.
4464:0024 saves DS/flags, loads its relocated data segment, and can
call44D0:0040,44B6:0075 and149B depending on33CC/33CE. Those
reachable effects, their pointer/header arguments and guard paths must
be admitted before asserting unchanged FONT fields across the entire
cleanup. Their existence does not turn a missing own clear into a
guaranteed successful release or a guaranteed stale-pointer use.

## Interpretation

Q-CONFIG-008 now has a named FONT release route and exact
save/release/restore order. Its remaining lifetime question is preservation
through the named preceding helpers, actual release/alias state and
whether an admitted later reader can follow the retained record before
reinitialization or exit. An automatic pointer-clear or root-null model
is ruled out for this body's own operations. Q-CONFIG-013 retains the
heap outcomes classified by FND-CONFIG-230/231.

No native defect, complete-reading declaration, queue closure or parity
promotion follows. The static result does not claim a read after release.

## Alternatives

Treating the wrapper's zero return as a field clear ignores the missing
assignment. Treating0609 as a null-root setter ignores its saved pointer
argument. Treating a shutdown caller as a universal lifetime proof assumes
all incoming routes and later execution. Treating the direct operand census
as a complete alias audit exceeds its recorded exclusions.

## How to reproduce

Run window_operands.py with the approved source and current
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv for A0E9..A0F1 and
A043..A047. Retain font-pointer-field-window.txt under
GAME_DIR/analysis/handler-census-20261106/ and verify the publication
controls and every raw candidate's interior classification.

Run resident_listing.py for39D1:0326..03A6,05E1..0632,
3D72:0B5F..0B84 and4464:0024..0083. Reports are
font-cleanup-connected.txt, font-cleanup-record-save-restore.txt,
font-cleanup-restore-tail.txt and font-cleanup-preceding-helpers.txt in
that directory; some continue into neighboring context. Use
font-cleanup-direct-callers.txt from direct_callers.py for39D1:0326.
Qualify its call using overlay_listing.py descriptor180,file67942..67A28
from the established+0762 entry, retained as
font-cleanup-established-owner.txt. The separate earlier interior-start
context report is not used to establish this caller's instruction stream.

In the read-only resident project run ReportDecompileMatches.java at
39D1:0326 with a0ed,0092,0609 and05e1, requiring its function
marker in font-cleanup-connected-decompile.log. Verify instructions
for missing assignments, argument cleanup and the root setter's input.
No original-game or emulated call was run; original-derived output stays local.
