---
id: FND-SOUND-044
title: Setup error normalization maps its bounded table to nonnegative results and preserves the negative-input bypass
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:04CE..1000:0507
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D4C8..0x0001D521
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table at 1517a20a; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0
environment: null
---

## Observation

Q-CONFIG-011 retains error-map values and consumers after FND-SOUND-031's
indexing reading. FND-EXE-491 maps DS offset o to file offset F760+o.
The helper's table at DS:DD68 therefore starts at file 1D4C8. Its
nonnegative input branch admits indices zero through hexadecimal 58,
inclusive: 89 one-byte rows. A controlled numeric table read covers
exactly that interval, with a separate repeated read. All reported bytes
are below 80 hexadecimal, so their signed extension is nonnegative.

### Normalized result categories

All numbers in this table are decimal. Each input is the selected table
index, not an interpretation of an operating-system error name. Ranges
include both ends. Every index zero through 88 appears once.

| Result stored in the normalized-error word | Selected indices |
| --- | --- |
| 0 | 0 |
| 2 | 2, 3, 18, 53, 63, 64, 67 |
| 4 | 4 |
| 5 | 5, 12, 16, 32, 33, 65 |
| 6 | 6 |
| 8 | 7–9 |
| 14 | 14, 37–49, 73–79, 81 |
| 15 | 50, 55, 66 |
| 17 | 17 |
| 19 | 1, 13, 60, 71, 87 |
| 20 | 10 |
| 21 | 11 |
| 22 | 15 |
| 23 | 35, 36, 68, 84 |
| 27 | 61 |
| 28 | 62, 82 |
| 30 | 19 |
| 35 | 52, 80, 85 |
| 37 | 86 |
| 40 | 22–26, 29–31, 57–59, 69, 83, 88 |
| 41 | 20, 27, 34 |
| 42 | 56, 70, 72 |
| 44 | 21, 28, 51, 54 |

For a nonnegative signed input above 88, the helper selects index 87,
stores 87 in DS:DD66 and stores normalized result 19 in DS:007F. Inputs
zero through 88 store their own selected index at DD66. In particular
the lower read/write rejection code 5 and handle-bound rejection code 6
retain their numeric values in the normalized word.

For negative inputs it negates the word and compares the result signed
against 48. A result greater than 48 selects the same index-87 fallback.
Otherwise it stores FFFF at DD66 and the negated word directly at 007F,
without table access. Thus -1 through -48 produce 1 through 48;
-49 through -32767 use the fallback. Input word 8000 remains 8000
after negation and takes the direct branch because the comparison is
signed. Those word values are hexadecimal where explicitly written.

Every branch returns AX=FFFF. The complete return at 1000:0504 removes
two argument bytes; the bounded read extends through 0507 to include
that instruction. Neither the mapping nor the direct branch reads a
later error consumer. The table's file-image values require preservation
against other writers/aliases for a native-state claim.

## Interpretation

This removes the initial error-map values, reachable-table extent and
normalization categories from Q-CONFIG-011. It binds numeric results of
the already read I/O wrappers without guessing names from a compiler
runtime or a modern error-code convention. Later consumers, other writers,
interrupt-return values and runtime preservation remain open.

## Alternatives

Assuming inputs above 88 map to the final row is wrong: the fallback uses
87, whose normalized value differs from index 88. Assuming every negative
input uses the table misses the direct bypass and wrapped 8000 case.
No native error was induced, no full consumer census performed and no
complete-reading or parity status promoted.

## How to reproduce

Use approved SOUND_DS.EXE, length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Run resident_listing.py with -I and
1000:04CE..1000:0507; run report.mjs table with source identity and
the exact layout below.
The table configuration uses start 0x1D4C8 (decimal 120008), count 89,
stride one, and one width-one field at offset zero. Count evidence is
the nonnegative 0..58 admission and 57 fallback; controls remain local.

Reports under GAME_DIR/analysis/disc-adv-survey-20261104/ are
setup-error-index.txt and its -repeat pair, both SHA-256
6d6e94b7735c37691a42e3a5d44d68c9d75ad33cdcab117c28253283dbd1a1eb;
setup-error-map.json and its -repeat pair both have SHA-256
77edec23159062a1af88dd511866cc38a4fac63339578dfc0dd3487dab382eb2.
The local setup-error-map-controls.json records the bounded reader input.
Keep original-derived numeric reports and listings only in that store.
