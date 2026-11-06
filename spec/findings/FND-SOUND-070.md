---
id: FND-SOUND-070
title: Sequence setup passes resource data and the allocated sequence block as separate driver arguments
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47B9:0002..47B9:008B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0430..4734:04D9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0721..4734:0740
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0CBE..4842:0CC4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0CD6..4842:0CDC
tool: resident_listing.py and window_operands.py at 196e25f6; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-016/069, FND-RNG-082/093 and FND-PARTY-321/322
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-069 distinguishes resource output pointer DS:3458
from final setup block DS:344A. This reading supplies their
known call-interface relationship and literal producer component;
it does not equate their backing or assume successful setup.

### Distinct arguments of one setup request

47B9:0002 saves BP/SI/DI and reserves four local bytes. It
requests 4842:0CBE with five formal groups in this order:
current DS:343E word, far pointer DS:3458, incoming word at
BP+6, incoming far pointer BP+8, and far pointer current DS:A3A4.
The outgoing stack occupies sixteen bytes. Stub 0CBE selects
AX 0097 and tail-jumps to the driver dispatcher. FND-RNG-082
already identifies this interface and the sequence-block argument.

The known FM loader passes word zero and DS:344A as the
incoming arguments. Thus its driver request receives resource data
3458 and the sequence block 344A separately. A missing local copy
between the globals is not an unexplained relationship: the two
fields serve distinct interface roles. Actual source and destination
bounds, selected driver and storage identity remain requirements.

0002 holds returned AX in DI. FFFF requests 49DE:000F with
error word 0E, selector 0069 and third word 11. FND-PARTY-321
bounds the conditional fatal route; the following instructions are
not proof it returns through an intact frame. For ordinary admitted
continuation the helper reloads configuration DS:3411 and skips
its patch loop when word +32 is zero or three.

Other values enter a loop requesting 4842:0CD6 with DS:343E
and held DI. Stub 0CD6 selects AX 009B. A non-FFFF returned
word is split into high and low bytes, passed as bank and patch to
47B9:01F5, then the loop queries again. FND-SOUND-016 supplies
the patch-reader interface; driver results, file effects and bounds
remain separate. FFFF ends the loop. There is no local iteration
limit or proof the selected driver eventually returns FFFF.
The common epilogue returns held DI in AX and restores SI/DI/BP.

### Known sequence-block and source-buffer producers

FND-RNG-082 records that initialization requests driver selector
0096 and zero-extends returned AX into dword 4E71:0C10.
The reviewed interval passes that extent to 4AB9:001C and
publishes DX:AX to DS:344A. Null branches to its allocation-error
continuation; nonnull stores progress stage six and allocates the
same extent for separate block DS:3446. The source-buffer branch
then tests byte 4E71:0C0F: one skips the allocation here, otherwise
it passes dword 0B5A to 4AB9:001C and publishes DS:3458.
This interval does not prove the whole initializer's success, prior
state or correspondence with an actual driver block.

Teardown tests current DS:344A, requests release through
4AB9:0043 for a nonnull pointer and explicitly stores dword zero
back at 344A. FND-PARTY-322 records separate release of DS:3458
and the ordinary shutdown callers; its source-buffer publication
uses the release helper's returned DX:AX. These are separate
lifetimes, not a shared pointer inferred from similar use.

### Controlled literal-access components

Window 3447..344F reports seventeen listed operands: seven
accesses to 344A/344C in initialization, teardown and FM setup;
one neighboring 3448 store; and nine accesses beginning at the
separate 344E field. The known 344A producer pair and setup
push are positive controls. Eleven raw candidates remain unpromoted.

Window 3455..345F reports twenty-one listed operands: eighteen
source-buffer accesses/literals, one adjacent 3456 store and two
unrelated immediates 345B. The source allocation stores and
0002's source-pointer PUSH are positive controls. Thirteen raw
candidates remain unpromoted. A passed address at 0552 permits
resource-reader writes without a direct MOV in that helper.
Neither query excludes computed, overlapping, copied or externally
changed pointers or alternate entries. String-store counts are zero
in matching routines; that alone excludes no other writer mechanism.

## Interpretation

Q-SOUND-004 can remove uncertainty about the known 3458/344A
relationship: acquisition data and an independently allocated sequence
block are separate arguments to selector 0097. Remaining evidence
names actual source/block extent and lifetime, driver/patch-loop
termination, errors, flag producers and state/frame preservation.
Q-SOUND-011 may reuse this interface where a buffer alias could change
music-table interpretation; unrelated driver operation is not added to
its layout claim. No complete reading, whole question closure or native
successful sequence setup is claimed.

## Alternatives

- A local copy must connect acquisition to setup: both pointers are
  passed separately to the same driver request.
- A successful-looking handle proves a bounded patch loop: it ends
  only when a driver query returns FFFF, with no local count bound.
- Similar allocation sizes imply shared storage/lifetime: the sequence
  blocks and source buffer have distinct publications and release paths.

## How to reproduce

Read the five location ranges with resident_listing.py at the tool
revision above as music-source-sequence-binding-contract.txt. Run
window_operands.py on the approved executable and DSUN inventory for
3447..344F and 3455..345F, retaining
music-sequence-pointer-window.txt and music-resource-buffer-window.txt.
Keep reports in GAME_DIR/analysis/handler-census-20261106/ and
preserve controls, raw candidates and all exclusions. Reuse the existing
driver-block, heap, patch and shutdown findings. No original game,
emulator session or emulated call supplied evidence.
