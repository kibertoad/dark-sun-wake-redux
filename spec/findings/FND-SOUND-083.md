---
id: FND-SOUND-083
title: Installed song sequence requests have no matching shipped archive table
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x000028C3..0x00002DD7
  - build: BLD-GOG-EN-1.1
    file: DARKRUN.GFF
    offset: 0x000003BF..0x000003DF
  - build: BLD-GOG-EN-1.1
    file: DARKSAVE.GFF
    offset: 0x000003BF..0x000003DF
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x002141E5..0x00214371
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00675506..0x0067748A
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00573CE9..0x00574455
  - build: BLD-GOG-EN-1.1
    file: RGN001.GFF
    offset: 0x00018F09..0x00019941
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0x0000F7A1..0x0000FC81
  - build: BLD-GOG-EN-1.1
    file: RGN033.GFF
    offset: 0x000152D1..0x00015BB9
  - build: BLD-GOG-EN-1.1
    file: RGN034.GFF
    offset: 0x00010F3B..0x000115CB
  - build: BLD-GOG-EN-1.1
    file: RGN036.GFF
    offset: 0x0000AA97..0x0000AD73
  - build: BLD-GOG-EN-1.1
    file: RGN037.GFF
    offset: 0x00015BB8..0x00016488
  - build: BLD-GOG-EN-1.1
    file: RGN038.GFF
    offset: 0x0000B27D..0x0000B589
  - build: BLD-GOG-EN-1.1
    file: RGN039.GFF
    offset: 0x0000CA11..0x0000CDDD
  - build: BLD-GOG-EN-1.1
    file: RGN03A.GFF
    offset: 0x000170D6..0x00017ABA
  - build: BLD-GOG-EN-1.1
    file: RGN03B.GFF
    offset: 0x000148E7..0x00015133
  - build: BLD-GOG-EN-1.1
    file: RGN03C.GFF
    offset: 0x000116A4..0x00011F08
  - build: BLD-GOG-EN-1.1
    file: RGN03D.GFF
    offset: 0x00016E02..0x0001777A
  - build: BLD-GOG-EN-1.1
    file: RGN03E.GFF
    offset: 0x000140D8..0x000148C4
  - build: BLD-GOG-EN-1.1
    file: RGN03F.GFF
    offset: 0x00017FE8..0x00018AA4
  - build: BLD-GOG-EN-1.1
    file: RGN041.GFF
    offset: 0x0001C68F..0x0001D29B
  - build: BLD-GOG-EN-1.1
    file: RGN042.GFF
    offset: 0x00017854..0x00018244
  - build: BLD-GOG-EN-1.1
    file: RGN043.GFF
    offset: 0x00011776..0x00011DEE
  - build: BLD-GOG-EN-1.1
    file: RGN044.GFF
    offset: 0x000188D1..0x000192D9
  - build: BLD-GOG-EN-1.1
    file: RGN045.GFF
    offset: 0x000166FF..0x00017023
  - build: BLD-GOG-EN-1.1
    file: RGN0FF.GFF
    offset: 0x00008427..0x000084C3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47E5:01C4..47E5:02B6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47E5:0552..47E5:05D1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:05B5..38FF:0615
tool: gff_tag_numbers.py and resident_listing.py at 546c0483; build-listing.mjs readList and published sourceXxh3 identity controls; reused FND-GFF-003/005, FND-SOUND-013/015/069/082, FND-CONFIG-151/234 and FND-PARTY-268/321
environment: null
---

## Observation

The source domain is exactly the twenty-six top-level installed GFF files in
BLD-GOG-EN-1.1.files.yaml. Their names match the actual top-level GFF set;
every length and XXH3-128 matches the manifest. No media, generated archive,
external file or in-memory archive record is inferred from that check.
FND-GFF-005 already accounts for their directory structures and resources.
This census reads tag tables, not arbitrary payload byte strings.

| Requested tag | Installed archives with a matching table | Resources |
|---|---|---|
| PSEQ | 0 of 26 | 0 |
| FSEQ | 0 of 26 | 0 |
| LSEQ | 0 of 26 | 0 |
| GSEQ | 0 of 26 | 0 |

The same reader's independent controls reproduce RESOURCE.GFF CSEQ/1000
and CHARSAVE.GFF PREF/100, one resource each. FND-GFF-003 identifies the
former tag's indexed table and the latter archive's plain tables, so the
two directory mechanisms are exercised. Its GFFI control also returns
five, seven and twenty resources in GPLDATA.GFF, OBJEX.GFF and RESOURCE.GFF,
with none in the other twenty-three archives. These positive resources
are known independently from FND-SOUND-015, FND-CONFIG-001 and
FND-GFF-003, not selected merely because this search finds them.

CSEQ is a different four-byte type from all four requested tags. Its one
78-byte resource cannot supply a request for FSEQ, even when the requested
number is 1000. This does not prove that no other code can consume CSEQ.

### Connected missing-type path

Reuse FND-PARTY-268 and FND-SOUND-069 for the four literal type arguments
and their unchanged forwarding through 47E5:0552. The current instruction
reading verifies the request selectors and result branches together:

| Stable configuration word +32 | Literal request | Query call | Zero-result branch |
|---|---|---|---|
| 0 | PSEQ | 47E5:0202 | 020A..021C |
| 1 or 2 | FSEQ | 47E5:023F | 0247..0249 |
| 3 | LSEQ | 47E5:0264 | 026C..026E |
| 4 | GSEQ | 47E5:0289 | 0291..0293 |

The tests reload the configuration pointer; they are not an atomic captured
switch. The table describes unchanged admitted bindings, not every possible
mutation across a child or an independent entry after those tests.

0552 clears its local length dword, passes its SS-qualified address with the
32-bit type/number to 38FF:05B5 and returns AX zero for a nonzero query result
(0584..058A). This occurs before its capacity test and before the resource-read
request at 05B7. That early path makes no new resource transfer into the
DS:3458 destination and does not clear an older destination pointer.
Reuse FND-CONFIG-151's size-query contract: a null output or archive, failed
signature, or exhausted typed-record lookup returns FFFF. An exhausted search
that encountered no matching type sets error four. FND-CONFIG-234 supplies
the exact type lookup and next-archive interface; the no-type case does not
need to resolve a numbered entry or read a TIMB payload.

Under faithful, unchanged archive records sourced only from the validated
installed set, valid type/frame arguments and returning lookup continuation,
none of the four requests can obtain a matching type from those files,
regardless of the song number. The result therefore takes the zero-result
branch. It requests error word 0x17 with selector 0x006A and third word 0x000B at
47E5:020C..0219, then jumps to the common epilogue 02B3. It does not fall
through to sequence setup 0296..02AD on that request path. The setup call,
new DS:3440 handle store and byte 4E71:0B50 store in that block are bypassed.
Unknown callback transfers or changed code/frame are not ordinary continuation.
This composition does not itself prove archive traversal or a child terminates.

This does not establish that all sound is absent. FND-SOUND-013 already
separates the disc branch, selected by configuration word +14 bit 1, from
this FM acquisition path. The shipped SOUND.CFG has that bit set; actual
runtime admission/preservation remains a separate question. Nor does an
acquisition error necessarily stop the outer song routine: FND-SOUND-069
retains the separate 0B56 clear/test and callback-state conditions.

## Interpretation

There is no shipped installed TIMB list to census through these four typed
song-request arms. A new TIMB parser would not settle their admission.
First distinguish missing-type lookup from successful sequence publication;
retain the driver's FND-SOUND-082 cache-progress conditions only for data and
entry paths that actually reach setup. This is a source/route restriction,
not a complete reading or a universal claim that the driver is never called.

## Alternatives

- Disc GFF files were not searched by this census. Other archive files,
  runtime-generated or modified directory records, alias writes and replaced
  tables remain outside its negative scope. No absence across all media or
  every live lookup is claimed.
- Configuration values outside 0..4 can bypass all four local request arms;
  changed selectors, independently entered setup code and the other direct
  callers of the shared setup wrapper remain possible and unqualified here.
- An absent file tag does not prove the RAM archive structures are faithful,
  valid or unchanged at a call. Actual registration, signature, traversal,
  frame and callback admission remain Q-CONFIG-014 and Q-SOUND-004.
- CSEQ/1000 is a control for a different type, not a replacement song payload.
  Its XMIDI semantics and any general consumer question are not settled here.

## How to reproduce

With the pinned evidence Python, run gff_tag_numbers.py GAME_DIR TAG for
PSEQ, FSEQ, LSEQ, GSEQ, CSEQ, PREF and GFFI. Require the four absent-table
counts and the independent controls above. The archive domain must first
match the manifest's twenty-six top-level .GFF paths exactly. Use the existing
build-listing.mjs readList with key files and the published sourceXxh3 reader
to compare each source's length/fingerprint with that manifest; reject missing,
extra or changed sources rather than generalizing the negative result.

Each directory location above is the file's little-endian directory offset
and size from header +0C/+10. Existing validated GFF layouts supply the
plain/indexed table strides; this is not a full read of every resource payload.
Read 47E5:01C4..02B6, 47E5:0552..05D1 and 38FF:05B5..0615 with
resident_listing.py on the approved installed DSUN.EXE, whose size/hash guard
is unchanged. Reuse the earlier lookup/traversal and error-selector findings.

Retained output is under GAME_DIR/analysis/sequence-input-census-20261107/:
installed-identities.json, directory-locations.json, the seven TAG-tables.txt
reports, song-selection-query.txt and size-query.txt. No game or original
function was executed, no runtime state supplied and no asset copied into Git.
