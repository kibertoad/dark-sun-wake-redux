---
id: FND-EXE-417
title: Tail reclamation flushes dirty records and tests callback output before releasing a predecessor link
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0DE9..1425:0FA3
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The 442-byte body at 0DE9 decodes as 170 instructions. It saves
BP, SI and DI and allocates eighteen local bytes. Current DS:00D0
equal to one returns AX zero before any dirty-record scan. Other
values, including zero, enter the later work; there is no initial
unsigned lower/upper count bound here.

### Dirty-record prefix

SI starts at zero and visits indices zero through five with word
stride 000E. A zero current indexed DS:3EF8 dirty word skips
the call. Nonzero calls 0BB4 with the current indexed DS:3EF4
identity and a far pointer to its DS:3EEC head. Six outgoing
bytes are removed and full returned AX tested through DX. Nonzero
returns that result immediately. Zero clears the currently indexed dirty
word before moving to the next index. Earlier successful clears and
callee effects remain when a later record fails; there is no rollback.
FND-EXE-415 describes this wrapper's lookup and second-slot request.

### Tail walk and inspection

After the six-record prefix, DX becomes zero and DI one. Each
outer iteration sets local BP-2 and BP-4 to zero. While the
current DS:39CE link selected by BP-2 times 0108 plus
BP-4 times four is nonzero, it stores that link in local BP-0A
and calls 060B with the stored word and far output pointers
SS:BP-2 and SS:BP-4. Ten outgoing bytes are removed and
full returned AX tested through DX. Nonzero returns immediately;
zero repeats from the updated local coordinates. No local link-range,
cycle, visited-node or count-derived traversal bound is applied.

When a zero link is reached, coordinates zero/zero set DI zero
and current DS:00D0 one, then finish. This root case does not
consume the saved link or invoke the inspection target.

A non-root tail reads its current DS:39CC encoded word into
BP-8, takes the high four bits into BP-6 and retains the low
twelve bits in BP-8. SI becomes zero. Each inspection request
pushes word eight, far local destination SS:BP-12, current SI,
local BP-8 and current DS:3F4E indexed by BP-6 times
000E. It calls the current far pair at similarly indexed
DS:3F42/3F44, removes twelve outgoing bytes and tests full
returned AX through DX. Nonzero returns immediately.

Zero compares only the destination's first word at BP-12 to one.
A different word sets DI zero and stops inspection/reclamation.
An equal word adds eight to SI at word width and repeats while
unsigned SI is below 4000. With preserved loop and local state,
positions 0000 through 3FF8 give 2048 inspection calls. The
body does not initialize the eight-byte destination before the first call
or before subsequent calls. A zero target result therefore does not
independently prove fresh destination bytes or an eight-byte transfer.
Only that first word is tested locally; the remaining bytes are not.

### Predecessor search and release

If DI remains nonzero, the body resets the two local coordinates
to zero and searches until the selected current DS:39CE link
equals the saved word at BP-0A. A mismatch calls 060B
with the current selected link and the two local output pointers,
removes ten outgoing bytes and tests full returned AX. Nonzero
returns; zero repeats the comparison. Unlike the first walk, this
search has no local zero-link terminator test. It also has no local
cycle, visited-node or iteration bound.

On equality it calls 09AD with a far pointer to the selected
current DS:39CE word, removes four outgoing bytes and tests
full AX through DX. Nonzero returns with that callee's failure
prefix intact. Zero decrements current DS:00D0 at word width,
without a local underflow check. If the re-read count equals one,
DI becomes zero; otherwise the outer tail inspection starts again
from zero coordinates. Final return copies current DX to AX.
FND-EXE-409 and FND-EXE-411 describe lookup/release effects.

### Preservation and admission

The body does not save or restore DS. SI and DI are saved only
for final return, without local saves around helpers or slot calls.
Current DS, loop registers, local coordinates, encoded word, saved
link and callback destination are consumed after calls. Aliases and
native preservation must be admitted before treating the saved link
as the same stable tail or the nominal call count as unconditional.
The initial count value neither bounds the link walks nor proves
that later decrements agree with the actual chain.

## Interpretation

This reads the adjacent consumer requested by FND-EXE-416 and
adds count/dirty writers and callback-output consumption. Q-EXE-007
retains complete callers, count/link/encoded-word and six-record writers,
stable structure, destination freshness/extents, aliases and segment/register
and native contracts. No complete reading, observed original bug,
atomic reclamation or successful native transfer is declared.

## Alternatives

Treating count one as a request to flush ignores its immediate return.
Treating the count as a walk bound adds tests absent from both searches.
Treating a zero callback result as an initialized output invents writes
outside this reading. Treating reclamation as testing all eight returned
bytes contradicts its first-word comparison. Treating a failed release as
rolling back prior dirty clears discards the ordered prefix.

## How to reproduce

At revision 80eace73 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus 0DE9..0FA3 in sixteen-bit mode. Require 442 bytes
and 170 instructions. Follow the count-one exit, six dirty tests and
clear ordering, both zero-coordinate walks, saved-link provenance,
root exit, encoded-word split, uninitialized callback destination,
first-word test, request loop, predecessor equality and release/count
ordering. Use FND-EXE-415/409/411 for callee dependencies.
Preserve writer, alias, termination and native-effect gaps. Licensed bytes
remain outside Git; no game, DOSBox or emulated call runs.
