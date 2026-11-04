---
id: FND-ITEM-062
title: Equipment casts start with the owner as one target and return a continuation counter before draining queued effects
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E2F0..0x0007E907
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000120E8..0x00017C28
tool: tools/research/exec-census/overlay_listing.py and tools/evidence/report.mjs table at 7c81b0f9; joint item inputs of FND-ITEM-060
environment: null
---

## Observation

FND-ITEM-061 binds the equipment request to 193 +0000 with owner as both
object arguments, selector D, level six, a far pointer to an initially zero
caller-local byte and final byte zero. This follows that selected route
through 193 +003C's full local body and the returning +0000 wrapper. It
removes the initial target-list and continuation-result alternatives from
Q-PARTY-109, without assuming preservation across the named children.

### Entry, DATA snapshot and first target

The +0000 wrapper forwards its arguments to +003C once. It saves the low-byte
result, unconditionally calls 179 +11DC to drain queued effects, then returns
the saved byte. There is no local retry loop or result-dependent skip of the
queue drain. FND-PARTY-107 and FND-MAGIC-020 describe the queue contract;
queued casts are additional work, not a second +003C call in this wrapper.

Before its first child call, +003C initializes local total/result words and
reads initial caster state. DS:45AA chooses either the script-position
globals or the caster's 37-byte slot and combatant metadata. A non-minus-one
second object supplies its position/extent-derived center and replaces the
four incoming position words. Thus the equipment caller's initial zero
position words are not retained coordinates under an admitted owner 0..47.
All record indices, segment identity and arithmetic/storage spans still
need valid native admission.

The body gets DATA D through 2C5F:0003, then copies exactly 73 bytes from
the returned far pointer into SS:BP-6C using 1000:0452. It replaces its local
DATA pointer with that SS snapshot. FND-CONFIG-099 supplies the bounded
forward-copy contract. No null/result test separates the getter from this
copy; source/cache/read and nonoverlap validity remain prerequisites.

The equipment final byte zero selects two sound requests using snapshot
bytes +0F and +16 through 2C5F:0A22, then jumps directly to +041B. It skips
the intervening presentation/object branches. These requests precede the
hit even on this route; FND-SOUND-007 describes their gates and children.
This is not a proof of audible output or sound-child flag preservation.

At +041B the admitted non-minus-one owner constructs a one-element local
list: its auxiliary word zero, target word owner and count one. It skips
the ordinary area-list construction at +0435. D=71 would subsequently call
197 +399F on that list; none of the 93 canonical property-one candidates
has D=71. With that source condition retained, the local signed loop makes
one dice request at level six and one 179 +0D2F hit request, with owner as
target and caster. The hit's low byte is ORed into a separate aggregate word.
Actual child writes and list/frame preservation remain required; this is
not a claim that a native hit succeeded or had only one downstream effect.

### Post-hit area pass is a separate gate

After the hit loop, D=26, 150 or 310 selects an extra target-list request
directly. Other selectors reread DATA D and test word +1A mask 02. A set bit
also requests 208 +0000 with the owner's position, D, kind -1 and mode one,
then calls 371D:050F(target, zero) for each returned target. This second
loop does not call the hit routine at its own site. Its outputs and effects
remain independent child contracts.

None of the 93 canonical candidates has an exceptional D above. Of their
matched canonical DATA rows, 92 clear word-+1A mask 02 and one sets it:
D=42, paired with RDFF/31030 chunk zero. Thus this source candidate alone
selects the extra area-pass gate under successful matching rereads. This
does not imply that an observed native area list is nonempty.

All 93 source DATA rows clear word-+11 mask 4000, so the conditional calls
between the initial hit and post-hit area gate are excluded when the local
snapshot is retained. Later bit-one aggregate handling can still call
195 +09A7 for an owner above minus one. No own instruction in +0000/+003C
reads or writes DS:6174; their children are not a closed flag-preserving
graph. In particular, the wrapper drains queued effects before returning
to the equipment caller that clears the flag.

### Continuation byte differs from the hit flags

At the tail, a null output pointer returns AL zero. Otherwise let C be the
signed byte through the far output pointer and R be bits 2..7 of byte zero
of the local DATA snapshot. The routine compares C signed with R-1. When
C is smaller it increments the byte and returns AL one; otherwise it writes
zero and returns AL zero. It does not return the aggregate hit flags here.

For the 93 source candidates, R is one for 92 payloads and four for one
payload (RDFF/31962 chunk zero, D=159). If the caller's initialized C=0 and
the matched snapshot remain intact to this tail, the 92 reset C to zero
and return zero, while the remaining one changes C to one and returns one.
The outer equipment hook does not retry on that one or test this result;
the wrapper still drains its queue and returns the saved byte, and the
equipment hook clears the flag on normal return. Neither zero nor one is
by itself proof of effect failure or success.

## Interpretation

Q-PARTY-109 now has the selected local main-cast path, its sound and DATA
children, initial single-owner target, separate post-hit area gate and
continuation-byte contract. The remaining work is actual child and queued
effects, source/cache/snapshot/list/output/frame preservation, use-time
flag values and independent flag writers/consumers. FND-MAGIC-018's duration
consumer cannot be assigned value one merely from the outer publication.
No runtime outcome, rule status, parity or complete-reading promotion follows.

## Alternatives

An area list at the initial selection would require the minus-one target
branch. Excluding every later area pass would ignore the post-hit DATA mask.
Treating AL as the hit result would ignore the separate aggregate and tail
counter. Retrying until AL zero would add a loop absent from +0000 and the
equipment caller. Clearing the flag before queued effects would change the
outer call order. A snapshot does not validate its original source pointer
or prevent frame aliases and later writes.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for descriptor 193,
range 7E2F0..7E907 hex. placement-cast-prefix.txt under
GAME_DIR/analysis/item-removal-20261104/ retains the complete entry-based
wrapper and main body. Check every selected zero-final-byte branch through
the return, both list gates, sound requests, DATA snapshot and counter tail.
Reuse FND-ITEM-061's argument binding and FND-PARTY-107's queue contract.

For RESOURCE.GFF length 5724669, XXH3-128
92fe031616f36062f99484b0f728210e, run report.mjs table at start 120E8 hex,
count/limit 320, stride 73, unsigned two-byte fields flags at offset 17 and
typeMask at offset 26. Count evidence is the independently verified DATA
catalog of FND-ITEM-057. Join its rows by D to FND-ITEM-060's item report,
selecting 1<=W<=319 and DATA[W-1].property & 3 equal to one. Count masks
4000 and 02, exceptional D values, and property >> 2; reconcile with 93.

placement-cast-data-controls.json, placement-cast-data.json and its
-repeat.json remain beside the listing. Both reports are byte-identical;
SHA-256 is 6da1db14f48da38c39fbc072766be57586d84af7bfce6484b83ca5d072c0bfcc.
The original is inspected read-only and never executed. No original code,
record fixture, sound or resource payload is committed.
