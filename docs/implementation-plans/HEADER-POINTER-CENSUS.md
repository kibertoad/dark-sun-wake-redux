# Header-pointer census support

Tooling for Q-EXE-033 and FMT-EXE-003. The existing fingerprint-checked
segment_table_readers.py counts 153 overlay fixup candidates that push a
header segment and trampoline offset, but does not emit their source sites
or following transfer. Add an optional candidate report while preserving
its default output. Reuse the same fixup and descriptor inventory.

The report must bound decoding to the source overlay, stop at the first
control transfer, and retain decode failures and exhausted windows. A
candidate is not a reached instruction or a proved callback argument.
Never infer a consumer across a jump, return or undecoded span. Original
contexts remain local in GAME_DIR. Synthetic controls cover truncation,
branches, calls, returns, invalid decoding and a transfer at the limit.
Exit: existing census totals unchanged; all 153 overlay push candidates
represented exactly once, with bounded unresolved cases retained; controls
and integration gate pass. This tooling closes no research obligation.
