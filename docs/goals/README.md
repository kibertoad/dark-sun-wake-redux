# Goals

One file per long-running goal while it runs, named after it:
`docs/goals/combat-static.md`. The
[work protocol](../../vendor/upstream/work-protocol.md#coding-agents-and-long-running-goals) (lines 482-533)
says how to write the condition. The files here are the list of goals running
(or, where sessions cannot push to the main branch, the `goal/` branches; see
below).

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

Areas: COMBAT. Batches: research only. Queue sections: Static.

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

`Scope` names the areas the goal claims. A goal takes up a queue item only if
every entry it names is in one of those areas, and adds an area to its scope
only while no other authoritative goal claim owns it. Where sessions can
push to main, the file and each scope change reach main before the first
batch that relies on them; where work lands through pull requests, the
claim's separate pull request is merged first.

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

`Dead ends` records tools and approaches that failed across the
whole goal, in a line or two each, so a resumed session does not repeat them;
what a research attempt tried on a question goes under its queue item's
`Tried:`. `Handover` holds what `docs/HANDOVER.md` holds, for this goal only,
and is rewritten at the end of every session under the goal. Progress is not
written here: the queue and the commits show it.

## Standing goals and run marker

An open-ended owner goal keeps its full condition across sessions. The handover
is a checkpoint; a finished batch, long conversation or pending check does not
complete that goal. Continue work when an available item remains. Honor an owner
request to stop or wrap up without starting another item. A wrap-up records the
remaining work and commits the handover; it preserves any existing no-push rule.
Goal completion and blocked status require the active thread's evidence audits.
The local no-push claim rules above remain authoritative; this does not authorize
a second goal in this clone or a push to main.

In Claude Code, start-session runs node tools/goal-run.mjs start <name> in the
claimed worktree. Its marker lives in that worktree's Git directory. The Stop
hook binds only an identified conversation; another worktree or conversation,
an absent goal, invalid input or missing session ID passes without consuming its
allowance. Three held-back stops with unchanged HEAD permit stops until another
commit. Explicit stop removes the marker; it does not delete the goal claim.
Codex's active thread goal manages its own continuation and status.
