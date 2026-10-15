---
id: FND-COMBAT-032
title: Effect removal compacts through the boundary row and separates key, index and code entry contracts
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FB63..0x0007FD1D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F95F..0x0007F9E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000828AB..0x000828C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3143:0061..3143:00A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:01DC..289B:02EA
tool: tools/research/exec-census/direct_callers.py, overlay_listing.py and resident_listing.py; installed-build fingerprint and relocation/fixup checks
environment: null
---

## Observation

Three exported entries of overlay 193 separate removal by owner-local
key, by row index and by effect code. Their far frames consume word-sized
argument slots, but the key/code selectors use only their low byte.
Direct-call candidates identify these trampoline bindings:

| Trampoline | Overlay entry | Inputs |
|---|---|---|
| 573B:0048 | +1873 | Owner word, key byte. |
| 573B:00C5 | +18E3 | Owner word, row-index word. |
| 573B:004D | +194F | Owner word, effect-code byte. |

The two removal calls in the application wrapper +166F both target
+194F. Its negative-code branch negates the byte before calling; its
positive-code replacement branch forwards that byte (FND-COMBAT-031).
The local key wrapper calls +18E3 directly. The direct-call scan also
finds an overlay 195 call to 573B:00C5 at +114F, with second argument
006E and an owner obtained from its local word. This establishes an
additional index-entry caller, not the validity of that caller's index.
Candidates from the scan are not a census of reached or indirect calls.

### Key removal checks before delegating

The key wrapper +1873 initializes its local result byte to zero and
calls the owner/key lookup +1C17 described by FND-COMBAT-031. It tests
the returned row's owner before testing the signed row index against
the current count. A passing match reads the row's effect-code byte,
sign-extends it and calls overlay 195 +1000 with owner and that code.
After that call it delegates to +18E3 with the saved row index and owner.
There is no lookup or ownership recheck between the handler and the
index-removal call.

The result byte from +18E3 becomes the wrapper's result. If nonzero,
the wrapper calls overlay 197 +1E7B with owner before returning the
saved result byte. Thus the local zero-result path covers no matched
row; it is not an error result from the index entry or either handler.
Their return and mutation effects remain separate prerequisites.

### Index removal copies the boundary record too

At +18E3 the body first saves row byte seven, the owner-local key, in
one byte of a local packet. This precedes its signed index/count test.
It does not locally test owner equality, nonnegative index or count
capacity. If index i is below count n, it calls 289B:01DC with:

- source: 4E71:024D + 10*(i + 1);
- destination: 4E71:024D + 10*i;
- byte count: sign-extended word (n - i), multiplied by ten in 32 bits.

For admitted 0 <= i < n <= 192 and unchanged table state, this is
10*(n-i) bytes, not 10*(n-i-1). It copies source rows i+1 through n
inclusive into rows i through n-1. In particular, deleting the last
active row still requests ten bytes from row n. It does not skip the
copy or merely leave that row unchanged.

The resident move helper selects forward copying when destination is
below source. For these bounded even spans the count is at most 1920,
there is one word-copy chunk, and no odd tail. Its normalization targets
are assigned locally to 02AE/02BD, as read in FND-COMBAT-031. This
direction preserves the retained active rows' relative order despite
overlap. It also requires a complete ten-byte source row at index n,
stronger than the owner-word read used by the lookup loops. The copied
boundary bytes become inactive after the following decrement; this
does not establish any particular sentinel contents.

Whether or not the copy branch was taken, +18E3 decrements DS:2100.
It then passes owner, request seven, a far pointer to the local packet
and zero to 371D:0002. Only the packet's key byte is assigned in this
local body; no contract for its other bytes is inferred here. If that
callee returns, the body returns AL = 1. There is no local rollback or
failure branch. A direct caller cannot acquire the key wrapper's earlier
checks merely by entering this exported index body.

### Code removal rechecks after compaction

The +194F entry initializes a persistent local changed byte to zero.
It calls resident 3143:0061 with owner and effect code to find the first
matching row. That helper starts from the owner lower bound, scans
same-owner rows and compares byte six. It returns the count on no match;
its loop reads the boundary owner before checking index against count.

For each matching effect-code byte, +194F calls overlay 195 +1000 with
the code zero-extended, unlike the key wrapper's sign extension. After
that call it saves row byte seven, conditionally makes the same
10*(n-i)-byte downward copy, decrements count, and makes the same
request-seven call to 371D:0002. It sets its changed byte and pass flag
to one, then checks the same index again because rows have shifted.
For a nonmatching code it increments the index instead.

The owner comparison again precedes the signed index/count comparison.
At the end of a pass, a set pass flag restarts from 3143:0061; a pass
without a deletion ends the loop. If the persistent changed byte is
nonzero, overlay 197 +1E7B is called once before return. The entry does
not load that changed byte into AL on its final return, so it is not
recorded here as a boolean-returning counterpart of the key wrapper.

Under valid sorted inputs, complete boundary backing, returning calls
that do not alter the relevant table/count or saved iteration state,
each deletion reduces count and each other step advances the index.
Those conditional local operations preserve owner sorting and retained
same-owner order. The handler and request calls have not been admitted
as nonmutating: unconditional global preservation or termination would
need their effects and all callers, not just this loop structure.

## Interpretation

This answers the two candidate decrements at file offsets 7FC23 and
7FCCC, their reached local removal branches, copy extent and ordering
for Q-COMBAT-009. Removal does not intrinsically reverse retained rows;
the complete producer invariant still depends on admission and calls.

It adds a concrete storage obligation: the compaction source includes
the full boundary record. Neither initial zero count nor a special
owner sentinel value supplies that backing guarantee. The count can
also be decremented by the index entry without a successful local
range check. This is a static contract distinction, not a claim that
the original reaches an invalid state or displays a native defect.

## Alternatives

- Interpreting both removal APIs as owner/code requests loses the
  separate key-byte lookup and unchecked row-index entry.
- A move length of ten times the number of retained active successors
  omits the extra boundary record the executable actually requests.
- The code-removal caller uses a zero-extended code, whereas the key
  wrapper sign-extends the row byte. They agree for 0..127 only; the
  full admitted ID domain remains unresolved.
- Return from an external handler is not evidence that it preserves
  count, rows, registers or lookup validity. No original run or emulated
  call was used to replace those missing contracts.

## How to reproduce

Use the locked evidence Python and fingerprinted installed DSUN.EXE.
Run direct_callers.py for 193+1873, 193+18E3 and 193+194F. Qualify the
local call edges with overlay_listing.py descriptor 193, file offsets
7FB63..7FD1D and 7F95F..7F9E6, and descriptor 195 at 828AB..828C7.
Use resident_listing.py for 3143:0061..3143:00A8 and
289B:01DC..289B:02EA. Ranges are hexadecimal and half-open.

Check the far-frame argument offsets, byte extension at both handler
calls, key capture before movement, source/destination order, signed
word subtraction before the 32-bit byte count, unconditional decrement,
and same-index/pass restart edges. Follow the move helper's assigned
normalization targets and direction branch. Retain listings only in
GAME_DIR/analysis/combat-effects/. No original function was executed.
