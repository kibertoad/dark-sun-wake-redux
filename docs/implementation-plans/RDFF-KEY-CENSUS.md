# RDFF kind-five first-word census

Research tooling for Q-SCRIPT-002 and Q-ACTOR-003. FND-SCRIPT-034 locates
kind-five field zero at record offset zero; its meaning is unresolved.
FMT-ACTOR-002 and FMT-PARTY-006 describe the first RDFF chunk. This tool
tests a resource-number relationship, without assigning a semantic field name.
There is no player-visible gameplay change or derived asset-pack revision.

Add `rdff-key-census ARCHIVE CONTROLS_JSON` to the existing Inspector,
implemented separately from Program.cs and using its existing GffArchive API.
Do not introduce a second GFF directory/index reader. Bound source size to
32 MiB, controls to 64 KiB, selected RDFF resources to 4096 and per-resource
size to 1 MiB. Verify explicit source SHA-256 before archive interpretation;
record source/config identity and deterministic sorted metadata in the report.

Controls require an exact source hash and explicit first-kind counts for
kinds one, two and five. Counts must be nonnegative, total positive, and
kind-five count positive. Reject unsupported fields and unknown first kinds.
Every RDFF first header must be type one with a bounded data span; expected
widths are 23, 49 and 23 for these kinds. For kind five, require the next
complete ten-byte header to be its end marker and retain trailing-byte counts,
including the known RDFF/500 case. Do not parse trailing bytes as extra chunks
or claim full-chain validation for other kinds.

For every selected resource report its identifier, signed first data word,
exact equality to the negated full resource number, separate low-word equality,
and trailing-byte count. Keep full-width and low-word comparisons distinct.
Report aggregate controls and matches/mismatches; do not print strings or raw
payloads. A data match alone does not establish native use, uniqueness or live
key admission. Reports derived from the original remain under GAME_DIR.

Acceptance:

- Synthetic tooling tests cover multiple kinds, exact negative identity,
  unequal words, a low-word-only alias, sorted output and trailing bytes.
- Truncated header/data/end marker, wrong type/kind/width, duplicate identifiers,
  resource/count/size limits, vacuous or missed controls, and malformed controls
  fail with a diagnostic rather than partial successful output.
- Source-hash mismatch is rejected before archive parsing. Synthetic test input
  needs no licensed file and any fixture files go in tests/fixtures/synthetic.
- Focused tests and Test.ps1 pass. The tooling commit changes no game spec,
  queue, parity, gameplay code or evidence status. A later research batch
  verifies the installed fingerprint, repeats reports and records conclusions.

Exit: the bounded command, documented limitations and meaningful synthetic
controls are committed as tooling after the full gate. Unknown relationships
remain unknown until the separate research batch consumes actual results.
