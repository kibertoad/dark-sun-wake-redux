---
id: FND-EXE-573
title: The disc drivers install vectors only for the handlers FND-EXE-496 decodes from, build no transfer on the stack and write no code, and every CD byte in them is a decoded interrupt, inside a decoded instruction or in table data
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CD:ALGDIG.ADV
    offset: 0x000006EA..0x00000727
  - build: BLD-GOG-EN-1.1
    file: CD:ARIADIG.ADV
    offset: 0x000009A2..0x000009E1
  - build: BLD-GOG-EN-1.1
    file: CD:PASDIG.ADV
    offset: 0x0000039D..0x000003DA
  - build: BLD-GOG-EN-1.1
    file: CD:SBDIG.ADV
    offset: 0x000003FB..0x00000438
  - build: BLD-GOG-EN-1.1
    file: CD:SBPDIG.ADV
    offset: 0x000004A5..0x000004E2
  - build: BLD-GOG-EN-1.1
    file: CD:SBAWE32.ADV
    offset: 0x00003B3D..0x00009BB3
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/disc_adv_entry_kinds.py)
environment: null
---

## Observation

Offsets are file offsets in the 19 disc drivers of FND-EXE-492. Each driver was decoded from the
starts FND-EXE-496 uses (its function table, the handlers its code passes as far pointers and
SBAWE32.ADV's two bounded tables), and each decoded file was searched for the three kinds of entry
that decode does not start from.

**Vectors the drivers install.** No file holds a `CD 21` with a `mov ah,25` or `mov ax,25xx` in
the 8 bytes before it, decoded or not. Ten decoded instructions load DS with a register zeroed
just before, two in each of ALGDIG.ADV, ARIADIG.ADV, PASDIG.ADV, SBDIG.ADV and SBPDIG.ADV, and no
other decoded instruction loads ES or DS that way. In each file the first of the two is in a
routine that takes the vector number from a word of its own segment, adds 8 (or 0x68 for 8 and
above), keeps the vector's old far pointer in its segment and stores the far pointer at `[bp+6]`
into the vector: ALGDIG.ADV `0x06EA` (store at `0x0722..0x0727`), ARIADIG.ADV `0x09A2` (`0x09DC`),
PASDIG.ADV `0x039D` (`0x03D5`), SBDIG.ADV `0x03FB` (`0x0433`) and SBPDIG.ADV `0x04A5` (`0x04DD`).
The second puts the kept pointer back. A search of every byte offset for an `E8` call to each
routine, and for a word equal to its offset, finds 21 calls and no word. Each call pushes
`push cs; push ax` after `mov ax,handler`:

| File | Calls | Handlers passed |
|---|---|---|
| ALGDIG.ADV | `0x0BF8`, `0x0CAE`, `0x0D7E` | `0857`, `0857`, `08C5` |
| ARIADIG.ADV | `0x080E`, `0x0D4E`, `0x0E1A`, `0x0E9F`, `0x11BF` | `0A7C`, `0A96`, `0A96`, `0A96`, `0A7C` |
| PASDIG.ADV | `0x094D`, `0x0A03`, `0x0AD3` | `0539`, `0539`, `05CA` |
| SBDIG.ADV | `0x092C`, `0x0997`, `0x0A4D`, `0x0B1D`, `0x0CFD` | `058F`, `058F`, `058F`, `05F9`, `0568` |
| SBPDIG.ADV | `0x09EA`, `0x0A55`, `0x0B0B`, `0x0BDB`, `0x0DFC` | `0639`, `0639`, `0639`, `06A3`, `0612` |

**Transfers built on the stack.** No decoded call or jump goes through an operand based on BP or
SS, and no decoded `ret` or `retf` directly follows a push.

**Code written at run time.** No decoded store with a CS override writes into a decoded
instruction. The 52 decoded string stores (`stos`, `movs`) that run after `push cs; pop es` fill
ranges given by the `mov di` and `mov cx` immediates before them, and none of those ranges covers a
decoded instruction.

**CD bytes.** Of the 79 bytes `CD` in the 19 files, 15 are the decoded interrupt instructions
FND-EXE-496 lists, 43 lie inside other decoded instructions, and 21 lie in bytes the decode does not
cover:

- PASFM.ADV `0x00BF` and PASOPL.ADV `0x00AB` are inside the function table at `0x002D..0x00C9`,
  in the offset words `14CD` and `16CD`.
- PCSPKR.ADV `0x010B` follows the text `IBM PC or compatible internal speaker` in the bytes
  before the first decoded instruction at `0x027F`.
- SBAWE32.ADV's 18 lie in `0x3B3D..0x9BB3`, the span that holds the two near tables the decode
  reads at `0x3B42` and `0x4428`. They sit among word values (`... 00 cd 00 c6 00 ...`).

The decode holds the interrupts that the slot writers of FND-EXE-496 depend on: GF1DIGI.ADV and
GF1MIDI.ADV store BX and ES as returned by interrupt 21 AH=35, and PASDIG.ADV, PASFM.ADV and
PASOPL.ADV use BX and DX or DX:BX from interrupt 2F AX=BC03, and each of those interrupts is a
decoded instruction.

## Interpretation

The drivers' interrupt handlers are installed by a direct store into the vector table, not through
DOS, and every handler a driver installs is one FND-EXE-496 already decodes from, so that decode
lacked no installed vector. The drivers build no transfer on the stack and write over none of
their own decoded code, so neither kind of entry is missing either. The byte search for `CD` is the
control: it finds the interrupt instructions without the decode, and every one it finds in a
driver is a decoded interrupt, a byte inside another decoded instruction, or a byte in a function
table, a description or SBAWE32.ADV's table span. The interrupts that the slot writers' data flow
requires are among the decoded ones.

With FND-EXE-496, this leaves the drivers' census with no missing kind of entry and a control. The
drivers make no program-execution request in the code their entries reach.

## Alternatives

- A vector-table store through a segment register loaded from memory, or any store whose segment
  is not zeroed in the three instructions before, is outside the vector search.
- A string store whose DI or CX is not an immediate before it would not be checked; all 52 found
  have both.
- SBAWE32.ADV's span `0x3B3D..0x9BB3` is not shown to be data byte by byte. FND-EXE-498 and
  FND-EXE-501 read its use from the sound utility.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, run
`python -I tools/research/exec-census/disc_adv_entry_kinds.py <install dir>
spec/builds/BLD-GOG-EN-1.1.files.yaml`. It reads the drivers from `game.gog` as
`disc_adv_exec_census.py` does, checks their identities against the manifest, and prints the
vector, stack and code-write searches, the callers of each install routine with the handler each
passes, and the `CD` byte classification for each driver.
