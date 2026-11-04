---
id: FND-COMBAT-116
title: The shipped restore spans separate effect rows and count while SAVE 21 supplies the nominal boundary row
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
    offset: 0x0004E57E..0x0004E672
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00044580..0x00044674
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B750..0x0004B758
tool: tools/evidence/report.mjs table and operand at 9e3da2e5; executable-reader 4.3.0
environment: null
---

## Observation

Q-COMBAT-065 retains destination extents and overlapping restore writes.
FND-SAVE-015 already enumerates all 61 shipped pointers and lengths;
FND-COMBAT-034 binds the effect slots and FND-COMBAT-045 gives the
row segment's backing. This comparison resolves their nominal overlap
question, without treating shipped lengths as reader capacity arguments.
All ranges below are hexadecimal and half-open.

The hash-checked table reporter rereads 61 four-byte pointer entries at
file 4E57E and 61 four-byte length entries at 44580. Fifteen pointer
entries are zero: one-based resources 5..10 and 30..38. Every segment
word of the other 46 resolves through an actual declared MZ relocation,
using that entry's stored offset and load segment 1000. Controls agree
with the earlier table: resource 24 resolves to 4E71:024D, resource 26
to 57E0:2100 and resource 46 to 4C13:0369.

Comparing the 46 nonnull spans using each shipped length shows no
other such span intersects resource 24's nominal row span or resource
26's count-word span. All 46 offset-plus-length calculations are below
10000, so this bounded comparison requires no offset wrap. It says
nothing about the fifteen initially null entries after runtime binding,
changed pointers, selected resource lengths or aliases introduced later.

| Resource or conditional read | Loaded span | Corresponding file span |
|---|---|---|
| SAVE/24, shipped length 780 | 4E71:024D..09CD | 43B5D..442DD |
| SAVE/26, shipped length 2 | 57E0:2100..2102 | 4F100..4F102 |
| SAVE/21, shipped length 88 | 4E71:09CD..0A55 | 442DD..44365 |
| Effect boundary owner read at index 192 | 4E71:09CD..09CF | 442DD..442DF |
| Complete boundary row at index 192 | 4E71:09CD..09D7 | 442DD..442E7 |

SAVE/21 therefore abuts rather than overlaps the nominal SAVE/24
payload. It contains the complete ten-byte boundary row used by the
effect helper contracts of FND-COMBAT-032/033/045. Those bytes are
not part of the nominal saved effect rows. Under admitted stable
pointers, selected shipped lengths and successful earlier transfers,
the reader requests SAVE/21 before SAVE/24 and SAVE/26, as its
ascending loop in FND-COMBAT-035 records. Subsequent writes and
child effects remain separate conditions; the table does not prescribe
a sentinel owner or an invariant value for the boundary bytes.

### Count-word backing

The complete 229-row descriptor read reproduces FND-COMBAT-045's
descriptor 110, relative segment 3E71, start zero, end 0C6D and flags
zero. Descriptor 218, at file 4B750..4B758, has relative segment
47E0, start zero, end A4EC and flags zero. FND-EXE-571 and
FMT-EXE-002 supply the resident-span interpretation. At load segment
1000 this latter span is 57E0:0000..A4EC, containing the entire
count word 2100..2102 without wrap. The backing is not evidence
that DS retains that segment at every consumer or that selected
resource 26 actually contains exactly two bytes.

## Interpretation

The nominal fixed destination table does not itself alias another of
its 46 known nonnull payload spans into the saved effect rows or count.
The boundary read is different: it lies wholly in SAVE/21's nominal
destination, not in a reserved extra effect-row payload. That identifies
a concrete earlier restore source relevant to boundary contents.

Q-COMBAT-065 need not repeat the fixed-table relocation, nominal
span comparison or count-word descriptor backing. Runtime bindings
for the fifteen null entries, other pointer writers, actual lengths,
segment identity and overlapping transfer/stack aliases remain open.
Q-COMBAT-064 owns the actual resource source and length; Q-COMBAT-066
owns child effects. No actual save, count bound, dedicated sentinel,
successful transfer or immutable storage has been established.
No rule or parity status changes.

## Alternatives

- Nominal disjoint spans do not establish runtime nonaliasing.
- The reader requests selected lengths, not this shipped length array.
- Resident byte backing does not reserve the boundary bytes for effects.
- Earlier successful resources can change boundary contents before the
  saved rows and count are requested.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use report.mjs with
size 634416 and XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
The table command takes start 4E57E, count 61, stride four and
two width-two fields at zero and two; the length table takes start
44580, count 61, stride four and a width-four field at zero.
For each nonnull pointer use operand with site 4E580 plus four
times its zero-based index, targetOffset equal to its offset word
and loadSegment 1000. Require a declared relocation at every site.

Compare each half-open interval [fileOffset, fileOffset + length)
with the two target intervals above; exclude the target entry itself
and retain all null entries as unmeasured. Read the descriptor table
at 4B080, count 229, stride eight with width-two fields at offsets
zero, two, four and six. Reuse FND-EXE-571 for field meanings.
Keep configurations and reports only under
GAME_DIR/analysis/hit-incoming-20261104/: restore-shipped-pointers.json,
restore-shipped-lengths.json, their -report.json counterparts,
restore-slot-<resource>-operand.json, restore-shipped-resolved-slots.json,
restore-resident-descriptors.json and its -report.json counterpart.
No original bytes, payloads or table dumps are committed.
