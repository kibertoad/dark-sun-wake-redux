---
id: RULE-SCRIPT-005
title: Script variables start at 0
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-009, FND-SCRIPT-010, SRC-LIBGFF-839B11D]
conflicting: []
split_with: []
related: [RULE-SCRIPT-004]
---

## Summary

When the game sets up its script state for a new game, every global and local script variable
is 0 and every script string is empty.

## When it runs

When the script state is set up for a new game. When that is in the original is not known.

## Parameters

None.

## Inputs

None.

## Procedure

```text
for i in 0..count(global_flags):
    global_flags[i] = 0
for i in 0..count(global_numbers):
    global_numbers[i] = 0
for i in 0..count(global_big_numbers):
    global_big_numbers[i] = 0
for i in 0..count(global_strings):
    global_strings[i][0] = 0
for i in 0..count(local_flags):
    local_flags[i] = 0
for i in 0..count(local_numbers):
    local_numbers[i] = 0
for i in 0..count(local_big_numbers):
    local_big_numbers[i] = 0
```

## Outputs

Every variable of those kinds is 0, and every global string is empty.

## Edge cases

None known.

## What the sources say

SRC-LIBGFF-839B11D, `src/gpl/state.c`, `gff_gpl_state_init`: clears 800 global flags, 400 global
numbers, 40 global big numbers, 32 global strings and 13 global names, and then the local flags,
numbers and big numbers (64, 32 and 40 of them). A comment on its local state says it will have
to be tied to a region. Its sizes are its own choices and have not been compared with this
build.

## Differences between builds

None known.

## Open questions

- Where and when the original clears the variables, whether local variables are cleared on a
  region change or at another time, whether a loaded saved game replaces them, and how many there
  are of each kind in `global_flags`, `global_numbers`, `global_big_numbers`, `global_strings`,
  `local_flags`, `local_numbers` and `local_big_numbers` (Q-SCRIPT-005).
