---
id: RULE-SCRIPT-005
title: Script variables start at 0
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-026, FND-PARTY-241, FND-PARTY-040, FND-SCRIPT-009, FND-SCRIPT-010, FND-SCRIPT-031, FND-SAVE-008, FND-SAVE-022, FND-COMBAT-034, FND-COMBAT-035, FND-SOUND-080, SRC-LIBGFF-839B11D]
conflicting: []
split_with: []
related: [RULE-SCRIPT-004]
---

## Summary

When the game sets up its script state, before the first script runs, every global and local
script variable is 0 and every script string is empty. The local variables are cleared again
each time the interpreter switches to another script.

## When it runs

The procedure runs once when the game sets up the script state, before it runs `MAS` 99
[FND-SCRIPT-026, FND-PARTY-040]. Its part for the local variables runs again whenever a call of
another script, or the return from one, makes a script other than the current one current, and
when a script run starts while the state is set up [FND-SCRIPT-026].

## Parameters

None.

## Inputs

None.

## Procedure

```text
# when the script state is set up
for i in 0..808:
    global_flags[i] = 0
for i in 0..200:
    global_numbers[i] = 0
for i in 0..20:
    global_big_numbers[i] = 0
for i in 0..10:
    for j in 0..42:
        global_strings[i][j] = 0
# when the script state is set up, and again whenever another script becomes current
for i in 0..72:
    local_flags[i] = 0
for i in 0..32:
    local_numbers[i] = 0
for i in 0..10:
    local_big_numbers[i] = 0
for i in 0..10:
    for j in 0..42:
        local_strings[i][j] = 0
```

## Outputs

Every variable of those kinds is 0, and every string is empty.

## Edge cases

- The set-up clears the global flags as 101 bytes, so the flags hold 808 bits; the local
  flags are 9 bytes, 72 bits [FND-SCRIPT-026].
- A script keeps its local variables across a call of another script only by copying them out
  with request 29 of instruction `0x22` before the call and back with request 30 after it; the
  shipped scripts do so (FND-SCRIPT-026).
- Requests 29 and 30 copy the four local blocks to and from one fixed
  533-byte snapshot. There is no nesting index: another save replaces the
  snapshot. Restore uses the current variable-block pointers and does not
  restore those pointer values. Successful preservation requires intact,
  independent buffers and unchanged saved bytes; the helpers do not validate
  those conditions [FND-SOUND-080].
- The interpreter does not clear the global variables after the set-up. The resident
  reset tests current `DS:13FB` at `277B:0330`: zero skips the reset. Nonzero
  enters a cleanup loop while `DS:264E` is unsigned greater than zero, then
  makes the two setup calls before reaching the global clear at `277B:035D`.
  On the returning continuation it clears the four global blocks, without
  clearing the locals there [FND-SCRIPT-031, FND-SCRIPT-026, FND-PARTY-241].
  This is conditional reset ordering, not a guarantee that the cleanup loop
  terminates or that its children preserve the pointers.
- The player Save Game wrapper calls the numbered writer with selector zero;
  the player Load Game wrapper calls the numbered reader with selector zero
  after its working-archive preparation [FND-SAVE-008, FND-SAVE-022]. Selector
  zero admits all 61 source entries; region-specific nonzero selectors admit
  only the first 17 [FND-COMBAT-034, FND-COMBAT-035]. Thus the player's
  numbered path includes both the global and local variable blocks, not just
  the globals. The source-table entries 29..36 (zero-based) are rebound to
  the eight variable pointers by the preparation routine [FND-SCRIPT-026]:

  | SAVE resource | Variable block | Recorded byte count |
  |---|---|---|
  | 30 | Global flags | 101 |
  | 31 | Global numbers | 400 |
  | 32 | Global big numbers | 80 |
  | 33 | Global strings | 420 |
  | 34 | Local flags | 9 |
  | 35 | Local numbers | 64 |
  | 36 | Local big numbers | 40 |
  | 37 | Local strings | 420 |

  These are the recorded source bindings and lengths, not an enforced
  saved-file schema. With those bindings and successful corresponding
  transfers, the load replaces both sets of block bytes. The reader requests
  whole resources into the supplied destinations, can stop after a failed
  read and does not roll back earlier writes; a failed transfer need not
  leave its destination unchanged [FND-COMBAT-035]. Save write results are
  not all checked, so a requested save is not proof of complete persistence
  [FND-SAVE-008]. No atomic round trip or unconditional post-load value is
  inferred. Later interpreter switches still apply the local-clear rule;
  the variable-block restore is not a restore of all interpreter metadata
  [FND-SCRIPT-026].

## What the sources say

SRC-LIBGFF-839B11D, [libgff’s state.c](https://github.com/dsoageofheroes/libgff/blob/839b11d0ac63492e28f70968cfc3d967828958f5/src/gpl/state.c), `gff_gpl_state_init`: clears 800 global flags, 400 global
numbers, 40 global big numbers, 32 global strings and 13 global names, and then the local flags,
numbers and big numbers (64, 32 and 40 of them). A comment on its local state says it will have
to be tied to a region. Its sizes are its own choices; this build's differ (FND-SCRIPT-026).

## Differences between builds

None known.

## Open questions

- Actual initialized-block postconditions under the admitted allocation,
  pointer and fill bindings still need the cases held by Q-SCRIPT-015.
  The reset guard and player save/load selector question is answered above;
  it does not establish this whole entry or guarantee a successful load.
