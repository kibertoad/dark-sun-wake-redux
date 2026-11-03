---
id: FND-SCRIPT-032
title: Six immediate-offset candidates resolve to callbacks outside the GPLI conversions
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
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0361..3A8E:0366
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007BAD1..0x0007BAE7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A7F4..0x0008A804
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C1E7..0x0008C1F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C260..0x0008C271
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00096227..0x00096237
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00098953..0x00098963
tool: far_pointer_words.py at 700bce0a; overlay_listing.py with FBOV fixups; tools/evidence/report.mjs operand; locked evidence Python and executable-reader 4.3.0
environment: null
---

## Observation

Q-SCRIPT-020 asks about incoming routes outside the known save calls.
FND-SCRIPT-030 answers adjacent relocated pairs, leaving split construction
among the other representations. This reading qualifies the six immediate
offset candidates in one diagnostic query; it is not a complete split-target
or incoming-transfer census.

The queried conversion selectors are 56EF:00A7 and 56EF:00AC, whose canonical
entries are file 0x71445 and 0x71659 respectively (FND-EXE-596). The corrected
typed-immediate query also includes 38FF:05B5 as the retained comparison target.
The repeated reports have 93 lines including their three headings and are
byte-identical. Among the conversion offset matches, six words qualify as
MOV/PUSH immediate candidates; all six are word PUSH operands.

Each has a preceding segment PUSH whose immediate is an actual FBOV fixup.
Resolving that segment operand with the corresponding offset gives:

| Offset-word file site | Segment-operand file site | Resolved selector | Canonical entry file offset |
|---|---|---|---|
| 0x8A7F8 | 0x8A7F5 | 571F:00A7 | 0x7B95C |
| 0x8C26C | 0x8C269 | 5787:00A7 | 0x8DE45 |
| 0x9622B | 0x96228 | 571F:00A7 | 0x7B95C |
| 0x98957 | 0x98954 | 571F:00A7 | 0x7B95C |
| 0x7BAD5 | 0x7BAD2 | 5713:00AC | 0x749B0 |
| 0x8C1F3 | 0x8C1F0 | 5787:00AC | 0x8D781 |

None is either conversion entry or a relocated alias of one. The selectors
agree with the already recorded callback identities in FND-UI-041,
FND-PARTY-044, FND-PARTY-217 and FND-EXE-626. The three 571F:00A7 sites
and the 5713:00AC site pass their pairs to 3A8E:0D3C. The two overlay-204
sites branch to the common request-dispatch continuation; FND-EXE-626 already
identifies them as request 40 and request 18 callback arguments. This finding
does not repeat or extend those callback behavior claims.

The previous rendered-text classifier also labeled file word 0x2FE44 as an
immediate. Its enclosing decoded instruction at 3A8E:0361 writes AX to an
ES-relative memory location with displacement 0xAC. That word is the memory
displacement, not an encoded immediate. The corrected report labels it
`no segment`. This is the only changed line against the retained preparation
report. Its removal does not establish any negative about other runtime targets.

## Interpretation

These six source-declared split literal pairs and the displaced memory word
do not supply additional constructions of either conversion selector.
The remaining incoming-route obligation is Q-SCRIPT-020. This removes finite
candidate leads, without asserting completeness of the diagnostic search.

## Alternatives

- An offset match alone does not identify a far target. The six source-declared
  segment operands above resolve elsewhere, ruling out these particular split
  literal pairs as direct constructions of the conversion selectors.
- Nearby relocation words alone would be ambiguous. Here the local listings
  show consecutive segment/offset pushes; the reader independently resolves
  each exact segment operand rather than selecting an arbitrary nearby word.
- The helper decodes from a six-byte backward window and returns its first
  covering instruction candidate. It does not prove that every possible literal
  construction is found, establish all instruction starts or follow computed,
  copied, aliased, unrelocated or runtime-modified values.
- No runtime callback admission, absence of other incoming routes, complete
  reading, native outcome or spec-status promotion follows. Q-SCRIPT-020
  retains those obligations. No original game was run.

## How to reproduce

Use DSUN.EXE, size 634416 and XXH3 e296af55ba2ecde7e77f555c90f33d0b.
Run the locked evidence Python with `-I`, far_pointer_words.py at 700bce0a,
the executable path and selectors `56EF:00A7 56EF:00AC 38FF:05B5`.
Repeat and compare bytes. The accepted report SHA-256 is
cab73eadcb3e4fb6c4074a0837b46c70d28c7aacf8ed6dae96c3702ccecc3705.
The tooling's synthetic tests independently distinguish displacement and
immediate fields, including equal values and field boundaries.

For each table row run `node tools/evidence/report.mjs operand CONFIG` with
source, the above xxh3, loadSegment 4096, site equal to its segment-operand
file offset and targetOffset 0xA7 or 0xAC as appropriate. Require relocated
true, FBOV fixup, and the listed loaded/canonical targets. Compare against
FND-EXE-596's independently known conversion targets.

Use overlay_listing.py on these descriptor/file ranges to retain context:
190 / 0x7BA12..0x7BE16; 202 / 0x8A727..0x8A9E7;
204 / 0x8BF29..0x8C487; 211 / 0x960C5..0x9654D;
212 / 0x9881C..0x98AE3. These are entry-based context readings, not a new
whole-program instruction census. For the displacement use resident_listing.py
on 3A8E:02B3..3A8E:05EF and inspect the instruction at 0361.

Reports remain in GAME_DIR/analysis/conversion-incoming-20261103/:
offset-word-typed.txt and its repeat, split-<segment-site>.json and
their reports, and overlay-<descriptor>-literal-context.txt.
resident-displacement-context.txt supplies the resident instruction context.
The earlier
offset-word-preparation.txt is diagnostic only. No original bytes or
instruction listings are committed.
