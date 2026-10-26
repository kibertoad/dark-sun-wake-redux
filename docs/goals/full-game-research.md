# Full game research

## Condition

Fully research and map Dark Sun: Wake of the Ravager within the supported
edition and preservation scope in docs/IMPLEMENTATION-PLAN.md. Reach the
research Audit exit: every queue item is answered in the spec or explicitly
accepted by the owner as out of reach; account for all source files, executable
functions, rules, screens and campaign dependencies, retaining unresolved
inventory and mapping gaps. Documentation and required batch gates pass.
Partial coverage, supported parity rows and moved queue items alone do not
complete this goal. There is no turn limit.

## Scope

Side: research. Areas: all game specification areas. Static research first,
then admitted emulated calls and session-package runs under docs/RUNTIME.md.
Research tooling and metadata-only closure mapping support that work.
The owner restarted this goal on 2026-10-20; it runs on main in the
single checkout. Push only when asked.

## Must not touch

Gameplay implementation under src/; other sessions' work or history;
generated spec/index/ and PARITY.md; proprietary content in Git.
Runtime restrictions and source-edition decisions remain binding.

## Dead ends

Retain the per-question Tried records and docs/HANDOVER.md tooling limits.
Do not repeat a retained search without new evidence, tooling or reading.

## Handover

- Stage: Slices. Research continues toward the full Audit condition.
- Updated 2026-10-26 when the session wound down at the owner's request; the
  goal continues. No workers or emulator runs of this goal are left running.
- Last full gate: 2026-10-25, Test.ps1 -NoRestore passed on 36e0bb1c,
  including 715 .NET tests.
- Unfinished: none in the tree. Unfinished and blockers outside this goal's
  batches are in docs/HANDOVER.md.
- Owner's order for mapping: keep the installed DSUN.EXE inventory stable,
  place the uncited functions, then complete readings.
- Done this session: every uncited installed DSUN.EXE routine is placed
  (FND-EXE-606 to FND-EXE-631); FND-EXE-632 is a trampoline census;
  FND-EXE-633 and Q-EXE-032 record three overlay 204 inventory rows that
  stop short. Q-PARTY-112 closed (FND-PARTY-257). Q-RNG-025 narrowed to
  the two callers that set the cast byte (FND-RNG-064). Q-RNG-005 split
  into Q-RNG-005, Q-RNG-029 and Q-RNG-030; seven of FND-RNG-058's nine
  transfers resolved (FND-RNG-065, FND-RNG-066).
- Also done: the start window, Preferences, inventory and Split readings
  (FND-UI-059 to FND-UI-067); the edge scroll, the opening leader's cell
  and the Walk planner (FND-EXPLORE-008 to FND-EXPLORE-015,
  RULE-EXPLORE-005 now supported with its procedure); where
  `control_locked` is set and where Space reaches the key routine
  (FND-AI-005, FND-AI-006). New items: Q-EXPLORE-010 (the planner's
  unread callees), Q-AI-005 (Agent run: does an effect's end clear the
  button's setting). A play-ready Walk click fails because slot 1 has
  no position at that checkpoint (FND-EXPLORE-015); start Walk runs from
  a new game instead. During an edge hold the emulated mouse driver
  returned the injected column only once in 50 calls (FND-EXPLORE-009),
  which may be a rust-dos defect; nothing was filed.
- Also done, later: Q-EXPLORE-003 (FND-EXPLORE-017, FND-EXPLORE-018,
  FND-INPUT-016), how a walker moves between cells (FND-EXPLORE-019 to
  FND-EXPLORE-021), the timer library's slots and the game tick
  (FND-TIME-012 to FND-TIME-010), every script trigger list and its events
  (FND-SCRIPT-027 to FND-SCRIPT-029, RULE-SCRIPT-013), and what a saved
  game holds, the key callback and the exit cleanup (FND-SAVE-018 to
  FND-SAVE-024, Q-SAVE-001, Q-SAVE-004 and Q-SAVE-005 closed). New tools in
  `tools/research/exec-census/`: `segment_offset_refs.py`,
  `scmd_walk_frames.py`, `gff_byte_search.py`.
- Found: BUG-COMBAT-001. With `DS:426D` at 9999, which the end of
  combat leaves until a leader key (FND-RNG-069, FND-RNG-070), a view
  click sends slot 9999 to `2D40:2F34`, which writes a record number over
  the July and August month lengths (FND-RNG-067, FND-RNG-068) and fills
  record 9999 with `0xFF`. From then on `28C9:1EDB` runs overlay 173
  `+059A(0, &DS:426D)` on every pass: a combat round step adds 60 to
  `4C13:035B` about 15 times a second, and the leader keys are refused
  (FND-RNG-074 to FND-RNG-077, FND-COMBAT-056, FND-COMBAT-057). Each
  pass is read (FND-COMBAT-060 to FND-COMBAT-062); F1 or F2 taken in the
  short gap after a pass's input flush, then EXIT or a save, ends it
  (FND-COMBAT-063, FND-COMBAT-064). Open: Q-COMBAT-037 (load, leader key
  after), Q-RNG-031, Q-RNG-037 to Q-RNG-039 and Q-RNG-034 (other routes
  to 9999). Also found: BUG-SAVE-001 (F3, Save, EXIT leaves `DS:627C` at
  1, so the next save or load quits to DOS; FND-SAVE-025).
- Later runs: walk timing in the first fight (FND-EXPLORE-022), no read
  of `4E71:0000` (FND-EXPLORE-023), grey Up (FND-INPUT-017), `wait_ms`
  at 1,000 µs with no music timer slot (FND-TIME-011, Q-TIME-005 open).
  Spell counts and the cast window read (FND-MAGIC-002, FND-MAGIC-003,
  RULE-MAGIC-001 supported). Run drivers in `tools/research/rust-dos-runs/`:
  `pass_counts.py`, `pass_bins.py`, `input_watch.py`, `walk_drive.py`,
  `bug_exit.py`, configs in `counts/`.
- Latest session, magic: spell casting, the cast window and memorisation
  counts (FND-MAGIC-002 to FND-MAGIC-024, RULE-MAGIC-001 to
  RULE-MAGIC-006); turn undead is DATA 306 from the TURN UNDEAD button
  (RULE-MAGIC-006, FND-EXE-634). Clock and timers:
  FND-TIME-012 to FND-TIME-014. Closed Q-MAGIC-006, Q-MAGIC-007,
  Q-MAGIC-018, Q-MAGIC-019 and Q-MAGIC-021; added Q-MAGIC-027 to
  Q-MAGIC-029.
- Latest session, combat: the attack routine, to-hit, damage, Armor Class
  and swings (FND-COMBAT-065 to FND-COMBAT-069; RULE-COMBAT-002, 003 and
  006 rewritten, parity Code partial with Defect notes), the difficulty's
  to-hit and hit point scaling (FND-COMBAT-070, RULE-COMBAT-007), mark 3
  and recovery (FND-COMBAT-071), and every field of the two combatant
  records with how records are chosen (FND-COMBAT-072 to FND-COMBAT-074).
  The rules use the new field names. Closed Q-COMBAT-002 and Q-COMBAT-005;
  added Q-COMBAT-038 to Q-COMBAT-043. Possible original bugs, unconfirmed:
  the two-weapon term is a bonus below Dexterity 6, and overlay 197
  `+287D` compares two locals only one branch writes (Q-COMBAT-041).
- Latest session, runs: the leader's walk outside a fight is timed
  (FND-EXPLORE-024, FND-EXPLORE-025; Q-EXPLORE-006 closed, Q-EXPLORE-014
  added for followers and pass length), and no read of `4E71:0000` over a
  walk, key steps and a save outside a fight (FND-EXPLORE-026). Only the
  first 6.9 s of a load were scanned: to finish Q-EXPLORE-012, run
  `counts/walk2-tload.json` from `D:/rust-dos-games/work/walk2/slprep:preload`
  with free memory (about 2.5 to 3 hours; one attempt failed when other jobs
  held most of the RAM). `walk_drive.py` now takes `out`, `trace_page` and
  `trace_count` from the config. Checkpoints `f3:won` (first fight won)
  and `exprep:explore` (play outside the fight) are in
  `D:/rust-dos-games/work/walk2/`.
- Runtime tools: `tools/research/rust-dos-runs/trampoline_census.py` and
  `indirect_targets.py` (armed breakpoints on indirect transfers, memory
  reads, write watchpoints, key and pointer inputs, and `--count` sites
  with callers and arguments, capped at 50 hits because each stop opens
  TCP connections and uncapped runs use up the ephemeral ports). Run them with
  `artifacts/evidence-python/Scripts/python.exe -I`.
- The owner asked whether runtime-first research belongs in the protocol
  or the template; an upstream issue or PR was offered and awaits their
  answer. File nothing upstream without it.
- Next:
  - Agent runs: Q-COMBAT-041 (the two-weapon term and `+287D` in play),
    the rest of Q-EXPLORE-012's load scan, Q-EXPLORE-014.
  - Static: Q-COMBAT-038, Q-COMBAT-040, Q-COMBAT-007, Q-COMBAT-042,
    Q-COMBAT-043, Q-MAGIC-023, Q-MAGIC-025, Q-MAGIC-027 to Q-MAGIC-029,
    and the other open Q-MAGIC items.
  - Q-RNG-031, Q-RNG-037 to Q-RNG-039 (copies and reads into `DS:426D`),
    Q-RNG-034, Q-COMBAT-037, Q-TIME-005.
  - Q-RNG-029 and Q-RNG-005. Q-RNG-030 is closed: the `2707` copies,
    the program's timer callbacks and the sound drivers' timer functions
    write none of the clock windows (FND-RNG-079 to FND-RNG-084).
  - Complete readings for RULE-RNG-001's functions.
  - Q-RNG-003, Q-RNG-007, Q-RNG-022, Q-RNG-024 to Q-RNG-027 and
    Q-PARTY-113.
  - Q-RNG-006 (Agent run) and Q-RNG-028 (Source).
