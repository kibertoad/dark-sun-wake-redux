---
id: FND-RNG-072
title: Besides overlay 173 +059A, the only code that takes the address of DS:426D is 31E0:2C86, which passes it to 31E0:2D6E when a placed slot equals DS:426D, and 31E0:2D6E stores there what 2D40:10AE returns for that slot
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2C86..31E0:2DBD
tool: tools/research/exec-census/immediate_search.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Addresses are static. `immediate_search.py <dsun> 426D` reports, besides the direct operands
`[0x426d]` and decodings inside other instructions, two pushes of `0x426D`: at `2ADD:0006`, the
same byte as `28C9:2146`, which passes the address to overlay 173 `+059A` (FND-RNG-070), and at
`34B5:0000`, the same byte as `31E0:2D50`.

`31E0:2C86(n)` loops `SI` from 0 below `n`. For each, it takes the word at `52A2:0030 + 10 × SI`
as a record number, record `q` at `DS:67BB + 37 × q`, and reads the slot `s` at offset 6 of the
8-byte entry, numbered by record `q`'s word `+0x01`, of the far table at `DS:67B7`. It goes on only
when record `q`'s words `+0x0A` and `+0x0C` plus 8 are multiples of 16 and bit `s` of the bitmap at
`52A2:0000` is set; it clears that bit. When the byte at `4F49:0C33 + 3 × q` is 2, it calls
`31E0:2D6E` with the far address `DS:426D` when `s` equals `DS:426D`, and with the address of its
own copy of `s` otherwise.

`31E0:2D6E(p)` stores 1 in `4E71:0000`, calls overlay 193 `+0DC7` (trampoline `573B:005C`) with
the slot at `p` and 0, calls the overlay 197 trampoline `575A:002F` with it when the combat word
`4C10:0019` is not 0, and calls `2D40:10AE` with it and 0, storing the result at `p`.

## Interpretation

The only stores to `DS:426D` through its address are overlay 173 `+059A`'s, which can be 9999, and
this one, which stores what `2D40:10AE` returns for the slot `DS:426D` already holds. With
`DS:426D` at 9999 it would pass 9999 to `2D40:10AE`, and so to `2D40:2F34`, only if a placed
record's table entry names slot 9999 and bit 9999 of `52A2:0000` is set; step 5 of FND-RNG-068
copies bit `r` into bit 9999.

## Alternatives

What `2D40:10AE` returns, the table at `52A2:0030` and its count `n`, and the far table at
`DS:67B7` were not read here.

## How to reproduce

`immediate_search.py <dsun> 426D`; disassemble `31E0:2C86` to `31E0:2DBD` from the load image.
