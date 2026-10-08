---
id: FND-EXE-257
title: Second setup helper separates external calls from direct pointer replacement and partial high-byte stores
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:107D..4AE5:10FB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:10FC..4AE5:1155
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00041159..0x0004115B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004117B..0x0004117D
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-256's helper at `4AE5:107D` covers seventy-six instructions
and 215 bytes across the two intervals above, excluding byte
`4AE5:10FB`. It has two unresolved computed far calls and returns far
with eight bytes of additional cleanup. It saves BP, DS, SI and DI,
loads DS through CS-relative word five and reads arguments through SS:BP.

Bit zero of state byte `0x0042` must be set or it returns AX `0xFFFF`.
With that bit set, bit one already set returns AX zero before argument
reads or setup. Otherwise it sets bit one, reads AX/DX from SS:BP plus
ten/twelve and SI/DI from plus six/eight. FND-EXE-256's four pushes make
the first pair its selected base and the second its selected extent,
conditional on intact outgoing argument and return storage.

It combines state words `0x0043` and `0x0045` by OR to test whether their
stored far pointer is all zero. A nonzero pointer selects the call path.
It copies SI/DI to AX/DX, adds `0x03FF` with carry at component widths,
and divides unsigned DX:AX by `0x0400`, without an explicit quotient
range guard. It exchanges the quotient into DX and the remainder into
AX, then sets AH to nine. Thus AL comes from the remainder's low byte;
AH overwrites its upper byte. The first far call at `4AE5:10CB` reads
the pointer through current DS-relative `0x0043`.

Returned AX zero rejects with AX `0xFFFF`. Nonzero stores returned DX
to current state word `0x0047`, sets AH to `0x0C` and makes another far
call at `4AE5:10D9`, again through current DS-relative `0x0043`.
No DS or pointer reload into a retained register intervenes: the call
itself reads current storage after the earlier call. Returned AX zero
again rejects. Neither failure explicitly rolls back the earlier bit-one
or possible word-`0x0047` publication.

On the second nonzero result it stores current BX/DX to words
`0x003A`/`0x003C`, then to `0x0049`/`0x004B`. It copies BX to AX and
adds current SI/DI to AX/DX at component widths with carry before the
common endpoint stores. SI and DI at these operations are post-call
register values; their preservation is not established by this body.
The pointer, AX/DX/BX result meanings and external register effects remain
unread contracts, not native allocation evidence.

The zero-pointer path first stores incoming AX/DX to state words
`0x003A`/`0x003C`, sets ES to zero and saves the state DS. It loads DS
from a relocated immediate: shipped operand `0x00041159` raw `0x45E3`
becomes `0x55E3` at load segment `0x1000`, corresponding at offset zero
to shipped offset `0x0004B030`. Through ES it reads word `0x0064` and
stores it to this new DS-relative word `0x002F`, then reads word
`0x0066` and stores it to word `0x0031`. It writes `0x003F` to
ES:`0x0064`, then the same relocated segment to ES:`0x0066`; that second
operand is at shipped offset `0x0004117B`. These are ordered separate
stores, not atomic replacement or observed native vector installation.

Still through the new DS, it writes AX as a word at `0x003A` and DL
as a byte at `0x003C`. It adds SI/DI to AX/DX and writes AX as a word
at `0x002C` and DL as a byte at `0x002E`. Those byte stores do not write
the adjacent high bytes. Their prior writers and any whole-word consumers
remain obligations. The state DS and this relocated DS are not presumed
to be distinct or equal solely from their offset names.

After restoring the state DS, the common path stores AX/DX to words
`0x003E`/`0x0040` and returns AX zero. All paths restore SI, DI, DS and
BP before the far return, conditional on intact saved storage. The direct
path leaves ES zero; the external-call path has no explicit ES restoration.
FND-EXE-256 consumes the full AX result and can re-enter its second pass;
the already-set bit-one shortcut can then avoid the helper's setup body,
subject to intervening state identity and writer admission.

## Interpretation

This follows the second writer's callee and separates request setup,
external results, pointer replacement and partial-width publication.
Neither nominal zero nor the caller's second pass proves successful
resource acquisition or preserved state. FND-EXE-253's corresponding
cleanup tests flag byte `0x0042`, word `0x0047` and the stored segment;
connecting those inputs still requires all producer and alias obligations.

Q-EXE-001 and Q-EXE-010 retain live pointer and flag writers, all caller
frames, external register/result contracts, arithmetic bounds, independent
output extents and partial-byte initialization. No complete_reading or
replacement inventory is established.

## Alternatives

Treating both paths as the same allocation call ignores direct pointer
stores on the zero-pointer route. Full-word high-component publication
ignores the byte-width stores. A repeat gate set after success is contradicted
by the early bit-one store. A retained original pointer for the second call
ignores its fresh memory operand and post-call DS dependency.

## How to reproduce

At revision `f8c4e1f`, use FND-EXE-236's original-source identity, region
and default x86-bounds limits. Set entry and sole entries to `0x000410CD`,
with no seeds or summaries. Check intervals `0x000410CD..0x0004114B`
and `0x0004114C..0x000411A5`, seventy-six instructions, computed far
calls at `4AE5:10CB` and `4AE5:10D9`, and far return with eight-byte
cleanup. Calls are assumed to return; complete CFG is not a Standard
complete reading or proof that division succeeds.

Decode both intervals from the shipped source in sixteen-bit mode with
Capstone. Query operands at `0x00041159` and `0x0004117B`, sourceKind
mz and targetOffset zero, under the same source hash. Follow each segment,
complete far pointer, partial store and result consumer independently.
Compare FND-EXE-256's argument setup and FND-EXE-253's cleanup inputs
without assuming live state preservation. Keep source, reports and
configurations in GAME_DIR.
