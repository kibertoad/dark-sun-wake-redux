---
id: FND-PARTY-275
title: Wrapped-word writer census isolates twenty bounded error stores and no fixed operand for the negative-38 alias
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0301..3A8E:0307
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0025..39D1:002B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0163..39D1:0169
tool: tools/research/exec-census/window_operands.py at 1b3eef97; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; FND-PARTY-274 and FND-COMBAT-093 reuse
environment: null
---

## Observation

Q-PARTY-124 retains actual writers to the wrapped selected words in
FND-PARTY-274. Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. The inventory is
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv at 1b3eef97, LF-normalized
SHA-256 `69555e580c3964e90ec43822629c29e3b909ecc264353b4dec0ccc82bdbf76ea`.

The existing window_operands reporter searches encoded displacements
and literal address candidates in 97FB..A110. Its inventory pass gives
940 operands; its every-other-byte pass gives 1149 candidates, kept
separate from established instruction starts. String-operation counts
are diagnostic only and do not determine effective destinations.

Intersect fixed DS destinations, without a base or index register, with
the selected words starting at `97FE + 18*k`, k=0..128. Include partial
overlaps and wider scalar stores, rather than requiring an exact start.
The three-byte lower margin covers ordinary byte/word/doubleword stores
starting before the first selected word. Larger accesses remain outside
that margin's coverage claim.

There are 63 inventory-decoded, access-flagged overlapping writers at
15 selected words. Thirty-four additional candidates overlap seven
selected words; every one is marked inside an inventory instruction.
They are not added to the inventory result or proved unreachable.
Checking the inventory's other fixed-DS first-memory operands, including
read/address labels, finds no extra destination instruction class beyond
the reported writers and the read-only comparison/test/push forms.

| Selected word | Inventory writer matches |
|---|---|
| 9B04 | 1 |
| 9B70 | 2 |
| 9BDC | 5 |
| 9BEE | 3 |
| 9C00 | 2 |
| 9C12 | 1 |
| 9D7A | 3 |
| 9D8C | 1 |
| 9D9E | 9 |
| 9DB0 | 20 |
| 9E76 | 1 |
| A038 | 4 |
| A04A | 1 |
| A0EC | 2 |
| A0FE | 8 |

These counts classify the retained decodes and operand forms, not
whole-program execution or effective alias coverage. No index-only,
register-only, copied-pointer or block-operation absence follows.

### Error-word stores

The selected word 9DB0 corresponds to logical row index -48. All twenty
inventory writers overlapping it are full-word literal MOV stores at
exactly 9DB0. Their complete value set is decimal
`0, 9, 16, 19, 21, 22, 26, 27`. None writes positive 1001 or merges
unknown old bits into that result. The window-registration failure store
at 3A8E:0301 writes 26, independently recorded in FND-COMBAT-093;
the census recovers that instruction as a positive memory-write control.
The initializer's 39D1:0025 writes zero and 0163 writes 27.

Consequently these twenty decoded stores cannot manufacture 1001 in
that selected word. They do not show the word is always one of those
eight values: an unsearched write or alias may change it between stores.

### Negative-38 alias

FND-PARTY-274 maps -38 to resident word 9E64. The full operand report
has no encoded operand at 9E61..9E65, including its raw candidate pass.
Thus there is no literal pointer at either selected byte and no direct
fixed byte/word/doubleword operand overlapping the word to qualify in
this census. This conclusion uses all operand kinds, not merely a
negative access-flag classification.

The result does not exclude a pointer formed from a different base,
an indexed access whose effective address reaches 9E64, or a larger
copy through the surrounding resident state. Neighboring actual writers
exist: 39D1:0043 stores the archive handle at 9E70, and 0154 writes the
pointer segment at 9E76. Their nominal ranges do not overlap 9E64.
The absence of a literal 9E64 operand therefore does not make its
initial zero a preserved runtime value.

## Interpretation

Remove the twenty literal error-word assignments as possible producers
of 1001, and remove an unknown literal/direct scalar operand at the
negative-38 alias in the tested domain. The other 43 inventory writer
matches, 34 inside-instruction candidates, larger/block/indexed writes,
copied pointers and segment aliases remain distinct mechanisms to audit.
The source and state questions now have actual word and instruction
candidates rather than a reusable unspecified-writers caveat.

No whole getter site closes: the 83 other direct sites and repeated-hook
preservation remain. FMT-PARTY-007 stays supported; no complete reading
or answered queue deletion follows.

## Alternatives

- An inside-instruction candidate is not an established store; it also
  cannot be dismissed as impossible without an incoming-entry analysis.
- A bounded set of literal full-word assignments says what those stores
  write, not that no other mechanism changes the same physical bytes.
- Empty direct operands at 9E64 do not exclude writes from a rebased or
  copied address, block operation or different segment alias.

## How to reproduce

Run window_operands.py with the fingerprinted installed DSUN.EXE,
the stated inventory and `97FB..A110`. Retain both passes, all operand
forms and the source identity. Select fixed DS destinations with no base
or index register, intersect their byte intervals with each selected
two-byte interval, and keep inventory and inside/outside/listed labels
separate. Do not use total report counts as writer counts.

Check every 9DB0 destination and its full width/literal. Compare the
3A8E:0301 control with FND-COMBAT-093's retained registration reading.
Inspect all operand kinds in 9E61..9E65 before claiming no direct scalar
overlap at 9E64. Source reports stay in
GAME_DIR/analysis/class-table-closure-20261106/ as
wrapped-alias-operands.txt and wrapped-alias-fixed-writer-candidates.json.
The JSON contains raw candidates too; apply their classifications before
interpreting counts. No original game ran.
