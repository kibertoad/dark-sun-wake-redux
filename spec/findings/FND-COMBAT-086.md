---
id: FND-COMBAT-086
title: Table initialization failure requests termination, and the registered exit handler calls table release
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067693..0x000676BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067942..0x0006796B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00072EA0..0x00072EB0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388..1000:041A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:019E..1000:01B0
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py and direct_callers.py at 6ebe3851; Capstone 5.0.9
environment: null
---

## Observation

FND-COMBAT-083 locates the combatant-table allocator at overlay 188
+0000 and its release helper at +03BA. FND-CONFIG-149 locates the
initializer call in overlay 180's +0141 routine, which the main routine
calls before the start window (FND-PARTY-031). The call at file 676AB
is followed by an AL test. Zero calls resident 1000:03DF with word one;
nonzero skips that call. The preceding initializer +046C has its own
separate zero-extended-AL test and termination-wrapper call.

Overlay 188 +0000 compares DS:193C with exactly one before doing the
metadata and allocation work. Equality jumps to its success tail, without
checking the primary table pointers or refreshing their copied aliases.
Other values enter the body. FND-CONFIG-150 and FND-COMBAT-083 record
the success and failure tails and pointer assignments. The bypass is not
itself proof of a live allocation.

Overlay 180 +0762 optionally calls 4A6E:0187 when DS:13F7 is nonzero,
then calls overlay 200 +05C5, 25AF:0036 and overlay 188 +03BA in that
order. There is no local result test before the table-release call.
FND-CONFIG-061 identifies +0141's registration of this +0762 trampoline
through 1000:030B, conditional on space in the exit table. FND-EXE-579
records the table's pointer producers and the computed callback site.
Thus this checked release route is explained by registration; an absence
of a literal direct call to +0762 would not exclude it.

The complete local runtime body 1000:0388 reads its third word argument
into SI. Zero enters the loop that decrements DS:3572 and invokes the
stored callbacks in reverse order. Nonzero skips that loop, the following
local 0163 call and the callback through DS:3676. Both paths then call
local far 01F3 and 0176. A nonzero second argument skips the termination
sequence and reaches RET 6. With second argument zero, current SI zero
also calls DS:367A and DS:367E; the path then calls 019E with the first
argument. 019E issues INT 21h with AH 4C and AL from that argument.
If that request returns, it changes CX and DX and jumps to 02B3; it does
not directly return to its caller (FND-CONFIG-163).

The four far wrappers supply these words to 0388:

| Wrapper | First word | Second word | Third word |
| --- | --- | --- | --- |
| 03DF | Its word argument | 0 | 0 |
| 03EE | Its word argument | 0 | 1 |
| 0400 | 0 | 1 | 0 |
| 040E | 0 | 1 | 1 |

The selected startup failure call uses 03DF, not the returning-cleanup
wrapper 0400. A direct-call candidate census for 0400 and 040E returns
no sites. The same census for 0388 returns the four near calls in these
wrappers, and the 03DF query finds the independently read far call at
file 676B6. These controls cover the searched near and declared far-call
forms, not arbitrary incoming transfers.

## Interpretation

The declared startup allocation-failure path requests process termination
through the callback-running exit wrapper. It is not a local failure return
that directly resumes window setup. The known later table-release route
is an exit-table callback, with its own preceding children. The runtime's
existence of a returning callback-running wrapper prevents treating every
possible exit-table invocation as inherently terminating. No direct caller
of that returning wrapper was found in the stated encoding census.

## Alternatives

An unconditional table-lifetime claim still needs DS:193C's admitted
producers, nonliteral pointer writers and aliases, later setup and actual
allocation/storage identity (Q-COMBAT-047). The bypass's success byte is
not a substitute for those obligations. DOS termination and interrupt
outcomes are external; the read fallback is not evidence of ordinary
continued play after failed allocation. Callback and child preservation,
including SI before its later test, remains conditional where not read.

The candidate census excludes indirect calls, jumps, split address-taking
and computed pointers; it cannot prove 0400 or 040E unreachable. Existing
FND-EXE-579 and FND-CONFIG-061 supply the selected exit-table registration
evidence rather than inferring it from a negative direct-call search.
No new game run, native failure or complete-reading declaration is claimed.

## How to reproduce

Read descriptor 180 ranges 67693 676BC and 67942 6796B, and descriptor
188 range 72EA0 72EB0, using overlay_listing.py on the licensed installed
DSUN.EXE. Read resident 1000:0388..1000:041A and
1000:019E..1000:01B0 using resident_listing.py. Bind all three argument
words through the actual pushes, distinguish the two independent gates,
and retain the post-interrupt fallback. Query direct_callers.py for
1000:0400, 1000:040E, 1000:0388 and 1000:03DF; check the named controls
against the bounded instruction readings. Reuse FND-CONFIG-061 and
FND-EXE-579 for registration and pointer producers instead of repeating
their earlier searches.
