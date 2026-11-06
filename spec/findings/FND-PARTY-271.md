---
id: FND-PARTY-271
title: Both known direct window-registration routes use WIND acquisition and the registration body spares child selector fields
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068914..0x00068978
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000689BB..0x00068A3F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059450..0x000594DE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:02B3..3A8E:05EF
tool: tools/research/exec-census/direct_callers.py, resident_listing.py and overlay_listing.py at e980ffef; Python 3.12.10, Capstone wheel 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 ReportDecompileMatches.java from scientific-method-engine 19.10.0; FND-COMBAT-040/088/089/093/095 and FND-UI-041/045 reuse
environment: null
---

## Observation

Q-PARTY-124 retains the variable child-resource type at 3A8E:03B4.
Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. This reads the connected direct
provider paths and separates their own selector writes from reached-child
or storage effects. It does not repeat the shipped child-tag census.

### Direct provider paths

The relocation/fixup-aware direct caller query for overlay 182 +00F6
returns the near call at +01AA and the overlay-172 far call at +00A6.
For its acquisition helper +00C4 it returns the near call at +0191
and overlay 172 +0078. These are the previously identified direct paths
in FND-PARTY-343, rather than absence conclusions from Ghidra references.
The independently read near and overlay-far calls control both forms.
Indirect calls, jumps, bypass entries and unsupported aliases remain
outside the query.

Overlay 182 +00C4 zeroes its own local far-pointer slot, zero-extends its
word number argument and requests literal WIND through 38FF:04AB.
It then returns the pointer slot. Its OR of the reader's AX status does
not branch or preserve that status as a separate success result before
AX is replaced by the pointer's low word. This agrees with
FND-COMBAT-040: a returned nonnull pointer is not itself a complete-copy
or successful-I/O certificate.

In the ordinary opener +016B, +0191 calls that helper with the supplied
window number. A nonnull returned pointer is forwarded to +00F6 at
+01AA without an intervening child-table write in the opener body.
The registration wrapper rejects null; otherwise it forwards that same
pointer and its two placement words to resident 3A8E:02B3.

The second path, overlay 172 +0020, also calls +00C4, at +0078.
It stores the returned pointer at 4C4D:0000, tests it, then passes that
stored pointer and two placement words to +00F6 at +00A6. Between
acquisition and that call there is no other child or own store through
the returned window pointer. This direct path does not construct an
alternative table of tags. The pointer global and call frames must still
retain their stated identity; storage aliasing is not excluded by the
nominal destinations alone.

FND-COMBAT-095 already establishes RESOURCE.GFF as the sole WIND
provider among the 26 fingerprinted top-level archives. FND-UI-002
and that finding account for all 28 source payloads, all 402 child tags,
zero byte +F2 and the zero high count byte +F4. The other 25 unchanged
archives and an unknown literal child tag in that source set are not
remaining alternatives. Runtime archive registration, modified providers
and transfer/storage identity are outside that source census.

### Registration selector and write domains

The resident registration loop uses the original window segment and
16-bit offset arithmetic to select:

`entry = window offset + 0x105 + zero_extend(window byte +0xF2) + 30*i`.

It tests i against the unsigned word at window +F3, not just the low
count byte. It reads entry +4 as the type and entry +8 as the resource
number before calling 38FF:0438 with zero optional length-output address.
Ghidra's connected decompilation agrees with those domains; instruction
reads decide byte versus word width and the unchanged segment.

The own child-entry stores are the returned pointer words at +2 and
+0, and rectangle words at +14, +16, +18 and +1A on the MENU/EBOX
branches. None of these stores covers entry +4..+B, the request's type
and number. The registration body's own window stores are link +EE,
placement +96/+98, rectangle +A6/+A8/+AA/+AC, flag changes +9E and
callback fields +F5/+F9/+FD. None covers selector byte +F2 or count
word +F3. This is a destination classification under valid, distinct,
nonwrapping window, child-resource and frame storage, not an alias proof.

The loop rereads the count and recomputes the entry after each child.
It also rereads the entry tag for constructor dispatch after acquisition.
The body calls resource and constructor helpers; it does not dispatch
the window's +F9/+FD callbacks on this registration path. Those callbacks
belong to later refresh in FND-UI-041. Reached helper writes or aliases
can therefore matter, but an unspecified refresh callback is not an own
registration-loop operation.

FND-COMBAT-093 records publication of the window head before region
and child setup. Thus the window is not proved private merely because
the ordinary opener has not yet installed its handler. For complete,
unchanged source payloads with the retained offset and count, the known
source table stays within its payload and has only APFM/BUTN/EBOX tags.
No whole-registration preservation conclusion follows from own stores.

## Interpretation

Both known direct registration routes use the literal WIND acquisition
helper. There is no separately constructed direct-caller tag table left
to classify in these routes. The own registration and caller stores do
not rewrite the request selectors under the stated storage conditions.
Reuse the existing sole-provider and complete child-table source bounds.

The live child-domain remainder is specific: incoming indirect/bypass
bindings; runtime archive/directory and transferred-buffer identity;
computed/copied/aliased or reached-helper writes that can change the
window pointer, byte +F2, word +F3 or selected entry type/number before
a request. Reaching a nonnull pointer does not remove incomplete-transfer
cases. Later refresh behavior and generic rendering success are separate
unless an actual effect reaches one of these selectors.

Q-PARTY-124 remains open; FMT-PARTY-007 remains supported. No complete
reading, answered queue deletion or original-game run is supplied.

## Alternatives

- A canonical-source bound does not prove that every live or partially
  transferred allocation contains that complete source.
- An unmodified child tag alone is insufficient if the table offset,
  count, pointer or request number selects a different location/domain.
- No own selector store does not mean every called constructor, allocator,
  resource operation or published alias preserves that storage.
- Direct-caller classification does not exclude computed or bypass entries.

## How to reproduce

Run direct_callers.py on installed DSUN.EXE for `182+00F6 182+00C4`.
Read overlay descriptor 182 at file ranges `68914..68978` and
`689BB..68A3F`, and descriptor 172 at `59450..594DE`, with
overlay_listing.py. Account for each pointer store and the exact two
near/far caller argument bindings, retaining all query exclusions.

Read resident_listing.py over `3A8E:02B3..3A8E:0440` and
`3A8E:0440..3A8E:05EF`. Reuse FND-UI-045's bounded five-tag dispatch
and FND-COMBAT-093's registration flow. In the reconciled read-only
INST-overlay-20261029g project, program INST-MAPPED.EXE, run
ReportDecompileMatches.java for 3A8E:02B3 with matches `0xf2`, `0xf3`
and `FUN_38ff_0438`. Check script completion, not merely process exit.

Reports remain under GAME_DIR/analysis/class-table-closure-20261106/,
with window-provider-caller-candidates, window-provider-wrappers,
window-secondary-caller-anchored, window-opener-handoff,
window-registration-head, window-registration-child-cases and
window-registration-decompile stems. Reuse FND-COMBAT-095's provider
census with its fingerprints and controls, not a new unguarded directory
listing. No original content is committed.
