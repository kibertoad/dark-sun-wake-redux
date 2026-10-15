---
id: FND-COMBAT-035
title: Numbered restoration reads effect rows before the count through the complete-resource reader
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x00070964
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070B4A..0x00070BFD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070CEE..0x00070D63
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04AB..38FF:04D5
tool: tools/research/exec-census/immediate_search.py, direct_callers.py and overlay_listing.py, resident_listing.py; installed-build fingerprint and relocation/fixup checks
environment: null
---

## Observation

The encoded-operand query for 157E and 1580 locates both the known
numbered-save writer (FND-COMBAT-034) and a second indexed consumer
in overlay 187 +0A6C. The known writer is a positive control for these
literal/displacement candidates; this query does not cover computed,
aliased or bulk accesses. Reading from the second consumer's entry
qualifies its numbered resource-request loop rather than relying on
the raw candidate addresses as instruction identity.

The entry is exported through 56EF:007F. Direct-call candidates include
local calls at +0F2C and +121B. Exported wrapper +0EBE, trampoline
56EF:0089, contains the +0F2C call, pushes selector zero and saves the
returned byte afterward. Its earlier calls include a file copy, a
setup request, archive opening and another preparation call. Their
success, selected archive and effects are not admitted by the existence
of this later call. This finding follows the numbered loop when reached,
not the whole native load transaction.

### Preparation does not replace these three slots locally

The +0A6C entry initializes a local success byte to one, a mode byte
to zero, a loop bound to 17 and the SAVE tag. It calls +1DD8 and, on
a zero returned byte, calls the diagnostic entry in overlay 180 +0867.
The visible continuation then calls +0D1A. Reaching that continuation
does not establish how the diagnostic returns or terminates.

The complete local +0D1A helper copies global far pointers into
source-array indices 4..9 and 29..37, using the array base DS:157E.
It has no child calls. It does not assign indices 23..25, their source
storage, or the length array in 4F38. Thus this helper alone does not
replace the effect row, neighboring-table or count slots identified by
FND-COMBAT-034. Earlier state writers and aliases remain outside that
local statement.

For selector zero the numbered loop bound becomes 61 and the mode
byte becomes one; otherwise the bound remains 17. The index starts
at zero and advances by one after each completed iteration. A zero
local success byte stops further iterations.

### Destination pointers and request lengths

For each index s, the loop loads the far pointer at DS:157E + 4*s.
If it equals the global pointer at DS:67B7, the loop requests a
12000-byte zero fill of that buffer through 1000:3FA2 before proceeding.
That special buffer's identity and the fill effects remain dependencies;
the branch is not silently discarded for other source entries.

A null source-array pointer skips the resource request. Otherwise the
pointer is copied into a four-byte local variable, and the loop passes
the far address of that variable to 38FF:04AB along with tag SAVE and
resource number selector*61 + s + 1. The address is an in/out pointer
slot containing the destination, not a null pointer requesting ordinary
allocation on this branch.

The 04AB wrapper forwards that address, the tag and number to local
07B1 with start zero and count FFFFFFFF. Its stack-guard path remains
an external dependency. FND-CONFIG-151 reads 07B1: the sentinel selects
the complete selected resource length, and a nonzero destination is
reused without an allocation or destination-capacity argument. That
finding retains active-archive, record selection, positioning and I/O
dependencies. The restoration loop does not instead push a length from
4F38:0000, compare it with a requested resource length, or cap the
effect count before making this request.

The loop tests the reader's full AX. Zero continues; any nonzero word
sets the local success byte to zero. It then increments s and exits
the loop on that flag. There is no local undo of earlier buffers, and
the in/out pointer slot is not copied back into the global source
array. FND-CONFIG-151 separately records that a reader failure after
transfer begins need not leave the existing destination unchanged.

### Conditional effect-state load order

If selector is zero, the source entries retain the values recorded by
FND-COMBAT-034, and the earlier iterations/calls permit continuation,
the local requests occur in ascending source order:

| SAVE resource | Supplied destination | Position in the effect-state sequence |
|---|---|---|
| 24 | 4E71:024D | Ten-byte effect-row storage. |
| 25 | 50B0:000F | The neighboring three-byte-record storage. |
| 26 | 57E0:2100 | The effect-count word. |

The writer's shipped 1920/252/2 lengths are not arguments of these
read calls. Under successful selected resources of those respective
lengths, the first two buffers are filled before the count word. A
failure can stop before that count request, leaving a prefix of the
requested state changed. This is not a promise of atomic restoration,
an accepted-save schema, or a claim that any installed save fails.

The loop itself does not inspect restored effect IDs, owner sorting,
row count or per-owner keys. It also does not load the category prefix
from the particular SAVE/25 destination. Later postprocessing and the
other numbered destinations have not been completely read here and
may affect those questions. No assertion of an end-to-end admitted
restored state follows from this local request ordering.

## Interpretation

This answers the concrete restore-request producer for effect rows and
count under Q-COMBAT-009: it is a full-resource transfer into a supplied
buffer, not a literal count store found by the earlier word-2100
candidate pass. It also explains why shipped writer lengths alone do
not establish the reader's transfer size or loaded-count domain.

Q-COMBAT-009 retains the admitted archive/resources, actual destination
backing, all state writers, postprocessing and caller conditions.
Q-COMBAT-011 retains complete category/membership initialization and
restoration. These are direct original-code observations for research;
no parser, restoration implementation or original execution was added.

## Alternatives

- Passing the destination's address does not clear its pointer: this
  local variable is assigned the nonzero table pointer before the call.
- The save writer's byte-length array is not a capacity argument to
  this reader. The actual wrapper uses the complete-length sentinel.
- Testing a full-word error and stopping later iterations does not undo
  successful earlier requests or a partial transfer in the failing one.
- A later count-word request does not prove count stayed zero until
  that point; prior state, other resource writes and caller preparation
  must establish its starting value and identity.

## How to reproduce

Use the locked evidence Python and fingerprinted installed DSUN.EXE.
Run immediate_search.py for 157E and 1580 with the known save-writer
accesses as controls. Run direct_callers.py for 187+0A6C and 187+0EBE.
Read overlay_listing.py descriptor 187 with bounds 7089C..70964,
70B4A..70BFD and 70CEE..70D63, and resident_listing.py for
38FF:04AB..38FF:04D5. All bounds are hexadecimal and half-open.

Follow the selector/loop-bound assignments, source-pointer load and
local in/out slot, wrapper's zero start and complete-length sentinel,
full AX failure test and flag-controlled loop stop. Compare source
indices 23..25 with the relocated data and explicit table queries in
FND-COMBAT-034. Keep archive/reader dependencies from FND-CONFIG-151
distinct from the caller's local request semantics. Original-derived
listings remain in GAME_DIR/analysis/combat-effects/; none are committed.
