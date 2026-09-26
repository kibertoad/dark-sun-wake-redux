---
id: FND-ACTOR-007
title: RDFF resources share numbers with OJFF resources outside 9,000 to 13,998 and fall into three size families
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x24274..0x66105E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..6237:0000
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`OBJEX.GFF` holds 1,643 `RDFF` resources, all in the range located above. None is numbered from
9,000 to 13,998. Compared with the 4,479 `OJFF` numbers (FND-ACTOR-001):

- 1,589 numbers have both an `RDFF` and an `OJFF` resource;
- every `OJFF` number outside 9,000 to 13,998 has an `RDFF` resource of the same number;
- the 2,890 `OJFF` numbers from 9,000 to 13,998 have none;
- 54 `RDFF` numbers have no `OJFF` resource.

The sizes fall into three groups:

- 1,216 resources of 68 bytes;
- 74 of `43 + 33 * n` bytes, `n` from 0 to 7: 43 (61 resources), 76 (4), 109 (1), 142 (1),
  175 (3), 208 (2), 241 (1) and 274 (1);
- 353 of `145 + 33 * n` bytes, `n` from 0 to 18: 145 (220), 178 (12), 211 (12), 244 (6),
  277 (45), 310 (18), 343 (25), 376 (5), 475, 508, 541, 574 and 640 (1 each), 673 and 706 (2 each)
  and 739 (1).

The label at `OBJEX.GFF#ALL/2` offset 3,611, which a Look panel in the opening region shows,
occurs followed by a NUL in 23 `RDFF` resources, each time at
offset 43: numbers 107, 200 to 213, 365 to 367, 370, 371 and 501 to 503. Six of them are 277 bytes,
six 310 and eleven 343. `OBJEX.GFF#RDFF/107` is at offset `0x659FD1`. No other resource of
`OBJEX.GFF` holds it; `OBJEX.GFF#ALL/2` is at offset `0x18E5F` and is 18,522 bytes. The label's six
bytes do not occur in `DSUN.EXE`, in any mix of upper and lower case.

## Interpretation

An `RDFF` resource holds data for the object of the same number, and the objects numbered 9,000
to 13,998 have none, which fits the range test before each request (FND-ACTOR-003,
FND-ACTOR-005). The 43-byte and 145-byte families may be a header followed by 33-byte records,
with a name at offset 43 in the larger family.

## Alternatives

Sizes that divide this way and a repeated text offset are patterns in the data; they do not show
how the game reads the resource, and no code that reads an `RDFF` payload has been found. The
68-byte resources may be a separate format.

## How to reproduce

Read the `RDFF` and `OJFF` numbers and sizes from the directory of `OBJEX.GFF` (FMT-GFF-001),
compare the number sets, group the sizes, and search every resource for the label's bytes.
