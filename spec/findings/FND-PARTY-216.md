---
id: FND-PARTY-216
title: Attached-record word 2 supplies the quantity that object placement reads and splits through field 1
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
tool: Python 3.12 with Capstone 5.0.9, tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py and item_field_descriptors.py
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073398..0x00073670
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:06C4..2D40:06E1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:0009..1AA0:0209
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004D40F..0x0004D410
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004D413..0x0004D416
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF48..0x0001FF52
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0002031C..0x00020321
---

## Observation

Overlay 188's code starts at shipped offset 72EA0; offsets below are relative
to that code. The fixed query verifies the installed executable and OBJEX.GFF
fingerprints before reading the selected shipped descriptors. FNFO source
spans are those of FND-PARTY-052 and FND-CONFIG-153.

2D40:06C4 reads a signed word descriptor at DS:5A33 plus 196 times the
zero-extended kind byte plus twice the zero-extended field byte. For kind 1,
the shipped FNFO 1 first row and FNFO 2 type bytes give:

| Field | Record offset | Type byte | Width at DS:040C + type |
|---|---|---|---|
| 0 | 0 | 9 | 2 |
| 1 | 2 | 3 | 2 |
| 2 | 4 | 8 | 2 |
| 3 | 6 | 3 | 2 |
| 4 | 8 | 7 | 2 |

FND-PARTY-051 describes the kind-1 table's 23-byte records and their copied
CHAR payloads. 1AA0:0009 uses the returned descriptor as an offset into the
kind's record. For type 3 and width 2 it reads a signed word, sign-extends
it to a dword, and returns the low word in AX and high word in DX. No
type conversion runs for type 3. The setter 1AA0:012A resolves the same
offset and width and copies two bytes from its dword value argument to
the field with 1000:3F5A. Both accessors have rejected-object and missing-field
status paths, as FND-PARTY-214 already notes for the reader.

### Placement request

188 +04F8 takes its first word argument as the requested total in SI. It
selects objects through 2D40:0D94, requires kind 2, and lists their graph
through 2D40:2196 into an SS-local buffer. In that list it visits kind-1
entries (the word +2 of a ten-byte entry, +058A). It installs each entry's
object metadata through 2D40:286C before reading the fields.

Unless its second argument is 32767, it reads field 0 with 1AA0:0009 and
requires the returned dword to equal the sign-extended second argument
(+0592..+05B5). It then reads field 1 (+05BB), consumes AX as a signed
word, and replaces a nonpositive result with 1 (+05C5..+05CC). This is
the available amount passed to +066E.

It compares the 16-bit sum of available amount and already moved amount
to the requested total as signed words. When that sum is greater, it chooses
requested total minus already moved amount; otherwise it chooses the available
amount (+05CC..+05E2). It passes this chosen amount, the original available amount,
the graph pointer/index and coordinates to +066E at +0616. AL nonzero
adds the chosen amount to the moved total and sets the success flag.

After success it restarts the graph walk when the last chosen amount is
less than the requested total (+0641..+064F). At its exit it returns AL 1
when the moved total reaches the requested total, or when the requested
total was 32767; otherwise AL 0. The scan/progress/overflow and callee
return obligations are not claimed complete here.

### Whole record and remainder

+066E first places the object identified by the negated record word zero
(FND-PARTY-182). The branches after its earlier success tests are:

- When available amount is not greater than chosen amount (+0707..+070F),
  it lists the placed object, calls deletion 2D40:2452 on that list, lists
  it again, and calls 2D40:288C with the source graph/index and new graph
  (+070F..+0758). It retains AL as its success byte. The effects of those
  recursive graph operations are not read in this finding.
- When available amount is greater than chosen amount (+075D..+07A9),
  it writes the chosen amount to field 1 of the placed object at +0764,
  restores the source object's metadata through 2D40:286C at +0787, and
  writes available minus chosen to that source object's field 1 at +079D.
  Both values are passed as words with a zero high word, to the dword
  setter argument. Setter status is not checked, and it sets success to 1.
- A zero success byte triggers unload of the placed object's entry
  (+07B2..+07C9), then the byte is returned in AL.

The direct-call census finds calls of +04F8 at resident file offsets D555
and D5B4 and its trampoline 5702:0066; +066E has the near call at +0616
and trampoline 5702:00CA. This is a candidate census, not complete indirect
caller coverage or admission of the callers' requests.

## Interpretation

Under the shipped kind-1 descriptors, the signed word at +02 is the
record's quantity used by placement: it supplies the available amount,
receives the moved amount on the new object, and receives the remainder
on the source object for a partial split. Nonpositive stored quantities
are treated as one by this placement reader; that does not rewrite them
as one or prove how other readers treat them.

This supplies a code-backed meaning for one previously unnamed field of
the attached records. It does not identify field 3 at +06 or establish
the complete inventory, equipment or contained-record graph.

## Alternatives

- Runtime FNFO buffer population/stability and record-table bindings are
  prerequisites of this shipped-field interpretation; FND-PARTY-052 and
  FND-CONFIG-153 retain their limits. A replacement descriptor could point
  field 1 elsewhere. No dynamic comparison is claimed.
- DS-relative accesses to the caller's SS-local graph, current-object
  preservation and recursive transfer/delete effects remain unproved.
- Zero/failed field reads are not distinguished by +04F8's count clamp;
  setter failures are not distinguished by the partial-split success flag.
- Signed overflow, admitted request/filter values, every caller and every
  quantity producer remain open. This is not a complete placement rule.

## How to reproduce

With the hash-locked evidence Python, run item_field_descriptors.py on the
installed game directory. Its kind-2 rows are controls matching FND-PARTY-052;
the result-driving kind-1 fields are 0 through 4. Run overlay_listing.py on
installed DSUN.EXE, descriptor 188, shipped range 73398..73670; and
resident_listing.py ranges 2D40:06C4..2D40:06E1 and 1AA0:0009..1AA0:0209.
Run direct_callers.py with targets 188+04F8 and 188+066E. Read each comparison
at its actual width and retain the accessor status and alias limits above.
