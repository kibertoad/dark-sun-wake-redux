---
id: FND-PARTY-046
title: ADD on the Create Characters screen loads a stored character and calls the placement routine, which increments the placed-object count, and Esc returns to the start window without a store that clears it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079179..0x00079256
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058336..0x000583C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000585D5..0x00058612
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058887..0x000589D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058A94..0x00058AF4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058CB5..0x00058E3A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079C93..0x00079D0A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0121..31E0:01E7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2522..28C9:252A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058AF4..0x00058B48
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A284..0x0007A308
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, trampoline_target.py); scientific-method-engine 15.3.0 `reach` (tools/research/exec-census/reach_config.py with the queries named below)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`.

**The character box menu.** A click on a character box of the Create Characters screen reaches
overlay 190 `+0D89` (FND-PARTY-045). When the box's record (`DS:19C5` plus 0x42 times the box
number in `4E71:0B44`) has 0 at `+0x0E`, an empty box, and the word at `4C10:0019` is 0, it shows
a menu with the choices `NEW`, `ADD` and `CANCEL` (strings at `DS:1DBA`, `DS:1DBE` and
`DS:1DC2`) through overlay 172 `+018F` (`+0E73`); with that word nonzero it shows a message that
characters cannot be added in combat instead. Choice 2, `ADD`, goes to `+0F00`, which calls the
pointer it was given (`+0F00`) and `28C9:2522`, and in both outcomes reaches `+0ECA`, which calls
overlay 171 `+0126` (trampoline `5664:0020`) with the box number (`+0ED4`). Choice 1, `NEW`,
calls overlay 186 `+03C1` and, when that returns 0, overlay 184 `+07D8` (trampoline `56DD:003E`,
`+0EF8`); when it returns nonzero a second menu offers to delete characters, and its first
choice also reaches overlay 171 `+0126`.

**The ADD window.** Overlay 171 `+0126` stores the box number to `4E71:0B44` (`+0132`), allocates
a buffer through `444C:0008` with 0x28 and 0x33, and opens window `0x4845` with handler `5664:002F`, which is
overlay 171 `+03C5` (`+0194`). `+03C5` handles event 6, a key, by looking it up among the 4
words at `+0928` and jumping through the 4 after them, and event 2, a button, by looking it up
among the 17 words at `+08E4` and jumping through the 17 after them:

| Button or key | Branch |
| --- | --- |
| `0x477D`, key `0x1C0D` (Enter) | `+0780`, confirm |
| `0x477E`, key `0x011B` (Esc) | `+0884`, cancel |
| `0x477F` | `+07D7` |
| `0x4780` to `0x4789` | `+0713`, a list line |
| `0x082D`, `0x284A` | `+0697` |
| `0x082E`, `0x284B` | `+06CD` |
| keys `0x4800`, `0x5000` | `+0567`, `+0405` |

The confirm branch shows a message that no characters are available when the word at
`4C4C:0000` is -1 (`+0785..+0799`). Otherwise it calls `3EBE:071F` with `0x477D` and
`28C9:2A81`, then `+0AA5` (`+07C1`). Both branches end at `+08D2`, which calls `+0E04`.

**The load.** `+0AA5` has no branch that skips its call at `+0C25`. It loads the `CHAR` record
(`push 0x52414843`) whose number is read from the list entry that `4C4C:0000` plus `4C4C:0002`
selects, into the slot held at `4E71:0B44`, through `2D40:000A` (`+0B23`). When that returns
-1 it skips the call at `+0B5A`; both paths reach `+0B62`, choose a position from one of three
branches that all rejoin at `+0BF6`, and call `31E0:0121` with the slot's value at `+0x10`
plus 0x12C, the slot number, the position and the byte at `DS:044A` (`+0C25`).

**The placement routine.** `31E0:0121`, while the count at `DS:264E` is below 1,500 (`0x5DC`),
calls `31E0:0EFF` with the count; when that returns -1 it returns -1. Otherwise it fills in the
table entry the result names and runs `inc word [264E]` at `31E0:01E3`. FND-PARTY-022 counts it
among the five routines that store to the count.

**Esc on the character screen.** The screen's handler, overlay 212 `+0967`, passes every key
whose low byte with `0x20` set is not `0x76` to overlay 190 `+139B`, with the far pointer
`57C9:002A`, overlay 212 `+08E3` (FND-PARTY-045). `+139B` looks the key up among the 33 words at
`+1F44` and jumps through the 33 after them; key `0x011B` goes to `+1953`. With the pointer
nonzero and the key `0x011B`, `+1953` calls the pointer, sets the byte at `+0x14` of each of the
four slot records at `DS:19C9` whose `+0x06` is nonzero and whose `+0x14` is 0, and calls
`28C9:2522`. That routine returns the word at `DS:0DAB`. When it is 0, `+1953` calls overlay
194 `+037B` (trampoline `574B:0020`) with 0 (`+19AE`), which builds window 19500 again
(FND-PARTY-045); otherwise it calls overlay 182 `+19F8` (trampoline `56BD:00BB`), `3D72:0D83`
and `28C9:2A81` with 1.

**The runs.** Each configuration is built by `reach_config.py` as in FND-PARTY-040, with the
instruction limit at 100,000.

1. `q017_add_window_reach.json`: the query of FND-PARTY-045 run 4 with overlay 171 `+03C5` as
   the first start in place of overlay 212 `+0967`, so the 22 fixed pointer values of
   FND-PARTY-044 run 6 as the other starts, the 149 targets of `q011_window_handlers_reach.json`,
   no control, and overlay 182 `+1856`, `+19F8` and `+1167` as leaves. 1,085 routines and
   86,437 instructions; 48 unresolved transfers, 52 interrupts, the five gaps and four contested
   bytes at `1000:02E9..02F8` of FND-PARTY-044. It reaches 51 targets, among them the count
   store `0x271E3` through `+07C1` and `+0C25` (two calls), the count store `0x27404`, the
   `DS:13FB` setter overlay 173 `+3CDA`, and the FND-PARTY-036 sites `2C5F:0CBF`, `2C5F:0CF1`
   and `0x777F7`. It reaches none of the stores of 0 to the count at `0x709B1`, `0x710A0` and
   `0x89297`, none of the other increments `0x709C5`, `0x710A8` and `0x892D0`, and none of the
   `DS:0DAB` setters `2B1B:000A`, overlay 182 `+15D7` and `+1167`.
2. FND-PARTY-045 run 4 (`q017_create_characters_reach.json`, start overlay 212 `+0967`), run
   again with engine 15.3.0: the same 1,222 routines and 99,085 instructions, with 66
   unresolved transfers. It reaches `0x271E3` and `0x27404` and none of the six other count
   stores above, the three `DS:0DAB` setters, overlay 192 `+03AC` or overlay 204 `+0169`.

The unresolved transfers of both runs have not been read, so neither run is usable as a
negative on its own.

## Interpretation

On an empty character box, `ADD` opens a list of stored characters; confirming it with a list
line selected loads that character into the box's slot and places it with `31E0:0121`, which
raises the placed-object count from 0 to 1 unless `31E0:0EFF` finds no free table entry. Nothing
on that path stores 0 to the count, so the count stays nonzero when the window closes.

Esc on the character screen returns to a start window built by the same routine as the first
one, provided `DS:0DAB` is still 0. Neither run reaches a routine that makes it nonzero, and
the start window's START GAME path reaches no count writer before the gate (FND-PARTY-031,
FND-PARTY-044). Under those readings, a player who adds a stored character and then chooses
START GAME reaches the gate at overlay 182 `+12DD` with the count nonzero, so the gate skips
the loader of characters 40 to 43 (FND-PARTY-021) and play begins with the party built on the
character screen.

## Alternatives

- `31E0:0EFF` returns -1 on an empty table, so `31E0:0121` returns before its increment: not
  read here. The routine is called with the count as its second argument; its body was not
  listed.
- A routine reached only through one of the unresolved transfers stores 0 to the count, or
  makes `DS:0DAB` nonzero, before START GAME: open until the 48 and 66 unresolved transfers are
  read.
- `NEW` places a character too: overlay 184 calls `31E0:0121` at `+12E5`, inside `+1029`, which
  `+0DE6` calls and trampoline `56DD:0057` exposes; whether `NEW` (`+07D8`) reaches it, and on
  which actions, is not read here.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 190
0x790C9 0x79400`, `190 0x796DB 0x79740` and `190 0x79C93 0x79D10`; `overlay_listing.py <dsun> 171
0x58336 0x58440`, `171 0x585D5 0x589D4`, `171 0x58A94 0x58AF4` and `171 0x58CB5 0x58E60`;
`resident_listing.py <dsun> 31E0:0121..31E0:01F0 28C9:2522..28C9:2560`; `trampoline_target.py
<dsun> 5664:0020 5664:002F 56DD:003E 56E9:0043 566A:0025 56BD:00BB`; `direct_callers.py <dsun>
184+1029`. Read the ADD window's tables as the
17 words at file `0x58AF4` and the 17 after them, and the 4 at `0x58B38` and the 4 after them;
read overlay 190 `+139B`'s as the 33 words at `0x7A284` and the 33 after them. Read the menu
strings at file `0x4EDBA`, `0x4EDBE` and `0x4EDC2`. Then, in the repository root, for each
query named in the runs, run `reach_config.py` and `python -I -m scientific_method_engine reach`
as FND-PARTY-040 gives.
