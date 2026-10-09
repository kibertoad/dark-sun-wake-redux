---
id: FMT-EXE-005
title: FBOV overlay code block with its fixup list
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE", "CD:DSUN.EXE"]
byte_order: little
size: null
text: false
definition: fmt_exe_005.ksy
evidence: [FND-EXE-003, FND-EXE-005]
conflicting: []
split_with: []
related: []
---

## Layout

One overlay's block in FMT-EXE-001's `payload`, at the `payload_offset` its FMT-EXE-003 header
gives, with that header's `code_size` and `fixup_size`. Overlay code is located in the spec by its
file offset: overlay 169's code starts at `DSUN.EXE+0x00057580` [FND-EXE-003].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | `code_size` | `BYTE[code_size]` | `code` | The overlay's code and data. A trampoline's `target` is an offset in it. | supported | FND-EXE-003 |
| | `fixup_size` | `UINT16LE[fixup_size / 2]` | `fixups` | Offsets in `code` of 16-bit words that each hold a segment-table index shifted left by three, with the low three bits 0. Every offset leaves room for its word inside `code`. | supported | FND-EXE-005 |
| | | | | Total size `code_size + fixup_size` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 49 blocks of the installed `DSUN.EXE`, with 8,262 fixups, and the sizes of all 49 of the
disc's [FND-EXE-003, FND-EXE-005]. FND-EXE-173 additionally validates both sources'
fixup-table dimensions, operand bounds and descriptor-index admission through the
bounded source reader; this does not establish native loader behavior.

## Open questions

- Q-EXE-010 retains overlay body reconciliation. FND-EXE-221 independently
  bounds an adjacent dispatch table, but three candidate targets lack decoded
  instruction starts in the corrected snapshot. Table dimensions do not
  establish runtime CS or complete body ownership. FND-EXE-273 reads one
  allocation unlink helper; live link/header admission, physical aliases and
  the remaining allocator contracts stay open. FND-EXE-274 reads the larger-count
  split helper; incoming segment identity, header writers and arithmetic bounds
  still prevent allocation closure. FND-EXE-275 reads the fallback alignment
  call sequence; its callee contract, failure effects and returned extent remain
  unresolved. FND-EXE-276 follows its request callee through guards and a
  saved-state return; arithmetic helpers, state updater and state writers remain
  open. FND-EXE-277 resolves local arithmetic and comparison helpers with
  word-width wrapping; state admission, updater effects and writer coverage
  remain open.
  FND-EXE-278 follows the final updater's ordinary cleanup and failure-bound
  publication; its far callee, state writers and admitted ranges remain open.
  FND-EXE-279 follows that far wrapper and error-helper cleanup; the interrupt,
  post-interrupt preservation and state/table writers remain unresolved.
  FND-EXE-280 follows the initial allocation helper's separate calls and shared
  publications; admitted state, remaining writers and interrupt effects stay open.
  FND-EXE-281 adds a segment-qualified list-link writer candidate; callers,
  segment admission, other writers and publication timing remain unresolved.
  FND-EXE-282 establishes one writer call's DS producer and a merge fall-through
  into unlinking; incoming state, other callers and header admission remain open.
  FND-EXE-283 adds selected cleanup's ordered head publications and retained
  outgoing-pair provenance; state admission, writers and lifetime remain open.
  FND-EXE-284 adds a relocation-backed gated cleanup caller; its earlier
  callees, pair/state writers and excluded caller kinds remain unresolved.
  FND-EXE-286 reads the two preceding callees' local publications and AX
  handling; nested calls, DS provenance and state writers remain unresolved.
  FND-EXE-287 resolves those nested wrappers and the temporary data segment;
  interrupt effects, preserved outer DS and pointer/state writers remain open.
  FND-EXE-288 adds the saved-pointer and byte-gate producer; interrupt-result
  admission, other writers and lifecycle callers remain unresolved.
  FND-EXE-289 supplies a relocated setup-call candidate without a grounded
  instruction path; caller reachability and state provenance remain open.
  FND-EXE-290 follows a previously named setup entry through that call and
  shared publications; incoming caller admission, allocation preservation,
  the second setup callee and state lifetime remain unresolved.
  FND-EXE-291 resolves that callee's local argument writers and hidden
  interrupt result; interrupt effects, DS admission, other writers and
  incoming caller provenance still prevent closure.
  FND-EXE-292 grounds one incoming path and its supplied-pointer arguments,
  including unchecked gate-setter continuations; storage production,
  upstream admission and state lifetime remain unresolved.
  FND-EXE-293 reads that storage producer's request increment and unchecked
  header-derived byte write; header production, returned extents and state
  admission remain unresolved.
  FND-EXE-294 connects the allocator's selected DX and DS and bounds one
  conditional header-derived address calculation; actual segment admission,
  header stability, aliases and extent lifetime remain unresolved.
  FND-EXE-295 adds another shared saved-DS writer and its nested allocator
  path; incoming callers, remaining helpers, aliases and state lifetime stay open.
  FND-EXE-296 follows its larger-existing-count helper's ordered header
  publications and discarded callee results; the other helper, admitted
  extents, aliases and state lifetime remain unresolved.
  FND-EXE-222 supplies conditional source-decoded tails at those candidates;
  saved-listing incompleteness alone does not exclude their decoding.
  FND-EXE-223 retains three unresolved computed transfers in the outer
  candidate; its whole-function body is not supplied by those tail readings.
  FND-EXE-224 resolves those traversal omissions under an explicit descriptor
  binding; native segment and ownership admission remain unresolved.
  FND-EXE-225 connects its entry to a stored resident trampoline target and
  records both omitted candidate bytes and extra analyzer-owned chunks.
  FND-EXE-226 supplies resident handler callee leads, retaining the unresolved
  far callback and native frame/segment admission before attributing transfers.
  FND-EXE-227 identifies the resident far-jump rewrite's separate segment
  source; live header production and post-call count/segment admission remain open.
  FND-EXE-228 supplies the segment-field producer store and retains its
  state-word, saved-header and intervening-callee admission dependencies.
  FND-EXE-229 separates a helper's word/carry results from the published
  segment reload; remaining state and callee admission is still required.

- Whether the loader replaces each fixup word with the segment its descriptor names, and which
  segment that is for a descriptor of an overlay (FND-EXE-007, Q-EXE-001).
  FND-EXE-175 supplies a bounded resident candidate; its caller admission and
  state/segment producers remain unread, so it does not settle the loader.
  FND-EXE-176 resolves its initial state segment and vector pointer; their
  writers and native admission still require reading. FND-EXE-252 adds a
  gated vector-exchange consumer with two unresolved cleanup callbacks.
  FND-EXE-178 separates their shipped defaults from concrete later writer leads.
  FND-EXE-179 records intervening callee restoration and external dependencies.
  FND-EXE-180 follows the paired external-target producer; live preservation
  and provider correspondence remain unresolved. FND-EXE-182 supplies a
  bounded shipped-interpreter match whose field/registration admission is open.
  FND-EXE-183 traces its packed-pointer producer and bounded counter return;
  counter admission, failure behavior and callback setup remain unread.
  FND-EXE-184 follows setup ordering and result consumption; callee effects,
  state admission, metadata extents and dispatch remain unresolved.
  FND-EXE-185 bounds the selected builder arm and its return; backing memory,
  admitted destination extent and callback dispatch remain unread.
  FND-EXE-186 reads the table consumer and return widths; selector/table
  producers, target contracts and native admission remain unresolved.
  FND-EXE-187 traces a backing-base producer and controlled CRT imports;
  input extent, loaded CRT effects, nonlocal continuations and lifetime remain open.
  FND-EXE-188 records shared and imported record-storage paths; state admission,
  callee effects, aliases and nonlocal continuations remain unread.
  FND-EXE-189 resolves local selector publication and a constant-callee return;
  gate/state producers, imported effects and waiting behavior remain open.
  FND-EXE-190 separates new and adopted shared-record publication; adopted
  extent, helper decoding, source globals and lifetime remain unresolved.
  FND-EXE-191 reads fixed-prefix pointer recovery; initialized input,
  admitted identifiers, failure completion and record lifetime remain open.
  FND-EXE-192 closes local query prefix/tail extent; existing atom provenance,
  runtime suffix preservation and returned initialized extent remain unresolved.
  FND-EXE-193 adds callback-slot consumers and a field +4 mutation path;
  setter inputs, other writers, copied targets and record lifetime remain open.
  FND-EXE-194 follows callback continuations and the failure import; indirect
  setter uses and loaded callee effects still prevent a complete reading.
  FND-EXE-195 adds a controlled negative literal-pointer search; excluded
  representations and runtime state still require evidence.

- Which analyzer-owned body fragments are native code under the original CS
  bindings (Q-EXE-010)? FND-EXE-173 separates their physical source regions
  from their valid overlay entries. FND-EXE-174 shows that fresh analysis after
  relocation-pair repair still assigns non-code fragments to overlay bodies.
  A descriptor-base dispatch reading keeps
  eleven targets inside code; the analyzer-alias reading places most outside.
  Native segment producers and every admitted target must settle those competing
  interpretations before the affected boundaries or denominator are accepted.
