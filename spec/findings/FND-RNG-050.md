---
id: FND-RNG-050
title: Of overlay 206 +0E1E's other callees six reach no draw, overlay 201 +05D5 reseeds and draws through the display offset loop while a sample plays and at one picture frame, and five reach draws only through long routes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F76E..0x0008F9C3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089D95..0x00089FE7
tool: tools/research/exec-census/reach_config.py, direct_callers.py, far_pointer_words.py, alias_far_words.py, overlay_listing.py and resident_listing.py at commit e1047cfa; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1; scientific-method-engine 19.10.0 reach
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 206's code starts at file `0x8E950` and
overlay 201's at `0x897C0`. The input is the installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`).

### The calls in `+0E1E`

Decoding overlay 206 `+0E1E` from its entry to its `retf` at `+1072` gives these calls besides
the cry `+151E` at `+0E77`, overlay 173 `+3136` at `+0FA4` and the dissolve `+1257` at `+103E`
(FND-RNG-043):

| Site | Callee | When |
|---|---|---|
| `+0E6B` | overlay 201 `+05D5` with the slot | state byte 2 and the absolute value of the word at `DS:67D6 + 37 * slot` is 430, in place of the cry |
| `+0E7E` | `2C5F:0A22`, the sound-effect player, with the cry's number | state byte 2 and that value not 430 |
| `+0E9D` | `1695:02E7` with the slot and `0xF460` | state byte 2 and the byte at `DS:45AA` is 0 |
| `+0EBB`, `+0EDD` | `31E0:1808` with the slot, `31E0:271E` with the slot and a byte | slot below 4 and the word at `DS:0DAB` is 1 |
| `+0F1F` | overlay 206 `+0A4E` with 2 | as the row above |
| `+0F45`, `+0F4E` | `2D40:3388` with the slot, two position words shifted right by 4 and a source line; overlay 190 `+26F8` with the slot | slot below 4 |
| `+0F71`, `+0F95` | `3143:00A8` with an index and 0x1E; overlay 197 `+30ED` with 0x120 and the index | slot of 4 or more with state byte 2, for each index 0 to 3 whose `3143:00A8` result is not -1 and names a row of `4E71:024F` holding the slot |
| `+1023`, `+102B`, `+1067` | the unload `31E0:05BB`, `2C5F:0139` | as FND-RNG-043 gives |

After `+0E6B` the routine goes on at `+0E84` as it does after the cry.

### Walks

A `reach` query per callee, with the targets `1000:0811` (the seed setter), `1000:0822` (the
generator), `2834:061C`, `2834:0519`, `28C9:391D` and `2D40:3A03` (FND-RNG-028's six), declares
the leaf `1000:0388` (FND-PARTY-039, FND-PARTY-043), the call `1000:02C1` as not returning
(FND-EXE-578), the jump `1000:252C` through the one word at `DS:398C` (FND-EXE-579), and the 112
`indirectCalls` of FND-RNG-033's query and of `q007_dsun_apfm_census.json` and
`newgame_declarations.json`, the declarations FND-RNG-045 reads. Each walk has a call inside its
start as a control and that call's callee as an instruction control (the leaf `190+26F8`, which
makes no call, has its `retf` at `+2719` as the instruction control).

| Start | Control | Routines | Unresolved | Targets reached | Negative usable |
|---|---|---|---|---|---|
| `2C5F:0A22` | `+0A5E` to `4611:0051` | 176 | 0 | none | yes |
| `31E0:1808` | `+1860` to `31E0:1893` | 48 | 0 | none | yes |
| `31E0:271E` | `+274B` to `31E0:1808` | 50 | 0 | none | yes |
| `2D40:3388` | `+33CB` to `25AF:09CB` | 11 | 0 | none | yes |
| overlay 190 `+26F8` | none | 1 | 0 | none | yes |
| `3143:00A8` | `+00B5` to `3143:0061` | 3 | 0 | none | yes |
| overlay 201 `+05D5` | `+05EB` to `2C5F:018C` | 1,835 | 2 | all but `2834:061C` and `2834:0519` | no |
| `1695:02E7` | `+02FC` to `1695:06FF` | 1,835 | 2 | the same | no |
| overlay 206 `+0A4E` | `+0A6B` to `2C5F:0271` | 1,835 | 2 | the same | no |
| overlay 197 `+30ED` | `+3125` to overlay 193 `+1873` | 1,835 | 2 | the same | no |
| `31E0:05BB` | `+062E` to `3D72:0B84` | 1,835 | 2 | the same | no |
| `2C5F:0139` | `+0146` to `362C:035C` | 1,835 | 2 | the same | no |

Every control is reached and every instruction control decodes. The two unresolved transfers in
the large walks are the far jumps through `cs:[bx + 0x18D]` at `2707:0188` and `cs:[bx + 0x591]`
at `2707:058C`. No walk has a gap or a contested instruction. The six large walks read the same
routines. The shortest chain the engine gives to the generator is, from `31E0:05BB`: `31E0:044C`,
overlay 193 `+1DB0`, `+1873`, overlay 195 `+1000`, `28C9:2C9E`, the movement routine `2D40:10AE`,
overlay 173 `+1CC1`, `+2560`, whose call at `+2737` draws (FND-PARTY-091); from overlay 197
`+30ED` it enters the same chain at overlay 193 `+1873`, from `2C5F:0139` through `31BA:000E`,
`31E0:0EFF`, `31E0:0D09`, `31E0:0B4A` and overlay 193 `+1DB0`, and from overlay 206 `+0A4E`
through `2C5F:0271`, `31E0:2C86` and `31E0:2D6E` to `2D40:10AE`. The chain from `1695:02E7` passes
`1695:06FF`, `1695:075D`, `172C:000C`, `172C:00A1` and `172C:018F` to the script handler
`172C:170B` (FND-SCRIPT-011). The shortest chain to the seed setter from `31E0:05BB`,
`2C5F:0139`, overlay 197 `+30ED` and overlay 206 `+0A4E` passes the movement routine and goes on
from the attack routine's call of overlay 173 `+24C7` at `+2290` to `+0E1E` itself, then overlay
201 `+05D5` and overlay 204 `+0000`; from `1695:02E7` it reaches `+0000` through the script
dispatcher overlay 204 `+0169` and the wrapper `+1F06` (FND-RNG-022). The engine gives one
shortest chain per target.

### Overlay 201 `+05D5`

`direct_callers.py <dsun> 201+05D5 204+0000` gives `+05D5` one caller, the far call at overlay 206
`+0E6B` through the trampoline `5778:0043`, and overlay 204 `+0000` the near call at `+1F3D` and the
far calls at overlay 193 `+0413` and overlay 201 `+075F` and `+0810`, FND-RNG-022's three, which
serve as the control. `far_pointer_words.py <dsun> 5778:0043` finds the trampoline's offset word
stored with a fixed-up segment only in that far call, and `alias_far_words.py <dsun> 5778:0043
28C9:0CFF` finds no other segment word that forms its address; the control `28C9:0CFF` returns
its five known pairs (FND-RNG-045).

From its entry `+05D5` reaches `+06B7` on every path (FND-UI-048 reads the dispatch on the
combatant's byte `+0x1F`). It requests a first `BMP` picture through `38FF:04AB` and, when the
request returns 0 with a pointer that is not 0, shows each of its frames with a wait of 400
through `1000:12FA` (`+06B7..+073E`). Then (`+073E..+0770`):

1. when `sound_on` (`DS:13F7`) is not 0, it plays sound 230 through `2C5F:0A22`;
2. when `digital_sound_on` (`DS:14E3`) is also not 0, it calls `fn_4611_0051`, which is not 0
   while a sample plays (FND-SOUND-007, FND-SOUND-008), and, each time that is not 0, calls
   overlay 204 `+0000` with magnitude 2, mode 4 and count 1 at `+075F` and calls `fn_4611_0051`
   again (`+0757..+0770`).

The test comes before the first call, so a sample that has ended makes no call. When the first
request fails the routine skips from `+06D4` or `+06DE` to `+0782`, past the sound and the loop.

It then requests a second `BMP` picture (`+0782..+07A9`) and, when that succeeds, shows its
frames `si` from 0 up; for the frame with `si` 7, when `sound_on` is not 0, it plays sound 8 and
calls overlay 204 `+0000` with magnitude 2, mode 4 and count 1 at `+0810` before the wait of 400
(`+07F4..+0820`). A picture of fewer than 8 frames makes no such call.

Overlay 204 `+0000` reseeds from the clock helper before it tests its mode or count, and, with mode
4 and count 1, draws once when both its offsets are 0, which they are at its start (FND-RNG-021).
So each call from `+05D5` sets `rng_state` from the clock and then draws once.

## Interpretation

Of the calls `+0E1E` makes besides the cry, `+3136` and the dissolve, the sound-effect player,
`31E0:1808`, `31E0:271E`, `2D40:3388`, overlay 190 `+26F8` and `3143:00A8` make no draw and do not
reseed on any path, on the walk's assumptions: each call returns, no interrupt handler is read
and the declared computed transfers hold.

For a removed combatant of state 2 whose word `+0x1B` has the absolute value 430, `+0E1E` runs
overlay 201 `+05D5` instead of the cry. With `sound_on` and `digital_sound_on` set and the first
picture loaded, that sequence reseeds the generator from the clock and draws once for every test
of `fn_4611_0051` that finds sound 230 still playing, so the number of reseeds depends on how
long the sample plays and the state that follows on the clock reading of the last one. With
`sound_on` set and a second picture of 8 frames or more, it reseeds and draws once more at frame
7. These are the only callers of `+05D5` and the conditions that reach the two overlay-201 calls
of the display offset loop.

`1695:02E7`, overlay 206 `+0A4E`, overlay 197 `+30ED`, the unload `31E0:05BB` and `2C5F:0139`, and
the rest of `+05D5`, reach the generator, `roll_sum`, `random_between` and the seed setter only
through the routes above, through the movement routine and the attack routine, the script
handler, or `+0E1E` itself. Whether those routes run when `+0E1E` calls them is not read.

## Alternatives

- A draw from an interrupt handler, or through a computed transfer the declarations do not cover,
  would not be found by the six negative walks; none of them has an unresolved transfer.
- The five long routes may never run from `+0E1E`'s calls: a static path through the movement
  and attack routines is a path in the call graph, not a shown sequence of calls. Each needs the
  conditions on its route read.
- The value 430 is read here only as a word of the object slot record that `+05D5` also passes,
  with the tag `RDFF`, to `2D40:000A`; which combatant carries it is not read.
- How many times `fn_4611_0051` reports a playing sample depends on the sound hardware and its
  driver; no run measured it.
- The overlays are outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 206 8E950 9004C` and `201 89D95 8A368`, reading
`+0E1E..+1073` and `+05D5..+0827`; `resident_listing.py <dsun>` over the first bytes of each
resident start to find its control; `direct_callers.py <dsun> 201+05D5 204+0000`;
`far_pointer_words.py <dsun> 5778:0043`; `alias_far_words.py <dsun> 5778:0043 28C9:0CFF`. For each
start, a query with that one start, the six targets, its control and instruction control and the
declarations above, then `reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
<config> <query>` and `python -I -m scientific_method_engine reach <config>`. Queries, reports and
listings are kept under `GAME_DIR/analysis/rng-0E1E-20261020/`. Nothing was executed.
