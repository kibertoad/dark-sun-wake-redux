---
id: FND-PARTY-045
title: The Load Saved Game window reaches no count, DS:0DAB or pointer writer unless a game is loaded, and the Create Characters window passes its events to overlay 190 routines that do
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D300..0x0007D32A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D6AC..0x0007D6FB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DD38..0x0007E048
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00098B67..0x00098C36
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C1F8..0x0008C21A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0D83..3D72:0DE5
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, immediate_search.py, store_values.py, direct_callers.py, trampoline_target.py); scientific-method-engine 15.0.0 `reach` (tools/research/exec-census/reach_config.py with the queries named below)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`.

**Load Saved Game (button `0x4B66`).** The start window's branch calls overlay 192 `+0000`
(FND-PARTY-044). `+0000` stores 1 to the byte at `50AF:0004` (`+0008`) and calls `+024B`, which
opens window `0x4844` with handler `+0785` and sets its `+0xFD` to `+022B` (FND-PARTY-044).
`+0015` stores 0 there and calls `+024B`; it runs only when the bytes at `DS:13FB` and `DS:0690`
are both nonzero (FND-PARTY-026). A scan of every byte of the file for a load of the segment
word `0x03A8` (descriptor 117) or `0x40AF` (the resident segment `50AF` before relocation) into
a register, followed within five instructions by a write to `ES:[4]`, finds only those two
stores.

`+0785` takes event 2 at `+0A38`: it looks the button number up among the 13 words at `+0D14`
(`mov cx, 0xD` at `+0A3E`) and jumps through the 13 words after them (`+0A54`):

| Button | Branch |
| --- | --- |
| `0x477D`, `0x47E0` | `+0B21`, confirm |
| `0x477E` | `+0C7C`, cancel |
| `0x4780` to `0x4789` | `+0A58`, which stores the button number less `0x4780` to the word at `4C4C:0000` and, while `50AF:0004` is nonzero, calls `+0DF4` |

The confirm branch calls `+03AC` (`+0BC4`), then `+05ED` while `50AF:0004` is nonzero
(`+0BDF`) and `+03FB` otherwise (`+0BEF`). When the byte at `DS:627C` is nonzero it then stores
0 to `DS:1462`, which ends the start loop (FND-PARTY-026), and returns -1 (`+0C0F..+0C1E`).
Otherwise it calls overlay 182 `+19F8` (`+0C21`) and `28C9:2522` (`+0C26`); when that returns
anything other than 1 it calls the gate routine, overlay 182 `+1167` (trampoline `56BD:00AC`),
with 0 (`+0C32`), and when it returns 1 it calls overlay 204 `+284D` while `50AF:0004` is
nonzero (`+0C47`) and `2C5F:018C` (`+0C58`). Both paths then call overlay 204 `+1D2E` with 5
(`+0C62`), store 1 to `DS:143F`, call `2C5F:0139` (`+0C6D`) and `28C9:2A81`, and return 0
through `+0ACE`.

The cancel branch calls overlay 186 `+00B8` with `0x0F` (`+0CA1`), `+03AC` (`+0CCB`), overlay
182 `+19F8` (`+0CEA`), `3D72:0D83` (`+0CEF`) and `28C9:2522` (`+0CF4`). When that returns
anything other than 1 it calls overlay 194 `+037B` (trampoline `574B:0020`) with 0 (`+0D00`),
which builds window 19500 again with handler overlay 194 `+0000` (FND-PARTY-026), and returns
0; when it returns 1 it returns 0.

`+03AC` closes the window held at `50AF:0000` with overlay 182 `+0128` when that double word is
nonzero, stores 0 to it, and calls overlay 190 `+319E` and `3D72:0D83`. Then, only when the byte
at `DS:0690` is nonzero, it stores 1 to `DS:13FB` and 0 to `DS:0690` (`+03E8..+03F4`).

`immediate_search.py` finds four stores to `DS:0690`: 0 at `27A7:0009`, which runs before the
start window (FND-PARTY-026), 0 at overlay 192 `+03F4`, and 1 at overlay 204 `+044A` and 0 at
`+0452`. The overlay 204 pair is one branch of a dispatch, which stores 1 to `DS:13FB` (`+0438`)
and the byte of its first argument to `DS:4458`, then 1 to `DS:0690` when its second argument's
double word is 1 and 0 otherwise.

`3D72:0D83` calls through the far pointer at `DS:A119` at `0x336C1` and `0x336F0` only while it
is nonzero; FND-PARTY-039 shows it stays 0.

**Create Characters (button `0x4B65`).** The start window's branch opens window `0x2CEC`
(`RESOURCE.GFF#WIND/11500`, SCR-UI-002) with handler overlay 212 `+0967` (FND-PARTY-044).
`+0967` handles three events:

- event 1: calls `+08E3`, stores 0 to `DS:1462` and returns -1 (`+0A14..+0A1D`);
- event 2: looks the button up among the 5 words at `+0A22` and jumps through the 5 after
  them. Button `0x283C` (`BUTN/10300`) goes to `+0A0A`, which calls `2C5F:0182` and returns.
  Buttons `0x2C24` to `0x2C27` (`BUTN/11300` to `/11303`, the character boxes) go to `+09D6`,
  which calls overlay 190 `+0D89` (trampoline `571F:003E`, `+09E9`). Any other button calls
  overlay 190 `+0802` (trampoline `571F:0043`, `+0A03`);
- event 6, a key: unless the key's low byte, with `0x20` set, is `0x76`, calls overlay 190
  `+139B` (trampoline `571F:0020`, `+09AB`).

Each overlay 190 call receives the 24-byte event record, copied by `1000:0699`, and the far
pointer `57C9:002A`, which is overlay 212 `+08E3`.

**The runs.** Each configuration is built by `reach_config.py` as in FND-PARTY-040, with the
instruction limit at 100,000. Every run has the 22 fixed pointer values of FND-PARTY-044 run 6
as starts besides the one named, the 149 targets of `q011_window_handlers_reach.json` (the 29
of FND-PARTY-040 and the 120 setter call sites of FND-PARTY-044), no control, and as leaves
overlay 182 `+1856` and `+19F8` (FND-PARTY-029) and the gate routine `+1167`.

1. `q017_load_entry_reach.json`, start overlay 192 `+0000`. 354 routines and 23,645
   instructions. It reaches overlay 182 `+01AA` and `+01BC`, the setter calls inside the window
   opener, and overlay 192 `+02B2` and `+02E7`, which set window `0x4844`'s fields. No other
   target.
2. `q017_load_close_reach.json`, start overlay 192 `+022B`. 158 routines and 9,927
   instructions; no target.
3. `q017_load_window_reach.json`, start overlay 192 `+0785`, with six more leaves: `+05ED`;
   `+03FB` (called only while `50AF:0004` is 0); and `2C5F:0139`, `2C5F:018C` and overlay 204
   `+284D` and `+1D2E`, which `+0785` calls only on the confirm branch after `+05ED`. 371
   routines and 24,777 instructions. It reaches the gate routine, as a leaf, through the
   confirm branch's call at `+0C32`; overlay 192 `+03AC` through the cancel branch's call at
   `+0CCB`; and overlay 182 `+01AA` and `+01BC` and overlay 194 `+049E` through `+037B`, called
   at `+0D00`. The only overlay 204 routines it reads are the two leaves.
4. `q017_create_characters_reach.json`, start overlay 212 `+0967`. 1,222 routines and 99,085
   instructions, under the limit. It reaches 70 of the 149 targets. Of the 29 it reaches the
   count stores `0x271E3` and `0x27404`, the `DS:13FB` setter overlay 173 `+3CDA`, and the
   FND-PARTY-036 sites `2C5F:0CBF`, `2C5F:0CF1`, `0x777F7`, `0x9106C` and `0x910C6`. Every
   fewest-call chain to them runs through one of the overlay 190 calls at `+09AB`, `+09E9` or
   `+0A03`.

Every unresolved transfer in runs 1 to 3 is a call FND-PARTY-039 or FND-PARTY-044 (run 2)
accounts for, or one of the four record-field calls of FND-PARTY-044 that run 2 there leaves
unresolved; for window `0x4844` those fields hold `+0785` and `+022B`, which are starts. All
runs report the five gaps and four contested bytes at `1000:02E9..02F8` that FND-PARTY-044
describes, so none is usable as a negative without that reading.

## Interpretation

Under the reports' assumptions, a player who opens Load Saved Game and leaves it without
confirming a load (cancel, or any slot choice followed by cancel) returns to a start window
built by the same routine as the first one, and no routine on the way reaches a placed-object
count store, a routine that can make `DS:0DAB` nonzero, a `DS:13FB` setter that runs, or a
pointer-image site. `+03AC`'s `DS:13FB` store is skipped, since `DS:0690` is 0 from
`27A7:0009` and the one store of 1, in overlay 204, is not reached on those paths.

On the start window, `50AF:0004` is 1, so a confirm always calls the load routine `+05ED`. After
it the handler either ends the start loop or calls the gate routine itself, then overlay 204 and
`2C5F` routines, and does not rebuild the start window. That path begins play from the loaded
game, not from START GAME; whether START GAME can still be chosen afterwards, and with what
state, is not read here.

Create Characters is the character screen. Its handler sends every key, every button other than
`0x283C` and every character-box click to overlay 190 routines from which routes reach two of
the count's stores, a `DS:13FB` setter and five pointer-image sites. Which player actions on that
screen take those routes, and whether the start loop then reaches START GAME with the state they
leave, is not read here.

## Alternatives

- `50AF:0004` is written through a pointer or a segment the scan does not name: the scan covers
  stores after an immediate segment load of descriptor 117's word; a store through a far pointer
  held in memory is not covered.
- The cancel branch's rebuilt start window differs from the first in a way the gate reads: both
  are built by overlay 194 `+037B`, here with 0 and at start-up with 1; what that argument
  selects is not read, and the run reaches no target through `+037B` beyond the window set-up
  calls.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 192
0x7D300 0x7D32A`, `192 0x7D6AC 0x7D6FB`, `192 0x7DD38 0x7E048`, `212 0x98B67 0x98C36` and `204
0x8C1A0 0x8C240`; `resident_listing.py <dsun> 3D72:0D83..3D72:0DF0`; `immediate_search.py <dsun>
0690`; `trampoline_target.py <dsun> 571F:0020 571F:003E 571F:0043 57C9:002A 56BD:00AC 574B:0020
56E9:0039`. Read the two jump tables of `+0785` as the 13 words at file `0x7E014` and the 13 at
`0x7E02E`. Then, in the repository root, for each query named in runs 1 to 4 run
`reach_config.py` and `python -I -m scientific_method_engine reach` as FND-PARTY-040 gives.
