---
id: FND-EXE-133
title: Physical byte-indexed prefix lookup contributes four for even set-bit counts and zero for odd counts
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    offset: 0x002F1770..0x002F1970
    kind: file-data
tool: Identity-verified bounded physical PE word queries and exhaustive byte-domain arithmetic comparison
environment: null
---

## Observation

FND-EXE-132 records a zero-extended sixteen-bit lookup at
`0x006F2B70` plus twice byte B in the selected prefix helper. Its
local byte width bounds B to zero through 255. The same verified shipped
PE maps that entire 512-byte table to the half-open physical range
`0x002F1770..0x002F1970`, with every two-byte word contiguous inside
file-backed data. The last covered byte is `0x002F196F`.
The final word starts at loaded address
`0x006F2D6E` and shipped offset `0x002F196E`.

Reading all 256 little-endian words gives only values zero and four.
Every word agrees with this complete domain rule: count the set bits in
the eight-bit index B; an even count selects four and an odd count selects
zero. Both classes have 128 indices. The zero count is even, so B zero
selects four. B 255 also selects four; B 127, 128 and 254 select zero.
These are physical table values, not an assumption about a familiar
processor flag convention. The exhaustive comparison includes every
admitted index rather than a few matching samples.

### Byte-indexed contributions

The complete values are in
[FND-EXE-133.byte_mask.csv](FND-EXE-133.byte_mask.csv), with one row for
each ByteValue and its MaskContribution. The CSV lists the same arithmetic
rule in numeric form, not executable bytes, instructions or a runtime
fixture. Its contributions are the zero-extended word T used by
FND-EXE-132's full-mask OR. It does not define other mask bits or replace
the width-specific zero/high-bit contributions described there.

All listed values concern the shipped file. This query does not prove
that no runtime writer changes the loaded table, that the selected helper
sees every possible index, or that its mask implements a complete named
machine-state contract. FND-EXE-132 separately records that its full/word
branches reread the low input byte while its byte branch retains it.
The observed contribution rule does not erase that distinction or the
remaining lifetime/concurrency conditions.

## Interpretation

The previously symbolic lookup contribution now has complete physical
value evidence over its byte domain. Combined with FND-EXE-132 it
supports the stated local mask arithmetic for the shipped lookup values,
without assigning broader semantic flag names or promoting the containing
format entry. Q-EXE-009 in FMT-EXE-006 remains open for input/mask/selector
and table writers, other prefix branch effects, publisher indirect targets,
remaining selected-callee paths and storage lifetime. This is not a
complete helper reading or actual PATH behavior.

## Alternatives

- Index zero selects four, not zero; the contribution follows even
  set-bit count rather than simply testing whether the byte is nonzero.
- A byte with its high bit set is not by itself sufficient: 128 selects
  zero while 255 selects four. Its high-bit contribution is separate.
- Agreement of every shipped word with the stated arithmetic rule does
  not establish loaded-table immutability or a complete semantic flag model.
- Matching representative values alone would not cover the table; the
  complete 256-index domain was read and compared here.

## How to reproduce

Verify FND-EXE-011's executable length and XXH3-128 identity. Recheck all
six full physical byte/word controls in FND-EXE-099 before relying on the
PE image-base and file-backed section mapping. FND-EXE-132 independently
supplies the lookup base, index width and stride. For each index zero
through 255, map `0x006F2B70` plus twice the index to shipped raw
file data, require both bytes contiguous and inside the file, and decode
a little-endian sixteen-bit value. Reject identity or mapping changes.

Independently count the index's eight set-bit positions and compare the
physical value with four for an even count or zero for an odd count.
Require no mismatches over the entire domain and require the CSV's indices
and contributions to agree with that domain rule. Check physical endpoints
and indices zero, one, two, three, 127, 128, 254 and 255 as explicit
controls. This is static file reading and arithmetic checking, not execution
of the original or a substitute helper. Keep raw query output local;
no native or emulated execution is part of this observation.
