---
id: FND-PARTY-044
title: Before the party-loader gate, the window and APFM callback fields hold no routine that reaches the count, DS:0DAB or pointer writers unless another start-window button runs first
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:07D2..39D1:0819
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:05B8..3A8E:05E9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0CDC..3A8E:0D6C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0211..3F96:02F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0603..3F96:0650
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0635..3D72:06D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0755..3F96:07B3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000689BB..0x00068A7D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069A18..0x00069B2D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081258..0x0008145F
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/field_stores.py, store_values.py, immediate_search.py, segment_references.py, direct_callers.py, trampoline_target.py, resident_listing.py, overlay_listing.py); scientific-method-engine 15.0.0 `reach` (tools/research/exec-census/reach_config.py with the queries named below)
environment: null
---

## Observation

**The seven calls.** Each is a far call through a double word of a record at `ES:BX`:

| File offset | Address | Field | Record | Skipped when |
| --- | --- | --- | --- | --- |
| `0x2F712` | `39D1:0802` | `+0xF5` | the far pointer at `DS:A0FD`, read only while `DS:A103` is nonzero | `DS:A0FD` or the field is 0 |
| `0x301E7` | `3A8E:0707` | `+0xFD` | the far argument of `3A8E:060D` | the field is 0 |
| `0x30397` | `3A8E:08B7` | `+0xF9` | the far argument of `3A8E:060D` | the field is 0 |
| `0x32A4A` | `3D72:012A` | `+0x70` | the record `3D72:0EB8` returns, required to hold `APFM` at `+0x00` (`3D72:008E`) and 2 at `+0x66` | the field is 0 |
| `0x32EB0` | `3D72:0590` | `+0x62` | the far argument of `3D72:0515`, whose tag argument must be `APFM` (`3D72:0529`) | the field is 0 |
| `0x332E7` | `3D72:09C7` | `+0x5E` | the far pointer at `DS:A15B` | `DS:A13D` is not 2 |
| `0x334F7` | `3D72:0BD7` | `+0x5A` | the far pointer at `DS:A15B` | `DS:A155` is not 2, or the field is 0 |

**Window records.** `3A8E:060D` reads tags `APFM`, `BUTN`, `MENU` and `EBOX` from 30-byte item
entries at `+0x105` of its record, so its record is a window. `3A8E:02B3` sets a window up:
it loads each item as a resource by tag and number with `38FF:0438` (`3A8E:03B4`) and, at its
end, stores the double word at `DS:A0F1` to `+0xF5` and 0 to `+0xF9` and `+0xFD`
(`3A8E:05CB..05E9`). `field_stores.py` finds, among stores with a base or index register and
displacement `0xF5`, `0xF7`, `0xF9`, `0xFB`, `0xFD` or `0xFF`, with or without an `ES` prefix,
only those three and one in each of `3A8E:0CDC`, `3A8E:0D0C` and `3A8E:0D3C`, which store their
second far argument to `+0xF5`, `+0xF9` and `+0xFD` of their first; the other hits it prints
are the same six instructions decoded after their operand-size prefix. `3A8E:02B3` has one
direct caller, overlay 182 `+0110`, in `+00F6`. Overlay 182 `+016B` loads a `WIND` resource
with `+00C4` (`+0191`), calls `+00F6`, stores its far argument at `[bp+0xC]` as the window's `+0xF5` with
`3A8E:0CDC` (`+01BC`) and draws the window with `3A8E:060D` (`+01CC`). `+00F6` is called
directly only from `+01AA` and, through trampoline `56BD:003E`, from overlay 172 `+00A6`.
`+016B` (trampoline `56BD:0048`) has 31 far callers and one near one, `+11D5`. `3A8E:0CDC`,
`0D0C` and `0D3C` have 7, 17 and 18 far callers, all in overlays.

**APFM records.** The `APFM` item set-up, `3F96:000B`, writes `+0x10..+0x17` and `+0x54` of the
record. FMT-UI-004 records bytes `0x5A..0x73` as 0 in every shipped `APFM` resource
(FND-UI-005). With `ES` prefixed displacements `0x5A`, `0x5C`, `0x5E`, `0x60`, `0x62`, `0x64`,
`0x70` and `0x72`, `field_stores.py` finds, besides `3F96:0255`, `02A2`, `02EF` and `0647` (the
setters `3F96:0211`, `025E`, `02AB` and `0603`, one per field `+0x5A`, `+0x5E`, `+0x62`,
`+0x70`): word stores to `+0x5C..+0x63` in `3EBE:0008`, the `BUTN` set-up that `3A8E:04CE`
calls; a store to `+0x64` in `3EBE:0991`, on a record it looks up by the tag `BUTN`; and
stores in overlays 173, 184 and 195 whose `ES` is loaded with the fixed segment words `0x308`
or `0x368`. Each setter looks the record up by window, the tag `APFM` and a number with `39D1:049C`. `3F96:0211`, `025E` and `0603` have no
direct caller, near or far. `segment_references.py` finds one word naming segment `3F96`
outside far calls, its own segment-table descriptor at file `0x4B1F0`, and
`immediate_search.py` finds no push or move of `0x0211`, `0x025E` or `0x0603`. `3F96:02AB` has
28 far callers; overlay 182 `+01EF` calls it with its far argument at `[bp+0xC]` (`+020B`).

`DS:A15B` is 0 in the file and is written only at `3D72:06AC`, with the record at `DS:A125`
after `DS:A129` is checked to be `APFM`, and at `3F96:079B`, with a record `39D1:049C` found by
the tag `APFM`. `DS:A13D` is set to 2 only at `3D72:06C5`, after both `+0x5A` and `+0x5E` of
that record are found nonzero; its other stores write `0xFFFF`, 0 and 3. Each store to `DS:A155`
copies `DS:A13D` (`39D1:0251`, `3D72:0B4C`, `0BE3`, `0C69`).

**The gate routine.** Overlay 182 `+1167`, after `DS:0DAB` is found 0, opens window `0x4C2D`
with `+016B` and handler `28C9:0CFF` (`+11D5`), passes `28C9:0CFF` to overlay 190 `+11A1`
(trampoline `571F:0034`, `+11EF`), which hands it to the `DS:A0F1` writer (FND-PARTY-039), and
sets the `+0x62` callback of `APFM` items `0x4B00` and `0x4B01` of that window to `28C9:1261` and
`28C9:0061` with `+01EF` (`+120F`, `+122D`). It then calls `39D1:0448`, `362C:0000`, `1BF3:27A8`
twice, `+0840`, overlay 200 `+0386` (trampoline `5773:0025`) and `3D72:0B84` before the gate at
`+12DD`.

**The start window.** Overlay 194 `+0000`, the handler of window 19500 (FND-PARTY-031), takes
event 2 for buttons `0x4B64..0x4B67` through the four words at `+0327`: `+013E`, the Start Game
branch; `+01F4`, which calls overlay 212 `+061C` (trampoline `57C9:0025`) at `+0256`; `+0261`,
which calls overlay 192 `+0000` (trampoline `5736:0020`) at `+02B7`; and `+02BE`, which stores 0
to `DS:1462` and so ends the start loop. After the overlay 212 and 192 calls it returns 0 through
`+030E` with `DS:1462` unchanged. Event 6 maps 19 key codes through the tables at `+032F` and
`+0355` onto these branches and the counters at `DS:6282` and `DS:6283`.

**Windows opened from those branches.** Overlay 212 `+061C` opens window `0x2CEC` with handler
`57C9:0043` (overlay 212 `+0967`), or stores that handler with `3A8E:0CDC` when the window
exists, passes it to the `DS:A0F1` writer, and sets `+0xF9` to `57C9:003E` (`+0000`), `+0xFD`
to `571F:00A7` (overlay 190 `+361C`) and `APFM` `+0x62` callbacks to `571F:0057` (overlay 190
`+0092`) and `57C9:002F` (`+0A57`). Overlay 192 `+024B` opens window `0x4844` with handler
`5736:002F` (`+0785`), passes it to the `DS:A0F1` writer and sets `+0xFD` to `5736:003E`
(`+022B`). Overlay 172 `+05DB` (trampoline `566A:002A`), called from overlay 180 `+0AB6` in
`+08E5`, opens window `0x2905` with handler `566A:004D` (overlay 172 `+0816`).

**The runs.** Each configuration is built by `reach_config.py` as in FND-PARTY-040. The 29
targets are those of FND-PARTY-040 (`q011_6393_reach.json`); the 120 setter sites are the call
instructions of every direct caller of overlay 182 `+016B`, `+00F6` and `+01EF`, `3A8E:0CDC`,
`0D0C` and `0D3C`, and `3F96:02AB`. Unless named, leaves are FND-PARTY-040's two.

1. `q011_gate_dispatch_reach.json`. Starts: the nine routines the gate routine calls from
   `+11D5` to the gate, overlay 182 `+016B`, overlay 190 `+11A1`, overlay 182 `+01EF`,
   `39D1:0448`, `362C:0000`, `1BF3:27A8`, overlay 182 `+0840`, overlay 200 `+0386` and
   `3D72:0B84`. Targets: `39D1:0752`, the routine holding `0x2F712` and the `DS:A0F1` and
   `DS:A0F5` calls, its five calls `0x2F696`, `0x2F6DC`, `0x2F74B`, `0x2F7A9` and `0x2FA65`,
   and the seven calls. Control: the call of `39D1:0609` at `0x334DF` in `3D72:0B84`. 231
   routines and 18,072 instructions. It reaches `0x301E7`, `0x30397`, `0x332E7` and `0x334F7`
   and no other target, and leaves 13 calls unresolved.
2. `q011_gate_dispatch_values.json`. The same, with 18 more starts: the start-up and exit record
   targets, `180+0848`, `1000:0387`, the `1000:09F4` putters and `1000:129F` (FND-PARTY-039),
   and `4AE5:1257`, `4AE5:1155`, `4AE5:0EA2` and `1000:02FC` (FND-PARTY-040). 302 routines and
   20,684 instructions, the same four targets. Its 19 unresolved calls are the start-up, exit,
   Ctrl-Break, putter and `DS:398C` calls of FND-PARTY-039 with values among the starts,
   `_setargv`'s return jump at `0x7B8A` (FND-PARTY-038), the overlay manager's `0x401F9`,
   `0x401FE` and `0x4041B`, `0x411C2` and `0x411CC`, which are `lcall [0x43]` at `4AE5:1172` and
   `117C` with `DS` loaded from `CS:0005` (the writer FND-PARTY-039 gives as `4BD8:0016..0023`
   is the same code at `4AE5:0F46`), and four of the seven calls.
3. `q011_setter_sites_reach.json`. Starts, leaves and controls of `q011_values_round0b.json`
   (FND-PARTY-040); the 120 setter sites as targets. 783 routines and 46,643 instructions. It
   reaches overlay 172 `+0696`, overlay 194 `+049E` and the setter calls inside `+016B` and
   `+01EF`.
4. `q011_window_handlers_reach.json`. Run 3 with starts overlay 194 `+0000` and overlay 172
   `+0816`, the 29 targets added, and overlay 182 `+1167` as a leaf. 806 routines and 48,351
   instructions. Of the 29 targets it reaches only `+1167`, as a leaf. It also reaches the
   setter calls at overlay 192 `+02B2` and `+02E7`, through overlay 192 `+0000` and `+024B`, and
   overlay 212 `+0688`, `+06B8`, `+06F8`, `+072B`, `+074B`, `+075E` and `+088D`, through
   overlay 212 `+061C`.
5. `q011_window_handlers_round1.json`. Run 4 with seven more starts, the values those calls
   pass: overlay 192 `+0785` and `+022B`, overlay 212 `+0967`, `+0000` and `+0A57`, and overlay
   190 `+0092` and `+361C`. It stops at the instruction limit (a gap at `0x69EDC`). It reaches
   12 of the 29 targets: `0x271E3`, `0x27404`, overlay 173 `+3CDA`, `2C5F:0CBF`, `2C5F:0CF1`,
   `0x777F7`, `0x9106C` and `0x910C6`, each with a fewest-call chain from overlay 212 `+0967`,
   and `0x89297`, `0x892D0`, overlay 182 `+15D7` and `+1167`, each with one from overlay 192
   `+0785`.
6. `q011_start_handler_reach.json`. Start overlay 194 `+0000` and the 22 fixed pointer values of
   FND-PARTY-039 and FND-PARTY-040 (the four `2660` routines added); leaves FND-PARTY-040's two,
   overlay 182 `+1167`, overlay 212 `+061C` and overlay 192 `+0000`. 364 routines and 23,649
   instructions. It reaches only `+1167`, as a leaf.
7. `q011_main_window_handler_reach.json`. Start overlay 172 `+0816` and the same 22 values,
   leaves those of run 4. 265 routines and 17,932 instructions; no target.

Every run reports the same five gaps and four contested bytes at `1000:02E9..02F8`: the
inventory row `1000:02AD` runs past the exit call at `1000:02C1` through the run-time library's
overlay error message at `1000:02C8` into a second routine at `1000:02E1`, and the message bytes
decode as a jump into it. The inventory rows `3EBE:001F` and `3EBE:09B6` start inside
instructions (the far call at `3EBE:001C`, the push at `3EBE:09B1`); the routines begin at
`3EBE:0008` and `3EBE:0991`.

## Interpretation

`+0x5A`, `+0x5E` and `+0x70` of an `APFM` record hold the resource's zero bytes: their only
setters are never called, and the other stores at those offsets write `BUTN` records or fixed
data segments. So `0x32A4A` and `0x334F7` are skipped, and `0x332E7` runs only in state 2, which
needs those fields nonzero. A window's `+0xF9` and `+0xFD` are 0 from its set-up until one of
the overlay setters runs, and its `+0xF5` is the handler the opener gives.

Before the gate, on the paths of FND-PARTY-031, the windows are 19500 (handler overlay 194
`+0000`), `0x2905` (overlay 172 `+0816`) and, inside the gate routine, `0x4C2D` (`28C9:0CFF`); no
`+0xF9`, `+0xFD` or `APFM` `+0x62` setter runs except the gate routine's own `28C9:1261` and
`28C9:0061`. From the window's opening at `+11D5` to the gate no window handler, `APFM` callback
or `DS:A0F1`/`DS:A0F5` value is called, so `28C9:0CFF`, which reaches the count's stores
(FND-PARTY-040, round 1), and the two `APFM` callbacks do not run before the gate. Overlay 194
`+0000` reaches the targets only through the gate routine itself or its `0x4B65` and `0x4B66`
branches, and overlay 172 `+0816` reaches none. Under the reports' assumptions none of the seven
calls opens a route to a target on a path where START GAME is the first start-window choice.

The `0x4B65` branch (overlay 212 `+061C`) and the `0x4B66` branch (overlay 192 `+0000`) open
windows whose handlers do reach targets, and the start handler returns to the start loop after
each. Whether a player who takes one of them before START GAME reaches the gate with a changed
count, a nonzero `DS:0DAB` or another pointer image is not read here.

FND-PARTY-040 took the gate routine's callees as starts, not its own body, so its round 0 did not
reach the `28C9:0CFF` store at `+11EF`; that store does run before the gate, but nothing dispatches
the value before it.

## Alternatives

- A record field is written through a block copy, a pointer register without a displacement, or
  a segment the search does not name: `field_stores.py` covers stores whose operand has the
  displacement and a base or index register; a copy of a whole `APFM` or window record is not
  covered.
- A setter with no direct call runs through a computed call: ruled out for `3F96:0211`, `025E`
  and `0603` by the absence of a segment word for `3F96` outside the segment table and of their
  offsets as immediates.
- The zero bytes FMT-UI-004 records are not what the loader places at `+0x5A..+0x73`: the set-up
  routines read `+0x28`, `+0x2A` and `+0x58` at the offsets FMT-UI-004 gives, so the record is
  the resource as stored.
- A window opened by a route outside the starts sets `+0xF9` or `+0xFD` before the gate: the
  setter-site runs reach no such call except from the `0x4B65` and `0x4B66` branches.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `field_stores.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv F5 F7 F9 FB FD FF`, the same with `--es`, and
`field_stores.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es 70 72 62 64 5E 60 5A
5C`; `store_values.py <dsun> A15B A15D A13D A155 0043 0045`; `segment_references.py <dsun> 3F96`;
`immediate_search.py <dsun> 0211 025E 0603`; `direct_callers.py <dsun> 39D1:0752 3A8E:02B3
3A8E:060D 3A8E:0CDC 3A8E:0D0C 3A8E:0D3C 3F96:0211 3F96:025E 3F96:02AB 3F96:0603 182+016B
182+00F6 182+01EF`; `trampoline_target.py <dsun> 571F:0034 5773:0025 56BD:0048 56BD:004D
566A:002A 566A:004D 57C9:0025 5736:0020 5736:002F 5736:003E 57C9:0043 57C9:003E 57C9:002F
571F:0057 571F:00A7`; `resident_listing.py <dsun> 39D1:0752..39D1:0854 3A8E:02B3..3A8E:060D
3A8E:060D..3A8E:08FB 3A8E:0CDC..3A8E:0D6C 3D72:0009..3D72:0140 3D72:0515..3D72:059E
3D72:0635..3D72:06D5 3D72:0942..3D72:09E0 3D72:0B84..3D72:0BF0 3F96:0000..3F96:0068
3F96:01C0..3F96:0320 3F96:05E0..3F96:0660 3F96:0755..3F96:07B3 3EBE:0000..3EBE:0060
3EBE:0990..3EBE:09C0 39D1:0248..39D1:0256 3D72:0B44..3D72:0B50 3D72:0C60..3D72:0C6D
4AE5:1155..4AE5:1185 1000:02AD..1000:0300`; and `overlay_listing.py <dsun> 182 0x68850 0x68AC0`,
`182 0x69990 0x69B40`, `194 0x81130 0x81470`, `192 0x7D54B 0x7D600`, `212 0x9881C 0x98AA0`,
`172 0x59A0B 0x59B10` and `180 0x67C60 0x67CB0`. Then, in the repository root, for each query
named in run 1 to 7 run `reach_config.py` and `python -I -m scientific_method_engine reach` as
FND-PARTY-040 gives.
