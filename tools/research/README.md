# Research drivers

Scripts that earlier sessions ran to accept toolkit reporters and to settle
research questions, kept here so the audit documents in `docs/` can name the
exact queries, declared regions, call models and assertions behind each result.
They are grouped by the engine release they were written against (`engine73`
to `engine120`) or by the review they belong to.

They are records, not part of the gate: `tools/Test.ps1` does not run them, and
a newer engine may reject an older configuration. Most read configurations from
`GAME_DIR/analysis/reporter-audit/` and write reports there, since a
configuration or report made from the original stays in `GAME_DIR`
(documentation standard, How to reproduce). Some read the output of an earlier
driver. Run one from the repository root with the locked tooling installed
(`./tools/Restore-ToolDependencies.ps1`) and `GAME_DIR` pointing at the
supported build; a driver stops with an error when `GAME_DIR` is unset. On a
machine where `GAME_DIR` is set for another project, set it for the run.

A finding never depends on one of these scripts alone: it states every value
its result depends on.
