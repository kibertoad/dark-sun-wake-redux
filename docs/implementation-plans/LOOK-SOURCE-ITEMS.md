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
