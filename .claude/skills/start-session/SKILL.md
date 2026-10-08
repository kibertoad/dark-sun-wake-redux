---
name: start-session
description: Resume restoration work at the start of a session. Use before any research or implementation in this repository, when resuming, when running a /goal, or when asked "where were we" or "what's next". Reads the handover and goal files, checks the tree, runs the documentation check, and picks the next item.
---

# Start a session

The rules are in the [work protocol](../../../vendor/upstream/work-protocol.md#sessions) (lines 321-329).
Goal discovery follows [Coding agents and long-running goals](../../../vendor/upstream/work-protocol.md#coding-agents-and-long-running-goals) (lines 482-533).
Open a linked section only when a step leaves a question it answers, read
only the lines the link gives, and never a section already read this session.
This skill is the procedure; where they differ, the protocol wins.

1. Decide the session's side: research (with tooling that reads the original)
   or implementation (with tooling that runs the rebuild). A session never
   holds both.
2. Discover the authoritative goal claim before selecting the handover. Where
   sessions cannot push to main, including an owner's no-push instruction,
   run `git branch --list 'goal/*'` at session start and again before starting
   or resuming a goal. Inspect `docs/goals/` at each listed branch's tip,
   not just in the current checkout: a branch whose tip still has its goal
   file holds the one running goal. Resume it in its own worktree; start no
   second goal, even for different areas. A copy on main claims nothing.
   A separate clone or cloud container cannot discover these local claims,
   even after fetching; it starts no goal without the owner's statement that
   none is running. Where claims can reach main, check the goal files there.
   Under a goal read its whole file and Handover section. With no goal read
   `docs/HANDOVER.md`. For a new goal follow `plan-work` and
   `docs/goals/README.md`: publish its claim first where authorized, or make
   its first commit on `goal/<name>` create the file in the shared clone and
   repeat the branch listing afterwards. If another goal is present, delete
   the just-created goal branch and start no goal. Goal-claim commits are
   separate from batches and carry no trailers.
   An implementation session reads no research goal files.
3. Compare the handover with reality: `git status`, `git log --oneline -10`,
   the current branch, and any `wip/` branch the handover names. Anything
   uncommitted that the handover does not mention belongs to someone else or
   to a crashed session: report it and leave it alone.
   Each session works in its own worktree or branch and writes only its own
   goal's handover. Never repair a message by amending an unchecked shared
   HEAD; confirm the commit belongs to this session first.
4. Verify the pinned rules with `node tools/upstream.mjs verify`. Read only
   the local copy under `vendor/upstream/`, assumed current.
   Check for updates or refresh it only when the owner asks in the current
   task, following `docs/UPSTREAM-RULES.md`. Run the documentation check in
   `--check` mode as `docs/VALIDATION.md` ("Spec checks") describes. A failure
   on a clean tree is the first thing to fix.
5. Read the plan's stage and current slice in `docs/IMPLEMENTATION-PLAN.md`,
   and check `docs/live-sessions/` for a request the owner has accepted.
   Where `coverage/` holds function inventories, `npm exec -- standard-coverage`
   prints how much of each file the spec cites and which functions no entry
   cites; read the figures there and commit none of them. `spec/index/` and
   `PARITY.md` on a branch are as old as the main branch it last took in;
   `node tools/upstream.mjs docs --generate` writes current copies, which are
   read and not committed.
6. Pick the next work:
   - Research: first triage the open reports in `docs/reports/` that the
     goal may take (`triage-report`). Then turn every `Spec gap:` note in
     `parity/` that has no queue item ID yet into a queue item
     (`research-item` step 1). Then, from
     every section of the queue alike: what the goal names; items that block
     the current slice; items others depend on (RNG, main loop, save format,
     state structures); items where one piece of evidence raises the most
     entries; items with the cheapest evidence. Within each step `Static`
     items come first, then `Emulated call`, then runs of the game. A `Live session` item needs its own
     static attempt under `Tried:`, unless it asks for the run that confirms
     a static reading.
   - Implementation: first remove any code in `src/` that cites a finding or
     an experiment (`FND-`, `EXP-`), with the tests that exercise it: its
     entry was superseded because the mechanic does not exist. Then rows
     whose Notes start with `Defect (R-...)`, adding a test that fails without
     the fix (extracting branch logic in `Game` into `Core` first) and removing
     the note. Then parity rows of the current slice
     from the goal or the plan, whose spec status is at least `supported`. Do
     not open `queue/` or `docs/reports/`.
7. Say in two or three lines what you picked and why, then hand over to
   `research-item`, `implement-rows` or `live-session`.

## Claude goal marker

When this session runs in Claude Code under an authoritative goal claim, run
node tools/goal-run.mjs start <name> in its own worktree after discovering the
claim. The Stop hook uses that worktree's Git-local marker, never a tracked file.
Codex goal lifecycle is controlled by its active thread goal, not this Claude hook.
