---
id: FND-SCRIPT-053
title: Trigger allocation flag references use a later descriptor alias and its check is not a general nonzero-failure test
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
    offset: 0x0004B080..0x0004B7A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057AD2..0x00057B94
tool: report.mjs table and corrected segment_references.py, segment_offset_refs.py, segment_displacement_writes.py at ad01b3e7; Python 3.12.10, Capstone distribution 5.0.9; FND-CONFIG-156 interface reuse
environment: null
---

## Observation

This examines the allocation-failure byte's concrete source bindings for
Q-SCRIPT-019. It reuses FND-CONFIG-156's allocator/checker contract, rather
than rereading the initialization chain as an assumed success path.
Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b.

### The first descriptor is not the only descriptor for a segment

A fingerprinted read of all 229 eight-byte segment-table rows agrees
with FND-EXE-571's table origin and count. Among resident flags-zero/one
rows, these eight loaded paragraph bases have more than one descriptor:

| Loaded segment | Descriptor indices |
| --- | --- |
| 15F3 | 2, 3 |
| 2707 | 15, 16 |
| 2D40 | 24, 25 |
| 3143 | 26, 27 |
| 4464 | 59, 60 |
| 4C0B | 86, 87 |
| 4C0C | 88, 89 |
| 55CE | 160, 161, 162 |

Both descriptors88 and89 have raw paragraph3C0C and flags zero.
Their low offsets are zero; their high offsets are zero and one,
respectively. Descriptor88 therefore contributes an empty span, while
descriptor89 names the one-byte span at loaded 4C0C:0000. They are not
two independent failure flags. Their overlay descriptor words are 02C0
and 02C8, respectively; both resolve to the same loaded segment base.

The older first-descriptor writer query for 4C0C:0000 searched 02C0
and returned no candidate, missing the independently known allocator
store. That report failed its positive control and supports no absence
claim. The corresponding first-descriptor segment query also missed
the code's 02C8 references. The separate tooling commit ad01b3e7 fixes
all three affected reference/context/writer reporters to retain aliases.

### Repaired controls recover the actual producer and checks

The corrected displacement-writer query recovers the byte-one store at
overlay169 +058C, file57B0C. Entry-based reading from +0552 confirms
the fixed segment word02C8 at +0588, ES assignment and the direct store.
The corrected offset-context query also recovers the preceding flag-zero
test at +0566 and the separate checker's flag-one test at +0605.

The corrected source-declared non-call/jump segment-reference query gives
three overlay fixup contexts at files57AE2, 57B08 and 57B81, all these
known bodies. Its two resident hits at 4B340 and 4B348 are the descriptor
table's paragraph words, not extra code writers. These are results within
the reporters' declared representation and instruction-window limits,
not an exclusion of other stores, split/computed segments or block aliases.

The complete allocation helper initializes its returned far pointer to
zero and calls the external allocator only when the flag is zero.
A zero returned pointer publishes flag one. The checker at +0600
returns AL zero for flag exactly one and AL one for every other byte.
FND-CONFIG-156 already reads those local branches; this corrected source
assessment verifies their binding through the actual descriptor alias.

Consequently, for a hypothetical admitted flag value other than zero or
one, allocation is suppressed and returns a null pointer, while the
checker permits continuation. This is a conditional branch consequence,
not evidence that a native writer produces such a flag. Initial byte
zero and this literal byte-one store do not prove the live flag's domain.
The external allocator, wider/aliased writes, storage lifetime and actual
phase history remain separate obligations. The known full-word FFFF to
retained-AL FF outer return asymmetry is still FND-CONFIG-156.

## Interpretation

Q-SCRIPT-019 has a verified local flag producer/check binding and a finite
reason the earlier empty census was invalid. A passing allocation check
still does not establish a valid trigger buffer. Actual flag value/history
and pointer/storage admission remain open; the possibility of nonboolean
input is neither silently excluded nor asserted to occur.

The duplicate-base table result bounds which segment targets need special
attention when reusing old first-descriptor reports. It does not invalidate
every report mentioning one of these segments: the actual query target,
controls, representation and conclusions must be checked. No executable
coverage, whole-entry complete reading or queue closure follows.

## Alternatives

An empty report that misses its known store is not proof of no writer.
Equal paragraph bases do not imply equal descriptor operands, nor do their
two rows provide independent storage. Treating the checker as rejecting
all nonzero bytes conflicts with its equality-to-one branch.

## How to reproduce

Use report.mjs table on the licensed source and fingerprint above, start
4B080 hexadecimal, count229, stride8; unsigned word fields paragraph,
high, flags and low at offsets zero, two, four and six. Retain
segment-descriptor-alias-config.json and its report under
GAME_DIR/analysis/handler-census-20261106/. Group flags-zero/one rows by
paragraph while retaining their zero-based row indices; retain
segment-descriptor-alias-groups.json and require the eight groups above.

With the corrected reporters, query 4C0C:0000 and require the known
store/read controls before interpreting a missing candidate. Retain
trigger-allocation-failure-writers-fixed.txt,
trigger-allocation-failure-segment-contexts-fixed.txt and
trigger-allocation-failure-offset-contexts-fixed.txt beside the rejected
earlier writer-candidates and segment-context reports. Use overlay_listing.py
descriptor169 on 57AD2..57B94 for trigger-allocation-flag-helper-bodies.txt;
keep +0552..059B and +0600..0614 distinct from the intervening clear helper.
Reuse FND-CONFIG-156 for their callers and return consumers. No original
game or emulated call supplied new evidence for this assessment.
