---
id: FND-SOUND-073
title: Tested encoded incoming forms do not bind the music-region gap candidate or provisional lookup reader
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:40D2..1BF3:40E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:4889..1BF3:4C09
tool: inventory_boundaries.py, immediate_search.py and far_pointer_words.py at 021f38f6; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

FND-SOUND-072 leaves the raw region candidate at 1BF3:488B and the
provisional reader at 490D unresolved. This bounded incoming census uses
the same installed DSUN.EXE, 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the current DSUN inventory.

The resident inventory-range near-transfer pattern query finds zero
patterns targeting either offset in segment 1BF3. Its positive control
finds the short jump from 40D2 to 40E4 in the established 3C90 stream
(FND-EXE-593). This covers the script's rel8/rel16 call/jump forms in
resident rows whose recorded segment is 1BF3. It does not cover omitted
ranges, other paragraph representations, indirect transfers or changed code.

The whole-file exact-word classifier finds only one word equal to 490D:
file 0x900FB, in overlay 206's fixup list, with no segment binding. The
only word equal to 488B is at file 0x393E2, in the load image with no
relocated segment following it. Its diagnostic neighboring decoding is
not an established instruction. Neither occurrence supplies a far pointer
or direct transfer to the queried target. The 4723 control finds the known
resident far-call operand at file 0x2B715 and overlay call operands,
including overlay 182 +13A5 and +13B0. Their operand classifications,
not the script's overlapping diagnostic instruction text, are the control.

The immediate/displacement search for 490D and 4889 returns no 490D
instruction candidate and one candidate naming 4889: 209E:000E, alias
1BF3:4ABE, the provisional byte lookup already retained by FND-SOUND-072.
This adds no established reader and is not a computed-target census.

## Interpretation

The tested encoded near-entry and exact-word binding components are now
answered. The ownership question survives: actual computed/aliased entry,
construction of a target, or a broader established owner could still bind
the gap. Q-SOUND-012 owns that finite remaining question separately from
Q-SOUND-004's music selection and playback questions. This is a split,
not an answered queue deletion or a complete reading.

## Alternatives

A direct call or adjacent relocated pointer using exactly 490D or 488B
is unsupported by these queries. Paragraph aliases, runtime-built or
nonadjacent pointers, entries elsewhere in the gap and modified code remain
outside that exclusion. The lookup-data interpretation and a separately
entered executable interpretation from FND-SOUND-072 remain open.

## How to reproduce

Use tools/research/exec-census/ at 021f38f6 with the installed DSUN.EXE.
Retain outputs in GAME_DIR/analysis/handler-census-20261106/:

- inventory_boundaries.py with coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv and
  targets 1BF3:490D, 1BF3:488B, 1BF3:40E4 gives
  music-region-gap-near-patterns.txt. Preserve the control and range limits.
- far_pointer_words.py targets 1BF3:490D, 1BF3:488B, 1BF3:4723 gives
  music-region-gap-exact-words.txt.
- immediate_search.py values 490D and 4889 gives
  music-region-gap-offset-candidates.txt.

No game execution, emulated call or new inventory ownership is inferred.
