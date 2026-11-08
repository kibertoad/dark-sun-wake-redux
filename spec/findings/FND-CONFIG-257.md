---
id: FND-CONFIG-257
title: SOUND.CFG constructors distinguish DMA words from flag counts and copy packed flags rather than a music resource number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND.CFG
    offset: 0x00..0x3B
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1429:042D..1429:077C
tool: Ghidra 12.1.3 ReportFunctionSummary.java; retained resident_listing.py reports from FND-SOUND-020/047; node tools/evidence/xxh3.mjs
environment: null
---

## Observation

This replaces FND-CONFIG-003, preserving its installed-file observations
but correcting its inferred flag-count and music-chunk interpretations.
Current source identities match the earlier records: SOUND.CFG is59 bytes,
XXH3-12864d376205212cb8a6b8a782b88576327; SOUND_DS.EXE is204593 bytes,
XXH3-128236c2dc23c071eca421eb5b427caee57. Offsets/addresses below are hex;
plain sizes and field values are decimal unless prefixed.

The installed file has two equal ten-byte blocks at00 and0A, each containing
five little-endian words:0x220,5,1,1,122. Word14 is11. Fields16 and24
occupy fourteen bytes each and contain the matching real-mode MIDI and
digital driver names followed by four NULs. The nine-byte tail32..3B has
words1,4,8,11 and final byte0. The disc has no SOUND.CFG; setup writes it.
The matching SOUND.INI records have card ID122, first MIDI/digital address
0x220, IRQ5, DMA1 and flag count1, with MIDI chunk11 and digital chunk8.
Equality of values does not identify a writer or distinguish the meanings
of two fields containing1 or11.

### Full constructor's two settings branches

The complete local1429:042D..063A body tests selected-record word+EC.
Before any child call it assigns all eleven output words00..14 as follows.
The output buffer begins at DS:E01B; every table entry is a source-record
word offset. The source pointer is reloaded from the far argument for each
read. These are actual operands and stores, not inferred device labels.

| Output offset | Record+EC zero | Record+EC nonzero |
|---|---|---|
| 00 | 2E | EE |
| 02 | 5A | F0 |
| 04 | 86 | F2 |
| 06 | 86 | F4 |
| 08 | 28 | 28 |
| 0A | 44 | F6 |
| 0C | 70 | F8 |
| 0E | 86 | FA |
| 10 | 86 | FC |
| 12 | 28 | 28 |
| 14 | DC | DC |

FND-SOUND-037 independently binds record2E to MidiAddress,5A to MidiIrq,
44 to DigitalAddress,70 to Irq,86 to Dma,28 to CardId andDC to the packed
Flags bits. Its Flags count is a local loop value, not record86 orDC.
The nonzero-EC branch's EE..FC fields need their own producer admission;
this finding does not infer their meaning from the zero-EC alternative.

Both branches next OR bit02 into output14 when DS:ECCA bit02 is set,
and bit08 when DS:ECCA bit08 is set. Other output bits are retained.
Thus output14 comes from packed flags plus these two override bits, not
the record's MIDI driver chunk atD8. Both bodies then select the already
read name-copy alternatives using DS:ECCC equal to1 versus other values,
and converge on the tail stores in FND-SOUND-020/047. They copy CardGroup,
literal4, digital chunk, MIDI chunk and the playback low byte to32..3A.
No local error test follows a name-copy call; the body returns far.

### Partial digital constructor

The complete local1429:063A..077C body uses the same EC split. It writes
only output0A..14 from the corresponding second-block table above, with
the same flag overrides; it leaves output00..08 unassigned locally. It
copies digital chunk to36 and playback low byte to3A, then only the
digital name at24 according to the same mode word. It does not locally
assign the music name, CardGroup, literal4 or MIDI chunk tail fields.
Each of its two branches makes one name-copy call and returns far.

Ghidra's decompiler and cross-references agree with the retained bounded
instruction listings at the deciding EC, word-width, source-offset, store
and override instructions. Both functions have complete local bodies;
the report lists ordinary incoming sites1429:0363/0786 for the full
constructor and03F1/0792 for the partial one. This is no all-entry census.
The decompiler's inferred pointer types and unrelated data labels are not
used as evidence; far LES reads and actual DS stores decide the mapping.

## Interpretation

On the admitted zero-EC branch, the fourth word of both settings blocks
duplicates Dma. It is not the matching record's flag count, despite the
installed values both being1. Output14 is a flags word whose installed
value11 also happens to equal the MIDI chunk. Actual MIDI chunk output is38.
This corrects FND-CONFIG-003 and FMT-CONFIG-001 without changing the59-byte
layout. The independent name/tail observations in SOUND-020/037/047 stand.

The local two-block and partial-block construction obligation of Q-CONFIG-005
is answered under valid disjoint, nonwrapping argument/output/stack storage,
retained DS and admitted name-copy behavior. Which source selection reaches
the routines, EC and override producers, EE..FC meanings, incoming storage,
aliasing and preservation through children remain open. Neither a successful
file write nor native driver installation follows from these local stores.

## Alternatives

Equal installed words alone supported the former flag-count/chunk readings;
the direct constructor operands distinguish them. Calling the two blocks
music and digital does not by itself establish their receiving driver API
or every field's purpose, particularly the nonzero-EC record alternative.
Ghidra's import emits unrelated parser warnings; the two reported constructor
bodies decompile successfully and their deciding instructions agree with the
retained listing. No game, setup executable or emulated function was run.

## How to reproduce

Verify the two source hashes with node tools/evidence/xxh3.mjs. Reuse the
FND-SOUND-020 selected-record listing and FND-SOUND-047 handoff report under
GAME_DIR/analysis/disc-adv-survey-20261104/. The handoff report SHA-256 is
590d6f8264f439205d4e488de7a13c0411bdbdac2e7672e550ddd244da803dad.
The new private Ghidra project and constructors-ghidra.log are under
GAME_DIR/analysis/config-block-construction-20261108/. Import the verified
SOUND_DS.EXE with Ghidra12.1.3 and run packaged ReportFunctionSummary.java
for1429:042D and1429:063A; compare branches and word/byte operands with
the retained full local listings. Keep all original-derived output local.
