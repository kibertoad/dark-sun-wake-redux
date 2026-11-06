---
id: FND-PARTY-273
title: The object-dice route forwards an effect-row DATA word whose 112 recorded source values exclude 1001
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F598..0x0007F798
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000920F6..0x00092115
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047110..0x000478F0
tool: tools/research/exec-census/direct_callers.py and overlay_listing.py at 1fb51114; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; FND-PARTY-106/272, FND-MAGIC-018/019 and FND-RNG-035 reuse
environment: null
---

## Observation

Q-PARTY-124 retains the incoming argument domains of the three word-input
helpers identified in FND-PARTY-272. Installed DSUN.EXE is 634416 bytes,
XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`.

The direct-call query recovers these six sites:

| Helper | Direct incoming sites |
|---|---|
| overlay 193 +1E81 | overlay 179 +0E45 |
| overlay 193 +1F88 | overlay 193 +04B0 and +1431, both near after PUSH CS |
| overlay 208 +07F2 | overlay 208 +0666 and +087C, near after PUSH CS; overlay 197 +1B83, far |

FND-MAGIC-018 and FND-RNG-035 independently identify the duration and
two dice calls, controlling the near and overlay-far forms. The query
keeps trampolines but does not cover computed calls, jumps or bypass
entries. This is not a whole-program incoming-target proof.

### Object-row source domain

FND-MAGIC-019 already connects the second dice call to an object's
effect row. The complete overlay-193 +12A8 reading loads the row index
from its second word argument. It rejects exact 00FF and FFFF, tests
the selected row's byte +2 low nibble, and loads DX from row word +0
at +12E6. The base is descriptor 122's 51F1:0000, stride 18 bytes.

The mode-zero default call at +1359 passes that DX as the DATA-number
argument of +13EC. The admitted mode-one call at +13B8 passes it to
+148E, which forwards the same word argument to +13EC at +14A0.
Neither forwarding path substitutes the slot or the level for this word.
There is no intervening call between this row-word load and the named
default forwarding call on either of those local paths.

FND-PARTY-106 records all 112 initial row words, not just the rows
used by one spell. Their minimum is -1 and maximum 311; exactly one
is -1 and none is positive 1001. These statistics are computed from
that already recorded list. Thus any valid unchanged source row rules
out 1001 at the first getter in +13EC, call +1400, regardless of which
of the two named modes reaches it. No unclassified value remains in
that source table's DATA-number column.

The selector in +13EC is loaded into DI before its first getter call.
After the getter and a 73-byte copy, +1431 passes DI to +1F88. The
copy's DI preservation is already FND-ITEM-103. Preservation of the
retained selector through the earlier getter and actual frame/buffer
storage remains necessary to extend the source exclusion to the later
dice request at +1F9F. This is not inferred from the source row alone.

The entry +12A8 does not compare its index against 112. Rejecting
00FF and FFFF is not a bound to rows 0..111. Valid row selection,
the effective base/extent and source-word preservation remain actual
admission obligations. FND-PARTY-106's segment-reference search excludes
computed aliases and escaped pointer writes; its initial table and
bounded writer observations must not be read as a universal no-write
proof for those excluded forms.

### Range-wrapper parameter identity

Overlay 208 +0866 loads its second input word into SI. It passes the
first and second inputs to overlay 177 +07AF, then pushes returned AX
and retained SI before its near call of +07F2 at +087C. SI is the
first argument seen by +07F2, the DATA selector; the earlier AX return
is its second argument, used for level. This path does not narrow the
selector to a byte merely because the helper returns a level.
The actual second-word producers and SI preservation through +07AF
remain open. FND-PARTY-272's local 1001 path still applies.

## Interpretation

The object route's possible canonical DATA numbers are fully enumerated
and exclude 1001. Remove an unknown source-table value as a remaining
alternative for that route. The remaining mechanisms are invalid or
changed row selection/base/extent, mutated or aliased row contents,
incoming bypasses and selector preservation before subsequent requests.
Other cast/duration and range callers retain their own input domains.

This does not remove a complete getter site from the remaining 83:
the source exclusion is conditional and does not cover every incoming
path or changed state. No queue item closes, no complete reading is
declared, and FMT-PARTY-007 stays supported.

## Alternatives

- An unexamined canonical row cannot supply 1001: all 112 recorded
  words are included. An out-of-range or changed row can select bytes
  outside that statement, and this entry does not bound every caller's index.
- A source bound before the first getter is not preservation of DI through
  all reached services before the repeated dice call.
- The range-wrapper's returned level cannot stand in for its retained
  DATA-number word. A word producer can still admit 1001.
- No missing direct caller is inferred from an empty Ghidra reference list;
  excluded indirect and bypass mechanisms remain explicit.

## How to reproduce

Run direct_callers.py on the fingerprinted installed DSUN.EXE with
`193+1E81 193+1F88 208+07F2`, and separately `193+13EC 193+148E`.
Retain the controls, near-call frame form, trampolines and exclusions.
Use overlay_listing.py descriptor 193 at `7F598..7F6DC`,
`7F6DC..7F77E` and `7F77E..7F798`; descriptor 208 at
`920F6..92115`. Bounds are hexadecimal and half-open.

Compute min/max, count and membership from FND-PARTY-106's entire
112-word list and retain its original source extent and writer exclusions.
Reuse FND-MAGIC-018/019 and FND-RNG-035 for the other incoming bindings;
this finding does not claim their upstream words are already bounded.
Reports remain under GAME_DIR/analysis/class-table-closure-20261106/,
with word-helper-incoming-candidates, object-dice-direct-parents,
object-row-number-producer-complete, object-dice-number-forwarding,
object-dice-forwarder and word-helper208-chain stems. No original ran.
