---
id: FND-CONFIG-244
title: Ordinary descriptor consumers do not construct the cross-row Preferences pointer candidate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:029B..4AE5:031B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0421..4AE5:04C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:07AD..4AE5:07C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B6D0..0x0004B6E0
tool: segment_table_readers.py and resident_listing.py at e1004c2b; pinned evidence Python, Capstone distribution 5.0.9 and xxhash 4.0.1; Ghidra 12.1.3 read-only consumer decompilations; reused FND-CONFIG-243 and FND-EXE-002/249/520/561/567
environment: null
---

## Observation

Q-CONFIG-014 retains actual consumption of FND-CONFIG-243's numerical
5782:0048 candidate. This reading tests the two already identified ordinary
segment-table consumers together, including the fixup child's offset producer.
It does not infer that all code uses only those consumers. Addresses, offsets
and unlabelled constants below are hexadecimal; counts are decimal.

The installed source remains DSUN.EXE, 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. CONFIG-243's source-bound row and
operand reports identify descriptor 202's end offset at file 4B6D2 and
descriptor 203's segment at 4B6D8. The former is unchanged 0048 and the
latter loads as 5782, so the hypothetical separated pair numerically names
the Preferences trampoline. Its actual use as a pair requires evidence.

### Descriptor walk consumes the extent only as a predicate

FND-EXE-561 identifies the walk at 4AE5:029B. With admitted manager DS
55CE, SI begins at 01A0, advances by eight and stops at 08C8: the 229
descriptor records. The loop tests flags bit one, then compares the current
record's word +2 with zero. The comparison does not copy that word to a
register or destination. A retained record instead supplies its segment
word +0 for the header-chain publication and subsequent header accesses.

Thus descriptor 202's value 0048 controls whether that record is skipped;
it is not carried forward as an offset when descriptor 203 supplies 5782.
The walk's stores build header links, loader identity and payload positions,
not an opener pointer from the adjacent descriptor fields.

The reached size helper 07AD..07C5 reads the current header's code/fixup
sizes and returns a paragraph-size calculation. It has no child or SI
assignment and performs no descriptor-field read. This removes an unread
local counter/extent propagation through that child. Actual header/table
and frame backing and unchanged segment/field bindings remain conditions;
this is not a native startup observation.

### Fixup replacement uses an aligned segment field

FND-EXE-249/520 identify the complete outer fixup body and optional child.
The outer body reads a fixup-list word, uses it as a destination offset in
the loaded overlay, and reads the old destination word. It preserves that
old word, clears its low three bits to obtain a descriptor byte index and
loads DS=55E8. Its table read at 0453 is exactly `[DI]`, an aligned
record-start segment field. It writes that segment to the loaded destination
before deciding whether the old word's low bit requests the optional child.

Under this table binding, the descriptor-202 end-offset field is not a
possible aligned table source even if the supplied index is outside the
229 admitted rows: its relative position is two modulo eight. This
alignment fact is not permission to treat an out-of-range index as valid.
Descriptor 203 can legitimately supply its segment word to a fixup that
selects that row; that alone does not supply a target offset.

The optional child 0466..04C6 loads DS from the replacement segment and
checks the surrounding loaded-overlay instruction bytes for its recognized
register-load/push sequence. A mismatch returns without undoing the earlier
segment replacement. On a match it reads the prospective offset from
ES:[BX+4] in that loaded overlay, searches header trampolines from offset
0020 using the header's count at +0C and five-byte stride, and, if a
target matches, replaces the loaded-overlay offset with the trampoline
position. It does not fetch that offset from the preceding descriptor's
end-offset field.

The actual offset therefore comes from a separate loaded-code field and
the reached header table. Admitted disjoint overlay/header/table/frame
backing is essential to this distinction. A copied value, physical alias,
changed segment or constructed source can fall outside it. FND-EXE-249's
independent zero-count LOOP cases, wrapping accesses, first-write ordering,
count preservation and native extent/return conditions remain intact;
this finding does not turn either search into unconditional success.

### Reused reader census and independent controls

The existing source-guarded table-reader report again finds nine relocated
words naming manager data 55CE and three naming table segment 55E8.
Four of those words belong to the descriptors that name these segments.
No overlay fixup names those four descriptors. The explicit DS=55E8
load in the fixup pass and the walk's known table accesses are positive
consumer controls, independently read in EXE-249/561/567.

The header-segment classification remains 216 resident far-transfer
operands, fifty descriptor words and two other resident sites; the overlay
classification remains 1929 far-transfer operands and 153 pushed trampoline
pointers, with no other classified fixup site. This reproduces EXE-567's
constant-segment mechanism, not a new whole-program reader absence proof.
Runtime segment arithmetic, memory-derived segments, copied or changed
data/code, other aliases, differently entered code and other consumers
remain outside that census.

Ghidra's complete local decompilations contain 46 lines for the walk,
29 for the outer fixup and 31 for its child. The instruction reads decide
the alignment, source segments, predicate-versus-value distinction and
write order. Ghidra's fixed data/string labels do not establish physical
field identity after segment loads. No canonical project was changed.

## Interpretation

Q-CONFIG-014 can remove an unread ordinary descriptor-walk/fixup construction
of this cross-row pair. Both known consumers have different evidenced
uses for these fields and for the actual fixup offset. Numerical equality
with a code address remains insufficient evidence of another caller.

The remaining candidate-consumption question requires a consumer outside
those admitted paths: another or computed/copied reader, transformed or
modified fields, a different segment/backing binding or alternate entry.
Its effect on readback would be an additional invocation of the Preferences
opener and its getter/store, or a mutation of their inputs. Retain that
mechanism and the ordinary callers' actual event/frame, reentrant, driver
and writer/lifetime conditions. RULE-CONFIG-003 remains supported, without
complete_reading. No whole queue item closes. No game process or emulated
function ran.

## Alternatives

- The walk's extent check constructs offset 0048: it only compares the
  field with zero; the later segment load reads a different field.
- The fixup pass can select that extent as an ordinary table source:
  its source index is aligned to the record's segment word.
- The optional rewrite gets its offset from the preceding descriptor:
  it reads the loaded overlay and compares against a header's trampolines.
- Those two paths exclude every consumer: the constant-segment census
  retains explicit construction, alias, mutation and alternate-entry limits.

## How to reproduce

Reuse CONFIG-243's pair-descriptors and operand reports under
GAME_DIR/analysis/preferences-root-contract-20261108/. For the unchanged
source, retain table-reader-census.txt from segment_table_readers.py under
GAME_DIR/analysis/descriptor-pair-consumers-20261108/. Require the twelve
declared manager/table segment words, known consumer controls and the
unchanged header-pointer categories before reusing its classification.

Use resident_listing.py for 4AE5:029B..031B, 0421..0475,
0466..04C6 and 07AD..07CC, retaining walk-fixup.txt and
fixup-child-size.txt. Only 07AD..07C5 is the size helper; the later
printed instructions start another body. Reuse EXE-249's complete fixup
and child reading and EXE-561's walk/interface, retaining their conditions.

Read-only with automatic analysis disabled, use DSUN.EXE in
handler-inventory-20261106/INST-handler-20261106 and
ReportDecompileWindow.java at 4AE5:029B, 0421 and 0466, lines
1..95. consumer-decompile.log retains the complete local output. Keep
all original-derived reports and projects in GAME_DIR. No new reporter
was introduced.
