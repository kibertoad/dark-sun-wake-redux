---
id: FND-CONFIG-239
title: Installed ADV 10 uses the DIGPAK adapter whose table has no music-level query selectors
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008133F..0x00081348
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:03BC..4842:040C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0A23..4842:0B06
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0D06..4842:0D1E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A6E:011F..4A6E:0187
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0003E39E..0x0003E3D4
tool: installed_adv_resources.py and resident_listing.py at 21e06e41; pinned evidence Python, Capstone 5.0.9 and xxhash 4.0.1; Ghidra 12.1.3 read-only data/decompiler reports; reused FND-CONFIG-236/237/238 and FND-EXE-646
environment: null
---

## Observation

FND-CONFIG-237 records ten Copy-driver AE/AF/B1 interfaces and six
resources without those entries, but leaves ADV 10 outside its Copy-table
reader. This connected source/interface review answers that remaining
ordinary-installer classification. Addresses, offsets and selectors below
are hexadecimal; resource IDs, sizes and counts are decimal.

### Source identity selects a different installer interface

Installed RESOURCE.GFF is 5724669 bytes, XXH3-128
92fe031616f36062f99484b0f728210e. Its directory identifies ADV 10 at
file offset 8133F, size 2720, XXH3-128 c88f074c5d85fd620c0736dbbdd94efe.
The source signature is DIGPAK at resource offset three, not the Copy
signature at offset two. The manifest-checked resource/directory report
regenerated identically to FND-CONFIG-237's retained report for all
seventeen resources and the nineteen manifested disc ADV files.

FND-EXE-646 already describes the distinct installer branches. The complete
local reading confirms that 4842:0A23 first searches sixteen slot pointers
for a null slot. Exhaustion returns FFFF before signature selection. With
a free slot and the admitted image pointer, its DIGPAK comparisons precede
the Copy comparisons. A recognized DIGPAK image is not indexed by its
first word to find a Copy table.

The DIGPAK arm rejects an already-set CS:0E22 registration word. Otherwise
it publishes the selected slot and normalized image pointer, and places
4842:0D7E in that slot's table pointer. The image remains relevant to other
adapter operations; this does not exclude its code from the game. The
Copy arm instead uses the image's first word as its table displacement.
Actual image/slot/code/frame admission and later table writers are still
conditions of this local selection result.

### Complete fixed adapter table and missing-selector result

The resident adapter table has thirteen selector/entry pairs followed by
its FFFF selector terminator. Its complete selector set is 64, 65, 66, 68,
78, 79, 7A, 7B, 7C, 7D, 7E, 85 and 86. AE, AF and B1 are absent from
that finite table. Selector 64's independently evidenced target 4842:13DA
is a positive control for the table mapping. FND-CONFIG-237's ADV 11
Copy table supplies the contrasting positive AE/AF/B1 entries; the current
unchanged archive identity permits reuse of its thirty multiplicity controls.

4842:03BC rejects a slot outside the sixteen entries or a null table.
Otherwise it scans four-byte pairs, comparing the requested selector before
checking the FFFF terminator after a miss. For AE, AF or B1 in the unchanged
adapter table, it reaches that terminator in a finite scan and explicitly
returns DX:AX zero. The positive 64 request instead obtains the known target.
This is an instruction-based result, not a failed raw-byte search.

4842:03F4 reads the slot word from the caller's stack and invokes that
lookup. A null returned pair selects its ordinary far return at 040B;
the pushed-target far return at 040A is used only for a nonnull pair.
Thus a missing selector returns AX zero without dispatching any DIGPAK
image function or adapter interrupt. This conclusion requires the admitted
unchanged table, normal nonaliased frames and ordinary continuation. It
excludes neither table replacement nor independently entered driver code.

FND-CONFIG-236 connects the query wrappers and consumer: the game's getter
requests AE first and maps a returned zero to 255 before requesting AF.
Consequently a reached AE request on this admitted adapter binding selects
that sentinel result, rather than exposing an unread ADV 10 current-level
word. B1 is also a missing dispatch on that table; the game's setter has
already stored its capped library byte before making its B1 request. No
hardware or audible result follows from either missing request.

### Finite installed-source classification

For the seventeen unchanged installed ADV resources on the ordinary
installer route, FND-CONFIG-237's ten Copy resources provide the three
entries, its six other Copy resources lack them, and ADV 10 supplies the
distinct resident adapter, which also lacks them. This completes that
source-interface partition. It does not prove which slot any runtime
caller actually uses, bound a changed table, or classify other media and
indirect entry mechanisms. ADV 10's Copy-reader rejection remains a true
tool limitation; it is not an unclassified ordinary query interface.

## Interpretation

Q-CONFIG-014 and the shared Q-CONFIG-002 dependency can remove the unread
ADV 10 Copy-query-table obligation for this ordinary installed-source route.
The remaining questions are actual caller/slot/table/sequence and frame
admission, later or aliased writes, other interface entry mechanisms and
the state lifetime affecting the getter and byte store.

The whole RULE-CONFIG-003 still does not qualify for complete_reading.
FND-CONFIG-235/236 cover local entry/getter/setter and inspected direct
button stores; FND-CONFIG-237 covers conditional Copy query, setter and
reset bodies; FND-CONFIG-238 admits one restored caller/result/store path.
They do not locate every caller or every write to configuration, slot,
sequence, frame or readback state, nor resolve all current indirect targets.
The source partition above removes one named gap without answering those
other obligations or declaring a partial reading complete.

This finding establishes neither universal initialization success nor a
whole-entry status, queue or parity promotion. No original game process
or emulated function ran.

## Alternatives

- ADV 10 requires an unparsed Copy query table on the normal installer
  path: its verified DIGPAK signature selects the separate resident adapter.
- Every installed ADV directly supplies AE/AF/B1 code: the six Copy misses
  and distinct DIGPAK adapter contradict that source-interface claim.
- A missing adapter query invokes the image's interrupt interface: lookup
  returns null and dispatch takes the ordinary return before any target call.
- A finite source table proves its current runtime binding: source bytes
  establish neither the selected slot nor preservation from its producer.

## How to reproduce

Use the project-local evidence Python and installed_adv_resources.py with
GAME_DIR and spec/builds/BLD-GOG-EN-1.1.files.yaml. Require source identity,
all seventeen resources and ADV 10's DIGPAK classification, with ADV 11 as
the Copy positive. Retain resource-dimensions.txt under
GAME_DIR/analysis/readback-interface-review-20261108/. Its SHA-256 is
af42e2351b76675c662a1679e200cdb093d79a9d4fcd4e07648646694ad465d0,
equal to the earlier retained resource-dimensions report.

Use resident_listing.py for 4842:03BC..040C and 4842:0A23..0B06;
retain installer-lookup.txt there. Reuse FND-CONFIG-236's corrected query
wrappers and getter reading. In the retained INST-handler-20261106 Ghidra
project, read-only without analysis, request ReportDataBytes.java for
4842:0D7E, 56 bytes, and ReportDecompileWindow.java for 4842:03BC lines
1..80 and 4842:03F4 lines 1..60. adapter-query.log contains complete
27-line and 13-line decompilations and the table bytes. Only the first
54 data bytes are the thirteen pairs and terminator; the following word
is not another table entry. Read the first terminator and requested-selector
comparisons in the instructions, not inferred decompiler return types.
Keep all original-derived reports local. No new reporter was introduced.
