---
id: FND-ITEM-098
title: Effect-slot production performs ordinary placement and immediate handling before sampled effect-row construction
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080CDB..0x00080ECE
tool: tools/research/exec-census/overlay_listing.py at 91f29dcb; Python 3.12.10, Capstone 5.0.7; composition with FND-MAGIC-019 and FND-ITEM-068/075/096/097
environment: null
---

## Observation

Q-PARTY-116 retains the effect constructor's actual slot/count/row inputs.
FND-MAGIC-019 identifies overlay 193 +29EB as the sampled caller's slot
producer and records its level/kind/lifetime fields. This reads its full
local ordering and binds ordinary placement to the later constructor.
Offsets, masks and sentinels below are hexadecimal; other counts are decimal.

### Object choice and cell admission

The low effect-number byte selects word +3 of its 18-byte row at
51F1:0000. Byte FF instead selects object number 8000 decimal. A hidden
position flag is set only when incoming x and y words are both FFFF and
the supplied height byte is FF. This flag does not bypass the initial
cell calls: signed x/16 and y/16, truncating toward zero, are passed to
25AF:00E1 and, on a nonzero AL, 25AF:014B.

Both calls returning nonzero rejects an ordinary position with returned
FFFF. A zero result from either, or the hidden flag, continues. For the
hidden case only then are both coordinates replaced by 2112 decimal and
height by zero. The preliminary cells for FFFF words are therefore based
on signed minus one divided by sixteen, not the substituted position.
Native grid/pointer admission and child preservation remain requirements.

### Ordinary placement precedes metadata and immediate handling

It calls 31E0:0121 with the chosen object, requested slot FFFF, retained
or substituted coordinates/height, category five and mode one. Exact FFFF
returns failure; every other word is retained as its slot without a separate
upper bound. FND-ITEM-096 supplies prepared flags 4D and mode one's
local gates; FND-ITEM-097 supplies conditional counted ordinary publication
when lower-load index/count/DF/span preservation holds. Those conditions
are not established by receiving a non-FFFF slot.

After placement it indexes the effect row again using the original low
byte, including FF on that input path; there is no second FF exclusion.
Row byte +5 low two bits select an optional 2C5F:0A22 request with 125,
124 or 12 decimal. Its result is ignored. It then writes the capped level
to object byte +15 low five bits, effect-number-plus-one at byte width
to object byte +0F, and kind low three bits to byte +15 high three bits,
as FND-MAGIC-019 records. Byte FF consequently wraps that +0F store to
zero if this path reaches it; valid effect-row storage is still required.

For nonhidden positions it calls overlay 193 +0AAD with the object's
current placed-entry index and zero. That result is ignored. Effect-row
byte +2 mask 10 then requests 25AF:01AE at the retained input cell;
mask 20 independently requests 25AF:017F there. Both are skipped by the
hidden flag. These coordinates are retained input coordinates, not freshly
derived from any child-mutated placed row. The immediate handler and
cell-child state/preservation remain separate obligations.

### Deferred request and return

A signed negative lifetime dword skips scheduling. Otherwise a local
payload receives the retained slot word and byte two, and 371D:01F9 is
called with the supplied queue byte, low32(base-dword+lifetime), kind three
and that local far pointer. The remaining payload bytes are not initialized
by this body. The scheduler result is ignored; final AX is the retained
slot and SI/DI/BP are restored. No scheduling-success or wider child
register preservation guarantee follows. FND-ITEM-075 separately records
kind-three consumption's detached slot and mode-zero dispatch.

FND-ITEM-068's sampled caller receives this result before starting its
point loop and calling 31E0:03CA. Thus an admitted successful ordinary
placement is a preceding count/row producer, while immediate handler,
cell and queue children can intervene before the first effect constructor.
A positive returned slot does not prove that count stayed positive or
that its ordinary row remained counted/unchanged through those children.

## Interpretation

This removes the slot producer's local object/position admission, ordinary
placement binding and metadata/immediate/deferred ordering from Q-PARTY-116.
Remaining evidence is actual effect-row/coordinate/lifetime inputs, ordinary
placement's admitted continuation, effect-row FF storage, immediate-handler
and cell/queue effects and preserved slot/count/row state at constructor
entry. Neither the zero-count boundary nor positive-count continuation is
admitted solely by the selected caller's nonnegative-slot gate.

## Alternatives

Treating the sampled effect constructor as the slot's first placement
ignores the preceding ordinary call. Treating hidden position as skipping
all cell reads ignores the pre-substitution gate. Treating returned slot
as scheduling or row-lifetime success ignores unchecked children/results.
No native defect, complete-reading declaration or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with overlay_listing.py for descriptor 193, file range 80CDB 80ECE.
GAME_DIR/analysis/item-removal-20261104/ holds
effect-slot-producer-contract.txt and its -repeat pair, both SHA-256
4ba0e9f746c3452511876d15e1b10e738824e74edfc1dadc244ee74df7b1d978.
Retain the cited ordinary-placement and sampled-caller conditions. Keep
reports local. No original game or isolated function was executed.
