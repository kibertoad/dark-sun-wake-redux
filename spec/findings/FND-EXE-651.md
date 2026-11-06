---
id: FND-EXE-651
title: Five direct release sites use four segment-source classes and the input-queue ownership flag depends on setup history
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0092..444C:00F6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4464:0024..4464:0083
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4464:014E..4464:01F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1622..1000:1702
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:174D..1000:177C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000503CE..0x000503D0
tool: Pinned Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9), executable-reader 4.3.0; direct_callers.py, resident_listing.py and window_operands.py at 5310ed70; retained EXE-284/290/292/295/296/297 and CONFIG-165/167 interfaces
environment: null
---

## Observation

This qualifies the direct release-dispatcher argument producers relevant
to FND-EXE-650's released-segment domain. The existing direct_callers.py
census for 1000:149B reports two relocated far sites and three near sites
preceded by push CS. The sites below are on the connected paths already
identified by the retained entry readings and checked in their contexts.
No computed-call/jump or complete caller-absence claim follows.

149B consumes only its argument's segment word at BP+8. The low word at
BP+6 does not select the released header (FND-CONFIG-167).

| Direct site | Segment supplied | Low word and producer condition |
| --- | --- | --- |
| 444C:00E6, file 397A6 | Wrapper's incoming far-pointer high word | Incoming low word is forwarded too. Null and marker-failure paths still reach the call after the wrapper's prior metadata read; the marker is not a release admission gate. |
| 4464:006D, file 398AD | Then-current DS:33CA | Then-current DS:33C8; exact-one 33CC gate, two preceding callees and then exact-one 33CE gate. |
| 1000:1693 | Saved original existing-allocation segment | The low word is allocator-return BX retained before copying; it is ignored by 149B. The new allocation segment is saved separately for the final return. |
| 1000:16DE | Remainder segment DI=(B+p) modulo 65536 | Low word is p. This is the nonterminal larger-existing-count branch, not the original block's release. |
| 1000:176A | Existing segment BX from 1702's input | Zero low word on the nonzero-existing-segment/zero-request branch. No marker wrapper supplies an ownership check here. |

The second site is the same physical instruction as 444C:01ED in
FND-EXE-284: 4464:0024 and 444C:01A4 are aliases of its cleanup entry,
not two different callers. The body reads the ownership flag and pointer
after 44D0:0040 and 44B6:0075; it does not use a pointer privately
captured before them. Entry CLI and saved flags do not establish atomicity
or preservation through every child or external effect.

FND-EXE-297 retains the copy-before-release path at 1693. Its saved
original-segment argument can be trusted only with the stated frame/copy
separation and retained-stack conditions; the maximum count formula alone
is not an argument-preservation proof. FND-EXE-296 retains the remainder
publication before 16DE. If its existing B is in FND-EXE-650's domain,
p>0 and mathematical B+p<=FFFF, the remainder segment stays there.
Its count and following-header writes still need their own extent inputs.
The 176A branch forwards its existing segment without normalizing the
ignored input offset or validating an allocated header.

### Ownership flag and saved input pointer

The controlled 33B8..33D0 operand census uses the unchanged declared-root
reach report retained by FND-EXE-647. It lists 123 operands. The direct
flag accesses are the read at 4464:005E and the writes at 4464:01A3
(one) and 01B3 (zero). Pointer publications are 01CC to 33CA and 01D0
to 33C8. An additional 33CE-indexed read candidate lies inside the
manager instruction stream; it is not another admitted flag writer.
The two real flag stores and the cleanup read are positive controls.

This is an encoded-operand census with a raw candidate pass. Stores via
adjusted bases, wider spans beginning elsewhere, string/block copies,
changed DS, native effects and other implicit aliases remain excluded.
In particular FND-INPUT-016's packet writer has a destination through the
saved buffer pointer; its actual destination must be classified rather
than dismissed because it has no absolute 33CE displacement.

Checked-image metadata reads source word DS:33CE at file 503CE as zero,
with no overlapping MZ relocation. That is the unmodified loaded-image
value, not every later flag value.

Setup 4464:014E has the direct caller at 39D1:0173 in the current direct
census, matching FND-EXE-292. Under its retained DS/pointer bindings that
caller tests a 444C:00FA result nonzero and supplies it with size 0400.
The setup's supplied-pointer arm publishes that pointer without writing
33CE. Only its null-pointer arm calls 15A5 and writes one at 01A3;
if both returned pointer words are zero it clears the flag at 01B3.
Cleanup does not clear the flag or the saved pointer on its own paths.

Consequently, for a history beginning with flag zero, using only that
recorded nonnull direct setup arm and the recorded cleanup, with the
same data-field identity and no excluded flag mutation, the flag stays
zero and the cleanup's 149B call is bypassed. This does not establish
every actual input/setup history, but removes the inference that the
recorded first supplied-buffer setup necessarily reaches that release.

Conversely, an admitted null-buffer setup can leave flag one. A later
cleanup can clear 33CC without clearing 33CE; a subsequent admitted
supplied-buffer setup does not reset 33CE. A later cleanup can therefore
forward the newer supplied pair through its exact-one flag test, provided
the gates/callees and storage preserve that history. This is a local
conditional history, not an observed sequence or a claim that another
setup caller exists. The flag does not by itself identify the origin of
the current pair on every history.

## Interpretation

The five direct release argument sources are now explicit. Q-EXE-033
needs the wrapper/existing-pointer histories, the saved-copy frame, the
remainder's nonwrapping extent and the queue's post-callee flag/pointer
history, rather than an unspecified release helper. The queue flag has a
concrete producer/gate model with both a bypassed direct-history case
and a retained-history case that reaches release.

Actual pointer domains and metadata histories are not proved by these
source identities. FND-EXE-649/650's numeric bounds still require those
inputs. No queue closure, complete-reading declaration or parity status
change is made.

## Alternatives

- The marker blocks release of an unowned pointer. FND-CONFIG-165's
  own branches contradict that gate interpretation; both paths call.
- Every 149B argument refers to a newly returned allocation. The copy
  branch releases the saved original, and shrinking releases a computed
  remainder. The input wrapper uses later shared fields.
- The ownership flag always describes the current pointer. The supplied
  arm's unchanged flag and cleanup's absent flag clear make that inference
  depend on the actual history and excluded mutations.
- The initial zero proves no cleanup release ever occurs. Other setup
  arms, indirect/alternate callers, changed fields or aliases remain open.
  The controlled reach report retains unresolved transfers, gaps and
  contested decodes; it is not a whole-program negative proof.

## How to reproduce

Use the licensed installed DSUN.EXE, size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run direct_callers.py for 1000:149B
and 4464:014E. Reuse the named entry readings and run resident_listing.py
for 444C:0092..444C:00F6, 4464:0024..4464:0083,
4464:014E..4464:01F3, 1000:1622..1000:1702 and
1000:174D..1000:177C. Track outgoing words and consumed BP+8,
keeping the pending copy arguments and the remainder distinct.

Run window_operands.py with the retained controlled
header-pointer-reach-report-20261106.json and window 33B8..33D0.
Require the two flag stores and one gate read as controls, and retain all
candidate/implicit-writer exclusions. Use the checked readMz API at load
1000 to resolve 57E0:33CE and verify source zero and no intersecting
relocation word. Follow both setup pointer arms and both cleanup gates;
do not assume callee DS or shared-field preservation.

Reports retained under GAME_DIR/analysis/ are
heap-release-direct-callers-20261107.txt,
input-queue-setup-direct-callers-20261107.txt and
input-queue-ownership-operands-20261107.txt. No original game run occurred.
