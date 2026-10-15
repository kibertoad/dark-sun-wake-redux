---
id: FND-RNG-019
title: Startup constructs the environment pointer array and separately schedules the timezone initializer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:0059
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:009F..1000:00D2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:013C..1000:015D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0220..1000:0264
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:29AA..1000:2A07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2172..1000:21D2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00050994..0x000509BE
    kind: file-data
tool: tools/research/exec-census/immediate_search.py, resident_listing.py, direct_callers.py and far_pointer_words.py; tools/evidence/report.mjs table; installed-build identity and MZ relocation validation
environment: null
---

## Observation

The bounded operand-candidate query for the clock's storage displacements
recovers a DS:3920 writer at 1000:29DA and a following read at 1000:2A00.
They belong to the contiguous near-return constructor 1000:29AA..2A07.
Its entry is established by the startup initialization record, rather
than by decoding around the candidate alone.

### Startup records and ordering

The established MZ entry caches its relocated data-segment literal in
CS:02C4. Before initialization dispatch it uses that cached segment as
ES and clears bytes 39C4..A4EC. This bulk write includes both shipped
timezone-label spans A4E4..A4E8 and A4E8..A4EC, but not the pointer words
3976 and 3978 that FND-RNG-018 reads. The clear also covers the graph
selector-table window discussed in FND-PARTY-218; shipped zeros in that
window alone are not a completed runtime binding.

Startup passes ES offsets 3994 and 39BE to 1000:0220 before its far call
to game main. The dispatcher walks six-byte records, selects the lowest
unconsumed priority, marks the selected record's mode byte FF and calls
its target. Equal-priority candidates retain the first encountered record.
Mode zero calls a near offset; nonzero calls the full far pointer. ES is
saved around the call and restored before another scan. This reading
does not assume every callback preserves the other bounds/registers.

The shipped 42-byte window contains seven records. At DS:39B2 the mode
is zero, priority 16 and target offset 29AA. It therefore supplies a
near constructor entry. At DS:39B8 the mode is one, priority 30 and
target offset 3B56. The latter target's segment word is an MZ relocation
at shipped offset 509BC, resolving to segment 1000 at the analysis load
base. Thus the timezone initializer is also a stored startup target,
independent of the direct converter calls already recovered.

The direct-call query has no direct hit for 29AA, but that is not an
absence claim: the reached dispatcher and actual record supply its
entry. Known allocator call sites at 29B3 and 29D3 are positive controls
for that query. The stored-pointer query independently recovers the
relocated 3B56 pointer at shipped offset 509BA. Candidate decoding of
the record bytes is not treated as executable code.

### Environment construction

Early startup captures the initial DS word at offset 002C into the
new data segment's word 008C. It scans that source segment from offset
zero for NUL-separated strings followed by an extra NUL. The scan starts
with budget 32767; reaching zero budget branches to the startup failure
path. On the accepted path it writes the consumed length plus the extra
terminator to DS:008A. It derives DS:008E from twice the counted strings
plus eight, rounded down to a multiple of eight. Source-segment admission
and effects of the intervening startup calls remain open.

The constructor performs these local steps:

1. It requests DS:008A bytes from 1000:2172 and tests the returned AX.
   Zero branches to the startup failure path without reaching the
   constructor's explicit DS:3920 assignment.
2. On a nonzero return it retains that near destination, makes ES equal
   to its current DS, switches DS to the stored source segment, clears
   the direction flag and copies the requested bytes. It restores DS
   before continuing. The original requested count is popped into CX
   before the copy, rather than assuming the allocator preserved CX.
3. It requests DS:008E bytes from the same allocator. It saves and
   restores ES around this call. It stores the returned AX into DS:3920
   before testing for zero. A zero return therefore leaves that root
   word zero before branching to the failure path; the prior payload
   copy has already occurred. No rollback appears in this local body.
4. On success it walks the copied payload through ES. For each nonempty
   string it writes its near start pointer into the array through DS,
   advances the array by two bytes and scans to that string's terminator.
   An empty initial payload writes no member pointer. The final array
   word is zero in either case. There is no separate per-member length
   stored in the array.
5. It restores SI and DI, copies DS:3920 into DS:0088 and returns near.
   Startup subsequently pushes DS:0088 before the game-main call.

The allocator's own entry/exit preserves SI and DI. Its deeper heap,
growth and failure effects have not been completely read here. The
constructor consumes its low-word returns as near offsets; requesting
a byte count alone is not proof of every allocator capacity or alias.

For a stable properly terminated source, accepted scan length is at
most 32767 bytes including its extra terminator. Each nonempty string
uses at least two bytes, so at most 16383 member pointers are needed.
The computed table request fits a word and can hold those pointers plus
the zero terminator. For the empty two-NUL source it requests payload
length two and table length eight, and writes an empty pointer array.
These are conditional arithmetic bounds, not observed admission of
every source or allocation-success path.

## Interpretation

The clock lookup's DS:3920 root has an identified startup producer and
near-pointer-array construction path. The label buffers have an explicit
startup bulk-zero producer. The timezone initializer has an additional
stored caller in the startup records. These remove those local producer
and stored-caller gaps from Q-RNG-005, without establishing all later
writers, callback preservation, heap contracts or input admission.

FND-RNG-017 measures the initializer invoked during the clock conversion;
it does not by itself observe the earlier constructor or standalone
startup initializer. Their native frames and array metadata still need
their own observation before that path is used as runtime evidence.

## Alternatives

- An empty direct-call result cannot exclude an initialization-record
  caller. Its indirect transfer must be resolved from the table and
  dispatcher, as it is here.
- The operand query is a candidate census over encoded immediates and
  displacements, not complete reader/writer coverage. Register-only,
  computed, far/aliased, helper-mediated, overlapping and bulk writes
  remain obligations. The startup bulk zero itself demonstrates a
  relevant write whose encoding contains no label-buffer offset.
- Source bytes can only be bounded by the scan when their identity and
  stability between scan and copy are established. Table-loop admission
  depends on that copy and allocator/callback effects.
- The startup failure continuation, other callbacks and every writer
  of the source/count fields are not a complete reading here. No native
  constructor run, full lifetime claim or parity promotion is made.

## How to reproduce

Use the locked project evidence Python and approved installed DSUN.EXE.
Run immediate_search.py for 3920, 3575, 3950, 395C, 3976, 3978, 397A,
397C and 397E. Confirm the known initializer stores, indexed calendar
reads and label-pointer reads as controls; qualify the recovered root
writer by reading the listed constructor and established startup ranges.
Run direct_callers.py for 1000:29AA, 1000:2172 and 1000:3B56, with
29B3/29D3 as allocator controls. Run far_pointer_words.py for the
constructor and timezone initializer, resolving the latter's relocated
segment field rather than trusting a decode of data bytes.

Use report.mjs table with source DSUN.EXE and XXH3
e296af55ba2ecde7e77f555c90f33d0b. Query shipped start 0x50994,
count/limit seven, stride six and fields mode/priority (width one at
offsets zero/one), target/segment_word (width two at offsets two/four).
The established dispatcher bounds and stride supply count evidence.
Keep configurations and source-derived outputs only in GAME_DIR.
