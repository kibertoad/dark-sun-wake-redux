---
name: end-session
description: Close a work session on this restoration - stop processes the session started, rewrite the handover (docs/HANDOVER.md, or the goal file's Handover) to the current state, commit it, push, and report. Use at the end of every session, before handing over, when a goal is met or dropped, or when stopping for any reason.
---

# End a session

The rules are in the [work protocol](../../../vendor/upstream/work-protocol.md#sessions) (lines 321-329).
Goal completion follows [Coding agents and long-running goals](../../../vendor/upstream/work-protocol.md#coding-agents-and-long-running-goals) (lines 446-497).
Open a linked section only when a step leaves a question it answers, read
only the lines the link gives, and never a section already read this session.

Follow the local post-commit process audit; reusable MSBuild nodes are not
orphans and agents never touch DOSBox or take the original-game run lock.

1. **Processes**: stop every process this session started (Ghidra and Java,
   test hosts, servers), and leave anything whose owner is uncertain.
   Reusable MSBuild nodes are not orphans. Never touch DOSBox: the owner runs
   it. Follow the post-commit orphan-process audit in `AGENTS.md`, including
   its log. Sessions here never take the run lock (`docs/RUNTIME.md`).
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
4. **Handover**: under a goal, rewrite the Handover section of its goal file;
   otherwise rewrite `docs/HANDOVER.md` from its section headings. State what
   is true now: stage, the last `./tools/Test.ps1` result with its date,
   unfinished work (with its `wip/` branch), blockers, and at
   most five next items naming queue items by ID, parity rows or a slice.
   Get the branch, commit and remote sync state from Git when needed; do not
   copy them into the handover. Never write what research found or tried.
   Delete what is no longer true instead of adding below it. Stay under 200
   lines. Commit the handover on its own, with no trailers: it changes
   nothing outside `docs/HANDOVER.md` and `docs/goals/`.
   Confirm the worktree, branch, staged paths and current HEAD belong to
   this session before committing. Use a new commit for the handover.
5. **Push** the branch unless `AGENTS.md` says the owner pushes or the user
   has instructed otherwise. Check Git directly for the branch's remote sync
   state when reporting it; do not copy a count into the handover.
6. **Report** the final status block from `research-item`, followed by one
   line on anything the owner has to decide or do, such as a live session
   request waiting for an answer.
