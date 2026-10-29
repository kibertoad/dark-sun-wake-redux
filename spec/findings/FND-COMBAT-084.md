---
id: FND-COMBAT-084
title: The numbered-load wrapper replaces the reader result when its local byte requests closing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070CEE..0x00070DE7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071A17..0x00071A87
tool: tools/research/exec-census/overlay_listing.py at 2d8d579d; Capstone 5.0.9
environment: null
---

## Observation

Overlay 187 +0EBE constructs a working filename using DS:44F2 and the
loaded darkrun.GFF name at DS:172D. After 4544:0000 returns, it zeroes
AH and tests AX, so only the returned AL controls the copy-result branch.
Zero returns AL zero immediately. The other branch performs the cleanup
and calls local +1BE7, saving its returned AL at SS:BP-1. It does not
test that byte before region preparation or the saved-table reader.

The reader +0A6C is called with argument zero. Its returned AL is stored
at SS:BP-2. After the object loop and second region-setup call, the wrapper
tests BP-1. Zero skips closing and returns the byte at BP-2. Nonzero calls
local +1C26 and replaces BP-2 with that helper's returned AL before returning
it. There is no combination of the reader and close results. FND-SAVE-022
and FND-COMBAT-081 describe the outer callers ignoring the wrapper result.

+1BE7 tests byte DS:167A. An already nonzero byte returns AL zero without
opening anything. Otherwise it calls overlay 182 +0000 with near filename
DS:172D, the far address of DS:144E and option words 2 and 04D4. A zero
returned AL calls overlay 180 +08A3. If that helper returns, control joins
the same DS:167A = 1, AL = 1 tail as the nonzero-open-result branch.
Thus the saved BP-1 byte denotes the local close obligation, rather than
an unconditional proof that the open succeeded.

+1C26 returns AL one without closing when DS:167A is zero. Otherwise it
passes the double word at DS:144E to 38FF:02B5 and tests the full returned
AX. Zero clears DS:167A and returns AL one. Nonzero calls overlay 180
+0867, then returns AL zero if that helper returns; it has no own store
clearing DS:167A on that branch. DS:144E is a numeric archive handle,
distinct from the active archive record pointer (FND-CONFIG-067).

FND-CONFIG-181 reads overlay 182 +0000's path construction and AX-to-AL
open-result conversion. FND-CONFIG-037 shows that a successful resident
open selects its new archive record. Neither that selection at the open
nor this wrapper's returned byte proves the selected archive remains
unchanged through all intervening calls before SAVE/5 and SAVE/6 reads.

## Interpretation

On a returning close path, a failed reader followed by close-helper success
can yield wrapper AL one. Conversely, close-helper failure replaces reader
success with zero. These are conditional control-flow results, not observed
native failures. The wrapper's result cannot serve as evidence that both
combatant tables were completely restored. When no close is requested,
the wrapper instead retains the reader byte in its local storage.

## Alternatives

A reading that combines copy, open, reader and close success into one
boolean is ruled out by the separate tests and BP-2 replacement. No
admitted save or actual operating-system failure is established here.
The error helpers may terminate rather than return; their returning
continuations above do not assert otherwise. Intervening callbacks,
stack aliases, state producers and active-archive changes remain with
Q-COMBAT-046, Q-COMBAT-047 and Q-COMBAT-048. The local byte stores and
tests do not establish all callers or all pointer lifetimes.

## How to reproduce

Run overlay_listing.py on the licensed installed DSUN.EXE with descriptor
187 and ranges 70CEE 70DE7 and 71A17 71A87. Decode the wrapper from
its established entry, not from an arbitrary byte inside an instruction.
Track both byte locals separately, the copy result's AH clearing, each
helper's AL versus AX test, the final BP-2 overwrite, and both error-helper
continuations. Resolve overlay callees through their declared FBOV fixups.
Compare FND-CONFIG-181, FND-CONFIG-037 and FND-CONFIG-067 for the already
recorded open wrapper, selection and numeric-handle contracts.
