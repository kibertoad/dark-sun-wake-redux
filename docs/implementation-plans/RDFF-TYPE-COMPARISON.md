# Kind-four inline/shared-row comparison

Research tooling for Q-ACTOR-007. FND-ACTOR-015 identifies all 1216 known
kind-four chunks in the installed 68-byte RDFF resources. FND-ACTOR-017
binds record_ref to the fifteen-byte ALL/4 rows and independently checks
RDFF/600, reference 61, as an indexed payload match. FMT-ACTOR-002 records
the competing full-corpus readings. FMT-ITEM-002's field schema stays unknown.

Outcome: a researcher can compare every known inline payload with the indexed
shared row and retain exceptions and duplicate-row ambiguity. This changes no
game behavior, extracted-content contract or field decoder.

Add Inspector `rdff-type-census ARCHIVE CONTROLS_JSON` in a separate helper.
Reuse GffArchive for directory/index/resource bounds. Reuse the existing source
verifier through a hash-only overload; retain the old key command's count checks.
Controls explicitly require sourceSha256, resourceCount, inlineCount, typeRows,
controlResource, controlReference and controlIndexedMatch. Reject missing/extra
properties and vacuous or missed controls. The installed values are 1643,
1216, 102, 600, 61 and true, with the approved source hash.

Bound source to 32 MiB, controls to 64 KiB, resources and type rows to 4096,
each resource to 1 MiB, and total reported matching-row relations to 131072.
Failures produce a diagnostic and no successful partial report.

This is the known fixed-shape comparison, not a general RDFF parser. Select
size-68 resources and validate the complete known shape: type-one/kind-one
header with field zero and length 23; type-three/kind-four header at offset
33 with earlier target zero, field five and length fifteen; zero header byte
three in each; and a complete end header at offset 58. Compare bytes 43..58.
Count other-size resources explicitly as outside this model; do not validate
their chains or infer absence of other layouts from their size alone.

ALL/4 must have typeRows times fifteen bytes. For each selected payload report
its unsigned reference, in-range status, indexed equality and every matching
shared row. Preserve indexed matches, other-row matches, no-row matches and
out-of-range references separately. Identical rows do not establish uniqueness.
Record source/config hashes and deterministic sorted metadata; emit no bytes
or strings from payloads. Original reports remain in GAME_DIR.

Acceptance: synthetic tooling tests cover each relationship class, duplicate
shared rows, sorted output, exclusions, malformed/truncated shapes, size/count
and relation limits, missed controls and source-hash refusal. Existing key-census
tests also pass after source-verifier reuse. Any file fixtures belong under
tests/fixtures/synthetic; constructed records require no licensed source.

Exit: focused controls and Test.ps1 pass before a tooling-only commit. It
changes no game spec, queue or parity. A separate research batch repeats the
installed query, verifies the independent control and retains every exception
before recording conclusions about the competing readings.
