# Upstream gaps observed during Survey

These are requests for the restoration template and shared analysis tooling.
They describe tooling behavior, not claims about the original game's rules.

## 1. Provide a standard function-inventory export path

The work protocol requires `coverage/<build ID>/<manifest path>.tsv`, but this
checkout had no coverage exporter or documented address convention. For
`BLD-GOG-EN-1.1/DSUN.EXE`, I added `ExportFunctionInventory.java` and
`Join-FunctionInventory.ps1`. The latter keeps only function starts within the
shipped file's resident image or overlay-code ranges and writes canonical file
offsets, because the overlays have no fixed runtime address.

**Request:** add a shared exporter and schema check for the allowed columns
(start address, size, optional researcher-given name and out-of-scope reason).
Document how to represent segmented and overlay addresses without retaining
code, bytes, strings or analyzer-generated names. The check should catch
duplicate starts, invalid sizes, and starts outside the mapped source ranges.

## 2. Make memory-block reports usable for mapped overlays

The local-only `FBOV` mapped image produced 3,546 Ghidra memory blocks.
`ReportMemoryBlocks.java` stopped at its 512-block limit without a report.
The limit is useful for bounded output, but it prevents inspecting this image's
block layout when diagnosing analyzer-discovered functions.

**Request:** let the shared reporter select a named block, address range, or
bounded page of blocks, while retaining an explicit output limit. This would
allow focused inspection without a broad memory-map export.

## 3. Account for analysis-view differences in coverage guidance

Ghidra 12.1.3 found 1,284 resident function starts in the original MZ import.
In the mapped-overlay import, 1,021 of those starts were absent, while the
mapped view also found 869 starts inside overlay-code ranges. The joined TSV
therefore uses the original import for resident code and the mapped import for
overlay code. The 2,153 rows are analyzer-discovered starts, not proof that
all original functions were found.

**Request:** describe coverage inventories as view-specific analyzer results,
and provide a repeatable way to combine views of a packed, overlaid or banked
executable. A single mapped import should not silently replace the native
import's function inventory.

## 4. Offer an offline rerun for the local test gate

`./tools/Test.ps1` invokes `dotnet test` with restore on every run. In this
restricted workspace, restore failed on NuGet's service or signature endpoint
even after a successful authorized restore had cached the packages. The same
script passed all 700 tests when network access was available.

**Request:** consider an explicit offline rerun option that uses an already
restored lock/assets state. Keep the normal CI path restoring packages from
NuGet.

## 5. Define a portable inventory path for disc manifest entries

The Survey rule asks for `coverage/<build ID>/<manifest path>.tsv`. The manifest
path `CD:DSUN.EXE` cannot be used verbatim as a Windows filename. This checkout
uses `coverage/BLD-GOG-EN-1.1/CD/DSUN.EXE.tsv` and retains `CD:DSUN.EXE` in each
start address. The join tool now accepts the manifest path explicitly.

**Request:** define a portable encoding of manifest paths for coverage files and
check that the path and each address prefix resolve to the same manifest entry.

## 6. Keep the authoritative research-batch rules available offline

The repository's `AGENTS.md` and `.claude/skills/research-item/SKILL.md` explain
the research procedure well enough to carry out a batch, but both say the
published Protocol wins if they differ. During the `FMT-CONFIG-004` batch, the
Protocol's research-batches page was unavailable through the available browser
tool, so a possible disagreement could not be checked against the authoritative
text.

**Request:** ship a versioned, locally readable copy or snapshot of the
authoritative research-batch rules with the template, and identify the upstream
revision it represents. Keep the remote page as the source of updates, with an
explicit way to detect when the local snapshot needs refreshing.

## 7. Accept canonical overlay offsets as executable finding locations

The build and Ghidra guide identify `DSUN.EXE` overlay code by shipped-file
offset because it has no fixed runtime address. The documentation checker
rejects `offset: 0x...` for an MZ executable and also rejects an `address:`
value written as `DSUN.EXE+0x...`. For FND-CONFIG-009 and FND-UI-033, the
location metadata therefore names only an overlay's resident header; the
precise code offset has to be written in the finding body.

**Request:** let an executable finding location use a build-defined canonical
file-offset notation for overlay, banked or packed code, and validate that the
offset falls within the build's documented mapped range. This would make the
machine-checked location as precise as the finding itself.

## 8. Resolve one FBOV far-call fixup target on demand

While tracing the Start Game setup helper, raw overlay instructions appeared
to call segments such as `0160` and `01D0`. Those words are FBOV descriptor
encodings; the descriptor table resolves them to different resident segments,
and one call resolves to an overlay trampoline. Treating the encoded word as a
resident segment sends a researcher to unrelated bytes. The current overlay
map reports code ranges, but does not answer this one-call target question.

**Request:** add a bounded lookup to the shared FBOV tooling that accepts one
shipped-file call-site offset, verifies that the segment operand has a fixup,
and reports its descriptor index, resolved segment and target address or
trampoline. Make an absent fixup explicit so the tool does not assign a
plausible target to an unrelocated word.

## 9. Distinguish a window image from copied control data in UI catalogs

`UiWindowResource` currently reports `Window.ImageResourceNumber` from
offset `0x3A` of a `WIND` record. In `WIND/18500`, that word is 10002 because
the record copies edit-box data; the window's own image field at `0xC2` is
zero. The read-only UI catalog therefore appears to assign `BMP/10002` to
the whole window, even though it belongs to the copied edit-box record.

**Request:** have the shared UI catalog expose the true window image field
separately from copied control data, and label the latter as uncertain until
its runtime use is established. A synthetic fixture with different values at
`0x3A` and `0xC2` would guard against this false screen-background claim.

## 10. Normalize resident MZ far-call targets before citing them

While tracing the message window's child registration, the raw far-call
segment operands `2EBE` and `2F96` initially looked like resident addresses.
In this Ghidra import the load image begins at segment `1000`, so the mapped
targets are `3EBE:0008` and `3F96:000B`. Reading the raw operands as mapped
addresses led to unrelated bytes; FND-CONFIG-031 was corrected in the next
research batch. This is separate from the FBOV fixup problem in item 8.

**Request:** give the shared executable-analysis tooling a bounded far-call
target reporter for ordinary MZ relocations. For one call site, show the raw
operand, whether its segment word is relocated, the import's load segment,
the mapped segment:offset and the shipped-file offset. Make the distinction
between a raw operand and a citable mapped address explicit.

## 11. Show function ownership alongside bounded call-chain reports

FND-CONFIG-092 corrected a chain that crossed an overlay setup routine's
return into the following frame handler. The setup and handler are adjacent
in the file but have distinct exported trampolines. A bounded instruction
window can hide that distinction when a call is inspected far from its entry.

**Request:** let the shared call-site reporter include the containing
analyzer function and any enclosing exported overlay entry, with their
bounded ranges and the evidence for ownership. Flag disagreement between
those views, and require an explicit boundary check before joining a call
to its supposed caller. Prologues and returns are useful warnings, but
must not silently stand in for a verified function boundary.

## 12. Require explicit counts when decoding bounded dispatch tables

During the FND-CONFIG-096 reading, a temporary manual table query requested
three tag entries from a dispatch loop whose instruction count was two.
The third read reached target-word data and failed while printing the
resulting non-ASCII text. No claim
was recorded from that extra read, but an ASCII-looking target could instead
have produced a false tag or branch.

**Request:** provide a shared bounded table reporter that requires an explicit
entry count and field widths, reports the source of that count, and separates
tag/value entries from target entries. Where the count comes from an observed
loop, report its instruction location alongside the table range. Reject reads
outside the declared layout rather than attempting text decoding across the
boundary. This would make a narrow dispatch query easier to reproduce and
review without retaining the table's original bytes.

## 13. Provide a target-specific incoming-call inventory for FBOV and MZ

FND-CONFIG-100 and FND-CONFIG-101 needed repeated temporary queries over
FBOV fixups, MZ relocations and one overlay's relative-call candidates to
trace a shared selector. The existing overlay map identifies code ranges;
it does not produce a bounded incoming-call report for one exported entry.

**Request:** add a shared reporter accepting one descriptor and trampoline
entry, with an explicit result cap. Report declared overlay far calls and
resident relocated far calls separately, and optionally search one declared
code range for relative-call candidates. Include canonical source offsets,
the inspected encoding and coverage boundary, and distinguish confirmed
instruction sites from byte-pattern candidates. A zero-result section should
say exactly what was searched, so it cannot be mistaken for proof that no
computed, aliased or differently encoded route exists. This complements the
single-call fixup resolution request in item 8.

For the FND-CONFIG-111 query, require the report to show both the stored
shifted index and decoded descriptor, and validate a known incoming-call
inventory as a positive control before accepting negative sections. Treating
the stored shifted index as the descriptor itself otherwise silently misses
calls. The existing eleven-call selector inventory supplied that check here.

For the FND-CONFIG-113 reading, also require relative-call reports to derive
their search range from the complete declared segment or overlay bounds.
A prefix ending at the target function's return misses callers later in the
same segment; report such a range as a partial search. Keep candidate discovery
across that range separate from bounded instruction verification at each hit.

For FND-CONFIG-114, canonicalize relocated pointers by resolved file target
as well as reporting exact segment:offset matches. Distinct DOS segment aliases
can name the same location. Report exact-pair and aliased-target results
separately, and preserve the exclusion of computed or unrelocated pointers.

## 14. Check known instruction hits before trusting a variable-use inventory

While preparing FND-CONFIG-108, a temporary variable-use query linearly
disassembled overlay 208 from its code-range beginning and reported no
exact uses of two fields. A separate entry-based reading had already
identified both reads. Decoding across the overlay's intervening data or
instruction-boundary gaps can lose alignment and miss later known code.
The zero-result query was discarded; no absence claim relies on it.

**Request:** make shared variable-use reporters start from established
function or exported-entry boundaries and validate at least one known
positive instruction hit when available. Separate raw operand-pattern
candidates from verified instructions, report undecoded ranges, and reject
a negative result when it misses its positive control. A declared overlay
code range is a containment bound, not proof that all its bytes can be
linearly disassembled as one instruction stream.

## 15. Preserve ordering and shared guards in incoming-call summaries

FND-CONFIG-119's seven calls to one helper are two guarded three-call
sequences followed by a conditional seventh call. A flat incoming-call list
could be mistaken for alternative dispatch branches. The distinction matters
because each invocation captures and restores state before the next begins.

**Request:** let the shared reporter group calls by verified containing entry
and show their local order, cleanup continuation and observed shared guard.
Keep a flat inventory for coverage, but label whether a group is a sequence,
branch alternatives or still unread. Do not infer preserved state or successful
return merely from consecutive call locations; retain callee effects as an
explicit gap unless separately read. This complements the function-ownership
request in item 11.

## 16. Resolve segment-load operands before citing data addresses

The FND-CONFIG-120 through FND-CONFIG-131 corrections replace raw segment
labels in earlier CONFIG findings. Resident segment loads need their MZ
relocation applied; overlay segment loads contain shifted descriptor indices
that need the FBOV table lookup. The instruction's numeric operand alone is
not the mapped segment, even when its following field offset is correct.

**Request:** extend the bounded target resolver in item 10 beyond far calls to
segment-register loads and stored far-pointer segments. Report the instruction
location, operand representation, declared relocation/fixup membership,
decoded descriptor where applicable, and canonical mapped segment. Require
that provenance alongside data-address labels in findings; preserve a raw
operand explicitly as raw when mapping is unresolved. This prevents the same
mapping error from recurring in data reads after call targets were corrected.

## 17. Resolve prefix-overlapping operand candidates to verified boundaries

While preparing FND-CONFIG-136, a raw pointer-operand search produced both
an actual prefixed double-word comparison and a word-width comparison
starting one byte into it. Both decode locally, but only the former starts
at the containing routine's established instruction boundary. Counting both
would invent a second use and could misstate the pointer's null check.
The overlapping candidate was rejected before recording the finding.

**Request:** have shared operand reporters retain prefixes and candidate
widths, group overlapping decodes, and classify them against a verified
entry-based instruction path before counting uses. Report unresolved
boundaries explicitly instead of selecting a width from a locally valid
decode. This complements item 14's alignment and positive-control checks.

## 18. Trace dispatch-index preprocessing before assigning input cases

FND-CONFIG-137's expression decoder subtracts an extended bit before
its table lookup. A direct lookup using the unnormalized input byte
would put two equivalent input forms into different branches. Checking
the preprocessing confirmed their common target; no contrary claim
was recorded from the raw table alone.

**Request:** include the verified input-to-index transformation in shared
bounded dispatch-table reports, alongside the count requested in item 12.
Separate raw table positions from original input values, and leave the
input case unresolved until its normalization and range checks are read.

## 19. Report effective operand size beside conversion mnemonics

While preparing FND-CONFIG-138, Capstone 5.0.7 printed the same
word-to-double-word conversion mnemonic for synthetic unprefixed and
operand-size-prefixed sign-extension instructions in 16-bit mode. Their
actual register widths differ. A synthetic single-instruction Unicorn
check preserved the upper word and extended the byte for the unprefixed
case, but extended the word into the full register for the prefixed case.
No original-game function was executed for that tool check.

The [Intel instruction reference](https://www.intel.com/content/dam/www/public/us/en/documents/manuals/64-ia-32-architectures-software-developer-vol-2a-manual.pdf)
defines the conversion by effective operand size. The original's three
relevant locations were checked for prefixes, and the finding describes
the register-width operation rather than trusting the printed mnemonic.

**Request:** have shared 16-bit instruction reporters show effective operand
size and register semantics for implicit-operand conversions, and add
synthetic prefixed/unprefixed positive controls. Mark a mnemonic/width
mismatch explicitly; a locally plausible mnemonic must not silently change
signed-byte fields into signed-word fields in an evidence record.
