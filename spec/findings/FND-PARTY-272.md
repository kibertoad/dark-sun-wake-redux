---
id: FND-PARTY-272
title: Two DATA-name getter sites exclude 1001 by local bounds while three word-input helpers admit it locally
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062038..0x00062067
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062AA4..0x00062AC5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080171..0x000801AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080278..0x00080295
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092082..0x0009209B
tool: tools/research/exec-census/overlay_listing.py at 2fbce3b7; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; retained FND-PARTY-265 exact caller-set comparison, FND-PARTY-157, FND-EXE-641 and FND-ITEM-057
environment: null
---

## Observation

This narrows Q-PARTY-124's remaining direct calls of the shared DATA
getter 2C5F:0003. Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. The retained caller/argument reports
from FND-PARTY-265 contain all five sites below; the raw and Ghidra site
sets agree exactly. No new literal-1001 search is used.

### Two locally bounded requests

| Getter call | Deciding local input path | Passed selector domain |
|---|---|---|
| overlay 176 +0B15, file 62055 | Load input word into AX. Signed inputs 0..33 receive 235; other inputs retain AX. Both routes then require signed AX at least 235 and below 269. | 235..268 |
| overlay 177 +09B4, file 62AB4 | Load input word into AX; negative values and signed values at least 235 take the null return. | 0..234 |

Neither path calls a child or rewrites AX between its deciding comparisons
and the pushed argument. Thus positive 1001 cannot be passed at either
getter site through the ordinary entry path. These bounds do not depend
on the earlier caller's intended power/spell domain or on unchanged
canonical resource bytes. Independent entries after the tests and invalid
instruction/frame state are outside this entry-based conclusion.

The power helper's complete input predicate corrects FND-EXE-611:
input 235, for example, skips the addition but passes both final tests.
FND-EXE-641 supersedes that finding while retaining its other routines.
Both accepted input domains still request only DATA 235..268.
FND-PARTY-157's spell-name bound agrees with the second site's tests.

### Three locally admitted word selectors

Three other small client prefixes have a different deciding condition:

| Entry and getter site | Local selector path before the getter |
|---|---|
| overlay 193 +1E81, call +1EA0, file 80190 | Input word BP+6 is loaded into AX and rejected only when FFFF. A separate byte BP+8 is raised to one when below one; this does not change AX. |
| overlay 193 +1F88, call +1F9F, file 8028F | Input word BP+6 is loaded into AX and rejected only when FFFF. Another argument is loaded into SI, not AX. |
| overlay 208 +07F2, call +0805, file 92095 | Input word BP+6 is loaded into AX and rejected only when FFFF. |

Each pushes that same AX with no intervening call. A formal input word
1001 therefore reaches the getter locally in each of these prefixes.
No read of a byte or bound below 319 in these prefixes can be substituted
for the actual word argument. This is a feasible local argument path,
not evidence that a real game caller supplies 1001. Incoming direct and
indirect argument producers remain necessary for that question.

FND-ITEM-057 reads the getter's sign-extended request on a cache miss.
With input positive 1001 and a different cache key, its local request is
DATA 1001; a matching cache key returns the fixed buffer without a new
resource request. Actual key/buffer identity, transfer and later content
interpretation remain separate from these caller-selector bounds.

## Interpretation

Remove the two name-helper sites from the unknown-positive-1001 direct
number domains. Of the 87 direct sites, three now exclude positive 1001
locally: these two and FND-PARTY-265's first equipment-hook request.
The repeated equipment-hook call remains its separate preservation
obligation, leaving 83 other direct sites. The three word-input sites
above belong to that 83 and require their incoming argument bindings;
their own non-FFFF tests do not supply the missing exclusion.

This is a request-number result, not proof that the shared getter buffer
always holds its requested DATA bytes. Indirect/bypass clients and other
resource routes remain Q-PARTY-124. FMT-PARTY-007 stays supported; no
complete reading or answered queue deletion follows.

## Alternatives

- Nonliteral argument syntax does not imply an unbounded value: the two
  name helpers decide their complete local request ranges before pushing.
- A familiar spell/item name does not bound the other helpers' word
  arguments. A formal 1001 path is locally admitted, but actual incoming
  callers may constrain it; neither universal use nor universal exclusion
  follows from these prefixes alone.
- Cache hits, content/key mismatch and failed transfers are not settled
  by excluding a number at two request sites.

## How to reproduce

Reuse FND-PARTY-265's data-getter-direct-callers.txt,
getter-arguments-console.txt and getter-call-set-comparison.json under
GAME_DIR/analysis/class-table-closure-20261106/. Match shipped file
offsets, not just counts or mapped-segment ordering.

Run overlay_listing.py for descriptor 176 at `62038..62067`,
177 at `62AA4..62AC5`, 193 at `80171..801AD` and `80278..80295`,
and 208 at `92082..9209B`. Bounds are hexadecimal and half-open.
Track signed comparisons, unchanged AX and the width of each input.
Retained reports in the same directory have getter-power-name,
getter-spell-name, getter-byte-helper193a, getter-byte-helper193b
and getter-byte-helper208 stems. The last three exploratory filenames
do not classify the input width: their actual selectors are words.
No original game ran.
