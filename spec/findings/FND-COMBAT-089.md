---
id: FND-COMBAT-089
title: APFM child acquisition starts with a null destination and RESOURCE.GFF is the only provider in the inspected top-level archives
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0384..3A8E:03CD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0438..38FF:04AB
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1E5F4..0x6A737
tool: tools/research/exec-census/gff_tag_numbers.py and resident_listing.py; tools/evidence/xxh3.mjs at 93609f94
environment: null
---

## Observation

The window child loader in 3A8E:02B3 computes a child entry using the
window's byte +F2, word index times 30 and base +105. At 03A4 it pushes
a zero double word, the entry's double word +8 and tag double word +4,
then calls 38FF:0438. It stores returned DX:AX into the entry's +0 far
pointer and checks the entire pointer for zero before its tag dispatch.
FND-COMBAT-088 binds that dispatch's APFM constructor branch.

FND-CONFIG-208 reads 38FF:0438 completely: the third argument is an
optional length-output address, not an existing resource buffer. This
child call leaves it null. The wrapper initializes its own local destination
pointer to zero and supplies its address to 38FF:07B1 with start zero and
the complete-length sentinel. Thus the child caller supplies neither an
existing APFM object nor an expected 116-byte capacity to this wrapper.
FND-CONFIG-151 records the reader's null-destination allocation path,
archive fallback and seek/read result gates. A wrapper rejection returns
a null pointer; a zero reader result returns the resulting local pointer.
The child's null-result gate precedes constructor dispatch, rather than
passing the previous child pointer through on failure.

The directory census with tag APFM covers exactly the 26 top-level GFF
files in the licensed installation. Their XXH3-128 values match their
entries in BLD-GOG-EN-1.1.files.yaml, including the initially small working
archives. RESOURCE.GFF is the sole provider and has 97 APFM resources,
matching the independently recorded count in FND-UI-001. The other 25
have no APFM table. The inspected set is:

- CHARSAVE.GFF, DARKRUN.GFF, DARKSAVE.GFF, GPLDATA.GFF, OBJEX.GFF
  and RESOURCE.GFF;
- the 20 RGN archives with suffixes 001, 032, 033, 034, 036, 037,
  038, 039, 03A, 03B, 03C, 03D, 03E, 03F, 041, 042, 043, 044, 045
  and 0FF.

The census has positive controls for both directory encodings. GFFI
returns 5 resources in GPLDATA.GFF, 7 in OBJEX.GFF and 20 in
RESOURCE.GFF, with 23 archives lacking that table; these are the plain
first tables documented by FND-GFF-003. PORT returns 178 resources in
GPLDATA.GFF and none in the other 25, matching its independently
documented indexed ranges in FND-GFF-003 and FMT-GFF-006. The APFM
positive count also matches its indexed RESOURCE.GFF table. No payload
bytes were executed or committed.

## Interpretation

For this inspected build snapshot, the other 25 top-level archive files
cannot supply an APFM table to the child lookup. Successful acquisition
of an APFM from the inspected RESOURCE.GFF has the header and initial
zero fields recorded in FND-UI-001 and FND-UI-005. The wrapper starts
with a null local destination, rather than intentionally requesting reuse
of an already modified APFM object. Actual archive selection, allocation,
transfer success and storage identity remain necessary to bind that source
to a live record.

## Alternatives

The disk census excludes the disc image, nested directories, numbered
saves, external files and later modified archives. FND-UI-001 independently
compares the disc's 97 APFM payloads with the installed provider, but
that comparison does not extend this census to every disc file or to a
runtime archive list. A dynamically registered or modified archive remains
an admission question, not something excluded by these 26 directory reads.

FND-CONFIG-208's signature and local-result gates do not validate a
payload tag or prove pointer validity. FND-CONFIG-151 retains allocator,
directory, offset, I/O and alias conditions. The zero local pointer rules
out caller-directed buffer reuse on this call form, not every possible
allocator alias or later copy. Q-COMBAT-049 retains those conditions and
the other live-record producers and writes. No whole-load callback
exclusion or complete-reading declaration follows.

## How to reproduce

Read resident 3A8E:0384..3A8E:03CD and 38FF:0438..38FF:04AB with
resident_listing.py on the licensed installed DSUN.EXE, using the parent
entry and APFM dispatch already located by FND-PARTY-044. Bind the zero
third argument through the wrapper's actual far-pointer accesses and track
the independent local destination. Reuse FND-CONFIG-208 and
FND-CONFIG-151 for their complete wrapper and reader contracts.

Run gff_tag_numbers.py on the installation directory with APFM, GFFI and
PORT separately. Its nonrecursive *.GFF scope must be exactly the 26
names above. Hash those files with node tools/evidence/xxh3.mjs and
compare with BLD-GOG-EN-1.1.files.yaml before reusing the negative result.
Keep modified working archives or additional files as a changed input,
not as the same census. Compare the positive counts with FND-UI-001
and FND-GFF-003, rather than accepting an empty search without controls.
