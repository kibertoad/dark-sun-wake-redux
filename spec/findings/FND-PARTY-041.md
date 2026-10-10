---
id: FND-PARTY-041
title: MAS 99 runs straight through string and number assignments and opcodes 0x6D and 0x70 to its stop, and none of their handlers reaches the count, DS:0DAB or pointer writers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x001FEAA5..0x001FEC40
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1CF7..172C:1D24
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1D7A..172C:1DA7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:37F3..2D40:382F
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/script_listing.py, store_values.py, resident_listing.py, trampoline_target.py); scientific-method-engine 15.0.0 `reach` (tools/research/exec-census/reach_config.py with q015_handlers_leaves.json, q015_hook_reach.json, q015_mas99_reach.json and q015_mas99_round.json)
environment: null
---

## Observation

**The script.** `MAS` 99 in the installed `GPLDATA.GFF` is 411 bytes at file offset `0x1FEAA5`.
Read as FMT-SCRIPT-001 from offset 0, with each parameter an expression as RULE-SCRIPT-004's
`read_number` reads it, it is:

| Offset | Opcode | Parameters |
| --- | --- | --- |
| `0x000` to `0x07A` | `0x0A`, 7 times | global string 4, 5, 7, 6, 1, 2 and 3 (variable kind 6), each with one kind-5 string |
| `0x085` | `0x16` | 9999, assigned to global number 29 (variable code 7) |
| `0x08A` | `0x16` | 1, assigned to global number 22 |
| `0x08F` to `0x0EF` | `0x6D`, 13 times | three literal numbers each |
| `0x0F7` | `0x70` | four literal numbers |
| `0x102` to `0x192` | `0x6D`, 19 times | three literal numbers each |
| `0x19A` | `0x31` | none |

The byte at `0x19A` is the resource's last byte; the loader's added `0x31` follows it. Every
parameter is a literal (bytes below `0x80`, or `0x91` with a word), a kind-5 string (`0x92 05`),
or a variable of kind 6 (`0x86`). No instruction is a jump, call, `if`, compare or return, and no
expression holds a nested instruction (`0x8C`), `0xB1`, a variable of kind 9, parentheses or an
operator. The two `0x16` instructions write with code 7, below `0x10`.

**The two opcodes.** `0x6D`'s handler `172C:1CF7` and `0x70`'s handler `172C:1D7A` (FND-SCRIPT-005)
begin with `push 3` and `push 4`, `call 0x367B`, and then call `172C:2B29` and `172C:2EA1`, or
`172C:2B89` and `172C:2DF8`, with constant segment and offset arguments. `172C:367B` reads that
many parameters (FND-SCRIPT-009).

**The handlers.** A `reach` run from the five handlers of the opcodes `MAS` 99 holds
(`172C:0B2E`, `172C:0DCD`, `172C:1CF7`, `172C:1D7A`, `172C:1127`), with these leaves:
`172C:367B` and `172C:3278` (parameter and expression reading, FND-SCRIPT-009, FND-SCRIPT-010), and
`172C:20B3` (the fetch advance, which calls `5702:00B1` when the new offset is not below the
buffer size at `4C13:0313`, FND-SCRIPT-006). Its targets are the 29 of FND-PARTY-040 plus the
error routine overlay 188 `+1901` (`0x000747A1`, trampoline `5702:00B1`) and overlay 199 `+0C21`
(`0x00088521`), which stores `28C9:0CFF` at `DS:A0F1` (FND-PARTY-040). It reaches 38 routines and
1,126 instructions, with no unresolved transfer, interrupt, gap or contested instruction, and no
target. Without the leaves, the same starts reach the error routine through `172C:3278` and
through `172C:2C1A`, `172C:2805` and `172C:20B3`.

**The buffer size.** The only stores to `4C13:0313` are at overlay 169 `+0124`, which stores its
second argument. The only direct call of overlay 169 `+0000`, at overlay 188 `+0481`,
passes `0x2710` (10,000).

**The hook.** When `DS:02F6` is not 0, `172C:018F` calls it with each opcode first
(FND-SCRIPT-005); its only nonzero value is `2D40:37F3` (FND-PARTY-039). That routine stores the
opcode at `DS:55C4`. When the opcode is `0x31` and the byte at `DS:143D` is not 0, it calls
trampoline `576C:0039`, overlay 199 `+0C21`. It then stores 1 or 0 in the byte at `4F49:000A` for
opcode `0x33` or any other, and returns. A `reach` run from `2D40:37F3` alone reaches 182 routines
and none of the 29 targets.

**`DS:143D`.** Its start value is 0. The stores to it are at overlay 189 `+0433` (0), overlay 190
`+3966` (1), and overlay 199 `+0581`, `+06AB`, `+07F9` and `+091F` (AL), `+0910` and `+0BE3` (0).
A `reach` run from the 74 starts of FND-PARTY-040's second round 0, the five handlers and
`2D40:37F3` (`q015_mas99_round.json`, 820 routines, 48,414 instructions, no stop at the limit)
reaches none of the stores of 1 or AL and none of the 29 targets.

## Interpretation

Since `MAS` 99 holds no jump, call or nested instruction, the interpreter runs its instructions
in the listed order, from offset 0 to the `0x31` at `0x19A`. Its expressions follow the
non-error branches of `172C:3278`, and its offsets stay below 411, far below the buffer size of
10,000, so its fetches do not call `5702:00B1`. The handlers of the five opcodes it runs reach no
routine that changes the placed-object count, sets `DS:0DAB` or makes the pointer an image other
than an `ICON`, and none reaches the routine that stores `28C9:0CFF`. The hook reaches that
routine only for opcode `0x31` while `DS:143D` is nonzero, and no route before the gate makes it
nonzero. The run of `MAS` 99 therefore opens no such route, unless its load fails and
`172C:0299` calls `5702:00B1` (FND-CONFIG-160).

## Alternatives

- The listing misreads the script: each instruction ends where the next opcode the interpreter
  dispatches begins, and the listing ends exactly at the resource's last byte, `0x31`.
- An opcode's handler changes something a later step reads: `0x6D` and `0x70` store their
  parameters through `172C:2B29`, `172C:2EA1`, `172C:2B89` and `172C:2DF8`; what those tables
  drive later was not read, and is outside the routes searched.
- The offset `172C:20B3` compares is the frame's offset in the buffer, not in the script: the
  loader places the script with its added stop byte inside the buffer (FND-SCRIPT-022), so a
  fetch inside the script stays below the buffer size either way.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<gff>` the installed
`GPLDATA.GFF` and `<dsun>` the installed `DSUN.EXE`, run in `tools/research/exec-census/`:
`script_listing.py <gff> "MAS " 99`; `resident_listing.py <dsun> 172C:1CF7..172C:1D24
172C:1D7A..172C:1DA7 2D40:37F3..2D40:3880`; `trampoline_target.py <dsun> 576C:0039`;
`store_values.py <dsun> --indexed 143D` and `store_values.py <dsun> 0313 0315 02F6 02F8 55C4`.
Then, in the repository root, for each query `q015_handlers_leaves.json`, `q015_hook_reach.json`,
`q015_mas99_reach.json` (the handlers and hook without leaves) and `q015_mas99_round.json`, run
`reach_config.py` and `python -I -m scientific_method_engine reach` as FND-PARTY-040 gives.
