---
id: FND-ITEM-099
title: Immediate effect handling decodes a signed effect byte and gates casts through target-list and distance results
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007ED9D..0x0007F0B7
tool: tools/research/exec-census/overlay_listing.py at aba65d70; Python 3.12.10, Capstone 5.0.7; composition with FND-MAGIC-019 and FND-ITEM-068/098
environment: null
---

## Observation

Q-PARTY-116 retains the immediate child between slot production and
sampled constructors. FND-ITEM-098 binds overlay 193 +0AAD with the
ordinary row index and mode zero; FND-ITEM-068 also calls it on accepted
effect-row indices. This records local decode/target/dispatch gates, without
admitting geometry buffers or casting children. Offsets and masks below
are hexadecimal; arithmetic counts are decimal.

### Row and effect decode

The entry reads slot word +6 from the supplied eight-byte row through
DS:67B7 before any own row-index/count gate. A signed negative slot returns
AL zero. Other slots select their 37-byte record without an upper bound.
Object byte +0F is sign-extended to a word and decremented. Only resulting
FFFF returns AL zero at this gate; thus object byte zero is the sentinel.

Bytes 1 through 127 decode to effect indices 0 through 126. Bytes 128
through 255 decode to signed -129 through -2 and do not take that sentinel
exit. Later 18-byte-row indexing uses word multiplication, without a
separate nonnegative or capacity test. FND-ITEM-098's byte-width store of
effect-number-plus-one is consequently not an unsigned inverse for every
input byte: inputs 0..126 round-trip under intact storage, 127..254 reach
these negative decoded words, and 255 stores zero and takes the sentinel.
Actual effect-number/table admission remains open; no native high-byte
input or invalid access is claimed.

### Geometry and target enumeration remain prerequisites

The effect row's byte +5 high nibble selects geometry. Zero requests DATA
through 2C5F:0003 using effect word +0, captures its unsigned word +9
divided by two, and forms a rectangle around the selected placed position.
Nonzero instead derives a rectangle and centre from placed/object fields,
including anchor offsets and unsigned dimension bytes; its radius is half
the smaller of those dimensions. These word-width calculations, their
pointer/segment identity and valid frame/metadata inputs require admission.

It passes the rectangle, zero, FFFF and a near stack-list address to
overlay 173 +389F, retaining returned AX as a signed list count. Zero
and negative counts produce no target iteration. Positive counts iterate
that many four-byte list records, taking the slot word at each record +2.
The consumer has no own list-capacity or target-slot bound. Near stack
reads use DS, requiring the appropriate DS/SS relationship and nonaliasing.

For each target it requests 2D40:382F from the target's current object
coordinates to the retained effect centre. Its returned signed distance
must be strictly below radius plus half the target's unsigned dimension
byte +13. Equality skips the target. Arithmetic is word-width; actual
distance semantics and source/target spans remain separate obligations.

### Selected immediate mode and result

For a passing target, mode byte zero calls overlay 193 +15B9(target,
decoded effect's low byte). Only exact AX FFFF continues to +12A8 with
target, the decoded effect word, source level/kind from object byte +15,
mode zero and final zero. It then calls +1519(target, effect low byte)
and sets its local result byte to one, without testing these latter results.
FND-MAGIC-019 supplies +12A8's mode-zero DATA/cast routing; their effects
are not assumed to preserve placement rows/counts.

Nonzero mode instead prepares a target-word payload and requests queue one,
kind five at signed clock-dword divided by sign-extended DS:9C0B plus one.
The divisor has no own zero guard; its validity and arithmetic admission
remain required for that branch. The selected mode-zero callers skip this
division and deferred request. That deferred branch does not set the local
result byte. The final AL reports whether the immediate path set that byte,
not a complete-effect or preserved-state success predicate.

The body's own persistent effects are requests to children; its explicit
result/list/geometry writes are local. Valid disjoint stack and pointed
storage are needed before classifying those local stores as harmless to
shared state. SI/DI/BP are restored on exit; child preservation at their
intermediate uses remains open. The slot producer ignores this AL before
its later cell/queue work and return to sampled construction.

## Interpretation

This removes immediate handling's local signed-byte decode, target/count/
distance/mode gates and return-byte meaning from Q-PARTY-116. Actual
effect domain, DATA/geometry identity, target-list production/bounds,
distance and +15B9/+12A8/+1519 child effects and preservation still matter
for slot/count/row state at constructor entry. A returned slot or handler
AL alone cannot establish those lifetimes.

## Alternatives

Unsigned effect-byte decoding would change high-bit cases. Treating every
nonzero target count as iterated ignores the signed loop. Treating equality
as a distance hit changes its strict comparison. Treating AL zero as no
child work ignores enumeration and deferred requests. No native defect,
complete-reading declaration or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with overlay_listing.py for descriptor 193, file range 7ED9D 7F0B7.
GAME_DIR/analysis/item-removal-20261104/ holds
effect-immediate-handler-contract.txt and its -repeat pair, both SHA-256
7087af5d67dd8f7f8c0f8b6b2b5723e5ffff8acfc1a079a61f643f441d8a4900.
Decode from the established entry, retaining all geometry/list/child limits.
Keep reports local. No original game or isolated function was executed.
