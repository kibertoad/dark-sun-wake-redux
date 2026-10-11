---
name: end-session
description: Close a work session on this restoration - stop processes the session started, rewrite the handover (docs/HANDOVER.md, or the goal file's Handover) to the current state, commit it, push, and report. Use at the end of every session, before handing over, when a goal is met or dropped, or when stopping for any reason.
---

# End a session

The rules are in the [work protocol](../../../vendor/upstream/work-protocol.md#sessions) (lines 379-389).
Goal completion follows [Coding agents and long-running goals](../../../vendor/upstream/work-protocol.md#coding-agents-and-long-running-goals) (lines 542-640).
Open a linked section only when a step leaves a question it answers, read
only the lines the link gives, and never a section already read this session.

Follow the local post-commit process audit; reusable MSBuild nodes are not
orphans, and agents touch DOSBox only through the session package.

1. **Processes**: stop every process this session started (Ghidra and Java,
   test hosts, servers), and leave anything whose owner is uncertain.
   Reusable MSBuild nodes are not orphans. Close every session-package run this
   session started, which stops its DOSBox-X process and releases the run
   lock; never touch the owner's DOSBox. Follow the post-commit orphan-process
   audit in `AGENTS.md`, including its log.
2. **Goal**: if the goal's condition holds, or the goal is dropped, the
   handover commit in step 4 deletes its file in `docs/goals/`, says which in
   its message, and moves anything in its Handover still worth handing on (a
   blocker, a `wip/` branch) to `docs/HANDOVER.md`; the batch that met the
   condition left the file in place. If it continues, add any new dead end to
   its file. A session under no goal that finds a goal-deletion commit on the
   main branch merges it first and keeps what it added to `docs/HANDOVER.md`
   unless this session dealt with it.
   Where claims cannot reach main, delete the completed or dropped goal's
   file on its authoritative `goal/<name>` branch: this ends the claim even
   before the owner merges it. A stale copy on main claims nothing. Leave
   the branch available for the owner's merge; do not delete another goal's
   branch or rewrite another session's commits.
3. **Working tree**: every finished batch is already committed. For anything
   half done, finish it, discard it, or leave it out of the batch commits and
   describe it under Unfinished in the handover. Where the working tree does
   not outlive the session (a cloud container), commit the half-done work to
   `wip/<working branch>` and push it there instead. Never commit to the
   working branch anything that fails the documentation check or
   `./tools/Test.ps1`, or mixes two kinds of batch.
4. **Handover**: under a goal that continues, rewrite the Handover section of
   its goal file; for a goal met or dropped, make the deletion in step 2;
   otherwise rewrite `docs/HANDOVER.md` from its section headings. First
   merge any commit on the main branch that changed the file holding this
   session's handover: one that deleted a goal file and added to
   `docs/HANDOVER.md` (keep what it added unless this session dealt with it),
   or a research batch that moved citations of an entry it superseded. Where
   the file names a superseded entry, move that citation to the entries its
   `superseded_by` names. State what
   is true now: stage, the last `./tools/Test.ps1` result with its date,
   unfinished work (with its `wip/` branch), blockers, and at
   most five next items naming queue items by ID, parity rows or a slice.
   When the session stops, name the reason (the owner's request to wrap up or
   stop, all work in scope blocked, an owner decision, a turn limit).
   Get the branch, commit and remote sync state from Git when needed; do not
   copy them into the handover. Never write what research found or tried.
   Delete what is no longer true instead of adding below it. Stay under 200
   lines. Commit the handover on its own, with no trailers: it changes
   nothing outside `docs/HANDOVER.md` and `docs/goals/`.
   Confirm the worktree, branch, staged paths and current HEAD belong to
   this session before committing. Use a new commit for the handover.
5. **Push** the branch unless `AGENTS.md` says the owner pushes or the user
   has instructed otherwise. When the owner asked to wrap up, push the work to
   the main branch unless the request or `AGENTS.md` says otherwise; where
   work lands through pull requests, or the session cannot push to the main
   branch, push the working branch (and open or update its pull request)
   instead, as "Wrapping up" in `docs/goals/README.md` says. Check
   Git directly for the branch's remote sync state when reporting it; do not
   copy a count into the handover.
6. **Continue or stop.** If the session worked under a goal whose condition
   does not hold and none of the stop reasons in `docs/goals/README.md`
   applies, the session's end is a checkpoint: run `start-session` now and
   take the next item in the same conversation, without reporting or ending
   the turn. A finished batch, a committed handover, a long conversation, a
   check running in the background or a question from the owner (answer it
   and go on) is not a reason to stop. A standing goal that stops with work
   left keeps its file, except where goals take turns on `goal/` branches,
   where it ends as a dropped goal does. Otherwise run
   `node tools/goal-run.mjs stop` and go on to the report, naming the stop
   reason that applies. When the owner asked to wrap up, follow "Wrapping up"
   in `docs/goals/README.md`: no new item starts.
7. **Report** the final status block from `research-item`, followed by one
   line on anything the owner has to decide or do, such as a live session
   request waiting for an answer.

## Goal checkpoint and stop

A handover is a checkpoint while the goal remains active. Resume start-session
and the next available item; do not redefine completion around a finished batch.
When the owner requests wrap-up, start no new item, finish or document the current
batch and commit the separate handover. Preserve existing no-push instructions:
wrap-up alone does not revoke them. When an actual stop reason applies in Claude
Code, run node tools/goal-run.mjs stop and record that reason. Follow the active
thread's completion and blocked audits for Codex; a hook timeout is no proof of
completion or of a blocker.
