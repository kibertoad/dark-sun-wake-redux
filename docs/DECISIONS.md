# Decisions

## 2026-10-10: Research the installed 1.1 game files

The owner decided that the restoration targets the installed files and that
game-rule research uses the installed `DSUN.EXE`. The disc copies of the eight
files replaced during installation are the older release and are excluded from
new findings, inventories, coverage figures, queue items and code. Earlier disc
comparisons remain historical and are not extended. Disc-only inputs used by
the installed game remain in scope; the source of `CHARSAVE.GFF` read at
runtime remains an open question. The exact file list and scope are recorded
in [SOURCE-EDITIONS.md](SOURCE-EDITIONS.md#disc-copies-are-not-studied).

## 2026-10-09: Exclude DOSBox from game-restoration research coverage

The owner clarified that DOSBox is an emulator on top of the game and is
not being reimplemented. Its functions do not count toward game research
coverage or complete-reading priorities. Retain existing host research as
historical context, but direct executable research to the game and its own
utilities. Distribution and launch configuration may still be documented
where needed to identify or import the licensed game.

Decisions the owner has made about scope and product that are not in the spec:
which editions are supported, what is out of scope, features outside the
parity matrix, release timing. Live sessions are answered in their request
files in `docs/live-sessions/`, and the tooling the work protocol asks for
needs no decision. Newest first. A decision that departs from the original is
also a deviation in `deviations/`. See the
[work protocol](../vendor/upstream/work-protocol.md#what-needs-the-owner) (lines 478-480).

Each entry is a `##` heading of the form `YYYY-MM-DD: what was decided`,
followed by the reason in a short paragraph and what it rules in or out.

When this file would pass 1,000 lines, its oldest entries move to a numbered
file in `docs/decisions/`, starting at `001.md`, filled up to the limit and
never changed after. This file keeps the newest entries and links to the
numbered files here. A decision is never reworded when it moves.

## 2026-10-08: Location corrections are made in place while nothing outside consumes the spec

No one outside this repository relies on its entry IDs yet, and no pull
requests are open, so a finding whose location needs correcting is edited in
place instead of superseded. The first use is FND-EXE-142, whose range
`0x004A2280..0x004A2282` checker 2.8.0 failed against the DOSBox inventory row
`0x004A0DE0`: that row's size counts the addresses of a body with gaps, so
start plus size lands on the first byte of the `jmp 0x004A18D2` the finding
describes. Its range now covers that `jmp` as well. This applies only until
the spec is relied on outside the repository; after that, corrections follow
IDENTIFIERS-7 and IDENTIFIERS-8.

## 2026-09-26: Every launch opens on a launch options screen, and Wide map view starts on

The rebuild's settings, the deviations that have one, are chosen on a screen
of the rebuild's own that opens before anything of the original's is shown.
Wide map view (`DEV-EXPLORE-001`) is its first option and starts on. The
screen is `DEV-UI-001`, and the plan's "Features outside the parity matrix"
describes it.

## 2026-09-21: Every screenshot in the DOSBox capture folder may be inspected

The owner authorized agents to read every screenshot in the configured DOSBox
`capture` folder. `docs/live-sessions/README.md` gives the terms: no copying,
committing or changing captures, and every semantic label still needs the
owner's confirmation.

## 2026-09-19: No new behaviour until the whole source corpus is extracted

Work on rules and interface meaning stopped until every supported source file
and resource was represented in the verified pack, so that later work reads
data the Extractor already preserves. Slice 2A was that gate, and
`AGENTS.md` ("Extract before extending behavior") keeps the rule.

## 2026-09-14: Grab-drag panning and a wider map view

The owner approved a held right-button grab-drag that pans the map
(`DEV-INPUT-001`), and a map view that fills a display of another shape
(`DEV-EXPLORE-001`).

## 2026-09-13: Routes use a planner of the rebuild's own

The rebuild does not reproduce the original's route planner, on the condition
`DEV-EXPLORE-002` states.

## 2026-09-12: Identity, supported edition and analysis source

The project is `DarkSunWakeRedux`, shown as *Dark Sun: Wake of the Ravager
Redux*. The supported edition is the owner's English GOG installation
(`BLD-GOG-EN-1.1`), and a later revision needs a manifest of its own. Agents
may always read that installation at `C:\GOG Games\Dark Sun 2` for evidence
work without asking again, using the executable identity `docs/GHIDRA.md`
records; if the path or fingerprint changes, they record the mismatch and draw
no edition-specific conclusion until the edition record is updated. The plan
approved on this date recorded that permission without a date of its own.
