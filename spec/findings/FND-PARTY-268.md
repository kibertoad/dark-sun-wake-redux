---
id: FND-PARTY-268
title: Nine variable-tag reader setups have local or known-caller non-DATA bindings while four retain unresolved argument domains
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068DEC..0x00068E8D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071E9E..0x0007201A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47E5:01C4..47E5:0378
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47E5:0378..47E5:05C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47E5:05C9..47E5:05D1
tool: Existing FND-SCRIPT-019, FND-ITEM-086, FND-SAVE-015, FND-EXE-610 and FND-COMBAT-160; Ghidra 12.1.3 with scientific-method-engine 19.10.0 bounded decompile and caller scripts; tools/research/exec-census/resident_listing.py, overlay_listing.py and direct_callers.py at 44dd935c; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1
environment: null
---

## Observation

This classifies FND-PARTY-267's thirteen variable-type request sites by
their actual local producers or known caller bindings. A memory or register
push is not itself an unconstrained type. These are ordinary-entry,
intact argument/frame contracts, not a proof of every native alias or
unknown incoming transfer. The installed DSUN.EXE identity remains length
634416, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`.

### Local type producers

Five sites have a recorded or newly checked local type choice:

| Request site | Local type choice | Evidence |
|---|---|---|
| `172C:0601` | GPL for selector one, MAS for two; other selectors are rejected | FND-SCRIPT-019's complete cache-fill body |
| `31E0:36DB` | BMP for zero low-byte flag, CBMP otherwise | FND-ITEM-086's provider reading |
| overlay 182 `+060F` | BMP for zero byte argument, ICON otherwise | bounded decompiler view and full local instruction reading here |
| overlay 187 `+0B18` | SAVE | FND-SAVE-015's table reader |
| overlay 187 `+20FD` | BMP | bounded decompiler view and full local instruction reading here |

Overlay 182 `+059C` first checks its cached number and flag. A hit returns
without requesting anything. A miss writes the complete local type
doubleword to ICON or BMP according to the byte argument; those are its
only type-selection branches. It supplies a separate local doubleword to
the size query and later passes the selected type by value to `+060F`'s
whole-resource call. The local type and size/output locals occupy separate
four-byte fields. The size gate, positioning requests, pointer installation
and cached-state publication do not establish load success or pixels.

Overlay 187 `+206E` initializes its local type to BMP and has no own later
store to that field before the whole-resource request at `+20FD`. Its
number can change with presentation state, but a number alone cannot turn
that BMP request into DATA. The later PAL request is a different site
already covered by the literal-type census. Other preparation and drawing
children are not made into class-mask layout claims.

These local choices remove their type-generation alternatives. They do
not assert successful acquisition or rule out independently entering after
the selecting stores. Unknown incoming targets remain separate.

### Parameterized helpers with known non-DATA callers

Four further sites forward a caller type, with known caller bindings:

| Request site | Helper | Known ordinary caller type bindings |
|---|---|---|
| `47E5:049E` | `47E5:0378` | Three direct calls in `47E5:02B6`, two BVOC and one FVOC |
| `47E5:05B7` | `47E5:0552` | Four direct calls in `47E5:01C4`, PSEQ, FSEQ, LSEQ and GSEQ |
| overlay 184 `+1593` | overlay 184 `+1562` | The direct preload caller supplies BMP (FND-EXE-610) |
| overlay 187 `+08CE` | overlay 187 `+08A5` | The direct `+146C` caller supplies numeric type three (FND-COMBAT-160) |

The sound helpers push the incoming type doubleword unchanged to their
resource calls. Their parent bodies push the named type constants directly
before the push-CS/near-call sequence. The direct-call search returns the
three and four sites respectively. Resource numbers and sound-driver
selection can differ, but none of these known caller setups supplies DATA.
No sound output or complete sound-service behavior is claimed.

The preload and slot-reader bindings are reused from existing findings,
not inferred from their names. Their helpers remain parameterized; the
known callers do not exclude a separately computed or indirect incoming
call with another type. Such targets stay in Q-PARTY-124's indirect scope,
rather than reopening the known non-DATA caller's resource number.

### Four unresolved type domains

The remaining direct request sites are:

- `2D40:00A9`: the chunk loader forwards its caller's type. Known CHAR and
  RDFF calls are already FND-PARTY-260's evidence; the loader has no own
  type comparison. Caller setup and unresolved entries still decide whether
  another type reaches its chunk interpretation.
- `3A8E:03B4`: the window-child loader reads type from the child record's
  doubleword `+4`. FND-COMBAT-089 describes this interface. The actual
  record sources and writers, not general graphics success, decide whether
  the requested type can be DATA.
- `47E5:0527`: the `47E5:0500` helper forwards the type and number arguments
  to the whole reader. It initializes a local output pointer separately;
  it has no own type restriction. The direct-call search finds no caller.
- `47E5:04E6`: the `47E5:04B8` helper forwards the incoming type, number,
  start and length to the lower reader using a local pointer variable.
  It returns a zero-status predicate. The direct search finds no caller.

No direct caller is not proof of an unreachable function. Stored,
computed and bypass entries remain unresolved where the controlled census
does not cover them. The last two helpers are parameter-forwarding
interfaces, not new evidence that the game requests DATA 1001.

## Interpretation

The thirteen-site remainder now has five local non-DATA choices, four
known-caller non-DATA bindings and four unresolved type domains. The
general input/indirect scope is retained; this is not an exhaustive absence
claim for every way to enter the parameterized helpers.

Q-PARTY-124 can reuse those nine bounded bindings rather than keep their
type choices and ordinary caller numbers on the unknown list. Its next
type work is the four named domains. Getter-number and interpretation-
affecting buffer access obligations remain separate. No question closes,
no complete reading is declared, and no native outcome is inferred.

## Alternatives

- A possible alternate entry is not a reason to discard a checked ordinary
  caller binding, nor is that binding proof that all alternate entries are
  impossible. Keep the two statements separate.
- A decompiler's nearest pushed argument can be PUSH CS at a near call.
  Instruction checks here identify the type push before that return-frame
  setup; no type is inferred from CS.
- The overlay-182 mapping is confirmed by ReportFbovOverlayMap.ps1. The
  initially rejected overlay-181 range does not describe a second producer;
  FND-PARTY-267 corrects the earlier descriptor label.
- An exploratory chunk-loader caller query returns fifteen Ghidra call
  references against sixteen raw candidates. Its nearest-push display also
  does not by itself resolve a tag assembled from words. No exhaustive
  chunk-loader caller or argument-domain claim is made from that query;
  keep the raw-only candidate and computed sources unresolved.

## How to reproduce

At 44dd935c, reuse the cited existing findings and their retained controlled
reports. Run overlay_listing.py on descriptor 182, range `68DEC..68E8D`,
and descriptor 187, `71E9E..7201A`, against the fingerprinted installed
source. Read every local type assignment and the later pushes, keeping
resource/output locals separate. The FBOV map gives descriptor 182 at
file 68850 hex and mapped 7365:0000; descriptor 187 is at 6FE30 hex.

Use resident_listing.py for `47E5:01C4..47E5:02B6`,
`47E5:02B6..47E5:0378`, `47E5:0378..47E5:04B8`,
`47E5:04B8..47E5:0500`, `47E5:0500..47E5:0552`,
`47E5:0552..47E5:05C7` and `47E5:05C9..47E5:05D1`.
Compare direct_callers.py for the four helper starts; inspect all seven
positive sound caller setups. Preserve the empty-caller limitation.

The read-only Ghidra view is INST-overlay-20261029g, INST-MAPPED.EXE.
ReportDecompileWindow.java at 7365:059C and 7AC3:206E with start one and
count sixteen gives the bounded initial tag choices; do not stitch more
decompile windows into full-function output. ReportDecompileMatches.java
on the four resident helper starts with their resource callee name gives
the bounded forwarding context. Instruction reports decide the widths.
Reports remain in GAME_DIR/analysis/class-table-closure-20261106/ under
variable-tags-connected, tag182-local, tag187-image, generic-type-forwarders,
sound-tag-parents and parametric-type-caller-candidates stems. No game ran.
The exploratory ReportCallArguments.java queries of 2D40:000A, 7AC3:08A5
and 77E9:1562 are retained as typed-input-callers; their nearest-push
output is navigation only and supplies no negative caller proof.
