---
id: FND-COMBAT-131
title: Character selection can produce FFFF and Save/Load navigation reads the inherited marker before normalizing that selection
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058230..0x00058336
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058336..0x00058475
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059014..0x0005915D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D54B..0x0007D6AC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DA85..0x0007DAC3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DAC6..0x0007DD35
tool: tools/research/exec-census/overlay_listing.py at 4c75189a; existing shared-field and navigation mapping FND-PARTY-046, FND-SAVE-026 and FND-COMBAT-085/129
environment: null
---

## Observation

Q-COMBAT-046 retains shared selection producers/lifetime, and
Q-COMBAT-063 needs that value for the selected load path. This
connects a known explicit sentinel producer to the consumer's read
ordering. It does not establish a native cross-screen transition,
valid table bytes or child preservation. Offsets below are hexadecimal.

### The character list has an explicit no-selection value

Overlay 171 +0126 writes zero to shared selection word 4C4C:0000
after its nonnull allocation branch, before list setup. It later calls
+0020, installs/draws the window and tests the current selection against
FFFF. Equality requests the no-available-character message and calls
+0E04; inequality returns without that cleanup call. FND-PARTY-046
already identifies the confirm handler's separate FFFF rejection.

+0020 scans candidate entries using current selected row plus top.
When its first scan finds no eligible entry, it resets top and selected
row to zero, marks its local retry byte and scans again. Failure of
that retry stores FFFF to 4C4C:0000 at +0115. Success bypasses the
sentinel store. Its final own operation requests 28C9:2A81(0) before
return. Eligibility inputs, counts, row extents and child behavior
remain requirements for choosing the actual outcome.

The complete cleanup +0E04 clears window pointer 4C4C:0004 and
conditionally releases DS:40C8, then makes interface and member-specific
calls as mapped by FND-EXE-609. Its own stores do not clear or clamp
4C4C:0000/0002. Those children and subsequent screen/event routes can
still alter the fields. A sentinel from the scan is therefore a concrete
producer requiring lifetime admission, not proof that every later screen
inherits it. Closing a window is not an own selection reset here.

### Save/Load setup consumes rather than initializes selection

FND-SAVE-026 reads overlay 192 +024B's ten-record request and absence
of an own selected-row/top reset. Its first selected-control request
adds 4780 to current selection at word width. With selection FFFF,
that control number is 477F, outside the ten row numbers 4780..4789.
This does not assign a meaning or effect to the requested control.

Later it calls +0DF4 with the current selected word. For stable top zero,
selection FFFF and retained list pointer B:O, that helper's description
pointer uses B:(O -125 +80 modulo 65536). The later load-path producer
in FND-COMBAT-129 similarly uses B:(O -125 modulo 65536) as its path
pointer if the same values remain when it forms the argument. Neither
observation proves those offsets name valid text or an actual filename.

### Navigation's first marker read precedes movement

FND-COMBAT-085 gives the callback's event-six mapping and complete
ordinary 0..9 cases. The 5000 and 4800 branches at +07C6 and +08E2
first reread Load/Save mode. In nonzero Load mode, each computes the
current indexed record and reads its first byte before the signed
comparison that increments, decrements or wraps selection. The old-marker
tests are at +07F1 and +090D respectively. An empty marker only skips
the old-control-state request; it does not retroactively guard that read.

Thus with the stated stable top-zero/FFFF inputs, each Load branch
first reads the marker through B:(O -125 modulo 65536), before its own
selection correction. A claim that the navigation key repairs selection
before any indexed access is contradicted by that ordering.

If this read and intervening calls permit continuation with fields
preserved, the 5000 branch treats signed FFFF as below nine and its
first movement increments it to zero. The 4800 branch treats it as at
most zero and its first wrap stores nine. Load mode's subsequent marker
searches still follow FND-COMBAT-085, so these first corrections are not
assertions of the final row. In Save mode, the initial marker read is
skipped; the old-control request still uses selection plus 4780 before
movement, and the single movement gives zero or nine respectively.

For general stable word top T, the indexed record uses low16(T+FFFF).
Consequently T zero gives index FFFF, while T one gives index zero.
The sentinel is not universally an out-of-request index independent of
top. Capacity, offset wrap, actual top and storage identity remain
conditions; no invalid native record or input sequence was constructed.

## Interpretation

An assumed initial selected row 0..9 cannot be justified solely by the
existence of ten Save/Load controls. A neighboring subsystem has an
explicit FFFF producer, cleanup has no own reset, and Save/Load's own
setup consumes the field. The navigation bodies can normalize that
value only after an earlier Load-mode indexed read.

This removes the sentinel producer, own cleanup/reset distinction and
pre-normalization marker ordering from unread work in Q-COMBAT-046/063.
Actual scan eligibility, returning children, later field preservation,
event delivery, valid list contents and native paint remain open. The
existing 0..9 navigation results stay valid under their stated assumptions.
No question closes and no rule/parity status is promoted.

## Alternatives

- The character-list confirm guard does not establish a Save/Load guard.
- Window cleanup does not itself reset the shared selection word.
- A later normalized row does not validate an earlier marker read.
- FFFF plus a nonzero top need not index the same record as FFFF plus zero.
- A known sentinel producer does not prove a native cross-screen failure.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. overlay_listing.py verifies length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read the listed descriptor-171 and
descriptor-192 ranges from their inventoried entries or the branch
targets already resolved in FND-COMBAT-085. Track the two scan-local
bytes, zero reset, retry sentinel and parent cleanup separately. In
navigation retain the entry's local retry-byte initialization, the old
marker read and subsequent signed movement comparisons. Compare
FND-SAVE-026 and FND-COMBAT-129 for current-base/top arithmetic.

Reuse save-shared-page-producers.txt and save-shared-page-consumers.txt;
retain save-selection-sentinel-cleanup.txt and
save-selection-sentinel-navigation.txt only under
GAME_DIR/analysis/hit-incoming-20261104/. The ranges and query values
above suffice to regenerate the claimed listings. No original bytes,
window image or native invalid-selection experiment is committed or run.
