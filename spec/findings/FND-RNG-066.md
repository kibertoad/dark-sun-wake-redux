---
id: FND-RNG-066
title: Of FND-RNG-058's nine unresolved transfers, one never runs, four chain to interrupt vectors saved from outside the program, one returns to the startup walker, two jump through eight-entry tables of segment 2707, and the timer callback slot 0x46 holds the handler's own chain entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1D66..1000:1D96
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1DE6..1000:1DFC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2A07..1000:2A2A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2D51..1000:2D62
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0244..1000:0264
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2877..1000:287F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2984..1000:298E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2707:0177..2707:01AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2707:057B..2707:05B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0027..44B6:0042
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0525..4842:054B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:10FC..4AE5:112D
tool: Capstone 5.0.9 over the load image, a direct-operand search over the function inventory's rows with an optional segment immediate, and tools/research/exec-census/far_pointer_words.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, with the load image at segment `1000` and `DS` = `57E0`.

**`1000:1D8D`, `call far [A4D0]`.** In the interrupt 24 handler `1000:1D66` (FND-EXE-578), the
call runs only when the word `DS:A4CE` is not 0 (`1000:1D7A`). The direct-operand search finds
two stores to `DS:A4CE`: 0 at `1000:1DBA` and 1 at `1000:1DF6`, in the routine `1000:1DE6`,
which also stores its two argument words at `DS:A4D0` and `DS:A4D2`, the only stores to them. A
byte search finds no far call `9A E6 1D 00 00` in the file and no near call to `1DE6` in segment
`1000`, and `far_pointer_words.py` finds no far pointer to `1000:1DE6` (its one hit, in overlay
184, is an immediate with no segment word).

**`1000:2A24`, `call far [A4DE]`.** In the interrupt 5 handler `1000:2A07`, the call runs when the
byte at the interrupted code's address is not `0x62`. `DS:A4DE` and `DS:A4E0` are stored only at
`1000:2D5B..2D62`, from `1000:0494` with 5, which is interrupt 21h function `0x35`: the interrupt
5 vector as it was before `signal` (`1000:2C77`) installs `1000:2A07`.

**`1000:298A`, `jmp [3916]`.** `DS:3916` is stored only by `pop word ptr [3916]` at `1000:2877`,
the routine's first instruction, which takes its return offset. `1000:2877` is the fifth startup
record (`DS:39AC`: type 0, priority 16), which the record walker calls with the near call
`call word ptr es:[bx+2]` at `1000:025C` for type 0, so the jump goes to `1000:0260`. No other
call to `1000:2877` was found.

**`2707:0188` and `2707:058C`, `jmp far cs:[bx+...]`.** Each is reached only after comparing the
word `bp+0xE` with 8 and branching away when it is 8 or more (`2707:017A`, `2707:057E`); `BX` is
that word times 4. The tables hold eight far pointers with segment word `0x1707` (relocated to
`2707`): at `2707:018D`, `01AD`, `01CB`, `01F1`, `0208`, `0218`, `0224`, `023A` and `026E`; at
`2707:0591`, `05B1`, `05C2`, `05EF`, `0600`, `062C`, `063C`, `0668` and `0678`. The first
table's cases copy with `rep movsb` from the segment `1BF3:42FB` returns for `bp+6` to the one it
returns for `bp+8` and end at `2707:0292`; the second table's cases call the near routines
`2707:0395`, `2707:02A8` and `2707:048E` and end at `2707:06A4`.

**`44B6:00B8`, `call far cs:[000C]`.** `44B6:000C` and `44B6:000E` are stored only at
`44B6:0039..0042`, from `44DE:0482` with 9 (interrupt 21h function `0x35`). A search for the
operands with segment word `0x34B6` finds no store from another segment.

**`4842:051A`, `call far cs:[011A]`.** `4842:011A` and `4842:011C` are stored only at
`4842:0534..053E`, from the interrupt 8 entry of the vector table, before the same routine stores
`4842:0519` with `CS` in `4842:0046..0049`. A search with segment word
`0x3842` finds no store from another segment.

**`4842:0498`, `call far cs:[si+6]`.** Besides `4842:0541`, slots are stored at `4842:07B7` from
`SI` and `DS`, the registration FND-EXE-585 reads.

**`55E3:003F`, `jmp far cs:[002F]`.** `55E3:002F` and `55E3:0031` are stored only at
`4AE5:110D..111F`, from the interrupt 19h entry of the vector table, before `4AE5:111F` and
`4AE5:1126` install `55E3:003F` for interrupt 19h. That path runs only when the far pointer at
`[0x43]` of the overlay manager's data is null.

## Interpretation

`1000:1D8D` never runs, since nothing calls `1000:1DE6`, so `DS:A4CE` stays 0. `1000:2A24`,
`44B6:00B8`, `4842:051A` and `55E3:003F` chain to the interrupt 5, 9, 8 and 19h handlers that
were installed before the program ran, outside the file's code. `1000:298A` returns to the startup
walker, which FND-RNG-058's walk already decodes. The table jumps of segment `2707` go to 16
cases, which with the routines they call are the code still to read. The timer callbacks are
`4842:0519`, an established entry, and the ones FND-EXE-585 lists; FND-RNG-065 sees `4654:09A9`,
`31E0:31A7` and `4842:0519` taken.

## Alternatives

A call to `1000:1DE6` through a pointer built at run time from separate words is outside the byte
and pointer searches. The handlers that run before the program are the machine's and DOS's; what
they write is not part of the game's code.

## How to reproduce

Disassemble the ranges above from the load image (`file offset = header + (segment − 0x1000) ×
16 + offset`), search the inventory rows for operands `[0xa4ce]`, `[0xa4d0]`, `[0xa4de]`,
`[0x3916]`, `[0x2f]` with segment word `0x45E3`, `[0xc]` with `0x34B6` and `[0x11a]` with
`0x3842`, search the file for the bytes `9A E6 1D 00 00` and for `E8` near calls in segment
`1000` landing on `1DE6`, and run `far_pointer_words.py <dsun> 1000:1DE6`.
