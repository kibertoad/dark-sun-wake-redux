---
id: FND-COMBAT-151
title: Reverse script conversion uses a narrowed signed scan bound and conditional near-local match flags with no own release request on read failure
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071659..0x00071869
tool: tools/research/exec-census/overlay_listing.py at ba8c9877; project-local Python 3.12.10 and Capstone 5.0.7; inventoried overlay entry and declared FBOV fixups
environment: null
---

## Observation

Q-COMBAT-066 retains the reverse script-entry conversion after object
reconstruction. FND-SCRIPT-017 and FND-SAVE-023 describe its normal GPLI
mapping and known callers. This reads the complete overlay 187 +1829
body for the remaining machine-width, memory-footprint and failure
qualifications. Offsets below are hexadecimal unless stated otherwise.

### Size and data are separate requests

The converter initializes local far table pointer BP-04 to zero, then
asks 38FF:05B5 for GPLI/1's length through SS:BP-08. A nonzero AX calls
the diagnostic with argument 180B. There is no immediate branch to the
converter's return after that call: if it returns, the body tests the
current unsigned double-word length for greater than zero. Zero reaches
the common diagnostic with argument 181B, followed by the epilogue if
that diagnostic returns.

A positive current length requests GPLI/1 through 38FF:04AB, with the
far address of local BP-04 as its in/out slot. The earlier null assignment
admits the allocator branch only while that slot remains unchanged.
FND-CONFIG-151 and FND-COMBAT-120 describe the shared size/data lookups,
fallback cursor, allocation and complete transfer. The converter does
not compare the data request's selected provider or completed count with
the earlier size request. Their identities, preservation and aliases are
separate obligations, not consequences of using the same tag and number.

A nonzero data-reader AX reaches the common diagnostic and then the
epilogue on its returning continuation. That branch skips the converter's
own 444C:0092 release request, even though the shared reader can assign
an allocated pointer before a later failure. This is local request order,
not evidence of a native memory leak, successful cleanup or a returning
diagnostic. No undo of partial output or destination changes is encoded.

### Exact count arithmetic

After a zero data-reader result, the converter divides its current
unsigned double-word queried length by six using a zero high dividend.
It retains only AX of that quotient as the local word scan bound; the
remainder and quotient high word are not checked. Let L be that queried
length and k = low16(floor(L/6)). Every table scan compares its word
index with k using signed comparison.

With unchanged valid locals, scans therefore visit indices zero through
k minus one only when signed k is positive. Zero or a high-bit-set k
causes no table-entry match iteration, while chain traversal and selected
trigger-record checks still proceed. No own divisibility, maximum-count
or allocation-capacity test is added. FND-SAVE-023's nominal shipped length
7896 decimal yields 1316, within the positive signed range; that file fact
does not admit the actual runtime provider/length pair.

Table addresses use the selected far pointer's segment and low-word
offset arithmetic for six times the index. There is no independent far
normalization or wrap/capacity guard. A mathematically positive scan bound
alone does not establish distinct, readable records.

### Two pairs per chained record

The initial chain index is the full word DS:5AF5. FFFF ends traversal;
no other head range, visited set or iteration cap is checked. Each record
uses word-width 19-times-index addressing in the declared 4F49 segment.
The two entry words are at record offsets eight and ten, and the paired
script words at offsets twelve and fourteen.

For each chain record, the converter initializes two local match bytes
through SS:BP-0E and SS:BP-0D. Its inner loop forms a near pointer to
those locals, but reads and marks it through DS rather than SS. The
ordinary first-match interpretation therefore requires those accesses
to denote the initialized local bytes, with valid nonaliased storage;
the body contains no explicit DS/SS equality test.

Under that condition, each table row is considered in order for each
still-unmatched pair. A match of the pair's entry word with the GPLI
row's first word writes the second word into the entry word and the
third word into the paired script word, then marks the pair matched.
Each pair keeps its first match. An unmatched pair is left unchanged.
The outer table loop continues through its bound even after both pairs
have matched, although the match flags suppress further pair comparisons.

After the scan it reads the next-link byte at record offset eighteen,
sign-extends it to the next word index and repeats until FFFF. Other
negative byte values do not terminate traversal. Chain/table range,
acyclicity, stable match flags and nonaliasing are not established here.

### Fixed trigger-record pass

When the argument's low byte is nonzero, the converter visits exactly
200 thirteen-byte trigger records. It loads the current far pointer at
DS:40C0 repeatedly for gate, compare and destination accesses, adding
the word-width record displacement. It skips a record whose word +2
is zero; this is not a test of whether word +0 is already an offset
or a GPLI entry number.

For a passing record, a normal SS-relative local match flag starts zero.
The scan compares word +0 with each GPLI row's first word in order.
Its first match replaces words +0/+2 with the second/third table words
and ends that record's scan. No match leaves those words unchanged.
No other trigger field is explicitly written by this body. Capacity,
pointer stability between those reads and aliases with table/stack/other
state remain unadmitted.

### Normal completion and caller result

After the chain pass and optional trigger pass, the normal path requests
444C:0092 for the current local table pointer, then reaches the epilogue.
FND-CONFIG-165 qualifies that wrapper's metadata checks, runtime request
and zero return; the request alone proves neither release nor preservation.
The converter has no own normalized aggregate success return combining
the size query, data request, diagnostics, remaps and release.

FND-COMBAT-111 establishes that the numbered reader calls this converter
after either transfer-loop outcome and retains its own transfer flag.
The converter invokes no script interpreter directly in its own body;
its external calls are size/data requests, diagnostics and release.
That local call inventory does not exclude aliases or transitive effects.

## Interpretation

This removes the reverse converter's own count arithmetic, match-flag
segment dependency, destination footprint, unmatched behavior and cleanup
order from unread work under Q-COMBAT-066. It qualifies the normal mapping
summaries without treating nominal shipped records as runtime admission.

For valid stable storage, the nominal positive table length, matching DS/SS
locals and appropriate incoming entry-number state, the body performs the
record conversions described by FND-SCRIPT-017 and FND-SAVE-023. Inputs
already in offset form are not distinguished by an explicit representation
marker; a coincident matching number can be remapped again, as those
findings already note.

Actual size/data provider identity, allocation/span validity, DS/SS and
chain/trigger bindings, pre/post values, aliases, failure/diagnostic behavior
and later caller work remain. Other incoming routes remain Q-SCRIPT-020;
trigger lifetime remains Q-SCRIPT-019. No native failure, complete reading,
rule or parity promotion is claimed, and no queue item closes.

## Alternatives

- The general scan bound is the full mathematical length divided by six:
  the machine retains only the quotient's low word and compares it signed.
- Both match-flag uses are inherently stack-relative: the two chained-pair
  flags are initialized through SS but accessed by a DS near pointer.
- Every returning path requests release: the data-error diagnostic path
  reaches the epilogue without the converter's own release call.
- A trigger's nonzero script word marks encoded form: both ordinary offsets
  and GPLI numbers can pass that same gate.

## How to reproduce

Resolve the fingerprinted installed DSUN.EXE through tools/game-dir.mjs.
With the project-local evidence Python and `-I`, run overlay_listing.py
for descriptor 187 and half-open bounds 0x71659 0x71869. Follow both
diagnostic continuations, the double-word division/word store and signed
backedges, segment defaults of the near-local match accesses, chain links,
trigger gates and release bypass. The retained original-derived listing
is only GAME_DIR/analysis/hit-incoming-20261104/restore-script-reverse.txt.
Reuse FND-CONFIG-151/165 and FND-COMBAT-120 for shared callees; do not infer
their success or stable providers from this local caller. No original-derived
listing or script payload is committed or executed.
