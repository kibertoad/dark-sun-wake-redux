---
id: FND-CONFIG-199
title: A rectangle transfer caller reads a second cleanup handle on a path that skipped its assignment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:0BD5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:282D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:28C5
tool: Python 3.14.7 and Capstone 5.0.7 complete bounded resident 16-bit reading and all local far-call MZ relocation checks
environment: null
---

## Observation

FND-CONFIG-195's release calls at file 0x000391DC and
0x000391EB belong to resident 4328:0BD5. The complete body
spans file 0x00039055..0x0003920C inclusive. It reserves twelve
frame bytes, saves SI/DI, reads the first two word arguments into
those registers and has the runtime stack-limit guard through
1000:2E48. Only the local word BP-0C is explicitly initialized,
to zero, before the main gates.

Current DS:33BC zero or equality of words DS:33B6 and 33B8
sets current DS:2FB8 to one and skips the later work. Otherwise
it inspects far DS:A14D. A null pointer bypasses its image-related
branch. A nonnull pointer supplies frame-reader arguments through
current A14D/A14F/A151 to 1BF3:76C2 and 771C; their returned
words participate in wrapped coordinate-plus-dimension-minus-one
calculations. Unlike FND-CONFIG-189, this body does not reject
signed dimensions below one immediately after those calls.

Signed horizontal and vertical comparisons decide whether to
skip that image-related branch. The admitted branch expands its
first two coordinate words to signed minima with A10D/A10F and
its last two stacked words to signed maxima with the derived
endpoints. It then calls 3D72:0DE5 with current DS:33B8 and
sets local BP-0C to one after return. FND-CONFIG-197 reads
that wrapper's temporary A033 replacement and normalized result.
The following common path clamps SI/DI to at least zero and
the last two words to at most current A039/A03B. If either
resulting first coordinate is signed-greater than its last one,
it returns directly, bypassing the later handle work and 0E10
call even if BP-0C was set earlier.

An admitted rectangle makes the first 1BF3:282D request with
current DS:33B6 and the four coordinates. AX is stored at
SS:BP-08, then compared with FFFF. A FFFF result goes directly
to cleanup at file 0x000391D3. Other results make a second
request with current DS:33B8 and the same coordinate form,
storing its AX at SS:BP-0A. A second FFFF likewise enters
cleanup; two nonsentinel words call 2D40:3B1D with the second
handle as its first argument and the first handle as its second.
The transfer wrapper result is not tested.

Cleanup first compares BP-08 with FFFF and releases it through
1BF3:28C5 only when unequal. It then independently compares
BP-0A with FFFF and releases it when unequal. Neither test
is a signed-greater-than-one or general index check. The results
are ignored. On the first-request-FFFF path, BP-0A has no own
preceding assignment: the second request and its store were skipped,
and entry initialized only BP-0C. Cleanup therefore reads the
preexisting frame word on that path. Its value is not established
by this local reading. The first slot has been assigned FFFF and
skips release, but that does not initialize the second slot.

After cleanup, nonzero BP-0C calls 3D72:0E10 with current
DS:33B8; zero skips it. All paths restore SI/DI and the frame
and far-return without an own overall AX normalization. All ten
encoded far-call segment operands in this body are declared MZ
relocations, including the two release calls, both requests,
frame readers, temporary-state wrappers and runtime guard.

## Interpretation

This is another direct release route outside the signed wrapper.
Its sentinel-only cleanup has a concrete local assignment gap on
first-request failure. That establishes encoded read ordering,
not a native invalid release or reproduced bug. FND-CONFIG-191
allows local request exhaustion or coordinate rejection, but this
caller's actual slot and coordinate inputs remain unestablished.
FND-CONFIG-194's hardware and slot effects remain conditional.
The earlier 0DE5 call also does not guarantee a matching 0E10
call on every later rectangle-rejection path.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain incoming callers, slot and
coordinate producers, frame-word provenance, aliases and native
VGA effects. One reading reaches the first-request failure with a
preexisting FFFF second word and skips both releases; another reads
a different second word and admits that direct call. A third supplies
a successful first request and assigns the second slot normally.
Complete caller/state readings and a bounded owner observation where
hardware decides the result would distinguish native outcomes.

The reading that every cleanup handle has an own assignment on
every entering path is ruled out by the first-request failure branch.
A reading that sentinel-only cleanup validates a handle index is
ruled out by the absence of that check. No native or emulated result,
accepted invalid input or original-game defect is claimed.

## How to reproduce

Read 4328:0BD5 from its entry through the far return at 0D8C.
Trace the two main bypasses, image branch, signed coordinate changes,
0DE5 call, common rectangle checks, requests and cleanup. Verify
that BP-0A's only own assignment follows the second request and
that first-request FFFF jumps to cleanup before it. Check both
sentinel tests, ignored results and the conditional 0E10 call.
Verify every far-call segment operand against the MZ relocation
table. Compare FND-CONFIG-190, FND-CONFIG-191,
FND-CONFIG-194 and FND-CONFIG-197 without treating local
failure possibilities as established native reachability.
