---
id: FND-SOUND-099
title: Setup refresh derives display geometry and copy gates from requested mode and native query fields
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1783..1000:1845
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:16A0..1000:16DB
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2671..1000:2690
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1BC3:00EC..1BC3:0113
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1ABD:000A..1ABD:001B
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D616..0x0001D636
tool: Ghidra 12.1.3 ReportFunctionSummary.java; resident_listing.py; tools/evidence/report.mjs table and x86-operand-candidates
environment: null
---

## Observation

Q-SOUND-010 retains actual mode/geometry and backing admission beyond
FND-SOUND-098. This assigns the controlled literal producers of its five
principal geometry/copy-gate fields, connects their mode-request callers,
and distinguishes loaded state from runtime history. The source is
SOUND_DS.EXE,204593 bytes,XXH3-128236c2dc23c071eca421eb5b427caee57.
Offsets and field values below are hexadecimal; counts are decimal.
All local contracts require admitted DS/stack and retained nonaliased
state. No native query, low-memory comparison, original or emulated call ran.

### Controlled producer cohorts

Reuse FND-CONFIG-259/260's unchanged25-region setup census, entry/transfer
declarations,58208 scanned starts, scanLimit1000000 and limit10000.
Repeated query results match; every report is untruncated with
partialSearch false and59 retained walk gaps. Independently read consumer
and producer instructions provide the positive controls. Replacing the
first control with its next byte rejects each negative configuration.

| Literal | Verified memory uses | Writes | Positive file controls | Rejected overlaps |
| --- | --- | --- | --- | --- |
| DEC0, column extent byte |8 |2 |27EB,2B91 |1 |
| DEBF, row extent byte |3 |1 |382F,2BE4 |2 |
| DEC1, transfer classification byte |4 |1 |299B,2BCB |2 |
| DEC2, direct-copy synchronization byte |2 |1 |2815,2C10 |1 |
| DEC7, direct-transfer enabling word |3 |0 |29A4,37ED |1 |

All five cohorts have zero verified other operands and zero unresolved
boundary candidates. The five write sites are the two column assignments
and three other field assignments in1000:1783 below. DEC7's three returned
uses are reads. This is not a claim that DEC7 can never change: computed,
implicit, block and alias accesses, other entry mechanisms, changed
streams and native history remain excluded or unresolved. No incoming
state is admitted solely because it lacks a literal writer here.

### Loaded state and mode-request callers

The bounded DS:DEB6..DED5 table shows initial bytes DEBE,DEBF,DEC0,
DEC1 andDEC2 zero, and words DEC3/DEC5 zero. DEC7 starts as word0001.
The constant at DEC9 has six nonzero bytes followed by NUL. These are
verified loaded-image values, not observations at a chooser call.

The recognized startup at1ABD:0011 calls1BC3:00EC with word0019.
That wrapper maps exact0019 to0003 and0032 to0040; other words return
without calling the mode-refresh wrapper. FND-SOUND-097/098's chooser
also supplies0032 on its larger-layout path and0019 when restoring it.
The wrapper saves/restores SI/BP, but relies on its called mode path
for other preservation.

1000:2671 receives one word. ExactFFFF substitutes the zero-extended
current byte DEBE; otherwise it forwards the supplied word to near1783.
After that child returns it copies byte DEBD to DEBC. It does not return
a normalized success predicate. Ghidra's recognized incoming sites bind
2671 to1BC3:00EC and1783 to2671; these references are not a census of
all possible computed/alternate entries.

### Refresh stores and native-result dependencies

1783 consumes only the low byte of its argument and first stores it as
DEBE, the requested mode. It calls16DB with AH0F, then stores returned
AH into DEC0. If returned AL equals current DEBE, it keeps that requested
byte. Otherwise it calls16DB with AH00 and current DEBE in AL, calls
16DB with AH0F again, and replaces DEBE/DEC0 with the second result's
AL/AH. The returned word is16DB's result, not necessarily an untouched
raw interrupt result;098 records that dispatcher's special AH00/0F paths.

The remaining stores are:

| Destination | Local source or condition |
| --- | --- |
| DEC1 byte |1 exactly when retained DEBE is04..3F except07; otherwise0. |
| DEBF byte |19 unless retained DEBE is40; then byte at0040:0084 plus1 modulo256. |
| DEC2 byte |1 only when DEBE differs from07,16A0's prefix comparison returns0, and16CD returns0; otherwise0. |
| DEC5 word |B000 for retained mode07, B800 otherwise. |
| DEC3 word |0000. |
| DEB8,DEB9 bytes |Both0000. |
| DEBA byte |DEC0 minus1 modulo256. |
| DEBB byte |DEBF minus1 modulo256. |

16A0 compares the terminated constant at DS:DEC9 with bytes beginning
F000:FFEA. It returns1 after matching the entire first string, without
requiring a terminator in the second; any unequal compared byte returns0.
Under retained initial constant state it therefore tests a six-byte prefix,
not an unbounded second string. Each pointer advances only its offset
word. This pure helper restores BP and does not change SI/DI/DS, but
leaves ES as the last loaded segment. Actual native bytes and constant
retention remain conditions.

16CD calls16DB with AH12 and BL10, then returns the low-byte difference
of returned BL and10, zero-extended to AX. A zero result means the
returned BL still equals10; this is not an admitted native adapter type.
The optional call is skipped when the prefix comparison succeeds or mode
is07. No native reply or hardware interpretation is inferred here.

### Consequences for the chooser's conditional profile

If the retained mode is03, these local stores give DEC1 zero, row extent
19, segment B800, base word zero and zero origins. The column extent still
comes from the query's AH. It is not established as50 by the request03.
If that extent is50 and DEC7 retains its initial1, the known consumers
in098 select the direct-transfer branch and have80 columns/25 rows;
an admitted full-width scroll scratch then needs exactly160 bytes.
The actual extent, retained flag, BIOS preservation and backing spans
must still be proved at use. Mode03 alone does not settle DEC2 or prove
that direct copying bypasses the port polling path.

Mode40 similarly clears DEC1 but obtains row extent from separate native
storage; a requested0032 does not by itself establish50 rows. Zero or
changed queried extents can also alter the byte-minus-one window bounds.
These are conditional source mappings, not observed native failure cases.

## Interpretation

The five literal producer sites and mode-request forwarding are assigned;
Q-SOUND-010 no longer needs a local reading to explain how these five
fields acquire their values. Actual query/low-memory replies, retained
DEC7/prefix/mode state, excluded writers, BIOS SI/DI/ES and port progress,
plus heap/backing/frame lifetime still prevent native chooser admission.
No complete-reading declaration, format-status promotion, whole queue
closure or parity validation is claimed.

## Alternatives

Requested mode is not proof of the queried mode or column count. Loaded
DEC7=1 is not an immutable-state claim. Prefix equality is not comparison
of two complete terminated strings. A returned zero from16CD is a byte
relation to its supplied BL value, not a measured device category.

## How to reproduce

Use the verified read-only SETUP-blocks project in
GAME_DIR/analysis/config-block-construction-20261108/.
display-state-producers-ghidra.log and display-state-callers-ghidra.log
hold the connected summaries. Regenerate display-state-producers-instructions.txt
for1000:1783..1000:1845 and1000:16A0..1000:16DB;
display-state-callers-instructions.txt covers1000:2671..1000:2690,
1BC3:00EC..1BC3:0113 and1ABD:000A..1ABD:001B.

display-<field>-query/report/repeat/negative files retain the five controlled
censuses. Each negative query replaces its first positive control with
the next byte, retaining the second control. Successful report/repeat
SHA256 values are:

| Field | SHA256 |
| --- | --- |
| DEC0 |7a7beeca0cc344d8787f9643ba61dda3f129e85d64dcd1dc1f3bf066ec3f18f8 |
| DEBF |7e8bfcb9a80cb9f9a30d2a004cb6fe20fc3655c2af07b34917985f87a55c1f1d |
| DEC1 |f49573b0be8efcdd970bb9dc85634de22c59cbaad8a77f1a91c1f46b2e0acc69 |
| DEC2 |202ee08c46bcbf5d2da911106aeb0171d9e9016405cbebfbfbe317194ce1c492 |
| DEC7 |2b9fa18ed038102cc37833775be648cce605af0a579f20cb9ebe3c4edfaec591 |

display-state-initial-query.json reads32 one-byte rows at file1D616;
its table result/repeat SHA256 is
317cc480a3d5747365208603e078f1c37e333b07429ecd5a44f014429d0f6f00.
Only successful controlled outputs support this finding. Keep all original-
derived configurations, literals and reports in GAME_DIR.
