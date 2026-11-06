---
id: FND-PARTY-323
title: Shutdown alias-pattern candidate is excluded because its supposed offset word is itself relocated
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
    offset: 0x00047BE2..0x00047BE4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047BEA..0x00047BEC
tool: tools/evidence/report.mjs operand at 5b227a5e with @scientific-method/executable-reader 4.3.0; FND-PARTY-322's alias_far_words.py report
environment: null
---

## Observation

FND-PARTY-322 retained one nearby-word candidate for Q-PARTY-136's
ordinary shutdown root. Installed DSUN.EXE is 634416 bytes,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Offsets are hex.

The candidate combines the declared segment word at file 47BEA
with the word eight bytes earlier at 47BE2. The raw latter word
is 428C. Applying the segment word's relocation gives 528F, so
the candidate search prints 528F:428C, numerically equal to
shutdown trampoline 56B2:005C. That search is a nearby raw-word
candidate report, not proof the two words are consumed as a pointer.

The existing operand reporter independently resolves both locations:

| File word | Raw value | Declared relocation | Loaded word at segment 1000 |
|---|---|---|---|
| 47BE2 | 428C | MZ relocation | 528C |
| 47BEA | 428F | MZ relocation | 528F |

Thus the proposed offset word is not an unchanged 428C after
loading. If the two unchanged loaded words were consumed as the
proposed offset/segment pair, they would form 528F:528C, linear
57B7C. The shutdown entry 56B2:005C is linear 56B7C. They
differ by 1000 bytes. There is no word wrap in either calculation.

This rules out the specific unchanged loaded pair as an incoming
shutdown pointer. It does not require an assumption about whether
some consumer actually combines the separated words: even that
combination fails to name the target under the declared relocation.
No instruction identity is inferred from the exploratory disassembly
around this data. The two explicit successful relocation resolutions
provide positive mapping evidence, rather than an empty reference search.

## Interpretation

Q-PARTY-136 can remove this named unchanged-pair candidate from
its unresolved shutdown admission. A later rewrite, arithmetic conversion,
different pairing or a different incoming mechanism remains part of the
existing changed/computed/indirect-transfer scope; none is established
by this raw-word coincidence. The known exact-one exit route from
FND-PARTY-322 and reentrant exit-reader conditions remain unchanged.

This removes one bounded candidate. It closes no whole queue item,
establishes no complete retained-number contract and declares no complete
reading. FMT-PARTY-007 remains supported.

## Alternatives

- The raw nearby word survives loading as offset 428C: it has its
  own declared MZ relocation and becomes 528C.
- The reported numerical alias proves a loaded pointer to shutdown:
  the loaded pair differs from that target by 1000 bytes.
- Excluding this pair proves all alternate shutdown entries absent:
  computed, modified, indirect and bypass mechanisms remain outside
  this specific exclusion.

## How to reproduce

Reuse FND-PARTY-322's retained getter-shutdown-root-pointer-controls.txt.
Run `node tools/evidence/report.mjs operand <config>` twice, with
configs naming the installed source, its XXH3-128, loadSegment 4096
and respective numeric site obtained from hex 47BE2 and 47BEA.
Omit targetOffset, whose default zero resolves each stored segment
independently. Require `relocated: true` and `kind: MZ relocation`
for both before using their loaded segment words in the pair calculation.

Configs shutdown-alias-word-47be2.json and shutdown-alias-word-47bea.json
and their corresponding -report.json outputs remain under
GAME_DIR/analysis/handler-census-20261106/. No original game,
emulator session or emulated call supplied evidence here.
