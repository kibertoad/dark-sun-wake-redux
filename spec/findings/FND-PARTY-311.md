---
id: FND-PARTY-311
title: APFM selector callers include a menu-cleared pointer path while the lookup setter has no tested incoming forms
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3BA6:0B5C..3BA6:0C12
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3BA6:0FA4..3BA6:113B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:059E..3D72:0629
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0635..3D72:06D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:021D..3D72:022A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0755..3F96:07B3
tool: direct_callers.py, far_pointer_words.py, alias_far_words.py and resident_listing.py at 7682c86e; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with packaged ReportDecompileMatches.java; FND-COMBAT-087/092, FND-CONFIG-095/097 and FND-PARTY-309
environment: null
---

## Observation

Q-PARTY-134 retains actual APFM record provenance before applying
FND-PARTY-309's unchanged-shipped-field exclusion. This traces the
concrete incoming forms of the two known pointer producers, rather
than assuming the lookup setter runs during member selection.
Installed DSUN.EXE is size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets are hexadecimal.

### Lookup setter is absent from the tested incoming forms

For 3F96:0755, direct_callers.py returns no encoded direct call;
far_pointer_words.py returns no matching adjacent relocated/fixed-up
far pointer. The alias-aware twelve-byte-neighborhood census examines
all 12965 segment words with none unreadable and returns no pair for
that target, including equivalent segment:offset aliases.

Controls are the independently known 3F96:02AB event-callback setter
calls, including 172+0981, and the selector call inside 0755 itself.
The same procedures recover those controls. These negative results
do not cover runtime-built target words, producer distances beyond
twelve bytes, unrelocated targets, near jumps, differently aliased
relative transfers or entries bypassing the root. FND-COMBAT-087/092's
local lookup/selection contract remains valid, but this census does
not admit an actual call to it from the cast-window interval.

### Four encoded selector callers

The direct census for 3D72:0635 finds these four established sites:

| Caller owner | Call | File offset |
|---|---|---|
| 3BA6:0B5C | 0C04, far | 31864 |
| 3BA6:0FA4 | 1115, far | 31D75 |
| 3D72:0009 | 0227, PUSH-CS/near | 32B47 |
| 3F96:0755 | 07A5, far | 35305 |

Each site is checked against its containing inventory body, not the
arbitrary paragraph address printed by the raw call search. The
near-call control is reached in the pointer-input routine; its earlier
selection publication is in FND-CONFIG-095. The menu callers and
0755 do not pass a new record argument to 0635: it reads the selected
record/tag state as FND-COMBAT-087 describes.

### A concrete non-resource pointer state

On the continuing 3BA6:0FA4 menu-selection path, 10F7 clears the
entire far DS:A125 pointer. Its own body has no A129 tag assignment.
At 1100 it tests A125 and its retained selection word; it may call
3D72:059E(1), then 1115 calls the selector. Thus pointer zero with
a retained tag is a specific producer history, not just a hypothetical
record built elsewhere. The earlier menu/path inputs and child
returns still need admission to reach that history.

059E reads the payload tag through A125 without an own null test.
Its only tag-specific children are BUTN and MENU, as recorded in
FND-CONFIG-097. If memory at the null pointer matches neither tag
and the stack guard is bypassed, it returns zero without a child
or an own repair of A129. Those memory and guard conditions are
not inferred from the shipped APFM resources.

Under retained segments/frame/global state, if A129 still says APFM,
0635 therefore reads through 0000:0000. Its four signed-coordinate
gates use words 0000:0020, 0024, 0022 and 0026, against current
DS:A109/A10B. Passing them reads 0000:0066 into A13F. Mode three
is selected first when A141 is nonzero and current A15B equals the
saved null A125. Otherwise, reaching mode two requires both double
words 0000:005A and 0000:005E nonzero. Its copy tail then publishes
current A125 into A15B. Interrupt-vector contents, their writers and
the coordinate/mode histories are separate evidence; no actual
values or native callback are asserted here.

The 0B5C caller has a different local guard. Its pointer clear at
0BE4 lies behind an explicit A129==MENU test at 0BAE. With that tag
unchanged, the following selector rejects APFM admission and chooses
its mode-zero tail. A helper or external tag change would need its
own evidence. The two menu callers cannot be treated as identical
null-APFM producers.

## Interpretation

The lookup setter is removed from the tested encoded incoming set;
its excluded transfer forms remain. The selector's current four
encoded callers are classified, and the remaining non-resource case
names a concrete clear-without-tag-clear path in 0FA4. Applying the
shipped-zero-field exclusion requires actual selected-pointer/tag
identity, not merely an APFM tag or nonnull prior A15B.

Q-PARTY-134 retains admission of these caller paths in the interval,
changed field and coordinate/mode histories, null-state memory and
excluded transfers. FND-COMBAT-095's unchanged shipped child graphs
contain no MENU entries; finding a menu helper is not proof that a
cast-window event reaches it. Q-PARTY-136 retains the intervening
guard/frame/segment/global preservation conditions. No whole client,
queue item or complete reading closes; FMT-PARTY-007 remains supported.

## Alternatives

- A saved lookup identifier alone binds the active graphics record:
  FND-COMBAT-087/092 distinguish that lookup from current selection.
- A pointer clear necessarily clears its separate tag: 0FA4 has no
  own tag assignment on the examined path.
- A null selection must return before field reads: 0635 has no own
  null check; tag, coordinate and mode gates decide its continuation.
- The menu clear establishes a native stale callback: path, state and
  interval admission remain unresolved, including the shipped MENU absence.

## How to reproduce

Use direct_callers.py for 3F96:0755 and 3D72:0635. Use
far_pointer_words.py for 0755 and alias_far_words.py for 0755,
0635 and control 3F96:02AB. Keep all search exclusions above.
Read resident 3BA6:0B5C..0C12 and 0FA4..113B from their established
entries, 3D72:059E..0629 and 0635..06D5, and the pointer-input
root through 0227. The selection-clear report includes an exploratory
suffix at 0629..0635 that is a table, not helper instructions.

Bounded Ghidra matches for 0635 at 3ba6:0b5c and 3ba6:0fa4 recover
both known selector calls; instructions decide the full pointer clear,
separate tag and guards. The requested a125 spelling supplies no
independent negative evidence when absent from decompiler text.
Retain cast-apfm-mode-setter-callers/pointers/alias-pairs,
cast-apfm-selector-callers/other-callers/pointer-caller,
cast-apfm-selection-clear-helper and selector-callers-ghidra logs in
GAME_DIR/analysis/handler-census-20261106/. Exploratory paragraph-alias
windows are not established roots. No original game, emulator session
or emulated call ran.
