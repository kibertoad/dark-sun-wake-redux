---
id: FND-PARTY-026
title: No direct call made between program start and the party-loader gate reaches a writer of the placed-object count or of the gate's mode words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:0337
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277D:0004..277D:0580
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2322..28C9:24E9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081130..0x00081634
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000699B7..0x00069B2D
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7; a recursive-descent call graph of DSUN.EXE built with Python 3.14.7 over the MZ relocations, the FBOV trampolines and fixup lists, and the committed function inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
environment: null
---

## Observation

**The call graph.** Decoding started at every start in the function inventory, every
trampoline target of the 49 overlays and the program's entry point `1000:0000` (file
`0x00005200`), and at every call target it found. Each routine was followed through fall-through,
conditional and unconditional jumps until a return, and through `jmp cs:[reg+table]` jump tables
whose size it could bound: by the compare before the jump (entries 0 to the compared value) or,
for a value-scan switch, by the count in `cx` and the table address in `bx`. Far calls were
resolved through the MZ relocations, or for overlay code through its fixup list and the target
overlay's trampolines. The graph has 2,333 routines; 160 indirect calls or jumps and 11 jump
tables (3 with no bound found, 8 in resident code whose code segment was not known) are left
unresolved, and 20 decodes stop on bytes that are not an instruction. The inventory lacks
routines that are called, for example `362C:0000` (file `0x0002B4C0`), which the gate routine
calls; the graph includes them.

**Targets.** The five routines that store to the placed-object count `DS:264E` (FND-PARTY-022);
the routines that can be first to make `DS:0DAB` nonzero, the setter `2B1B:000A`, overlay 182
`0x15D7` and `0x1167` (FND-PARTY-024); and the routines that set the byte at `DS:13FB` to 1
(overlay 173 `0x3CDA`, overlay 192 `0x03AC`, overlay 204 `0x0169`). Overlay 182's `0x1856` and
`0x19F8` were treated as calling nothing, which FND-PARTY-024 shows for `DS:0DAB` equal to 0.

**What runs before the gate on a new game**, read from the code:

1. The entry point calls the routines at file offsets `0x000053B0` and `0x00005420`, then
   `277D:0004` (file `0x0001C9D4`); its other calls come after that one returns. All its callees
   other than `277D:0004` reach 11 routines.
2. `277D:0004` reads the command line and calls, before the start window, the routines at file
   offsets `0x00005BB0`, `0x00006F38`, `0x00007E77`, `0x00008086`, `0x00008987` and
   `0x0003F531`, overlay 180's `0x0141`,
   `0x0867` and `0x08E5`, and overlay 187's `0x2546` or `0x28B3`. It then stores 0 to the bytes at
   `DS:0690` and `DS:13FB` and calls overlay 194's `0x037B` (trampoline `574B:0020`) with 1.
3. Overlay 194's `0x037B` builds window 19500 (FND-UI-035), passing the far pointer
   `0x0610:0x002A`, whose fixup makes it trampoline `574B:002A` and so overlay 194's `0x0000`, to
   overlay 182's `0x016B`, and returns; it runs no loop of its own.
4. `277D:0004` then loops while the byte at `DS:1462` is nonzero. With `DS:13FB` 0, each pass
   calls file offsets `0x00026434`, `0x0002F88D`, `0x00005899` and `0x0002F62F`, then
   `0x0002CFDC` when the byte at `DS:1424` is nonzero, then `28C9:2322` (file `0x000201B2`), then
   `0x000264BF`. `28C9:2322` calls file offsets `0x000263BD` and `0x0002172A`, `0x0001D54C` when
   the byte at `DS:13F7` is nonzero, and `0x0003B361` when the bytes at `DS:14E3` and `DS:14E4`
   are both nonzero; then, when the word at `DS:0D9C` is 0, it returns 0 at `0x00020376`. Its other
   calls, among them two to `2C72:0009` that lead to the count's writers, are on the path where
   that word is nonzero. Overlay 192's `0x0015`, called at the end of a pass, runs only when
   `DS:13FB` and `DS:0690` are both nonzero.
5. Overlay 194's `0x0000` handles event 2 for the four buttons, and event 6 for keys, which can
   jump to the same Start Game branch (overlay 194 `0x013E`, `DSUN.EXE+0x0008126E`); event 1
   stores 0 to `DS:1462`, which ends the loop.
6. The Start Game branch calls `3EBE:071F` and `28C9:2A81` twice each in each of three passes,
   overlay 194's
   `0x0504`, and overlay 187's `0x2424` and `0x206E`, and then the gate routine (FND-PARTY-021).
7. The gate routine, before the gate at overlay 182 `0x12DD`, calls overlay 187's `0x2B6C` (when
   the byte at `DS:13F7` is nonzero), its own `0x236F`, `0x016B`, `0x01EF` and `0x0840`, overlay
   190's `0x11A1`, overlay 200's `0x0386`, `39D1:0448`, `362C:0000`, `3D72:0B84`, and `1BF3:27A8`
   twice (FND-PARTY-025).

**Result.** The routines reachable by direct calls from the calls in steps 1, 2, 4, 6 and 7 and
from overlay 194's `0x037B` number 685, and none of them is a target. Among those 685 routines are
60 call sites whose target the graph does not resolve:

| Group | Sites (file offsets of the calls) |
|---|---|
| Far pointers at `DS:3676`, `DS:367A`, `DS:367E` and a table indexed from `DS:A446`, in `1038:0008` | `0x000055A1`, `0x000055B1`, `0x000055C9`, `0x000055CD` |
| A near or far pointer at offset 2 of a record through `es:bx`, in `1022:0000` and `1026:0004` | `0x00005455`, `0x0000545C`, `0x00005496`, `0x0000549D` |
| A near pointer argument at `[bp+0Ah]`, in `10A1:000B` | `0x00005C37` |
| A far pointer local at `[bp-4]`, in `11EB:0004` and `11F5:0000` | `0x00007110`, `0x000071BC` |
| A near jump through `DS:398C`, at `1252:000C` | `0x0000772C` |
| Far pointer tables at `DS:3F42`, `DS:3F46` and `DS:3F4A` | `0x00009A3C`, `0x00009B83`, `0x0000A04A`, `0x0000A09A`, `0x0000A219`, `0x0000AAA1`, `0x0000AAFB`, `0x0000AEF1`, `0x0000B0A8` |
| A far pointer at offset `0xB8`, through CS once and DS twice | `0x0000B704`, `0x0000B79C`, `0x0000B7C9` |
| Pointers at `DS:02F6` and in a table at `DS:030A` | `0x0000C667`, `0x0000C675` |
| A far pointer at offset `0x0C` of a record through `es:si` | `0x00016E3E` |
| A near table at `DS:61B8` and a far pointer at `DS:1154` | `0x000172C9`, `0x000172D0` |
| Far pointers at `DS:A0F1` and `DS:A0F5`, and at offset `0xF5` of a record through `es:bx` | `0x0002F696`, `0x0002F6DC`, `0x0002F712`, `0x0002F74B`, `0x0002F7A9`, `0x0002FA65` |
| Far pointers at offsets `0xF9` and `0xFD` of a record through `es:bx`, in `3AEE:000D` | `0x000301E7`, `0x00030397` |
| Far pointers at offsets `0x70`, `0x62`, `0x5E` and `0x5A` of records through `es:bx` | `0x00032A4A`, `0x00032EB0`, `0x000332E7`, `0x000334F7` |
| A far pointer at `DS:A119` | `0x000336C1`, `0x000336F0` |
| Far pointers at `DS:346D` to `DS:3492` | `0x0003C6D9`, `0x0003CB07`, `0x0003F007`, `0x0003F071`, `0x0003F57D`, `0x0003F6ED`, `0x0003FDBB`, `0x0003FDE2` |
| A far pointer at `DS:0043` | `0x00040FA5`, `0x00040FB0`, `0x00040FBD`, `0x00040FCB`, `0x00040FF5`, `0x00040FFF`, `0x0004111B`, `0x00041129`, `0x00041270` |

## Interpretation

Through direct calls and bounded jump tables, nothing that runs from program start until START
GAME reaches the gate can change the placed-object count, make `DS:0DAB` nonzero, or set
`DS:0D9C`: the count is still 0 at the gate, so the party loader runs, unless one of the 60
unresolved calls reaches a writer or one of the two reservations of FND-PARTY-025 fails. The
window callbacks at `0x000301E7` and `0x00030397` are how the start window's own callback is
called, so they reach overlay 194's `0x0000` at least; what else each pointer can hold is not
shown.

## Alternatives

- A writer runs before the gate through one of the 60 unresolved calls: not ruled out; it needs
  the producers of each pointer.
- A writer runs through a jump table the graph could not bound, or through code the graph
  decoded wrongly: not ruled out for the 11 unresolved tables and the 20 stopped decodes, none
  of which the graph places on these paths.
- The player's choices before START GAME change this: this reading covers a session in which
  START GAME is the first choice made on the start window; Load Saved Game and Create Characters
  enter overlays 192 and 212, which were not followed.

## How to reproduce

Build the call graph as described, with the far targets resolved as in FND-PARTY-021 and
FND-PARTY-022. Treat overlay 182's `0x1856` and `0x19F8` as leaves. Take as starting points the
callees of the entry point other than `277D:0004`, the far calls in `277D:0004` before the one at
`0x0001CC94`, overlay 194's `0x037B`, the per-pass calls in step 4, the Start Game branch's calls
in step 6 and the gate routine's calls in step 7, and search breadth-first for the targets.
Disassemble `277D:0004` from `0x0001C9D4` to `0x0001CF50`, `28C9:2322` from `0x000201B2` to
`0x00020379` and overlay 194 from `0x00081130` to `0x00081634` to read the conditions in steps 2
to 5.
