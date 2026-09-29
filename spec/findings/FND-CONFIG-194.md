---
id: FND-CONFIG-194
title: Graphics-slot release conditionally lowers the pool cursor and compacts following blocks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:28C5
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete resident 16-bit reading with header-derived MZ direct-call checks
environment: null
---

## Observation

FND-CONFIG-184 locates the complete resident
1BF3:28C5 body at `0x000139F5..0x00013A8F`.
It saves BP, DS, SI and DI, clears the direction flag,
and selects DS=CS. Before inspecting the supplied
handle, it reads indexed VGA register five through
ports 03CE/03CF, sets bit zero there, and writes
indexed sequence-register two with value 0F through
port 03C4. Every local exit later reads indexed VGA
register five and clears bit zero before restoring
the saved registers. It does not restore the prior
register-five bit, all other port state or the
incoming direction flag.

It doubles the supplied word handle into a slot offset
without an own index or sign bound. If the selected
slot's C04 flag word has any bit in 00A0, it skips
slot and paragraph changes. Otherwise it sets bit
0080 in that flag word. If bit 0040 was already set,
it exits without changing the E4E paragraph cursor;
this is the reference-slot path. The flag write occurs
before that second test. Neither skipped path has a
local success/error return value.

For a selected slot without those bits, the body reads
its +204 count as a word and subtracts it from CS:E4E
with word arithmetic. It sets a search endpoint to
the selected slot's +4 segment plus that count,
also with word arithmetic. There is no own check that
the selected slot is within the pool, that its count
is nonzero, that it is the last allocation, or that
the subtraction remains inside the initialized range
from FND-CONFIG-193. A signed caller guard or an FFFF
sentinel check elsewhere is not a check in this body.

It then scans at most 256 slot offsets, zero through
01FE in steps of two. For each slot it skips a flag
with any bit in 00E0 and compares the remaining slot's
+4 segment to the current endpoint. A match changes
that slot's +4 segment to endpoint minus the released
count, selects DS as the old endpoint and ES as that
new segment, and copies the matching slot's +204 count
times sixteen bytes forward from offset zero. It
advances the endpoint by the moved slot's count,
then restarts the 256-slot scan. If no match is found,
it exits. The destination and source segment ranges
and the count are not independently validated.

This is a chain of adjacent-block searches, not one
single full-array pass. Each match restarts the scan;
there is no own total-move or cycle bound. In particular,
malformed zero-count/alias state can fail to advance
the endpoint or slot segment. FND-CONFIG-183's ordinary
allocator computes counts from geometry, and
FND-CONFIG-193 gives the two fixed roots nonzero counts,
but neither establishes every direct release caller's
slot state. The wrapper 2D40:3BD1 in FND-CONFIG-184
skips signed handles at most one, protecting the two
fixed root handles on that wrapper route only.

Normal return restores BP/DI/SI/DS. A compaction path
changes ES without restoring it; both paths clear DF
without restoring its incoming value. AX and other
unsaved registers/flags are not normalized to a
release-success result. All paths touch VGA ports,
so memory copies and a local return cannot establish
native pixels, hardware restoration or accepted storage.

## Interpretation

The local release contract is conditional on slot flags
and may mutate the cursor and relocate later buffers.
The reference-slot path marks the slot free but leaves
the cursor unchanged. The ordinary path lowers the cursor
before searching for adjacent blocks; it does not roll
that cursor back if no later block matches. Its valid
memory and hardware effects depend on admitted slot,
count, segment, alias and native VGA state.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain direct callers,
slot/flag/count/segment writers, accepted pool bounds,
aliases, finite compaction and native hardware outcomes.
One reading supplies a finite chain of valid adjacent
blocks; another reaches this body with already-free,
reference or malformed metadata. Complete callers and
producer invariants, with owner observations for hardware
outcomes, would distinguish their native reachability.

A reading that every call reclaims paragraphs is ruled
out by the initial 00A0 and subsequent 0040 gates.
A reading that every returning path preserves ES, DF
or prior VGA state is ruled out by the copy and port
paths. No native or emulated result is claimed.

## How to reproduce

Read resident 1BF3:28C5 through 295E, including both
port phases, the two flag gates, E4E subtraction,
256-slot flag/endpoint scan, forward-copy count and
restart target. Check the moved slot's segment assignment,
DX endpoint update, saved-register pops and absence of
ES/direction-flag or port-state restoration. Compare
FND-CONFIG-193's initial roots and FND-CONFIG-184's
signed wrapper guard, keeping other direct callers and
native storage/hardware effects open.
