---
id: FND-EXE-482
title: Sound utility string conversion dispatch uses normalized classification and signed default width
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0F3F..1000:0F4C
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0FB3..1000:0FFE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:11C7..1000:1204
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:128B..1000:1348
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D608..0x0001D609
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x000027DA..0x000027DC
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

After a non-doubled percent marker, the formatter retains its output cursor
in SS:BP-4, sets CX zero, extra-count word BP-0E zero, flags word
BP-2 to 0020, prefix byte BP-0B zero, and width and precision words
BP-8 and BP-0A to FFFF. It zero-extends the next format byte into AX,
copies it to DX and BX, subtracts 20 from BL, then rejects normalized
BL values at least 60 unsigned. Otherwise it loads BL from current
DS:[BX-21AB]. BH remains zero. A resulting unsigned BX above 17
selects fallback 1384; otherwise twice BX indexes the word table at
CS:13B8. This bounds the dispatch index, not the classification allocation.

For byte 73, normalized BX is 0053 and the classification address is
DS:DEA8. Under admitted DS 1E36, shipped byte 1D608 is 11.
Shipped word 27DA..27DC is 11C7, so that conditional classification
selects the string branch. Using the unnormalized byte as the classification
index would read a different byte and does not describe this dispatch.

The string branch saves the format continuation offset in BP+6 and
the conversion byte in BP-5, then takes the argument offset from BP+4.
Flag 0020 clear reads one offset word through SS and uses current DS
as source segment, advancing the argument offset by two. Flag set loads
the far pair through SS and advances by four. A zero offset in the near
case, or an all-zero pair in the far case, substitutes current DS:DE4E;
the substitute's content and extent are not admitted here.

Near helper 0F3F preserves DI, sets CX FFFF and AL zero, scans ES:DI
with repeated byte comparison, complements CX and decrements it, then
restores DI. It reports no exhaustion separately. With an admitted stable
terminator among 65535 scanned bytes, the resulting CX is its preceding
byte count, from zero through FFFE. With all scanned bytes nonzero it
also returns FFFE. Offsets wrap without segment adjustment. The string
branch compares that CX with precision word BP-0A unsigned and reduces
CX to precision only when larger, then joins 128B.

For the plain percent-s path with default state and nonaliased frames,
128B takes the source cursor into SI, restores the output cursor into DI
and width FFFF into BX. Default flags skip alternate-prefix selection.
Extra count zero is added to CX. The leading-space loop compares BX
and CX signed and emits spaces while BX is greater, decrementing BX.
With source count N below 8000, signed BX minus one is not greater,
so it emits no leading spaces. With N from 8000 through FFFE it emits
FFFF minus N spaces, finishing BX equal to N. This is a conditional
consequence of the default sentinel, not a conventional width guarantee.

Default flags skip the added prefix and extra-zero paths. Nonzero CX
is subtracted from BX, then exactly CX source bytes are copied into
SS:DI with the local flush sequence described by FND-EXE-481; the
terminator is not among these source bytes. A positive signed remaining
BX emits that many trailing spaces. In the default cases above there
are none: BX is negative after the subtraction for N below 8000, or
zero for N at least 8000. Zero count skips copying and has no padding.
The path then reloads the saved format pair at 0F95. Callback preservation,
source stability and nonaliasing are necessary for these count consequences.

## Interpretation

This conditionally binds FND-EXE-480's plain conversion marker to far
argument consumption and a bounded scan whose exhaustion is ambiguous.
The fixed scan cap is not a source allocation bound or a terminator check.
Q-EXE-007 retains actual DS, classification and pointer producers, substitute
content, frame and source/destination extents, aliases, callback-state lifetime,
other modifiers and conversion branches, and the optional consumer.
No complete formatter reading, valid output or launch exclusion is claimed.

## Alternatives

An unnormalized classification lookup selects the wrong table byte. Treating
FFFE as a proven length ignores exhaustion. Treating FFFF width as always
absent ignores the signed comparison for counts at least 8000. Treating
the scan cap as an output bound overlooks padding and repeated conversions.

## How to reproduce

At revision 5b4bcd0 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Use Capstone 5.0.7 in sixteen-bit mode, MZ header size 1400 and
modeled load segment 1000. Decode shipped half-open ranges
233F..234C at IP 0F3F, 23B3..23FE at IP 0FB3,
25C7..2604 at IP 11C7 and 268B..2748 at IP 128B, CS 1000.
Use conversion byte 73, subtract 20 before classification, bind conditional
DS 1E36 and inspect shipped 1D608 and dispatch word 27DA..27DC.
Track the unsigned precision and signed width tests separately, default flags
0020, width/precision FFFF and extra count zero. Check symbolic source
counts 0000, 7FFF, 8000 and FFFE, and distinguish scan exhaustion from
a terminator at the final scanned byte. Licensed bytes stay outside Git;
no original process, DOSBox or emulated call runs.
