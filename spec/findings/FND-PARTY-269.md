---
id: FND-PARTY-269
title: Remaining DATA type routes have bounded chunk and window sources while the two unbound readers have no relocated nearby pointer candidate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:080A..28C9:0829
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006EF49..0x0006F12D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007129C..0x00071445
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000704C9..0x0007054D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:31B9..1000:31E1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0716..1000:0793
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:384D..1000:386D
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py and alias_far_words.py at bcdb8dbd; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; Ghidra 12.1.3 bounded decompile matches from scientific-method-engine 19.10.0; existing FND-UI-002, FND-COMBAT-089, FND-ITEM-103 and FND-PARTY-264/268
environment: null
---

## Observation

This narrows Q-PARTY-124's four remaining type domains without assuming
they are all actual DATA requests. Installed DSUN.EXE is 634416 bytes,
XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`.

### Chunk-loader candidate classification

The retained raw direct-call census has sixteen candidates for
`2D40:000A`. The Ghidra caller report lists fifteen of them. Thirteen
Ghidra-listed setups have a complete literal CHAR or RDFF type push;
the two others push the halves of a returned tag.

The raw-only candidate is at file `0x1E6B4`, `28C9:0824`, within the
inventoried `28C9:05CF` body. Its visible setup pushes literal RDFF,
then the resource number and slot, before the far call. Thus fourteen
candidate setups are literal CHAR/RDFF; the Ghidra omission is not
evidence of another DATA type. This instruction/argument classification
does not prove each candidate reachable from every entry or close the
raw/Ghidra reference discrepancy.

The two returned-tag setups use overlay 187 `+0699`:

| Caller | Tag-builder arguments | Separate word left for chunk-loader flag |
|---|---|---|
| overlay 184 `+1FCA` | number FF hex, prefix selector zero | seven |
| overlay 187 `+1550` | incoming region number, prefix selector two | FFFF hex |

These bindings require word-level stack accounting. Each caller first
pushes a doubleword containing the builder's second argument in its low
word and the later loader flag in its high word. It then pushes the
builder's number. Removing four bytes after the builder consumes its
two word arguments and leaves the loader flag. Treating the doubleword
as one builder argument, or counting only the nearest three pushes,
would assign different parameters.

The builder copies eight bytes from DS:1672, the shipped prefix template
`RDOJRGOT`, into its local eight-byte area at SS:BP-8. It converts the
number to base sixteen in a separate buffer at BP-58 and uppercases it.
It then constructs its return from the selected two prefix bytes and
two number characters; a one-character number gets a leading zero.
The prefix reads use a near DS-based address derived from BP, not an
explicit SS override. DS/SS correspondence is therefore a real input
condition, not supplied by the earlier SS copy alone.

For intact corresponding frames, an unchanged copied template and the
two retained prefix selectors above, the selected pairs are RD and RG.
In both cases the return's lowest byte is R, so it cannot be DATA's D,
independently of the number characters. The first call produces RDFF
under that domain. This is a source-backed exclusion, not a claim that
runtime template bytes, segments or aliases have already been admitted.

The numeric conversion does not need an assumed unbounded output. Its
wrapper uses the input word zero-extended at base sixteen; the leaf's
division loop emits one through four digits and a terminating zero.
That output cannot reach the separate prefix area from BP-58. The leaf
saves/restores SI, DI and ES, and the uppercase helper saves/restores
SI and stops at that terminator. FND-ITEM-103 already supplies the exact
eight-byte copy primitive's DS/SI/DI preservation. These local contracts
do not establish native source/frame validity or the tag-builder's other
prefix-selector inputs.

### Window-child source bound

FND-UI-002 already reads all 402 child records in the 28 installed WIND
resources: 209 APFM, 184 BUTN and nine EBOX. None has type DATA.
FND-COMBAT-089 binds the child loader's request to the record's type
doubleword at +4. With that source record retained to the request, no
child number can turn its non-DATA type into DATA 1001.

This removes a possible missing or unclassified shipped child tag, not
all live child-tag writers. Actual provider/record identity and direct,
indexed, copied or aliased changes to the live +4 field remain distinct
questions. No window rendering or generic I/O success is inferred.

### Unbound generic reader target forms

FND-PARTY-268 finds no direct caller for `47E5:04B8` or `47E5:0500`.
A new alias_far_words query for both roots examines the same 12,965
readable relocated/fixed-up segment words as FND-PARTY-264 and returns
no nearby offset/segment candidate for either root. Reuse that finding's
independent direct-call, stored-pair and separated-store controls, with
the same source fingerprint and unchanged reporter.

Consequently there is no newly unknown literal/nearby relocated binding
to trace for those two helpers. Runtime-computed segments, target parts
assembled more than twelve bytes apart, unrelocated words, copied targets,
near/bypass entries and other excluded transfers remain. Neither an empty
direct caller list nor this bounded result proves the helpers unreachable.

## Interpretation

The remaining type work has concrete sources: the tag builder's DS:1672
template and copied near-frame reads for two chunk-loader callers; live
window-child type +4 and its provider/writers; and the actually unresolved
incoming mechanisms for the two generic readers. The fourteen literal
chunk setups, canonical prefix/number result and complete shipped-window
tag partition need not be researched again as unknown type alternatives.

Q-PARTY-124 retains those actual admission/writer/target conditions, other
getter-number domains and interpretation-affecting accesses. No queue item
closes, no complete reading is declared and FMT-PARTY-007 is not promoted.

## Alternatives

- The fixed prefixes apply to the two caller selectors with the copied
  source and segment correspondence stated above. Other selectors or
  changed template/frame bytes are not ruled out by a shipped string.
- The raw-only RDFF setup is not a negative Ghidra proof. Its incoming
  control flow and the reason for the missing reference remain outside
  this argument-classification result.
- A source census of all shipped child tags is not a live-record invariant.
  A general statement about harmless callbacks would not settle a write
  that could change the builder prefix or the live child type.
- The unbound-reader pointer query excludes only its declared target forms.
  No complete indirect-call or whole-program reachability result follows.

## How to reproduce

Reuse the sixteen raw candidates and the typed-input-callers Ghidra report
retained with FND-PARTY-268. Inspect the literal RDFF setup at
`28C9:080A..0829`, and the complete caller bodies with overlay_listing.py
for descriptor 184 `6EF49..6F12D` and descriptor 187 `7129C..71445`.
Run the same reader for descriptor 187 `704C9..7054D` and account for
both halves of the doubleword pushes before its calls. Use
resident_listing.py for the three runtime-helper ranges cited above and
reuse FND-ITEM-103 for `1000:0452`.

The read-only reconciled Ghidra project is INST-overlay-20261029g,
program INST-MAPPED.EXE. ReportDecompileMatches.java at mapped 7AC3:0699
with FUN_1000_0452, FUN_1000_31b9 and FUN_1000_384d gives bounded connected
context; instruction reads decide argument widths and DS/SS identity.
Mapped addresses are not shipped evidence locations.

Reuse FND-UI-002's complete WIND child census and FND-COMBAT-089's type
binding. Run `alias_far_words.py <dsun> 47E5:04B8 47E5:0500`; retain the
empty candidate lists, readable-segment count and FND-PARTY-264's controls
and exclusions. The new reports under
GAME_DIR/analysis/class-table-closure-20261106/ have chunk-raw-only-anchored,
chunk-tag184, chunk-tag187, chunk-tag-builder, tag-builder-leaves,
tag-builder-conversion-anchored, tag-builder-decompile and
unbound-reader-pointer-candidates stems. No original game ran.
