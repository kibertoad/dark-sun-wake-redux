---
id: FND-ITEM-092
title: Known bitmap backend callbacks have distinct success mappings rather than a uniform complete-transfer guarantee
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0009..1425:0107
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:02E1..1425:0329
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:041A..1425:0462
tool: Composition of FND-EXE-391/395/396/397/398/399/584 with FND-ITEM-090/091 at bf7eb80e; bounded resident_listing.py cross-checks, Python 3.12.10, Capstone 5.0.7
environment: null
---

## Observation

Q-PARTY-116 retains installed backing-store callback targets and effects.
FND-ITEM-090 supplies the concrete chunk calls through DS:3F42/3F46.
The existing executable findings already read the known target producers
and local callback contracts. This composes those answers; it does not
repeat their native-service research or claim exhaustive runtime writers.
Addresses, offsets and result codes below are hexadecimal.

### Known target families and registration limit

FND-EXE-584 identifies four producer families for 14-byte backend records.
Each writes read pointer +0, write pointer +4 and cleanup pointer +8,
with an argument word at +0C. Segment operands are MZ relocations.
Their immediate-displacement writer search does not exclude block copies
or stores through a different base address, and its initial zero records
do not establish later registration or a valid selected backend index.

| Known read/write pair | Existing local contract | Result limit |
| --- | --- | --- |
| 1425:00DB / 00AF | FND-EXE-391/399, interrupt-67 prefix, forward word/odd-byte copy and final request | Wrapper zero requires all reached native AH tests zero; nonzero maps to 0006. |
| 1425:0009 / 0044 | FND-EXE-398, DOS seek followed by read/write wrapper | Carry-mapped result becomes zero or 0005; native transferred count is discarded. |
| 1425:02E1 / 02FE | FND-EXE-395, 15F3:0423/04DA and interrupt-15 move helper | Callee zero or 0007 passes through; carry, not native AX, selects success. |
| 1425:041A / 0437 | FND-EXE-396/397, 15F3:06B6/076B and the installed driver entry | Every reached move requires returned AX exactly one; failed test maps to 0008. |

The pointer table in FND-EXE-584 is a finite set of known producers,
not proof that every live call selects one of those intact pairs. Reading
their code does not settle registration completion, subsequent writers,
selected handle/block identity or preservation by native services.

### Consequences for bitmap transfer success

The mapped-copy pair can return 0006 after its explicit buffer copy when
the final native request fails. An earlier failed prefix skips that local
copy. Forward copy bounds come from the captured count; no local disjointness
test makes them overlap-safe. Earlier native effects are not rolled back.
FND-EXE-399 retains the request-to-request DX preservation obligation.

The file pair skips all native work for zero callback count. A nonzero
count requests positioning, ignores that result and then requests the
transfer. The native read/write wrappers discard AX's returned count and
produce one minus carry; the callbacks map that to zero or 0005.
Consequently callback zero does not itself prove successful positioning
or completion of every requested byte. SI/DI held across the positioning
request still require the preservation described by FND-EXE-398.

The interrupt-15 pair has separate odd-byte scratch paths and bulk moves.
The shared helper's zero count makes no native request. Its nonzero path
has explicit descriptor initialization, word-count arithmetic and native
carry tests; the outer routines' zero result does not validate descriptor,
scratch, buffer or native-mapping state. A later failure can follow earlier
staging or native writes. FND-EXE-395 retains those complete local branches.

The driver-entry pair writes request-record fields before every count guard.
Zero bulk count can return zero with those writes retained and no move.
Odd-byte staging can issue multiple moves and write scratch or buffer bytes
before a later exact-one test fails. Untouched high count words, record
producers, installed entry identity and per-call preservation remain explicit
in FND-EXE-397; final saved-register restores do not settle intermediate use.

FND-ITEM-090 propagates the first nonzero callback result and advances
its remaining count by the requested chunk on zero. None of these local
success mappings supplies a separate completed-byte count for that loop.
The provider/reload result handling in FND-ITEM-086/087 must therefore
remain conditional on the selected family's native content outcome.

## Interpretation

This removes the known backend families' local callback identities,
argument/result mapping and failure-publication behavior from Q-PARTY-116's
unread-child obligations. Remaining evidence concerns complete writer/
registration lifetime and active selection, native service/driver outcomes,
per-call preservation and valid buffers/metadata/scratch. The existing
findings retain their original narrower areas and open obligations; this
composition neither supersedes them nor closes their research questions.

## Alternatives

Treating all zero callbacks as complete transfers erases discarded counts
and unchecked positioning. Treating all errors as no mutation erases
post-copy failures and staging writes. Treating known producers as an
exhaustive live target census exceeds FND-EXE-584's exclusions. No original
run, native failure, complete-reading declaration or parity promotion follows.

## How to reproduce

Use the independently worded contracts and reproduction procedures in
FND-EXE-391/395/396/397/398/399/584 and FND-ITEM-090/091. Their licensed
input is BLD-GOG-EN-1.1 DSUN.EXE, length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Check each producer's relocated pair
against its callback argument order and result consumption, then bind that
mapping to 1425:1AA1/1C58. Existing reports remain in their findings' local
stores. This result depends on those retained readings, not on a new
negative writer search or a newly assumed runtime backend.
