---
id: FND-ITEM-029
title: Current inventory-range segment-write output leaves item entry identity and child preservation separate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00000000..0x0009AE30
tool: tools/research/exec-census/segment_register_writes.py at 5fe52a88; Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9)
environment: null
---

## Observation

Q-ITEM-011 combines segment identity with frame preservation. Existing
FND-PARTY-215 supplies startup's SS replacement, FND-PARTY-219 observes
DS equal to SS at main in its fixed run, and FND-RNG-053/061 supply earlier
segment-writer readings. None observes this item's later near-pointer call.
The existing inventory-range reporter is reused on the current inventory,
rather than treating those earlier census counts as current or building a
second scanner.

For coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv at 5fe52a88, the reporter emits
705 apparent DS writes in 287 routine rows. It reports no DS writes in
the requested forwarding wrapper at file 64F5F, hit wrapper at 6500F or
effect switch at 65653. None of these three rows appears in its SS output.
These are results of linear decoding of each declared range, not a claim
that all native paths or all bytes were decoded as established instructions.

The SS output contains eighteen apparent writes. Five match independently
recorded ordinary sites: startup 1000:00BB, the temporary pair in
1000:1464, and the timer handler's pair in 4842:040C. The other thirteen
lie in trampoline rows, where ordinary linear decoding interprets the
stub operands as instructions. FND-ITEM-020's independent boundary audit
and FND-RNG-053/061's trampoline distinction must be retained; these are
not thirteen additional native stack changes.

Requested positive controls reproduce the startup DS assignment, both
temporary SS assignments, and the timer handler's DS load/restore and
SS pair. The latter demonstrates that the inventory now includes the
timer writer omitted by the oldest census. The reporter also recognizes
LDS/LSS syntactically, but this run supplies no independently established
positive LSS case and makes no absence claim for that instruction kind.

## Interpretation

The requested item rows supply no own decoded segment assignment to
explain a mismatch. This does not prove their full preservation: the
reporter does not follow children, interrupts, overlay loading, unresolved
transfers or runtime-written instructions. It does not detect every
possible decode failure or establish ownership for arbitrary linear bytes.
FND-RNG-096's abstract DS model also assumes a value for SS in some
transfers and treats overlay calls specially; its final clock-path result
cannot simply be applied to these item paths.

Segment entry producers and reached children remain Q-ITEM-011. Saved-byte,
frame-span and alias admission can be investigated independently and are
Q-ITEM-013. This split does not raise a rule's status, establish DS/SS at
the item call, or claim preservation of the flags byte.

## Alternatives

- Equality at main is not equality at every later item call.
- No writer reported in a routine row is not proof of a whole-call contract.
- Trampoline operands are not ordinary instructions merely because a
  decoder gives them segment-register mnemonics.
- The DS count includes syntactic output, not 705 proved native writes.

## How to reproduce

Resolve installed DSUN.EXE through tools/game-dir.mjs. With the locked
evidence Python run:

```text
python -I tools/research/exec-census/segment_register_writes.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv 0x0006500F 0x00065653 0x00064F5F 1000:0000 1000:1464 4842:040C
```

The reader verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. It searches only ranges declared in the
named inventory, from each range start, in 16-bit Capstone mode. Its
recognized write forms are MOV/POP of DS or SS, LDS and LSS; the docstring
defines the exact operand test. There is no CFG or execution model, no
claimed full-source byte scan, and no negative-query completeness result.
Keep output only in GAME_DIR/analysis/hit-incoming-20261104/
item-segment-writers.txt. No original listing or bytes are committed.
