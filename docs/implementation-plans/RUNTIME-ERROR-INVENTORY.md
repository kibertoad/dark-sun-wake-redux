# Runtime-error inventory boundary repair

Tooling for Q-EXE-033 uses FND-EXE-655 and FND-PARTY-343 to represent the
installed resident error entries and inline span. It changes analysis metadata,
not game behavior, original bytes or the reach query's return assumptions.

Use a fresh isolated copy of the already repaired handler snapshot. Export the
whole main-code region and the narrow 80-byte interval before mutation. A
source-MD5-guarded repair must require the exact old function body and a complete,
nonoverlapping partition, clear only that interval, define its data and decode
only the two declared code ranges. Require complete instruction coverage of
each new body, no changed memory bytes and unchanged unrelated function bodies.
Rollback any failed postcondition. Applying the same repair again must be inert.

Synthetic Ghidra controls cover source mismatch, malformed/overlapping/incomplete
partitions, a boundary cutting an instruction, rollback, unchanged bytes and
unrelated functions, valid application and idempotence. Synthetic fixture bytes
are invented; all licensed exports and projects stay under GAME_DIR/analysis/.

For the licensed acceptance case, require code bodies 02AD..02C4 and
02DF..02FD and data 02C4..02DF under modeled segment 1000. Measure the prior
instruction/data classes rather than assuming every byte of the old body was
an instruction. Duplicate post-repair exports must agree. Compare all old rows;
only the combined error row may change and the independently established entry
may be added. Preserve canonical project files and all other inventory rows.

Reconcile the measured rows into the existing joined installed inventory and
its provenance/region side files using the established export procedure.
Update affected ownership hashes and assess every plan. Keep edition scopes,
missing inventories and unassigned/shared rows visible. Do not extrapolate this
installed-source result to the disc executable or silently change reach roots,
noReturn declarations, stops, leaves or code regions.

Exit: synthetic controls, source-bound export comparisons, ownership assessments
and full integration pass; a reproducible committed procedure accounts for the
metadata change. No complete reading, parity validation or queue closure follows
from metadata repair alone. No new owner decision is needed for this tooling.
