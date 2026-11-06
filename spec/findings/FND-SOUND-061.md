---
id: FND-SOUND-061
title: Music-table literal accesses concentrate in loader and selector while failure paths preserve mutated globals
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:043B..2834:0508
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:000C..2834:043B
tool: window_operands.py and resident_listing.py at 8729386a; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-011/012 and FND-COMBAT-082
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
Q-SOUND-011 seeks all accesses affecting FMT-SOUND-002. This
finding settles the inventory-listed literal window component,
not every possible file-name, alias or indirect access.

### Controlled field-window census

The window 424B..4258 reports 32 operands in listed routines:
21 in selector 2834:000C, nine in loader 2834:043B and two
at adjacent DS:4256 in overlay 179. The last two do not name
the table pointer, count or retry word and their word spans do
not overlap those fields under unchanged DS binding. Listed
matching routines have zero string stores.

The selector's 21 operands comprise the retry-word comparison
at 009E, count-byte reads at 0172/01BE/0267, and seventeen
loads or near-offset reads of the table pointer at 424E. The
loader's nine include the header destination literal 4252,
three count reads, two pointer-word stores and three later
pointer reads. Known loader stores 0496/049A and selector
retry comparison 009E are positive controls from FND-SOUND-011/012.
The loader request at 046A can write count and retry fields
through an address argument; no direct MOV to them is required.

Eight raw candidates remain outside established instruction starts:
two are interior decodings of known pointer operands; six are
outside listed starts, five computed memory read/write decodings
with displacement 4252 and one immediate 4252. They are candidates,
not established readers/writers. Their actual instruction ownership,
base/segment values and reachability remain unresolved here.
The census also excludes computed pointers with different literal
displacements, overlapping writes beginning outside the window,
alternate entries, external writes and modified code.

A separate name window 08BF..08C8 reports three listed operands:
two selector accesses to neighboring state 08C0 and the loader's
filename offset 08C2, a positive control. Its 787 raw candidates
are retained without claiming they all address that filename.
No claim of a unique runtime file opener follows from that window.

### Complete local loader branches

043B initializes a local double-word result to zero and opens the
near name DS:08C2 through 44DE:0086 with mode one. Handle
FFFF returns zero directly. Otherwise it requests three bytes
through 44DE:01AF into DS:4252 and compares returned DX:AX
with exactly three. Failure reaches close/cleanup; bytes already
written to count/retry are not restored locally.

For a complete header it reads count byte 4252, sign-extends AL
to AX, multiplies by six at word width, zero-extends that result
to EAX and passes it to 444C:00FA. It publishes returned DX:AX
at 4250:424E without a local null-result test. Thus for unchanged
count c the requested extent is UINT16(6 * INT8(c)), widened
unsigned, not a generic unsigned-byte count times six. Counts
0..127 give 0..762; 128 gives FD00 and 255 gives FFFA. The
shipped count 38 still gives 228 bytes, consistent with FND-SOUND-011.
These arithmetic cases do not assert that changed files are admitted.

It reloads count and recomputes that extent for the data read, using
the published pointer offset and the allocation's held DX segment.
It then reloads count a third time and recomputes the expected
extent for comparison with the read result. Actual count/pointer/DX
retention is required to identify these three extents and destinations.
There is no local saved immutable count or capacity validation.

When the data count matches, it closes a non-FFFF held handle
and returns one without inspecting close success. On either header
or body count mismatch it closes the held handle, tests current
global pointer 424E for nonzero, and passes it to 444C:0092
before returning zero. It does not clear the pointer after release
or restore count/retry/header fields. On header failure the pointer
can still be a previous global value; no new allocation is required
to enter that cleanup path. Actual allocation/release semantics,
source-file identity and OS outcomes remain separate conditions.

## Interpretation

Q-SOUND-011 can remove the unread known literal field sites and
loader-local arithmetic/failure ordering. The access census still
retains eight field-window candidates, name-window candidates,
unknown aliases/indirects and actual byte/buffer bindings. Successful
generic I/O is not required merely to describe the shipped layout,
but failed reads are writes to the loaded structure and are now
explicitly included. No complete reading, universal writer exclusion,
whole queue closure or observed native failure follows.

## Alternatives

- No direct store means count/retry cannot change: the header read
  writes through a pointer argument and may partially succeed.
- Every byte count becomes unsigned six times count: the loader
  sign-extends before its word multiply.
- Failure leaves no table state: partial header writes and pointer
  publication precede later failures, and cleanup does not clear them.

## How to reproduce

Run window_operands.py on the approved executable and
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv for 424B..4258 and
08BF..08C8. Inventory LF SHA-256 is
820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.
Retain music-table-fields-window.txt and music-table-name-window.txt
under GAME_DIR/analysis/handler-census-20261106/. Read 043B..0508
with resident_listing.py as music-table-loader-contract.txt; reuse
FND-SOUND-012's selector reading and verify its reported operand
sites against the controlled census. Preserve both controls and all
raw candidates/exclusions. No original game, emulator session or
emulated call supplied evidence.
