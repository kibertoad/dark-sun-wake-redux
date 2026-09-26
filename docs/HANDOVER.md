# Restoration handover

## Branch and integration state

Work is on the branch `spec-migration`, open as pull request #1 against `main`.
Consult `git status`, the remote refs and the pull request's checks rather than
assuming this document proves push or CI state.

The repository is a configured Dark Sun: Wake of the Ravager restoration. The
approved `docs/IMPLEMENTATION-PLAN.md` remains authoritative. Slice 3 is active
and incomplete; later slices are not started.

## What this session did

- Moved every legacy record into `spec/` under the documentation standard: 23
  areas, their rows in `parity/`, their open questions in `queue/`, and the
  owner's runs in `docs/live-sessions/`. The legacy ledgers are gone, and
  `PARITY.md` and `deviations/` replace them.
- Recorded the deliberate departures `DEV-INPUT-001`, `DEV-EXPLORE-001`,
  `DEV-EXPLORE-002`, `DEV-TALK-001` and `DEV-UI-001`, and the new `unknown`
  rule `RULE-EXPLORE-005` with `Q-EXPLORE-006`.
- Added the launch options screen of the plan's "Launch options" section, with
  Wide map view (`DEV-EXPLORE-001`) as its first option.
- Made CI pass on hosted runners: the Actions audit, the installer's
  `/NOEXTRACT=1` switch, and software OpenGL for the installed smoke test.

## Next work

Research, each by its queue item or live session request:

1. Preferences ranges, defaults and About presentation: `Q-CONFIG-001`
   (`docs/live-sessions/preferences.md`) and `Q-CONFIG-002`, for `SCR-UI-007`.
2. The shipped START GAME party: `Q-PARTY-001`
   (`docs/live-sessions/shipped-party.md`), `Q-PARTY-005` and `Q-PARTY-009`.
3. Map movement and view: `Q-EXPLORE-001` to `Q-EXPLORE-006` and `Q-ACTOR-001`.
4. Conversation and scripts: `Q-TALK-001` and `Q-SCRIPT-006`; quest steps wait on
   `Q-SCRIPT-002` (`Q-QUEST-001`).
5. Opening combat: `Q-COMBAT-001` (`docs/live-sessions/opening-combat.md`).

Implementation gaps the parity rows record against evidence already in the spec:
keypad movement (`RULE-EXPLORE-004`), refusing key 6 in combat
(`RULE-EXPLORE-002`), footprint sizes and the 21-cell area (`RULE-EXPLORE-003`),
and the ranged-attack pointer pair (`RULE-INPUT-002`).

## Open owner decisions

- The opening leader's cell: the rebuild uses `(74,91)` and `FND-EXPLORE-003`
  gives `(74,93)`; `Q-EXPLORE-005` would settle it.
- Whether the F9 preview (`DEV-TALK-001`) should stay mandatory or move onto the
  launch options screen.
- The launch options screen has not been checked in a live window.

## Local-only content

`UserContent/` is the persistent ignored asset pack. Reuse it between batches;
refresh it only when extractor code or the pack contract changes. It must never
be committed or redistributed. Owned screenshots, decoded probes, and other
research output remain under ignored `analysis/original/`.

## Wrap-up gates

Before handing over any future batch, run `tools/Test.ps1`, build the solution,
verify the retained pack, and run the no-window content smoke when pack content
is applicable. After committing, perform the repository's orphan-process audit
and record only processes actually stopped.
