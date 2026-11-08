---
id: FND-CONFIG-265
title: Card81 reads ordered SNDSCAPE file fields before paired extended-record writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 15FD:000F..15FD:04F8
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1BD4:0244..1BD4:0256
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1BD4:0274..1BD4:0318
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3A0B..1000:3AA3
tool: Ghidra 12.1.3 ReportFunctionSummary.java; resident_listing.py; tools/evidence/report.mjs table
environment: null
---

## Observation

Q-CONFIG-005 retains the file-based producers beyond FND-CONFIG-264.
This reads the complete local card81 root and its reached string, open,
close and marker wrappers. Source identity is SOUND_DS.EXE, 204593 bytes,
XXH3-128 236c2dc23c071eca421eb5b427caee57. Offsets and record values below
are hexadecimal; character counts are decimal. Contracts require admitted
far pointers, backing spans and preservation across called functions.
Native file delivery and installed source values are not demonstrated.

The root calls FND-CONFIG-262's1000:307B source-vector lookup with the
case-sensitive name `SNDSCAPE` at DS:066E. It tests the full DX:AX pointer;
zero returns AX0. Otherwise it copies that value to SS:BP-0082, appends
the backslash at DS:0677 and `sndscape.ini` at DS:0679, then calls
1BD4:0274 with that path and mode `rt` at DS:0686. The wrapper forwards
both far arguments to1000:2C11 and returns its DX:AX unchanged; its
zero/nonzero branch does not choose a different result. A null result
returns AX0 before any record assignment.

The path copy1000:3A7A and concatenation3A0B clear direction, locate NUL
with a maximum FFFF scan count and copy through the located terminator
under admitted terminated strings. Concatenation replaces the destination
terminator. Both return the destination far pointer and restore DS/SI/DI/BP;
neither receives a destination capacity or rejects an oversized path.
Actual source/path span, overlap and wrapping therefore remain conditions.

### Marker helper and successive fields

1BD4:02AA receives a stream and a terminated marker. Its current marker
pointer initially equals the supplied start. An empty current suffix
returns AX1 without another read. Otherwise it calls1000:2F81; full AXFFFF
returns AX0. It applies1000:0EC9 independently to the returned word and
the sign-extended marker byte, compares their words, and increments the
marker offset only on equality. A mismatch resets the marker pointer to
the start, without rereading the mismatching byte as a potential first
character. It neither seeks nor rewinds. Thus it is a forward,
case-folded contiguous-marker search with this reset rule, not a general
overlap-preserving substring algorithm. FND-SOUND-020 supplies0EC9's
classification contract; FND-SOUND-030 supplies the byte reader's
conditional unsigned-byte/FFFF results.

The root invokes that helper four times on the same stream, in this order:

| Marker and source location | Following reads and conversion |
| --- | --- |
| `Port=` DS:0689 | Read three bytes and append NUL. Exact compare against `320`, `330`, `340`, `350` at DS:068F/0693/0697/069B selects word0320/0330/0340/0350 respectively. No match returns AX0. |
| `IRQ=` DS:069F | Read two bytes. If the second is ASCII `5`, keep both and append NUL; otherwise terminate after the first. Convert with1000:2713, retaining AX as the first-block interrupt value. |
| `DMA=` DS:06A4 | Read one byte, append NUL, convert with2713 and copy AX into both local transfer-channel words. |
| `SBIRQ=` DS:06A9 | Read one byte, append NUL and convert with2713, retaining AX as the second-block interrupt value. |

Each marker result must be nonzero. Each following byte read is stored
from AL and compared as AL against FF, not as AX against FFFF. Therefore
ordinary byte FF and the reader's FFFF result both take this root's
failure path. This differs from the marker helper's full-word test.
Other byte values are not checked for numeric syntax. FND-CONFIG-261's
decimal-prefix/sign/modulo contract applies; a no-digit result may be zero.
The root consumes both IRQ characters even when it discards the second.
Its special `5` rule does not mean arbitrary two-digit interrupt values
are admitted as two-character strings.

1000:3A4A, used for the four port comparisons, clears direction, scans
the second argument to NUL within FFFF iterations and compares the first
argument against that span including NUL. With these admitted literals,
AX0 means exact equality. It returns the difference of zero-extended
bytes at the stopping position and restores DS/SI/DI/BP. No case folding
or numeric conversion is involved in these comparisons.

### Publication and local exits

Only after all four field stages succeed does the root call1BD4:0244.
That wrapper forwards the stream to1000:2873; the root ignores its result.
All earlier failure branches return AX0 without reaching that explicit
close or any of the root's record stores. This establishes local call
order, not the absence of later program cleanup or child side effects.

The root then independently tests card81 membership in MainGroup via
17FB:0346 and DigitalGroup via17FB:03A4. FND-CONFIG-259 supplies those
helpers and the index helpers01B0/020C. For each admitted group it calls
the corresponding index helper again before every store, reloads the
group far pointer and uses the low-word product of index and FE:

| Record word | Value |
| --- | --- |
| EE | Selected port word0320/0330/0340/0350 |
| F0 | Converted `IRQ=` result |
| F2, F4 | FFFF |
| F6 | Fixed0220, initialized before source lookup |
| F8 | Converted `SBIRQ=` result |
| FA, FC | The same converted `DMA=` result |
| EC | 0001, stored last |

The MainGroup EC store is15FD:039B; DigitalGroup EC is04A8. Their EE
stores are02BB/03C8. Stable target identity across repeated lookups is
a preservation condition. There is no own MusicOnlyGroup or DS:ECCE
assignment. Even when neither group is present, the admitted continuation
formats a message through1000:394E, copies it to DS:B93C and calls190F:0000
before returning AX1. Those children's results are ignored locally;
their storage and presentation effects remain conditional.

## Interpretation

This assigns two EC and two EE controlled-census sites to card81's local
file parser and paired stores. Q-CONFIG-005 retains two of each in the
card82 family, excluded computed/implicit/block/alias accesses, actual
source/path/stream/group admission and downstream behavior. Ordered marker
searches do not establish a freely reordered INI-key reader or successful
native detection. No original or emulated call was run, and no complete
reading, format-status promotion or parity validation is claimed.

## Alternatives

A source name match is not a successful open. Case folding in marker
search does not change the case-sensitive source-vector name lookup.
Ignoring the explicit close result does not establish persistence of a
stream or a successful release. Treating local AX1 as populated records
contradicts the independent group-absence branches.

## How to reproduce

Use the verified read-only SETUP-blocks project under
GAME_DIR/analysis/config-block-construction-20261108/.
remaining-extended-producers-ghidra.log contains15FD:000F;
card81-file-helpers-ghidra.log contains the six helper summaries.
Generate card81-file-instructions.txt with resident_listing.py for
15FD:000F..15FD:04F8,1BD4:0244..1BD4:0318 and1000:3A0B..1000:3AA3.
Inspect AL-versus-AX branches and actual far argument pairs directly.

card81-literals-query.json reads66 one-byte rows from fileFDCE, the
bounded DS:066E..06AF interval named by the literal operands above.
table result and repeat SHA256 are both
f55a07a38e8239328285cdaa649e11ce3344ab61be1c959d2bc78ae21929d039.
Reuse the unchanged ec-query.json/ee-query.json controlled cohorts and
their exclusions from FND-CONFIG-259/260/262/263. Retain all original-derived
reports and literal data only in GAME_DIR.
