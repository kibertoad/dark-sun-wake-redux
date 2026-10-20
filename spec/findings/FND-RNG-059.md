---
id: FND-RNG-059
title: On a walk from every established entry only the seed setter and the generator name rng_state, no literal pointer points into it, and the one indexed array below it that the walk writes reaches it only for file handles 67 and 68
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0886..1000:08B6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07B3..1000:07BE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3DD5..1000:3DE1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3EDD..1000:3EE8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F00..1000:3F0B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000507C2..0x000507C4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0005084A..0x0005084E
tool: tools/research/exec-census/window_operands.py, direct_callers.py and resident_listing.py at the commit that adds this finding; scientific_method_engine 19.10.0 reach; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

FND-RNG-001 and FND-RNG-002 locate `rng_state` at `DS:384A..384E`, read and written by the
generator `1000:0822` and written by the seed setter `1000:0811`. FND-RNG-058's walk from the 21
established entries decodes 165,558 instructions and leaves nine computed transfers unresolved.

**Direct and literal operands.** `window_operands.py <dsun> <report> 384A..384E` lists, among the
walk's instructions, the setter's stores at `1000:0817` and `1000:081D` and the generator's two
reads and two stores of the doubleword and its read of the high word at `1000:0840`, each through
`DS` with no base or index register, and nothing else: no other memory operand of any segment
whose displacement is in the window, and no immediate there. The three hits of the every-byte pass
lie inside the setter's and the generator's instructions. The census's positive controls for each
kind are these accesses, located by FND-RNG-001 and FND-RNG-002 with another tool, the indexed read
of the month table at `1000:38E5` and the literal pointer `0x3980` at `1000:3B58` (FND-RNG-058).

**Operands just below and above.** The same run over `3676..384A` and `384E..3920` finds these
writes with a base or index register:

| Write | Index | Bound |
|---|---|---|
| `1000:0893`, word `[BX+0x37C4]` = 0, in the startup routine `1000:0886` | `BX` = 2 × handle | the loop runs the handle from 5 while it is below the word at `DS:37C2` (`1000:0886..1000:08B6`) |
| `1000:089F` and `1000:08AF`, byte `[BX+0x3686]` and word `[BX+0x3690]`, in the same loop | `BX` = 16 × handle | the same, so the last bytes written are at `DS:37B1` |
| `1000:3E38`, `and word [BX+0x37C4], 0xFDFF`, in `1000:3DCC` | `BX` = 2 × its first argument | the routine returns at `1000:3DDF` unless the argument is below `DS:37C2` (`1000:3DD5..1000:3DE1`) |
| `1000:07B8`, `and word [BX+0x37C4], 0xFDFF`, in `1000:07B0` | `BX` = 2 × its first argument (`1000:07B3..1000:07BE`) | none in the routine |
| `1000:3F05`, `or word [BX+0x37C4], 0x1000`, in `1000:3EDA` | `BX` = 2 × its first argument (`1000:3EDD..1000:3EE8`, `1000:3F00..1000:3F0B`) | none in the routine; it runs after interrupt 21 service 0x40 succeeds with that argument as the handle |

The word at `DS:37C2` holds 20 in the shipped file (`0x507C2..0x507C4`), and the walk has no
instruction that writes it. `direct_callers.py <dsun> 1000:07B0 1000:3EDA 1000:3DCC` finds eight
near calls of `1000:07B0` and seven of `1000:3EDA`, each after `push cs`, at `1000:3078` to
`1000:3EB7`; four of them, at `1000:3E12`, `3E2B`, `3E7A` and `3EB7`, are in `1000:3DCC` after its
check and pass its `DI`, which it does not write after loading it. The other writes the run lists in the two
windows have no base or index register. The shipped doubleword `rng_state` holds 1
(`0x5084A..0x5084E`).

## Interpretation

Within the code the walk reaches, `rng_state` changes only in the seed setter and the generator;
no instruction names it, through `DS` or another segment register, and no literal forms a pointer
into it. The handle flag words at `DS:37C4` run up to `DS:37EC` for the 20 handles; a word for
handle 67 or 68 would be `rng_state`'s low or high word. The startup loop and `1000:3DCC` keep the
handle below 20; `1000:07B0` and `1000:3EDA` write the word for whatever handle they are given,
which is the handle the operating system accepted for a seek or a write.

Before the startup seed, `rng_state` is 1 from the load image.

## Alternatives

- The callers of `1000:07B0` and `1000:3EDA` were not followed to where their handles come from; a
  handle of 67 or 68 needs a process handle table larger than the default one.
- Writes through the literal pointers into the stream records at `DS:3682..37C2` (`0x3682`,
  `0x3692`, `0x36AE` to `0x36B5`) and through pointers formed outside the windows (heap or far
  pointers, string stores and copy helpers with register destinations) were not traced; `0x3850`
  at `1000:0CB8` is a string source. A different segment and offset pair naming the same bytes
  would not show.
- Code behind the walk's nine unresolved transfers was not read.
- No emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE` and `<report>` FND-RNG-058's walk: `window_operands.py <dsun> <report> 384A..384E` and
`window_operands.py <dsun> <report> 3676..384A 384E..3920`; `direct_callers.py <dsun> 1000:07B0
1000:3EDA 1000:3DCC`; `resident_listing.py <dsun> 1000:07B0..1000:07C4 1000:3DCC..1000:3E40
1000:3EDA..1000:3F10 1000:0886..1000:08B6 1000:0CA8..1000:0CD8`; and the words at file offsets
`0x507C2` and `0x5084A`. Outputs are kept under `GAME_DIR/analysis/rng-storage-20261020/`.
Nothing was executed.
