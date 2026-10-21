---
id: FND-EXE-578
title: No interrupt 21 request in the installed DSUN.EXE selects AH=4B and the file holds no interrupt 2E; of its 100 interrupt 21 sites the walk from its established entries reaches 80, and its two stack-built interrupt routines are called only for interrupts 2F and 21 with AX values other than 4Bxx
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1E8C..1000:1FF4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2F03..1000:2F13
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1E32..1000:1E41
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:03CB..2660:03ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:0630..2660:0641
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067CB1..0x00067CC1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067D42..0x00067D53
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:0176
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2C77..1000:2DA2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0018..44B6:0075
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0140..4AE5:0162
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:053E..4842:0561
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:1103..4AE5:112D
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_census.json, dsun_interrupt_census.py, far_pointer_words.py); Capstone 5.0.9 for the byte and reference searches
environment: null
---

## Observation

The file is the installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`). Resident addresses are written in the segment of the
segment-table descriptor whose span holds them (FND-EXE-571), with the load image at segment
`0x1000` and file offset `0x5200 + (segment - 0x1000) * 16 + offset`. Overlay code is given as
`DESCRIPTOR+OFFSET` in the text and as a file offset in the locations (FMT-EXE-003). The data
segment is `57E0`.

**Regions.** The census reads 205 regions: the code of every overlay (descriptors with flags 3) and
the span of every resident descriptor with flags 0 or 1 that is not empty (FND-EXE-571).

**Sites in the whole file.** Searching every byte of the file for the pairs `CD 21`, `CD 2F` and
`CD 2E` finds 101, 5 and 0. In the regions there are 100 `CD 21` and 4 `CD 2F` pairs, all in
resident code: in segments `1000`, `15F3`, `44DE` and `4AE5`, and the `CD 2F` pairs at
`15F3:00CC`, `15F3:00D8`, `4AE5:0F40` and `4AE5:0F49`. The pairs outside the regions are bytes of
fixup lists: the `CD 21` at file `0x6AF68` is the fixup offset word `0x21CD` in overlay 182's list,
and the fifth `CD 2F`, at file `0x5ECB2`, lies in overlay 173's list.

**Entries.** The `reach` walk starts from:

1. The MZ entry point `1000:0000`.
2. The startup records `DS:3994..39BE`, which the record walker `1000:0220` runs when the entry
   code calls it at `1000:0149` with those bounds (six-byte records: a type, 1 for a far pointer, a
   priority and a pointer): `4AE5:0D27`, `1000:0886`, `1000:11C1`, `1000:12D9`, `1000:2877`,
   `1000:29AA` and `1000:3B56`.
3. The exit record `DS:39BE..39C4`, run by the exit walker `1000:0264` called at `1000:0170`:
   `4AE5:0193`.
4. The installed interrupt handlers, from every AH=25 site and every store into the vector table:
   - `1000:01A7` for interrupt 0 (AH=25 at `1000:01EF`). The other startup AH=25 sites,
     `1000:01FB` to `1000:021C`, restore the vectors 0, 4, 5 and 6 saved at startup.
   - The handlers passed to the set-vector routine `1000:04A3` (AH=25 with the vector in AL and
     the handler in DS:DX) by its seven near callers, each handler a relocated segment word with an
     offset: `1000:0338` for 23 (`1000:037F`), `1000:1D66` for 24 (`1000:1DDE` and `1000:1E0A`),
     `1000:2AF9` for 0 (`1000:2D31`) and `1000:2A07` for 5 (`1000:2D70`). In `signal`
     (`1000:2C77`), the call at `1000:2D90` installs `1000:2A88` for 6 (signal 4), and two
     paths jump to it with other values: `1000:2B6A` for 4 (signal 8, after `1000:2D31` installs
     `1000:2AF9` for 0), and for signal 2 either `1000:2BDB` for 23 or the vector kept at
     `DS:A4DA`. The seventh caller, `1000:2C3C` in
     `1000:2BDB`, restores the vector kept at `DS:A4DA`. The callers of `signal` (`277B:0247`,
     `277B:0257`, `277B:0267`) pass signals 8, 4 and 11 only, so `1000:2BDB` is never installed
     and is not a start.
   - `44B6:00B5` for 9, passed by `44B6:0018` to the set-vector routine `44DE:046D` (AH=25 at
     `44DE:047B`), whose other caller restores the saved vector.
   - `4AE5:04F4` for 3F (FND-EXE-562), from the far pointer at words 2 and 4 of the overlay
     manager's data (AH=25 at `4AE5:0160`).
   - The timer module's interrupt 8 handler `4842:040C`, written into the vector table at
     `4842:0557` with ES 0, and the routine `4842:0519` it stores; its other vector-table write,
     at `4842:0587`, restores the saved vector.
   - `55E3:003F` for 19, written into the vector table at `4AE5:111F` with ES 0; the write at
     `4AE5:1195` restores the saved vector.

   A search of every byte offset of the regions for a store whose segment register was last
   loaded, in the 80 bytes before it, from a register cleared to 0 finds only these vector-table
   writes and three stores whose segment register comes from another path: `1000:3F71` and
   `1000:3F96`, whose ES an `les` loads, and `4AE5:0CF1`, which follows
   an unconditional jump.
5. The 157 indirect jumps of the form `jmp cs:[bx + table]` whose index a `cmp` and `ja` or `jbe`,
   or a value scan, bounds, declared exhaustive, and every far call or jump whose segment word has
   an MZ relocation or an overlay fixup (7,562), an overlay's trampolines leading to the code
   offsets they name.

The targets are the 104 interrupt pairs and the two far calls through `[bp-4]` at `1000:1F10` and
`1000:1FBC`. The instruction controls `277B:0024` (the routine the entry code calls far at
`1000:0158`) and `188+1604` (the experience award of FND-PARTY-086) are both decoded.

**Result of the walk.** It reaches 1,362 routines and decodes 105,588 instructions in 1,478
ranges. It reports 83 unresolved computed transfers, 5 gap bytes and 4 contested starts, all of
the gaps and contested starts at `1000:02E9..1000:02F8`: the walk decodes the run-time error
message that follows the call at `1000:02C1`, which does not return, as code, and those
instructions overlap the message routine's own code after the text. It reaches 80 of the 100 `CD 21` sites, all 4 `CD 2F`
sites and both calls through `[bp-4]`. The walk's own list of interrupt instructions it decoded
holds 80 `int 21` and 4 `int 2F` instructions, every one at a site of the byte search, and no
`int 2E`.

**AH at each site.** Read from the start of the decoded range that holds a reached site, or from
the start of the inventory range that holds an unreached one (a gapless run of at most 40 bytes
for `1000:2002` and `44DE:0437`, where the inventory decode does not land on the site), the last
instruction that writes AH before the interrupt is an immediate load at every site but one:

| Sites | AH (count) |
|---|---|
| reached `int 21` (80) | 07 (1), 09 (1), 19 (1), 1A (1), 1C (1), 25 (8), 2A (1), 2C (1), 2F (1), 30 (5), 35 (8), 36 (1), 3C (2), 3D (6), 3E (9), 3F (4), 40 (5), 41 (1), 42 (5), 44 (5), 47 (1), 48 (1), 49 (1), 4A (2), 4C (1), 4E (1), 4F (1), 52 (1), 58 (2), 67 (1), 68 (1) |
| unreached `int 21` (20) | 0B (1), 0E (1), 1C (1), 2B (1), 2D (1), 39 (1), 3A (1), 3B (1), 3F (1), 40 (5), 43 (2), 44 (1), 4C (1), 56 (1), and `1000:2F0F`, which loads AH from its first argument |
| `int 2F` (4) | AX 4300 at `15F3:00CC` and `4AE5:0F40`, AX 4310 at `15F3:00D8` and `4AE5:0F49` |

For each immediate load, a search of the site's region at every byte offset for relative jumps,
calls, loops and `jcxz`, and of the declared far transfers and jump-table entries, finds a transfer
that lands after the load and up to the interrupt, from outside the span between them, for nine
sites. None of the nine sources is an instruction start of the code around it: `1000:02DC` lies
in the message text above, `1000:043F` in a `shr bx, cl`, `1000:07E1` in a `mul bx`, `1000:089E`
in a `shl bx, cl`, `1000:296C` in a `mov ds, cs:[2875]`, `44DE:002A` in a `mov dx, 47E0`,
`44DE:0277` in a `loop`, `44DE:0367` and `44DE:036A` in two `push word [bp+...]`, and `44DE:05BA` in
an `and cx, 27`.

**The argument-selected site.** `1000:2F03` sets AH from its first argument, AL from its third and
DX from its second, then issues `int 21`. Its only caller is the near call at `1000:1E3C`, which
pushes 0, 0 and 54 (AH=54, get verify flag). No other near call in the 64 KiB frame of segment
`1000` names it, and no word of the file equal to its offset is a relocated far call or pointer,
an overlay fixup or an immediate beside a segment word.

**Interrupts built on the stack.** Decoding every byte offset of every region for a `mov` or `push`
whose immediate has the low byte `CD` finds two instruction starts that build an interrupt
instruction in stack memory:

- `1000:1EB4` writes `55 CD n 5D CB` (n its first argument) into a local buffer, loads AX, BX, CX,
  DX, SI and DI from the input record its second argument points to and DS and ES from the
  segment record its fourth points to, and calls the buffer through a far pointer built from the
  buffer's offset and SS, at `1000:1F10`. Its only caller is `1000:1E8C`, which reads the segment
  registers into a local record and passes it.
- `1000:1F50` writes `8B 6E DE CD n` and a far return into a local buffer, loads the registers
  from the record its second argument points to, and calls the buffer at `1000:1FBC`.

For interrupts 25 and 26 both routines write a different tail; neither is passed 25 or 26.
`1000:1E8C` is called by the relocated far calls at `2660:03E8` (interrupt 2F, record AX 1510),
`2660:063C` (interrupt 2F, AX 1500) and overlay `180+0B6E` (interrupt 21, AX 4452). `1000:1F50` is
called only by overlay `180+0ADC` (interrupt 2F, AX 150D). In each caller the record's AX word is
written with the immediate shown and not written again before the call. `1000:1EB4` has the one
near caller `1000:1EAA`, inside `1000:1E8C`, and neither `1000:1EB4` nor `1000:1F50` is named by a
relocated far call, an overlay fixup or an immediate beside a segment word.

The other hits of the immediate search are not interrupt instructions: the eleven stores
`mov byte [di], CD` in overlays 172, 175, 202 and 209 are bytes of `add ax, 2BCD` decoded one byte
late. The eleven `push` hits of a word ending in `CD` are eight instructions, since the three
`push dword 3BCD` in overlay 178 also decode without their operand-size prefix: `push 2CD` at
`167B:019B` and pushes in overlays 178, 188, 190 and 211, each pushing a data offset or value as a
call argument, after a `push ds` or among the other pushed arguments.

**AH=4B by any immediate load.** A byte search of the regions for `B4 4B` (`mov ah, 4B`) and
`B8 xx 4B` (`mov ax, 4Bxx`) at every offset finds three hits: `4AE5:0474`, the operand of
`cmp ah, B8` at `4AE5:0472`, and `66 B8 50 4B 14 00` at `180+09BB` and `187+0E0C`, which load
EAX with the dividend `0x00144B50` before a `div`; neither overlay holds an interrupt pair.

**DOS's other entries.** Every far call in the file to offset `0005` or `0050` has a relocated
segment word naming `38DC`, inside the load image, except the bytes `9A 05 00` at file `0x4B94B`,
which are followed by `CD 3F`, an overlay trampoline.

## Interpretation

The kinds of established entry the walk had are the MZ entry point, the startup and exit records,
the interrupt handlers the startup code, the set-vector routines and the vector-table writes
install, the timer module's stored routines, bounded jump tables and relocated far transfers, with the overlay trampolines.
It did not have the routines the program registers at run time and calls through pointers (the 83
unresolved transfers: window and event callbacks, signal and critical-error handlers, the Ctrl-Break
handler, the overlay manager's hooks, function tables and the saved interrupt vectors), so 20 of
the `int 21` sites stay candidates.

Every interrupt 21 instruction in the file is censused, reached or not, and none selects AH=4B,
the DOS service that loads and runs a program. The two routines that build an interrupt on the
stack are called only for interrupt 2F with AX 1500, 1510 and 150D, the CD-ROM extensions'
installation check, device request and drive-letter services, and for interrupt 21 with AX 4452,
a version query. The one argument-selected site is called only with AH=54. The file holds no
interrupt 2E, the command interpreter's back door. So no code in `DSUN.EXE`, resident or overlay,
requests a program load from DOS. What the census does not cover is code outside the file that the
program transfers to: handlers whose addresses come from the interrupt vector table (AH=35 at
eight reached sites, and the vectors the timer and interrupt 19 modules save), the XMS driver's
entry from interrupt 2F AX 4310, and any other code address the program obtains from outside the
file. Which of the 83 unresolved transfers can reach such code is Q-EXE-007's remaining part.

## Alternatives

- FND-EXE-574 recorded the same census from 17 starts. Its list of installed handlers gave each
  caller of `1000:04A3` one handler and missed four: `1000:2B6A`, which reaches the shared call
  at `1000:2D90` through a jump, and `44B6:00B5`, `4AE5:04F4` and `55E3:003F`, which other
  set-vector routines and vector-table writes install. Its walk therefore left those handlers
  undecoded and reported 1,344 routines, 105,184 instructions in 1,457 ranges and 77 unresolved
  transfers. The walk from the four handlers reaches no interrupt site that the 17-start walk did not
  reach, and every site keeps its reading and value.

- A transfer whose target the walk did not resolve could enter code the census covers between
  an AH load and its interrupt only through a computed transfer that lands there; the window search
  covers relative branches, declared far transfers and jump tables, not computed calls or jumps.
  Such a landing would need a stored code address that points between two instructions of a DOS
  request wrapper.
- A program could also be loaded through code that writes interrupt instructions anywhere other
  than the stack, or through a far call to `PSP:0050` built at run time; the immediate searches
  cover an immediate `CD` written by an instruction, not bytes copied from data. No data byte pair
  outside the fixup lists is `CD 21`, `CD 2F` or `CD 2E`.
- AH can be loaded other than by `mov ah` or `mov ax` with an immediate; the AH reading marks such
  a write "not immediate", which happened only at `1000:2F0F`.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
`q007_dsun_census.json` holds the 21 starts, the 106 targets (the file offsets of the 104 pairs and
of the two calls through `[bp-4]`), the two instruction controls and `decodedRanges`.
`far_pointer_words.py <dsun> 1000:2F03 1000:1E8C 1000:1EB4 1000:1F50` lists the words equal to each
routine's offset. The near callers were found by decoding `E8` and `E9` at every byte offset of
file `0x5200..0x15200` and keeping those whose target is the routine. The immediate searches decode
an instruction at every byte offset of every region and keep `mov` or `push` with an immediate
whose low byte is `CD`, and search the regions for the byte patterns `B4 4B` and `B8 xx 4B`.
