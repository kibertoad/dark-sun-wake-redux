# Goals

One file per long-running goal while it runs, named after it:
`docs/goals/combat-static.md`. The
[work protocol](../../vendor/upstream/work-protocol.md#coding-agents-and-long-running-goals) (lines 550-648)
says how to write the condition. The files here are the list of goals running,
together with the `goal/` branches where some or all sessions cannot push to
the main branch (see below).

The batch whose work meets the goal's condition leaves the file in place, and a
later commit deletes it, moving what is still worth handing on to
`docs/HANDOVER.md` and leaving the rest of that file as it was. For the
session's own goal, met or dropped during the session, that is the session's
handover commit. Any other goal, such as one dropped between sessions or found
to have been met by an earlier batch, loses its file in a commit of its own. A
commit that creates a goal file, changes the areas it claims or deletes it is
not a batch: it changes nothing outside `docs/HANDOVER.md` and `docs/goals/`,
leaves the documentation check and the fast gate passing, and carries no
trailers. Git keeps the deleted file.

A goal file:

```markdown
# combat-static

## Condition

Every item under Static in queue/COMBAT.md is closed or moved to Blocked with
what was tried, the documentation check passes on the last commit, and each
batch ended with a status block; or stop after 40 turns.

## Scope

Side: research. Areas: COMBAT. Queue sections: Static.

## Must not touch

Other areas' entries, queue files and parity rows. `src/`.

## Dead ends

None known.

## Handover

- Stage: Slices.
- Last gate: 2026-09-25, `./tools/Test.ps1` passed.
- Unfinished: none.
- Blockers: none known.
- Next: Q-COMBAT-015, Q-COMBAT-017.
```

`Scope` names the goal's side of the clean room, research or implementation,
and the areas it claims. A goal takes up a queue item only if every entry it
names is in one of those areas, and adds an area to its scope only while no
other authoritative goal claim owns it. Another goal's claim counts only when
that goal is on the same side or its file names no side, so a research goal and
an implementation goal whose files name their sides may claim the same area.
Where sessions can push to main, the file and each scope change reach main
before the first batch that relies on them; where work lands through pull
requests, the claim's separate pull request is merged first.

`Dead ends` records tools and approaches that failed across the
whole goal, in a line or two each, so a resumed session does not repeat them;
what a research attempt tried on a question goes under its queue item's
`Tried:`. `Handover` holds what `docs/HANDOVER.md` holds, for this goal only,
and is rewritten at the end of every session under the goal. Progress is not
written here: the queue and the commits show it.

### Where no session can push

Where sessions cannot push to main, including an owner's no-push instruction,
only one goal runs in the shared clone. At session start, and again before
starting or resuming a goal, run `git branch --list 'goal/*'` and inspect
`docs/goals/` at each listed branch's tip. A branch `goal/<name>` whose tip
still has `docs/goals/<name>.md` holds the claim. Resume it in its own
worktree; start no second goal, even for different areas. If none runs, make
the new branch's first commit create its goal file, then repeat the listing.
If another listed branch now has its goal file, delete the just-created
branch and start no goal. A copy of a goal file merged onto main claims
nothing; deleting the file on its goal branch ends the claim before that
deletion is merged. Keep the branch for the owner's merge.

Worktrees of one clone share local branches; separate clones and cloud
containers cannot see the claims, even after fetching remote-tracking branches.
A session outside the shared clone's worktrees starts no goal unless the owner
says none is running. Each session uses its own worktree or branch and writes
only its own goal file's handover. Under no goal, merge a goal-deletion commit
found on main before rewriting `docs/HANDOVER.md`, keeping its additions unless
the session dealt with them.

## Standing goals

An open-ended owner goal keeps its full condition across sessions. The handover
is a checkpoint; a finished batch, long conversation or pending check does not
complete that goal. Continue work when an available item remains. Honor an owner
request to stop or wrap up without starting another item. A wrap-up records the
remaining work and commits the handover; it preserves any existing no-push rule.
Goal completion and blocked status require the active thread's evidence audits.
The local no-push claim rules above remain authoritative; this does not authorize
a second goal in this clone or a push to main.

A standing goal's file names its side, research or implementation. Its
condition is the state the Audit stage ends in, not a turn count: for research,
every queue item is closed or is one the owner has accepted as out of reach;
for implementation, every parity row is `validated` or `deviated`, or has a
note other than a `Spec gap:` note saying why it can be neither. Work that is
only blocked for now does not meet it. A goal with a bounded scope also stops
when it reaches the number of turns its condition gives. Where no session can
push to main, the two standing goals take turns: one that stops with work left
deletes its goal file and moves its handover to `docs/HANDOVER.md`, and its
side's next turn starts a new goal with the same condition.

### Where only some sessions can push

Where the owner authorizes a push to main in some tasks and forbids it in
others, a goal keeps the form the session that started it could use: its file
is on main when that task authorized the push, and on a `goal/` branch
otherwise. A goal file on main claims its areas for every session unless the
shared clone has a `goal/` branch of the same name, whose tip then decides.
Before starting a goal or adding an area, fetch and read `docs/goals/` at the
remote's tip (`git ls-tree --name-only origin/main:docs/goals/`) and run the
`goal/*` listing; check both again once the claim commit has landed, and
withdraw on a clash (delete the branch, or land a commit on main that deletes
the goal file, or only the added area). A goal resumed under a different
authorization keeps its form; a goal on main resumed without push
authorization takes only items in areas already claimed, and its batches and
handover stay on its working branch until they reach main.

### Run marker

In Claude Code, start-session runs node tools/goal-run.mjs start <name> in the
claimed worktree. Its marker lives in that worktree's Git directory. The Stop
hook binds only an identified conversation; another worktree or conversation,
an absent goal, invalid input or missing session ID passes without consuming its
allowance. Three held-back stops with unchanged HEAD permit stops until another
commit. Before a turn that ends waiting on a batch a subagent runs, or on an
answer from the owner, with no other item in scope to take meanwhile, node
tools/goal-run.mjs wait records HEAD in the marker. If HEAD is still there at
the next stop, that one stop passes without spending the allowance; the wait
ends there, or earlier at a commit or another start, and a later turn that
still has nothing to do runs wait again. Explicit stop removes the marker; it
does not delete the goal claim.
Codex's active thread goal manages its own continuation and status.
