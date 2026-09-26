---
id: FND-SCRIPT-005
title: The script interpreter dispatches bytes 0x00 to 0x80 through a 129-entry table at 57E0:030A, and 15 of its entries and every byte above 0x80 stop the script
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:018F..172C:01C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:030A..57E0:040C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:20A2..172C:20B3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:00EE..172C:00FE
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The routine at `172C:018F` takes one byte, the opcode. When it is above `0x80` it calls
`172C:20A2`. Otherwise, when the double word at `57E0:02F6` is not 0, it far-calls the address
held there with the opcode as argument, and then calls the near routine in segment `172C` whose
offset is the word at `57E0:030A + 2 * opcode`. `57E0` is the program's data segment, which the
routine's `DS` holds.

The 129 words from `57E0:030A` (file offsets `0x4D30A` to `0x4D40C`) are initialized in the file.
As `opcode:offset`, all in segment `172C`:

```text
00:1F49 01:0A69 02:0A99 03:0AAD 04:0AC1 05:0AD6 06:0AEA 07:0AFE 08:0B13 09:12D2 0A:0B2E
0B:0B50 0C:1F3B 0D:1F71 0E:0C5F 0F:12A3 10:0CD1 11:0D67 12:0D93 13:0DA0 14:0DA9 15:0DC9
16:0DCD 17:0DD4 18:0ED3 19:0EE5 1A:0D0B 1B:11D7 1C:120C 1D:1241 1E:131B 1F:1331 20:0C8B
21:0EE9 22:0F84 23:0F75 24:0F79 25:0FC2 26:20A2 27:0E31 28:1012 29:0E89 2A:1016 2B:101C
2C:1022 2D:0B57 2E:1030 2F:1034 30:111C 31:1127 32:112C 33:1150 34:11B5 35:124C 36:126D
37:127F 38:12BC 39:12DD 3A:1347 3B:136B 3C:139C 3D:13C5 3E:1587 3F:15E2 40:13E0 41:151C
42:1610 43:1643 44:165D 45:1677 46:1692 47:16AD 48:16B9 49:1162 4A:20A2 4B:0D9C 4C:20A2
4D:20A2 4E:20A2 4F:16BD 50:16E1 51:1705 52:170B 53:20A2 54:1742 55:20A2 56:20A2 57:20A2
58:1763 59:1804 5A:18A5 5B:1917 5C:1989 5D:174D 5E:1A43 5F:1758 60:20A2 61:0EB9 62:1B30
63:1B42 64:1B65 65:1C9D 66:1CCA 67:1B6E 68:1B88 69:1BC2 6A:1BFC 6B:1C36 6C:1C70 6D:1CF7
6E:1D24 6F:1D51 70:1D7A 71:20A2 72:20A2 73:20A2 74:20A2 75:20A2 76:1DA7 77:1DCF 78:1DF7
79:1E23 7A:1E53 7B:1E7B 7C:1EA3 7D:1ECF 7E:1EFF 7F:1F1D 80:0D39
```

Fifteen opcodes, `0x26`, `0x4A`, `0x4C` to `0x4E`, `0x53`, `0x55` to `0x57`, `0x60` and `0x71` to
`0x75`, lead to `172C:20A2`. That routine stores `0xFFFF` in the word at `4C0E:0009` and calls
`172C:00EE`, which stores 1 in the byte at `4C13:0326`. The interpreter's loops stop when that
byte is not 0 (FND-SCRIPT-007).

An earlier Ghidra reading took the table to be at `4C13:030A`, found zeros there in the file, and
concluded that it was filled at run time.

## Interpretation

The byte code has 129 instructions, `0x00` to `0x80`, each with its own handler, apart from the
fifteen that share the handler that stops the interpreter; a byte above `0x80` where an
instruction is expected stops it too. The far routine at `57E0:02F6`, when one is set, sees each
opcode before its handler runs. The earlier reading used the wrong segment for `DS`; the table
the code indexes is complete in the file.

## Alternatives

What sets the double word at `57E0:02F6`, and to what, was not found. Whether any code writes the
table at run time was not checked; no direct write to it was seen in segment `172C`.

## How to reproduce

Disassemble `172C:018F`, then read 129 words from file offset `0x4D30A`
(`0x5200 + (0x57E0 - 0x1000) * 16 + 0x030A`) and `172C:20A2` and `172C:00EE`.
