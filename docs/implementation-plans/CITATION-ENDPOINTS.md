## Citation endpoint audit tooling

Tooling outcome: a read-only Ghidra diagnostic checks explicitly supplied
half-open code citation endpoints against the current decoded listing. It
distinguishes aligned, interior, mapped-undecoded and unmapped endpoints;
an optional return requirement detects a span that stops just before a RET.
It reports addresses and classifications only. Listing agreement does not
prove source identity, interior coverage, reachability, callers or a complete
reading. Original-program queries remain local and do not alter analysis.

Acceptance: synthetic x86 controls include a six-byte branch, a one-byte RET,
an interior start/end, an aligned end omitting the required return, undefined
mapped bytes and an unmapped range. Invalid/reversed ranges and excessive
query counts fail explicitly. The synthetic harness may disassemble only its
named synthetic imported program. Exit: those headless controls, repository
validation and a bounded read-only source-query rerun pass; findings, statuses
and inventories are not changed by this tooling batch.
