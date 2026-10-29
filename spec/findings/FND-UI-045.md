---
id: FND-UI-045
title: The window routines keep a five-bit state in the low bits of the flags word, registration sets bits 0x40 and 0x80 from the children, and no literal operand reads bits 0x800 or 0x2000
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: claude
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:02B3..3A8E:05EF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:060D..3A8E:06FA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0930..3A8E:0946
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0A4C..3A8E:0A63
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0C2B..3A8E:0C7B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:1069..3A8E:1089
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0442..3EBE:0461
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:07CB..3EBE:07F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:08CA..3EBE:08EF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:012F..3F96:0144
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:056B..409B:0586
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0067..3EBE:0073
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:0885..409B:0891
tool: tools/research/exec-census/field_reads.py, field_stores.py, field_offsets.py and resident_listing.py at commit 7cdc59c1; Python 3.14.7, Capstone module version 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FMT-UI-001 gives the word at window offset `0x9E` as `flags`, with two
known bits: the child dispatcher `3D72:0EB8` passes over a window with
bit 4 set and stops after one with bit `0x100` set (FND-UI-068). The
shipped values are 256 in windows 14000 to 14002, 8,192 (`0x2000`) in
19500, 2,048 (`0x800`) in 19501 and 0 in the others. Q-UI-001 asks
what the other bits do.

**Census.** Every instruction with a memory operand through a base or
index register and displacement `0x9E` or `0x9F`, as a read and as a
write, was listed by decoding each range of the resident function
inventory linearly and, as a second pass, at every byte of the load
image and of every flags-3 overlay code span. Displacements `0x9C` and
`0x9D` were listed too, for wider accesses that would cover the flags
word, and immediate or `lea` additions of `0x9E` and `0x9F`, for a
pointer rebased onto the field.

- Displacement `0x9F` gives no read and no write.
- The linear pass gives 26 reads and 7 writes at `0x9E`, all resident,
  all through `ES:BX` loaded from a far pointer. They are listed below.
- Every second-pass hit at `0x9E` that the linear pass did not print is
  one of three kinds: the same instruction decoded one byte later without
  its `ES` prefix; an `add byte [bp+si+9Eh], bl` or `idiv` decoded inside
  another instruction (the `idiv` lies inside the near jump at
  `4842:0CE5`, one of the driver thunks whose `mov ax, 9Eh` loads a
  function number); or the two `or` instructions at `3A8E:050B` and
  `3A8E:053A`. Those two lie in `3A8E:0440..05AA`, which the inventory
  leaves out of `3A8E:02B3`, and are instruction starts reached through
  that routine's bounded tag table (below).
- Displacement `0x9C` occurs only as `les`, `cmp`, `push` and one store
  of a far pointer in module `409B`, each through the routine's first
  argument, which is the edit box record (409B's own constructor stores
  its window at that record's `+0C`, below). Displacement `0x9D` occurs
  only as byte reads in overlays 209 and 210, which do not reach the
  flags word. The only rebasing hit is the driver thunk `mov ax, 9Fh`
  at `4842:0CEE`.

Positive controls are the two FND-UI-068 tests at `3D72:0F12` (`0x100`)
and `3D72:0F2B` (4), both found.

**Whose record.** In module `3A8E` the pointer is the window argument
or a link in the list of registered windows at `DS:A105`. In `3EBE:071F`,
`3EBE:07FF` and `3F96:0097` it is the routine's first argument, a window
in which `39D1:049C` then looks up a child by tag (`BUTN` or `APFM`) and
number. The button painter `3EBE:042B` loads it from its button's far
word `+54`, which the button constructor sets from its second argument
at `3EBE:006E`; registration passes the window there. `409B:0552` loads
it from its box's `+0C`, which the box constructor sets from its second
argument at `409B:088C`, again the window. `39D1:097D`, `3EBE:0BFD` and
`4066:0003` use the current window at `DS:A11D` (FND-CONFIG-216).

**Low five bits.** The three writes that are not single-bit changes each
read the word, keep the bits `0xFFE0`, store it, then set one bit:

| Routine | Store | Reached |
|---|---|---|
| `3A8E:02B3` | low five bits := 2 | after the child loop, on its one success path; every failure path returns `0xFFFF` earlier |
| `3A8E:060D` | low five bits := 8 | on every path with a nonnull window, after it moves the window to the head of the list (unless `DS:9DEE` bit `0x400` is set or it is already there), stores it at `DS:A11D` and calls `4328:00E0`, `4328:005A`, `4328:009E` and `3A8E:0003` |
| `3A8E:08FB` | low five bits := 4 | after `3A8E:0F7F` returns 0 for a nonnull window; it then moves the window to the tail of the list |

`3A8E:0D9A` clears bit 4 of the window it makes current
(FND-CONFIG-096). No instruction in the census writes bit 1 or bit
`0x10`.

The readers of these bits:

- `3A8E:0C2B` returns 1 for a window number with no registered window,
  16 for the window at `DS:A11D`, and otherwise the word masked to its
  low five bits (FND-UI-044).
- The button painter `3EBE:042B` masks the window's word to five bits
  and goes to its exit at `0680` when the result is below 8.
- `3EBE:071F` sets or clears bits of a button's words `+58` and `+0C`,
  chosen by its third argument through a six-entry table; `3EBE:07FF`
  copies a formatted string into a button's tail bytes; `3F96:0097`
  passes a frame's `+0C` to `3CFA:0586` and `3CFA:0423`.
  Each then repaints only when the window has bit 8 or bit `0x10` set.
  `3EBE:07FF` also passes each of the two bits as a 0-or-1 argument.
- Bit 4 is tested twice by `3A8E:0003`, by the redraw
  loop `3A8E:1048`, which calls `3A8E:060D` for every registered window
  without it, by the dispatcher `3D72:0EB8`, and by `409B:0552`, which
  jumps past its body to `079A` when its box's window has it.

**Bits set from the children.** Registration `3A8E:02B3` loads each
child and compares its tag with the five tags at `3A8E:05EF`: `ACCL`,
`APFM`, `BUTN`, `MENU` and `EBOX`, whose word targets follow at
`3A8E:0603`. A tag that is none of them skips to the next child.

- `ACCL` (`3A8E:0537`) sets bit `0x80`.
- `BUTN` (`3A8E:04B3`) calls the button constructor `3EBE:0008` and, on
  a result other than `0xFFFF`, sets bit `0x2000` of the button's word
  `+0C` when the window's word `+A4` has bit 2 and the button's word
  `+0E` has bit 1 clear. It then sets the window's bit `0x40` when the
  button's byte `+6C` is not 0. The constructor does not write `+6C`.
- `APFM`, `MENU` and `EBOX` call their constructors and change no flag
  bit.

The key-event builder `39D1:097D` calls the ACCL search `4066:0003` only
with bit `0x80` set and the button mnemonic search `3EBE:0BFD` only with
bit `0x40` set; each search tests its bit again (`4066:0020`,
`3EBE:0C31`) (FND-CONFIG-216).

**Bit `0x4000`.** The removal routine `3A8E:0A26` passes the window's
word `+A0` to `1BF3:28C5` when bit `0x4000` is set (FND-CONFIG-168). No
instruction in the census sets it.

**Bits `0x800` and `0x2000`.** No instruction in the census tests, sets
or clears either bit of the window word. The only instructions that touch
them are the three read, mask and store sequences above, which keep them.

## Interpretation

The low five bits of a loaded window's flags word are a state the window
routines own, and the value in the file does not survive registration:
2 once registered, 8 once `3A8E:060D` has made it current and drawn it,
4 once `3A8E:08FB` has moved it back. Bit 4 marks a window that
`3A8E:0003`, the redraw loop, the dispatcher and the edit box skip; bits
8 and `0x10` let a control change repaint at once. Bit `0x10` has no
writer in the census, so on these routines the repaint test depends on
bit 8 alone.

Bits `0x40` and `0x80` are set by registration from the children: `0x80`
when the window has an `ACCL` child, `0x40` when one of its buttons has a
nonzero byte `+6C`. They admit the two keyboard searches. The shipped
`BUTN` records have 0 at `+6C` (FND-UI-004), so the `0x40` path needs a
writer of that byte that this census does not cover.

Bits `0x100`, `0x800` and `0x2000` come from the file and pass through
registration. `0x100` stops the dispatcher; for `0x800` (window 19501)
and `0x2000` (window 19500) this census finds no consumer.

## Alternatives

- The low five bits are designer-set flags read from the file: every
  write above replaces them, and every shipped window has them 0.
- Bit `0x2000` of window 19500 is the bit `0x2000` registration sets on
  some buttons: that store goes to the button's word `+0C`, not to the
  window.
- Bits `0x800` and `0x2000` are unused: consistent with the census, but
  not established by it. A whole-record copy, a word read into a
  register through a different displacement after rebasing by a computed
  value, a callback, or code outside the decoded spans could still read
  them.

## How to reproduce

Use installed `DSUN.EXE`, size 634416, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`, and the locked evidence Python,
with the resident inventory `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`, in
`tools/research/exec-census/` at commit 7cdc59c1:

1. `field_reads.py <dsun> <inventory> 9e`, then `9f`, then `9c 9d`;
   `field_stores.py` with the same displacements; `field_offsets.py
   <dsun> <inventory> 9e 9f`.
2. Map each `outside` hit's file offset to the inventory rows (load image
   header `0x5200`, segment `0x1000`) and decode around it.
3. `resident_listing.py <dsun>` over `3A8E:02B3..05F0`,
   `3A8E:060D..0710`, `3A8E:08FB..0A26`, `3A8E:0A26..0A70`,
   `3A8E:0C2B..0C7B`, `3A8E:1048..10A7`, `3A8E:0003..0070`,
   `3A8E:01A0..01E0`, `3EBE:0008..042B`, `3EBE:042B..0480`,
   `3EBE:071F..07F3`, `3EBE:07FF..08F0`, `3F96:0097..0150`,
   `409B:0552..0590`, `409B:0853..08C0`, `39D1:097D..0B10`,
   `3D72:0F1F..0F35` and `4842:0CD0..0CF8`.
4. Read the five tags at file `0x300CF` (`3A8E:05EF`) and the five words
   after them.

The reports are under `GAME_DIR/analysis/ui-window-flags-20261018/`. No
original game or function was executed.
