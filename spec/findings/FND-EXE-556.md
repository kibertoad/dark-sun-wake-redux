---
id: FND-EXE-556
title: Game manager cleanup follows mutable cache callbacks and leaves native results unchecked
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0193..4AE5:01B5
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:0192..4AD6:01B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:09B4..4AE5:09CB
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:09B3..4AD6:09CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0BB5..4AE5:0BCF
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:0BB4..4AD6:0BCE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0EA2..4AE5:0ECD
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:0EA1..4AD6:0ECC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:1155..4AE5:11A9
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:1154..4AD6:11A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:1257..4AE5:1258
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:1256..4AD6:1257
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00040055..0x00040057
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0003FF64..0x0003FF66
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004AF62..0x0004AF66
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0004AE72..0x0004AE76
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-555's shipped cleanup target 4AE5:0193, or 4AD6:0192
on disc, saves BP and DS and loads DS from CS word 0005/0004.
That shipped word is relative segment 45CE/45BF. MZ relocation index
4418 at 3AE5:0005, or 4405 at 3AD6:0004, gives modeled
DS 55CE/55BF with load segment 1000.

It tests DS word 0128. Nonzero manufactures a far return and calls
0140/013F; zero skips it. FND-EXE-562 reads that vector-swap and
handle helper, and FND-EXE-565 relates the disc manager. The caller
does not test its returned carry or AX. It next pushes CS and calls
near through current DS word 0084, then pushes CS and calls near
through current DS word 0082. These supply far return frames in the
current code segment. Neither callback result is tested; the second target is
loaded after the first callback. The caller restores DS and BP and returns
far without incoming cleanup. Reaching that return depends on all callees'
return and preservation contracts.

Both shipped callback words hold 1257 installed or 1256 on disc;
that target contains a single far return. They are initial no-ops, not
permanent targets. FND-EXE-563's cache setup writes word 0084 to
0EA2 at 09B9, and word 0082 to 1155 at 0BBA.
The disc writes are 09B8 and 0BB9 with target offsets 0EA1 and
1154. Each store follows the corresponding setup's cache flag publication;
the following word-0080 store and zero AX return remain as recorded there.
These are positive writers, not a census of all writers or aliases.

The EMS callback 0EA2/0EA1 saves BP and DS and reloads the
same manager data segment. Bit 0004 clear in byte 0038 returns
without later stores. With that bit set, word 0032 equal to FFFF
also returns without clearing byte 0038. Otherwise it loads that word
into DX, sets AH 45 and requests interrupt 67. On continuation it
stores FFFF into current DS word 0032 and zero into current DS
byte 0038, without testing returned AH, carry or any other result. It
then restores DS/BP and returns far without incoming cleanup. The stores
are unconditional on the native result, under admitted native continuation and
segment preservation; they do not prove that the native release succeeded.

The extended-memory callback 1155/1154 likewise saves BP/DS and loads
the manager data segment. Byte 0042 zero returns immediately. Otherwise
word 0047 nonzero selects two calls through the far pointer in current
DS words 0043/0045. The first supplies AH 0D and DX from
0047. On return the second supplies AH 0A and reloads DX from
current DS word 0047. No first result is tested, and no local
DS reload separates the calls. After the second call it returns without
locally clearing byte 0042 or word 0047. Actual driver target,
result meaning and preservation remain unadmitted.

With word 0047 zero, the extended callback instead sets ES zero and
reads ES word 0066. It compares that word with modeled segment
55E3 installed or 55D4 on disc. The comparison operand has MZ
relocation index 4449 at 3AE5:118B installed, or 4436 at
3AD6:118A on disc; the shipped words are 45E3/45D4.
Mismatch returns without clearing byte 0042. Equality saves the manager
DS, sets DS to the compared segment and copies DS words 002F
and 0031 into ES words 0064 and 0066 in that order. It
restores manager DS and clears byte 0042, then restores incoming DS/BP
and returns far. There is no local ES save/restore on this branch and
no guard on the current offset word at ES:0064 before replacing it.
The native vector's ownership, prior-pointer producers and atomicity are not
established by this static local reading.

## Interpretation

The cleanup table target reaches mutable cache cleanup callbacks in a fixed
order. Its initial no-op words cannot exclude later native cleanup, and the
callbacks' state changes cannot prove native success. Q-EXE-007 retains
complete pointer/state writers, aliases, actual segments and frames, driver and
interrupt contracts, caller coverage and the remaining general callback
registration dependencies. No complete manager cleanup or game launch exclusion
is claimed.

## Alternatives

Permanent no-op callbacks contradict the cache setup stores. Clearing EMS state
only after checked release contradicts the absent result test. Always clearing
the extended-memory marker contradicts both its driver-call path and the vector
mismatch path. Giving both driver calls the originally loaded handle ignores
the second reload. Treating the vector check as a full far-pointer comparison
ignores its segment-only comparison. Treating near callback dispatch as expecting
a near return ignores the pushed CS word.

## How to reproduce

At revision dae67257 require both identities from FND-EXE-350. Use MZ
header size 5200, modeled load segment 1000, relative code segments
3AE5/3AD6 and relative data segments 45CE/45BF. Decode each
address range in Locations in sixteen-bit mode, ending after its final
instruction. Read the two shipped callback words at the file-data locations;
data bases are 4AEE0/4ADF0. Read header relocation count at 0006
and table offset at 0018 and check the four indices/operands above.
Trace the root's zero/nonzero handle guard, ordered target reloads and far
frames, and every EMS flag/handle and extended marker/handle/vector branch.
Follow result tests and store order rather than assigning native success.
Use FND-EXE-562/563/565 for the already-recorded helper and setup context.
No negative caller or writer search is claimed. Licensed bytes stay outside
Git; no game process, DOSBox or emulated call runs.
