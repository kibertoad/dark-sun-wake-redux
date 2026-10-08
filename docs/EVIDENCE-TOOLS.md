# Bounded evidence tools

## Measured work baseline

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

Existing committed inventories remain separate from these fresh snapshots until
mapping and body anomalies are reconciled. In particular, fresh definitions
outside declared overlay code ranges are retained and reported rather than
clipped to make the standard reporter accept them. Local report output and
analysis projects stay in GAME_DIR; no generated report is committed.

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
The protocol's [Creating and checking an inventory](../vendor/upstream/work-protocol.md#creating-and-checking-an-inventory) (lines 454-462)
says how to export and check one. Checker 2.8.0 reads an optional `ranges`
column after the four above: half-open `start..end` ranges separated by
spaces, whose total is the row's size and one of which holds the start, for a
function whose body is not one range from its start. It no longer reads the
`.provenance.tsv` and `.regions.tsv` files beside an inventory as inventories.
A row without `ranges` is measured as one contiguous body, and the range-end
check fails any range that ends on that body's last byte (start plus size
minus one). The DOSBox inventory's sizes count the addresses of bodies with
gaps (BLD-GOG-EN-1.1), so a correct range can fail there; check such a
failure against the instructions before moving an end. The shared
`ExportFunctionInventory.java` still writes no `ranges` column.

`npm exec -- standard-coverage` reads the committed inventories and prints, per
file, the share of in-scope functions and bytes that an entry's `locations`
cite, and the uncited functions; `--list`, `--json` and `--require-complete`
are its other modes. Run it when the figures are needed and read them from its
output; they are printed on demand and never committed.

## Instruction-derived reports

The `x86-trace`, `x86-uses`, `x86-arguments`, `x86-effects`, `x86-returns`,
`x86-memory`, `x86-incoming`, `x86-guards`, `x86-allocation`, `x86-dispatch`,
`x86-operand`, `x86-operand-candidates`, `x86-target`, `x86-bounds`, `x86-owner`,
`x86-callees` and `x86-pointers` commands run the report of the same name from
`@scientific-method/executable-reader`, which hands every one but `pointers` to
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
far, indirect, computed and runtime-written transfers remain outside the search.
An empty target result with passing controls is not a complete caller declaration.
Keep configurations and licensed-source results in GAME_DIR, never in Git.
### Overlay function-body anomaly classification

`overlay-bodies` accepts a hash-guarded `sourceKind: mz` configuration with
`formatControls` (including a positive `overlays` count) and `ranges`.
Each range has file-offset `start`, exclusive `end` and a separate `entry`.
The bounded MZ/FBOV reader supplies resident and overlay code/fixup intervals;
inter-block and trailing remainders are called zero padding only when every
byte in that remainder is zero. Other bytes stay explicitly unclassified.
Every span is partitioned completely, without dropping analyzer body fragments.
Entry placement is separate: an overlay-entry function can own a resident or
fixup-range fragment in an analyzer snapshot. That does not prove native flow,
valid function ownership, or justify widening Code ranges. Invalid spans and
format-count controls fail; at most 4096 spans are accepted. Licensed-source
configs/reports remain in GAME_DIR. Inventory changes need separate research.