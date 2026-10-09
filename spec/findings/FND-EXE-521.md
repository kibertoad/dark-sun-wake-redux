---
id: FND-EXE-521
title: Descriptor 198's first trampoline enters a 227-byte procedure whose four dispatch tables name only its own instruction starts
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087459..0x0008753C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008753C..0x000875C2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004C8A0..0x0004C8A5
tool: Capstone 5.0.7 16-bit linear disassembly and xxhash 4.0.1
environment: null
---

## Observation

In the installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`), descriptor 198's first trampoline at
`0x0004C8A0` is `CD 3F 49 01 00`: target word `0x0149` (FND-EXE-225).
Descriptor 198's code block starts at `0x00087310` and is 1,468 bytes long
(FND-EXE-520). Offsets below are block offsets, which FND-EXE-520 shows are
CS offsets while the overlay runs.

A linear 16-bit decoding of block offsets `0x0149..0x022C` (file
`0x00087459..0x0008753C`) has 90 instructions and ends with `retf` at
`0x022B`. The procedure saves BP, reads two words of the caller's frame,
`[bp+6]` into CX and `[bp+8]` into AX, sets DX to `0xFFFF`, and returns DX
in AX with a plain `retf` (no argument bytes removed). It makes no call and
no interrupt. Besides the frame and its own tables, it reads only the word
at DS:`0x420E` (offsets `0x01D7` and `0x01E5`). Every transfer in it is a short jump to
an offset inside `0x0149..0x022C` or one of four table jumps:

| Jump at | Selector test before it | Index | Unsigned bound | Table (CS displacement) | Slots | Table file interval |
|---|---|---|---|---|---:|---|
| `0x0163` | AX = 0 | CX - 1 | 19 (`ja 0x01A2`) | `0x028A` | 20 | `0x0008759A..0x000875C2` |
| `0x01B1` | AX = 1 | CX - 1 | 17 (`ja 0x01C3`) | `0x0266` | 18 | `0x00087576..0x0008759A` |
| `0x01D2` | AX = 2 | CX - 1 | 17 (`ja 0x01F4`) | `0x0242` | 18 | `0x00087552..0x00087576` |
| `0x0205` | AX & 3 nonzero | CX - 10 | 10 (`ja 0x0217`) | `0x022C` | 11 | `0x0008753C..0x00087552` |

Each index is shifted left once before the jump, so a slot is one word.
The four tables are contiguous and fill `0x0008753C..0x000875C2`. All 67
words in them are instruction starts of the decoding above:

| Table | Distinct targets |
|---|---|
| `0x028A` | `0x0168`, `0x016D`, `0x0172`, `0x0177`, `0x017C`, `0x0181`, `0x0186`, `0x018B`, `0x0190`, `0x0195`, `0x019A`, `0x019F`, `0x01A2` |
| `0x0266` | `0x01B6`, `0x01BB`, `0x01C0`, `0x01C3` |
| `0x0242` | `0x01D7`, `0x01E5`, `0x01F1`, `0x01F4` |
| `0x022C` | `0x020A`, `0x020F`, `0x0214`, `0x0217` |

The two-byte `jmp 0x01EC` at `0x01E3` follows an unconditional jump and is
named by no table and no jump, so 225 of the 227 bytes are reachable from
`0x0149`, the same 225 bytes FND-EXE-224 reached under the descriptor-base
binding.

## Interpretation

Each dispatch compares its index unsigned against its own table length and
jumps past the table when the index is larger, so every 16-bit value of the
two frame words selects either a table slot or the skip. The instructions
descriptor 198's dispatch reaches are therefore those of
`0x00087459..0x0008753C` other than `0x000874F3..0x000874F5`, whatever the
callers pass and whatever DS:`0x420E` holds. With FND-EXE-520 placing CS
offset 0 on the block's first byte, the trampoline enters this procedure at
`0x00087459`, and the descriptor-base reading of FND-EXE-173's eleven-slot
table holds for all four tables. This settles Q-EXE-020.

The procedure never leaves `0x0149..0x022C` except by its far return. The
analyzer body of FND-EXE-225 also holds `0x00087F40..0x00088218`, which lies
in descriptor 199's code, and `0x000950CC..0x0009513F`; no transfer of this
procedure reaches either.

## Alternatives

FND-EXE-173's analyzer-alias base is ruled out by FND-EXE-520. Index
producers that pass larger values do not add targets: the unsigned
comparisons send them to the skip offsets `0x01A2`, `0x01C3`, `0x01F4` and
`0x0217`, which are themselves instruction starts listed above. The
selector tests overlap (AX = 1 or 2 also passes `test ax, 3`), so one call
can take two table jumps in turn; both sets of targets are already in the
tables above. A caller that enters the block somewhere other than a
trampoline target, or code that rewrites the block at run time, is outside
this reading, as in FND-EXE-520.

## How to reproduce

Run `python -I tools/research/exec-census/descriptor198_dispatch.py
<install dir>/DSUN.EXE` from the commit that adds this finding, with the
locked evidence Python (Capstone 5.0.7, xxhash 4.0.1). It checks the file's
size and XXH3-128, disassembles block offsets `0x0149..0x022C` of the code
block at `0x00087310`, and prints the four tables at CS displacements
`0x022C` (11 slots), `0x0242` (18), `0x0266` (18) and `0x028A` (20) with
whether each word is an instruction start. Nothing is written or executed.
The trampoline bytes are read separately at file offset `0x0004C8A0`.
