---
id: FND-COMBAT-160
title: Per-record resource serialization writes 37 bytes but its matching reader has no record-size bound and caller slot admission remains conditional
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070670..0x00070713
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007118B..0x0007129C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073704..0x00073890
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at c273a716; Python 3.12.10; Capstone 5.0.7
environment: null
---

## Observation

Q-COMBAT-045 retains pointer/resource producers of presentation slot 319.
FND-EXE-639 identifies overlay 187's per-record writer +0840 and reader
+08A5. This reading separates their own transfer extents and follows the
known immediate slot producers without treating direct-call candidates as
an exhaustive incoming-reference census.

### Writer and reader have different size contracts

The complete +0840 body takes slot, signed word resource number and
double-word tag arguments. A nonzero result from 37ED:0064 on current
DS:144E returns AL zero before writing. Otherwise it requests removal
through 37FC:05C5 and continues regardless of that returned result.
It then calls 37FC:00E8 with exactly 37 bytes from
DS:(67BB + low16(37 * slot)), the supplied tag and sign-extended number.
Its final AL is one only when this write request returns AX zero.
There is no own slot bound or exclusion of 319. This records request
arguments/results, not atomic archive replacement or backend success.

The complete +08A5 body computes the same record address, stores it in a
local far-pointer slot, then passes that slot's address, supplied tag and
sign-extended resource number to 38FF:04AB. It returns AL one for AX zero,
otherwise zero. It supplies neither expected size 37 nor destination
capacity. FND-CONFIG-151 establishes that this complete-resource wrapper
reuses a nonnull destination and requests the selected resource length.
Consequently this reader's own body does not enforce a one-record span.
A reported failure also does not establish that destination bytes were
unchanged. Actual selected provider, length and backing remain prerequisites.

For admitted resident DS and slot 319 the computed record begins at 95D6,
and its entry-index word is 95D7. An exact 37-byte transfer ends at 95FB
exclusive. No resource content or alternative length was tested here.

### Known reader and writer callers

The relocation/fixup-qualified direct-call search reports one reader call,
overlay 187 +14EC in the complete +146C routine. That call supplies SI,
ordinal plus one and numeric tag three. FND-COMBAT-159 resolves SI to a
party slot or allocator result on admitted ordinary paths, excluding 319
only under the initialized, uncorrupted free-list contract. That slot
argument alone does not bound a selected resource's length.

The same search reports two writer calls, +108C in +0FB7 and +13E8 in
+135B, and identifies the exported reader/writer trampolines. FND-EXE-639
already describes the first caller's four-party-slot loop.

The complete +135B routine uses its incoming slot for coordinate writes,
an earlier helper and the +0840 request. It supplies numeric tag three
and a resource number derived from a loaded counter plus five at word
width. It has no own slot range check. Following successful requests it
updates the counter resource and requests unloading the placed entry
selected by that presentation record. These are per-record transfer
operations, not the numbered SAVE/1 whole-table writer.

The known call to +135B is overlay 188 +09B2 in the complete +0864 body.
This body's SI comes from its incoming slot. It rejects a zero kind byte
in the indexed three-byte object table. When the destination region differs
from current DS:140C and its low byte is not FF, it passes SI unchanged to
+135B with the destination region and transformed coordinates. There is
no own literal exclusion of 319 on this path. Its incoming slot domain,
intervening helper effects and a reachable populated temporary kind remain
unadmitted; a conditional call is not evidence that slot 319 reaches it.

## Interpretation

This removes the per-record wrappers' own sizes, result propagation and
known immediate slot-source branches from Q-COMBAT-045's unread work.
The exact writer size cannot be substituted for the reader's actual
selected length without source/provider admission. Likewise, the known
writer's unchecked slot argument does not prove a populated temporary
record is serialized during play or present in a numbered save.

Remaining questions include other pointer/bulk producers, selected resource
lengths and providers, incoming slot/free-list/kind domains, storage aliases
and save-call timing relative to temporary-slot lifetime. Computed calls,
jumps and function-pointer routes are excluded from the direct-call search.
No universal save-admission, runtime failure, gameplay bug, evidence-status
or parity promotion follows.

## Alternatives

An ordinary exact-size record emitted by the matched writer is distinct
from an unverified resource selected through archive fallback. A conditional
cross-region serialization path without a slot bound is distinct from an
actually reached call with slot 319. Neither can close the timing question
for the numbered whole-table save.

## How to reproduce

Resolve licensed DSUN.EXE through tools/game-dir.mjs. With the project-local
evidence Python run overlay_listing.py descriptor 187 with complete half-open
hexadecimal file bounds 70670 70713 and 7118B 7129C, then descriptor 188
with bounds 73704 73890. Run direct_callers.py with targets 187+08A5 and
187+0840 and verify the reported sites in their complete bodies; the reader
call's complete +146C body is retained with FND-COMBAT-159.
Approved readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and apply declared mappings.
The combined listing/call report is
GAME_DIR/analysis/hit-incoming-20261104/temporary-record-resources.txt.
Keep original-derived output in that approved local store. No original ran.
