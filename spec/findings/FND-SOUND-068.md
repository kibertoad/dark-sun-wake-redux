---
id: FND-SOUND-068
title: Music busy status can accept a request error sentinel while stop uses distinct disc and sequence cleanup paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A32:0185..4A32:0310
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4ABF:01E5..4ABF:0265
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:0404..2660:0482
tool: resident_listing.py at ceff7079; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-008/012/013/067
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-071 infers music status/stop roles from their callers.
This reading supplies their local branch and result contracts for
Q-SOUND-004, without treating a software status as audible evidence.

### Busy query

4ABF:01E5 returns AX zero for configuration word +8 equal to
71 through current DS:3411. Otherwise it reloads that pointer
and tests word +14 bit one. With that bit set it calls 2660:0404
and returns normalized AX one for any nonzero result, zero otherwise.
There is no configuration-pointer null guard in this body.

0404 returns AX zero when 4E71:0C3F is zero. Otherwise it writes
0C at 4E71:0C48 and requests 035E with command three, far pointer
4E71:0C48 and length eleven decimal. It tests returned AX bit
0200 and returns normalized AX one if set, zero if clear. It does
not first compare the returned word with FFFF. FND-SOUND-067
records 035E's error sentinel FFFF, whose bit 0200 is set. Thus,
under that error-return continuation, this busy query returns one;
it is not proof that a device is playing or that the request succeeded.
Actual service outcome and frame/backing admission remain open.

With configuration bit one clear, 01E5 requests 4842:0D06 with
current DS:343E and DS:3464. First result one returns AX one.
Otherwise it queries again: second result two returns zero. Other
second results cause a third query, but every continuation after
that query returns zero. The third query is not removed from the
contract merely because its result cannot produce one locally.
All arguments are reloaded; actual handle/state stability and child
side effects remain conditions. No local data-state write occurs.

### Stop gates and disc branch

4A32:0185 saves BP/SI. It returns when configuration word +8
is 71 and word +14 bit one is clear, or mode word 4E71:0B51
is three or five. It reloads configuration. If bit one is set and
current far pointer DS:3492 is nonnull, it calls that pointer and
returns immediately, ignoring its result. FND-SOUND-013's known
pointer producer selects 2660:0447; changed targets remain open.
If the pointer is null, stop falls through to sequence cleanup even
with the disc bit set, unlike the song-start branch's null skip.

0447 returns AL zero for disabled 0C3F. Otherwise it requests
035E with command 0085 and two zero dwords. Exactly AX FFFF
returns AL zero; every other status stores FFFF at 4E71:0C3B
and returns AL one. Only AL is normalized. It does not establish
that every other returned status means successful stop, and its
caller ignores the result. Current-track publication is conditional
on the sentinel test, not verified device silence.

### Complete local sequence-cleanup branches

If DS:3464 is not FFFF, stop queries 0D06 with DS:343E and
that word, retaining AX in SI. Results zero or two clear DS:3464
to FFFF. If DS:3466 is then also FFFF it returns. Otherwise,
when DS:3464 remains non-FFFF, it requests 4842:0CFA with
DS:343E and DS:3440, sets DS:3464 FFFF, sets DS:3468 FF
and clears 4E71:0C28. The query and request words differ; no
handle correspondence is inferred from their positions.

It next returns if DS:3466 is FFFF. Otherwise it queries 0D06
with DS:343E and DS:3466. Results zero or two clear 3466,
request 4842:0CC4 with DS:343E and DS:3442, then take the
conditional release path below. Other results, if the reloaded
3466 is still non-FFFF, request 4842:0D18 with DS:343E,
DS:3442, zero-extended 4E71:0C14 and zero, then 0CFA with
DS:343E/DS:3442 and clear 3466. They also request 0CC4
before conditional release, including when the reloaded 3466
became FFFF. These requests have no local success-result test.

Both release continuations test current DS:344E for a nonnull
far pointer and DS:3452 nonzero. When both hold they pass that
pointer to 4AB9:0043, publish returned DX:AX back at 344E and
clear 3452, without a local return-value test. Child release semantics,
actual pointer/flag histories and callbacks remain unresolved.
All branches join the SI/BP restore and ordinary RETF at 030F.
There is no common AX success result for the outer stop body.

Known local state writes name the sequence fields above and 4E71
status bytes/words, not DJ.DAT's count/retry/pointer globals. Actual
child, heap, frame and segment aliases remain Q-SOUND-011 where
they could change that table's interpretation or binding.

## Interpretation

Q-SOUND-004 can remove the unread busy/stop local contracts and
retain exact status-sentinel, selected target, sequence-state, release,
frame and external-output obligations. A true busy result can follow
an error sentinel; a stop return or published marker does not prove
silence. The known caller meanings remain conditional interface roles,
not hardware validation. No complete reading, whole queue closure or
observed native audio failure is claimed.

## Alternatives

- Busy one proves successful status retrieval: disc query can map
  FFFF to one through its bit test.
- Disc stop skips everything for a null pointer: the outer stop instead
  continues into sequence cleanup.
- Every stop return uses one Boolean convention: the inner disc helper
  assigns AL, while the outer body has no common AX assignment.

## How to reproduce

Read the three location ranges with resident_listing.py at the tool
revision above and retain music-stop-status-contract.txt under
GAME_DIR/analysis/handler-census-20261106/. Reuse FND-SOUND-067
for the request error sentinel. Track repeated configuration/state
loads, AX versus AL, all secondary-state cleanup branches and distinct
query/request arguments. No original game, emulator session or
emulated call supplied evidence.
