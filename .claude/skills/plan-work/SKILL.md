---
name: plan-work
description: Plan restoration work - record the project's stage, write or revise slices with checkable exit criteria, seed and reprioritise the research queue from the spec and coverage reports, and write /goal conditions and goal files. Use when starting a project stage or slice, when the queue or plan looks stale, or when asked what to work on next over days rather than minutes.
---

# Plan work

The rules are in the work protocol's [Stages](../../../vendor/upstream/work-protocol.md#stages) (lines 32-82),
[The queue](../../../vendor/upstream/work-protocol.md#the-queue) (lines 84-123) and
[Coding agents and long-running goals](../../../vendor/upstream/work-protocol.md#coding-agents-and-long-running-goals) (lines 482-533).
Open a linked section only when a step leaves a question it answers, read
only the lines the link gives, and never a section already read this session.
Planning changes `docs/IMPLEMENTATION-PLAN.md`, `queue/` and `docs/goals/`,
never code or spec entries apart from new `unknown` entries.

## Stage and slices

1. Establish the stage (Intake, Runtime access, Survey, Slices, Audit) by checking
   each earlier stage's exit criteria in the protocol against the repository.
   Record it in the plan. Do not move the stage forward on a criterion you
   could not check.
2. Each slice in the plan names the spec areas or entries it needs and the
   parity rows it must bring to `implemented`, `deviated` or `validated`, and each target
   is one `docs/RUNTIME.md` makes reachable: without runs of the original,
   format rows whose entries list files can reach `validated`, but rule and
   screen rows, and formats with no files (memory structures, messages), stop
   at `implemented`, and a row that needs a live session nobody has accepted
   stays at `implemented` with the gap in the risks. The first slice gives the
   rebuild a headless runner that a test drives from a fixture. Exit criteria
   are statements a script or reviewer can check, naming queue items by ID. Put questions only the owner can
   answer in the plan's owner questions, and nothing else there.
3. Remove anything from the plan that is status narration, a dated
   checkpoint or a list of what was done. Git has it.

## Seed the queue

1. For every spec entry below `established` whose Open questions has something
   workable, and every `unknown` entry, make sure a queue file has an item,
   under the section for the evidence it needs, in the area of the first entry
   it names, with an ID from that file's `Next ID:` line. A question a static
   reading can settle goes under `Static`; a call of one function goes under
   `Emulated call`; a run of the game goes under `Live session`,
   since `docs/RUNTIME.md` says no agent runs the game here. A `supported` entry gets a
   `Static` item to complete its reading and an `Emulated call` item where
   the harness reaches its functions, or, only where it depends on something
   the code does not decide, a run item to confirm it. Every rule the code
   decides gets an `Emulated call` item, since its fixture is what a
   `validated` row's tests replay. Items duplicating one another are merged,
   and
   the merged item keeps one of their IDs.
2. During Survey: until every path of the installation's listing is in the
   manifest or the build's Other files, the plan says what is still missing
   and Survey stays open. Every file the manifest lists as `data` without a format
   entry gets an `unknown` format entry and a queue item (CD audio tracks need
   none); so does every screen the manual mentions. Where function
   inventories exist in `coverage/`, run `npm exec -- standard-coverage`
   (`--list` names the entries citing each function): a large function no
   entry cites gets a queue item against the nearest entry or a new
   `unknown` one. Its figures are printed when wanted and never committed;
   the plan names the command and copies none of the shares it printed. The protocol's
   [Measuring progress](../../../vendor/upstream/work-protocol.md#measuring-progress) (lines 423-476)
   says how to set an early baseline and report coverage by area.
3. Move items that block the current slice to the top of their section.

## Goals

A goal condition names a state the agent can show by running something,
names its scope, and has a turn limit, for example:

```text
Every item under Static in queue/COMBAT.md is closed or moved to Blocked with
what was tried, the documentation check passes on the last commit, and each
batch ended with a status block; or stop after 40 turns.
```

Write the goal file from `docs/goals/README.md`. Where sessions can push to
main, check the goal files there for overlapping claims, and get the file and
each scope change onto main before the first batch that relies on it; where
work lands through pull requests, merge the claim's separate pull request first.
Do not push against an owner's instruction.

Where sessions cannot push to main, run `git branch --list 'goal/*'` at
session start and again before starting or resuming a goal. Inspect each
listed branch's tip for its goal file. Only one goal runs in the shared
clone, even across different areas. Resume that goal on its branch in its
own worktree. If none runs, start `goal/<name>` and make its first commit
create `docs/goals/<name>.md`; list the branches again after that commit.
If another branch now has its goal file, delete the just-created branch and
start no goal. The branch tip holds the claim; a merged copy on main claims
nothing, and deleting the file at the branch tip ends it. A separate clone
or cloud container starts no goal unless the owner says none is running;
fetching remote-tracking branches does not make local branch discovery work.
Creation, scope-change and deletion commits are separate from batches, change
only `docs/goals/` and `docs/HANDOVER.md`, and carry no trailers. Use a separate
worktree or branch for each session and keep its handover in its goal file.
Give the user the condition to paste after `/goal` when proposing a new goal.
Split research and implementation into separate goals. An implementation
goal's condition allows no change under `spec/` beyond added open questions
and `unknown` entries, and accepts a `partial` row only with a `Spec gap:` note. Never write a goal
like "finish the combat system": nobody can check it.

## Required runtime parts

When creating or moving a run item, name each capability part its Settles it
needs. Use only parts whose answers in docs/RUNTIME.md admit that work; an
unavailable or unverified required part blocks the item. Person-only parts
require an owner live session. Per-part answers do not override the prohibition
on agent DOSBox operation, attachment, captures or run-lock acquisition here.
