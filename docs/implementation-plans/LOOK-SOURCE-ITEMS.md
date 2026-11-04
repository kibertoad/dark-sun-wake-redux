# First-item source inputs to the Look classifier

Research tooling for Q-PARTY-115. FND-ITEM-036 leaves actual type-50 item
inputs open after settling the local classifier predicate. FND-ACTOR-015/016
give the 1643-resource first-kind controls; FND-PARTY-248/252 give the
quantity/value/type/word/name offsets; FND-ACTOR-018 supplies an independent
629-reference target subset in the known inline resources.

Reuse Inspector's bounded GffArchive reader and RdffKeyInspection's complete
first-header validation and source/hash/count controls. Add a fixed
rdff-look-census mode which summarizes the first kind-one record of each
RDFF resource, selecting type index 50. Read words +02/+06/+0A/+10/+14
only after the established 23-byte extent check. Report sorted numerical
metadata, resource sizes and named/unnamed counts; no text or payload bytes.
Require a nonempty selected domain, and retain that later chunks, attached
items in other positions, runtime writes and live Look calls are unexamined.

Synthetic acceptance uses distinct values at every selected offset, named
and unnamed type-50 controls, a different-type exclusion, other first kinds,
missed counts, malformed extents and duplicate inputs. The inherited source
verification must still precede archive interpretation. Run the selected
tooling gate, then run the original-source census as a separate research
batch and independently check the existing inline subset and other-size
controls. Exit: committed bounded command and passing controls; the finding
may exclude unchanged first-copy inputs but cannot exclude later name writes
or prove global absence of the Look effect.

## Full-chain extension for Q-PARTY-115

Before implementation, extend the existing bounded Inspector with
`rdff-look-chain-census ARCHIVE CONTROLS_JSON`. Reuse source identity,
archive parsing and first-kind controls. FND-ACTOR-015 and FND-PARTY-259
give ten-byte headers, type one first, later types two through four,
backward chunk references, payload widths by kind and the end marker.
Inspect every kind-one 23-byte payload before the first complete end
header, selecting type 50 and reporting chunk index as well as resource.
Do not parse bytes after the end marker. Report chunks, all item records,
selected named/unnamed items and trailing-resource count deterministically.
Require a nonempty type-50 domain. Bound each chain to 80 chunks; reject
truncated headers/payloads, wrong widths/kinds/types and forward references.
The existing first-only command retains its contract.

Synthetic acceptance must distinguish first/later items, names 9998/9999,
valid trailing bytes, excluded non-item kinds, malformed later chunks,
missing end markers, forward references and excessive chains. Run the
required tooling gate before committing. A separate research batch consumes
the licensed-source report, compares independent known chunk counts and
records remaining admission/mutation limits. No gameplay or pack changes.

## Initial types before a later type change

Q-PARTY-115 also needs the name domain for items initially outside type 50.
Extend the full-chain report with deterministic counts by original type,
named/unnamed totals for all item payloads, and metadata-only locations of
unnamed items (resource, chunk index, type and name number). Keep the existing
type-50 rows and limits. Read no new payload fields. The source-domain check
must reconcile summed type counts and all-item totals with the independently
known chunk table. Synthetic controls must include an unnamed non-50 item,
distinguish it from named and unnamed type-50 items, and preserve first/later
and trailing exclusions. Exit: passing tooling gate followed by a separate
source finding; source validity never proves later name preservation.

## Attached item-test input domains

For Q-ITEM-006, add all-kind-one payload histograms to the full-chain report:
unsigned byte +12 hex, signed position byte +13, signed bonus byte +16,
and unsigned word +10. FND-RNG-035 and FND-PARTY-217/248 describe these
consumer widths. Reuse source/hash/first-kind/chain controls and read only
the established 23-byte spans. Report sorted value/count rows named by field,
without labels, assets or new gameplay semantics. Each histogram must account
for the same item total, including later payloads and non-50 types.

Synthetic acceptance distinguishes the four offsets, unsigned 255 from signed
minus one, signed minus 128 and 127, and a nonzero high word byte. Preserve
the existing chain/end/trailing controls. Run the selected tooling gate;
record licensed-source counts separately as research. Source absence never
excludes later writes, restores, aliases or input domains outside OBJEX.GFF.

## Joint reset inputs

Q-PARTY-109 needs joint fields rather than marginal histograms to apply
FND-ITEM-059's reset destination to FND-ITEM-056/057's source candidates.
Add deterministic all-item `itemHookInputs` rows containing resource/chunk,
type word +0A, quantity word +02, word +0E, selector word +10 and signed
bonus byte +16. Read only the already validated 23-byte payloads. Keep
type-50 `rows` and all existing domains unchanged. This additive private
report metadata remains schema 1; consumers must select the new property
explicitly, and no asset-pack contract changes.

Synthetic acceptance must preserve pairings across first and later items,
non-50 types, signed bonus edges and distinct full-width destination words,
and exclude a trailing decoy after the end header. Run the selected tooling
gate before commit. A separate research batch joins these rows to controlled
type flags and DATA properties, verifies all-item/domain reconciliation and
records conditional source counts. Source pairing does not prove runtime
preservation, native reset invocation or successful effect dispatch.
