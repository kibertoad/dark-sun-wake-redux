---
id: FND-ITEM-087
title: Bitmap reload publishes allocation before transfer and pool exhaustion requests global reset
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:31F7..31E0:328F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:3C02..31E0:3D2A
tool: tools/research/exec-census/resident_listing.py at ef7f9a51; Python 3.12.10, Capstone 5.0.7; composition with FND-ITEM-084/086 and FND-CONFIG-184
environment: null
---

## Observation

Q-PARTY-116 retains bitmap reload/allocation effects after FND-ITEM-086's
provider reading. This reads both complete local bodies. Addresses,
offsets, masks and sentinel words below are hexadecimal; other counts
are decimal. Native pool/cache identity and external children remain separate.

### Reload admission and publication

31E0:31F7 first reads byte +0E at 52A2:0CA8+16*index with word-width
offset arithmetic, before its index gate. Mask two set and second-argument
low byte zero returns AL one immediately. Otherwise index FFFF or unsigned
index greater than DS:265D returns AL zero. Equality with that count is
admitted locally. The initial read therefore requires storage even for
indices that the later gate rejects, and the bypass does not validate them.

It selects that cache record again. If mask two is clear, its size dword
+2 is passed to 31E0:3C02 and returned DX:AX is stored at far field +0A.
Null returns AL zero with that store retained. Mask two set bypasses this
allocation; a nonzero second low byte can thus force a reload into the
existing buffer. There is no own buffer-null or span check on this route.

It calls 1425:1954 with current DS:45A2, record offset dword +6, buffer
far pointer +0A and only the low word of size +2. Nonzero returned AX
returns AL zero without an own buffer release or field rollback. Zero
sets mask two in the record and returns AL one. No other flag is cleared.
BP is restored; no ES/scratch or full AX result guarantee is inferred.

FND-ITEM-086's hit caller supplies second low byte zero and ignores AL.
Consequently its returned index alone does not prove reload allocation,
backing-store transfer or readable frame content, even with a local gate
and explicit residency store. The allocator can also request global cleanup.

### Pool allocator gates and search

31E0:3C02 requires both DS:265C and DS:265B nonzero and unsigned requested
dword at most 37600. DS:14E3 selects two equivalent local gate branches.
Rejected requests return DX:AX zero without the pool scan. Zero size is
not independently rejected.

It initializes the best remainder to FFFFFFFF and selected index to FFFF.
Each search begins at pool index zero. It reads the size dword at
DS:95FB+8*index before checking unsigned index at most DS:2650, stopping
at a zero size or an index beyond that inclusive bound. Valid sentinel/
boundary storage is therefore required separately from the count bound.

For an unsigned size at least the request, it selects that entry only if
the unsigned remainder is strictly less than the retained best remainder.
Ties retain the earlier entry. Index increments and near offsets keep
word width; there is no own array-capacity or wrap-termination guarantee.

If no entry was selected, it calls 31E0:3A66(request), ignores its return,
then repeats the search once. A second miss calls that child again before
leaving the two-pass loop; there is no third scan after the second call.
Pool mutation, count identity and successful reclamation are not supplied
by this local loop. A selected index must also be signed greater than
FFFF to reach allocation; high-bit indices fail that later signed test.

### Selected pool state and exhaustion

For a selected admitted index it saves the far pointer at pool record +4.
It calls 1000:0586 with DS and the near address of that pointer field,
and request in CX:BX. It then subtracts the request from that pool size
at doubleword width and returns the previously saved far pointer.
The pointer-update helper's complete effects, valid spans, aliases and
preservation remain open; this caller does not check its result. A selected
pool pointer can itself be null, with no own null gate before these updates.

With no signed-admitted selection it requests 5773:002A, 5773:0039 and
5773:0025(37600 dword), in order, then explicitly returns null. The first
is FND-CONFIG-184's global cleanup, whose local contract includes object
animation-word changes, releases, cache residency clears and enable-byte
clear under its gate. The later services' effects remain requirements;
this is not a harmless failed allocation or proof of a completed reset.
SI/DI/BP are restored by this allocator; other preservation is not asserted.

## Interpretation

This removes reload's local pre-read/index/allocation/transfer gates and
the pool allocator's local search, selection, retry and reset-request
ordering from Q-PARTY-116. A provider hit can return its index after a
reload failure whose allocator requested global cleanup. Whether an actual
request reaches that path and what survives require native cache/pool/
buffer state, producer/lifetime evidence and the named children, not a
blanket success or nonmutation assumption.

## Alternatives

Assuming the index check protects the first read reverses the order.
Treating the count as an exclusive bound changes equality. Treating each
reclamation request as followed by a rescan invents a third pass. Treating
null as no state change ignores pool updates or global reset requests.
No native failure, complete-reading declaration or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with resident_listing.py on the two half-open location ranges in order.
Under GAME_DIR/analysis/item-removal-20261104/,
effect-bitmap-reload-allocation-contract.txt and its -repeat pair both have
SHA-256 e5746e61bf2712c4c7282bed68373e72b1829aedc2a7b4076b8c60e07d556e3d.
Keep reports local. No original game or isolated function was executed.
