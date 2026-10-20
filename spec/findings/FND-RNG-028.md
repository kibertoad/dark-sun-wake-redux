---
id: FND-RNG-028
title: The direct-call census of the generator, the seed setter and the four reducers finds 117 sites, and recorded findings describe the calls at 52 of them
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0811
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0822
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:0519
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:061C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:391D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3A03
tool: tools/research/exec-census/direct_callers.py at commit c40653e3; Python 3.14.7, xxhash 4.0.1
environment: null
---

## Observation

One run of the direct-call query over the installed `DSUN.EXE` takes six targets: the seed
setter `1000:0811` (FND-RNG-002), the generator `1000:0822` (FND-RNG-001), `chance_in_ten`
`2834:0519` (FND-RNG-006), `random_mod` `2834:061C` (FND-RNG-003), `roll_sum` `28C9:391D`
(FND-RNG-005) and `random_between` `2D40:3A03` (FND-RNG-004). It searches three kinds of site:
far calls in the load image whose segment word carries an MZ relocation, far calls in FBOV
overlay code whose segment word is one of that overlay's fixups and names descriptor 0, and near
calls in the target's own load-image segment, with or without a preceding `push cs`.

| Target | Load-image far | Overlay far | Near | Total |
|---|---|---|---|---|
| `1000:0811` seed setter | 0 | 2 | 0 | 2 |
| `1000:0822` generator | 4 | 25 | 0 | 29 |
| `2834:0519` `chance_in_ten` | 0 | 0 | 4 | 4 |
| `2834:061C` `random_mod` | 0 | 0 | 11 | 11 |
| `28C9:391D` `roll_sum` | 0 | 60 | 0 | 60 |
| `2D40:3A03` `random_between` | 0 | 11 | 0 | 11 |

Positive controls, each found before this query by other means, are returned for each kind of
site: the four load-image far calls of the generator are the three reducers' calls that
FND-RNG-001 found through Ghidra's references and script instruction `0x52`'s call
(FND-SCRIPT-011); overlay 204 `+001E`'s fixup-backed far call of the seed setter is FND-RNG-009's;
and the eleven near calls of `random_mod` after `push cs` are the two in `2834:000C` and nine in
`2834:0519` that FND-RNG-003 counted.

### Sites whose draw a recorded finding describes

Overlay sites are written `descriptor +offset in the overlay's code`.

| Target | Site | Described by |
|---|---|---|
| generator | `172C:170B` handler, load-image file `0x0000DBCB` | FND-SCRIPT-011 |
| generator | inside `random_mod`, `roll_sum` and `random_between` | FND-RNG-003, FND-RNG-005, FND-RNG-004 |
| generator | 173 `+2737` | FND-PARTY-091 (`+2560`'s roll of 1 to 20) |
| generator | 173 `+2A6B` | FND-PARTY-092 (`+2A5D`'s repeated scaled draws) |
| generator | 174 `+0136` | FND-PARTY-121 (`+012F`'s spell weight) |
| generator | 174 `+0684` | FND-PARTY-242 (`+0634..+06B6`'s list pick) |
| generator | 183 `+1148` | FND-PARTY-071 (the reroll button's icon) |
| generator | 188 `+0817` | FND-RNG-024 |
| generator | 198 `+032B` | FND-PARTY-158 (`+02B2`'s list pick) |
| generator | 204 `+00E1` | FND-RNG-021 |
| generator | 204 `+0A29` | FND-PARTY-169 (`+0A0B`'s draw modulo 100) |
| generator | 204 `+166A` | FND-ACTOR-014 (request 8's pair draws) |
| seed setter | 180 `+065E`, 204 `+001E` | FND-RNG-009 |
| `chance_in_ten` | `2834:000C` +0327, +0345, +0391, +03D9 | FND-SOUND-012 |
| `random_mod` | `2834:000C` +0182, +02FD; `2834:0519` +0535..+0608 (nine) | FND-RNG-003, FND-RNG-006, FND-SOUND-012 |
| `roll_sum` | 179 `+2B59` | FND-PARTY-099 (`roll_sum(1, 20)`) |
| `roll_sum` | 179 `+2BF0` | FND-PARTY-150 (`roll_sum(1, 100)`) |
| `roll_sum` | 183 `+159D` | FND-PARTY-071 (`roll_sum(4, 4)`, four times) |
| `roll_sum` | 186 `+05FC`, `+0615`, `+0626` | FND-TEXT-008 (the default name's resource) |
| `roll_sum` | 195 `+148C` | FND-PARTY-168 (`roll_sum(2, 4)`) |
| `roll_sum` | 197 `+387E` | FND-PARTY-249 (`roll_sum(1, 100)`) |
| `roll_sum` | 210 `+00BD` | FND-PARTY-074 (the hit die roll) |
| `roll_sum` | 210 `+0D6E` | FND-PARTY-082 (the hit die roll) |
| `random_between` | 176, five sites | FND-RNG-025 |
| `random_between` | 179, six sites | FND-RNG-026 |

That is 52 of the 117 sites: all 19 load-image and near sites, both seed-setter sites, ten
generator overlay sites, ten `roll_sum` sites and all eleven `random_between` sites.

### Sites no recorded finding describes

Generator, 15 overlay sites: overlay 173 `+0E99`, `+0EDB`, `+1BE2`, `+31DF`, `+3208`, `+3B21` and
`+3B3D`; overlay 184 `+0AEA`, `+1AE5`, `+1B51`, `+1BCD` and `+1C39`; overlay 197 `+1082`;
overlay 206 `+1360` and `+1382`.

`roll_sum`, 50 overlay sites: overlay 179 `+145E`, `+14C8`, `+14EF`, `+1725`, `+18AA`, `+1D01`,
`+1E06`, `+1F74`, `+214E`, `+2188`, `+21B9`, `+21D7`, `+225D`, `+229D`, `+22D3`, `+2308`, `+24E1`,
`+25DC` and `+25FE`; overlay 191 `+0D2A`, `+0DE8` and `+0EC6`; overlay 193 `+20B1`; overlay 195
`+005F`, `+0087`, `+020D`, `+03E9`, `+0585`, `+05DE`, `+0637`, `+06E5`, `+0705` and `+1624`;
overlay 197 `+11E8`, `+17F0`, `+1A18`, `+1B55`, `+3194`, `+3368`, `+33B5`, `+3426`, `+3445` and
`+39AD`; overlay 198 `+0451`; overlay 204 `+21B7`; overlay 206 `+15B8`, `+1600`, `+16A6` and
`+16E8`; overlay 211 `+1EDF`.

Overlay 197 `+1082`, `+17F0` and `+1A18` lie inside routines FND-PARTY-112 reads for two spells
only; its text names one `roll_sum(1, 100)` test in `+1760` without saying which of the two sites
makes it, and does not describe a draw in `+0E8F`. They are listed here as not described.

## Interpretation

The direct-call part of Q-RNG-001's census is taken: the generator has 29 direct sites, of which
the reducers and script instruction `0x52` hold four, and the rest are overlay sites; `roll_sum`
is reached only from overlays; `random_mod` and `chance_in_ten` are reached directly only from the
music selector and from `chance_in_ten` itself. Every recovered `random_between` site and every
recovered seed-setter site is already described. What remains of the direct census is to qualify
the 15 generator and 50 `roll_sum` sites above as instructions reached from established entries,
and to give each its rule, its arguments and, for `roll_sum`, how the caller reads the total.

## Alternatives

- Each hit is a byte pattern. For the 65 sites in the second list no instruction decode from an
  established entry has been recorded, so any of them could still be bytes inside other
  instructions or data; the controls show that the query finds real calls of each kind, not that
  every hit is one.
- The query excludes indirect calls, far calls built in registers or memory, jumps used as calls,
  noncanonical segment aliases of the targets' segments and far calls from the load image
  without a relocation. Those remain Q-RNG-008; access to `rng_state` other than through these
  routines remains Q-RNG-007.
- Overlay 173 `+2A5D` and others like it draw repeatedly and act as further dice helpers; their
  callers are not part of this census of the six targets.
- A site that a finding describes for one argument or one caller (FND-PARTY-112, FND-PARTY-158's
  shipped bytes) is described only that far; the rule each such caller serves is the rule its own
  finding gives, not a claim of this census.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`:
`direct_callers.py <dsun> 1000:0822 2834:061C 28C9:391D 2D40:3A03 2834:0519 1000:0811`, where
`<dsun>` is the installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`);
the script checks both. The output lists each hit's file offset and overlay offset; the counts
above are its line counts per target and kind. The output is kept as `direct.txt` under
`GAME_DIR/analysis/rng-callers-20261020/`. The described sites were matched by reading each
finding named above for the routine and offset range that contains the site. Nothing was
executed.
