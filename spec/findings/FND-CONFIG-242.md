---
id: FND-CONFIG-242
title: The music getter's modeled direct and declared-pair entries reduce to the known Preferences call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A6E:011F..4A6E:0187
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B279..0x0008B281
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000873A9..0x000873AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000874EE..0x000874F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087538..0x0008753C
tool: direct_callers.py, alias_far_words.py, far_pointer_words.py and overlay_listing.py at df32a167; pinned evidence Python, Capstone distribution 5.0.9 and xxhash 4.0.1; Ghidra 12.1.3 read-only getter decompilation; reused FND-CONFIG-235/236/238/240 and FND-EXE-224
environment: null
---

## Observation

Q-CONFIG-014 retains caller and reentrant-entry admission for
RULE-CONFIG-003. This finite installed-source census distinguishes known
entry references from unresolved pointer construction. It does not try
to prove the getter can never run during exit callbacks. Addresses and
offsets below are hexadecimal; counts are decimal.

The reporters each verify installed DSUN.EXE, 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. FND-CONFIG-235/236's retained
entry-based Preferences listing independently places the getter call at
overlay 203 +0039, file 8B279. Its declared fixup maps descriptor 81
to resident 4A6E. The following instruction stores AL to DS:26B4 at
+003E. This prior instruction/result reading is the census's positive
control, not a caller inferred from the search itself.

### Direct call forms

direct_callers.py reports that one far call and no near-call pattern for
4A6E:011F. The far search covers MZ-relocation-backed call operands in
the resident image and declared-fixup-backed call operands in every overlay
code range. The near search covers the target's canonical resident segment
window using the script's sixteen-bit displacement arithmetic. It does not
search near jumps, indirect calls, return transfers or every segmented
spelling. Its output labels raw patterns; the sole positive is separately
instruction-checked by the existing Preferences entry reading.
The independent control target 4842:0B4D recovers the installer's near
call at 4842:0A9A after PUSH CS and the digital caller's far call at
49E9:01DC, both already established in FND-CONFIG-240. Thus the near
search has a working source control as well as the getter's far control.

### Declared segment and nearby raw-offset forms

alias_far_words.py examines 12965 declared MZ-relocation/FBOV-fixup segment
words, with zero unreadable segment mappings. For each segment capable of
forming the target with a word offset, it searches twelve bytes on either
side for that required raw offset word. The only candidate is the same
Preferences call: segment operand at file 8B27C and offset word two bytes
earlier. It is canonical 4A6E:011F, not a newly found alias.

This removes unclassified additional candidates within that exact unchanged
raw-offset/declared-segment model. It does not cover a raw word that becomes
the needed offset after its own relocation or later arithmetic; the
FND-PARTY-323/327 limitations remain applicable. No standalone all-form
far-jump, alternate-stream or runtime-pointer absence claim follows.

far_pointer_words.py supplies a second view of the canonical raw offset.
It finds eleven occurrences: five in the resident load image, five in
overlay code and one in an overlay fixup list. Only two have immediately
following declared segment words. One is the known Preferences call;
the other names 401B:011F, a different linear target. One further occurrence
is classified as a MOV immediate with no nearby declared segment word;
the remaining eight have no declared adjacent segment or typed MOV/PUSH
immediate classification. Incidental decoder renderings around those raw
words do not establish instructions or pointer use.

The MOV immediate is at overlay 198 +01DE, file 874EE. FND-EXE-224
already supplies the conditional descriptor-relative reading of its owner,
entry +0149, with all four bounded table dispatches. On that model this
arm puts 011F in DX, joins the later local tests and ultimately copies
DX to AX before returning far. There is no software call or code-pointer
publication in that conditional owner body. This is a returned word on
the known stream, not an encoded getter call or static far-pointer pair.
Its consumer could still use a returned word in an excluded runtime
construction; no global scalar-only use is inferred. The other overlay-198
raw occurrence lies in the owner-stream comparison at +0099, file 873A9.

The reused jump-table reading retains its actual CS/frame and dispatch
admission conditions. A linear window's diagnostic string comments are
not evidence that these numbers are text pointers. The current resident
Ghidra snapshot decompiles the getter completely in eighteen lines,
consistent with FND-CONFIG-236. It has no containing function for the
separate overlay query; that resident-view diagnostic is not evidence
against the conditional overlay reading or a general decompiler limitation.

## Interpretation

Q-CONFIG-014 can remove an unread census of these supported incoming-call
and unchanged declared-pair forms. All returned target candidates reduce
to the independently known Preferences call. The canonical raw-offset
occurrences add no second encoded target under the stated pair model.

The remaining caller obligation is actual Preferences root/event and frame
admission, including possible invocation during exit-time reentrancy, plus
unqueried jumps, indirect/table/return transfers, dynamically built or copied
pointers, more distant split words, changed streams and relocated-offset
reuse. Existing native evidence admits one restored Preferences path
(FND-CONFIG-238), not its whole invocation history. Relevant configuration,
slot, sequence and byte-store writer/lifetime conditions also remain.

This finite entry census cannot close the whole readback contract or prove
absence of reentrant reads. RULE-CONFIG-003 remains supported, with no
complete_reading declaration. No game process or emulated function ran.

## Alternatives

- Every raw 011F word denotes the getter: only one modeled declared pair
  names it; the other adjacent pair names a different segment.
- The returned 011F immediate is another encoded getter call: its
  conditional owner stream moves the word through DX into AX and returns.
- One known direct caller proves one possible invocation context: indirect,
  constructed, differently entered and reentrant paths remain unclassified.

## How to reproduce

Use the checkout-local evidence Python and the three existing entry reporters
with installed DSUN.EXE and target 4A6E:011F; add control 4842:0B4D
to direct_callers.py and require both known control sites. Under
GAME_DIR/analysis/music-getter-entry-census-20261108/, retain
direct-callers.txt, alias-pairs.txt and offset-words.txt. Require source
identity, 12965 readable segment mappings, the sole known pair and all
eleven raw-offset occurrences before reusing the census. Compare the
positive site against FND-CONFIG-235/236's preferences-connected.txt under
GAME_DIR/analysis/music-level-contract-20261107/.

Reuse FND-EXE-224's descriptor-relative table bounds and conditional owner
reading. overlay_listing.py with descriptor 198 and file windows
87310..87459 and 87459..8753C supplies offset-compare-owner.txt and
offset-immediate-owner.txt. Do not promote its excluded gap or printed text
comments to executed instructions or pointer semantics.

Read-only, with automatic analysis disabled, use the retained
inventory-reconcile-20261103/projects/INST-single-20261103b project and
DSUN.EXE program with ReportDecompileWindow.java at 4A6E:011F, lines
1..90. getter-result-decompile.log records the complete getter and the
separate missing-overlay-function diagnostic. The latter query did not
change the canonical project. Keep all original-derived outputs local.
No new reporter was introduced.
