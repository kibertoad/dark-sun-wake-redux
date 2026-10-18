---
id: FND-PARTY-241
title: Global script number 129 is cleared before the first script runs and changes after that only by block clears and saved-game loads, so GPL/255 0x0327 never takes function 46's 9999 branch in a game the program saved itself
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057580..0x00057B94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070B4A..0x00070BE0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:06E6..2D40:073E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:035D..277B:038C
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, immediate_search.py and script_decode.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`). Global script numbers
are the `INT16` values behind the far pointer `4C13:0347` (RULE-SCRIPT-004, kind 7); number 129
is the word at offset `0x102`.

**Where the block comes from.** Overlay 169 `+0000`, far-called from overlay 188 `+0481`, gets
four blocks from its allocator `+0552` (a far call to `444C:00FA` that sets the byte `4C0C:0000`
when it returns 0) and stores the second at `4C13:0347` (`+00BF..+00D0`). After the four are
allocated and the failure byte tested, it calls `+059B`, which calls `1000:3FA2` four times with
a block, the fill byte 0 and a count: `0x190` bytes for the block at `4C13:0347`, so all 200 words
become 0. Then it calls `172C:000C` with 99 (`+0129..+0142`). The
release routine `+0174` frees the block and sets the pointer to 0. The main routine `277B` clears
the same block with the same call and count at `277B:0376..277B:0387`.

**Who reads or writes the pointer.** The search for the displacements `0x347` and `0x349` finds,
apart from overlay 169's stores, pushes and clear and four instructions that only use the same
number (two near jumps, an `add` and an `adc`), only:

- the script variable code in `172C`: the kind-7 read at `172C:1FBF`, the address of a kind-7
  variable left at `4C13:024D` at `172C:2287..172C:22B3`, and the kind-7 store of the setter at
  `172C:24A1` (`172C:24FD..172C:2507`), all indexed by a variable number a script operand gives;
- the clear in `277B` above;
- overlay 187 `+0D1A` (file `0x70B90`), which copies the pointer into the save table at
  `DS:15F6`, entry 30 of the far pointers from `DS:157E`. The sizes table at `4F38:0000`
  gives entry 30 `0x190` bytes. The save loop at overlay 187 `+0934` writes each entry as one
  chunk, and the load at `+0AA2` clears each entry's block with `1000:3FA2` and reads the
  chunk back into it. No instruction addresses `DS:15F6` other than that copy.

**Scripts.** In the decode of FND-SCRIPT-024 every kind-7 operand has a literal number, and
number 129 occurs once, read through opcode `0x1F` at `GPL/255` `0x032C` (FND-PARTY-237).

**The kind-9 variables 0x21 and 0x22.** The 15 far pointers of `global_name_pointers` at
`4C13:0000` are in the file at `0x41330` with segment words `0x3C10` and `0x3C13`: entries
`0x20` to `0x24` point at the words `+0x19`, `+0x1B`, `+0x1D`, `+0x1F` and `+0x21` of segment
`4C10`, entries `0x25` to `0x2C` at words between `+0x357` and `+0x369` of `4C13`, and `0x2D`
and `0x2E` are 0 in the file. The displacement search finds word stores to `4C10:001B` and
`4C10:001D` only in `2D40:06E6`: it looks up the object its argument names with `2D40:0D94` and, unless that
returns 9999, stores the object's words at `DS:67C5` and `DS:67C7` (37-byte records), each
shifted right by 4, and clears `4C10:001F`. Opcode `0x09`'s handler `172C:12D2` reads one
expression and far-calls `2D40:06E6` with it. No shipped script assigns a kind-9 variable.
`GPL/255` runs `0x09` on `var9[40]` at `0x0306`, copies `var9[34]` and `var9[33]` into `var2[0]`
and `var2[1]`, runs `0x09` on `var9[44]`, and then passes `var2[1] * 1000 + var2[0]` as the fourth
parameter at `0x0327`.

## Interpretation

The pointer at `4C13:0347` is set only by overlay 169 `+0000`, which clears the block before any
script can read it, so global number 129 starts at 0. After that, no code writes the word by
itself: the block is cleared whole, or replaced whole by a saved game's chunk, and the only
script access to number 129 is a read. A saved game the program wrote holds the block as the
scripts left it, so number 129 is 0 there too. `0x1F` then gives 0, which is not 9999, and
function 46 at `GPL/255` `0x0327` does not call overlay 204 `+18CD`. The kind-9 variables `0x21`
and `0x22` are an object's two position words divided by 16, so the fourth parameter would be
the first word of `var9[40]`'s object times 1000 plus its second, and the spell its second word
divided by 16; that cast does not happen while number 129 is 0.

## Alternatives

- A saved game changed outside the program could hold any value at offset `0x102` of the save table's entry 30;
  that is not play.
- Writes through a computed address that is not a displacement from `4C13:0347`, such as a block
  copy whose destination is read from another pointer, were not searched for; the save table is
  the only copy of the pointer found.
- Instructions whose handlers write through the address at `4C13:024D` (RULE-SCRIPT-011) write
  only variables a script names, and no script names number 129 outside the read.
- Where overlay 204 `+18CD` lands a cast with -1 as the caster was not read, since the branch
  that makes one here does not run.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 169 57580 576F4` and `169 57AB6 57B94`;
`direct_callers.py <dsun> 169+059B 169+0552 169+0000 169+0174 187+0D1A`;
`immediate_search.py <dsun> 347 349`, `15f6 158e 158a` and `1b 1d 19 1f 21`;
`resident_listing.py <dsun> 277B:0340..277B:03A0 172C:1FA0..172C:1FD0 172C:2270..172C:22C0
172C:24A1..172C:2510 172C:12D2..172C:12DC 2D40:06E6..2D40:0740 1000:3FA2..1000:3FC1`;
`overlay_listing.py <dsun> 187 70B40 70BE0`, `187 70740 70800` and `187 708C0 70960`; and read
the 15 doublewords at file `0x41330` and the sizes at file `0x44580`. Search the
`script_decode.py GPLDATA.GFF OBJEX.GFF --all` listing for `var7[` and `-> var9[`. The listings
are retained under `GAME_DIR/analysis/party-global129-20261018/`. No original function or game
process was run.
