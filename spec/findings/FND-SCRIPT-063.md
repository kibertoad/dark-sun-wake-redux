---
id: FND-SCRIPT-063
title: Restore bitmap allocation and cleanup update the primary pointer without clearing the secondary pointer on null or release paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071C08..0x00071C41
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x000709A8
tool: overlay_listing.py at 6b0ec4cf; Ghidra 12.1.3 read-only INST-overlay-20261029g; FND-COMBAT-035/111/147 and FND-CONFIG-165/167 interface reuse
environment: null
---

## Observation

This reads the connected bitmap acquisition and reader cleanup relevant
to Q-SCRIPT-025's intervening child effects. Installed DSUN.EXE is
634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
FND-EXE-639 identifies acquisition overlay187 +1DD8; the complete body
and its reader caller are checked here for failure and pointer publication.

### Acquisition publishes the primary result before testing it

The helper requests 3136 bytes through 444C:0008 with a second
double-word argument one. On return it stores segment DX at DS:55BA,
then offset AX at DS:55B8. It tests the two returned words together.
If both are zero, it returns AL zero without storing DS:55B4/55B6.
The old secondary-plane pointer therefore retains its prior bits under
preserved DS and independent storage; it is not explicitly nulled.

On a nonnull result, the helper reloads the published primary words,
adds 1568 to its offset in word arithmetic and stores that offset and
the same segment at DS:55B4/55B6, then returns AL one. There is no far
segment carry in this addition. This describes pointer derivation, not
a returned-capacity or successful backing-memory guarantee. The shared
allocation/header obligations remain Q-CONFIG-013.

The bounded Ghidra view confirms the conditional secondary assignment.
Its aliases 5B7C:1BF8 and 5B7C:1BF4 denote primary DS:55B8 and secondary
DS:55B4 respectively. Instructions establish the actual segment, ordered
word stores, null test and low-word addition; inferred decompiler types
are not used as a far-pointer normalization contract.

### Allocation failure is not assigned to the transfer-success byte

The reader initializes local BP-1 to one before acquisition. A zero
acquisition AL calls the diagnostic with argument 1693. If that call
returns with the relevant bindings preserved, the reader continues to
the preparation helper and resource loop without an own store of zero
to BP-1. Preparation copies the primary pointer to SAVE/9's table slot
(FND-SAVE-021). It does not copy the secondary pointer.

FND-COMBAT-035's loop checks the placed-buffer equality/optional fill
before its null-slot skip. Provided that continuation is reached, a
null SAVE/9 slot skips its read without itself clearing BP-1. Neither
that skip nor BP-1 remaining one proves the bitmap allocation succeeded.
Other resource requests can still set BP-1 zero, and a diagnostic or
child may not return as assumed.

If the loop leaves BP-1 nonzero, the reader requests map reconstruction
+1CEE. FND-COMBAT-147 shows that helper gates on the map pointer DS:0538
and its GMAP read result, not on the earlier bitmap allocator result.
On its admitted zero-reader-result path, its two grid passes reread
primary DS:55B8 and secondary DS:55B4. Thus the earlier allocation-failure
diagnostic is not a local branch that necessarily prevents those bitmap
reads. Actual null/retained fields reaching them require the missing
diagnostic, transfer, pointer and child-preservation admission evidence.

### Shared cleanup clears only the primary field

After either numbered-loop outcome, the reader tests the full primary
pointer at DS:55B8. Null skips the release request. Nonnull is supplied
to 444C:0092, after which returned DX:AX is published back to that primary
field. FND-CONFIG-165 establishes the wrapper's explicit zero return on
its normal returning continuation. There is no reader store clearing
secondary DS:55B4 in this cleanup, before the clock copy and later
conversion continuation (FND-COMBAT-111).

Under preserved bindings and returning calls, the local states are:

| Path | Primary pointer | Secondary pointer |
| --- | --- | --- |
| Null acquisition return | zero | prior value retained |
| Nonnull acquisition return | returned pointer | derived offset plus 1568, same segment |
| Cleanup with null primary | zero | current value retained |
| Cleanup after returning release wrapper | zero | current value retained |

The release wrapper's zero is not evidence of actual deallocation.
FND-CONFIG-165/167 and FND-EXE-281/282/283 retain its marker, linked-header,
shared-DS, runtime and alias effects. Clearing the primary field does not
settle whether a separate trigger allocation or its metadata is preserved.

## Interpretation

Q-SCRIPT-025 can reuse these exact bitmap acquisition/cleanup postconditions
instead of treating both fields as jointly cleared or acquisition failure
as a transfer-loop failure. Remaining work concerns the real allocator
result, diagnostic continuation, current secondary value, selected transfers,
release metadata and aliases that can reach the trigger/stack/table state.
This is not evidence that a native load reaches invalid bitmap storage,
that a release succeeds or that a player-visible defect occurs.
No queue closure or complete-reading declaration follows.

## Alternatives

A null allocation does not imply a null secondary pointer: that field
has no store on the helper's null branch. A returning diagnostic is not
an assignment to BP-1. A release wrapper's returned zero does not clear
every alias or prove that its lower operation succeeded.

## How to reproduce

Run overlay_listing.py on the supported executable, descriptor187,
71C08..71C41, retaining trigger-restore-bitmap-allocation.txt under
GAME_DIR/analysis/handler-census-20261106/. Compare the existing
trigger-save-reader-connected.txt for 7089C..70BFD, particularly acquisition,
diagnostic continuation, BP-1 tests and primary release/store sequence.

In read-only INST-overlay-20261029g, program INST-MAPPED.EXE, use
ReportDecompileMatches.java at 7AC3:1DD8 with 55b4, 55b8, 0620, 620 and
at 7AC3:0A6C with 1dd8, 55b8, 55b4. Require both function markers and
the matching conditional assignments; retain
trigger-restore-bitmap-lifecycle-decompile.log in the same local directory.
Reuse FND-COMBAT-147's full-pointer gate and two bitmap passes and the
release interfaces above. No original run or emulated call was performed.
