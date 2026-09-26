---
id: FND-COMBAT-007
title: The routine that calls the status panel routine has eight callers in five segments
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:06CB..2C5F:070E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:33CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:13E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:0C08
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

`2C5F:06CB` has eight direct calls: `28C9:33CE`, `28C9:342E` and `28C9:348D`, all in one function,
and `2C5F:0175`, `2C5F:031E`, `31E0:13E2`, `31E0:1759` and `362C:0C08`, each in a function of its
own. Each passes two small constants: `(1, 1)` at six of them, `(1, 0)` at `2C5F:0175` and
`362C:0C08`. The eight instructions before each call hold no other constant.

`2C5F:06CB` first calls `2C5F:0334` with the first argument, the byte second argument and the word
at `4E47:0000`. It then calls the panel routine `2C5F:03F1` with the same two arguments only when
the word at `4C10:0019` is neither 0 nor 1 (FND-COMBAT-023).

## Interpretation

`2C5F:06CB` redraws something from several places in the game and adds the status panel when the
game is in combat. The first argument picks a drawing page (FND-COMBAT-022).

## Alternatives

The callers may be reached from combat and from elsewhere; none was followed further.

## How to reproduce

List the references to `2C5F:06CB` in Ghidra (six far calls and two near calls), and read eight
instructions before each. A byte search for `9A CB 06 5F 1C` finds the six far calls.
