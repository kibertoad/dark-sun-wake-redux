---
id: FND-ITEM-069
title: Controlled direct-operand searches find the equipment flag's known stores and duration read without another byte-word-doubleword overlap
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065112..0x00065117
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077CB1..0x00077CB6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077CD2..0x00077CD7
tool: tools/research/exec-census/immediate_search.py and operand_range.py at 91d4600f; existing entry-based overlay_listing.py controls; Capstone module 5.0.7, distribution 5.0.9
environment: null
---

## Observation

Q-PARTY-118 requires controlled flag-writer/consumer coverage before using
the equipment caller's store as a use-time value. FND-ITEM-059 independently
identifies its two stores and FND-MAGIC-018 the hit routine's duration read.
This searches direct encoded references, checks those controls and separates
numeric/overlapping decode candidates from named flag accesses. It does
not close computed, aliased, bulk or startup initialization writers.

### Search domain and controls

The source is installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The raw candidate query decodes from
every byte of its MZ load image and every flags-three overlay's code span,
using source-defined mappings. The independent anchored query decodes the
declared inventory ranges from their starts, retaining that restart-after-
failure and linear ownership are not complete control-flow evidence.

The inventory has 3230 rows. Its SHA-256 after removing a BOM and normalizing
line endings to LF is
69555e580c3964e90ec43822629c29e3b909ecc264353b4dec0ccc82bdbf76ea.
The private controls record source, inventory, decoder and script identities,
the searched domains and explicit exclusions. Reuse requires those inputs
to match, not merely the existence of a report.

Both exact-6174 queries recover the three independent byte-access controls:

| Established entry/site | Direct access |
| --- | --- |
| 179 +0D2F, site +0E32 | Compare byte at default-DS offset 6174 with zero |
| 189 +326A, site +3301 | Store byte one at default-DS offset 6174 |
| 189 +326A, site +3322 | Store byte zero at default-DS offset 6174 |

FND-ITEM-059's full entry reading validates both stores. A new listing from
179 +0D2F through its duration branch agrees with FND-MAGIC-018's read.
Default DS must still be admitted at each site before identifying its
effective address with the shared flag. No use-time value is inferred.

### Complete classification within the queried forms

The exact raw query yields six decodes. Three are the controls above;
three use 6174 as an arithmetic immediate, not as the flag displacement.
Those raw numeric matches are not independent field references or proof
that their instruction starts execute.

To cover direct byte, word and doubleword operands whose starting
displacement could overlap the flag byte, the second query uses starts
6171 through 6174 inclusive. Its nine raw decodes comprise the same
three byte controls, four arithmetic-immediate matches and two decodes
of a word store at displacement 6171 (one begins inside its segment prefix).
A word beginning at 6171 ends before the flag; it is not an overlap.

The anchored range query has six records: the three controls, that
segment-overridden word store and two resolved branch-target immediates.
The branch targets' low words are numeric query matches, not memory
operands. There is no additional direct byte/word/doubleword flag overlap
among the raw or anchored results. The explicit interval matters: a
doubleword beginning at 6171 could reach 6174, but no such candidate
was found in this queried code domain.

This result covers these encoded operand forms, not all effective-address
constructions. It excludes pointer arithmetic/aliases, implicit string
operations, bulk or wider accesses beginning before the queried interval,
changed/restored/self-modified code and code outside the declared load-image
and overlay spans. It neither proves that startup clears the flag nor rules
out a later bulk restoration of it. Raw all-offset decoding also cannot
establish an alternate entry merely because its bytes decode.

## Interpretation

Q-PARTY-118 no longer needs an additional undecided direct byte/word/
doubleword site from this controlled source domain. The known ordinary
consumer remains the hit routine's duration gate; FND-MAGIC-018/167 give
its local consequence. Computed/aliased and bulk/wider producers, initial
state, actual DS and frame identity, callbacks/reentrancy and preservation
to each immediate or queued effect remain explicit obligations. No native
flag value, complete writer census, confidence or parity promotion follows.

## Alternatives

Counting arithmetic or branch immediates as field references would confuse
numbers with effective addresses. A word store at 6171 does not overlap
6174. Conversely, three matched direct controls cannot prove that no
computed or bulk write changes the flag. Entry-based confirmation of the
three sites is distinct from raw-decoder candidate coverage.

## How to reproduce

With the project evidence Python, run immediate_search.py against the
approved DSUN.EXE with 6174, then with 6171 6172 6173 6174. Run
operand_range.py with the same executable and current
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv, first 6174 6175 and then 6171 6175
(upper bounds exclusive). Require all three independently recorded controls.
Check operand kinds and widths rather than counting numerical matches.

Reuse FND-ITEM-059's placement-equipment-hooks.txt for the store entries.
Run overlay_listing.py for descriptor 179, 6500F..65140 hex, preserving
the duration branch. The complete entry-based prefix is retained as
equipment-flag-hit-prefix.txt under GAME_DIR/analysis/item-removal-20261104/.
The report is a prefix, not a complete hit-routine reading.

The same private directory retains equipment-flag-candidates.txt,
equipment-flag-inventory-operands.txt, equipment-flag-overlap-candidates.txt,
equipment-flag-overlap-operands.txt and equipment-flag-census-controls.json.
The exact raw/anchored report SHA-256 values are respectively
22c7b2ab1165c356ea8d673a5d604a5c6d80dcf06bb73d8bc94b7bacde326e3a and
5931b3e1c7b7751f12be724a27c54b9454978a85cafd5091e5cd4c6fd68f1b62;
the controls also retain both overlap-report digests. No original code,
payload or runtime state is committed or executed.
