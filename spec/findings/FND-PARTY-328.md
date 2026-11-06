---
id: FND-PARTY-328
title: Queue producers bound their own index transitions but do not establish immutable dispatch kinds
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000654BC..0x00065653
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E2F0..0x0007E32C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E795..0x0007E7D3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00042C64..0x00042C70
tool: Connected reread with locked Python 3.12.10 and Capstone distribution 5.0.9; shared x86-scan-index and table through tools/evidence/report.mjs at 006a18ef (reader 4.3.0, engine 19.10.0); retained Python 3.14.7 observations from FND-PARTY-107
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 179 `+0D2F`, the hit routine, takes a slot `t`, a second slot and an item
number `i` (FND-PARTY-101), and is called from overlay 179 `+0D1A` (FND-PARTY-102), overlay 179
`+1299` and overlay 193 `+04DE`.

**The queue.** Overlay 179 `+12B1` (trampoline `56A7:0084`) and `+1304` (trampoline `56A7:0089`)
add a 14-byte record at `4DA1:0000 + 14 * n`, `n` being the byte at `DS:0915` after adding 1 to it,
holding their first three arguments at `+0`, `+2` and `+4`; `+1304` also fills `+6` to `+0D`. When
the signed byte is 5 or more they call `+12B1` with the same second argument twice, 307 and 6 instead,
and that call meets the same test (`+12B1..+1372`). `+11DC` takes the records from the last down,
and for each reads the word at `4DA1:0054 + 2 * n`: for 0 it calls overlay 193 `+003C` with the
record's words at `+2`, `+0` and `+4` first, and for 1 it calls `+0D2F` with the words at `+0`,
`+2` and `+4` first (`+11E4..+12AF`). The six words at `4DA1:0054` are 0 in the file, the segment
being descriptor 102's, at file `0x00042C10`. Of the 26 words `segment_references.py` finds naming
segment `4DA1`, none leads to a store at `+0x54` to `+0x5F`: the stores are those of `+12B1` and
`+1304` (for admitted indices 0 to 5 their records end at `+0x54`); the others are the segment table's entry,
reads of words at `+0x60` and `+0x62`, and overlay 179 `+09AD`, which reads a `VECT` resource to
`4DA1:0060`.

**Overlay 193 `+003C`** passes its third argument as `i` at `+04DE`, with `t` from its local word
at `bp-0x1A` and its first argument as the second slot (`+0045..+0048`, `+04A5..+04E3`). It is
called from overlay 193 `+0029`, in `+0000` (trampoline `573B:007F`), which passes its own
arguments in the same order and then calls `+11DC` (`+0006..+0037`), and from `+11DC` at `+125C`.

The third argument each caller pushes is:

| Caller | Routine called | Third argument |
| --- | --- | --- |
| overlay 179 `+1028` | `+12B1` | 305 |
| overlay 179 `+2707` | `+12B1` | 311 |
| overlay 197 `+0F7C` and `+10F1` | `+12B1` | 307 |
| overlay 197 `+180F` | `+12B1` | 307 on its literal route; the forwarded number on reflection routes (FND-MAGIC-020, FND-PARTY-288) |
| overlay 197 `+17A3` | `+12B1` | 124 |
| overlay 197 `+1A3C` | `+12B1` | 308 |
| overlay 197 `+1A2B` | `+1304` | -1 |
| overlay 197 `+1202` | `+12B1` | its register `SI` |
| overlay 198 `+00EF` and `+013D` | `+12B1` | its register `SI` |
| overlay 193 `+22CF` | `+0000` | its register `DX` |
| overlay 176 `+03BD` and `+048A` | `+0000` | 235 plus an argument or `DI` |
| overlay 177 `+04FE` | `+0000` | its argument at `bp+0xA` |
| overlay 204 `+19AF` | `+0000` | its local word at `bp-2` |

`data_bytes.py <install>/RESOURCE.GFF 19` gives the byte at `+0x19` of `DATA` 124 as 35, of
`DATA` 305 as 69, and of `DATA` 307, 308 and 311 as 0.

## Interpretation

The recorded direct segment-reference search did not identify a dispatch-kind
writer. That is a search result with exclusions, not a proof that no writer
exists or that the hit branch can never run. Under retained zero dispatch
kinds, valid indices and the other recorded storage conditions, the runner
selects overlay 193 `+003C`. A retained kind one instead selects its hit call;
other kind values skip both calls. The kind-one enqueue candidate's third
argument -1 does not by itself exclude later mutation or other writers.
There is no unconditional exclusive-entry claim for DATA 104 or 225 from
this search. FND-MAGIC-020 and FND-PARTY-288 also correct the old literal-only
307 description of reflection's call at `+180F`.

### Connected index and storage boundary

The complete runner and both producers were reread over file
`0x654BC..0x65653`. The index is a signed byte. The initial load-image byte
at file `0x4D915`, modeled `57E0:0915`, is 255, interpreted as -1. This is
an initial value, not an invariant after archive loads or arbitrary writes.

Both producers test the signed index before incrementing it. An admitted
input in -1..4 produces a new index in 0..5 and a 14-byte record offset
0..70 in segment 4DA1. Their own stores then stay below offset 84, so those
stores do not touch the six dispatch-kind words at 84..95. There is no
lower-bound guard: an input below -1 also passes the signed test and can
form a negative sign-extended index with wrapped word addressing. The old
claim that the test alone keeps every index in 0..5 was too broad.

The runner enters its body only while the signed byte is greater than -1.
With an admitted index 0..5, disjoint frame/global/record storage, valid
returned client calls and no intervening index mutation, it processes the
current record and decrements the byte once. It rereads the byte for the
next iteration. Calls can enqueue new records, so the decrement is of the
post-call global value rather than a captured index. Neither this local
loop nor initial -1 proves an arbitrary live queue remains bounded.

With a stable index of at least five and preserved code/frames, the full
producer branch calls `+12B1` without making room, using the same second
argument twice, number 307 and level six. The child repeats that same
branch. There is no finite ordinary return or publication within this
unchanged chain. This does not assert observed stack exhaustion: changed
index state, interrupt effects or faults are outside that conditional
reading, and actual reachability of a full queue remains open.

A reused shared scan-index query for displacement `0x0915`, limit 10000,
found 13 raw candidates. Its independent control was the producer's known
increment at file `0x655B1`, and the control passed. Ten candidates are
instructions on these three established streams: reads `0x654C7`,
`0x6550B`, `0x655B5`, `0x65608`; tests `0x65583`, `0x6559A`, `0x655ED`;
decrement `0x6557F`; increment `0x655B1` and increment `0x65604`.
The raw candidate at `0x655B4`
corresponds to a decode beginning inside the increment, rather than its
start. The remaining raw interiors `0x654C6`, `0x65607` likewise are not
new instructions on the known streams. Alternate entries are not excluded.

The candidate classification is therefore ten known instruction starts
and three operand-interior starts; the known writer mechanism consists of
two increments and one decrement. None of the ten is an independent reset
or dispatch-kind store. This does not exclude wider overlapping stores,
computed pointers, block loads, changed code or aliases, and does not
establish global absence from a literal displacement query.

The reused index covers 205 declared regions, 588250 scanned source bytes
and 262652 candidate rows. It retains 9235 undecoded offsets and 1748
unresolved call candidates. Query candidates, undecoded ranges and calls
were all below their limits. Cache hit key is
`e1138eb3579777496a6dcc064f664c856638eb41ae05e401fa6308d059773b2b`;
identity is Capstone distribution 5.0.9, schema two, prepared configuration
SHA-256 `52c50ee5be807744b66bf4b152d60ec30c80c98e87d8e5bfd4eefa877bce9a14`
and engine sources SHA-256
`4ed563cfd2d76148abb07bb073418256a9c1d096d0affb6ce3428d237e571e63`.
FND-SAVE-044 records the source-region regeneration and independent controls
for the reused index. No candidate cache is original instruction ownership.

## Alternatives

- This supersedes FND-PARTY-107: its unconditional never-taken conclusion,
  exclusive hit-entry inference, literal-only 307 reflection row, and bound
  inferred from a signed upper test were unsupported. Other source/caller
  observations above are retained rather than repeated as new censuses.

- A store to `4DA1:0054..005F` through a pointer formed elsewhere, or through a record index above
  5, is outside the search; the ordinary -1..4 producer input domain bounds their own writes,
  but that domain requires admission and the signed test has no lower guard.
- The computed arguments of overlay 197 `+1202`, overlay 198 `+00EF` and `+013D`, overlay 193
  `+22CF`, overlay 176 `+03BD` and `+048A`, overlay 177 `+04FE` and overlay 204 `+19AF` were not
  traced.
- Actual full-queue admission, changes during recursion and the host/guest
  outcome are not established by the conditional recursive chain.

## How to reproduce

For this connected reread use `overlay_listing.py <dsun> 179 654BC 65653`.
The script checks DSUN.EXE size 634416 and XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b` and resolves overlay fixups. Follow each
signed index gate, both publication branches, both runner client paths and
the global decrement after returning clients.

Reuse FND-SAVE-044's declared scan configuration and local cache. Replace
only query with kind displacement, value 2325, controls [415153], limit
10000, retaining all source/mapping/decoder identity fields. Run
`node tools/evidence/report.mjs x86-scan-index CONFIG`. Require the 13
candidate offsets, the known increment control and untruncated exclusions;
qualify candidates against the complete three bodies, not by raw decoding.
For the initial byte, use the bounded table reporter with the same source
and fingerprint, start 0x4D915, count/stride/limit one and a single width-one
field at offset zero; countEvidence names this one index byte. Interpret
its unsigned 255 as the signed byte -1.

Reports and queries remain under
GAME_DIR/analysis/handler-census-20261106/ with stems
reflected-queue-index and reflected-queue-initial-index. No game was run.

The retained caller/data observations reproduce with the older queries:

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 179 654BC 65660`,
`193 7E2F0 7E3A0` and `193 7E790 7E7E8`; `direct_callers.py <dsun> 179+12B1 179+1304 179+11DC
193+0000 193+003C`, reading the pushes before each call; `segment_references.py <dsun> 4DA1`; the
six words at file `0x00042C64`; and `data_bytes.py <install>/RESOURCE.GFF 19`.
