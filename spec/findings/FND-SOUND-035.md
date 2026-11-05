---
id: FND-SOUND-035
title: Setup numeric extraction distinguishes no token, unassigned sign tokens and a successful zero-valued bare hexadecimal prefix
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0B26..1AF6:0BB3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0C8F..1AF6:0CDE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:072E..1000:08BE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0B43..1000:0B62
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0C22..1000:0DEF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3991..1000:39E7
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:038E..1AF6:0446
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:026F..1AF6:038E
tool: tools/research/exec-census/resident_listing.py at 22ffd7cc; Python 3.12.10, Capstone 5.0.7
environment: null
---

## Observation

This takes the numeric-assignment and repeated-count continuation parts
of Q-CONFIG-006 beyond FND-SOUND-024/025/028/034. The short token cases
below are synthetic reading cases, not modifications to the licensed
file or executed experiments. They assume a valid retained line and
classification/format tables, disjoint stack/output storage and no
offset wrap. Addresses, offsets and word values are hexadecimal.

### Numeric helper return versus assignment

1AF6:0B26 searches the retained line for a classified decimal digit or
minus. If none is found within its signed length comparison, it returns
zero before writing the supplied output word. Its token copy has no
independent twenty-byte destination bound: it checks the source index
against the line length, rather than the destination index against the
local array. No result after exceeding that local storage is claimed.

Once it forms a short terminated token, it calls 0C8F. That wrapper
reserves a local output word without initializing it, supplies its far
address to scanner wrapper 1000:39C4, discards the returned assignment
count and reads the local word regardless. 0B26 then copies that word
through its caller's output pointer and returns one. Thus a scanner
return and 0B26's success result are different channels.

For an isolated minus followed by the token's NUL, worker 1000:0C4D
records a negative sign, then gets FFFF from the character callback.
It sets its numeric status to FFFF and returns through the no-number
path. The scanner does not resolve or write the variadic output pointer
when that status is nonpositive. Its input-failure tail returns FFFF
when its assignment count is still zero. The 0C8F local consequently
has no assignment from this scan; 0B26 nevertheless stores its retained
stack word and returns one. Neither a zero word nor an unchanged caller
field follows from this path.

A short repeated-sign token such as two minuses followed by one digit
also makes no assignment: the second minus fails the first-digit test
under the explicit decimal base, with numeric status zero. The scanner
returns zero assignments, which the same wrapper ignores. A token
beginning minus-zero-x takes the decimal format because 0C8F recognizes
hexadecimal format only when the token itself starts with zero/x. The
decimal worker accepts zero, then stops at x, assigning zero rather
than a negative hexadecimal magnitude. These cases remain distinct
from searching a line that contains no digit or minus at all.

For a token consisting only of zero/x or zero/uppercase-X, the wrapper
selects the hexadecimal format. The worker initializes numeric status
to one and magnitude to zero, accepts the initial zero, consumes the
base prefix and reaches the digit loop. NUL fails the digit test and
ends this loop without changing the initialized success status. The
scanner therefore assigns word zero and increments its assignment
count. There is no requirement for another digit after that prefix on
this path. This also covers a source suffix beginning with a hex letter
when the preceding extractor stopped immediately before that letter.

### Record and group continuation

The reached CardId arm at 1AF6:0411 passes the record's far address plus
0028 directly to 0B26 and tests only that helper's AX. A zero result
returns record failure. A one result continues the record loop, even
when the scanner did not assign the wrapper's local. CardGroup at 042B
does the same with record offset 002A. FND-SOUND-028 retains other
record-arm gates; the record reader does not roll back earlier stores.

The group NumberOfRecords arm passes the address of its own local count
to 0B26 and likewise tests only its return. On a one result it publishes
that word to the selected global count before allocating or clearing
storage. A later allocation failure does not undo this global store.
Another admitted NumberOfRecords tag can repeat this branch: there is
no seen-count guard. Its new allocation result replaces the local far
pointer before the null check, and this branch calls no release routine
for the previous pointer. The record counter DI is not reset by a new
count tag. Actual heap effects and the reachability of repeated count
tags under the outer signed tag bound remain conditional.

On a record helper failure the group returns a zero far pointer rather
than its retained allocation. Its local failure exits do not free that
allocation or revert a published count or earlier record stores. This
describes the local loss of the return pointer, not an observed native
heap state. FND-SOUND-024's separate tag/record counters and early
EndOfGroup return remain unchanged.

## Interpretation

The representative missing-token, sign/non-assignment, bare-prefix-zero
and repeated-count local failure/continuation cases are removed from
Q-CONFIG-006's remaining work. A single invalid-number rejection rule
would contradict the different helper, scanner and caller tests.
Returning one does not prove a numeric assignment was made; malformed
non-assignment does not leave the caller's word untouched.

## Alternatives

The unassigned wrapper word depends on retained stack contents, not a
newly defined numeric default. This finding assigns it no fabricated
value. Longer/other tokens, overflow, complete nested/source context,
state/alias lifetime and remaining parser failure paths stay
Q-CONFIG-006. Native heap, stream, error mapping and DOS outcomes stay
Q-CONFIG-011. No malformed original file was created or run, and no
complete-reading or parity promotion is claimed.

## How to reproduce

Use approved installed SOUND_DS.EXE, length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Invoke resident_listing.py with -I
and the eight location ranges above in order. Retain
setup-numeric-failure-continuation.txt and its -repeat pair under
GAME_DIR/analysis/disc-adv-survey-20261104/. Both have SHA-256
ac615f82b65a7badcbb775b5fdc8d374213150faa7a3222e23506882815078ef.
Reuse FND-SOUND-025's format/classification/dispatch controls and
FND-SOUND-028's record-arm controls. Track each callback/status exit,
whether the output pointer is reached, the wrapper's uninitialized
local, and caller success tests separately. Retained reports contain
original-derived listings and are never committed.
