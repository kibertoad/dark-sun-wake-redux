---
id: FND-SOUND-067
title: Disc-song interface publishes track after unchecked request and separates packet status from audible success
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:0665..2834:0673
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A32:0011..4A32:00E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:0250..2660:0404
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:06F8..2660:0871
tool: resident_listing.py at 3017f348; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-071/013/061 and FMT-SOUND-002
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-013 records the dispatch gates and song-plus-one path.
This finding completes the local disc-start/request interface, not
its external device behavior or the separate FM children.

2834:0665 forwards its word argument unchanged to 4A32:0011
and returns. Reuse FND-SOUND-013 for configuration and mode gates.
On the disc branch, a nonnull current DS:348E is called with the
incoming word plus one at word width. The branch ignores the result;
a null pointer skips it. Neither path assigns a common success value.
The known pointer producer selects 2660:0250; changed targets remain
unresolved. The unchanged shipped song range 1..35 therefore requests
track arguments 2..36, not a proof of audible tracks.

### Complete disc-start local body

0250 holds its track in SI and initializes fourteen local bytes to
zero. Disabled byte 4E71:0C3F returns AL zero. Otherwise it compares
zero-extended last-track byte 0C43 with SI signed, rejecting when
last track is less. There is no local lower-bound test. Shipped
positive arguments do not settle changed/negative argument admission.

It calls 06F8 with the track and three zero words, retaining DX:AX.
It computes word-width seven times track+1 and track, loads the far
table pointer DS:3482 and reads each entry's dword +2. Both values
pass through 080B. It subtracts current from next with dword wrapping,
then requests 032C with two dwords: the retained first result minus
DS:3496, and the difference plus DS:3496 minus DS:349A. There is
no local range/underflow/capacity test on those derived arguments.

After 032C returns, it stores the held track at 4E71:0C3B and
sets AL one without testing the result. Only AL is normalized; no
common full-AX Boolean is assigned on those returns. Actual SI/frame
preservation, track table, offset globals and child outcomes remain
required. This publication does not confirm the request succeeded.

032C returns AL zero when 0C3F is disabled. Otherwise it forwards
its two dwords with command 0084 to 035E. Exactly AX FFFF returns
AL zero; every other AX sets AL one. This tests a sentinel, not all
possible device-status bits. Its result is ignored by 0250.

### Request construction and returned status

035E returns AX FFFF when 0C3F is disabled. Otherwise it writes
these fields relative to request base 4E71:0C53:

| Offset | Local write |
|---|---|
| +0 | Header-length byte 0D |
| +1 | Zero unit byte |
| +2 | Low byte of incoming command word |
| +3 | Zero status word |
| +0D | Zero address-mode byte |
| +0E | First incoming dword |
| +12 | Second incoming dword |
| +16 | Zero dword |

The header's other bytes are not cleared by this body. The helper
constructs an SS-local register block with AX 1510, BX 0C53 and
CX from 4E71:0C35 and passes its near offset twice to 1000:1E8C
with interrupt number 2F. Reuse FND-SOUND-013 for that service
wrapper's purpose; actual block backing, segment convention and
handler writes are not established by these local assignments.

After ordinary return, nonzero local word BP-4 yields AX FFFF.
Otherwise it returns the current status word at 4E71:0C56. The
external handler and prior request bytes remain evidence obligations.
No successful service or hardware result is inferred.

### Position acquisition and local table writes

06F8 reuses FND-SOUND-013's locally ineffective logical-not-and-two
gate. It can request 066C when 0C3D is zero and returns dword zero
if that byte stays zero. A null DS:3482 requests allocation of seven
times last-track+2 bytes; a null result returns zero. Otherwise it
constructs an SS-local seven-byte request with byte zero 0B and
byte one the track low byte, calls 035E with command three and
ignores its result. It copies local bytes 0/1, dword +2 and byte +6
into the selected seven-byte table entry. No local initialization
of the remaining response bytes precedes the request.

It adds ((signed argument two * 60 + signed argument three) * 75
+ signed argument four) to the response dword at dword width, then
passes that value to 080B. In the known 0250 call those arguments
are zero. 080B takes the low three bytes of its input and computes
((byte two * 60 + byte one) * 75 + byte zero), returning DX:AX.
These are local encoding calculations; valid response domains and
physical timing units remain external evidence, not assumptions.

Known local writes concern DS:3482/3484, its pointed track table,
the request block, selected-track word and stack. They do not name
DJ.DAT's globals at DS:424E..4255. Actual heap/table/frame aliases
and external writes can still affect the music table; the local
interface alone does not settle its lifetime.

## Interpretation

Q-SOUND-004 can remove the unread local 0250/035E interface and
retain actual track/response/status domains, table/heap/frame binding,
external handler effects and native output. Q-SOUND-011 can reuse
the known child write targets rather than absorbing all device behavior
into layout. Unknown aliases and the FM branch's children remain open.
No complete reading, whole question closure or playback validation follows.

## Alternatives

- A published track proves successful playback: 0250 ignores 032C's
  result before publication.
- AL one is a normalized AX success result: only AL is assigned.
- Request return proves initialized response bytes: the caller ignores
  status and does not initialize all response bytes before the request.

## How to reproduce

Read all four location ranges with resident_listing.py at the revision
above. Retain music-song-disc-interface-contract.txt under
GAME_DIR/analysis/handler-census-20261106/. Track word/dword widths,
AL versus AX, outgoing push order and ignored results; keep DS track
and DJ table pointers distinct. Reuse the existing configuration/pointer
and service findings without inferring external success. No original game,
emulator session or emulated call supplied evidence.
