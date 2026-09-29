---
id: FND-CONFIG-192
title: The admitted handle primitive prepares shared copy state and touches VGA ports on every returning transfer path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:43AE
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete resident 16-bit reading from entry through every local branch and return
environment: null
---

## Observation

FND-CONFIG-191's 2D40:3B30 admits two nonnegative
handles whose coordinate validators return nonzero AL,
then passes them to resident 1BF3:43AE. The complete
primitive spans `0x000154DE..0x00015853`, ending with
far return at `0x00015852`. It saves DS/SI/DI,
selects DS=CS and doubles each stacked word handle.
It contains no further call, interrupt or own return-
normalized success/error result. Its later IN/OUT port
operations are hardware dependencies, not ordinary memory
behavior. It is read statically here, never executed.

For each supplied handle, it copies reference, count,
four coordinate and flag words from the slot arrays
at offsets 4, 204, 404, 604, 804, A04 and C04
into separate CS-relative scratch groups. It derives an
inclusive horizontal word-group count from the difference
of coordinates individually shifted right two, plus one,
and an inclusive row count from the coordinate difference
plus one, with word arithmetic. FND-CONFIG-191 bounds
the wrapper's separate screen-coordinate predicates, not
all direct callers or the primitive's native slot inputs.

When bit 0040 of a selected flag is set, it follows
that slot's +4 word as the next slot offset until a
flag without that bit is reached. The resulting slot
provides a memory segment word, count, coordinate bounds
and derived inclusive stride/row values for that side.
It repeats the process for the second handle. There is
no own handle-index, chain-length/cycle, memory extent,
reference validity or capacity check before these accesses.
Shared scratch fields are overwritten before transfer begins;
valid chains, slots and aliases remain input conditions.
FND-CONFIG-193 reads one initializer that assigns two fixed
root slots and pool words before later requests; the
primitive does not check that it ran or that those words
and slots remain unchanged.

The first side's resulting segment later becomes source DS;
the second becomes destination ES. The transfer row count
is a signed minimum of the two selected row counts.
The horizontal count is a signed minimum of the selected
inclusive coordinate widths, then is reduced by one for
the per-row remaining-column state. There is no own
positive-size check at that point. Under stable admitted
wrapper fields with ordered coordinates inside 320 by
200, those selected counts are positive; incomplete direct
callers and malformed/reference state are separate cases.

The source and destination starting rows and a direction
word depend on a signed comparison of their selected
starting row coordinates. A source row word signed-at-least the destination row
selects direction one and the original rows. A smaller
source row selects minus one and advances both starting rows
by transfer row count minus one, using word arithmetic.
Each side's initial memory offset combines the selected
horizontal coordinate shifted right two, its root horizontal
origin and its row displacement times the root stride.
Stride 80 (0050 hexadecimal) has a separate shift/add
route equivalent in the named word range to that product;
other strides use multiplication. Derived segments/offsets
are not checked against an accepted buffer extent.

A separate scratch flag is initially zero. Equal source
and destination root segment words, equal adjusted starting
row words, and inclusive horizontal interval overlap set it
to one. The interval predicates are signed comparisons;
this is the local scratch-selection rule, not a universal
memory-overlap or alias-safety guarantee. It does not compare
all root metadata or prove that different segments are
disjoint storage.

At each transfer row, source DS and destination ES are
selected from the root segment words, and the initial
source/destination offsets are saved in CS scratch. On
the flagged overlap route, the body reads/writes VGA
ports 03CE/03CF to set bit zero for indexed register
five, writes ports at 03C4 for indexed register two
with value 0F, and copies a source byte-group count to
fixed far scratch AFFB:0000. For positive admitted
width W, that count is floor((W-1)/4)+1. It then
uses that scratch as source and clears bit zero through
a further 03CE/03CF sequence before the later transfer.
There is no own scratch-capacity, saved-original-port-state
or accepted-native-mapping check.

Both the flagged and unflagged routes select the source
plane from the selected horizontal coordinate's low two
bits, write an indexed value through port 03CE, select
a destination mask through the four-byte table at CS:26DC
and write it through port 03C4. They copy word pairs
and an odd remaining byte forward between source DS and
destination ES. The remaining horizontal count and a
four-phase counter bound plane phases under positive
admitted width; source plane and destination mask advance
between phases, incrementing their offsets on the respective
wrap. The mask table's runtime contents and all VGA effects
remain open. Memory transfer instructions alone do not
establish a final pixel value under those port settings.

The next row restores the saved initial offsets, then
adds or subtracts the two root strides according to the
direction word. A decremented word row count of zero
ends the body; other values repeat the row setup. No
independent zero/negative row rejection protects a direct
caller with malformed state. Under the explicit admitted
positive row conditions, it takes that finite row count;
those conditions do not prove native input reachability,
correct overlap handling or accepted buffer capacities.

On normal completion the body clears the direction flag
and restores saved DI, SI and DS. It does not restore
the earlier shared CS scratch contents or snapshot all
VGA state. AX, other unsaved registers and flags are
not normalized to a performed-copy result. The forwarding
wrapper's ignored or forwarded result in FND-CONFIG-191
therefore remains separate from hardware success, accepted
contents and the outer service's state commit.

The resident mapped base is the established 1BF3 target,
not an overlay trampoline. The complete body contains
no direct far-call segment operand to relocate. A query of the
header-derived MZ table finds no relocation anywhere in this
complete body, including the AFFB operand. AFFB is a literal
scratch segment used by its overlap path; table-
derived source/destination segment words remain dynamic.
No own ordinary-DS 0DAB/1440/cache-field store occurs:
its own nontransfer metadata stores use the selected CS
segment, while the actual copied ranges follow dynamic
segments and may alias other state. An absent literal
mode write is not a complete absence-of-effects claim.

## Interpretation

The primitive has a complete local path reading, with
shared preparation state, reference walks, direction and
scratch selection, port operations and phased transfers.
Its finite positive-input counts and restored registers do
not establish accepted runtime slots, buffer extents, overlap
safety, VGA behavior or final rendered pixels. Even a
nonoverlap transfer path touches ports. The caller's admitted
coordinate bytes and later state commit cannot replace those
remaining producer and hardware conditions.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain all direct callers,
slot/flag/reference/segment/coordinate/count and mask-table
producers, finite chains, accepted capacities and offsets,
aliases, actual VGA/scratch mapping and native presentation.
One reading supplies valid admitted slots and hardware state
for a finite transfer; another has a different reference,
mask or alias configuration despite accepted coordinates.
Complete input evidence and owner observations where the
code does not decide hardware outcomes would distinguish
their native reachability and results.

A reading that only the overlap path touches ports is
ruled out by the common plane setup. A reading that a
normal return restores all shared/graphics state is unsupported:
CS scratch remains changed and port values are not generally
snapshotted/restored. A RAM-only emulation of MOVS does
not establish VGA pixels or accepted scratch storage. No
native or emulated result is claimed; Q-SCRIPT-007 must
not treat this as an interrupt-free hardware validation case.

## How to reproduce

Read resident 1BF3:43AE through 4722 from its entry,
including both reference walks, scratch groups, signed count
and row comparisons, word arithmetic and stride-80 special
routes. Follow the segment/row/interval scratch predicates,
AFFB staging, every port access, common plane/mask setup,
odd/even forward copies and row-direction termination.
Check CS-selected metadata versus dynamic source/destination
ranges, final direction flag and saved-register restoration.
Compare FND-CONFIG-191's separate wrapper predicates;
retain direct callers, reference/segment/mask producers,
capacities, aliases and hardware outcomes instead of deriving
pixel parity or universal rollback from a local return.
