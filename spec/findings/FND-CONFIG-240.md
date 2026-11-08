---
id: FND-CONFIG-240
title: Copy-driver descriptor publication and caller slot results have distinct failure and lifetime conditions
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0A23..4842:0B73
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0293..4734:02ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49E9:018F..49E9:01E9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0591..4734:07FD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0003D9DA..0x0003D9DC
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0006C8DD..0x0006C8FB
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0006A7FE..0x0006A800
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0006ED84..0x0006EDA2
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0006E1B5..0x0006E1B7
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000715E2..0x00071600
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00070B34..0x00070B36
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00073F43..0x00073F61
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00071C91..0x00071C93
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00076048..0x00076066
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007581B..0x0007581D
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000783B1..0x000783CF
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000778B9..0x000778BB
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007AC59..0x0007AC77
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00078B48..0x00078B4A
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007D093..0x0007D0B1
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007C4DD..0x0007C4DF
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007FB2E..0x0007FB4C
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007D883..0x0007D885
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000845B3..0x000845D1
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00081EA6..0x00081EA8
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00088534..0x00088552
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00085E8B..0x00085E8D
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008ABDB..0x0008ABF9
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00089DC0..0x00089DC2
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008C081..0x0008C09F
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008B496..0x0008B498
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008ECAB..0x0008ECC9
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008DE82..0x0008DE84
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008FAF8..0x0008FB16
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008F346..0x0008F348
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0009254F..0x00092571
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0009154A..0x0009154C
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00092426..0x0009254F
tool: resident_listing.py, installed_adv_resources.py and adv_function_stores.py at ebb74e68; pinned evidence Python and Capstone 5.0.9; report.mjs table; Ghidra 12.1.3 isolated BinaryLoader projects and packaged recovery/instruction/decompiler/data reporters
environment: null
---

## Observation

This connected reading removes the local publication, descriptor-result and
caller-result ambiguity from Q-CONFIG-014. It reuses FND-CONFIG-239's
installed-source interface partition and FND-EXE-646's ordinary acquisition
route. Addresses, resource-relative offsets and selectors below are
hexadecimal; IDs, versions, limits and counts are decimal.

### Publication precedes descriptor acceptance

4842:0A23 selects the first null far pointer in the sixteen-entry slot
array at CS:0126. Exhaustion returns FFFF before publishing a new pointer.
After the recognized Copy signature, it adds the image's first word to its
incoming offset and publishes that table pointer at 0A8A/0A8F. Only then
does it request a descriptor through 0B4D.

0B4D supplies the slot, a far pointer to 4842:0A0F and selector 64 to the
lookup/dispatch chain already read in FND-CONFIG-239. It is not selector
66. On ordinary completion the installer rejects a null descriptor or
an unsigned descriptor first word greater than CS:03BA; otherwise it
returns the selected slot index. The installed resident comparison word
is 211. Its preservation from the source image is a condition, not a
writer census.

Neither post-publication rejection branch locally clears the slot. This
is a statement about those instructions, conditional on child continuation
and frame/storage admission. It does not establish that the unchanged
installed providers reach either rejection, or exclude child/aliased writes.
DIGPAK's separate publication and registration conditions remain as recorded
in FND-CONFIG-239.

### All sixteen installed Copy descriptor providers

The manifest-checked archive is RESOURCE.GFF, 5724669 bytes, XXH3-128
92fe031616f36062f99484b0f728210e. DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The unchanged source dimensions match
FND-CONFIG-237/239. In every Copy resource the first directory pair at
resource offset 2D is selector 64 with the entry below. This first-match
control avoids relying on a dictionary that might retain a later duplicate.
The returned descriptor's source first word is 200 in every case.

| ADV ID | Entry | Descriptor |
|---|---|---|
| 1 | 21A6 | 00C7 |
| 2 | 0C96 | 00C7 |
| 3 | 0B21 | 0073 |
| 4 | 2379 | 00C7 |
| 5 | 08F4 | 00C7 |
| 6 | 0B6B | 0073 |
| 7 | 21D8 | 00C7 |
| 8 | 0C29 | 0073 |
| 9 | 2372 | 00C7 |
| 11 | 27D4 | 00C7 |
| 12 | 2770 | 00C7 |
| 13 | 0E96 | 007B |
| 17 | 0CB2 | 00C7 |
| 18 | 0E9C | 0073 |
| 19 | 0879 | 00C7 |
| 20 | 1078 | 0073 |

The fifteen providers other than ADV 20 have thirty-byte bodies. Each
returns its own segment in DX and the listed nonzero descriptor offset
in AX, after writing its segment to descriptor offset +0A. The interior
IRET used to restore saved flags is the only call target in these bodies;
its opcode is independently checked at entry +13. Under ordinary
disjoint image/frame conditions these bodies do not change the version
word or call an external initialization helper.

ADV 20's thirty-four-byte provider at 1078 calls 1046, then unconditionally
replaces AX with 0073, sets DX to its own segment and writes that segment
at 007D. Both 1046 return values, zero and 00FF, are discarded. It is not
valid to equate the helper's failure result with descriptor-query failure.

The reached software helpers occupy 0F4F..1078. 1046 requests DOS's
current process segment, reads its environment segment and initializes an
internal environment cursor at CS:013C/013E. 0FAF searches environment
entries using 0F4F's fixed prefix comparisons and repeated byte scans.
On a match 0FDB reads environment bytes and writes three words at
CS:007F, 0081 and 0083. These writes and the cursor writes do not overlap
the descriptor version at 0073 in an admitted disjoint image. Neither
helper calls another software function beyond that finite chain.
The process/environment contents, direction and scan termination, DOS
effects, segment/frame aliasing and normal return remain explicit
conditions. No native environment or sound-device outcome was observed.

The installed provider versions are therefore acceptable against the
installed comparison word after admitted normal completion and unchanged
bindings. This source result removes an unread provider/version alternative;
it does not prove successful installation in every native configuration.

### Caller results and teardown are separate state

The music caller acquires its ordinary image through 47E5:0074 with
argument zero, publishes the returned pointer at DS:340D/340F and, after
the enabled gate, calls the installer at 4734:02D6. At 02DE it stores AX
in DS:343E before testing for FFFF. The digital caller uses acquisition
argument one and DS:3409/340B, calls at 49E9:01C2 and stores the result
at 01CA in DS:3436 before testing FFFF. Its successful continuation requests
the descriptor again. The current instruction boundaries put those two
installer calls one byte earlier than FND-EXE-646's contextual labels;
no other claim from that finding is superseded here.

A stored FFFF result thus need not describe whether the installer's local
slot publication already happened. It also is not a sequence handle:
FND-CONFIG-236's getter separately uses DS:3464, with DS:343E supplying
the driver slot for a reached query.

4842:0B06 clears both pointer words for an unsigned slot below sixteen,
before checking whether to clear the matching DIGPAK registration flag.
An argument FFFF skips that clear. The body neither releases the image
nor stores to the game's DS:343E word. A subsequent ordinary lookup on
a still-null slot returns zero; the getter's reached AE query then selects
255 as described in FND-CONFIG-239. This requires that no intervening
publication or alias has replaced that slot.

The complete local teardown body 4734:0591..07FD contains conditional
child cleanup and release calls, but no direct call to 4842:0B06. Its
final stores clear the music image pointer at 07D1, music slot word at
07DA, digital image pointer at 07E0 and digital slot word at 07E9.
Those slot stores write zero, not FFFF, and do not themselves clear
the separate library array. Earlier child effects and actual image
ownership/release cannot be inferred from these final stores.

## Interpretation

Q-CONFIG-014 can reuse the connected local producer/provider/result
contract instead of treating descriptor selector, source versions,
publication order or the two caller-result destinations as unread.
It still needs the actual slot/table/code/frame admission and continuous
writer/lifetime histories: other callers, reset/removal callers and
reached cleanup effects; changed source/table/compatibility words;
aliasing or computed/copied writes; sequence publication/current-level
writers and other media or indirect entry mechanisms. ADV 20 additionally
retains actual DOS/environment and termination admission.

RULE-CONFIG-003 remains supported with no complete_reading declaration.
This partial map neither closes Q-CONFIG-014 nor establishes universal
initialization success, an audible result or a native failure. No game
process or emulated function ran.

## Alternatives

- Descriptor-helper failure necessarily makes ADV 20 installation fail:
  the provider discards that AX result before returning its descriptor.
- Installer failure locally undoes its Copy publication: neither
  post-publication rejection branch contains a clear.
- Clearing the game's slot word removes its library table: the words
  belong to distinct state, and the local final stores do not access the array.
- Source version acceptance proves native readiness: callback, environment,
  frame, alias, table-preservation and later lifecycle admission remain.

## How to reproduce

Use the checkout-local evidence Python and the existing source-identity
reporter installed_adv_resources.py with GAME_DIR and the build manifest.
Use adv_function_stores.py RESOURCE.GFF 64 to locate candidate entries;
control the result with the first directory pair, not last-match dictionary
semantics. With report.mjs table, read each source resource's directory word,
first selector/entry pair, descriptor first word and interior IRET opcode.
Use source-identity guarded tables; no game code is executed.

Under GAME_DIR/analysis/driver-slot-handoff-20261108/, retain
resource-dimensions.txt, descriptor-candidates.txt and the per-resource
fields configurations/results. Source-bound table configs contain one row
with the resource size as stride and the named relative fields. The resident
compatibility config reads one two-byte word at file offset 3D9DA.

Reuse FND-CONFIG-237's bounded resource-copy procedure to put each verified
Copy slice in its own local BinaryLoader x86:LE:16:Real Mode project at base
zero, with analysis disabled. Recover the named 64 entry using
RecoverCitedFunctions.java, then request ReportInstructionWindow.java
(entry,25), ReportDecompileWindow.java (entry,1,100) and bounded
ReportDataBytes.java. Check each actual return rather than treating the
instruction window's end as a function end. The fifteen ordinary bodies
end at entry +1E; ADV 20 ends at 109A. Recover its helper entries 1046,
0FAF, 0FDB and 0F4F and inspect the complete chain. The retained provider
logs and adv20-descriptor/helper/env/compare logs preserve those readings.
Interior IRET endpoints need the original encoded byte control, as in
FND-CONFIG-237.

Use resident_listing.py for 4842:0A23..0B79, 4734:0293..02ED,
49E9:018F..01E9 and 4734:0591..07FD; retain producer-consumers.txt and
teardown-caller.txt. Reuse FND-CONFIG-236/239 for the query consumer.
Ghidra 12.1.3 renders register-form segment loads backwards: the helper's
104D loads ES from BX, verified from encoding and independent decoding.
Its p-code direction is correct. Keep the known upstream display defect
separate from the source behavior; see docs/GHIDRA.md. Retain all original
bytes, derived reports and projects only in GAME_DIR. No new reporter
was introduced.

