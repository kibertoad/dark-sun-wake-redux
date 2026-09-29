# Upstream gaps observed during restoration work

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
text. Direct opens of both the Protocol root and the documentation
Standard were also unavailable through the browser tool on 2026-09-29.
The adapted local skills and templates remain usable, but cannot verify
whether either published page differs from them.

**Request:** ship a versioned, locally readable copy or snapshot of the
authoritative Protocol and Standard rules with the template, and identify
the upstream revision it represents. Keep the remote page as the source of updates, with an
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

FND-CONFIG-144 also corrects FND-CONFIG-134's numeric argument reading:
a pushed immediate was a declared overlay segment fixup for a far output
pointer. Reading the callee's argument loads confirmed the pointer shape.

**Request:** extend the bounded target resolver in item 10 beyond far calls to
segment-register loads, stored far-pointer segments and pushed segment
arguments. Report the instruction
location, operand representation, declared relocation/fixup membership,
decoded descriptor where applicable, and canonical mapped segment. Require
that provenance alongside data-address labels in findings; preserve a raw
operand explicitly as raw when mapping is unresolved. This prevents the same
mapping error from recurring in data reads after call targets were corrected.

## 17. Resolve overlapping operand candidates to verified boundaries

While preparing FND-CONFIG-136, a raw pointer-operand search produced both
an actual prefixed double-word comparison and a word-width comparison
starting one byte into it. Both decode locally, but only the former starts
at the containing routine's established instruction boundary. Counting both
would invent a second use and could misstate the pointer's null check.
The overlapping candidate was rejected before recording the finding.

FND-CONFIG-142 later rejected four apparent writes starting one byte
before verified reads. Overlap handling therefore needs to cover preceding
instruction bytes as well as stripped prefixes. Local decodability alone
does not establish a write or its operand width.

FND-CONFIG-148 also rejects an apparent address store assembled
from the middle of an addition and its following jump. A matching
immediate can be an overlapping instruction candidate rather than
an address-taking use, even when the entire candidate decodes.

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

## 20. Distinguish shared callees from recursive call paths

FND-CONFIG-140 and FND-CONFIG-141 trace two expression branches that
converge on one lookup routine. An early description called this a cycle;
reading the complete lookup and its children showed no call back into the
seed search. The corrected findings record convergence. The shared lookup
also contains a conditional table write, so revisiting its node must not
turn it into an assumed read-only leaf.

**Request:** label shared-node reuse separately from a verified recursive
path in bounded callee summaries. Reserve a cycle label for an edge back
into the current traversal path, and carry the shared node's known effects
and unresolved dependencies at each caller. This complements item 11's
function ownership and item 15's local call ordering.

## 21. Preserve segment provenance for near state pointers

FND-CONFIG-145 follows a BP-derived near pointer into helpers that
access it through DS. Equating those addresses without checking the
caller would assume a DS-to-SS relationship that the bounded reading
has not established. That assumption would turn conditional count
consumption into an unsupported runtime termination claim.

**Request:** shared argument and effect reports should retain the segment
used to form and dereference a near pointer, including implicit SS for
BP-relative operands and default DS for ordinary indirect accesses.
Require segment-state provenance before merging them into one state
buffer, and report aliasing as unresolved when that provenance is absent.
This complements item 16's relocated far-pointer mapping.

FND-CONFIG-181 extends this concern to filename construction: a far
copy writes an explicit SS destination, numeric conversion writes through
near DS offsets, the caller reads SS bytes, and filename stores use DS.
Shared summaries must retain each step's segment provenance rather than
infer one buffer from matching offsets or loaded template lengths.

## 22. Validate descriptor selection before range and fixup inventories

The caller query following FND-CONFIG-147 failed while treating a
resident descriptor as an overlay. The scratch selector tested the
resident/code flag instead of the overlay flag. The same selector had
been used for FND-CONFIG-146's literal query. FND-CONFIG-148
supersedes that method description after a validated repeat confirms
its positive and rejected candidates.

**Request:** shared bounded-query entry points should validate the
spec-defined descriptor flag, header trap, resident trampoline bounds,
payload and fixup bounds, and fixup operand membership before scanning.
Require the build's known range and fixup counts as positive controls,
and fail at range construction instead of allowing invalid ranges to
be silently clipped or used in a negative finding.

FND-CONFIG-183 follows a raw call through its declared descriptor and
resident trampoline to a complete overlay body, while FND-COMBAT-009
retains an unreconciled mapped-decompiler target for the same caller.
Shared reports should retain all three identities: the raw fixup token,
the descriptor/trampoline destination and the analysis environment's
mapped address. A decompiler target name must not silently replace that
chain. Reconcile mapping provenance before merging the two readings.

## 23. Distinguish function-body byte counts from contiguous bounds

The 07B1 resource reader's inventory size is 546 bytes, while its
contiguous entry-to-return span is 549 bytes. A skipped three-byte
instruction explains the difference. Adding the inventory's size to its
start truncated the scratch reading before the return; FND-CONFIG-151
uses the verified instruction-path return instead.

**Request:** shared bounded readers must treat the inventory size as a
body-byte count, not an end address. Derive local read bounds from
verified exits and control flow, report holes or discontiguous bodies,
and reject a claimed complete reading that lacks its return or tail
transfer. Keep richer boundary reports local-only so the committed
inventory retains the Standard's allowed columns.

FND-CONFIG-158 also follows a mode-setter branch beyond its first far
return to a second return. Finding one return is not a complete bound:
shared readers should follow every reachable branch target and exit before
claiming that a function's local effects have been covered.

## 24. Allow overlapping starts when an explicit control-flow edge proves them

FND-CONFIG-155 follows an internal call into a byte that also belongs
to the preceding linear instruction. The target is a valid alternate
return path with a matching saved-flags stack frame. Rejecting it solely
because a linear decoder started another instruction earlier would
invent an unread callee and miss the flags restoration.

**Request:** shared boundary validation should retain per-path instruction
starts and explicit incoming edges. Distinguish a locally decodable overlap
with no verified incoming path from an overlap reached by a confirmed
branch or call, and check that target's continuation separately. This
complements item 17's rejection of unverified overlapping candidates;
it must not impose one global linear boundary set on all control flow.

## 25. Carry the direction flag into string-store effect reports

FND-CONFIG-154's width prefix uses repeated string stores without a
local direction clear. FND-CONFIG-155's helper clears direction for
its own writes, then restores the caller's saved flags. Treating that
helper's clear as a permanent caller state would falsely make the
following prefix's forward write span unconditional.

**Request:** shared effect summaries should include the incoming direction
flag and local clear, set and restore operations for string instructions.
Report their write span conditionally when the entry state is unknown,
and propagate saved-flags restoration across callees. This complements
item 24's alternate return path and item 21's memory provenance.

## 26. Preserve return widths and failure encodings at each caller

FND-CONFIG-156 reads an initializer whose early allocation-failure
path returns the word FFFF. Its caller stores only AL and the next
caller tests only for zero. Treating every nonzero result as success
would discard a concrete failure path. A different wrapper in
FND-CONFIG-157 explicitly compares the full return word with FFFF
before producing a boolean.

**Request:** shared call summaries should retain each callee's return
width and known result encodings, every caller's truncation or extension,
the stored width and the actual branch predicate. Do not infer successful
initialization from a nonzero check when failure encodings also pass it.
Keep live failure occurrence and player-visible consequences separate
from the statically demonstrated branch contract.

FND-CONFIG-190 adds unsigned frame-index rejection returning FFFF,
raw dimension words and a signed consumer gate. Shared return summaries
must preserve that distinction: the same word can be an explicit index
failure or a raw field rejected by the consumer. Neither a matching width
nor a nonnull source establishes accepted resource contents or extent.

## 27. Check effect ordering at early exits and before external failure

FND-SCRIPT-019 corrects an old finding that aged slots on every loader
return and a rule that treated a failed transfer as leaving its slot
unchanged. Direct branches skip the wrapper's age loop, while the fill
body writes bounds and ages before requesting the transfer. A nonzero
result alone proves neither unchanged state nor a successful rollback.
The old finding and rule remain superseded records with living citations
moved to their replacements.

**Request:** shared effect reports should show the ordered writes and
external calls for each return path, including early bypasses and state
already changed before failure. Require an explicit rollback path before
claiming transactionality, and distinguish the external error callee's
unknown effects from the caller's own writes. This complements item 15's
call ordering and item 26's return-width contracts.

FND-CONFIG-168 additionally bounds a conditional child-result exit after linked-list and
count changes, with caller pointer clears ignoring the returned result.
FND-CONFIG-179 supplies resource calls that set AX flags without a branch
using them; a subsequent field comparison supplies the actual predicate.
A pointer assigned before I/O can bypass a later request after failure.
Shared summaries should track the last flag producer for each branch and
keep output assignment separate from accepted resource content.

FND-CONFIG-183 and FND-CONFIG-184 further separate returning cleanup
from a later process-termination request. Shared summaries should retain
whether each continuation assumes the callee returns, preserve field
writes after unchecked services, and flag port accesses even in a path
whose later slot handling is skipped. An ignored result is not evidence
that the process stays alive or the cleanup completed successfully.

FND-CONFIG-186 and FND-CONFIG-187 add a temporary caller flag cleared
inside an earlier callee, and a length rejection that applies only when
the query succeeds. Both branches after a failed query join the same
transfer continuation. Shared effect summaries should carry callee writes
through apparent caller brackets and distinguish result-gated validation
from unconditional rejection. Keep both cache layers' write ordering and
post-call register provenance instead of equating a cached number with
accepted replacement content.

FND-CONFIG-189 adds distinct snapshot exits and a state commit after
stored FFFF handle requests, while the outer wrapper explicitly returns
zero (FND-CONFIG-188). Shared summaries should identify every path that
reaches a saved-state restoration or bypasses it, and separate skipped
presentation work from later committed state. A common return instruction
or wrapper normalization must not imply one universal effect contract.

FND-CONFIG-191 additionally distinguishes an unchecked wrapper
invocation from its deeper primitive gate. A FFFF handle is rejected
before the graphics primitive, even though the outer caller did not test
it; a separate request can return FFFF after partial metadata writes.
Propagate both local and child predicates before describing an actual
primitive attempt, and track failure writes independently at each layer.

## 28. Exclude superseded rules from active function ownership

Replacing RULE-SCRIPT-001 with RULE-SCRIPT-010 exposed a documentation
checker conflict: the function-definition collection still reads the
superseded rule's Procedure block, reports duplicate definitions, and
assigns living references to the historical rule. The later rule-validation
loop already skips superseded entries. This was verified in the cached
shared checker at toolkit revision 6e3cad31b6d61280a4649a873cf890377b402a75.
The historical Procedure now uses a `historical-text` fence, preserving
its content while distinguishing it from active function declarations.

**Request:** exclude superseded rules when collecting active function
ownership, just as they are excluded from active procedure validation.
Retain their text and supersession links for history, and add a checker
fixture that replaces a rule while retaining its original declarations.

## 29. Check progress across restarted scans and repeated invalidation

FND-SCRIPT-022 reads a nominally 16-slot room search that restarts its
index after a collision and retries after cache invalidation. A wrapped
end increment can restore the same candidate; invalidating an already
free selected slot can also leave the search state unchanged. Counting
slots or recording an eviction call does not by itself prove termination.
The finding keeps these conditional states separate from evidence that
the ordinary game's producers can reach them. FND-CONFIG-161 similarly
places its final far return after a repeated external poll: whether that
return is reached depends on the callee's result sequence.

**Request:** shared loop-effect summaries should identify restart edges
and the state that must change for progress. Check wrapped arithmetic
and repeated no-op invalidation before claiming a bounded search or
successful eviction. Keep local repeated-state examples distinct from
native reachability, and retain comparison signedness at each gate.

## 30. Carry runtime mode and pre-store guards into cleanup summaries

FND-CONFIG-163 follows a guard before a callback setter's pointer write.
Its runtime path uses mode one, bypassing the exit-callback-table loop
recorded for mode zero in FND-CONFIG-061. Reusing the ordinary exit summary
would incorrectly place that registered cleanup loop on this branch.
The bypass does not prove that other callees or the operating system have
no cleanup effects.

**Request:** shared call summaries should retain guards before apparently
simple stores and carry each runtime mode through cleanup branches. Name
which callback tables and indirect calls a branch reaches or bypasses.
Keep interrupt requests, successful termination and other cleanup effects
separate; a wrapper's return instruction does not establish that its
interrupt or cleanup dependencies return.

## 31. Preserve output aliasing and the register origin of loop predicates

FND-CONFIG-164 reads a poll whose callers pass the same scratch address
for two outputs. Ordered stores overwrite the first value with the second,
while the loop predicate instead comes from a returned register copied
from the interrupt's BX. Treating the scratch word as the return condition,
or describing the two outputs as independent, would misread this path.

**Request:** shared effect summaries should retain output-argument aliasing,
ordered writes and the register or field that supplies each branch predicate.
Distinguish a wrapper's deterministic copies from the interrupt or external
callee's actual result sequence. This complements item 21's memory provenance
and item 29's loop-progress requirements.

## 32. Check whether validation precedes the access it appears to protect

FND-CONFIG-165 reads a pointer wrapper that dereferences metadata before
its later null test. A marker-failure branch sets a flag, but still reaches
the same runtime call and normal returned zero. The existence of those
checks does not establish a protected read, suppressed runtime request or
successful release. The named callers themselves test nonnull fields;
invalid native inputs or failures are not inferred from the local ordering.
FND-CONFIG-167 further traces a local runtime rejection result discarded by
the outer wrapper's zero return. This reinforces the difference between
clearing the caller's field and proving a successful operation.

**Request:** shared safety/effect summaries should check that a guard
precedes and controls each access or call it is said to protect. Preserve
failure-flag writes and downstream calls on rejected paths. Distinguish a
returned cleared pointer from a callee's successful resource release.

FND-CONFIG-171 extends item 32's guard requirement to freshly loaded
indirect targets: the nonnull test precedes a callee, then the call reloads
the pointer field. A check protects the actual target only with preserved
field and segment provenance. Shared summaries should retain intervening
writers/callees and distinguish a checked snapshot from a later reload.


## 33. Distinguish recursive error propagation from an originating error

FND-CONFIG-172 reads a MENU helper whose FFFF return only propagates
FFFF from its recursive call. Finite valid leaves return zero; an error
branch in its caller therefore does not establish a local error origin.
FND-CONFIG-168 retains that caller's conditional ordering without claiming
an ordinary failing invocation. Cyclic/invalid graph states and actual
termination remain separate provenance questions.

**Request:** shared call/effect summaries should follow each propagated
result to its producing leaf or external source. For recursive groups,
distinguish base-case results from values merely passed around the cycle.
An encoded error edge alone should not prove reachable failure. Retain
finite-traversal and valid-state assumptions rather than inferring either
an actual failure or unconditional successful termination.

FND-CONFIG-174 adds encoded caller failure tests around a helper that
normalizes every local normal return to zero and discards nested results.
FND-CONFIG-175 shows why a preservation summary should name the complete
callee paths and guard assumptions: its fixed copies preserve SI/DS on
bypass paths while changing ES, and zero can also mean a skipped copy.
These cases reinforce result-origin analysis without broadening it into
unconditional success or preservation claims.


## 34. Bound transform output counts independently of input counts

FND-CONFIG-177 reads a pairwise region operation whose private output
rejects a seventeenth append. Two input counts individually at most 16
can still produce more than 16 admitted pairs. A four-by-five conditional
case shows the local error origin without establishing that native
producers permit those duplicate records. The wrappers' later output copy
also does not turn the preceding work into a general rollback guarantee.

**Request:** shared format and effect summaries should distinguish input
count bounds, generated cardinality and destination capacity. Follow
pairwise expansion, splits and repeated appends to the actual write gate;
do not use each input's bound as proof that an output fits. Retain producer
invariants and local staging/alias assumptions separately from failure
reachability or atomicity claims.


FND-CONFIG-178 adds a syntactic upper bound of four append calls per
input-record iteration, with all candidate writes routed through the
private capacity gate. A control-path upper bound is not proof of a
feasible geometry case or an ordinary native input. Shared summaries
should report those distinctions explicitly and retain alias/guard
conditions when using a private-write bound to support a safety claim.

FND-CONFIG-182 further separates a path helper's copy count from its
explicit zero-byte position. The zero uses the original destination base,
while the copy advances by the prefix length. Shared summaries should
track the base of each write and signed or wrapped length gates; a limit
argument alone does not prove termination at the conventional boundary
or valid storage on every branch.

## 35. Reconstruct stack arguments through the callee before grouping pointers

FND-CONFIG-179 supersedes FND-CONFIG-169 after FND-CONFIG-180
checks the callee's argument widths. Grouping pushes by an adjacent
segment fixup had assigned the mask word to the callback offset. The
callee instead consumes a word mask, a far callback and a word identifier.
Its forwarding to separate setters confirms those boundaries.

**Request:** shared call reports should map pushed words into the callee's
BP-relative argument widths, accounting for near/far return frames and
explicit widening. A relocated segment operand locates a segment; it does
not by itself determine the surrounding argument boundary. Keep competing
groupings open until the consuming widths and forwarding settle them, and
supersede incorrect findings while updating active citations.

## 36. Preserve overlapping memory access widths across call summaries

FND-CONFIG-187's callers write bytes at 332C and 332E, while
FND-CONFIG-188's services test words at those offsets. A zero low byte
can therefore coexist with an active nonzero word guard through an
unwritten neighboring byte. Treating the named byte flag as the entire
callee predicate would incorrectly claim that active work is admitted.

**Request:** shared effect and provenance reports should retain every
read/write width and complete addressed byte interval. When an access
overlaps a wider consumer, keep the other bytes' producers as explicit
conditions, rather than merging them into the low-byte field's meaning.
Carry those conditions through normalized wrapper results and synthetic
fixture definitions. This complements item 26's return-register widths.

## 37. Separate ordinary memory transfers from hardware presentation evidence

FND-CONFIG-192 reads a graphics primitive whose common transfer path
programs VGA ports, while an overlap branch also uses a fixed scratch
segment. The memory counts can be derived for admitted positive inputs,
but ordinary RAM copying does not reproduce the hardware interpretation
or prove that the scratch segment is accepted native storage. Restored
DS/SI/DI also does not imply restored shared scratch or complete VGA state.

**Request:** shared static and emulation reports should classify port I/O
as a hardware boundary alongside interrupts, retain its placement on
common and conditional paths, and distinguish RAM effects from rendered
pixels. A fixture with substituted RAM or mocked port values must identify
what it actually tests and leave native output unconfirmed. Keep slot,
segment, count, mask and alias assumptions separate from the transfer
algorithm's local completion and register restoration.
