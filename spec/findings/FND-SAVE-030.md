---
id: FND-SAVE-030
title: The runtime's stored stream-exit hook conditionally flushes four fixed records through a separate DOS-write route
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F14..1000:3F37
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:36DE..1000:36EA
tool: resident_listing.py at aa68664b; connected FND-EXE-500/503/504/558/559/579 and FND-SAVE-027/028 interfaces
environment: null
---

## Observation

This classifies the known DS:3676 exit-hook target separately from the
registered167B:0029 callback and archive close-all. Installed DSUN.EXE
is634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
It identifies a conditional write-request route, not an observed native flush.

### Hook selection and four-record loop

FND-EXE-579 identifies the stored callback used at1000:03B1, shipped
as the return1000:0387 and explicitly published as1000:3F14 by
36DE..36EA. FND-SAVE-027 places this callback after the initial
exit callbacks and exit-procedure records, before the runtime's later
vector restore and termination sequence. Actual publication and unchanged
pointer/frame state remain prerequisites to selecting3F14.

3F14 saves SI/DI, sets DI=4 and SI=3682, then tests each record's
word+2 for any bit in0003. A selected record is passed by near offset
to2F13 through PUSH CS/near CALL, with two-byte caller cleanup.
The caller does not inspect returned AX. Each returning iteration advances
SI by16 and decrements DI. With admitted segments and callee preservation,
the four record starts are3682,3692,36A2 and36B2. This fixed loop is
distinct from2F94's aggregate walk using a configured count.

The hook does not itself close handles, check delivered counts, or produce
SAVE payloads. It returns after its loop and restores the saved SI/DI.
No absence of transitive writes follows from that short body.

### Connected flush and native-write conditions

FND-EXE-503 completely reads2F13. Each nonzero supplied record offset
must match its word+14. A nonnegative signed word+0 takes its recorded
local count/pointer branches without the negative-count write request.
A negative word+0 instead computes quantity word+6 plus word+0 plus1,
subtracts it from word+0 and resets word+10 from word+8 before
calling3DCC. Its arguments are the sign-extended byte+4 handle,
word+8 source offset and computed quantity. Failure does not undo those
earlier stores; flags0200 can suppress its mismatch error result. The
outer four-record hook ignores that result in either case.

FND-EXE-504 supplies3DCC's full downstream contract. A handle outside
current37C2 is rejected. Quantities0 andFFFF have their own no-write
return. Other admitted quantities can reach3EDA directly, or its text
translation/chunk path, according to indexed handle flags. The optional
positioning call and DS-based stack-offset handling retain their recorded
preservation, alias, DS/SS and native conditions.

FND-EXE-500 supplies3EDA: indexed handle flag0002 rejects locally;
otherwise it requests DOS INT21 with AH40, BX=handle, CX=count and
DX=source offset. This is the independently recorded census site3EFB,
separate from44DE:0264/0285 used by the archive writer. Native output
success and the identity of the handle's target are not established by
the request. In particular, a sign-extended record byte is not proof
that this write goes to a saved-game or working-archive file.

### Distinct registered callback and retained runtime gap

The other known callback167B:0029 calls1425:1684 and then the
delete wrapper44DE:02A9 with DS:4022, as FND-EXE-558/559
already record. Its slot dispatch and native deletion retain their own
state/target conditions; it is not the four-record stream hook. Reuse
that complete local evidence rather than treating all exit callbacks as
one flush operation.

The current load_entry.py archive_fields reader records active/head
pointers and their handle fields but does not read the dirty byte+2C
at shutdown. Its retained load reports therefore cannot establish a dirty
shutdown instance. The historical bug_exit.py driver likewise supplies
no current-pin, state-admitted shutdown dirty-record observation through
its documented contract. No new native run was attempted here.

## Interpretation

The Quit-side rule can describe two known conditional pending-write
mechanisms: dirty archive preparation and this stored stream-exit hook.
Neither mechanism by itself implies a new gameplay snapshot or successful
persistence. Q-SAVE-007 retains actual stored-hook/record/handle state,
the indirect slot targets, remaining service/callback routes and native
outcomes, rather than an unread generic stream callback body.
No complete-reading declaration or whole queue answer is recorded.

## Alternatives

Treating every selected stream as a write ignores2F13's signed-count
branch and lower rejection gates. Treating every zero flush result as
successful output ignores the suppression flag and discarded return.
Treating3F14 as the configured-count aggregate confuses distinct loops.
Treating167B:0029 as the stream hook confuses distinct callback storage.

## How to reproduce

Run resident_listing.py1000:3F14..3F37 and36DE..36EA, retaining
shutdown-stream-table-dispatch.txt under
GAME_DIR/analysis/handler-census-20261106/. Qualify the stored pointer
and03B1 consumer with FND-EXE-579 and FND-SAVE-027. Compose
FND-EXE-503/504/500's complete flush/write readings; bounded checks
are retained as shutdown-stream-close-prefix.txt and
shutdown-stream-counted-write.txt in the same directory. The prefix
includes neighboring context, not a replacement for the full2F13 reading.
Verify signed count, pre-call stores, handle/quantity gates, text branch
and the3EFB request separately. Reuse FND-EXE-558/559 for the distinct
registered callback. Do not infer shutdown dirty-byte observations from
load-time pointer/handle reports. All original-derived reports stay local;
no original or emulated function was executed.
