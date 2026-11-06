---
id: FND-SOUND-051
title: The startup preference helper clamps a byte and publishes a record-relative sound adjustment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45E1:0097..45E1:021F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067206..0x000672DF
tool: resident_listing.py and overlay_listing.py at c7070b73; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-011/049/050
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses and offsets below
are hex. FND-SOUND-011's startup caller loads DS:26B5 into AL,
pushes AX, then zero, calls 45E1:0097 and removes four bytes.
It does not clear AH there. The helper reads only the low byte of
its second argument, so the caller's unspecified high byte does not
enter the recorded parameter arithmetic.

The helper has a four-byte local pointer and returns with LEAVE,
RETF at 021D/021E. It skips the body for configuration word +8
at pointer DS:3411 equal to 71, or mode word 4E71:0B51 equal
to five or two. Otherwise it clamps the unsigned second-argument
byte to decimal 127, updating that byte in its incoming stack slot.
It selects the far record from 50C3:(0D2F + 6*first argument),
using word-width multiplication and addressing. It has no local
index bound, type test or null-record check.

It publishes the first argument's low byte to 4E71:0B6A. It queries
4842:0C82 with DS:3436 and, for result two, requests 0C8E.
If the current identity at 0B6A differs from 0B6B, it requests
47E5:02B6 with the full first argument. Result FFFF requests
49DE:000F with 0B, 006A and four, then takes the common return.
This is the nonfatal selector distinguished in FND-PARTY-321;
the earlier identity publication has no local rollback.

After another identity comparison, a changed identity and record
word +2 equal to FFFF request 4842:0C7C with DS:3436, the far
pointer at DS:3420 and that record word. The configuration byte
+3A then selects the following continuation:

- One requests 4842:0D06 with DS:343E and DS:3464. Result one
  with 4E71:0C1F zero skips the 0CA0 request, but still starts
  the stream through the following 0C76/0C88 requests.
- Zero requests 4842:0CA0 with DS:3436 and the zero-extended,
  clamped second-argument byte. The one-valued branch also makes
  this request when its preceding conditions do not skip it.
- Other unchanged values skip the stream requests and identity
  publication, proceeding directly to the adjustment store.

The non-one route reloads the configuration pointer and byte;
actual preservation is needed to apply a stable-value description.
The stream continuation passes DS:3436, DS:3420 and record word
+2 to 4842:0C76, then requests 0C88 with DS:3436 and copies
current 0B6A to 0B6B. No success return is tested there.

Except for the early gate/error returns, all those continuations
compute a word subtraction: zero-extended clamped argument byte
minus zero-extended record byte +0C, and store the word at
4E71:0B68. Under unchanged inputs its signed range is -255
through 127. FND-SOUND-050's digital default-parameter path adds
that word to its record byte +0C before its signed 15..127 clamp.
For the same unchanged record, that arithmetic cancels the record
baseline and leaves the clamped preference byte before that final
clamp. For another record it retains the difference of baselines.
This algebra does not establish actual record identity or retention.

## Interpretation

Q-SOUND-002 can replace the unread preference-helper obligation
with this byte clamp, stream-request ordering and record-relative
adjustment contract. It is not simply a hardware master-volume
write: it also selects a record, can request stream activity and
publishes shared state. Driver parameter units, actual startup
record/configuration admission, child effects, frame and storage
retention and audible output remain unresolved. There is no common
success result, complete-reading declaration or native sound evidence.

## Alternatives

- The complete incoming preference word controls the adjustment:
  the helper explicitly zero-extends its low byte.
- Every admitted call sends the parameter to the driver: one gate
  skips 0CA0, and other configuration values skip stream requests.
- The default parameter always equals the preference: cancellation
  requires the same record baseline and preserved state; a subsequent
  clamp and driver semantics still apply.

## How to reproduce

Use the approved static readers at the tool revision above with
resident range 45E1:0097..45E1:021F and overlay descriptor 180,
file range 67206..672DF. Retain sound-preference-adjustment-contract.txt
and sound-preference-startup-caller.txt in
GAME_DIR/analysis/handler-census-20261106/. Track instruction boundaries,
low-byte argument use, outgoing argument order and both identity reads.
Reuse FND-SOUND-050 for the consuming default path. No original game,
emulated call or emulator session supplied this evidence.
