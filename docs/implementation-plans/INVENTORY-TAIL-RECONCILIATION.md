# DSUN inventory tail reconciliation

Tooling on the research side of the clean room, for Q-EXE-032 and
FND-EXE-633. The outcome is an executable research denominator that includes
the documented routine tails, without changing gameplay or promoting claims
from inventory coverage.

Use only the supported installed DSUN.EXE and the existing mapped-image,
declaration, clipping and metadata-export procedure in docs/EVIDENCE-TOOLS.md.
Keep licensed derivatives, static projects and reports in GAME_DIR. Restore
missing approved tooling locally; verify the pinned Ghidra release checksum
and its Java requirement before use. Never edit generated inventory rows by hand.

Acceptance:

- Verify the source and mapped-image identities and all overlay fixups before
  analyzing a replacement snapshot. Record tool versions and exact arguments.
- Follow the canonical declaration and flow-repair sequence on a disposable
  snapshot. Check script-level completion and failures as well as process exit.
  Preserve incomplete analysis and unexpected body changes as explicit gaps.
- Export each view twice with complete ranges, provenance and region partitions;
  require byte-identical metadata, entry and body consistency, and no unexpected
  entries without instructions. Review any clipping into fixup-table bytes.
- Verify the three FND-EXE-633 controls through their documented returns, then
  apply the boundary test to every overlay row. Retain legitimate adjacent
  entries, partial-instruction cases and unresolved transfers as distinct cases.
  Any new checker must have synthetic controls before it supplies a negative.
- Reconcile through the existing join/staging checks. Explain all changed
  starts, body ranges and region accounting; refresh affected closure-plan
  inventory hashes only against the accepted regenerated files.

Exit: Q-EXE-032 closes only when its full boundary check and regenerated
inventories pass, evidence records the result and the repository gate passes.
A restored tool or a successful export alone does not close it. No owner
decision is required for this tooling work; supported editions and scope stay
as recorded.
