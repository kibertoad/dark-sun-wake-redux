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
