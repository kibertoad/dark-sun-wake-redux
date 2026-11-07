---
id: FND-SOUND-093
title: Startup size configuration does not allocate buffers, and paired retained-buffer state selects release
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AAB:0008..4AAB:0047
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:01D0..4734:026B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AB9:0043..4AB9:006A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050420..0x00050424
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050458..0x0005045C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004451F..0x00044520
tool: resident_listing.py at 377bc237; pinned evidence Python and Capstone 5.0.9; Ghidra 12.1.3 focused decompiler/data reports; reused FND-SOUND-011/082/092, FND-CONFIG-165/167 and FND-EXE-583
environment: null
---

## Observation

Q-CONFIG-014 retains gate/slot preservation through startup children.
This connected assessment determines which retained-buffer states actually
invoke release before the first music-driver gates in FND-SOUND-092.
Addresses are hexadecimal. Installed DSUN.EXE is 634416 bytes with
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b, load model 1000.

### Pre-setup arguments are stored sizes, not immediate allocations

FND-SOUND-011/CONFIG-159 identify the startup call to 4AAB:0008 with
two double-word arguments: decimal 25000 and either 12000 or zero.
Its complete 63-byte body makes no call and allocates no memory.
If byte 4E71:0B53 is not zero, it returns without its data stores.
Otherwise it stores its second double-word argument to 4E71:0B5A.
When byte 4E71:0B67 is also zero, it first stores the first argument
to 4E71:0B63. The first guard is exact zero, distinct from the sound
wrapper's exact-one ready check in FND-PARTY-320.

The routine has no direct pointer publication to DS:3420 or DS:3458,
no gate/slot store and no guaranteed success-result assignment. Calling
this routine is therefore not evidence that either buffer already exists.
The actual initialized image has null far pointers at both fields and
zero at reuse byte 4E71:0C0F. These are source values, not a proof of
their preservation up to a later entry.

### Paired-pointer admission in 4734:0133

The initializer's prefix at 01D6 tests DS:3458 and then DS:3420.
If both sampled far pointers are zero, 01E6 jumps to 026B, bypassing
all releases and reuse-byte stores in this component. No child occurs
between those initial pointer tests. With stable pointer values, its
other cases are:

| Entry pointer state | Local route |
| --- | --- |
| Both null | No release and no assignment to reuse byte 0C0F. |
| Both nonnull | No release; the later pair of nonnull tests selects reuse byte one. |
| Exactly one nonnull | Enters the release region, separately retests each current pointer, releases those then nonnull and clears each field after its child returns; stores reuse byte zero afterward. |

The mixed route reloads the second pointer after the first release. A
child that populates or changes that second field can change the second
request; initial nullness alone does not exclude it. Likewise, the first
field is cleared after its release returns, overwriting any returned
mutation of that field. This is ordered release/clear behavior, not atomic
cleanup or rollback. A later asynchronous change can also affect the final
nonnull tests at 0250/0258, which may set the reuse byte to one.

The both-null branch leaves the reuse byte unchanged. Its source value
zero does not justify treating that branch as a reset to zero on every
entry. FND-SOUND-082's later source-allocation gate consumes the current
reuse byte; a retained value one must remain an admission question when
the corresponding allocation path is reached.

### Release effects belong to reached release paths

4AB9:0043 accepts one far pointer. If current DS:346D is nonnull, it
passes the pointer to that callback; otherwise it forwards it to
444C:0092. The wrapper has no own null-pointer test or direct data store.
FND-CONFIG-165/167 already read the default child's metadata accesses,
returned-zero behavior and deeper runtime dependencies. FND-EXE-583
locates the callback field and its source initialization; its direct-call
search does not establish the absence of computed or aliased setters.

On the admitted both-null path neither default release nor custom release
is invoked by this component. Their full heap/callback effects cannot
alter the gate through a call that this path does not make. Conversely,
the mixed-pointer path retains actual callback/default selection, input
metadata/backing, return, register/frame preservation and global-write
effects relevant to DS:3444/343E. A generic heap-proof requirement for
all entries would obscure this path distinction.

## Interpretation

Q-CONFIG-014 can remove unread size-producer and retained-buffer admission
semantics. The remaining source-to-entry question is whether the pointers
and reuse byte retain their source values through the actual preceding
calls, configuration callback and startup history. Once admitted null at
these tests, release is not an intervening child in this region. Other
startup children and asynchronous effects remain FND-SOUND-092's obligations.

This does not prove the whole first native startup, successful allocation,
valid release metadata, universal gate preservation or a complete reading.
No queue item closes and no original process or emulated function ran.

## Alternatives

- The pre-setup size call allocates the buffers: its complete body only
  performs guarded double-word stores.
- Every startup must discharge release's entire heap contract: the admitted
  both-null and both-nonnull routes invoke no release here.
- Both-null always normalizes reuse to zero: it skips the reuse stores.
- A second pointer observed null before release cannot be released later:
  the caller reloads it after the first child returns.

## How to reproduce

Use resident_listing.py on the approved executable for the three code
ranges above, including the closing returns at 4AAB:0046 and 4AB9:0069.
The initializer subrange starts at admitted instruction 01D0; initial
01F0/01CF offcut contexts are rejected for prefix interpretation.

In the existing isolated resident Ghidra project, read-only without
analysis, decompile 4AAB:0008 and 4AB9:0043 and inspect initialized data
at 57E0:3420 and 3458, four bytes each, and 4E71:0C0F, one byte.
Retained reports under GAME_DIR/analysis/startup-buffer-admission-20261107
are buffer-producers.log and final-buffer-controls.txt. Earlier truncated
or offcut windows remain diagnostic context; final-buffer-controls.txt is
the admitted control. Reuse the existing release and startup findings;
no original-derived code or data is committed.
