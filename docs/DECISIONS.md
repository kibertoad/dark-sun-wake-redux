# Decisions

Decisions the owner has made about scope and product that are not in the spec:
which editions are supported, what is out of scope, features outside the
parity matrix, release timing. Live sessions are answered in their request
files in `docs/live-sessions/`, and the tooling the work protocol asks for
needs no decision. Newest first. A decision that departs from the original is
also a deviation in `deviations/`. See the
[work protocol](../vendor/upstream/work-protocol.md#what-needs-the-owner) (lines 328-330).

Each entry is a `##` heading of the form `YYYY-MM-DD: what was decided`,
followed by the reason in a short paragraph and what it rules in or out.

When this file would pass 1,000 lines, its oldest entries move to a numbered
file in `docs/decisions/`, starting at `001.md`, filled up to the limit and
never changed after. This file keeps the newest entries and links to the
numbered files here. A decision is never reworded when it moves.

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
