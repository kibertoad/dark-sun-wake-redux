---
id: FND-SAVE-028
title: Shutdown close-all can request pending archive metadata writes through the existing dirty-record preparation path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:02B5..38FF:0339
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:022C..44DE:02A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067A06..0x00067A11
tool: resident_listing.py at 3dfbbc59; retained q007_dsun_census reports and dsun_interrupt_census.py; FND-COMBAT-096/113/121/122/123/124 and FND-SAVE-015/027 interfaces
environment: null
---

## Observation

This connects the shutdown root's close-all argument to an existing
file-write mechanism for Q-SAVE-007. It reuses controlled reports before
adding tooling. Installed DSUN.EXE is634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The result is a conditional request
path, not an observed successful native write or an automatic new save.

### Close-all dispatch and failure boundary

FND-SAVE-027 qualifies overlay180+0762's call at+0829 into
38FF:02B5 with double-wordFFFFFFFF, after the graphics/sound cleanup.
02B5 first requires the archive-signature check to return a nonzero byte.
For that all-ones argument it repeatedly reads active record DS:9D9F.
Null ends successfully. Nonnull supplies the record's full double-word
handle at+28 to a recursive02B5 call through PUSH CS/near CALL.
Any nonzero returned AX stops close-all withFFFF; zero repeats against
a fresh active pointer. There is no separate count or iteration budget.
Termination depends on admitted record/link updates and stable call state.

The ordinary-handle path resolves a record, fails with error18 if null,
then calls37FC:08FB before closing its word handle at+4 through
44DE:003A. FND-COMBAT-096 supplies that preparation/close ordering
and later list mutations. A preparationFFFF result prevents that own
close request and causes close-all to stop; it does not undo requests
already made by earlier records or earlier preparation steps.

### Dirty preparation reaches two counted writes

FND-COMBAT-096/124 completely read08FB's own branch. A zero
record byte+2C skips its file requests. Nonzero instead seeks to zero,
requests28 header bytes from record+10 through44DE:022C, seeks
to the directory position at+1C, requests the length at+20 from
record+31 through the same writer, then requests the AH68 handle
operation. Each returned position/count/status has its recorded check.
Success clears+2C; a failed check returnsFFFF without rolling back
earlier byte requests. Fresh field reads require the existing identity,
capacity, same-handle and callee-preservation conditions.

FND-COMBAT-113 supplies022C's counted-write interface, including
its DOS AH40 sites0264/0285 and partial/error behavior. Its write
quantity is selected by the passed count, not by the name of the outer
operation as shutdown or close. FND-COMBAT-121/122/123 supply
concrete metadata-changing producers of the dirty byte; dirty state is
not merely an invented flag value. Their actual record/graph/lifetime
admission is still required at shutdown.

Thus the known close-all path can request pending header/directory
writes before exit. This path does not itself gather the live SAVE1..61
state described by FND-SAVE-015 or copy DARKRUN.GFF to a numbered
save. Such operations would require their own reached callers. Which
file an actual record handle names and whether the OS completes a write
remain unproved here; no live saved-game or working-archive change is
reported from this static composition.

### Reused interrupt census and controls

The retained q007_dsun_census-report.json matches this source identity,
Capstone5.0.9 and pypcode4.0.1. It reports1363 routines,105647
instructions,84 unresolved transfers, five gaps and four contested sites;
negativeUsable is false, with neither truncation nor instruction limit.
These are the report's original scope and counts, not a shutdown-only walk
or proof of absent file operations.

Its associated after-interrupts.txt lists ten AH40 site classifications.
Five are reached in that declared walk:1000:02AA,1000:3EFB,
15F3:01A4 and44DE:0264/0285. Five remain candidates in that walk:
1000:02EC,1000:12AF/12B9 and15F3:0871/0881. A candidate label
does not establish that its bytes execute, or exclude it from a different
entry route. The contested02EC region and its incoming branch retain
their original qualifications.

The independently read022C write requests in FND-COMBAT-113 and
archive header-write call in FND-COMBAT-096 are positive controls for
the same mechanism this shutdown preparation uses. FND-SAVE-015's
explicit Save producer remains separate from this close-time flush; its
full lower transfer binding is not inferred from the operation's name.
The known diagnostic02AA site is another control, but its handle-two
binding cannot be propagated to the other writers.

The report also retains the computed-AH interrupt1000:2F0F and
runtime interrupt-dispatch calls at1F10/1FBC. Direct create/delete/rename,
attribute and commit requests, other service selectors, unsearched code,
computed transfers and external callbacks require their own classifications
for a transitive no-write claim. AH40 sites alone do not census every
way a file's contents or metadata can change.

## Interpretation

Q-SAVE-007 now has a positive conditional shutdown-to-write-request
path with named dirty producers, not just an absence of an explicit Save
call. Its remaining scope is actual record/handle/dirty and external
admission, explicit-save transfer controls and unresolved callback/service
mechanisms. This does not establish that Quit saves current gameplay,
that every close writes, or that shutdown is universally write-free.
No complete-reading declaration or whole queue answer is recorded.

## Alternatives

Treating close-all as read-only ignores08FB. Treating every close as a
write ignores its clean-byte branch. Treating metadata flush as a new
gameplay snapshot adds calls absent from this branch. Treating a partial
interrupt census as a global negative ignores its negativeUsable flag
and explicit undecided transfers.

## How to reproduce

Reuse the matching q007_dsun_census-config.json/report.json and
after-interrupts.txt in GAME_DIR/analysis/handler-census-20261106/.
Check source identity, versions, counts, limits and negativeUsable before
reusing any classification. Regeneration uses reach_config.py with the
committed q007_dsun_census.json profile, the pinned engine reach command,
then dsun_interrupt_census.py with the source, current inventory, report
and configuration. Preserve the independently read controls and exclusions;
do not describe a raw candidate as an established instruction.

Run resident_listing.py38FF:02B5..0339 and retain
shutdown-close-all-dispatch.txt in the same directory. Compose its
ordinary-handle call with the existing08FB and022C complete local
readings, checking dirty selection, seek/write/commit ordering and each
failure propagation. The report ends after the close call's argument pop;
later unlink behavior is reused from FND-COMBAT-096, not newly inferred.
No original-game or emulated call was run and all original-derived
reports remain local.
