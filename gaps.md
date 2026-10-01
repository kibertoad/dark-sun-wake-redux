# Upstream gaps observed during restoration work

These are the remaining requests for the restoration template and shared analysis
tooling. They describe tooling behavior, not claims about the original game.

Current website, toolkit and template main revisions are adopted, including the revised PR 27/16/28 changes and subsequent PE32 and conditional-access refinements. Website `ca39d07`, toolkit `b870642` and template `8d0eef3` add call-target, bounds, owner, incoming-coverage and carry/loop reporter capabilities; the facts gate passes and Dark Sun acceptance cases are recorded in docs/REPORTER-CASE-AUDIT.md. Game-specific requests close only after their own acceptance cases pass. Gaps 1, 2, 3, 4, 8, 10, 12, 13, 14, 16, 18, 19, 22, 23, 24, 25, 28 and 38 are removed after verification with the adopted tools; unsupported queries and partial searches remain open.
Delivered capabilities, closure evidence and remaining limits are recorded in
[the adoption record](docs/TEMPLATE-ADOPTION.md).

## 5. Define a portable inventory path for disc manifest entries

**Current disposition:** portable generation and synthetic/installed committed-identity checks pass; Merged PR 33 adds explicit evidenced legacy-path checking. Full disc inventory verification against its distinct executable remains pending.

The Survey rule asks for `coverage/<build ID>/<manifest path>.tsv`. The manifest
path `CD:DSUN.EXE` cannot be used verbatim as a Windows filename. This checkout
uses `coverage/BLD-GOG-EN-1.1/CD/DSUN.EXE.tsv` and retains `CD:DSUN.EXE` in each
start address. The join tool now accepts the manifest path explicitly.

**Request:** define a portable encoding of manifest paths for coverage files and
check that the path and each address prefix resolve to the same manifest entry.

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


## 39. Resolve the effective segment of frame-indexed accesses

FND-CONFIG-198 reserves an SS stack frame but accesses its numeric
BP-derived offsets through BX without a segment override. Those loads
and stores use DS. Naming the storage a local array before checking
DS/SS provenance would turn a conditional memory contract into an
unsupported safety claim.

**Request:** shared segmented-code reports should show the effective
segment of each memory access, including the default selected by the
final addressing register. Track an address copied from BP separately
from the segment used after it is moved or added to BX. A stack-size
reservation alone must not establish the capacity or identity of the
storage actually accessed. Keep DS/SS equality, frame aliases and
callee preservation explicit in static summaries and emulator fixtures.


## 40. Check cleanup-slot assignment on each failure edge

FND-CONFIG-199 identifies cleanup that reads a second frame word after
first-request failure skipped the second request and its assignment.
A sentinel test is present, but the local reading does not establish
that word's value on the bypass path. Successful-path initialization
cannot be carried backward onto every cleanup predecessor.

**Request:** shared static reports should track assignment separately
for each incoming cleanup edge, including partial acquisition and early
request failure. Distinguish sentinel comparison from index validation
and mark residual frame contents as unknown. Report the encoded read
and its missing local producer without promoting it to a native defect
until caller state and failure reachability have been established.


## 41. Account for the terminator separately from returned output length

FND-CONFIG-203's literal formatter path returns a character count
but writes an additional zero byte at that offset. FND-CONFIG-202's
caller supplies a six-byte frame area; its size alone does not establish
fit before the format and conversion outputs have been read.

**Request:** shared buffer summaries should report character count,
terminator writes, actual destination capacity and wrapped offset
behavior separately. A count equal to capacity is insufficient when
termination adds another write. Keep hypothetical long-input cases
separate from admitted native inputs and reproduced defects.


## 42. Separate requested bytes, allocator extent and clearing capacity

FND-CONFIG-209 and FND-CONFIG-210 distinguish a wrapped request,
a paragraph-based admission bound, a header-derived marker location
and chunked clearing through a returned far pointer. Treating any one
of these as the buffer capacity would hide the remaining allocator
and header provenance questions.

**Request:** shared allocation summaries should report arithmetic width
and overflow, admission units, returned pointer normalization, header
extent units and the range actually written separately. A bounded fill
chunk is not a total-capacity guarantee. Preserve lower allocator state
and effects as dependencies until read, and distinguish a local request
or marker write from a verified allocation contract.


FND-CONFIG-211 additionally identifies an accepted growth request followed
by alignment rejection and a null wrapper return. Allocation summaries
should retain preceding request effects and the identity of saved versus
later returned pointers; a failure sentinel does not itself establish
rollback or unchanged allocator state.


## 43. Preserve caller ranges when judging arithmetic admission

FND-CONFIG-212 identifies a signed high-word admission gate that is
not a universal unsigned bound, while the named allocation callers
supply a narrower nonnegative range for which it rejects overflow of
the intended local address range. A hypothetical large-pattern case
alone would misrepresent those callers' demonstrated input contract.

**Request:** shared arithmetic reports should show the encoded predicate,
normalization modulus and established caller input ranges separately.
Keep static counterexamples labeled as arithmetic examples until their
native reachability is established. A restricted caller range can support
a local conclusion without upgrading it to a universal safety claim.

