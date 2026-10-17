---
id: FND-PARTY-228
title: The inventory opener refreshes cached image 9999 before requesting the current-item pointer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00074D69..0x00074DC7
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py; Python 3.12.10, Capstone wheel 5.0.9 (module version 5.0.7), xxhash 4.0.1
environment: null
---

## Observation

The exported overlay 189 entry +001E, trampoline 5713:00B1,
starts at file 749CE. Its declared body ends at 74DDF. This finding
qualifies the cache/pointer tail reached after the preceding window,
projection and callback work; it does not establish all those children
or complete opener admission. The retained decode begins at the
declared entry, not at an arbitrary byte near the cache comparison.

At 74D69 the opener calls 572F:0025 and tests returned AL.
FND-PARTY-227 records that predicate's complete local body: AX is
one exactly when current DS:1A34 differs from FFFF. If AL is zero,
the opener passes image number 19101 (4A9D hexadecimal) with word
flag one to 56BD:006B.

Nonzero AL selects the other branch. It reads mapped 5072:0044,
multiplies that word by 23 at word width, and selects the record
through far table current DS:19C1. It compares record word +0C
with exactly 9999 (270F hexadecimal). Any other value skips the
following refresh. Equality rereads 5072:0044, passes that word
to 571F:00C5, then reconstructs the record using freshly loaded
5072:0044 and current DS:19C1 and stores returned AX at +0C.
The result is stored without testing for zero or failure.

Both nonzero-predicate paths then call 572F:0020. They pass its
returned AX, with flag zero, to the same 56BD:006B service.
FND-PARTY-227 supplies that current-item helper's zero-triggered
resolver/store and no-selection fallback. FND-PARTY-036 identifies
the service's flag zero as BMP and flag one as ICON; it and
FND-CONFIG-187 retain the service's additional cache and size gates.
This tail maps the number/kind request, not a successful pointer
image or a native pixel result.

Under unchanged selection/table state and normal child preservation,
a starting cache of 9999 invokes the resolver here. If it returns
zero, the subsequent current-item helper invokes it again because
its own cache test is zero. A starting cache of zero skips this
opener refresh and is resolved only by that helper. Other nonzero
values skip both local resolver gates. These are conditional static
paths, not native observations; external changes can alter selection,
the stored word or the later helper's branch.

FND-PARTY-196's shipped character-record survey puts the +0C word
between zero and 2910 across its 151 attached records. None of
that surveyed source set supplies 9999. This does not establish
whether another admitted producer writes 9999 at runtime or whether
this opener branch occurs in a game session.

## Interpretation

The opener's exact 9999 gate is separate from the zero gate in the
shared current-item helper and the inventory drawing path. It is
not a universal invalid-image test or proof of an invalidation
protocol. Q-PARTY-087 retains complete producer coverage, other
consumers and selection/resource/storage admission. This finding
removes only this opener's local refresh and pointer-request branch
from that item.

## Alternatives

- Treating this refresh as a zero test substitutes the shared helper's
  condition for the opener's equality with 9999.
- Treating a zero result as a single failed resolution omits the
  later helper's possible second resolver request.
- Treating the surveyed source-record range as proof that the branch
  is unreachable ignores runtime producers and other admitted inputs.
- Treating every word at offset +0C as an item cache ignores record
  kind, table identity and stride. No producer census is claimed here.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and locked evidence Python.
Query direct_callers.py for 189+001E to verify its trampoline,
then run overlay_listing.py descriptor 189, 749CE..74DDF,
hexadecimal and half-open. Retained report:
GAME_DIR/analysis/party-record-heads/drawing-20261018/inventory-open-cache.txt.

Follow the reached tail from 74D69 through 74DC7 and compare its
children with FND-PARTY-225, FND-PARTY-227 and the pointer service
findings cited above. Keep the equality sentinel, zero result,
post-child record reconstruction and normal preservation conditions
separate. Compare the source-value claim with FND-PARTY-196 rather
than interpreting a source range as a runtime reachability proof.
