# Bounded evidence tools

## Resident segment descriptor aliases

For Q-SCRIPT-019/024, the existing segment reference, offset-context and
displacement-writer reporters now search every descriptor row whose
resident paragraph matches the named segment. The previous first-row
selection could miss a positive control when another descriptor named the
same segment. `segment_aliases.py` shares this metadata selection; the
reference reporter retains its existing flags-0/1 restriction. Resident
relocation words remain separate from overlay descriptor words, including
when their numeric values coincide.

The synthetic addressing controls cover later aliases, view flags, no
match and numeric collisions. These changes do not extend the reporters'
instruction windows, establish candidate ownership, resolve computed or
split segment bindings, or prove live storage. Rebuild affected retained
reports and require their independent positive controls before using an
absence claim. Earlier missed-control reports remain rejected diagnostics.

## Overlay near-transfer pattern query

`tools/research/exec-census/overlay_near_patterns.py` serves Q-PARTY-136's
alternate-entry audit. Existing `direct_callers.py` searches calls, while
`inventory_boundaries.py` searches branches in resident inventory rows;
this query adds raw near call/jump/Jcc/loop patterns across explicitly named
overlay code regions. It excludes fixup data, far/indirect/table/dynamic and
operand-size-32 transfers. Inventory-start labels do not establish reachability.

Run it with the locked Python, installed DSUN.EXE, the installed inventory,
decimal-descriptor/hex-offset targets and one independently established
`--control DESCRIPTOR+OFFSET@FILE_SITE` per overlay. All controls must match
a decoded inventory instruction start before any result is printed. Failed
controls reject the query. Reports include source identity, normalized
inventory hash, module/distribution decoder versions, exact code bounds,
controls and exclusions. Keep reports in GAME_DIR. Synthetic tests cover
signed displacements, 16-bit IP wrap, truncated instructions, operand-interior
patterns and rejected controls; the tooling commit alone establishes no
game behavior or absence claim.

## Type-50 first-item Look inputs

`dotnet run --project tools/DarkSunWakeRedux.Inspect -- rdff-look-census ARCHIVE CONTROLS_JSON`
reuses the first-header/source controls of rdff-key-census below. It selects
type 50 only from validated first kind-one records and reports numerical
quantity/value/word-0x10/name inputs, resource sizes and named/unnamed totals.
The selection must be nonempty. The named predicate is unsigned name below
9999; it is not a complete Look or item-interaction simulation. Existing
bounds and source verification apply. Later chunks/items, runtime mutation,
provider identity and live classifier admission remain outside the model.

## Kind-four inline/shared-row comparison

`dotnet run --project tools/DarkSunWakeRedux.Inspect -- rdff-type-census ARCHIVE CONTROLS_JSON`
compares the known 68-byte RDFF shape with the opaque fifteen-byte rows of
ALL/4. It reuses GffArchive and verifies the source hash before interpreting
the archive. This does not decode FMT-ITEM-002's unknown fields.

Controls require `sourceSha256`, `resourceCount`, `inlineCount`, `typeRows`,
`controlResource`, `controlReference` and `controlIndexedMatch`, all explicit.
For the installed study the counts are 1643, 1216 and 102, with independent
RDFF/600 control reference 61 and indexed match true (FND-ACTOR-017).
Unknown properties, vacuous counts and any missed control refuse the report.

The model validates both known headers and the complete end-header extent.
Other resource sizes are counted as outside the model. It retains indexed
matches, matches at other rows, unmatched payloads, out-of-range references
and duplicate-row ambiguity separately. A matching payload does not establish
unique row identity, field meaning, complete reader coverage or native admission.

Bounds: source 32 MiB, controls 64 KiB, resources/type rows 4096, each resource
1 MiB, matching-row relations 131072. A limit rejects rather than truncates.
The report contains sorted metadata and source/config hashes, without payload
bytes or strings. Synthetic tests cover relationship classes, malformed shapes,
control and bound failures. Original-derived reports stay in GAME_DIR;
their interpretation belongs to a separate research batch.

## Kind-five RDFF first-word census

`dotnet run --project tools/DarkSunWakeRedux.Inspect -- rdff-key-census ARCHIVE CONTROLS_JSON`
uses the existing GFF reader and emits only source/config hashes, counts and
sorted resource metadata. Controls require `sourceSha256` (64 lowercase hex
digits) and explicit integer `kindOne`, `kindTwo`, `kindFive` first-chunk counts.
The total must be at most 4096, and kindFive must be positive. Unknown control
properties, a source mismatch or any missed count reject the census.

The command bounds the source to 32 MiB, controls to 64 KiB, each RDFF to
1 MiB, and resources to 4096. It checks first type-one chunks of kinds one,
two and five at widths 23, 49 and 23. For kind five it checks the immediately
following complete end header and reports trailing-byte extent without reading
trailing bytes as additional chunks. Other kinds' later chains are not validated.

The signed first word is compared separately with the full negated resource
identifier and its low word. Low-word aliases are not exact matches. A match
does not establish field meaning, native use or live admission. Synthetic tests
cover these distinctions, boundary refusals, missed controls and source hashing.
Keep all reports/configs derived from licensed sources in GAME_DIR and record
game conclusions in a later research batch.

## Offset-word candidate diagnostics

`python -I tools/research/exec-census/far_pointer_words.py DSUN_EXE SEG:OFF ...`
verifies the installed executable fingerprint and reports matching words.
MOV/PUSH immediate labels require a typed immediate operand and both queried
bytes inside its encoded immediate field. Matching memory displacements do
not qualify. Synthetic controls in `tests/evidence/far-pointer-words.test.mjs`
cover equal displacement/immediate values, byte/word/dword immediates, field
boundaries, relative branches and nonzero addresses without licensed input.

The six-byte backward holder search remains heuristic. Its decoded candidates,
raw far-call patterns and nearby segment words do not establish instruction
ownership, runtime admission or a complete census of computed calls. Keep
original-derived reports in GAME_DIR and identify the tool revision when
recording a research result; older rendered-text immediate labels need review
before reuse. Far-call and relocated-pair classifications are unchanged.

## Measured work baseline

### Runtime-error entry/data supplement, 2026-11-07

FND-EXE-655 and FND-PARTY-343 support the installed-source partition in
`tools/ghidra/dsun-runtime-error-boundary.json`. Follow the bounded plan in
[RUNTIME-ERROR-INVENTORY](implementation-plans/RUNTIME-ERROR-INVENTORY.md).
Run `ApplyEntryDataBoundary.java` with that JSON on a fresh isolated copy of
the handler snapshot, after its existing declared-entry repairs. This separate
partition configuration creates the additional entry; the general declared-entry
file is unchanged. Do not substitute ordinary entry creation for the partition.

The script requires the source MD5, exact original body and complete partition,
rejects enclosing transactions and rolls back failed postconditions. It preserves
memory bytes, the original function identity and every unrelated function body.
Reapplication is inert. Run `tools/ghidra/Test-EntryDataBoundary.ps1` with
`-GhidraHome` and `-JavaHome` for the pinned installations. Its invented fixture
checks nine rejection cases, rollback, valid application and idempotence. The
controls caught a nested FlatProgramAPI transaction bug before licensed metadata
was changed; both transaction levels now close explicitly. The final controls pass.

Retained licensed acceptance exports and logs are under
GAME_DIR/analysis/runtime-error-inventory-20261107/. Ghidra 12.1.3 used an
isolated copy of INST-handler-20261106; source MD5 is
90b09ee26f2bc3a8d20063712636241c and XXH3 is
e296af55ba2ecde7e77f555c90f33d0b. Export before and after with
ExportResearchBaseline.java, passing the output stem, source XXH3, snapshot ID,
exporter SHA-256, `initialized`, source MD5 and region, in that order. Use
1000:0000..1000:4259 and 1000:02AD..1000:02FD; duplicate both after exports.
The recorded exporter SHA-256 is
fd88d69d04118cfbfecf48f03b6768e8a0cc9dbf795a2b3786c5e6b3baa1adba.
Require completion markers: an initial export missing MD5 failed despite
headless exit zero and was replaced by the corrected before export.

The old 80-byte body becomes 23- and 30-byte bodies, with 27 data bytes.
The narrow region changes from 80 instruction bytes to 53 instructions and
27 data, with no undefined bytes. All 3222 unrelated program bodies and all
211 other main-region rows are unchanged. Main-region functions change 212
to 213, instruction bytes 16157 to 16130, data 74 to 101, and undefined
bytes stay 754. Duplicate exports agree. The joined inventory has 3232
functions and 484659 body bytes; prior repairs remain intact. Disc files
are unchanged. All four ownership plans require refreshed hashes.

Run `tools/ghidra/Verify-RuntimeErrorInventory.ps1 -AnalysisRoot` with the
retained acceptance directory and `-CanonicalProjectRoot` with
GAME_DIR/analysis/handler-inventory-20261106/. It checks duplicate exports,
source identity, precise row/classification changes, the committed join and
canonical project hashes. Acceptance reports `reconciled=True` and
`canonicalUnchanged=true`. No reach roots, regions, return assumptions or
negative-search conclusions change. This is mapping, not a complete reading.

### Declared handler supplement, 2026-11-06

FND-PARTY-293 supplies the source-bound 1000:2BDB..2C56 handler.
The entry is added to dsun-declared-entries.json. ApplyDeclaredEntries.java
on an isolated copy of INST-single-20261103b creates exactly one function
of 123 bytes, with 235 existing declarations and zero failures. The
canonical snapshot remains unchanged. Reports and the repaired copy are
under GAME_DIR/analysis/handler-inventory-20261106/.

Reproduce with the existing headless scripts and pinned Ghidra: export
1000:0000..4259 and 1000:2BDB..2C56 before applying the resident
declarations; apply ApplyDeclaredEntries.java with the committed declaration
file; export the main region and the narrow handler again, exporting the
handler twice to separate stems. ExportResearchBaseline.java receives the
installed XXH3, snapshot INST-handler-20261106, its own SHA-256,
mode initialized and verified source MD5 90b09ee26f2bc3a8d20063712636241c.
Require its completion markers and identical inventory/provenance/region
files for the duplicate narrow exports, not just headless exit zero.

The measured main-region function count changes 211 to 212; every old
body is unchanged. The region's instruction bytes change 16034 to 16157
and undefined bytes 877 to 754, with data bytes 74 unchanged. All 123
new body bytes are now decoded instructions, with no body outside the
region or entry missing an instruction. The joined inventory adds only
this row: 3231 functions and 484686 union-counted body bytes. Its existing
FND-PARTY-278 body repair and other region classifications are retained.
Current ownership hashes are refreshed and assessed separately; the new
handler remains unassigned there. This is mapping, not a complete reading.

The twelve `tools/research/exec-census/q007_dsun*census.json` profiles now
include this handler as a root and its interior callback at 1000:2C46 as an
instruction control. A root cannot serve as its own instruction control.
Regenerate each configuration with `reach_config.py DSUN.EXE INVENTORY_TSV
OUT_JSON QUERY_JSON`, then run the pinned engine's `reach OUT_JSON` command.
Keep configurations and reports in GAME_DIR/analysis/handler-census-20261106/.
The before/after base comparison uses the same repaired inventory and engine
19.10.0: 21 to 22 starts, 1362 to 1363 routines, 105588 to 105647
instructions, and 83 to 84 unresolved transfers. Both retain five gaps,
four contested sites and `negativeUsable: false`; neither hits its limit.
The new interior control passes and remains an unresolved computed call.
These are tooling acceptance measurements, not a research conclusion or a
comparison against the older engine used for historical findings.

`tools/ghidra/Measure-ResearchBaseline.ps1` runs the pinned Protocol's
denominator audit for the configured build. Supply `-GameDirectory`, an
outside-repository `-OutputDirectory`, `-GhidraHome` and `-JavaHome` from
`docs/GHIDRA.md`. It reuses the saved DOSBox reading project and imports missing
static snapshots. No original program is executed. Prepare `sources/` beneath
the output directory with the hash-verified CD `DSUN.EXE` as `CD-DSUN.EXE`,
`CHARTRAN-UNPACKED.EXE` using the Inspect tool's `unlzexe` command, and
`INST-MAPPED.EXE` / `CD-MAPPED.EXE` using `New-FbovMappedImage.ps1` on their
respective editions. The runner rejects mismatching shipped and unpacked hashes.

Each frozen snapshot is exported twice with `-readOnly -noanalysis`.
`ExportResearchBaseline.java` writes complete body ranges, provenance and
instruction/data/undefined region partitions, including counts outside recognized
bodies. Completion markers and byte-identical duplicate files are mandatory.
MZ initialized regions are an upper-bound envelope including data and padding;
overlay regions are explicitly mapped code ranges, and PE uses loaded executable
regions. Undefined bytes are uncertain, not proved undiscovered code.

Run `node tools/evidence/work-baseline.mjs <OutputDirectory>` to produce
`work-comparison.json` and `citation-coverage.json` beside the local audits.
The comparison separates discovery/boundary changes from research citations,
unions overlapping bodies, accounts for identical CD aliases, and reports
parity, data-format coverage, queue sizes and formal complete-reading availability.
Whole-file search citations can touch every function without reading its behavior.
Neither citation coverage nor container-format coverage is gameplay completion.

The SVIEW, PATCH, CHARTRAN and SOUND_DS inventories use repeated,
range-aware snapshots validated against their unchanged starts and sizes.
Their provenance and regions files accompany them. SOUND_DS was migrated
after FND-CONFIG-213 and FND-CONFIG-214 corrected the finding ranges that
ended on a function's last instruction. `migrate-inventory.mjs` hashes the
exporter source as the checkout holds it, so the recorded exporter revision
matches only a checkout with Windows line endings.
DOSBox is excluded by the owner's scope clarification in `docs/DECISIONS.md`;
its historical inventory is archived under `docs/host-analysis/` and the
baseline excludes it through `tools/evidence/research-scope.json`.
The disc aliases `CD:SOUND_DS.EXE` and `CD:CHARTRAN.EXE` now have
metadata inventories under `coverage/BLD-GOG-EN-1.1/@CD/`. Their inventory,
regions and provenance bytes are identical to the installed counterparts.
The build manifest records equal size and XXH3-128 for both shipped pairs;
CHARTRAN also records equal unpacked size/hash, matching its existing
unpacked-snapshot provenance. This reuses those validated snapshots rather
than claiming a new disc import, a second independent reading or new code.
Reproduce the association by checking those manifest identities, then copying
all three metadata files from each installed inventory to its `@CD` path.
Keep the original snapshot/tool/exporter provenance; do not relabel it as a
new analysis. Counts per manifest file include these duplicate associations;
unique binary understanding and accepted readings do not increase.

The two DSUN inventories
come from the reconciliation path below. It clips an overlay body only to the
code range that holds its entry, and only after a finding shows that nothing
reaches the removed part of that overlay's own fixup table; the clip log lists
every removed range. Local report output and
analysis projects stay in GAME_DIR; no generated report is committed.

The DSUN reconciliation path joins two views of each edition. For the
resident view, copy the resident snapshot (the plain MZ import, auto-analyzed)
to a new project. Write the overlay plan with `python -I
tools/ghidra/fbov_overlay_plan.py <shipped> <mapped image> <mapping JSON>
<plan.json>`, and go on only when it prints `mismatches 0`: every overlay's
fixed-up code then equals the mapped image's. On the copy, without `-readOnly`, run
`tools/ghidra/SetFbovTrampolineBodies.java <plan.json>`, then
`tools/ghidra/AddFbovOverlayBlocks.java <plan.json>`, then
`SetFbovTrampolineBodies.java` again, which must report no changed header. The
first gives each trampoline a function of its five bytes, its `INT 3Fh` with no
fall-through (FND-EXE-593), and removes the functions that decoded past it.
The second adds each overlay as a file-backed overlay
block at its mapped segment, writes the fixups, creates functions at the
trampoline targets and reanalyzes only those blocks, so resident functions that
only overlay code calls get their starts. Then run
`tools/ghidra/CorrectNearTransfers.java apply 55E8:0000 229`, which gives
each near relative call and jump the target CS:((IP + displacement) & FFFF)
in place of the one Ghidra 12.1.3 wraps at the instruction's linear 64K page,
drops computed-jump references that leave the jump's CS window, and removes
the functions only those wrong targets created (FND-EXE-597). Then run
`tools/ghidra/ApplyDeclaredEntries.java tools/ghidra/dsun-declared-entries.json
resident`, which removes the declared false starts, decodes their bytes again
from the routine that owns them and creates functions at the declared entries
(FND-EXE-594, FND-EXE-595, FND-EXE-597, FND-EXE-599), and then
`tools/ghidra/ApplyDeclaredFlows.java tools/ghidra/dsun-declared-flows.json
resident`, which decodes the declared runs of FND-RNG-058's walk, gives the
declared table jumps their targets and removes the other computed-jump
targets, and repairs the declared returns and fall-throughs (FND-EXE-597).
`python -I tools/research/exec-census/walk_declared_flows.py` writes the
flows file's `jumps` and `decode` lists from the walk report; `returns`,
`fallThroughs` and the `jumps` entries that name a finding as `evidence` (table
jumps the walk does not declare) are written by hand, and the script keeps them.
It also preserves manually reviewed `decode` rows with a nonempty `evidence`
string, including FND-EXE-633's three tails tested by FND-EXE-635. An exact
generated range/owner match retains the manual evidence; stale unevidenced
rows are regenerated. Conflicting owners and duplicate manual ranges fail.
FND-EXE-636 and FND-EXE-637 add three interior decode ranges and five internal
entry removals. The canonical overlay candidate reproduces 874 rows with a
fully explained range delta. FND-EXE-638 regenerates the resident input,
preserves every resident row and accepts the repaired join through staging.
Change those files, never the inventory,
to add or drop a start or a flow. Run `CorrectNearTransfers.java` once more,
then `tools/ghidra/FillInstructionBodies.java`, which adds to each body the
bytes of its own instructions that Ghidra left out because a call targets them
(FND-EXE-601). Export it with
`ExportResearchBaseline.java`, passing the plain import's resident regions
(the rows of its regions file as start..exclusive-end), which leaves the
overlay blocks out. For the overlay view, copy the
relocation-corrected overlay snapshot (revision 2 mapper) to a new project,
run `CorrectNearTransfers.java apply 55E8:0000 229 <mapping JSON> <shipped
DSUN.EXE>` (the mapping and the shipped file name the trampoline targets,
which it keeps as entries), `ApplyDeclaredEntries.java <entries> overlay
<mapping JSON>` and `ApplyDeclaredFlows.java <flows> overlay <mapping JSON>`,
`CorrectNearTransfers.java` again and `FillInstructionBodies.java` on it, then
`tools/ghidra/ClipBodiesToEntryRegion.java` on the copy without
`-readOnly`, passing the overlays' mapped code ranges from
`ReportFbovOverlayMap.ps1`. It removes from each function body the
addresses outside the code range that holds its entry, which FND-EXE-520
shows near flow cannot reach, and prints every removed range.
A removed range in the entry's own fixup table needs a finding that nothing
reaches it (FND-EXE-523 to FND-EXE-525 cover the current snapshots). Export
the copy twice with `ExportResearchBaseline.java` and the same regions, and
list entries with no decoded instruction with
`ReportEntriesWithoutInstruction.java` (read-only). Then
`node tools/evidence/join-overlay-views.mjs <config.json> <staging-directory>`
joins the resident view and the clipped overlay view. The config names
`build`, `manifest`, the shipped `source` with its `xxh3` and `md5`, the
`mapping` JSON, `resident` and `overlay` objects with each export's `-a`
`stem` and the `source` it read, and `expectedWithoutInstruction` with
`resident` and `overlay` lists of segment:offset entries. Resident rows keep
segment:offset starts; overlay rows become shipped-file offsets, and the
join rejects any overlay body range outside its entry's Code ranges row,
overlapping or duplicated bodies, changed counts and unexpected
entries without an instruction. It writes the inventory and its provenance
and regions files under the `@CD` path convention. Like
`migrate-inventory.mjs`, it hashes tool sources as checked out, so run it
from a checkout with Windows line endings.

The committed `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` was staged by this join,
with the trampoline, near-transfer, declared-entry, declared-flow and fill steps,
from the regenerated resident and repaired overlay views (FND-EXE-638,
FND-EXE-637), preserving the resident rows of FND-EXE-601.
`coverage/BLD-GOG-EN-1.1/@CD/DSUN.EXE.tsv` comes from the `CD-`
single-import resident view of 2026-10-10 and the `overlay-owned` snapshot of
2026-10-09, without those steps; it is a diff reference only. `standard-coverage`
accepts both.
Their provenance files name both snapshots and the exporter, clip and join
revisions. The installed inventory expects every start to have a decoded
instruction; the disc inventory expects `5662:00F7`, a trampoline start in an
overlay header that overlay code calls, to have none.
`report.mjs inventory-check` predates the `ranges` column and rejects these
files, so check them with `standard-coverage`.

`python -I tools/research/exec-census/inventory_tails.py <installed DSUN.EXE>
<inventory.tsv> [--view file|mapped]` reports a bounded linear tail census.
The default reads the committed file-offset overlay rows and explicitly skips
resident rows; `mapped` reads an overlay-view export using the established
mapped segment addresses. Source identity and MZ/FBOV code bounds are checked.
Malformed ranges, overlaps, empty selections and count/size limits fail.
The JSON includes every overlay row, decoder/tool/input identities and separate
return, unconditional-jump, adjacent-entry, other-body, unowned, code-end and
incomplete-decode classifications. It decodes only each body's final range:
this is neither reachability analysis nor a complete-reading declaration.
Keep source-derived reports in GAME_DIR, never Git. Use the project-local
locked Python explicitly when an inherited EVIDENCE_PYTHON names another checkout.

`node tools/evidence/migrate-inventory.mjs <views.json> <staging-directory>
<manifest-path>...` stages unchanged single-view inventories with full body
ranges. It verifies source XXH3/MD5, exporter SHA256, repeated exports, ordered
unique starts, unchanged starts/sizes, entry inclusion, body unions, region
containment and anomaly counters. It refuses annotated historical rows and
multi-view mappings pending explicit reconciliation. Check both exporter
completion markers in the original log and run the standard coverage checker
against staged files before replacement. Staging uses exclusive writes and
never replaces a committed file. Keep source/configs/logs local; only the
address/count inventories, provenance and region metadata are committed.

Requires Node.js 22 or later and the packages `./tools/Restore-ToolDependencies.ps1` installs. Run synthetic tests with
`node --test tests/evidence/evidence.test.mjs`; the canonical validation gate
runs them too. These tools read metadata and never run an original executable.
They implement Standard v1 conventions; report schemas are tooling interfaces,
not a new spec version. Keep reports/configs under ignored `analysis/original/`.

## Identity and locations

Run `node tools/evidence/report.mjs operand analysis/original/operand.json`.
A config names `source` relative to the config, its `xxh3` (the XXH3-128 hash the
build entry gives, as 32 lower-case hex digits), `loadSegment` (default 4096),
numeric `site` at the segment operand, and numeric `targetOffset`. Every command,
including the `x86-` ones, refuses a source with another hash and a config that
still names a `sha256`; the report's `sourceIdentity` repeats the `xxh3` it checked.
`node tools/evidence/xxh3.mjs <file>...` prints a file's xxh3 the same way.
The source may be an ordinary MZ or an MZ followed by a Borland FBOV envelope.
Other extended formats fail explicitly. Resident ranges include data: location
resolution alone does not establish an instruction or behavior.

The operand report preserves its raw value, declared relocation/fixup kind,
descriptor index, loaded segment:offset, shipped-file location and canonical
trampoline destination. It resolves pushed segments and segment loads as well
as call operands. An absent relocation stays unresolved. A finding cites the
build/file and the Standard's address or overlay `offset`, and describes the
mapping. File offsets are independent of analyzer view addresses.

Use `incoming` with numeric `target`, `limit` (default 100) and `controls`
(coverage controls: file offsets of known far-call sites to any target). It
examines all declared MZ relocations and FBOV fixups for far-call byte
candidates and canonicalizes segment aliases. A control need not call `target`;
it proves the search decoded real far calls in this domain, which is what makes
a negative result usable. The report lists each control with the canonical
target it resolved to. A missed or unresolved positive control fails. A capped report explicitly says truncated.
A call-byte candidate whose target cannot be mapped is listed under `unresolved`.
Candidates still need entry-based instruction verification. Near/computed calls,
unrelocated pointers and unresolved instruction boundaries remain excluded.
A zero result never proves absence outside this declared domain.

## Control flow and tables

In Ghidra, run `ExportBoundedFlow.java <entry> <limit> <ignored-output.json>`.
The script follows explicit flows from one entry and stops at other recognized
entries. Missing instructions and the limit leave edge targets for review.
It emits starts, lengths, flow kinds, successors, callees and analyzer ownership,
without code text or bytes. Addresses are Ghidra-view coordinates, not canonical
file offsets. Keep the import mapping and its provenance alongside this report.

Run `flow` with a config containing `graph`, numeric `entry` and `limit`.
The reviewer retains explicit overlapping starts, reports unresolved targets,
indirect edges, ownership disagreements and hardware/terminal boundaries, and
counts the union of reached instruction bytes separately from contiguous spans.
A local-path result does not establish input coverage, callee effects, loop
termination, hardware behavior or a complete Standard reading. Inspect those
conditions in the finding. The Ghidra hardware mnemonic warning is a lead;
manual review still covers architecture-specific I/O and memory-mapped devices.

Run `table` with source identity and a `table` object containing `start`,
`count`, `stride`, `countEvidence`, optional `limit`, and `fields` of
`name`, `offset`, `width` (1, 2 or 4). It reads only unsigned numeric fields
within the declared layout. Record the input-to-index transformation and
range checks separately before assigning semantic dispatch cases. No ASCII
fallback reads into neighboring target data.

## Inventories from multiple views

Export each relevant Ghidra view's functions to a local TSV with the header
`start\tsize` and one row per function: its start as the view shows it and its
body byte count. Leave out analyzer names, code and strings; `inventory` refuses
any other column. Raw segmented exports need the same documented load mapping
as the resolver. For a different overlay import, first convert its starts to
`0x` file offsets using that import's documented map.

An `inventory` config includes source identity, `build`, `manifest`, and
`views`. Each view has a distinct `name`, an input `path` and explicit ownership
`ranges` with numeric inclusive `start` and exclusive `end`. Ranges must lie in
the reader's mapped source regions, and no two views may own overlapping ranges.
The report counts accepted and excluded rows per view and rejects duplicate or
aliased ownership. Choose which view owns a region explicitly; never overwrite
conflicting rows. An inventory size counts body bytes, so it cannot define an
end address.

The output writes each start in the standard's notation for the file's format,
which is what `npm exec -- standard-coverage` reads:

- MZ (the default `sourceKind`, or `mz`): ranges are file offsets, and the
  config keeps `loadSegment` at its default 4096, the segment the standard
  places the load image at. A start in the load image is written `SSSS:OOOO`.
  A segmented input keeps its spelling; a file offset there becomes the
  segment:offset whose offset is below `0x10`, which names the same byte.
  Segmented starts map only into the load image. A start in a Borland FBOV
  overlay is written as an eight-digit file offset (`0x00000210`) and must lie
  inside a row of the build entry's Code ranges for the file, which the config
  gives as `codeRanges`, a list of numeric half-open `start` and `end` file
  offsets, each inside one overlay payload. Without a matching row the join
  fails, as the coverage check would.
- PE32 (`sourceKind: "pe32"`): ranges and starts are virtual addresses at the
  header's image base, and a start must lie in an executable section. It is
  written with eight digits (`0x00401000`). PE32+, LE, LX and NE files are
  refused: the reader has no section model for them yet.

The result gives TSV and the canonical destination, such as
`coverage/BLD-EXAMPLE/@CD/GAME.EXE.tsv` for manifest `CD:GAME.EXE`. Optional
`writeRoot` creates that path with exclusive creation and never overwrites an
inventory. Review replacements before moving a new TSV into place. Reject unsafe
or nonportable manifest paths rather than silently renaming them. Commit only the
permitted inventory columns; keep view reports/configs local. Document the
selected views and exclusions in the build/Ghidra guide in project-authored words.
Coverage describes analyzer-discovered functions, not every function that exists.
The protocol's [Creating and checking an inventory](../vendor/upstream/work-protocol.md#creating-and-checking-an-inventory) (lines 564-572)
says how to export and check one. The pinned checker reads the protocol's
`ranges` column, the half-open body ranges of a function whose body is not one
range from its start. From standard-checker 6.0.0 `standard-coverage` also reads
the `.provenance.tsv` and `.regions.tsv` files beside an inventory: the
provenance must give the manifest's `xxh3`, and the regions file must have the
columns `kind`, `start`, `size`, `instructions`, `instructions_outside`, `data`,
`data_outside`, `undefined` and `undefined_outside`, with a `region` row for
each measured region, an `outside` row for each stretch of a body in no region
and an `entry` row for each start where no instruction starts, and its rows
must agree with the inventory's bodies. `ExportResearchBaseline.java` writes
that layout, and `migrate-inventory.mjs` and `join-overlay-views.mjs` carry it
through, writing each start as the inventory writes it. The engine's
`ExportFunctionInventory.java` Ghidra script (`docs/GHIDRA.md`) writes all
three straight to the inventory's `coverage/` path from one analysis view, with
ranges where a body needs them; the protocol commits the provenance and regions
files beside the inventory. This repository's `inventory` writes, and its
`inventory-check` accepts, only the four columns above, so an inventory with a
`ranges` column is checked by `standard-coverage`, the documentation check and
the engine's `x86-inventory-check`. A row without `ranges` is measured as one
contiguous body, and the range-end check fails any range that ends on that
body's last byte (start plus size minus one). The DOSBox inventory's sizes count
the addresses of bodies with gaps (BLD-GOG-EN-1.1), so a correct range can fail
there; check such a failure against the instructions before moving an end.

`npm exec -- standard-coverage` reads the committed inventories and prints, per
file, the share of in-scope functions and bytes that an entry's `locations`
cite, and the uncited functions; `--list`, `--json` and `--require-complete`
are its other modes. A byte that two rows list counts once, and the shared
bytes are reported apart. An inventory with a row it cannot read gets no
figures: it is printed as not measured, and `--json` lists it under
`unmeasured`. Its per-function lists are printed on demand and never
committed. `node tools/progress-report.mjs` takes its per-file totals, with
the protocol's other progress measures, into `docs/PROGRESS.md`, which is
committed so each snapshot shows the change since the one before.

Once inventories exist, the documentation check also fails a range in an entry
whose end is the last byte of an inventoried function, since ranges are
half-open and such a range stops a byte short. The message gives the end the
range should have. A range that ends there on purpose goes in the entry's
`ends_on_last_byte` (`docs/SPEC-ENTRY-TEMPLATES.md`).

## Instruction-derived reports

The `x86-trace`, `x86-uses`, `x86-arguments`, `x86-effects`, `x86-returns`,
`x86-memory`, `x86-incoming`, `x86-guards`, `x86-allocation`, `x86-dispatch`,
`x86-operand`, `x86-operand-candidates`, `x86-target`, `x86-bounds`, `x86-owner`,
`x86-callees`, `x86-reach`, `x86-call-order`, `x86-inventory-check`,
`x86-pointers`, `x86-table`, `x86-bodies`, `x86-imports` and `x86-unpack`
commands run the report of the same name from
`@scientific-method/executable-reader`. It runs `pointers`, `table`, `bodies`,
`imports` and `unpack` itself and hands every other one to
`scientific-method-engine`. Keep configurations and reports in `GAME_DIR` and out
of commits. Run `./tools/Restore-ToolDependencies.ps1` to install exact locked npm
and Python dependencies. The default interpreter lives in
`artifacts/evidence-python`; `EVIDENCE_PYTHON` may select an explicit interpreter.
See [the shared contract and local routing](BOUNDED-EVIDENCE-REPORTERS.md).

`package-lock.json` and `tools/evidence/requirements.txt` identify the adopted
registry releases and archive hashes. NoRestore verifies installed versions
without installation or network fallback. Upgrade locks only for reviewed
published releases, then rerun source controls and the canonical validation gate.
Rule snapshots are independent and remain unchanged by package installation.
`tools/evidence/legacy-image.mjs` re-exports the packaged MZ/FBOV reader.
Existing lightweight `incoming`, `flow` and `table` commands retain their scope;
the instruction-derived variants supply the additional analysis.

Use `x86-target` before citing a far call's target: it keeps the raw operand,
the relocation or FBOV fixup, the stored descriptor word and decoded index, the
trampoline and the canonical target, and compares an analyzer's address with
each instead of replacing them. Use `x86-bounds` and `x86-owner` before joining a
call to its caller or bounding a reading by an analyzer's size: an analyzer's
size is a body-byte count and is never added to a start to make an end.
`x86-owner` includes checked-entry reached ranges, source-derived overlay exports
and explicit boundary checks. Incomplete, contested, gapped or entry-limited
coverage refuses a join; supplied export metadata is rejected. Give
`formatControls` the build's known relocation, descriptor, overlay, fixup and
trampoline counts so a misread table fails the query. Declare a resident
segment's bounds from the build's code ranges in `segments` so an incoming
search over part of it is reported as partial.

`x86-reach` lists the target sites a set of starts reaches over resolved calls
and jumps, with the fewest-call chain to each and every reached transfer it
could not resolve, which is the walk a census of a mechanism's sites starts
from. `x86-inventory-check` lists the resolved direct call targets that no row
of a committed inventory starts at. `x86-bodies` says where each byte of given
function bodies lies by the MZ and FBOV tables, and gives every FBOV
descriptor's words, span and loaded address. `x86-imports` gives the import a
PE32 file's import tables put in each import address table slot, checked
against a positive control. `x86-unpack` decodes an LZEXE 0.90 or 0.91, EXEPACK
or PKLITE 1.00 to 2.01 executable without running its stub, writes the unpacked
file by the reader's layout rule, and prints the `size`, `xxh3`, `format` and
`tool` that the build's `unpacked` item gives. The toolkit guide covers their
inputs and limits.
## PE32 executables

The `x86-` commands also accept `sourceKind: "pe32"` for i386 executables. The
loader derives preferred-base mappings from validated source sections. Regions
and entry/control sites are file offsets; flat memory query offsets are VAs. The
toolkit guide documents 32-bit frames, scaled addressing, the flat-segment
assumption and the unsupported indirect and runtime routes. This project's
executable is MZ/FBOV, so these queries keep the segmented 16-bit model.


## Committed inventory verification

`inventory-check` reads the same hash-guarded source as `inventory` (MZ/FBOV, or PE32 with `sourceKind: "pe32"`), with `build`, `manifest` and, for overlay starts, `codeRanges`. Its `inventory` object names a local input `path` and `repositoryPath`, which must match the generated portable coverage destination; the local `path` must end with that `repositoryPath`, so the file read is the one the destination check names. Every start must be in the standard's notation for the file, as `inventory` writes it: an upper-case `SSSS:OOOO` in the MZ load image (any spelling of the byte), an eight-digit upper-case file offset inside a Code ranges row in overlay code, or an eight-digit upper-case virtual address in an executable PE section. Starts must be unique, counting two spellings of one byte as the same start. Body byte counts are positive/bounded and are never interpreted as end addresses. Committed TSV columns are start, size, optional researcher-authored name and out_of_scope. No analyzer names/code/bytes belong there; analyzer default names such as `FUN_0040` are rejected. A configured project retaining a historical path supplies `legacyPath` plus nonempty `legacyEvidence`; the checker validates that exact safe path but continues to report the portable canonical destination. A legacy allowance is an explicit research input, not proof of an arbitrary path's provenance.

The pinned reporter now includes `x86-operand` for instruction-owned MOV/PUSH segment immediates, direction-sensitive strings, saved flags and local IRET. Overlap proof follows a proven target through its continuation; callers reached only through contested starts remain unresolved. See the exact pinned BOUNDED-EVIDENCE-REPORTERS.md for query contracts.

Computed near word jumps can use evidenced, explicitly exhaustive `indirectJumps`
tables in CFG reports. Path reports still stop there. `x86-pointers` inventories
MZ relocation and FBOV fixup word pairs as exact, alias, unresolved or excluded
candidates; these are never proof of runtime pointer use. See the pinned guide
for limits, provenance and partial-search controls.

`x86-operand-candidates` inventories encoded displacement/immediate matches,
preserving prefix order/repeats, width and overlapping spans. Verified entry-path
memory uses, rejected overlapping decodes and unresolved boundaries stay separate.
Implicit operands and relative branch targets are excluded. Controls reject raw
starts; scan/result caps keep partial coverage and incomplete groups explicit.
See the pinned guide for the exact contract and exclusions.

`x86-callees` reads a bounded established-entry graph breadth-first, distinguishing
shared-node reuse from a verified cycle-closing route. Per-caller references to
`calleeSummaries` retain reachable memory observations, assumptions and unresolved
dependencies. Effects remain incomplete; no missing write proves a read-only
callee. Unchecked entries and caps refuse boundary/completeness claims.

Argument/effect reports retain caller LEA formations and link consumed near
pointers to dereference segment choices and producers. Equal offsets alone never
prove DS=SS. Propagated segment equality and matching/affine symbolic offsets
qualify modeled storage merging; unknown relationships and formation evictions
refuse it. Pointer-related reads remain in effect reports; see the pinned guide.

### Independent physical PE32 transfer candidates

`node tools/evidence/report.mjs pe-transfers <local-config.json>` requires
`sourceKind: pe32`, `source`, the explicit `xxh3`, `targets` and `controls`.
Each target/control is a half-open `{start, end}` address range; controls must
not overlap targets and each must produce a candidate. `limit` defaults to 128
and is at most 16384; an overflow fails rather than returning a partial search.

The i386-only scanner examines every possible five-byte E8/E9 start in loaded,
physically backed executable sections, computes signed rel32 destinations with
32-bit wrapping, and reports source offsets, destinations and matching ranges.
Virtual-only bytes, unmapped raw padding, data sections and cross-region encodings
are excluded. Instruction boundaries and prefix interpretation are unverified:
candidates can lie in operands or data embedded in code. Rel8/rel16, conditional,
far, indirect, computed and runtime-written transfers remain outside the default search.
An explicit `forms` list can select `call-rel32`, `jump-rel32`, `jump-rel8`,
`conditional-rel8` and `conditional-rel32`. These add EB, 70..7F and
0F-80..0F-8F opcode forms, with the signed displacement read at the appropriate
width and the destination measured after two, five or six bytes. Every selected
form requires an independent positive control with a matching `kind`; a control
hit of another kind cannot satisfy it. Unknown or duplicate forms are rejected.
The report lists selected forms, encoding lengths and remaining exclusions.
Rel16, LOOP/JCXZ-family, far, indirect, computed and runtime-written transfers
remain excluded. Prefixes and instruction starts are still unverified.
An empty target result with passing controls is not a complete caller declaration.
Keep configurations and licensed-source results in GAME_DIR, never in Git.
### Function-body classification

`x86-bodies` runs the reader's `bodies` report. It takes a
hash-guarded `sourceKind: mz` configuration with `formatControls` and
`functions`, each an `entry` and `body` ranges as file offsets with exclusive
ends, and partitions every fragment by the MZ and FBOV tables into resident
image, overlay stub, overlay code and fixup table per descriptor, zero padding
(only when every byte is zero) and undeclared bytes. The entry is placed on its
own, and parts of another kind or descriptor are marked outside the entry's
region, so another overlay's code counts as outside. A function may carry
`candidate: { ranges, evidence }`, such as a reading's instruction span; the
report then gives the bytes both sides hold and each side's own, with neither
treated as right. It decodes no instruction and establishes neither native
flow nor ownership, and it never justifies widening Code ranges.
Licensed-source configs/reports remain in GAME_DIR. Inventory changes need
separate research.

## Local script assignment paths

`python -I tools/research/exec-census/script_local_paths.py LISTING CONFIG`
reads a retained `script_decode.py --all` index. Config names `resource`
(`GPL/NUMBER` or `MAS/NUMBER`), decimal `roots`, selector instruction `target`,
local-number index `local` and the raw listing's `listingSha256`. Optional
`maxStates` caps all roots together (default 20000, maximum 100000);
`maxDepth` bounds additional modeled local frames (default 32, maximum 49).
Use the project-local locked evidence Python. Regenerate the index using its
recorded source/decoder identities; the tool verifies the listing hash, not
the original files from which someone generated it.

Local calls and menu targets pair with their own return continuations.
The report stops at the first requested opcode-0x5E instruction per candidate
path and lists last direct named assignments, including explicit zero,
expression assignments and no known assignment as distinct cases. Other
effects remain marked; a later assignment starts a new direct history.
Cross-script calls, unknown or missing targets, rejected spans, unmodeled
returns, menu selection/overlap and caps remain explicit unresolved boundaries.
Conditions and menu availability are not evaluated. Paths in the fixed listing
do not prove opcode-callback effects, alias or storage preservation, runtime
admission, actual selector values or complete readings.

Synthetic controls are in `tests/evidence/script-local-paths.test.mjs` and
the canonical gate runs them. Installed-index acceptance uses the 38 declared
controls of FND-COMBAT-101; retain original-derived configs and reports in
GAME_DIR/analysis/local-selector-paths-20261103/matched-menu/. Earlier files
in that store are diagnostic, preceding corrected menu frame pairing.
Recording a research conclusion from the reports is a separate research batch.

## Script selector census

`python -I tools/research/exec-census/script_selector_census.py LISTING CONFIG`
uses the same bounded index grammar and operand splitter. Config requires
`listingSha256` and a `controls` object: `total`, `sentinel`, `otherLiteral`,
`nonliteral`, `sentinelLiteralSecond`, `sentinelNonliteralSecond` and `errors`.
All are explicit expected counts; a missed control rejects the report.
FND-COMBAT-100 records the original-index controls. The decoder's matching
inline error diagnostic and final summary count as one error; unmatched or
duplicate diagnostics fail instead of becoming silent coverage.

The report groups top-level indexed opcode 0x5E starts by first-operand class,
then distinguishes zero/nonzero literal selector low words and nonliteral
selectors. Nonliteral fields are unknown, not zero. It records listing, config,
tool and parser hashes, retains the decoder gap and excludes nested starts.
Runtime admission, parameter preservation and native effects remain separate.
The tool starts no game process and changes no resource.

Synthetic controls in `tests/evidence/script-selector-census.test.mjs` cover
nested commas, full-width sentinel identity, word narrowing, missing controls,
error diagnostics and input/output bounds. Original configs and the successful
repeated `census-c.json`/`census-d.json` reports are retained under
GAME_DIR/analysis/selector-domain-census-20261103/. The earlier empty failed
report is diagnostic only. Research conclusions belong in their own batch.
