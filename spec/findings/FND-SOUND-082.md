---
id: FND-SOUND-082
title: Sequence setup requires patch-cache progress because ADV 11 rescans missing instruments and installation can retry unchanged
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
    address: 4842:0CD6..4842:0CE2
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000833D4..0x00083407
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00083521..0x0008357B
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008357B..0x000836C6
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000836C6..0x000837A9
tool: resident_listing.py and adv_function_stores.py at c5dc0943; Ghidra 12.1.3 RecoverCitedFunctions, ReportDecompileWindow, ReportInstructionWindow and ReportDataBytes; retained source/operand review at 196e25f6; FND-SOUND-016/069, FND-CONFIG-237, FND-RNG-082/093 and FND-PARTY-321/322
environment: null
---

## Observation

This whole-entry replacement retains FND-SOUND-070's argument and pointer
producer assessment. Its seventeen/twenty-one operand measurements below are
historical components at their recorded revision, not a new census against
the current inventory. Fresh evidence here reads the caller's query/install
loop together with ADV 11's cache membership, insertion and eviction paths.
No game, driver or emulated function was executed.

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

### ADV 11 returns the first missing cache key

RESOURCE.GFF is the installed 5724669-byte file, XXH3-128
92fe031616f36062f99484b0f728210e. ADV 11 occupies
0x00081DDF..0x00085DC4, XXH3-128 0faa31676d7d993a393da11bde9e7885.
The single source-order table entries 009B and 009C point to resource-relative
1742 and 18E7. Stub 4842:0CDC selects 009C; the caller at 47B9:021E
passes driver word, bank word, patch word and the acquired far blob pointer,
removing ten argument bytes afterwards. It discards any driver result and
requests release of the blob. Reuse FND-SOUND-016 for its file-reading path;
a returned status is not a cache-membership check.

009B reads its sequence handle from SS:[BP+8]. FFFF, a zero segment in the
sequence's leading far pointer, or a failed TIMB marker comparison produces
FFFF without proving any instrument was loaded. Other handles dereference
the CS:293B pointer array as FND-CONFIG-237 describes. A recognized TIMB
pointer advances by eight bytes, supplies a 16-bit count, then supplies packed
two-byte instrument keys. Each key is looked up through 15F5; the first
missing key is returned. When every tested key is found, the query returns
FFFF. It has no persistent next-item cursor or non-stack store.

The cache lookup 15F5..1628 tests indices 0 through 191. An entry is a match
only when flag CS:0C4B[index] has bit 80 set and its bank/patch bytes at
CS:0ACB[index] and CS:0B8B[index] match the key's high/low bytes. It returns
the index or FFFF and changes no cache membership. The unsigned limit test
is at 1618..161E; this is a bounded search, not a payload bound.

In 009B, DI receives the count at 176F and the first key is tested before
DI is decremented at 1784. With an unchanged positive count N, valid storage
and returning lookups it tests at most N keys. A count of zero does not
mean an empty scan: the post-test 16-bit decrement can wrap and permit
65536 tests if no earlier missing key stops it. No extent check proves
those reads remain in the TIMB payload; actual count, offset and backing
admission remain required. Repeated outer calls restart at the first key.

### Installation and the actual progress obligation

009C packs bank/patch bytes at 18F3..18FB and uses the same lookup. A hit
skips inserting another blob. A miss with a null input pointer skips the
insertion path; neither case is an unconditional newly-loaded-key result.
The insertion path scans for a slot whose bit 80 is clear. It compares the
16-bit sum of used bytes CS:0D11 and the blob's leading length word against
capacity CS:0D0F at 192B..193A. Nonwrapping arithmetic, valid blob/arena
bounds and storage disjointness cannot be inferred from that predicate alone.

With a slot and admitted space, it publishes the new used-byte count, key
bytes and flag 80 before copying the blob through the arena pointer
CS:0D0B and updating channel mappings. A source-copy failure or alias is not
proved absent merely by finding those stores. With no slot or a failed
space predicate it instead calls eviction 179C and unconditionally restarts
at 1917; it does not test whether eviction removed anything.

Eviction scans 192 flag entries and excludes entries without bit 80 or with
bit 40. Among eligible entries it selects by the stored two-word age. When
none is eligible, 17EE..17F4 goes straight to the epilogue at 18E0, leaving
cache membership and used bytes unchanged. The other arm removes an entry,
compacts its stored bytes and updates offsets/mappings; that arm can remove
a key required earlier in the same TIMB list.

A concrete conditional no-progress path follows: assume a missing key,
nonnull input, valid/disjoint frame, unchanged driver state, and all 192
slots occupied with bit 40 set. The installer finds no slot; eviction finds
no eligible entry; the retry encounters the same state. This is a static
path argument, not a claim that shipped play reaches those conditions or a
recorded hang. Actual flag writers and cache/arena admission decide whether
it can happen in the selected game path.

For a stable, admitted positive-count TIMB list, one sufficient outer-loop
progress condition is that every returning patch handoff adds its requested
missing key and retains every already-resident required key, with no other
change to the list, cache identity or frame. This also excludes the early
handle/pointer/header bypasses and a required key equal to FFFF, which would
be indistinguishable from the caller's stop marker. The finite required-key set then
strictly grows until the query returns FFFF. The caller's ignored driver
result, eviction of earlier keys and unchanged retry prevent assuming this
condition. An arbitrary handle or finite list alone supplies no such proof.

009B and 009C save flags and mask interrupts, then use the separately entered
IRET bytes at 1791 and 19BD to restore saved flags before their final far
returns. Source-byte controls at 1790 and 19BC verify that overlapping stream,
following FND-CONFIG-237/FND-EXE-594. Ghidra's inferred offcut-call signatures
are not the frame or return proof. Externally changed or aliased storage is
not excluded by the local instruction reading.

## Interpretation

Q-SOUND-004 can remove uncertainty about the known 3458/344A
relationship: acquisition data and an independently allocated sequence
block are separate arguments to selector 0097. Remaining evidence
names actual source/block extent and lifetime, driver/patch-loop
termination, errors, flag producers and state/frame preservation. For the
ADV 11 patch branch the remaining progress condition is now explicit:
actual TIMB count/backing, required-key cache retention, insertion/eviction
eligibility and arena capacity, and preservation across file/driver/release
calls. Local lookup and restart behavior are no longer unspecified.
Q-SOUND-011 may reuse this interface where a buffer alias could change
music-table interpretation; unrelated driver operation is not added to
its layout claim. No complete reading, whole question closure or native
successful sequence setup is claimed.

## Alternatives

- A local copy must connect acquisition to setup: both pointers are
  passed separately to the same driver request.
- A successful-looking handle proves a bounded patch loop: the query rescans
  missing membership, and the installer can retry unchanged. Conversely FFFF
  can mean a bypassed/invalid TIMB prefix, not only all keys resident.
- A finite cache or finite list proves outer progress: eviction can lose a
  required key, and no-eligible-entry retry has no local decreasing measure.
  The conditional path above is not established as reachable in shipped play.
- Similar allocation sizes imply shared storage/lifetime: the sequence
  blocks and source buffer have distinct publications and release paths.

## How to reproduce

For the retained components, use the tools and DSUN inventory at 196e25f6.
Read the five DSUN location ranges with resident_listing.py as
music-source-sequence-binding-contract.txt. Run window_operands.py for
3447..344F and 3455..345F, retaining
music-sequence-pointer-window.txt and music-resource-buffer-window.txt.
Keep reports in GAME_DIR/analysis/handler-census-20261106/ and
preserve controls, raw candidates and all exclusions. Reuse the existing
driver-block, heap, patch and shutdown findings. No original game,
emulator session or emulated call supplied evidence.

Fresh reports are under GAME_DIR/analysis/patch-enumeration-20261107/.
Run adv_function_stores.py on installed RESOURCE.GFF with selectors 9B 9C
only as a candidate classifier; the other driver results are not semantic
coverage claims of this finding. The source identity and ADV 11 slice are
those verified in FND-CONFIG-237. In its isolated ADV11 project, use read-only,
no-analysis headless processing. Recover entries 1742/18E7 and 15F5/179C in
separate reports, requiring two recoveries and zero failures in each.
ReportDecompileWindow uses windows 1..100/1..120 and 1..80/1..120;
ReportInstructionWindow uses counts 90/130 and 80/160. Respect the exact
function spans above when a window continues beyond a function.
ReportDataBytes uses seven bytes at 1790 and 19BC for the return controls.
The source-order table control reads the resource's word-zero table offset
and selector/entry pairs through FFFF, requiring one selected pair per query.

Use resident_listing.py on DSUN.EXE for 47B9:0002..47B9:008B,
47B9:01F5..47B9:0236 and 4842:0CD6..4842:0CE2. The retained
ADV11-enumeration-install.log, ADV11-lookup-eviction.log,
ADV11-return-controls.log, game-query-patch-loop.txt, query-install-stubs.txt
and table-order-control.json supply the fresh outputs. No supplied cache
state was seeded or executed. The conditional retry and count-zero cases
come from the instruction order and word arithmetic, not a runtime fixture.
