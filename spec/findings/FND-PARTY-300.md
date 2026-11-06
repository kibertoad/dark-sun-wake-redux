---
id: FND-PARTY-300
title: Controlled cache-key operands and the helper's local copy narrow the two-getter transition boundary
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0003..2C5F:0051
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084C1C..0x00084C59
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092115..0x000921C3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006DFCB..0x0006E023
    kind: file-data
tool: window_operands.py and overlay_listing.py at 95a90642, Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; retained getter/reflection/helper reports; FND-ITEM-057, FND-PARTY-070/298/299 and FND-CONFIG-151
environment: null
---

## Observation

Q-PARTY-134 retains the relationship between the reflection flag's
getter and the helper's later getter. This reads their cache-key and
local-copy boundaries, not every generic archive operation. Offsets
are hexadecimal and resource number 1001 is decimal.

### Controlled explicit cache-key census

The current 3231-row DSUN.EXE inventory's operand window 0F48..0F4D
returns two established operands: the key comparison at 2C5F:0020
and key store at 0044. Both use displacement 0F4B. These independently
known read/write sites from FND-ITEM-057 are the controls; no string
store occurs in that function. No other established operand in the
searched window writes the key or forms its address.

The raw pass returns two additional candidates. The CS-prefixed compare
begins at the displacement byte of the preceding jump at 2C5F:001E,
not an established instruction start. The other apparent PUSH immediate
starts at overlay 184 +0F65, in FND-PARTY-070's dispatch table: it
crosses the last button word 4B68 and the first target word. It is not
an established cache-key address producer. Unknown transfers into those
bytes remain outside the result; a linear decode of table data proves
no ordinary code path there.

This excludes additional explicit operands only in the stated window
and decoded inventory paths. Earlier-starting wider writes, computed
or indexed effective addresses, block/string operations elsewhere,
segment aliases and external effects remain outside the census.

### What the first result implies for the next request

Let N be the retained non-FFFF selector and K the current cache key.
For the getter's own ordinary branches, FND-ITEM-057 supplies:

| First call | Own key publication | Consequence at the second call if N, key and segment identity are retained |
|---|---|---|
| K equals N, hit | None | Another hit; no new lower-reader request |
| K differs from N, miss returns AX=FFFF | None | Another miss; it retries the reader |
| K differs from N, miss accepted | N | A hit; it reuses the fixed buffer |

The failed miss explicitly returns a zero pointer but does not cache
that null result or publish N. The table does not assume that child
effects or aliases preserve the key: that is the stated retention
condition, distinct from the getter's own publication mechanism.

On the selected route from overlay 197 +1827 to +184C, the intervening
own instructions test the returned pointer's flag and caster, then push
the helper arguments. They contain no key store or further child call.
Overlay 208 +0885's prefix likewise only builds its frame and calls
the getter with its first argument. Thus there is no own explicit cache
invalidation between these two getter calls. Retained DI is essential:
the later argument is current DI, not a fresh reload of the first input.
If DI changes, the two requests need not have the same selector.

### Local interpretation stops depending on the shared buffer

After its second getter, +0885 copies 49 bytes (73 decimal) into
SS:BP-4E through BP-6 inclusive. It then rebinds its working pointer
to that local buffer. Its only subsequent own memory writes are that
local pointer's offset/segment words at BP-4/BP-2; its calculation
reads the copied fields and one fixed-table byte and makes no more
calls or explicit interrupts before returning.

For admitted nonwrapping, distinct local/frame storage and a completed
copy, later shared-buffer contents are therefore not inputs to this
result calculation. The source must be admitted during the copy, and
the local bytes, pointer words and continuation must survive until
their reads. Asynchronous or aliased writes are not excluded merely
because this own suffix has no child call.

## Interpretation

FND-PARTY-298/299's first-failure/second-intact case requires an actual
retry outcome under retained key and number; it is not explained by
the getter returning a cached null. First accepted acquisition instead
selects a hit on the next same-number call, without proving buffer/key
coherence. FND-CONFIG-151 retains partial-transfer effects and absence
of buffer rollback; a successful key history alone does not admit
current bytes after later failed requests for another resource.

Q-PARTY-134 now needs actual key/number retention, the selected hit or
retry content outcome through the copy, active vector state where a
null is used, and the local frame/queue continuation. It need not
invent an own between-call invalidation or require shared-buffer
immutability after the local copy for the helper's AX calculation.
The excluded pointer/key write mechanisms remain named above.

FMT-PARTY-007 stays supported. Both getter sites remain Q-PARTY-124;
no complete reading, whole-client exclusion or queue closure follows.

The remaining independently answerable obligations split out of the
overgrown Q-PARTY-134 in this batch. Q-PARTY-134 keeps original producer
domains; Q-PARTY-135 owns the two-getter binding and local-copy case,
Q-PARTY-136 the named helper register/saved-word/runtime preservation,
and Q-PARTY-137 queue-record/index/kind/frame retention. Their explicit
settling conditions preserve the pre-split scope. Three added items and
one narrowed parent count as a split, not answered questions or closures.

## Alternatives

- A changed number, key or data segment can choose another transition;
  the retained-input table does not exclude those writers or transfers.
- A hit need not contain the resource named by its key; the getter does
  not validate the shared bytes and failed lower reads need not roll back.
- Local copying narrows a lifetime boundary; it does not itself admit
  the source, frame backing or external/interleaving effects.

## How to reproduce

Run window_operands.py on the installed fingerprinted DSUN.EXE and
current inventory with window 0F48..0F4D. Require both getter controls
and retain the two raw candidates. Compare the getter's established
001E..0024 and FND-PARTY-070's 22-button/22-target table. The retained
data-cache-key-raw-context linear table decode is diagnostic, not code.
Use the dispatch's established entry 184+0DA7 to verify table use.

Reuse di-preservation-data-getter and reflected-cast-producers reports
in GAME_DIR/analysis/class-table-closure-20261106/. Reuse the complete
reflection-helper-context and reflection-copy-context in
GAME_DIR/analysis/handler-census-20261106/. The new data-cache-key-window,
raw-context and table-dispatch reports are in that latter store. Follow
the two calls, key publication and post-copy suffix, retaining the
number and frame conditions. No original run or emulated call ran.
