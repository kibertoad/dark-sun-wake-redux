---
id: FND-ITEM-084
title: Image refresh recursively visits linked slots and publishes a cache handle before dimension reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:3388..31E0:3568
tool: tools/research/exec-census/resident_listing.py at 9a053164; Python 3.12.10, Capstone 5.0.7; composition with FND-IMAGE-006 and FND-ITEM-082/083
environment: null
---

## Observation

Q-PARTY-116 retains effects of the flag-one child called before animation
acquisition. FND-ITEM-082 binds that call to 31E0:3388; FND-IMAGE-006
identifies its bitmap-cache child. This reads the complete local refresh
body. Addresses, field offsets, masks and sentinel words are hexadecimal;
other numbers are decimal unless marked.

### Recursion and image selection

The first argument is an unsigned slot. Values greater than 320 return
AL zero. Equality is admitted locally, unlike FND-ITEM-082's parent bound
of strictly below 320. There is no own DS:265B/265C enable-byte gate.
The routine examines object word +21 at DS:67BB+37*slot with word
arithmetic. Unless it is FFFF, it recursively calls itself with that word
and the low bytes of the other two arguments. It ignores returned AL and
then handles the original slot. No cycle/depth check or link update occurs.
Terminating recursion, stack capacity, admitted indices and child preservation
are therefore requirements; a recursive zero result does not stop the parent.

It starts with object word +19 as its image number. For kind byte two at
4F49:0C33+3*slot, the following table maps word +6 of the 49-byte record
selected by the adjacent owner word through DS:19C9. A match replaces
both the local image and object word +19; a nonmatch retains the old word.

| Source word (hexadecimal) | Replacement (hexadecimal) |
| --- | --- |
| 01AE | 2AF9 |
| 16F7 | 2AFA |
| 019F | 2AFD |
| 021D | 2AFF |
| 0223 | 2AFE |

After this kind branch, unsigned slots below four with nonzero DS:068E
override the image and object word +19 with 2AF9. This later override
does not require kind two. All record/index products and pointer offsets
keep word width; these tests do not supply table or ownership admission.

### Handle publication and frame dimensions

It calls 31E0:3568 with the selected image and the third argument's low
byte, storing returned AX to object word +10 before checking for FFFF.
FFFF returns AL zero with that store and preceding image/recursive writes
retained. The function does not release the previous handle itself.
Other returned words select a 16-byte cache record at 52A2:0CA8+16*handle
using word arithmetic, without a local upper-handle check.

When the second argument's low byte is nonzero, it clears object bytes
+12 and +15 and copies DS:145E to object dword +1D. Otherwise those three
fields are retained. The frame byte +12 then selects a four-byte entry
starting at offset six of the buffer reached through cache far field +0A.
The entry's two words become BX/CX for 1000:060E, while the cache's far
field +0A becomes DX:AX. The helper's returned DX:AX is dereferenced and
its byte zero is stored in object byte +13.

It repeats the frame-entry/cache reads and helper call, then stores byte
+2 of the newly returned pointer to object byte +14. The two dimension
reads are separate requests, not one retained pointer snapshot. Their
identity and valid frame/buffer spans remain obligations; neither request
has its own frame-count, null-pointer or success test in this caller.
Finally it writes FF to cache-record byte +0F and returns AL one.

The function has no own store to animation word +16 or descriptor counts.
Its direct writes include linked objects' image/handle/dimension state via
recursion, so disjoint storage and child effects matter before composing
it with acquisition. SI/DI/BP are restored on return. AL is the explicit
result; full AX, ES and scratch preservation are not inferred.

## Interpretation

This removes image refresh's local recursion, override, handle-publication,
reset and dimension-read ordering from Q-PARTY-116's flag-one-child
obligation. Combined with FND-ITEM-082, the parent ignores refresh AL and
continues its separate acquisition gates. A refresh failure is consequently
not an automatic rollback or cancellation of acquisition. Native link
termination, owner/cache/frame identity, buffer spans, cache-provider and
pointer-helper effects and other writers remain open.

## Alternatives

Calling recursion guarded does not prove an acyclic chain. Treating AL zero
as no mutation ignores image and handle publication before the failure
return. Treating the dimensions as one stable buffer read ignores the
repeated lookup/helper sequence. The inclusive local slot bound is not
proof that slot 320 has valid native storage. No native defect, successful
bitmap load, complete reading or parity promotion is claimed.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and the approved resident_listing.py
with project-local evidence Python -I on the half-open location range.
Under GAME_DIR/analysis/item-removal-20261104/,
effect-image-refresh-contract.txt and its -repeat pair both have SHA-256
d7cbfe7de1a388fca01a5854b846424621fddf44683546936c5f049f95f6a2ac.
Keep reports local. No original game or isolated function was executed.
