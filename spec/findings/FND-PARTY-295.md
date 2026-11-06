---
id: FND-PARTY-295
title: Loader selector operand census identifies one established indexed writer with an explicit pointer-admission condition
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0140..4AE5:016B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:06B1..4AE5:06E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000866A1..0x00086715
tool: tools/research/exec-census/window_operands.py, operand_range.py, store_values.py, resident_listing.py and overlay_listing.py at a993a06e; Capstone 5.0.9, xxhash 4.0.1, Python 3.12.10
environment: null
---

## Observation

Q-PARTY-134's loader selector is the byte at offset 0111 in the
state segment, initially 55CE (FND-EXE-176). All offsets here are hex.
The installed source remains 634416 bytes with XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The inventory is the repaired
3231-row DSUN.EXE inventory, not an exhaustive instruction census.

The window reporter over 010E..0112 finds nine operands in established
inventory paths: six reads, one indexed word write and two immediate
values. It recovers both selector loads at 4AE5:0149/0153 and the
word load at 4AE5:06D7 as positive read controls. The latter supplies
trampoline string stores; the source load itself writes no selector byte.
The report retains three string stores in that routine and fifteen
candidates outside established instruction starts.

The positive write control is overlay 197 +32D4, file 866D4.
Reading its actual entry +32A1 through its far return at +3314 shows
that argument 005C takes a branch which loads a word from an indexed
record under current DS:19C5. It then reloads the far pointer from
DS:19C9 for a store at +C4, and reloads DS:19C5 before word stores
at +112 and +110. The final +110 store uses this latter pointer
directly: the earlier record-index displacement is no longer in BX.
There is no intervening call between that reload and the +110 store.

This is an actual word write through ES:BX, not a fixed write to the
loader state. It can touch the selector only if its admitted physical
destination or the following byte aliases the selector's actual backing.
Resolve the current DS:19C5 pointer and state-segment identities at that
point before excluding it. Matching displacement 0110 alone proves
neither an alias nor separation.

The raw store reporter for 010E/010F/0110/0111 finds six patterns.
Two are byte stores to 0110, which do not touch 0111 under the same
segment binding. Two indexed arithmetic patterns lie inside the verified
selector-load and trampoline-word-load encodings. The final two are
the real overlay store and its prefix-stripped interior. The broader
window report also retains an interior FRSTOR candidate omitted by the
store reporter's mnemonic filter. None of these raw interiors is an
established additional writer; bypass entry remains unresolved.

## Interpretation

The established explicit-displacement paths in this window contain no
fixed selector writer and one indexed writer with a named pointer source.
This removes an unspecified local-writer search from Q-PARTY-134, while
retaining that pointer's alias admission. Larger overlapping writes,
other effective displacements, computed pointers, string/block transfers,
segment aliases, raw bypass entries and external effects remain outside
this bounded result. No exhaustive no-writer claim follows.

FMT-PARTY-007 stays supported. No client, queue item or complete reading
closes; runtime routing and the other preservation obligations survive.

## Alternatives

- The indexed store can be disjoint from the loader state under admitted
  allocation identities; the offset alone cannot establish that condition.
- A raw interior can become relevant through an unresolved transfer;
  rejecting it as an ordinary instruction does not settle that transfer.

## How to reproduce

Run operand_range.py with 010E 0112, window_operands.py with
010E..0112 and store_values.py with --indexed 010E 010F 0110 0111.
Use the installed source and current inventory. Require the two selector
read controls, trampoline word read and overlay write control above.
Decode resident 4AE5:0140..016B and 06B1..06E4; decode overlay 197
file 866A1..86715 from its established entry. Reports named
loader-selector-operands, window, stores, interiors and indexed-context
remain in GAME_DIR/analysis/handler-census-20261106/. Keep the raw
candidates and string-store exclusions. No original game ran.
