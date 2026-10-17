---
id: FND-PARTY-224
title: Inventory drawing reuses attached-record word 0C as a cached BMP resource number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00076487..0x00076668
tool: tools/research/exec-census/overlay_listing.py and trampoline_target.py; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-217 reads the map and kind-one graph/record selection in
descriptor 189 +1AD7, file 76487. The field it reaches at file
76501 is a word at offset 0C hexadecimal of the selected 23-byte
record through DS:19C1. It loads this word into DX and tests it
against zero. Nonzero skips the bitmap-number resolver and reaches
the resource request with that word.

### A zero word requests a number, then stores it for later draws

For a zero word in a main logical position four through 29, the body
rereads the current 4E71:0B44, graph pointer from DS:6189 indexed by
that word, map pointer DS:617D and selected graph entry. It passes
the entry's record-number word to 571F:00C5. The trampoline resolver
maps this to descriptor 190 +2F2F at file 7B26F.

On return it keeps AX in DX. It then reconstructs the selected record
again from current 4E71:0B44, graph and map state, and writes DX to
that record's word +0C. There is no new zero-result test before the
following resource request. A returned zero can therefore be stored
and passed as resource number zero; it is not locally replaced with
a different number or a placeholder image.

The post-child reconstruction is material: the body does not retain
one record pointer across 190 +2F2F. Current-character, graph, map,
record-base and register preservation are prerequisites for treating
the write as a cache update to the same record originally read.

The logical-position 30..35 branch selects an entry through the word
map at DS:9B3E plus twice the position and graph pointer DS:619D.
It likewise reads the selected 23-byte record's word +0C, calls the
same resolver for zero and stores the returned word after rereading
its selection state. The later logical-position branch uses the
primary map/graph route; its zero case rejoins the earlier resolver
path. These local routes do not identify all callers or the gameplay
roles of those other positions.

### The cached word is an unsigned BMP identifier, not an image pointer

The common request at file 7663D clears a local far pointer, passes
its SS address as the output, zero-extends DX to a doubleword and
passes tag BMP to 38FF:04AB. The caller removes twelve argument bytes.
It ignores returned AX and instead tests the resulting local far
pointer. Null skips subsequent drawing; nonnull continues into image
dimension and draw requests outside this observation's scope.

This establishes the word's use as a BMP resource number. It is not
the acquired image pointer, and it is not an ICON-tag request. No
local upper-number guard or signed rejection occurs before the
zero-extension. The path can request any retained unsigned word
value, conditional on admitted record storage.

## Interpretation

Runtime attached-record offset +0C is a cached bitmap number in the
reviewed inventory drawing paths: nonzero is reused, zero invokes a
resolver, and the returned word is stored before a BMP request.
This answers that field-use part of Q-PARTY-020. It does not specify
all producers or invalidation rules, the resolver's complete inputs,
serialization effects or native drawing success.

The source file/archive selected by the registered resource lookup,
the acquired pointer's validity on partial failure, all image effects
and state preservation remain open under Q-PARTY-085. No original
function was executed and no complete reading is claimed.

## Alternatives

- A far image pointer would need more than the word read here, and
  the actual acquired pointer is held separately in the local output.
- An ICON identifier does not match the common request's BMP tag.
- Treating zero as an unconditional no-image flag omits the resolver
  and the subsequent request even when the resolver returns zero.
- Reusing a single selected-record identity across the child omits
  the explicit post-child rereads.
- Nonnull output is not proof of a complete valid image or a
  successful render; the acquisition status is not consumed here.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python.
Read overlay_listing.py descriptor 189 from the established +1AD7
entry at file 76487 through 76668, hexadecimal and half-open.
Resolve 571F:00C5 with trampoline_target.py. Use FND-PARTY-217's
graph/map provenance and the entry's explicit slot branches.

Decode from the reached instruction stream. The committed inventory
has a one-byte gap inside the four-byte far-load instruction starting
at 7654C. Restarting at 7654F decodes an operand byte as an opcode;
use the bounded stream from the entry and verify the preceding zero
branch and returning child continuation. The inventory's fragments
are not independent instruction-entry evidence or a complete body.

Trace the nonzero skip, zero resolver, current-state rereads, word
store and common unsigned BMP request independently. Retain zero
resolver output, missing image pointer and changed selection state as
separate conditions. The report remains at
GAME_DIR/analysis/party-record-heads/drawing-20261018/draw-linear-context.txt.
The resolver's body and rendering backend are not treated as read.
