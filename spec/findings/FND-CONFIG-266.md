---
id: FND-CONFIG-266
title: Card82 derives an SW32 file path and gates paired records on ordered section fields
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:0006..158E:06FF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1BD4:0318..1BD4:0344
tool: Ghidra 12.1.3 ReportFunctionSummary.java; resident_listing.py; tools/evidence/report.mjs table
environment: null
---

## Observation

Q-CONFIG-005 retains the last two EC/two EE sites after FND-CONFIG-265.
This reads their card82 root and its drive-byte producer. The input is
SOUND_DS.EXE,204593 bytes,XXH3-128236c2dc23c071eca421eb5b427caee57.
Offsets and record values are hexadecimal; character counts are decimal.
Actual file delivery, frame/source/record spans and child preservation
remain conditions. No native file or interrupt result is inferred.

### First path and line-derived second path

1BD4:0318 reserves a16-byte register frame, initializes only its AX word
to3305 and calls1000:20DF with vector21 and that same frame as input and
output. It takes the returned byte at frame+06, adds40 modulo256 and
returns it in AL. Other initial frame bytes are not explicitly cleared.
FND-CONFIG-263 supplies the reached register dispatcher/layout, including
its separate segment capture and carry/error route. This establishes a
local register binding, not the actual returned drive byte or DOS behavior.

158E:0006 starts a string at SS:BP-0138 with that byte and NUL, appends
`:\autoexec.bat` from DS:05C0 and calls1000:2C11 with mode `rt` at05CF.
It requires a nonzero DX:AX stream, then calls1BD4:02AA for `SW32.EXE`
at05D2. FND-CONFIG-265 supplies the forward case-folded marker search and
string concatenation contract; it neither rewinds the file nor imposes
a token boundary on this marker.

After a successful marker search the root saves1000:2D48's DX:AX as a
two-word position P. Each backwards-search iteration calls1000:2CC8 with
the stream, P and origin0, decrements P modulo2^32 and reads1000:2F81.
It discards the first returned byte by replacing its local with zero.
Later iterations stop on AL colon,FF or newline; FF/newline return AX0.
Thus its gate is byte-wide, unlike the marker helper's full-word FFFF
test. Tell/seek results are not validated by this root.

On the colon branch it computes P-1; a negative high word chooses seek
position zero, otherwise it passes P, with origin0. It reads a retained
byte and rejects ALFF, then consumes two more bytes without testing them.
It then reads into SS:BP-00E8 with index initially zero, increments the
index after each read and stops on newline or when the count exceeds80.
This loop does not reject FFFF separately. After the loop it rejects
exactly count80, not all counts at or above80. Other counts decrement
the index and replace that last stored byte with NUL. The loop can
reach count81; this reading does not normalize that behavior into a
conventional length check.

It calls1000:2873 on the first stream, ignoring the result, then scans
backwards from before that terminator until a backslash is found and
replaces it with NUL. There is no lower-bound check in that backwards
scan; an admitted preceding backslash and backing span are conditions,
not evidence that all input lines supply them.

The second path at SS:BP-0098 consists of the retained byte, `:\` at
DS:05DB, the truncated line string, `\` at05DE and `sw32.ini` at05E0.
It opens that path through1000:2C11 with `rt` at05E9 and requires a
nonzero DX:AX stream. No actual path or successful open is established.

### Ordered second-file fields

All searches below use the same second stream and the forward marker
helper. Each marker must return nonzero. Each listed following byte
must have AL other than FF; its full AX is not tested. Read bytes are
terminated locally with NUL before conversion.

| Search order | Following conversion |
| --- | --- |
| `[SB]` DS:05EC, then `Port=`05F1 | Read three bytes;2713 decimal-prefix conversion, retain AX and pass it to164C:0042. Require remapped word other than FFFF; retain as second-block address. |
| `Irq=`05F7 | Read one byte;2713 AX becomes second-block interrupt. |
| `Dma=`05FC | Read one byte;2713 AX becomes the shared transfer-channel value. |
| `[MIDI]`0601, then `Port=`0608 | Read three bytes;2713 AX through164C:0042, requiring non-FFFF; retain as first-block address. |
| `Irq=`060E | Read one byte;2713 AX becomes first-block interrupt. |

FND-CONFIG-261 supplies2713's whitespace/sign/prefix/modulo behavior;
FND-CONFIG-262 supplies0042's finite decimal-word-to-address remap.
These are not general hexadecimal text conversions. Section searches do
not add a boundary to later key searches: a key may be found farther
forward than its apparent section, under the helper's exact reset rule.

### Paired record writes and exits

After every stage succeeds, the root calls1000:2873 on the second stream
and ignores its result. Earlier failure branches return AX0 without
reaching this explicit close or the root's record stores. The first
stream's explicit close occurs only at the earlier line-acceptance point.
Later program cleanup and child effects are not excluded by these facts.

The root independently checks card82 in MainGroup and DigitalGroup via
17FB:0346/03A4. FND-CONFIG-259 supplies their membership/index contracts.
Each group uses a fresh01B0/020C index call and reloads its far pointer
before every store, with the low-word index*FE displacement:

| Record word | Value |
| --- | --- |
| EE | Remapped `[MIDI]` address |
| F0 | Converted `[MIDI]` interrupt |
| F2,F4 | FFFF |
| F6 | Remapped `[SB]` address |
| F8 | Converted `[SB]` interrupt |
| FA,FC | The same converted `[SB]` transfer-channel value |
| EC | 0001, stored last |

EC stores are158E:059C/06A9 and EE stores04BF/05CC. There is no own
MusicOnlyGroup or DS:ECCE assignment. Repeated group identity, pointers
and indices require preservation. After both conditional groups it
formats a message through1000:394E and copies it to DS:B93C through3A7A.
Only a nonzero caller argument at BP+06 adds the190F:0000 notice call.
It then returns AX1, even if neither group was present. These child
results are ignored locally; presentation/storage admission remains open.

## Interpretation

This assigns the last two EC/two EE literal-census writer sites to a
conditional path/field/record contract. Together with FND-CONFIG-259/260/
262/263/264/265, all25 EC writes and20 EE writes in the controlled
cohorts now have local assignments. This is not a census of every possible
state change: the59 retained walk gaps and computed, implicit, block,
alias, changed-stream and native access exclusions still apply.
Q-CONFIG-005 retains actual producers' admission/lifetime and game-side
consumers. No original or emulated call was run, no complete reading or
format-status promotion is claimed and parity remains unvalidated.

## Alternatives

The first search does not establish that a command is executable or even
active in the text file. Ignored seek/close results do not prove successful
positioning or release. A successful local return is not successful
hardware detection or proof that either group exists. The count80 branch
and unchecked backwards scan must remain explicit conditional behavior.

## How to reproduce

Use the verified read-only SETUP-blocks project in
GAME_DIR/analysis/config-block-construction-20261108/.
remaining-extended-producers-ghidra.log contains158E:0006;
card82-drive-ghidra.log contains1BD4:0318. Generate
card82-file-instructions.txt with resident_listing.py for
158E:0006..158E:06FF and1BD4:0318..1BD4:0344. Resolve Ghidra's inferred
DX values and line-array positions against actual far returns and operands.

card82-literals-query.json reads83 one-byte rows at fileFD20, the operand-
named DS:05C0..0612 interval. Result/repeat SHA256 both equal
39e51b7437b3369f364bcac5df08e1b58c1360f8ec26a57e2267cbf3c2692a8c.
Reuse the unchanged ec-query.json/ee-query.json controlled cohorts and
their documented positive/interior-negative controls, limits and exclusions.
Keep original-derived code, literals and reports only in GAME_DIR.
