---
id: FND-PARTY-276
title: Sixteen more overlapping constant stores exclude 1001 and sixteen raw candidates are operand-size-prefix interiors
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0036..38FF:0045
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:034D..38FF:0356
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0053..39D1:009C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:00E8..39D1:00F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:02FF..39D1:0308
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0819..39D1:0822
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0B09..39D1:0B12
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0BDF..3A8E:0BE8
tool: retained FND-PARTY-275 window_operands.py census and tools/evidence/report.mjs table at 39f0c577; Python 3.12.10, Capstone wheel 5.0.9, xxhash 4.0.1; FND-CONFIG-033/036 reuse
environment: null
---

## Observation

Q-PARTY-124 retains 43 inventory writer matches and 34 raw candidates
after FND-PARTY-275's error-word exclusions. The unchanged source is
installed DSUN.EXE, 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Reuse that finding's exact selected
word intervals, inventory identity, controls and decoder exclusions.

### Additional constant stores

Positive word 1001 is low byte E9, high byte 03. A store that forces
either byte to a different value cannot itself leave that word as 1001,
even when it starts before or after the selected word. No prior value
of the other byte is needed for these local exclusions.

| Selected word | Decoded stores | Deciding written byte(s) |
|---|---|---|
| 9D9E | 38FF:0036 and 034D clear the doubleword at 9D9F; 003F clears the word at 9D9D. | The first two force the selected high byte to zero; the third forces its low byte to zero. |
| A038 | 39D1:0053 clears the word at A037; 007A, 0088 and 0096 write bounds 027F, 013F and 0289 at A039. | The first forces the low byte to zero. The other three force the high byte to 7F, 3F or 89, respectively. |
| A04A | 39D1:00E8 clears the doubleword at A049. | Both selected bytes become zero. |
| A0FE | Doubleword clears at 39D1:02FF, 0819 and 0B09, and 3A8E:0BDF, all targeting A0FD. | Both selected bytes become zero. |
| 9B70 | overlay 202 +09D2 clears the word at 9B71. | The selected high byte becomes zero. |
| 9BDC | overlay 208 +1689 writes word FFFF at 9BDC. | Both bytes differ from 1001. |
| 9D7A | overlay 211 +0600 and +0AFD clear the word at 9D79. | The selected low byte becomes zero. |

There are sixteen stores in this table. The bound assignments agree
with the independently read display modes in FND-CONFIG-033/036.
These are exclusions of the stores' immediate results, not proof that
intervening aliases cannot write another value later. The counter updates
near these clears and the register-sourced stores are not folded into
the constant exclusions.

Together with the twenty error stores in FND-PARTY-275, 36 of the 63
inventory writer matches cannot manufacture positive 1001. Twenty-seven
inventory matches retain their actual value/retention obligations.

### Operand-size-prefix candidate classification

Sixteen of the 34 raw candidates have all these properties:

- The candidate starts exactly one physical byte after an inventory
  doubleword store, after normalizing the resident segment spelling.
- The effective fixed destination is the same as its parent instruction.
- The raw candidate decodes a word store where its parent decodes a
  doubleword store.
- A source-guarded byte query finds 66 hex at every parent start.

Thus those sixteen starts are immediately after an operand-size prefix,
not sixteen additional independent fixed-address store instructions in
the inventory stream. The query reads exactly those sixteen prefix bytes;
it does not infer prefixes merely from equal candidate counts.

For example, the raw start 37F8:0001 at file 2D181 is one byte after
37ED:00B0 at 2D180, with the same 9D9F destination. Both segment
spellings identify adjacent physical bytes. Dropping the leading 66
changes the store width, so it is not a claim of identical full effects.

Possible incoming entries at these prefixless starts remain a classified
bypass mechanism. They cannot be dismissed just because a different
entry decodes the prefix. Eighteen other raw candidates remain
unclassified: fifteen select 9A50, two select 98E8 and one selects 9A08.
They retain their inside-instruction labels and need their own enclosing
instruction and incoming-entry assessment.

## Interpretation

Remove the sixteen constant-store results as possible 1001 producers.
Remove sixteen unidentified raw instruction explanations by recording
their exact prefix/parent relationship, while retaining incoming bypass
entry as a distinct unresolved mechanism. The next fixed-writer audit
has 27 inventory value matches and 18 unclassified raw candidates,
plus the sixteen classified prefixless-entry alternatives.

Larger/block/indexed/copied and segment-alias writes remain outside
this fixed-store classification. No whole getter site closes: the 83
other direct sites and repeated-hook preservation remain. FMT-PARTY-007
stays supported; no complete reading or answered queue deletion follows.

## Alternatives

- A partial store can rule out 1001 at that moment without proving
  anything about the selected word before the store or after another write.
- A prefix-interior decode is not a new inventory instruction, but an
  independently admitted target there would execute at the narrower width.
- Grouping the remaining writers by address does not establish their
  register values, count bounds, pointer lifetime or effective aliases.

## How to reproduce

Reuse FND-PARTY-275's wrapped-alias operand and fixed-writer reports.
Exclude raw inside/outside/listed candidates before counting inventory
matches. Inspect the sixteen literal stores above and intersect each
written interval with the selected word; require a forced byte different
from E9/03 rather than assuming a whole-word overwrite.

For every raw resident word-store candidate, normalize the file position
as `5200 + 16*(segment-1000) + offset`, all values hexadecimal.
Pair it with an inventory doubleword store one byte earlier and require
the same fixed destination. Use report.mjs table with the source hash to
read one byte at each of those sixteen parent starts; every value is 66.
The retained query uses one bounded row with sixteen named byte fields
at their respective relative positions, rather than reading unrelated data.

Local reports under GAME_DIR/analysis/class-table-closure-20261106/ are
wrapped-alias-prefix-pairs.json, wrapped-alias-prefix-byte-query.json,
wrapped-alias-prefix-bytes.json and
wrapped-alias-bounded-overlap-stores.json. The source report remains
the unchanged FND-PARTY-275 census. No original game ran.
