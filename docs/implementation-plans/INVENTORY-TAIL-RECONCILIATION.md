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
  The bounded census reads the hash-guarded installed source and the existing
  MZ/FBOV descriptors (FMT-EXE-001 through FMT-EXE-003), validates TSV body
  ranges against their own overlay, and supports canonical file offsets or
  the existing mapped segment addresses explicitly. It linearly decodes each
  body's final range, not every branch. Reject malformed counts, overlaps,
  source mismatches and empty overlay selections. Synthetic cases cover
  prefixed returns, return-valued immediate bytes, conditional versus
  unconditional jumps, incomplete instructions, adjacent entries and gaps.
  Reports include the decoder/input identities, counts and every row's result;
  they remain local and provide no complete-reading or reachability claim.
- Reconcile through the existing join/staging checks. Explain all changed
  starts, body ranges and region accounting; refresh affected closure-plan
  inventory hashes only against the accepted regenerated files.
- Preserve explicitly evidenced manual decode rows when regenerating walk
  declarations, including when a walk temporarily emits the same range or
  later stops emitting it. Replace stale generated rows, keep evidence on
  exact matches, and diagnose conflicting owners, duplicate manual ranges
  and empty evidence. Synthetic tests exercise this without licensed inputs.

Exit: Q-EXE-032 closes only when its full boundary check and regenerated
inventories pass, evidence records the result and the repository gate passes.
A restored tool or a successful export alone does not close it. No owner
decision is required for this tooling work; supported editions and scope stay
as recorded.
