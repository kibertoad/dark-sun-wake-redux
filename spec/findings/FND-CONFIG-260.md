---
id: FND-CONFIG-260
title: Card 7C setup fills extended settings from five source fields before marking its main and digital records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 154E:000D..154E:0406
tool: Ghidra 12.1.3 ReportFunctionSummary.java; resident_listing.py; tools/evidence/report.mjs x86-operand-candidates; checkout-local evidence Python 3.12.10
environment: null
---

## Observation

FND-CONFIG-259 identifies22 other literal EC stores after its group-marker
reading. The EE query below leads to154E:000D, which contains two of those
stores and a complete paired extended-value producer. Source SOUND_DS.EXE
is204593 bytes, XXH3-128236c2dc23c071eca421eb5b427caee57. Addresses,
offsets, card numbers and explicit constants below are hexadecimal;
source-field positions and counts are decimal. Data segment is1E36.

### Source and conversion-result contract

The complete local body initializes its source far pointer to zero and
calls1000:03A9 with a ten-byte count and a stack-buffer argument. It then
calls1000:307B with the fixed far argument DS:05B6, stores the actual
returned DX:AX in that source pointer, and tests both words. A zero pointer
takes AX0 to the shared far-return epilogue. No record stores occur on
that local branch; called-helper effects are separate.

On a nonzero pointer, the body calls190F:0000 with two fixed far arguments,
then scans five comma-separated source fields. The first four scans stop
at comma; the last stops at NUL. Before fields2 through5, one comma is
skipped if present. Each field is copied into the same local buffer and
terminated with NUL. The cursor increments its offset word only, retaining
the far segment. The caller imposes no local field-length test or premature-
NUL alternative on the comma scans. Valid delimiters, bounded field/frame
storage and preserved source/segments therefore remain conditions.

Fields1,2,4 and5 are passed as far stack-buffer strings to1000:2713;
their actual returned AX words are saved separately. Field3 is scanned
and terminated but has no conversion call or retained setting value.
This finding does not establish2713's accepted numeric spelling, range,
overflow or malformed-value behavior. Denote the four returned words A,
B,C andD in source-field order. The caller transforms A only for these
six exact values; any other A is unchanged:

| A from field1 | Stored address word P |
|---|---|
| 00DC | 0220 |
| 00D2 | 0210 |
| 00E6 | 0230 |
| 00F0 | 0240 |
| 00FA | 0250 |
| 0104 | 0260 |

Thus these comparisons/remaps are word operations on a conversion result,
not proof of a hexadecimal or decimal source grammar.

### Two independent group updates

The body first calls17FB:0346 with card7C. On nonzero membership it
repeatedly calls17FB:01B0, recomputes that MainGroup record's far address
and writes these word fields. It then independently tests DigitalGroup
membership with17FB:03A4 and uses17FB:020C for its corresponding writes:

| Record field | MainGroup card7C | DigitalGroup card7C |
|---|---|---|
| EE | P | P |
| F0 | D, from source field5 | C, from source field4 |
| F2 | B | B |
| F4 | B | B |
| F6 | P | P |
| F8 | C | C |
| FA | B | B |
| FC | B | B |
| EC, written last | 0001 | 0001 |

Membership, first-index lookup, group pointers and signed counts are the
existing contracts in FND-SOUND-021 and FND-CONFIG-259. Every field store
repeats the index lookup and low-word index*254 address formation rather
than retaining one target pointer. Under valid disjoint group/record/frame
spans and preserved group state, each group therefore receives all eight
extended words before its EC marker is set. Without that preservation,
the repeated calls cannot be collapsed into one immutable target.

There is no local MusicOnlyGroup update in this body. After the independent
groups, it doubles the saved field5 local, then assigns AX1 and returns
through the same epilogue. The doubled local is not used by a later explicit
record store in this body. On admitted completed nonzero-source execution,
the local result is1 even if neither group contains card7C; it is not a
successful device-installation or file-write result.

Ghidra reports the full body through0405 and two ordinary incoming sites
in1861:0008. The retained instructions verify the actual DX:AX source
binding, five scans/four conversion calls, six word remaps, all field sources,
membership/index joins, repeated target formation, final EC stores and
local return ordering. Inferred decompiler far-return registers are not
substituted for the explicit DX:AX stores.

### Controlled EE representation census

Reuse the source/region/entry/transfer baseline of FND-SOUND-045/047 and
FND-CONFIG-259, querying literal EE with scanLimit1000000 and limit10000.
Positive controls are the constructor read at file5AD1 and this producer's
first MainGroup store at6AD3. Both are verified memory uses; the negative
control5AD2 inside the constructor instruction is rejected.

Repeated reports scan58208 starts in25 declared regions, are untruncated
and partialSearch=false, and retain59 walk gaps. They contain23 verified
memory uses, no verified other operands,27 rejected overlaps and no
unresolved literal boundaries. Twenty verified uses are writes; the two
in this body are now assigned their conditional record contract. Eighteen
other EE writes remain, alongside20 of FND-CONFIG-259's other EC writes.
Computed displacements, implicit/block access, aliases, changed streams
and native reachability remain excluded; this is not complete producer
coverage or a census of every extended-word representation.

## Interpretation

This supplies one actual extended-field producer connected to a subsequent
EC mark. On its admitted retained MainGroup route, the nonzero-EC full
constructor in FND-CONFIG-257 obtains first-block F0 from source field5
and second-block F8 from field4. The digital record's F0/F8 both use field4.
Those are explicit source distinctions; they are not universal meanings
for all cards' extended fields or observed native device settings.
Q-CONFIG-005 retains the other producer sites, source grammar/helper effects,
input/record lifetime and downstream application. No complete reading,
entry-status promotion, native run or original-content fixture is claimed.

## Alternatives

EC1 alone does not prove which fields were populated or survived. Equal
installed blocks do not remove the distinct MainGroup F0 assignment.
Interpreting AX1 as device success omits both absent-group alternatives and
the lack of a native device/file result check in this local return path.
Other producer modules may assign different sources to the same record words.

## How to reproduce

Use the verified SETUP-blocks project under
GAME_DIR/analysis/config-block-construction-20261108/ and packaged
ReportFunctionSummary.java for154E:000D. extended-record-copy-ghidra.log
retains the result. Generate extended-record-copy-instructions.txt with
resident_listing.py against the verified source, range154E:000D..154E:0406.
No source lookup is executed; this reads licensed local program bytes only.

The ee-query.json copies the FND-CONFIG-259 base, changing query.offset
and its controls as above. ee-report.json and ee-repeat.json have matching
SHA-25662731f7f4d8b7b60c88e76cb5c3df8658b5a8456772362b6b44735cb82f96030.
ee-negative-query.json and ee-negative.log retain the failed boundary control.
Keep all original-derived output in that local store.
