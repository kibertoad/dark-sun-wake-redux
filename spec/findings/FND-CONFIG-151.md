---
id: FND-CONFIG-151
title: The metadata resource wrappers query length and request a complete transfer into the supplied buffer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04AB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:05B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1
tool: Python 3.14.7 and Capstone 5.0.7 complete local 16-bit entry readings; declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-150 passes FNFO resource two and the metadata
buffer at DS:60ED through resident wrappers 38FF:05B5
and 38FF:04AB. Their complete local bodies occupy
`0x0002E7A5..0x0002E805` and
`0x0002E69B..0x0002E6C5` respectively. Both have a
stack-limit guard before their main body; the guard callee is
not characterized here.

The 05B5 wrapper rejects a null far output address by setting
DS:9D99 to 24 and returning FFFF. Otherwise it writes a
zero double word to that output before calling the archive
signature check 39A9:012E. A zero returned byte exits with
FFFF. A null active archive pointer DS:9D9F sets error 12
and exits with FFFF. Other cases call local 04D5 with the
active archive, type, resource number and far length-output
address, forwarding its returned AX.

Local 04D5 (`0x0002E6C5..0x0002E7A5`) searches
for the requested type with 39A9:0045. It retains a byte
indicating whether any matching type was encountered. A type
record without its high flag bit calls number lookup
39A9:01BC; a found number supplies the double word at
record offset eight as length. The high-bit branch calls
local 0615 to obtain its length and offset. A successful path
writes length through the supplied far output address and returns
zero. Missing records try the next archive through 39A9:0157.
An exhausted search sets error six if a type was encountered,
or four otherwise, and returns FFFF. A failed high-bit lookup
with an existing error also returns FFFF. FND-CONFIG-038
and FND-CONFIG-040 read the shared lookup and traversal
helpers; local 0615's external I/O remains a dependency.

The 04AB wrapper calls local 07B1 with the requested type,
number, start offset zero, count FFFFFFFF and the supplied
far pointer to a destination pointer. Local 07B1's complete
body occupies `0x0002E9A1..0x0002EBC6`, ending at
return `0x0002EBC5`. Its inventory counts 546 body bytes;
the contiguous span is 549 bytes. Following local branch edges
from the entry leaves the three-byte instruction at 0x0002EAC4
unreached, accounting for that difference. It checks archive state and the output
address, then searches the same typed records and fallback
archives. Its selected length must be signed-positive and its
selected offset signed-nonnegative; otherwise it returns FFFF
with the missing-type or missing-number error.

Count FFFFFFFF selects the complete recorded length. Other
count values undergo a signed comparison of start plus count
against that length. The metadata wrapper supplies zero start
and the complete-length sentinel, so this alternative range
check does not supply a destination-capacity limit.

If the caller's destination pointer is zero, 07B1 asks an
allocator for the selected count and stores the result through
the output address, failing with error ten when allocation
returns zero. Otherwise it reuses the caller's pointer without
allocation or a capacity argument. FND-CONFIG-150 supplies
the nonzero DS:60ED pointer, so its ordinary selected path
uses that existing buffer.

The reader adds start to the selected file offset, calls
37FC:08FB with the archive record, and requires zero returned
AX. It then calls local 0A72 with the archive's handle and
computed offset and requires the returned double word to equal
that offset. Finally it calls 44DE:01AF with the handle,
destination and selected count. A returned double word unequal
to count, or the prior unequal offset, sets error 21 and
returns FFFF. Success stores count in DS:9D91 and returns
zero. FND-CONFIG-152 reads the transfer helper for the
bounded metadata request. Allocation, archive preparation,
file-positioning and operating-system outcomes are not proved
by this local graph alone.

Declared MZ operands at `0x0002E7D3`,
`0x0002E6EB`, `0x0002E754` and `0x0002E777`
map raw 29A9 to 39A9. The resource reader's operands
at `0x0002EB4D` and `0x0002EB95` map raw
27FC to 37FC and raw 34DE to 44DE. Wrapper guard
operands at `0x0002E6A7` and `0x0002E7B1`
map raw zero to 1000. DS is mapped 57E0 as identified
by FND-SCRIPT-005.

## Interpretation

The FNFO size call has a concrete length-output contract;
it does not merely return a success byte. The following resource
call supplies the full selected length to the transfer helper
and reuses the metadata buffer. This is a block-producer path
outside FND-CONFIG-142's selected literal stores to DS:60ED.
For a stable selected record with positive length at most 98,
FND-CONFIG-150's size check bounds the requested transfer
to DS:60ED..614E. It does not by itself prove the same
record or length is selected again, a successful transfer, or
valid metadata semantics.

Failure after transfer begins does not imply an unchanged
buffer: the local reader has no restoration of existing bytes.
The output pointer can also be assigned before a later error
when allocation was needed. These local bodies do not establish
atomicity or successful live state. Their signature, archive,
stack and I/O dependencies remain explicit.

## Alternatives

Q-CONFIG-008 retains selected-archive and record stability
between the size and read calls, archive-preparation and
positioning effects, local 0615's I/O, transfer outcomes,
accepted resource bytes, cleanup, and later metadata changes.
One path returns after a successful full transfer; another
fails before or during it and may leave different stored state.
The local branches do not select which occurs in an original
session. No unconditional flag-preservation, buffer-validity
or metadata-range claim is raised.

## How to reproduce

Read the four complete bodies at the stated bounds. Follow
length-output initialization, type/number lookup, high-bit
alternative, archive fallback, sentinel-count selection,
existing-versus-allocated destination, file-position and
returned-count comparisons, all local errors and success stores.
Verify the named MZ relocation operands before labeling call
segments. Confirm 07B1's return from its instruction path;
its inventory size is a body-byte count, not a contiguous end.
Compare FND-CONFIG-150's two call arguments and signed
maximum check, FND-CONFIG-038 and FND-CONFIG-040's shared
lookups, and FND-CONFIG-152's bounded transfer path.
Keep operating-system outcomes separate from static requests.
