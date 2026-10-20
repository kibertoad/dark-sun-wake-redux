---
id: FND-RNG-054
title: The clock initializer sees the environment block DOS hands the program at startup, copied once and read only by its own TZ lookup; nothing else writes the array root or reads the program's environment pointer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:0059
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:013C..1000:015D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:29AA..1000:2A07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:315A..1000:31B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3B56..1000:3B6F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024..277B:05A1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:076B..15F3:0851
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:01C1..4AE5:0263
tool: tools/research/exec-census/immediate_search.py, direct_callers.py, far_pointer_words.py and resident_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

FND-RNG-019 reads the startup scan of the environment block and the constructor `1000:29AA` that
copies it and builds the near-pointer array rooted at `DS:3920`; FND-RNG-018 reads the lookup
`1000:315A` and the initializer `1000:3B56`. This reading follows what else reaches them.

**Where the block comes from.** At the program's entry `1000:0000` reads the word at offset `0x2C`
through the `DS` the operating system loaded, before it loads its own data segment `57E0`, and
stores it at `DS:008C` (`1000:0010..1000:001D`). The scan and the constructor read the block only
through that segment word. The scan stops at the first double terminator within 32767 bytes and
fails otherwise (`1000:0033..1000:0039`).

**The root and the program's pointer.** `immediate_search.py <dsun> 3920 0088 008A 008C 008E`
decodes every byte of the load image and of every overlay's code. Among instructions whose memory
operand is one of these offsets with no base or index register:

| Offset | Writes | Reads |
|---|---|---|
| `3920` | `1000:29DA` in the constructor | `1000:2A00` in the constructor, `1000:317A` in the lookup |
| `0088` | `1000:2A03` in the constructor; `15F3:0821` in a routine with its own `DS` | `1000:014C`, pushed as the third argument of the program's main routine |
| `008A`, `008C`, `008E` | startup `1000:001D`, `1000:0046`, `1000:0055` | startup `1000:0028`, the constructor `1000:29AC`, `1000:29C0`, `1000:29CD`; the overlay manager `4AE5:01D4` and `4AE5:020C` |

The write of `0088` at `15F3:0821` is in the routine `15F3:076B`, which loads `DS` from `CS`
before its stores (`15F3:0775..15F3:0777`), so it is not the data segment's word. The overlay
manager's two reads load `DS` with `57E0` first and then read the block itself, not the copy:
FND-EXE-568 reads them as the search for the program's own path after the strings and for a
`PATH=` entry. The other hits are inside other instructions, `ES`- or `CS`-prefixed operands
(overlay 183 file `0x6B5D8` and `0x6B63B`, overlay 184 `0x6D6E5` and `0x6D735`, `4842:07E1`), or
`lea` of the manager's own `55CE:008C` buffer. The constructor's write of `3920` and the lookup's
read are the positive controls for writes and reads.

The program's main routine `277B:0024`, called far from `1000:0158` with the words at `DS:0084`,
`0086` and `0088` pushed, reads its argument words at `bp+6` and `bp+8` only; no instruction in
`277B:0024..277B:05A1` reads `bp+0xA`, where the third argument lies.

**The lookup.** `direct_callers.py <dsun> 1000:315A 1000:3B56 1000:29AA` finds one call of the
lookup, the near call at `1000:3B5E` after `push cs` in the initializer, two of the initializer
(`1000:387A` and `1000:3993`, in the clock conversion) and none of the constructor.
`far_pointer_words.py` finds no stored far pointer to the lookup (one overlay-189 fixup word
without a segment), and for the initializer only the startup record FND-RNG-019 reads. The lookup
walks the array from its first member, compares the first byte, then requires `=` at the name's
length and the whole name to match, and returns the byte after the first `=` of the first member
that matches; a zero root, a zero member or an empty member ends the search with 0
(`1000:317A..1000:31B1`). The initializer passes the name at `DS:3980`, `TZ`.

## Interpretation

The only environment string the clock reads is the value of the first `TZ=` entry, in the order
of the environment block the operating system gives the program when it starts; the overlay
manager's reads of `PATH=` and the program path go to its own buffer and not to the clock. The block is
copied once into the near heap at startup, and the array over it is built once; no code writes
the root again or keeps another pointer to the copy that is used, since the main routine ignores
its environment argument and the lookup's only caller is the TZ initializer, which reads the
value (FND-RNG-018). The value lies inside a block of at most 32767 bytes.

Which strings the block holds is decided by the shell that starts the program, not by the game:
the GOG installation's DOSBox configuration starts `DSUN` from its autoexec section without a
`SET` line, and FND-RNG-017 observed no `TZ` entry under the pinned runtime.

## Alternatives

- The census lists encoded displacements only. A write into the copied block or the array through
  a pointer formed another way (a heap overrun from a neighbouring allocation, a computed base)
  is not excluded; nothing found hands out a pointer into them besides the lookup's return value.
- Calls of the lookup through a register or a near pointer table were not searched.
- The contents of a DOSBox or other shell's environment were not read; the game's behaviour with a
  `TZ` entry follows FND-RNG-018's branches.
- No emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`: `immediate_search.py <dsun> 3920 0088 008C 008A 008E`; `direct_callers.py <dsun>
1000:315A 1000:3B56 1000:29AA`; `far_pointer_words.py <dsun> 1000:315A 1000:29AA 1000:3B56`;
`resident_listing.py <dsun> 1000:0000..1000:0060 1000:0130..1000:0160 1000:29AA..1000:2A07
1000:315A..1000:31B9 1000:3B56..1000:3B70 15F3:076B..15F3:0851
277B:0024..277B:05A1 4AE5:0140..4AE5:0290`, and the GOG installation's `dosbox_darksun2_single.conf` read as text.
Outputs are kept under `GAME_DIR/analysis/rng-env-20261020/`. Nothing was executed.
