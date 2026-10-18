---
id: RULE-SCRIPT-005
title: Script variables start at 0
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-026, FND-PARTY-241, FND-PARTY-040, FND-SCRIPT-009, FND-SCRIPT-010, SRC-LIBGFF-839B11D]
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
- The interpreter does not clear the global variables after the set-up. The main routine clears
  them, without the locals, at `277B:035D` (FND-PARTY-241), and loading a saved game that holds
  them replaces them whole [FND-PARTY-241, FND-SCRIPT-026].

## What the sources say

SRC-LIBGFF-839B11D, [libgff’s state.c](https://github.com/dsoageofheroes/libgff/blob/839b11d0ac63492e28f70968cfc3d967828958f5/src/gpl/state.c), `gff_gpl_state_init`: clears 800 global flags, 400 global
numbers, 40 global big numbers, 32 global strings and 13 global names, and then the local flags,
numbers and big numbers (64, 32 and 40 of them). A comment on its local state says it will have
to be tied to a region. Its sizes are its own choices; this build's differ (FND-SCRIPT-026).

## Differences between builds

None known.

## Open questions

- When the main routine's clear at `277B:035D` runs, and which saves hold the variable blocks
  (FND-COMBAT-034 writes them only for selector 0), so whether loading a saved game the player
  made replaces the local variables too (Q-SCRIPT-005).
