# Bounded evidence tools

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

Run `ExportFunctionInventory.java <new-output.tsv>` in each relevant Ghidra
view. The exporter writes only `start` and `size` (body byte count), without
analyzer names, code or strings. Raw segmented exports need the same documented
load mapping as the resolver. For a different overlay import, first convert
its starts to canonical `0x` file offsets using that import's documented map.

An `inventory` config includes source identity, `build`, `manifest`, and
`views`. Each view has a distinct `name`, an input `path` and explicit ownership
`ranges` with numeric inclusive `start` and exclusive `end` file offsets.
Ranges must lie in the resolver's declared source regions, and no two views may
own overlapping ranges. Segmented starts map only into the resident image. The report counts
accepted and excluded rows per view and rejects duplicate or aliased ownership.
Choose which view owns a region explicitly; never overwrite conflicting rows.
An inventory size counts body bytes, so it cannot define an end address.

The result gives TSV and the canonical destination, such as
`coverage/BLD-EXAMPLE/@CD/GAME.EXE.tsv` for manifest `CD:GAME.EXE`. Each start
retains the manifest prefix (`CD:GAME.EXE+0x00000210`). Optional `writeRoot`
creates that path with exclusive creation and never overwrites an inventory.
Review replacements before moving a new TSV into place. Reject unsafe or
nonportable manifest paths rather than silently renaming them. Commit only the
permitted inventory columns; keep view reports/configs local. Document the
selected views and exclusions in the build/Ghidra guide in project-authored words.
Coverage describes analyzer-discovered functions, not every function that exists.

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

`inventory-check` uses the ordinary hash-guarded MZ/FBOV source, `build` and `manifest`. Its `inventory` object names a local input `path` and `repositoryPath`, which must match the generated portable coverage destination. Every canonical start must carry the same manifest prefix, be unique by numeric offset and lie in mapped source. Body byte counts are positive/bounded and are never interpreted as end addresses. Committed TSV columns are start, size, optional researcher-authored name and out_of_scope. No analyzer names/code/bytes belong there. A configured project retaining a historical path supplies `legacyPath` plus nonempty `legacyEvidence`; the checker validates that exact safe path but continues to report the portable canonical destination. A legacy allowance is an explicit research input, not proof of an arbitrary path's provenance.

Inventory checks require eight uppercase hexadecimal offset digits, reject analyzer default names, and verify that the input file ends with its declared repositoryPath. These checks supplement researcher provenance; they do not establish it.

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
