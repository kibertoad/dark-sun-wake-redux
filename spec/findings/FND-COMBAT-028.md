---
id: FND-COMBAT-028
title: The data segment holds nine far pointers at 57E0:0663 to the names New, Okay, Stunned, Out Cold, Dying, Animated, Petrified, Dead and Gone
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0663..57E0:0687
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:14E9..57E0:1526
tool: Python 3.14.7 byte search
environment: null
---

## Observation

From file offset `0x4E463` (`57E0:0663`), `DSUN.EXE` holds far pointers with the stored segment
`0x47E0`, which the loader relocates to `57E0`. The first nine point to these NUL-terminated
strings, in this order:

| Index | Pointer at | String at | String |
|---|---|---|---|
| 0 | `57E0:0663` | `57E0:14E9` | `New` |
| 1 | `57E0:0667` | `57E0:14ED` | `Okay` |
| 2 | `57E0:066B` | `57E0:14F2` | `Stunned` |
| 3 | `57E0:066F` | `57E0:14FA` | `Out Cold` |
| 4 | `57E0:0673` | `57E0:1503` | `Dying` |
| 5 | `57E0:0677` | `57E0:1509` | `Animated` |
| 6 | `57E0:067B` | `57E0:1512` | `Petrified` |
| 7 | `57E0:067F` | `57E0:151C` | `Dead` |
| 8 | `57E0:0683` | `57E0:1521` | `Gone` |

The strings are stored one after another, and the class names follow them from `57E0:1526`
(`Cleric`, `Druid`, `Fighter` and so on). A search for a `push` of any of the nine string
offsets, and for an indexed load with the displacement `0x0663` or `0x0665`, found none.

## Interpretation

The list names the conditions a character can be in. Okay, Stunned, Out Cold, Dying and Dead
suggest more steps between healthy and dead than the manual's conscious, unconscious and dead
(RULE-COMBAT-003).

## Alternatives

No code that reads the list was found, so which condition each index stands for, and whether the
game uses the list at all, is not shown. The `Okay` the status panel draws is another copy at
`57E0:0F5B` (FND-COMBAT-022).

## How to reproduce

Read `DSUN.EXE` from file offset `0x4E463` as far pointers, convert each offset to a file offset
with `f = x + 0x57E00 - 0x10000 + 0x5200`, and search the file for `68` and each offset and for
the displacements.
