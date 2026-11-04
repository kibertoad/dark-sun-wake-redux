---
id: FND-CONFIG-221
title: Window-callback dispatch has no global fallthrough and zero window count rewrites the global event kind
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:071F..39D1:0752
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0752..39D1:0817
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0819..39D1:0854
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0854..39D1:0913
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:05E1..39D1:0632
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DA85..0x0007DAC3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E00C..0x0007E014
tool: tools/research/exec-census/resident_listing.py and overlay_listing.py at 4893147e; bracket preservation FND-CONFIG-171/175 and Save/Load callback FND-COMBAT-081
environment: null
---

## Observation

This replaces FND-CONFIG-079's erroneous window-zero/global-fallback
reading and retains its bounded event-copy and callback-route facts.
Q-COMBAT-046 needs event admission into Save/Load; Q-CONFIG-008
reuses these shared message-callback routes. Offsets below are hexadecimal.
All routing is the own body after its guard with valid frame/DS storage;
actual callbacks, asynchronous effects and aliases remain conditions.

### Dispatch routes are separate branches

39D1:071F copies its 24-byte event argument onto the outgoing stack
through 1000:0699, calls local 0752, saves returned AX in its frame,
calls 4328:0B4C and restores that saved word before returning. This
is a by-value stack copy, not a guarantee that other event storage or
the saved result survives aliases and children. FND-CONFIG-171 bounds
the post-call helper; it is not assumed inert.

0752 selects the following own routes:

| Event first word / state | Own callback route and event word |
| --- | --- |
| Three | Nonnull DS:A0F5 is called with the copied record; null returns zero. This is not the A0F1 hook. |
| One | Return FFFF before window-count selection. |
| Other kind; word DS:A103 zero | Nonnull DS:A0F1 is called after changing this stack record's first word to three; null returns zero. |
| Other kind; DS:A103 nonzero; current DS:A0FD and its window +F5 callback nonnull | Call that window callback with the copied event. |
| Other kind; DS:A103 nonzero; either current pointer or its +F5 callback absent | Clear DS:A0FD, then call nonnull DS:A0F1 with the copied event; null returns zero. |

On the zero-count route, 07B6 stores three to BP+6 and the later call
is directly through A0F1. It does not jump back to the event-three
branch or substitute A0F5. On the nonzero-count absent-window/callback
route, 0819 clears A0FD without an own rewrite of the event kind.
The selected count route is not rechecked after a callback returns.

### A zero window result does not invoke the global hook

The window branch snapshots DS:A0FD locally, checks current global
pointer and window +F5, calls 0609 and copies the event onto the stack.
It rereads +F5 through the saved window pointer for its indirect call
at 0802. Target and pointer preservation between test and call remain
required; the earlier nonnull test does not freeze the callback.

It saves callback AX in SI, calls 05E1 and tests SI at 0810.
Zero jumps directly to 084F, which clears AX before returning.
Nonzero jumps to the shared SI-to-AX return at 0793. Neither edge
reaches 0819's global fallback. This absence of an own fallback edge
does not require assuming that 05E1 preserved the callback result.
Under FND-CONFIG-175's valid distinct guard-bypass conditions, SI is
preserved and the return reflects that result: zero returns zero;
nonzero returns the full nonzero word.

Each other admitted callback route also brackets its call with 0609
and 05E1 and retains AX in SI. The global target is reread at the
call instruction after an earlier test. FND-CONFIG-175 supplies the
conditional DS/SI preservation of the named bracket children; guards,
buffers, saved frame, actual targets and interrupt timing remain open.
These branches contain at most one own indirect callback invocation
per dispatch. Calls inside helpers or reentrant callbacks are not excluded.

### The event-wait loop is another admission layer

0854 obtains a packet through 4464:02FE and calls builder 097D.
After a zero builder result, nonzero event word +2 or +4, or kind
one/four/five, reaches an AX-zero return from this loop. It does not
perform its own 071F call on those branches. The caller may dispatch
the returned record separately; no record is deemed discarded merely
because this helper returns zero.

For the other zero-identifier cases, it copies the event and calls
071F. A zero dispatch result loops back for more input; nonzero
reaches its returning result. The no-packet A0F5 path and builder's
own return also have independent result tests. Thus a pointer-hit
identifier reaching the output record and a callback invocation are
different steps. This retains the limited loop observation from the
old finding without treating it as an unconditional dispatcher call.

### Consequence for the Save/Load handler

FND-COMBAT-081 maps overlay 192 +0785. It tests kind two and six;
other kinds jump to +0D0C, which sets SI FFFF and returns that word
without an action child. Therefore, if A0F1 still names this handler,
the zero-window-count route's preserved kind-three copy cannot enter
its load or navigation branch. Its FFFF return is not a completed load.

A nonzero count with absent current window can instead forward kind
two or six unchanged through A0F1. Which handler/window is registered,
what the count means at that instant, event fields, local/storage
preservation and subsequent control gates still need admission. Merely
finding the Save/Load pointer in the global hook proves none of them.

## Interpretation

The global callback is an alternative dispatch route, not a second
attempt after a window callback returns zero. Registered-window count
also changes the event contract forwarded globally. This removes two
incorrect assumptions from Q-COMBAT-046's input-admission work and
corrects shared citations in Q-CONFIG-008. No native event, stale
callback execution or double invocation is asserted.

The known copy, window-field and global registration mechanisms remain
available under their stated conditions. Actual caller/event producers,
count/target lifetime, guard outcomes, aliases and external effects
remain unresolved. No question closes or evidence status is promoted.

## Alternatives

FND-CONFIG-079 incorrectly said a zero window result falls through.
The 0812 zero edge returns zero, and its nonzero edge also returns;
neither reaches the global branch. The old finding is retained as
superseded. Its correct 24-byte copy and conditional window/global
routes are retained here with the count-zero event rewrite explicit.

An unchanged kind-six global event requires the nonzero-count branch
under the described inputs. A zero count can still call A0F1, but it
forwards kind three and is not the same action-admission case. Neither
case establishes physical keyboard mapping or native window state.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. The readers verify length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read the listed resident ranges,
excluding the inventoried gap 0817..0819. Trace 07B6's stack-word
rewrite, 0802's window callback, both edges after 0810 and the distinct
0819 global path. Keep bracketing calls and their conditions explicit.
Read descriptor 192 at 7DA85..7DAC3 and 7E00C..7E014 to check the
kind-three rejection without decoding tables as instructions. Compare
FND-CONFIG-083/171/175/216 and FND-COMBAT-081/098.

Retain event-dispatch-routes.txt only under
GAME_DIR/analysis/hit-incoming-20261104/. No original bytes or event
memory are committed, and no original game or emulated call is run.
