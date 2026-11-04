---
id: FND-COMBAT-108
title: MAS 65's eighteen encoded box triggers cannot accept an unchanged category-two replacement event
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0290..31E0:02FE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:0573..1695:06FF
tool: retained script_decode.py index of FND-COMBAT-100; PowerShell exact-line registration classification; resident_listing.py at 0a705cf0 with actual MZ relocations
environment: null
---

## Observation

FND-COMBAT-107 connects GPL/119's conditional request 9 to a possible
cell/box trigger run. FND-COMBAT-101 identifies MAS/65's box registrations
as declared entry controls for the four GPL/119 paths reaching its local
selector. This reading tests those encoded records against the replacement
loader's category, rather than assuming their coordinates must match.

### The index has eighteen literal box registrations in MAS/65

The retained index SHA-256 remains
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
Selecting all canonical MAS/65 top-level cell/box registration lines gives
18 instructions. Every selected line is opcode 0x6A with seven simple
integer operands; none is rejected by full-line literal matching. There
are no indexed top-level 0x68, 0x69 or 0x6B registrations in this selection,
or indexed nested occurrences of these four opcodes in MAS/65.

All eighteen name GPL/119 and encode category zero as their seventh
operand. Four name entry offsets 146, 336, 422 and 456 once each; fourteen
name offset 531. These entry offsets are decimal, matching the controls
in FND-COMBAT-101. The four first entries are its selector-reaching roots;
the other fourteen registrations do not add fourteen new distinct roots.

Global top-level controls reproduce FND-SCRIPT-038's 150 opcode-0x68
and 321 opcode-0x6A instructions, including 63 cell and 26 box instructions
whose final operand is literal two. These nonzero-category controls distinguish
MAS/65's local zero result from a selection that never admits category two.
FND-SCRIPT-038 independently maps
0x6A's seventh operand to byte +0x0A of the box record. These are counts
of decoded instructions, not proof of which registrations execute or remain
in the live list. The GPL/37 gap and changed/native entry streams remain
outside the retained index.

### The loader and walker use a concrete category threshold

FND-COMBAT-107's loader compares its returned slot signed against four.
The at-least-four branch assigns category two, which is passed to the
trigger wrapper if the later DS:0DAB gate admits the call. The smaller-slot
branch selects zero or one by comparison with the named leader word.
The returned slot's actual domain is not established here.

FND-SCRIPT-028's box wrapper retains the event category in the word
4C0D:0001 before its coordinate helper and walker. The walker first tests
the box coordinates, then zero-extends record byte +0x0A into a word and
compares it signed with the current category word. A lower record value
skips that record and continues the list; it does not call the script entry
for that record.

Thus an unchanged zero-category record fails against an unchanged category
two regardless of its coordinates. Each of MAS/65's eighteen encoded
records is excluded as that event's selected record under those conditions.
This does not establish the state at the native comparison: parameter and
record preservation, segment/storage identity and effects through the
coordinate helper remain conditions, not implicit harmless-call assumptions.

### A normal free-list setup supplies a relevant conditional slot range

FND-PARTY-119 records the small free-list initialization and the helpers
used by the lower loader: its initialized small list contains slots 5
through 47, with other allocation paths also described there. If those
allocation/list and return-preservation conditions apply, a slot from that
small list takes the loader's category-two branch. The initialization alone
does not admit the list at GPL/119's call, exclude later writers or prove a
particular returned slot. No additional free-list invariant is inferred.

## Interpretation

The eighteen unchanged MAS/65 records are not evidence that replacement
must reenter GPL/119 merely because the replacement is placed inside one
of their boxes. With the category-two producer and walker input preserved,
their encoded categories already exclude them, without a coordinate proof.

Q-COMBAT-062 must instead establish a smaller-slot/category alternative,
changed category/record state, or a different admitted cell/box record to
make this chain enter a script. Other registrations, copies/restores, list
replacement, aliases and native entry paths are not excluded. A different
record's script could also reset interpreter state without selecting GPL/119.
The outer-frame continuation obligation remains independent of this filter.

No actual trigger execution, whole-list absence, native slot bound, local-bank
preservation or complete reading is established. The rule's status is unchanged.

## Alternatives

- Coordinate overlap alone cannot admit these eighteen encoded records for
  category two: each still fails the category comparison.
- Treating the eighteen records as the complete live list would require
  registration, replacement and preservation evidence not supplied by an index.
- Initial small-list membership does not prove every later replacement slot
  is at least four. Retain allocation, restoration and other-writer limits.
- An unchanged category-zero event can pass the same records' threshold;
  excluding the category-two case is not a universal trigger exclusion.

## How to reproduce

Reuse FND-COMBAT-100's source fingerprints and FND-COMBAT-104's retained
index. In PowerShell select full lines using case-sensitive regex
`^MAS/65 [0-9A-F]{4}: (68|69|6A|6B) `.
Require each selected line to match
`^MAS/65 [0-9A-F]{4}: 6A (-?[0-9]+), (-?[0-9]+), (-?[0-9]+), (-?[0-9]+), (-?[0-9]+), (-?[0-9]+), (-?[0-9]+)$`.
Parse captures as integers; group captures five, six and seven as entry,
script and category. Retain rejected lines explicitly: accepted 18, rejected
zero, all category zero and the five entry groups above.

For MAS/65 lines also search nested markers with
`\{[0-9A-F]{4}: (68|69|6A|6B) `; retain the zero result as an indexed-only
limit. Reproduce the global opcode controls with
`^[A-Z]+/[0-9]+ [0-9A-F]{4}: 68 ` and the corresponding `6A` pattern.
Within each global selection require the category-two controls by matching
`, 2$` at the end of the whole line: 63 cells and 26 boxes.
Do not interpret a substring in another operand as a top-level instruction.

Use resident_listing.py at the stated tool commit for both half-open ranges
in Locations, with the fingerprinted DSUN.EXE and locked evidence Python.
Trace the loader's signed slot branch and actual category push, then the
wrapper's category store and the walker's zero-extension, signed comparison
and reject continuation. Reuse FND-SCRIPT-038/028 for record construction
and FND-PARTY-119 for the qualified allocation context. Do not replace those
children or their preservation obligations with stubs.

The native category-control listing is retained as
mas65-trigger-category-controls.txt under
GAME_DIR/analysis/local-bank-preservation-20261103/. The decoded index
remains under GAME_DIR/analysis/reader-admission-20261029/. No coordinates,
script text, native listing or original resource bytes are committed.
