---
id: FND-CONFIG-243
title: Preferences has two ordinary opener calls with distinct menu and keyboard admission gates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067EB0..0x00067F43
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006810E..0x00068139
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079DA6..0x00079DE3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079FFE..0x0007A024
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A204..0x0007A20C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B240..0x0008B281
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B6D0..0x0004B6E0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0007A226..0x0007A228
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0007A266..0x0007A268
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0007A284..0x0007A2C6
tool: direct_callers.py, alias_far_words.py, overlay_listing.py and trampoline_target.py at 799bad3b; report.mjs operand/table; pinned evidence Python, Capstone distribution 5.0.9 and xxhash 4.0.1; Ghidra 12.1.3 read-only mapped-view queries; reused FND-UI-046/055, FND-INPUT-012, FND-CONFIG-117/235/236/238/242 and FND-EXE-002
environment: null
---

## Observation

This addresses Q-CONFIG-014's ordinary Preferences root-to-readback
admission. It connects the two known caller sites with their event/key
gates and the already-read getter consumer. It does not establish actual
event delivery or exclude exit-time reentrant invocation. Addresses,
overlay offsets, file offsets and unlabelled constants below are hexadecimal; counts and button
IDs are decimal.

### Finite installed-source entry census

The reporters verify installed DSUN.EXE, 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. direct_callers.py for overlay
203 +0000 finds trampoline 5782:0048 and two far-call patterns:
overlay 181 +0281, file 68131, and overlay 190 +1EC4, file 7A204.
FND-UI-055 and FND-INPUT-012 independently identify those same sites
on their caller streams, supplying the source controls. No same-overlay
near-call pattern is reported. Control target 203 +0529 recovers all three
retained near calls at +0154, +0166 and +04ED on the button streams. This is
a controlled finite call-encoding result, not an all-entry absence proof.

The trampoline reader independently resolves 5782:0048 to 203 +0000
and 56B9:002A to the menu-close helper 181 +0612. The alias-pair
reader covers 12965 declared MZ-relocation/FBOV-fixup segment words,
with zero unreadable mappings. It finds the two known call operands and
one additional separated-word candidate in resident data. Thus a pair
search cannot be summarized as containing only the two calls.

### Game Menu button and forwarded key events

FND-UI-046/055 already describe the fourteen-control dispatch. In the
handler's local body, kind one selects its separate close route; kind six
forwards a non-Tab key packet to overlay 190's key routine, supplying
56B9:002A as close callback. This makes a key delivered through the menu
subject to the keyboard path below, rather than the button path.

Kind two reaches the button-selection route. Except for its special
zero/10313 controls, it first sets the selected control's frame to four
and calls 28C9:2A81 with one. It reloads the control word after those
children and then searches the fourteen-entry table. Consequently a
pre-child event snapshot alone does not establish the selected button
after a child that can change the frame.

The control-10303 arm at +025E sets its frame to five, calls 28C9:2A81
with one, calls menu close +0612, then unconditionally calls the
Preferences trampoline after ordinary return. That arm contains no
event-state or turn-combatant rejection. Parent/event admission and
the children can still prevent reaching it; this is not universal menu
availability or a preservation proof for readback state.

### P uses an extra state gate before the common turn check

FND-INPUT-012 bounds the first exact-key table at 33 words and the two
later dispatch tables at twenty words. No word in the unchanged exact-key
table folds to character p under `(key | 0x20) & 0xFF`. The p index in the
first later table selects +1A82, not the +1A8E entry used directly by
the character-window keys. The p index in the final table selects +1EC4.

+1A82 calls 28C9:2522 and rejects a full AX result of zero. The existing
getter reading FND-CONFIG-117 identifies that result as DS:0DAB, with
no child call. A nonzero result falls through to +1A8E, which rejects
signed DS:426D values at least four. Negative word values pass that
particular signed comparison; this finding assigns no valid gameplay
domain to them. P therefore has both the extra nonzero event-state gate
and the common signed turn check; it does not bypass the latter.

The continuing path processes the current window as already recorded in
FND-INPUT-012. At +1CBE it calls the supplied far close callback when
nonnull, then reloads the key word from its incoming frame and repeats
the character fold and index bound. An admitted p before the callback
does not guarantee p at this final reload. With p still present and the
tables/code/frame intact, dispatch reaches +1EC4, calls Preferences and
joins the ordinary key epilogue after its return. Callback targets,
their returning effects and actual key/frame retention remain conditions.

### The opener reloads its own readback gates

FND-CONFIG-235/236 supply the complete local readback component. Before
testing DS:13F7, the opener calls overlay 182 +1856 and +1FDB with
argument two. Nonzero 13F7 invokes 49E9:00FD and records its low-byte
result, optionally copying the library effects byte, then separately reads
DS:1436. Only a nonzero second gate reaches the music getter and its
immediate AL store at +003E. These are current reads after earlier children,
not values supplied by the caller's event admission. The opener does not
retest 13F7 after its intervening sound query. Any claim about actual
preservation must respect that order and the known child effects.

The mapped Ghidra view supplies a complete 32-line decompilation of the
opener agreeing with this order. Its menu function decompilation does not
resolve the computed button target, so the established table/instruction
reading decides that arm. Only the first seventy of the key routine's
1223 decompiler lines were requested; those are not a complete key reading.
The original-source bounded listings and reused table evidence supply the
specific key/branch claims above. No canonical project was changed.

### The additional data pair retains a consumption question

The candidate combines raw word 0048 at file 4B6D2 with the declared
segment word at 4B6D8, which loads as 5782. Unlike FND-PARTY-323's
excluded shutdown pair, this supposed offset word is not relocated:
the numeric loaded combination really can name 5782:0048.

FND-EXE-002's table base and eight-byte row layout identify these as
different descriptor fields. Descriptor 202 at file 4B6D0 has segment
477D, end offset 0048, flags three and start zero. Descriptor 203 at
4B6D8 has segment 4782, end offset 004D, flags three and start zero.
Thus the search combines the former row's resident extent with the next
row's segment. In the known table format these are not one stored code
pointer. The bounded table and operand readers independently control the
field values and relocation classification.

Those meanings do not prove that no other consumer can combine the
separated words as a pointer, copy them, or transform them later. Actual
pair consumption remains an explicit Q-CONFIG-014 entry-mechanism question.
Neither a data-row classification nor numerical target equality proves
an invocation. This prevents closing the complete declared-pair admission
obligation from the two direct calls alone.

## Interpretation

The two ordinary source caller sites now have connected event/key, child,
frame-reload and getter-store contracts. Q-CONFIG-014 can reuse those
local predicates instead of treating the menu and p calls as unread or
equivalent admission paths. FND-CONFIG-238 admits one restored native
Preferences caller/result/store path; it does not supply every event,
callback or uninterrupted initialization-to-opener history.

Retain the named cross-descriptor pair's actual consumption, other incoming
and constructed-pointer mechanisms, parent/root and exit-time reentrancy,
actual child/frame/gate retention and the configuration/slot/sequence and
byte-store writer histories. RULE-CONFIG-003 remains supported with no
complete_reading declaration. No whole queue item closes. No game process
or emulated function ran.

## Alternatives

- P bypasses the turn check: its extra state check falls through to it.
- The menu button and p have identical local gates: the button arm has
  neither keyboard rejection, while forwarded menu key events use the key path.
- A key sampled before close must choose the same final opener: the key
  is reloaded after the close callback.
- The pair scan yields only calls, or a relocated-offset false positive:
  the third candidate is a valid numerical combination of two descriptor fields;
  its actual use as a pair is unestablished.

## How to reproduce

Under GAME_DIR/analysis/preferences-root-contract-20261108/, retain
root-callers.txt from direct_callers.py for 203+0000 with control
203+0529, root-pairs.txt from
alias_far_words.py for 5782:0048 and trampoline-controls.txt for that
trampoline and 56B9:002A. Require source identity, both independently known
far sites, 12965 readable segment mappings and the additional data candidate.

Reuse the committed source-guarded overlay_listing.py for descriptor 181
67EB0..67F43 and 6810E..68139, and descriptor 190 79DA6..79DF0,
79FFE..7A024 and 7A1F8..7A20C. The last window's prefix starts inside
a previous instruction; only the independently established +1EC4 call and
following epilogue are used from it. The other windows start at the declared
root or established local targets. Reuse CONFIG-235/236's opener listing
and UI-046/055 and INPUT-012's independently bounded dispatch tables.

report.mjs table configurations retain the 33 exact keys, the p-index
entries at file 7A226 and 7A266, and the two descriptor rows at 4B6D0.
The operand configs at 4B6D2 and 4B6D8 retain the contrasting relocation
results. All configs fingerprint the same source. Keep their numeric reports
local; printed text guesses are not pointer-type evidence.

Read-only with analysis disabled, query INST-MAPPED.EXE in retained project
inventory-reconcile-20261029/projects/INST-overlay-20261029g with
ReportDecompileWindow.java at mapped 9604:0000 lines 1..90,
72CB:0000 lines 1..160 and 8314:139B lines 1..70. caller-decompile.log
preserves the bounded results and unresolved menu dispatch. Mapped addresses
are analysis bindings; original-source locations above delimit the claims.
All original-derived outputs remain in GAME_DIR. No new reporter was added.
