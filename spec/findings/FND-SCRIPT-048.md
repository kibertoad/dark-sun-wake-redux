---
id: FND-SCRIPT-048
title: The startup region-script gate retains an early unsigned count test and reloads the region at dispatch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000699B7..0x00069D75
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0330..277B:03E2
tool: overlay_listing.py and resident_listing.py at e9f284d9; Ghidra 12.1.3 ReportDecompileMatches.java read-only on INST-overlay-20261029g; Python 3.12.10, Capstone distribution 5.0.9
environment: null
---

## Observation

This connected reading assesses the known startup gate's local decision to
call the regional MAS dispatcher. It reuses FND-COMBAT-053's caller chain,
FND-SCRIPT-028's dispatcher, FND-SCRIPT-007/019's script entry/loader
interfaces and FND-SOUND-071/074/075's region producers. It does not
establish a native startup, callback sequence or retained registration.
Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b.

Overlay 182's complete inventory body starts at 1167 and ends after its
far return at 1524, file 699B7..69D75. Before the long setup path it
updates present party combatant bytes, then tests current DS:0DAB at
11BE. Nonzero jumps directly to the epilogue and does not reach the
regional script call. On the zero path, each of the two resource calls
at 1255 and 1271 has a returned-AX comparison against FFFF; equality
also exits before that call. There is no general child-success test
for every intervening setup operation.

At 12A7 it compares current DS:264E with four using unsigned JA.
It saves byte one to SS:BP-1 for counts zero through four, otherwise
zero. The byte is published at 12B5, before the call to 3D72:0B84.
The gate has no later direct assignment to that byte. In particular,
the fresh count comparisons at 12BD, 12DD and 12EC construct arguments
for the region/object preparation helper 0365; they do not replace the
saved MAS-dispatch decision. FND-COMBAT-079 supplies that helper's
conditional object-rebuild interface.

Many calls intervene before the gate stores one to DS:0DAB at 141E.
At 1424 it tests the saved byte, not DS:264E. Zero skips directly to
the party-trigger loop. Nonzero freshly pushes DS:140C at 142A and
calls 1695:000B at 142E. The region is therefore not saved together
with the count decision and is not the value necessarily passed to
0365 earlier at 12FB. The local path has no equality check between
these two region reads.

The bounded decompiler matcher independently exposes a saved Boolean
from the early count comparison, separate later count expressions and
the conditional dispatcher call. Its inferred dispatcher signature
omits the region argument. The instruction-level push and relocated
call establish that argument; the inferred signature is not used as
evidence that it is absent. No decompiler output is committed.

On an intact returning path with the saved stack byte preserved, a
count change after 12B5 does not change this local dispatch choice.
For example, an early count of four followed by five still admits the
call; an early five followed by four still skips it. FFFF is unsigned
greater than four and selects the skip, not the low-count branch.
These are consequences of the local tests, not observed game runs.
Actual frame aliases, child continuations and the region read remain
separate input obligations.

### Dispatch is not yet execution of MAS/1

FND-SCRIPT-028's callee stores its incoming region, then requires script
state two before pruning and invoking selector-two script execution.
FND-SCRIPT-007 supplies the interpreter's separate ready-state check;
FND-SCRIPT-019 retains cache, load and failure conditions. Thus reaching
142E does not itself prove successful loading or execution of MAS/1.
That specific resource also requires the fresh region argument to be one.

The resident reset's 277B:03C1 store writes region 50, not one. It lies
on the nonzero-DS:13FB path beginning at 0330 (FND-SCRIPT-031), after
the cleanup calls and before the trigger-pool fill. The region argument
therefore needs a producer/history that reaches the gate; the CLI region
store of FND-SOUND-075 cannot be assumed to survive this later reset.
The default file value, a parser store, this reset store and a later
restored/current region are different points in the history. This does
not prove that MAS/1 is unreachable or that the default is never changed.

## Interpretation

Q-SCRIPT-024's known startup route has a specific local admission boundary:
the setup gate must reach the dispatch test with its early unsigned
low-count byte retained, and its fresh region must select resource one.
The local count decision and argument construction no longer need an
unidentified gate reading. Actual callback admission, children, frame and
region history, ready state, loading and registration retention remain
open. Independent transition and restored-record paths are not excluded.
No whole-entry complete reading, native execution or queue closure follows.

## Alternatives

Using the count at 142E instead of the saved byte omits the earlier
assignment. Treating the count as signed misclassifies FFFF. Treating a
parser override as the eventual region omits a later reset and other
writers. Treating a dispatcher call as completed script execution omits
its state guard and loader.

## How to reproduce

Use the pinned evidence Python and overlay_listing.py descriptor 182 for
699B7..69D20 and 69D20..69D79, retaining mas-admission-start-gate.txt
and mas-admission-start-gate-tail.txt under
GAME_DIR/analysis/handler-census-20261106/. The inventory body ends at
69D75; discard the next function's prefix from the tail context.
Use resident_listing.py 277B:02C9..277B:03E2 and
277B:03A0..277B:03DE for mas-admission-main-reset-prefix.txt and
mas-admission-region-default.txt. Reuse the prior reset and producer
findings rather than inferring their entire child behavior from these spans.

For independent navigation, open retained project
GAME_DIR/analysis/inventory-reconcile-20261029/projects/INST-overlay-20261029g
read-only with analysis disabled, process INST-MAPPED.EXE and run
ReportDecompileMatches.java at 7365:1167 with patterns 264e and 000b.
Retain mas-admission-gate-decompile.log beside the listings. Verify the
reporter's containing-function marker and matches, not only headless exit
zero. The retained overlay mapping converts 7365:1167 to shipped file
699B7; only shipped locations are cited above. No original game or
emulated call was run.
