---
id: FND-PARTY-040
title: Before the party-loader gate, routes to the count, DS:0DAB and pointer writers open only through the script interpreter's opcodes or its loader-error routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007330C..0x00073331
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000576AD..0x000576C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:000C..172C:00EE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0100..172C:015E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3317..172C:335F
tool: scientific-method-engine 15.0.0 `reach` with Python 3.14.7, Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with the queries q011_startup_reach.json, q011_values_round0.json, q011_values_round0b.json, q011_values_round1.json and q011_opcodes_round0.json; overlay_listing.py, resident_listing.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

**The runs.** `reach_config.py` builds each configuration as in FND-PARTY-037 (132 regions, 7,562
far transfers, 157 jump tables), with the instruction limit at 100,000, the engine's maximum.
Every run has the two leaves of FND-PARTY-037 and these 29 targets, as file offsets or
addresses: the eight stores to the placed-object count, `0x000271E3`, `0x00027404`,
`0x000709B1`, `0x000709C5`, `0x000710A0`, `0x000710A8`, `0x00089297` and `0x000892D0`
(FND-PARTY-022); the routines that can first make `DS:0DAB` nonzero, `2B1B:000A`, overlay 182
`+15D7` and `+1167` (FND-PARTY-029); the routines that set `DS:13FB` to 1, overlay 173 `+3CDA`,
overlay 192 `+03AC` and overlay 204 `+0169` (FND-PARTY-031); `3D72:120B`; and the 14 sites of
FND-PARTY-036.

1. **Start-up records.** Starts: the seven start-up record targets of FND-PARTY-039. No control.
   76 routines; no target is reached.
2. **Round 0.** Starts: the 40 of FND-PARTY-037, plus the values FND-PARTY-039 gives for the
   start-up and exit records, the Ctrl-Break handler, the `1000:09F4` putters, `DS:398C`,
   `1BF3:1154`, the `1BF3:61B8` table and `DS:3676..367E`: 66 starts. Controls as in
   FND-PARTY-037. The other values of FND-PARTY-039 are written by code; their writing
   instructions are added as 39 more targets (`0x1B9DF`, `0x583C0`, `0x597E7`, `0x5FDB8`,
   `0x67250`, `0x6725B`, `0x683AF`, `0x69A3F`, `0x69A8F`, `0x69F8D`, `0x69FA1`, `0x6A31D`,
   `0x6A331`, `0x6D973`, `0x73BC0`, `0x7945A`, `0x794C5`, `0x7BACB`, `0x7BC32`, `0x7D5CE`,
   `0x81601`, `0x87FB7`, `0x880D6`, `0x88155`, `0x88431`, `0x884EE`, `0x8987E`, `0x8A80D`,
   `0x8A9BA`, `0x8B2FA`, `0x94423`, `0x95D3`, `0x96200`, `0x9651B`, `0x96EE`, `0x9828`,
   `0x988D3`, `0x98ABB`, `0x990E`). The opcode table at `DS:030A` is left out. Both controls are
   resolved. 775 routines and 46,459 instructions; no gap from the limit. None of the 29 targets
   is reached. Of the writers, these are: the four `DS:3F42` writers, `DS:02F6`'s (`0x73BC0`),
   `DS:3475`'s and `DS:3479`'s calls (`0x6725B`, `0x67250`), `DS:3486`'s (`0x1B9DF`), the call
   at `0x81601` that passes overlay 194 `+0000` to the `DS:A0F1` writer, and the call at
   `0x884EE` that passes `28C9:0CFF` to it. Every route to `0x884EE` passes overlay 180 `+0141`,
   overlay 188 `+046C`, overlay 169 `+0000`, `172C:000C`, `172C:00A1`, overlay 188 `+1901`
   (the target of trampoline `5702:00B1`), overlay 199 `+0C21` and `+0BC1`. A second run
   (`q011_values_round0b.json`) adds eight starts for transfers round 0 leaves unresolved: the
   overlay manager's near routines `4AE5:1257`, `4AE5:1155` and `4AE5:0EA2` (the start values
   of the manager's words `0x82` and `0x84`, with `DS` = `55CE`, and the values `4AE5:0BBA` and
   `4AE5:09B9` store there), `1000:02FC` (its word pair `0x86`), and `1BF3:659B`, `1BF3:65DA`,
   `1BF3:65BE` and `1BF3:6600`, the four words at `1BF3:6393`. It reaches 783 routines and the
   same targets and writers.
3. **Round 1.** Starts: round 0's, plus the values of those writers: `2D40:37F3`, overlay 180
   `+0000`, `49D2:0017`, the four `2660` routines, the twelve `DS:3F42` values, overlay 194
   `+0000` and `28C9:0CFF` (87 starts). The walk stops at the instruction limit (a gap at
   `0x0005C334`). It reaches `0x000271E3`, `0x00027404`, overlay 173 `+3CDA`, `2C5F:0CBF`,
   `2C5F:0CF1`, `0x000777F7`, `0x0009106C` and `0x000910C6`, each with a fewest-call chain that
   starts at `28C9:0CFF`.
4. **Opcode table.** Starts: round 0's, plus the 115 routines of the table at `DS:030A`. The walk
   stops at the instruction limit. It reaches 15 targets, among them all eight count stores and two of the three
   `DS:0DAB` routines, each with a fewest-call chain that starts at one of the handlers
   `172C:0F84`, `172C:1A43` and `172C:0F79` (opcodes `0x22`, `0x5E` and `0x24`).
5. **Each handler alone.** For each of the 115 routines, a run with that routine as the only
   start and the 29 targets, with no control; none stops at the limit. 32 routines, each for one
   opcode, reach at least one target: opcodes `0x08`, `0x0B`, `0x0D`, `0x1A`, `0x22`, `0x24`,
   `0x25`, `0x2A`, `0x2B`, `0x2D`, `0x2F`, `0x32`, `0x35` to `0x3C`, `0x42`, `0x43`, `0x44`,
   `0x48`, `0x4F`, `0x50`, `0x51`, `0x54`, `0x5C`, `0x5E`, `0x62` and `0x80`. 30 of them reach
   `0x000271E3` and `0x00027404`, and 31 reach `2C5F:0CBF` and `2C5F:0CF1`.

**The script call.** Overlay 188 `+046C`, when the byte at `DS:193D` is 0, calls overlay 169
`+0000` with `0xC8` and `0x2710`. That routine calls `172C:000C` with 99, 0 and 2 at `+0142`.
FND-CONFIG-160 reads this as the call that runs `MAS` 99, and `172C:000C` runs it through
`172C:00A1`, which reads each opcode with `172C:20F5` and calls `172C:018F` with it until the
script's frames unwind or it stops (FND-SCRIPT-007). `172C:018F` calls the table entry the
opcode selects (FND-PARTY-039). The other direct call of `172C:018F`, at `172C:3358`, follows a
`jmp cs:[bx+0x35A3]` at `172C:3327` that dispatches on a value from `0x80` to `0xE2`, and passes
the next byte `172C:20F5` reads.
`5702:00B1` is the routine `172C:0299` calls when the loader returns 0 (FND-CONFIG-160).

**Left unresolved in round 0.** 69 indirect transfers: the 61 of FND-PARTY-037 and eight more.
`jmp [0x3916]` at `0x7B8A` is `_setargv`'s return to its caller (FND-PARTY-038). `lcall [0x1154]`
at `0x17303`, `0x1731C` and `0x173E6` are in three of the `1BF3:61B8` table's routines, which
`1BF3:6199` calls after `1BF3:615B` has stored the pointer. `call [0x84]` and `call [0x82]` at
`0x401F9` and `0x401FE` follow `push cs` and read the manager's words, and `lcall [0x86]` at
`0x4041B` reads `1000:02FC` (FND-EXE-563). `call [di+0x6393]` at `0x1767E` indexes four words
with the `DS` that `1BF3:6492` restores, which was read here as `CS`, as for the variables at
`0x1166` and `0x115C` around it, but not traced to its callers.

## Interpretation

Under the report's assumptions, and with the pointer values FND-PARTY-039 enumerates, the only
routes from the places FND-PARTY-031 starts from to a routine that changes the count, sets
`DS:0DAB` or makes the pointer an image other than an `ICON` pass through the run of `MAS` 99:
either an opcode whose handler leads there, or the loader-error routine that leads to
`28C9:0CFF` being stored at `DS:A0F1`. Whether the gate is passed with the count at 0 therefore
turns on which opcodes `MAS` 99 executes, with which operands, and whether its load fails. A
handler that reaches a target in a static walk may reach it only for some operands.

## Alternatives

- One of the 29 targets is reached without the script: not ruled out; the 8 calls through
  record fields of FND-PARTY-039 stay unenumerated, and the `DS` of `call [di+0x6393]` was not
  traced.
- A route opened by round 1's values other than `28C9:0CFF`: not ruled out; round 1 stopped at
  the instruction limit.
- The script run happens after the gate: the routes found start from the routines FND-PARTY-031
  takes to run before it, so each is a route, not an observed order.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in the repository root, for each query `<q>` in
`tools/research/exec-census/` (`q011_startup_reach.json`, `q011_values_round0.json`,
`q011_values_round0b.json`, `q011_values_round1.json`, `q011_opcodes_round0.json`): `python -I
tools/research/exec-census/reach_config.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
tools/research/exec-census/<q>` and then `python -I -m scientific_method_engine reach <config>`.
For each handler run, replace the starts of `q011_startup_reach.json` with `172C:` and the
handler offset. Then, in `tools/research/exec-census/`, run `overlay_listing.py <dsun> 188
0x73300 0x73330`, `overlay_listing.py <dsun> 169 0x57690 0x576D0`, `resident_listing.py <dsun>
172C:000C..172C:0100 172C:0100..172C:0190 172C:3300..172C:3360`, `direct_callers.py <dsun>
172C:018F 169+0000 172C:000C` and `trampoline_target.py <dsun> 5702:00B1`.
