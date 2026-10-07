---
id: FND-EXE-097
title: PATH preparation uses a prefix-base return while its stored handler forwards before the copy tail
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6AD0..0x006D6AE5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6AE6..0x006D6B69
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D50A0..0x006D51AD
tool: Ghidra 12.1.3 PUBLIC explicit stored-handler recovery and bounded caller/producer reading
environment: null
---

## Observation

FND-EXE-096 identifies stored handler `0x006D6AD0` at the preparation
helper's record field, written by `0x006D69CD`. Recovery of that exact
entry succeeds without executing the original. Its body adds twenty-four
to incoming EBP, reads full adjusted-frame word -84, writes all ones to
adjusted-frame state -88 and supplies that word to `0x00600EB0`.
FND-EXE-052 records the forwarder's selection/publication and saved-state
transfer. No state comparison or local object release precedes this call
in the recovered handler prefix. The incoming frame identity and actual
native handler admission remain unresolved.

The physical normal continuation of that forwarding call is `0x006D6AE6`,
the ordinary prefix-copy tail in FND-EXE-096. It freshly reads adjusted-frame
argument slot twelve and object local -96, adds twelve to current EAX,
reads payload through the object and calls memcpy with that adjusted EAX,
payload and the argument value in its first three slots. In an ordinary
allocation continuation, EAX there comes from the prefix producer's return.
After an unexpected returning handler forwarder, EAX would instead be that
forwarder's current return. Physical sharing of the tail does not prove
the same destination, count, valid object or originating frame on both routes.
Following normal copy completion, the tail joins the saved-U decision
recorded in FND-EXE-096. This does not establish a native handler return.

The ordinary preparation caller supplies resulting span T first and its
previous preceding capacity-like word second to `0x006D50A0`, followed
by a local address and old prefix in auxiliary slots. FND-EXE-035 bounds
this producer's direct consumption of the first two arguments, capacity
growth/rounding, prefix stores and returned base. Its direct body does
not read those third and fourth original argument slots; indirect record
machinery and aliases remain separate. These extra caller slots do not
prove that the producer copies old payload or writes the supplied local.

On normal production the returned word is the allocation base N, not
N plus twelve. The producer writes adjusted capacity at N plus four,
zero at N plus eight and zero at N, in that order, then cleans its record
and reloads the saved N for return. FND-EXE-096 saves that raw return and
uses N plus twelve for prefix/tail copy destinations and later object
pointer publication. Its common final stores target current payload minus
four, payload minus twelve and payload plus T. Under valid, unchanged
N and object publication these correspond to prefix offset eight,
prefix offset zero and payload's terminator respectively. The common
stores do not replace the producer's adjusted-capacity word at prefix
offset four. Callee/record effects and aliases can invalidate that identity.

The producer's mutable first argument may grow beyond original T before
allocation. This caller nevertheless keeps its separately saved original
T for final length publication, and keeps separate U for the retained-tail
copy count. Rounded capacity, requested allocation bytes, published length
and copied tail count therefore have distinct producers. Their equality
must not be inferred from the producer's return or header initialization.

For the conditional PATH preparation case in FND-EXE-096 where A, C, T
and U are zero and the captured length equals the fresh length, a selected
new-prefix route supplies first producer input zero. If that input remains
zero through record setup and the second input is unchanged, the direct
producer does not double or pad it: zero is not greater than the unsigned
second word, zero plus thirteen is at most 128, and the allocation argument
is thirteen. The initialized three full prefix words consume twelve bytes;
the preparation caller's zero-length terminator is the byte at N plus twelve.
This is the direct arithmetic and store sequence under stated invariants,
not proof that allocation succeeds or those invariants hold in a native
PATH lookup. A caller's captured length is not a full writer/lifetime census.

Recovery changes the exported analyzer ownership metadata: the ordinary
entry's body count changes from 435 to 303 and the new handler entry has
154 body bytes. Ordinary branches into the shared tail remain present.
These sizes are analyzer body counts, not contiguous spans, loss of code
reachability or a complete coverage result. Only starts and sizes are
exported; no instructions or original data are committed.

## Interpretation

The new-prefix path has a grounded base-to-payload adjustment and distinct
capacity/length/copy-count publications, while the saved handler forwards
before entering a physically shared copy tail on an unexpected normal return.
Q-EXE-009 retains native frame admission, all captured/fresh input writers,
allocator callbacks, source ranges, record/copy effects, aliases and actual
PATH list contents. Existing FND-EXE-035 supplies the producer's bounded
contract; this finding adds its admission in the preparation caller and
does not establish the entire allocation lifecycle or safe shell resolution.

## Alternatives

Treating the raw producer return as already payload-adjusted, assuming
the producer consumes every auxiliary caller slot, publishing rounded
capacity as the preparation's saved length, or treating handler fall-through
as the ordinary allocation result is ruled out or unsupported. A thirteen-
byte zero-input allocation request is not a thirteen-byte payload capacity.
Smaller analyzer ownership after handler recovery does not mean the ordinary
entry stopped reaching its shared continuation.

## How to reproduce

Verify FND-EXE-011's identity and FND-EXE-096's actual stored target writer
and allocation/copy call sites. Recover only entry `006D6AD0` through
RecoverCitedFunctions, then read twelve instructions there, restricting
the handler prefix through `006D6AE5` and retaining the following ordinary
tail separately. Read eighty-five at `006D50A0`; use FND-EXE-035's five-
instruction `006D51C4` exit control and its exact rounding gates. Use
FND-EXE-096's ordinary-tail windows and FND-EXE-052's forwarding contract.
Trace input-slot consumption, N versus N plus twelve, adjusted capacity
versus saved T/U, store order and every unexpected normal continuation.
Check zero-input arithmetic under the explicitly stated stable-input
invariants; do not infer occurrence. Export function starts/body counts
after recovery and review changed ownership separately from branch reachability.
Keep reports and the saved analyzer database local and run no original.
