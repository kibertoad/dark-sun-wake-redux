---
id: FND-EXE-500
title: Game diagnostic chain counts source bytes and selects buffered or DOS-write character paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:37D8..1000:37F2
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:37D8..1000:37F2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:345B..1000:349D
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:345B..1000:349D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:35F8..1000:3603
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:35F8..1000:3603
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3313..1000:3449
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:3313..1000:3449
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3EDA..1000:3F14
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:3EDA..1000:3F14
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0005094A..0x0005094B
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0005082E..0x0005082F
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-499's 37D8 helper sets ES to current DS, loads its incoming
offset into DI and scans for zero with CX FFFF, AL zero and forward
direction. It returns the complemented remaining count minus one in AX,
restores DI and BP and returns far without incoming cleanup. It performs
no allocation check. An admitted stable terminator among the 65535 scanned
bytes produces its preceding count, zero through FFFE; exhaustion also
returns FFFE. Offsets wrap without segment adjustment. SI and DS are
unchanged locally, but ES is set to DS.

The selected flag-eight branch of near 345B holds the incoming record
offset in DI and original byte count at SS:BP-2. It tests record word
two for 0008. On that selected branch it repeatedly tests the old count
for zero while decrementing the incoming count word. Nonzero advances
the incoming source offset by one, reads the old source byte through
current DS, sign-extends it to AX and calls local far-returning 3313
with byte word and record offset. It removes four argument bytes.
Full AX FFFF returns zero immediately; every other word continues.
Count exhaustion returns the held original count. The shared suffix restores
DI, SI, SP and BP and near-returns with six-byte argument cleanup.
Flag-eight-clear branches are outside this selected 345B reading.

For both editions, 3313 saves SI and DI, holds the incoming record
offset in DI and stores the incoming low byte in a shared current-DS
byte: A4E2 installed, A42C on disc. Its record word zero is tested
signed against FFFF. A value less than minus one selects the buffered
path: increment that word, retain and increment record offset ten, then
write the byte through the old buffer offset. If record flags word two
has bit 0008 and the current shared byte is 0A or 0D, it calls
2F13 with the record offset. Full AX zero selects the ordinary return;
nonzero returns FFFF. Other buffered cases return ordinarily.

The slow path rejects flags with any 0090 bit set or bit 0002 clear,
setting flag 0010 before returning FFFF. Otherwise it sets flag 0100.
With record word six nonzero, it calls 2F13 when record word zero
is nonzero and rejects a nonzero result. It negates current word six
into record word zero, retains and increments record offset ten and
writes the current shared byte through the retained buffer offset. It
performs the same flag-eight/line-byte flush test. These state updates
precede any later flush failure and are not rolled back locally.

With record word six zero, it sign-extends record byte four, doubles
the word and tests indexed flags bit 0800 at DS:37C4 installed or
DS:3738 on disc. A set bit calls 07B0 with that sign-extended byte,
zero quantity pair and mode two, removes eight bytes and ignores the
returned pair. If the current shared byte is 0A and record flag 0040
is clear, it requests one byte through 3EDA from DS:394A installed
or DS:38BE on disc. Under admitted DS 57E0/57D7 those shipped
bytes at 5094A/5082E are 0D. A result other than one takes failure
handling; otherwise it requests one byte from the shared-byte address
through 3EDA. That result also must equal one.

Failure handling tests current record flag 0200. A set bit selects the
ordinary return despite the short or failed request. Otherwise it sets
flag 0010 and returns FFFF. The ordinary return reloads the current
shared byte into AL and clears AH; it is not necessarily a snapshot of
the original incoming byte. Every return restores DI, SI and BP and
returns far without incoming cleanup. Native preservation, writable aliases
and re-entry into the shared byte remain unadmitted.

The 3EDA helper doubles its incoming handle word at word width and
tests indexed flags bit one at DS:37C4 installed or DS:3738 on disc.
A set bit supplies error word five to near 06BA and makes no interrupt.
Otherwise it supplies AH 40, BX incoming handle, CX incoming quantity
and DX incoming source offset to interrupt 21. Carry set passes returned
AX to 06BA. Carry clear holds AX while setting indexed flag 1000,
then returns that held AX. It restores BP and returns far without incoming
cleanup. Neither table-index validity nor native write behavior is admitted.
There is no local program-execution request in this selected helper path.

## Interpretation

This follows the diagnostic into a counted byte branch and character routine
with explicit buffering, line-byte and native-write paths. Local failure flags,
return words and request quantities do not prove delivered output. Q-EXE-007
retains actual DS, record/table initialization and extents, aliases and lifetime,
345B's other branches, 2F13/07B0/06BA, native results and preservation,
the surrounding game caller and other launch-capability coverage. No complete
output reading, successful display or whole-game launch exclusion is claimed.

## Alternatives

Treating scan exhaustion as a valid length ignores its shared FFFE result.
Treating every non-FFFF character result as an independently verified write
ignores buffering and flag 0200's failure bypass. Treating shared-byte reads
as one retained input ignores reloads after calls. Treating updated count
and pointer fields as transactional ignores stores before flush failure.

## How to reproduce

At revision 1d0d4eb require both DSUN.EXE identities from FND-EXE-350.
Use Capstone 5.0.7 in sixteen-bit mode, MZ header size 5200 and
modeled load segment 1000. Decode shipped 89D8..89F2, 865B..869D,
87F8..8803, 8513..8649 and 90DA..9114 at the corresponding
1000 addresses in both editions. Verify opcode 98 at 347E, 33D0,
33EA, 340D and 3427 as byte-to-word sign extension. Keep the
edition-specific shared-byte, indexed-flag and extra-byte addresses distinct;
inspect shipped 5094A and 5082E for the conditional extra-byte value.
Track signed record count, pre-call stores, full-word failure tests, flag
0200 bypass, incoming cleanup and AH 40 request inputs. Licensed bytes
stay outside Git; no original process, DOSBox or emulated call runs.
