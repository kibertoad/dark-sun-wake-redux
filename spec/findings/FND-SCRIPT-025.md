---
id: FND-SCRIPT-025
title: Script instructions 0x01 to 0x05, 0x07, 0x11 and 0x76 to 0x7F change the byte, word or double word at their first parameter's address in place
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0A69..172C:0B13
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0D67..172C:0D93
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1DA7..172C:1F3B
tool: tools/research/exec-census/resident_listing.py at commit 0cd7fdf9; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Addresses in segment `4C13`. Each handler calls `172C:367B(n)` (FND-SCRIPT-009), which leaves
parameter i's value at `00B4 + 4 * i` and the far address of the variable it named at
`00D4 + 4 * i`. The handler loads the far pointer at `00D4` with `les bx`, reads the
memory there, combines it with parameter 1 at `00B8` where it has one, and writes the result
back through the same pointer, restoring `ES` from `00D6` before the store:

| Opcode | Handler | n | Read at the address | Operation | Written |
|---|---|---|---|---|---|
| `0x01` | `0A69` | 2 | double word | `cdq`, `idiv dword [00B8]` | double word, the quotient |
| `0x02` | `0A99` | 1 | byte | `dec byte` | byte |
| `0x03` | `0AAD` | 1 | word | `dec word` | word |
| `0x04` | `0AC1` | 1 | double word | `dec dword` | double word |
| `0x05` | `0AD6` | 1 | byte | `inc byte` | byte |
| `0x07` | `0AFE` | 1 | double word | `inc dword` | double word |
| `0x11` | `0D67` | 2 | double word | `imul eax, dword [00B8]` | double word |
| `0x76` | `1DA7` | 2 | byte | `add al, byte [00B8]` | byte |
| `0x77` | `1DCF` | 2 | byte | `sub al, byte [00B8]` | byte |
| `0x78` | `1DF7` | 2 | byte, `movzx` to 32 bits | `imul eax, dword [00B8]` | low byte |
| `0x79` | `1E23` | 2 | byte, `movzx` to 32 bits | `cdq`, `idiv dword [00B8]` | low byte of the quotient |
| `0x7A` | `1E53` | 2 | word | `add ax, word [00B8]` | word |
| `0x7B` | `1E7B` | 2 | word | `sub ax, word [00B8]` | word |
| `0x7C` | `1EA3` | 2 | word, `movzx` to 32 bits | `imul eax, dword [00B8]` | low word |
| `0x7D` | `1ECF` | 2 | word, `movzx` to 32 bits | `cdq`, `idiv dword [00B8]` | low word of the quotient |
| `0x7E` | `1EFF` | 2 | double word | `add dword [bx], eax` with eax from `00B8` | double word |
| `0x7F` | `1F1D` | 2 | double word | `sub dword [bx], eax` with eax from `00B8` | double word |

No handler tests the divisor, writes the byte `4C0E:0000` that `172C:24A0` sets after a write
to a local variable (FND-SCRIPT-009), or touches the accumulator `031B` other than through the
address. Each ends with `ret` after its store.

In the shipped scripts decoded with FND-SCRIPT-024's layouts, `0x03` begins 91 instructions,
`0x7A` 90, `0x7C` 32, `0x7B` 21, `0x7D` 20, `0x7E` 17, `0x7F` 3 and `0x07` and `0x11` one
each; `0x01`, `0x02`, `0x04`, `0x05` and `0x76` to `0x79` begin none, apart from the `0x77` at
`GPL/37` `0x0D37` whose operand the reader rejects.

## Interpretation

These are in-place arithmetic on a variable: decrement and increment of a byte, word or double
word, double-word division and multiplication, and add, subtract, multiply and divide on a byte
and a word. The opcode, not the variable's kind, chooses the width, so a word operation on a
flag variable changes the 16 flags of two bytes, and one on a literal parameter changes the
accumulator, whose address the expression reader leaves for a literal (RULE-SCRIPT-004).
Division is signed with truncation, and a zero divisor, or a double-word quotient that does not
fit, raises the processor's divide error. A local variable changed this way does not set the
locals-changed byte.

## Alternatives

- Reading the byte and word multiplications and divisions as signed fails: the handlers
  zero-extend the stored value before the 32-bit `imul` or `idiv`.

## How to reproduce

Use the installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`) and
the locked evidence Python. In `tools/research/exec-census/`, run `resident_listing.py` for
`172C:0A69..172C:0B13`, `172C:0D67..172C:0D93` and `172C:1DA7..172C:1F3B`, and the opcode
counts with `script_decode.py` over `GPLDATA.GFF` and `OBJEX.GFF` as FND-SCRIPT-024 describes.
The listing is retained as `handlers.txt` under
`GAME_DIR/analysis/script-instructions-20261018/`. No original function or game process was
run.
