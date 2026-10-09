---
id: FND-EXE-288
title: Cleanup pointer and byte gate are published by an unchecked interrupt setup path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0018..44B6:0075
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:0482..44DE:0496
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0011..44B6:0018
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The setup at 44B6:0018 saves DS, AX and DX and loads DS from a
relocated immediate resolving to 57E0. Byte 0x33D4 equal to one
bypasses setup and reaches a local AX-zero assignment followed by
saved DX/AX/DS restoration and far return. Thus even that AX-zero
assignment does not replace the saved incoming AX at ordinary return.

Other byte values prepare one outgoing word by pushing AX, pushing BP,
setting BP from SP and writing nine at SS-relative BP plus two.
Restoring BP leaves nine as the argument. The far call resolves to
44DE:0482. That wrapper saves ES and BX, sets AH to 0x35, loads AL
from SS-relative BP plus six, reaches INT 21h and copies post-interrupt
ES and BX to DX and AX before restoring ES/BX/BP and far-returning.
The setup removes its argument, then writes returned DX and AX to
CS-relative words 0x000E and 0x000C, respectively. It makes no local
status test before these pointer-word publications.

The setup saves flags and disables maskable interrupts. It pushes CS
and AX, temporarily saves BP and writes 0x00B5 over that pushed AX
at BP plus two. It restores BP, pushes AX again, saves BP again and
writes nine over this latest pushed AX at BP plus two. After restoring
BP it calls the resolved 44DE:046D wrapper of FND-EXE-287. The three
outgoing words are therefore nine, 0x00B5 and current CS, rather than
the two AX values originally pushed. It removes six argument bytes and
restores flags without testing an interrupt result.

It then pushes CS and near-calls 44B6:0011. That body sets AH to two,
reaches INT 16h, clears AH and far-returns. The setup stores returned
AL to then-current DS-relative byte 0x33D6 and one to byte 0x33D4,
in that order, before its common saved-register restoration. No local
rollback of the saved pointer or gate publication occurs. This final
interrupt wrapper does not locally save or restore DS.

## Interpretation

FND-INPUT-005 already summarizes this setup. This reading supplies the
precise writer and argument provenance for FND-EXE-286/287's cleanup:
the saved CS-relative pointer pair comes from the first interrupt
wrapper's ES/BX result, and the later cleanup selector is nine while
those pointer words remain intact. The byte gate is published after
the setup requests, without a success test on either INT 21h request.
An exact-one gate therefore records the local setup path, not proven
successful external installation.

Q-EXE-001 and Q-EXE-010 retain interrupt contracts, returned-pointer
admission, other pointer/gate writers, DS preservation across interrupts,
aliases and lifecycle callers. The final byte stores use DS then in force;
initial selection of 57E0 does not alone prove that segment remains
selected after all calls. No complete reading or original run is claimed.

## Alternatives

Treating the saved pointer as a constant handler ignores its interrupt
producer. Treating the setup's outgoing offset as returned AX ignores
its explicit replacement by 0x00B5. Treating the gate as proof of
successful installation ignores the absent status tests. Treating the
local AX-zero assignment as a returned success status ignores saved AX
restoration.

## How to reproduce

At revision d2da78a, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. Resolve
operand site/targetOffset pairs (0x00039D7C, zero),
(0x00039D94, 0x0482) and (0x00039DBC, 0x046D) with the committed
operand reporter and loadSegment 0x1000. With locked Capstone 5.0.7
in sixteen-bit x86 mode decode shipped half-open ranges
0x00039D78..0x00039DD5, 0x0003A462..0x0003A476 and
0x00039D71..0x00039D78 at initial IPs 0x0018, 0x0482 and 0x0011.
Follow the exact-one bypass, each temporary-BP argument writer, saved
pointer stores and final byte publications. Compare FND-EXE-286/287's
cleanup consumer and wrapper contract. Source and reports stay outside Git;
no original execution or complete caller/writer search is claimed.
