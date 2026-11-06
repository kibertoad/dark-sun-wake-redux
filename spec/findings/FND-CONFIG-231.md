---
id: FND-CONFIG-231
title: Terminal release publishes root changes before requesting a smaller bound and does not roll them back on rejection
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1367..1000:13CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:143B..1000:1464
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:149B..1000:14C4
tool: retained resident_listing.py reports at 90bdf95a; Ghidra 12.1.3 read-only INST-single-20261103b; FND-CONFIG-165/167/225 and FND-EXE-548/549 interfaces
environment: null
---

## Observation

This composes the terminal branch's metadata publication with its connected
bound updater and outer return contract. Installed DSUN.EXE is634416 bytes,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Existing findings already
read the service bodies; the new distinction is the exact requested pair
and persistent metadata for each terminal predecessor choice.

### Four terminal choices

149B selects1367 when the nonzero released segment P equals current
last root CS:135D. Let F be first root CS:135B, R=P:0002 and
T=R:0008 where that field is read. Under valid, stable, separate header,
link, code, argument and saved-slot storage, the instruction paths are:

| Selected condition | Metadata published before17F2 | Requested segment:offset |
| --- | --- | --- |
| P equals F | Clear first, last and free roots135B/135D/135F | P:0000 |
| P differs from F; R:0002 nonzero | Set last root to R | P:0000 |
| P differs from F; R:0002 zero; R equals F | Clear all three roots | F:0000, which is R:0000 |
| P differs from F; R:0002 zero; R differs from F | Set last root to T, unlink R through143B | R:0000 |

The first branch does not read a predecessor. The other branches load
DS=P and then DS=R before testing R's word two. The final branch pushes
R and zero as the outgoing pair before unlinking; it does not rely on
the unlink helper to return that segment in DX. The other branches form
the pair from retained DX, replacing DX with F in the third case.

For final-branch unlink, R:0006 equal to R clears the free root. Otherwise
A=R:0004 and B=R:0006 select writes B to A:0006, A to B:0004,
and A to CS:135F. These are the same unlink destinations recorded in
FND-CONFIG-211/230. This branch does not itself insert P into the free
list or add P's size into a predecessor header. The bound request removes
the terminal extent from the runtime's current-end representation if admitted;
metadata changes alone do not prove backing memory was relinquished.

### Rejection and later service outcomes

Before17F2,1367 reloads DS from CS:1361. FND-CONFIG-167 and
FND-EXE-549 supply the exact normalization/comparison and updater ABI.
17F2 rejects a candidate below the lower bound or above the upper bound
with AXFFFF before calling177C. It does not undo any table-row metadata
change above. Thus even a locally rejected request can leave all three
roots cleared, or a changed last root and modified free-list links.
No native request is necessary for this conditional rejection result.

An admitted comparison calls177C. Cache equality commits the requested
current pair without a native request. The other branch can reach298E's
DOS request. Sentinel FFFF from298E commits the pair and cache; other
results update the upper pair and return failure without locally replacing
the current pair. FND-EXE-549 retains the carry-set BX=FFFF ambiguity,
native preservation and backing effects. All these outcomes occur after
the metadata publications; none has a local root/link rollback path.

The encoded result chain is177C nonzero to17F2 zero, or177C zero to
17F2 FFFF. A direct17F2 bounds rejection also yieldsFFFF. 1367 removes
the four outgoing argument bytes and returns that AX without a status
rewrite. 149B restores its saved registers and DS from1361 and likewise
does not normalize AX. The ordinary outer444C:0092 return clears DX:AX
regardless of this status (FND-CONFIG-165).

For the wrapper's admitted matching-marker path, its earlier marker clear
also precedes all terminal metadata and bound work. A caller storing the
returned null pointer therefore cannot infer that the marker, roots, links,
current pair and upper pair changed transactionally, or that failure preserved
the old heap state. This is an ordering contract, not an observed failed run.

### Preservation boundary

The terminal helper and unlink make no own SS assignment. Their metadata
writes are the named root and link destinations; they do not themselves
clear every removed block's header. The native updater branch retains its
callee/frame/register and storage obligations. Under intact frames, separated
metadata and unchanged1361, the common149B suffix restores incoming DS.
No universal interrupt preservation, actual heap membership or successful
release is inferred from that suffix.

The decompiler exposes1367's calls and root clears but collapses dynamically
loaded headers into fixed data aliases. Instructions decide R/T identities,
the pushed R pair and publication order, retaining FND-CONFIG-227's limits.

## Interpretation

Q-CONFIG-013 can reuse the four terminal choices, exact requested pairs
and pre-request publication order. Together with FND-CONFIG-230 this
classifies ordinary terminal and nonterminal release branches. What remains
is actual root/predecessor/link and bound admission, aliases and backing,
changed frames/segments and external outcomes. Rejected release is not a
state-preserving operation, even on the no-native-call bounds path.

No whole question is answered and no complete-reading or parity promotion
is recorded. No original-game or emulated invocation was made.

## Alternatives

Always requesting P:0000 misses the free-predecessor cases. Always clearing
the roots misses the allocated-predecessor and nonfirst-free-predecessor cases.
Treating root changes as proof of service success reverses their order.
Treating a null wrapper result as a successful release ignores discarded AX.
Treating every failure as involving DOS ignores17F2's local rejection.

## How to reproduce

Reuse heap-coalescing-release-connected.txt in
GAME_DIR/analysis/handler-census-20261106/, from resident_listing.py
1000:1367..1464 and149B..14C4. Read1367 through13CA with the
143B unlink continuation and149B caller; the nonterminal13CA body is
context and its contract is FND-CONFIG-230. Follow each outgoing pair
on the stack, not inferred C argument names. Compose the existing
17F2/177C/298E interface readings in FND-CONFIG-167 and FND-EXE-549
without turning their native conditions into observations.

In the resident read-only Ghidra project, run ReportDecompileMatches.java
at1000:1367 with17f2,143b,135d,135b and135f. Require the1367
function marker in heap-terminal-release-decompile.log under that same
local directory and verify dynamic segments and pair widths in the listing.
Keep all original-derived reports local; no bytes or decompiler output
are committed.
