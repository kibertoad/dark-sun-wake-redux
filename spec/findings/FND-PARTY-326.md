---
id: FND-PARTY-326
title: Controlled overlay near-transfer census finds no encoded entry to the two operand-interior mode-write candidates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068850..0x0006AD08
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FE30..0x00072B8B
tool: overlay_near_patterns.py at af49cea8; Python 3.12.10, Capstone module 5.0.7/distribution 5.0.9, xxhash 4.0.1; FND-PARTY-325's retained owner and caller reports
environment: null
---

## Observation

Q-PARTY-136 retains alternate entry to FND-PARTY-325's operand-interior
mode-write candidates. Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets below are hexadecimal.

The committed query scans every byte of overlay 182's code at
68850..6AD08 and overlay 187's code at 6FE30..72B8B. It excludes
their fixup lists. Its targets are 182 +2383 and 187 +277E,
the two remaining overlay candidate starts from FND-PARTY-325.
The resident candidate's narrower branch audit remains separate.

The query tests complete unprefixed rel8/rel16 call, jump, conditional
jump and loop encodings using 16-bit instruction-pointer arithmetic.
It finds zero patterns to either target, including patterns starting
outside inventoried instruction ranges or inside other instructions.
This is a negative result for those exact source regions and transfer
model, not a whole-program reachability claim.

### Independent source-case controls

FND-PARTY-325's retained owner listings and caller report establish
these expected sites before the new query is interpreted:

| Overlay | Target | Expected source file site | Kind |
|---|---|---|---|
| 182 | +2384 | 6ABCF | Short conditional branch |
| 182 | +24B4 | 6ABD1 | Near long jump |
| 182 | +236F | 699CC | Near call |
| 187 | +2797 | 725A7 | Short conditional branch |
| 187 | +27F6 | 72394 | Near long jump |
| 187 | +2B6C | 723A5 | Near call |

All six controls pass at inventoried instruction starts. The query
validates every control before printing target results. Its synthetic
controls separately test signed displacement decoding, short and long
forms, loop forms, truncated encodings, word-IP wrapping and rejection
of missing or operand-interior controls. Synthetic success alone is
not evidence about these game-source targets.

The input inventory is the installed DSUN inventory, normalized to LF
SHA-256 820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.
It supplies control ownership labels, not the search boundaries: the
negative pattern scan covers the full declared code regions above.
No empty inventory range is used as a negative source search.

## Interpretation

Q-PARTY-136 can remove unqueried same-overlay unprefixed near-transfer
patterns as possible entry mechanisms for these two named candidates.
FND-PARTY-325 already classifies their normal owner streams and the
tested direct-call forms. Far, indirect, table, computed-segment,
operand-size-32, dynamically changed and unqueried-code transfers
remain outside this exclusion. Actual mode history, physical aliases,
block/string writes and between-read preservation remain as recorded.

This removes a bounded incoming-transfer component. It does not close
the whole queue item, classify every writer or declare a complete
reading. FMT-PARTY-007 remains supported. The broad locations above
identify census scope, not complete readings of all functions they cover.

## Alternatives

- A missed inventoried range explains the zero result: candidate patterns
  are scanned at every byte of the full declared overlay code.
- Failed controls can support the result: the query rejects output until
  all six independently selected source controls pass.
- No modeled near pattern proves no alternate entry: far, indirect,
  computed, modified and other excluded mechanisms remain unresolved.

## How to reproduce

Use overlay_near_patterns.py with the installed source and inventory,
targets 182+2383 and 187+277E, and these control arguments:
182+2384@6ABCF, 182+24B4@6ABD1, 182+236F@699CC,
187+2797@725A7, 187+27F6@72394 and 187+2B6C@723A5,
each preceded by --control. Require matching source/inventory identities,
six PASS controls, the stated code bounds and zero target patterns.

Retain getter-mode-overlay-near-reviewed.txt under
GAME_DIR/analysis/handler-census-20261106/. The earlier
getter-mode-overlay-near-controls.txt has only the original two controls;
use the reviewed six-control report for this finding. No original game,
emulator session or emulated call supplied evidence here.
