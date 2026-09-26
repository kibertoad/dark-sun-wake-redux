---
id: FND-SOUND-005
title: DSUN.EXE holds no VOC signature, does not name SOUND_DS.EXE and has no DOS EXEC setup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0472..4AE5:0477
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportDosInt21Services); a PowerShell 5.1.26100.9444 scan of the whole file; repeated with Python 3.14.7 byte searches of the whole file
environment: null
---

## Observation

Searches of the whole of `DSUN.EXE`, 634,416 bytes, resident image and overlays alike, find:

- no occurrence of the 19 bytes `Creative Voice File`;
- no `SOUND_DS`, in any mix of upper and lower case;
- no `B4 4B` (`MOV AH, 4Bh`) and no `MOV AX` with 4Bh as its high byte. The bytes `B8 xx 4B` occur
  three times, at file offsets `0x404C4`, `0x67B9B` and `0x70C3C`: the first is inside
  `CMP AH, 0B8h` at `4AE5:0472` followed by a jump, the other two inside `MOV EAX, 00144B50h`.

Ghidra's report of the 101 decoded `INT 21h` sites found none with `MOV AH, 4Bh` in the twelve
instructions before it.

## Interpretation

The game does not check a whole Creative Voice File header and never starts the sound setup
program or any other program. It reads its voice files with a routine that knows their layout
(FMT-SOUND-001) and plays them through its own sound library (FND-SOUND-007, FND-SOUND-008).

## Alternatives

A service number loaded from a variable would not show in these searches. The setup program is run
by `SOUND.BAT`, outside the game.

## How to reproduce

Search `DSUN.EXE` for the VOC signature, for `sound_ds` ignoring case, and for `B4 4B` and
`B8 ?? 4B`, and decode the three matches of the last.
