---
id: FND-CONFIG-185
title: The named mode path sets a word and calls the resident region loader with its optional-byte argument zero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:06B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:01FA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:01CD
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete local 16-bit readings and declared FBOV/MZ operand mapping
environment: null
---

## Observation

FND-CONFIG-179's initial mode-two sequence calls resident
362C:06B9, calls its filename helper, then passes current
word DS:140C and word zero to resident 362C:01FA.
The complete 06B9 span is `0x0002BB79..0x0002BB84`.
It assigns current word DS:29EB to eight and returns
without a callee or own AX normalization. It does not
write byte DS:29EA or word DS:0DAB. The following
filename helper has the separate archive/segment effects
in FND-CONFIG-181 and FND-CONFIG-182.

FND-REGION-005 previously reads the general 01FA
resource sequence. The complete resident body spans
`0x0002B6BA..0x0002B81C`. It saves SI, retains the
first word argument there and initializes a local far
pointer to zero. Current byte DS:29EA zero skips its
active work. Other values test far current DS:29EF.
Null requests a pointer through 444C:0008 with double
words one and 00003100, stores returned DX:AX there,
and re-tests the field. Still null passes near DS:2A1C
to error helper 56B2:0034, then takes the local return
if that helper returns. The error helper's termination
request is bounded in FND-CONFIG-062. This is not a
native allocation or termination observation.

The admitted pointer path calls local far 01CD, then
passes current word DS:13FE and word zero to resident
1BF3:4723. That latter service's complete effects and
input-state provenance remain open. 01CD's complete
span is `0x0002B68D..0x0002B6BA`. It zeros 520
bytes at far mapped 54AB:0514 and 260 bytes at
54AB:0410 through runtime 1000:3FA2, without its
own null/capacity checks or normalized result. Following
that service, 01FA fills 260 words at 54AB:0208
with sixteen times index remainder by twenty, and 260
words at 54AB:0000 with sixteen times its quotient.
These own loops have no calls, use unsigned division
and stop before index 260. Accepted capacity and any
service changes remain conditions.

Current byte DS:14E5 zero requests type PAL followed
by a space through 38FF:04AB, sign-extending post-call
SI to a double-word number and supplying the far address
of its local pointer. A returned AX zero passes that
pointer to 444C:0092 and replaces the local pointer with
returned DX:AX; other results skip that release. The
release wrapper's explicit zero after a returning runtime
call is read in FND-CONFIG-184. This path alone does
not prove palette application, accepted bytes or complete
cleanup after every resource failure. FND-CONFIG-151
bounds resource pointer assignment before possible I/O
failure. The named request's actual SI and DS depend
on preservation by intervening calls.

Next it requests RMAP with zero-extended current SI
and the far output address of current DS:29EF. Nonzero
returned AX requests MAP followed by a space with the
same argument construction; zero skips that fallback.
There is no branch using the fallback result before the
second argument's low byte is tested. The original
stacked word zero in the named parent call supplies
low byte zero, so that call locally skips both the later
GMAP request and the 98-by-98 bit-clear loop, provided
its stack argument remains unchanged through callees.
An error returned by MAP does not itself stop the named
zero-argument continuation to local return. These gates
do not prove a selected archive or valid map contents.

In the general body, nonzero low byte enters the GMAP
request only if current far DS:0538 is nonzero. The
following nonzero-byte gate independently enters a loop
that clears bit five at far DS:0538 plus 128 times row
plus column, for rows and columns zero through 97.
There is no pointer-null or resource-result gate protecting
that loop. The named zero argument avoids both blocks
by local branch logic, not by claiming the pointer valid.
All normal local paths restore the saved SI; there is
no own overall AX normalization or DS:0DAB store.
The parent ignores returned AX and calls overlay 200
before its later mode comparison (FND-CONFIG-183).

Every resident far-call, mapped table segment and runtime
fill operand above was checked through header-derived
MZ relocations. The local 01CD call has a far frame
through push-CS/near-call. No accepted source contents,
rendered region, stable DS or native outcome is inferred
from the named number or the zero option argument.

## Interpretation

The named mode call fixes the resident loader's optional
byte to zero and therefore skips its own optional GMAP
and bit-clear blocks under preserved-stack conditions.
Earlier allocation, table, palette and RMAP/MAP steps
have independent gates and results. A returning MAP
failure or error helper does not by itself stop the
parent's later work, but a process-termination outcome,
changed segment or invalid storage remains separate.
The nearby word-eight setter does not establish the
loader's byte gate or the parent's later mode value.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain all callers,
byte/word/pointer and archive producers, SI/DS/stack
preservation, capacities, aliases, allocation/runtime and
4723 effects, actual resource I/O and later mode changes.
One reading supplies a zero gate and skips active loading;
another admits valid state and obtains accepted resources;
a third returns a resource failure with assigned fields.
Complete producer/callee and native outcome evidence would
distinguish them. A zero optional argument does not decide
the earlier resource path.

FND-REGION-005's earlier general finding left callers
and the second argument unknown. This named overlay
caller settles that argument for this path only, not all
callers or region behavior. The local signed PAL number
and unsigned RMAP/MAP numbers likewise do not prove
negative inputs are reachable. No native or emulated
experiment is claimed.

## How to reproduce

Read resident 362C:06B9 through 06C3, 01FA through
035B and local 01CD through 01F9 from their entries.
Verify all declared MZ operands and the local far frame.
Map the named parent's DS:140C/zero pushes into BP+6
and low byte BP+8. Follow byte-zero and null-allocation
bypasses, fill counts, unsigned table loops, sign versus
zero extension, PAL release gate and RMAP/MAP fallback.
Separate the named zero-option return from both general
nonzero-option blocks. Retain allocation, runtime 149B,
4723, error and I/O effects and register/storage producers
instead of interpreting an ignored result as accepted content.
